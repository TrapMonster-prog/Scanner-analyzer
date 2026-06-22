using System;
using System.Management;
using System.Runtime.InteropServices;

namespace Scanner_analyzer.Components
{
    public class InformationGeneral : IMethods
    {
        public string? Hostname { get; private set; }
        public string? Workgroup { get; private set; }
        public string? Domain { get; private set; }

        [DllImport("Netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int NetGetJoinInformation(
            string? lpServer,
            out IntPtr lpBuffer,
            out int pdwBufType);

        [DllImport("Netapi32.dll")]
        private static extern int NetApiBufferFree(IntPtr Buffer);

        private enum NetSetupJoinStatus
        {
            NetSetupUnknownStatus = 0,
            NetSetupUnjoined,
            NetSetupWorkgroupName,
            NetSetupDomainName
        }

        public void Collect()
        {
            // Получаем имя компьютера стандартным средством .NET
            Hostname = Environment.MachineName;

            try
            {
                // Способ 1: Сбор данных через WMI
                using (var searcher = new ManagementObjectSearcher(
                    "root\\CIMV2",
                    "SELECT Workgroup, Domain, PartOfDomain FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var workgroup = obj["Workgroup"]?.ToString();
                        var domain = obj["Domain"]?.ToString();
                        var partOfDomain = obj["PartOfDomain"]?.ToString();

                        if (partOfDomain == "True" && !string.IsNullOrEmpty(domain))
                        {
                            Domain = domain;
                            Workgroup = null;
                        }
                        else if (!string.IsNullOrEmpty(workgroup))
                        {
                            Workgroup = workgroup;
                            Domain = null;
                        }
                        break;
                    }
                }
            }
            catch
            {
                // Если WMI упал, свойства останутся null, и сработает проверка ниже
            }

            // Способ 2: Если WMI не вернул данные, используем WinAPI
            if (string.IsNullOrEmpty(Workgroup) && string.IsNullOrEmpty(Domain))
            {
                GetJoinInformationViaAPI();
            }

            // Способ 3: Защита «от дурака» — если оба метода не вернули имя рабочей группы
            if (string.IsNullOrEmpty(Workgroup) && string.IsNullOrEmpty(Domain))
            {
                Workgroup = "WORKGROUP";
            }
        }

        private void GetJoinInformationViaAPI()
        {
            IntPtr bufPtr = IntPtr.Zero;
            try
            {
                int status = NetGetJoinInformation(null, out bufPtr, out int bufType);

                if (status == 0 && bufPtr != IntPtr.Zero) // 0 == NERR_Success
                {
                    var joinStatus = (NetSetupJoinStatus)bufType;
                    string? joinInfo = Marshal.PtrToStringUni(bufPtr);

                    if (joinStatus == NetSetupJoinStatus.NetSetupDomainName && !string.IsNullOrEmpty(joinInfo))
                    {
                        Domain = joinInfo;
                        Workgroup = null;
                    }
                    else if (joinStatus == NetSetupJoinStatus.NetSetupWorkgroupName && !string.IsNullOrEmpty(joinInfo))
                    {
                        Workgroup = joinInfo;
                        Domain = null;
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки WinAPI, так как в конце Collect() сработает дефолтное значение
            }
            finally
            {
                // Блок гарантирует освобождение памяти в куче Windows даже при сбоях
                if (bufPtr != IntPtr.Zero)
                {
                    NetApiBufferFree(bufPtr);
                }
            }
        }

        public void OutputInformation()
        {
            Console.WriteLine("\n2. Название узла, рабочей группы/домена");
            Console.WriteLine($"Имя компьютера: {Hostname}");

            if (!string.IsNullOrEmpty(Domain))
            {
                Console.WriteLine($"Статус сети:   Компьютер находится в домене");
                Console.WriteLine($"Домен:         {Domain}");
            }
            else
            {
                Console.WriteLine($"Статус сети:   Компьютер в рабочей группе");
                Console.WriteLine($"Рабочая группа: {Workgroup}");
            }
        }
    }
}
