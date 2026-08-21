using Microsoft.Win32;
using System.Management;

namespace Scanner_analyzer.Components
{
    public class InformationAdvanced : IMethods
    {
        private List<string> advancedInfo = new List<string>();
        public void Collect()
        {
            advancedInfo.Clear();
            // Гостевая учётная запись
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Name, Disabled FROM Win32_UserAccount WHERE Name='Guest'"))
                {
                    foreach (ManagementObject guest in searcher.Get())
                    {
                        bool disabled = (bool)guest["Disabled"];
                        advancedInfo.Add($"Гостевая учётная запись: {(disabled ? "Отключена" : "Включена")}");
                    }
                }
            }
            catch 
            { 
                advancedInfo.Add("Гостевая запись: не удалось определить."); 
            }

            // Кэширование учётных данных (CachedLogonsCount)
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon"))
                {
                    if (key != null)
                    {
                        object? cached = key.GetValue("CachedLogonsCount");
                        advancedInfo.Add($"Кэширование паролей (CachedLogonsCount): {cached ?? "не задано (по умолчанию 10)"}");
                        key.Close();
                    }
                }
            }
            catch
            { 
                advancedInfo.Add("Кэширование паролей: ошибка чтения."); 
            }

            // UAC
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System"))
                {
                    if (key != null)
                    {
                        int enableLUA = (int)(key.GetValue("EnableLUA", 1) ?? 1);
                        advancedInfo.Add($"Контроль учётных записей (UAC): {(enableLUA == 1 ? "Включён" : "Отключён")}");
                    }
                }
            }
            catch 
            { 
                advancedInfo.Add("UAC: ошибка чтения."); 
            }

            // Простой общий доступ (RestrictAnonymous)
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Lsa"))
                {
                    if (key != null)
                    {
                        int restrict = (int)(key.GetValue("restrictanonymous", 0) ?? 0);
                        advancedInfo.Add($"Ограничение анонимного доступа (RestrictAnonymous): {restrict}");
                    }
                }
            }
            catch 
            { 
                advancedInfo.Add("RestrictAnonymous: ошибка чтения."); 
            }
        }

        public void OutputInformation()
        {
            Console.WriteLine("\nДополнительные параметры безопасности");
            foreach (var info in advancedInfo)
                Console.WriteLine(info);
        }
    }
}
