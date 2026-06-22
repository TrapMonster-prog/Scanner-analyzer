using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;

namespace Scanner_analyzer.Components
{
    public class InformationShares : IMethods
    {
        private List<string> shares = new List<string>();
        private List<string> errors = new List<string>();

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHARE_INFO_2
        {
            public string shi2_netname;
            public uint shi2_type;
            public string shi2_remark;
            public uint shi2_permissions;
            public uint shi2_max_uses;
            public uint shi2_current_uses;
            public string shi2_path;
            public IntPtr shi2_passwd;
        }

        [DllImport("Netapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int NetShareEnum(
            string? servername,
            uint level,
            out IntPtr bufptr,
            uint prefmaxlen,
            out uint entriesread,
            out uint totalentries,
            ref uint resume_handle);

        [DllImport("Netapi32.dll", SetLastError = true)]
        private static extern int NetApiBufferFree(IntPtr buffer);

        private const uint MAX_PREFERRED_LENGTH = 0xFFFFFFFF;
        private const int NERR_Success = 0;

        private const uint STYPE_DISKTREE = 0;
        private const uint STYPE_PRINTQ = 1;
        private const uint STYPE_DEVICE = 2;
        private const uint STYPE_IPC = 3;
        private const uint STYPE_SPECIAL = 0x80000000;
        private const uint STYPE_TEMPORARY = 0x40000000;

        public void Collect()
        {
            shares.Clear();
            errors.Clear();

            bool apiSuccess = false;
            bool wmiSuccess = false;

            try
            {
                apiSuccess = GetSharesViaAPI();
            }
            catch (Exception ex)
            {
                errors.Add($"[API] {ex.GetType().Name}: {ex.Message}");
            }

            if (!apiSuccess)
            {
                try
                {
                    wmiSuccess = GetSharesViaWMI();
                }
                catch (UnauthorizedAccessException)
                {
                    errors.Add("[WMI] Доступ запрещён — требуются права администратора.");
                }
                catch (ManagementException mex)
                {
                    errors.Add($"[WMI] {mex.ErrorCode}: {mex.Message}");
                }
                catch (Exception ex)
                {
                    errors.Add($"[WMI] {ex.GetType().Name}: {ex.Message}");
                }
            }

            if (!apiSuccess && !wmiSuccess)
            {
                try
                {
                    GetSharesViaCommand();
                }
                catch (Exception ex)
                {
                    errors.Add($"[CMD] {ex.GetType().Name}: {ex.Message}");
                }
            }

            if (shares.Count == 0)
            {
                shares.Add("Общие ресурсы не найдены или доступ запрещён.");
                shares.Add("Подсказка: запустите программу от имени Администратора.");
            }
        }

        private bool GetSharesViaAPI()
        {
            IntPtr bufPtr = IntPtr.Zero;
            uint entriesRead = 0;
            uint totalEntries = 0;
            uint resumeHandle = 0;

            int result = NetShareEnum(
                null,
                2,
                out bufPtr,
                MAX_PREFERRED_LENGTH,
                out entriesRead,
                out totalEntries,
                ref resumeHandle);

            try
            {
                // Обрабатываем и полный успех (0) и частичный (234)
                if ((result == NERR_Success || result == 234)
                    && entriesRead > 0
                    && bufPtr != IntPtr.Zero)
                {
                    int dataSize = Marshal.SizeOf<SHARE_INFO_2>();
                    IntPtr current = bufPtr;

                    for (uint i = 0; i < entriesRead; i++)
                    {
                        var share = Marshal.PtrToStructure<SHARE_INFO_2>(current);
                        string typeDesc = GetShareTypeDescription(share.shi2_type);

                        shares.Add(
                            $"Имя: {share.shi2_netname,-20} | " +
                            $"Путь: {(share.shi2_path ?? "N/A"),-30} | " +
                            $"Тип: {typeDesc,-25} | " +
                            $"Описание: {share.shi2_remark ?? "-"}");

                        current = IntPtr.Add(current, dataSize);
                    }

                    return true;
                }
                else if (result != NERR_Success)
                {
                    // Логируем код ошибки WinAPI для диагностики
                    errors.Add($"[API] NetShareEnum вернул код: {result} " +
                               $"(Win32: {Marshal.GetLastWin32Error()})");
                }
            }
            finally
            {
                if (bufPtr != IntPtr.Zero)
                    NetApiBufferFree(bufPtr);
            }

            return false;
        }

        private bool GetSharesViaWMI()
        {
            var scope = new ManagementScope(@"\\.\root\CIMV2");
            scope.Options.EnablePrivileges = true;
            scope.Connect();

            var query = new ObjectQuery(
                "SELECT Name, Path, Description, Type FROM Win32_Share");

            using (var searcher = new ManagementObjectSearcher(scope, query))
            {
                bool found = false;

                foreach (ManagementObject obj in searcher.Get())
                {
                    using (obj)
                    {
                        found = true;
                        string name = obj["Name"]?.ToString() ?? "Unknown";
                        string path = obj["Path"]?.ToString() ?? "N/A";
                        string desc = obj["Description"]?.ToString() ?? "-";
                        string type = obj["Type"] != null
                            ? GetShareTypeDescription(Convert.ToUInt32(obj["Type"]))
                            : "N/A";

                        shares.Add(
                            $"Имя: {name,-20} | " +
                            $"Путь: {path,-30} | " +
                            $"Тип: {type,-25} | " +
                            $"Описание: {desc}");
                    }
                }
                return found;
            }
        }

        private void GetSharesViaCommand()
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "net.exe",
                Arguments = "share",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.GetEncoding(866),
            };

            using (var process = new Process 
            { 
                StartInfo = processInfo 
            })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit(5000);

                if (!string.IsNullOrEmpty(error))
                    errors.Add($"[CMD stderr] {error.Trim()}");

                ParseNetShareOutput(output);
            }
        }

        private void ParseNetShareOutput(string output)
        {
            if (string.IsNullOrWhiteSpace(output)) return;

            var lines = output.Split(
                new[] { '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries);

            bool dataStarted = false;

            foreach (var line in lines)
            {
                string trimmed = line.Trim();

                // Пропускаем пустые строки
                if (string.IsNullOrEmpty(trimmed)) continue;

                // Строка-разделитель "---..."
                if (trimmed.StartsWith("---"))
                {
                    dataStarted = true;
                    continue;
                }

                // Служебные строки
                if (!dataStarted) continue;
                if (trimmed.Contains("успешно") ||
                    trimmed.Contains("successfully") ||
                    trimmed.Contains("команда"))
                    continue;

                // net share выводит колонки: Имя  Ресурс  Замечание
                // Разделяем по 2+ пробелам (колонки фиксированной ширины)
                var parts = System.Text.RegularExpressions.Regex
                    .Split(trimmed, @"\s{2,}");

                if (parts.Length >= 1 && !string.IsNullOrEmpty(parts[0]))
                {
                    string shareName = parts[0].Trim();
                    string sharePath = parts.Length > 1 ? parts[1].Trim() : "N/A";
                    string shareDesc = parts.Length > 2 ? parts[2].Trim() : "-";

                    shares.Add(
                        $"Имя: {shareName,-20} | " +
                        $"Путь: {sharePath,-30} | " +
                        $"Описание: {shareDesc}");
                }
            }
        }

        private string GetShareTypeDescription(uint type)
        {
            uint baseType = type & 0x0FFFFFFF;
            bool isSpecial = (type & STYPE_SPECIAL) != 0;
            bool isTemporary = (type & STYPE_TEMPORARY) != 0;

            string baseDesc = baseType switch
            {
                STYPE_DISKTREE => "Disk Drive",
                STYPE_PRINTQ => "Print Queue",
                STYPE_DEVICE => "Device",
                STYPE_IPC => "IPC",
                _ => $"Unknown ({baseType})"
            };

            var flags = new List<string>();
            if (isSpecial) flags.Add("Admin/Special");
            if (isTemporary) flags.Add("Temporary");

            return flags.Count > 0
                ? $"{baseDesc} [{string.Join(", ", flags)}]"
                : baseDesc;
        }

        public void OutputInformation()
        {
            Console.WriteLine("Общие ресурсы (NetBIOS Shares)");
            Console.WriteLine(new string('-', 60));

            if (errors.Count > 0)
            {
                Console.WriteLine("[ Диагностика ]");
                foreach (var err in errors)
                    Console.WriteLine($"  {err}");
                Console.WriteLine(new string('-', 60));
            }

            foreach (var share in shares)
                Console.WriteLine(share);

            Console.WriteLine(new string('-', 60));
            Console.WriteLine($"Всего найдено: {shares.Count} ресурс(ов)");
        }
    }
}