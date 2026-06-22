using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace Scanner_analyzer.Components
{
    public class InformationUpdates : IMethods
    {
        private List<string> updates = new List<string>();

        public void Collect()
        {
            updates.Clear();

            // Определяем версию ОС
            bool isWindows8OrNewer = IsWindows8OrNewer();

            if (isWindows8OrNewer)
            {
                CollectUpdatesViaDism();
            }
            else
            {
                CollectUpdatesViaWmi();
            }

            if (updates.Count == 0)
                updates.Add("Обновления не обнаружены или недоступны для чтения.");
        }

        private bool IsWindows8OrNewer()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Version FROM Win32_OperatingSystem"))
                using (var os = searcher.Get().Cast<ManagementObject>().FirstOrDefault())
                {
                    if (os != null)
                    {
                        string version = os["Version"]?.ToString();
                        if (!string.IsNullOrEmpty(version))
                        {
                            // Версия 6.2 = Windows 8, 6.3 = 8.1, 10.0 = 10/11
                            return Version.TryParse(version, out var ver) && ver.Major >= 10 ||
                                   (ver.Major == 6 && ver.Minor >= 2);
                        }
                    }
                }
            }
            catch { /* Если не удалось определить, считаем по-старому */ }
            return false;
        }

        private void CollectUpdatesViaDism()
        {
            try
            {
                var psi = new ProcessStartInfo("dism", "/online /get-packages /format:table")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var proc = Process.Start(psi);
                if (proc == null) throw new Exception("Не удалось запустить DISM");

                string output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();

                if (proc.ExitCode != 0)
                {
                    updates.Add("Ошибка DISM. Возможно, нет прав администратора.");
                    return;
                }

                var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                bool headerPassed = false;
                foreach (var line in lines)
                {
                    // Пропускаем служебные строки и пустые
                    if (line.Contains("---") || string.IsNullOrWhiteSpace(line))
                        continue;

                    if (!headerPassed)
                    {
                        // Первая непустая строка без "---" — заголовок
                        headerPassed = true;
                        continue;
                    }

                    // Ищем "Installed" в строке
                    if (line.Contains("Installed") || line.Contains("Установлен"))
                    {
                        // Извлекаем идентификатор пакета (содержит KB номер)
                        string packageId = ExtractPackageId(line);
                        if (!string.IsNullOrEmpty(packageId))
                        {
                            updates.Add(packageId + " | Установлено");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                updates.Add($"Ошибка при получении обновлений через DISM: {ex.Message}");
            }
        }

        private string ExtractPackageId(string dismLine)
        {
            var parts = dismLine.Split('|');
            if (parts.Length > 0)
            {
                string fullId = parts[0].Trim();
                // Ищем "KB" + цифры
                int kbStart = fullId.IndexOf("KB");
                if (kbStart >= 0)
                {
                    int kbEnd = kbStart + 2;
                    while (kbEnd < fullId.Length && char.IsDigit(fullId[kbEnd]))
                        kbEnd++;
                    string kbNumber = fullId.Substring(kbStart, kbEnd - kbStart);
                    return kbNumber;
                }
                return fullId.Length > 80 ? fullId.Substring(0, 77) + "..." : fullId;
            }
            return null;
        }

        private void CollectUpdatesViaWmi()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT HotFixID, Description, InstalledOn FROM Win32_QuickFixEngineering");
                foreach (ManagementObject fix in searcher.Get())
                {
                    var id = fix["HotFixID"]?.ToString() ?? "Без ID";
                    var desc = fix["Description"]?.ToString() ?? "Нет описания";
                    var date = fix["InstalledOn"]?.ToString() ?? "Дата не указана";
                    updates.Add($"{id} | {desc} | {date}");
                }
            }
            catch
            {
                // Fallback на wmic (то же самое, что WMI)
                try
                {
                    var psi = new ProcessStartInfo("wmic", "qfe get HotFixID, Description, InstalledOn /format:csv")
                    {
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    using var proc = Process.Start(psi);
                    string output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit();

                    var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Skip(2);
                    foreach (var line in lines)
                    {
                        var parts = line.Split(',');
                        if (parts.Length >= 4 && !string.IsNullOrWhiteSpace(parts[2]))
                        {
                            updates.Add($"{parts[2].Trim()} | {parts[3].Trim()} | Установлено");
                        }
                    }
                }
                catch
                {
                    updates.Add("Не удалось получить список обновлений (требуются права администратора).");
                }
            }
        }

        public void OutputInformation()
        {
            Console.WriteLine("\n3. Установленные обновления безопасности");
            if (updates.Count == 0)
                Console.WriteLine("Обновления не найдены или недоступны.");
            else
                foreach (var u in updates)
                    Console.WriteLine(u);
        }
    }
}