using Microsoft.Win32;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Scanner_analyzer.Components
{
    public class InformationRegistry : IMethods
    {
        private List<string> regAclInfo = new List<string>();

        public void Collect()
        {
            regAclInfo.Clear();
            string[] keys = {
                @"HKEY_LOCAL_MACHINE\SAM",
                @"HKEY_LOCAL_MACHINE\SECURITY",
                @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa",
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies"
            };

            foreach (var keyPath in keys)
            {
                try
                {
                    RegistryKey? key = null;
                    if (keyPath.StartsWith("HKEY_LOCAL_MACHINE"))
                        key = Registry.LocalMachine.OpenSubKey(keyPath.Substring("HKEY_LOCAL_MACHINE\\".Length), RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.ReadPermissions);
                    // Другие кусты можно добавить аналогично

                    if (key != null)
                    {
                        var acl = key.GetAccessControl();
                        regAclInfo.Add($"Ключ: {keyPath}");
                        foreach (RegistryAccessRule rule in acl.GetAccessRules(true, true, typeof(NTAccount)))
                        {
                            regAclInfo.Add($"  {rule.IdentityReference} : {rule.AccessControlType} -> {rule.RegistryRights}");
                        }
                        regAclInfo.Add("");
                        key.Close();
                    }
                    else
                    {
                        regAclInfo.Add($"Не удалось открыть ключ {keyPath} (возможно, нет прав)");
                    }
                }
                catch (Exception ex)
                {
                    regAclInfo.Add($"Ошибка при чтении {keyPath}: {ex.Message}");
                }
            }
        }

        public void OutputInformation()
        {
            Console.WriteLine("\n11. Разрешения ключей реестра");
            foreach (var line in regAclInfo)
                Console.WriteLine(line);
        }
    }
}