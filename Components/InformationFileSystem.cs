using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Scanner_analyzer.Components
{
    public class InformationFileSystem : IMethods
    {
        private List<string> fileSystemInfo = new List<string>();

        public void Collect()
        {
            fileSystemInfo.Clear();

            try
            {
                string systemDrive = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.Windows)) ?? "C:\\";
                DriveInfo driveInfo = new DriveInfo(systemDrive);
                fileSystemInfo.Add($"Системный диск: {driveInfo.Name}");
                fileSystemInfo.Add($"Тип файловой системы: {driveInfo.DriveFormat}"); // Покажет NTFS
                fileSystemInfo.Add($"Доступное место: {driveInfo.AvailableFreeSpace / 1024 / 1024 / 1024} ГБ из {driveInfo.TotalSize / 1024 / 1024 / 1024} ГБ");
                fileSystemInfo.Add(new string('-', 40));
            }
            catch (Exception ex)
            {
                fileSystemInfo.Add($"Ошибка получения инфо о диске: {ex.Message}");
            }

            string[] paths =
            {
                Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32"),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
            };

            foreach (var dir in paths)
            {
                try
                {
                    var di = new DirectoryInfo(dir);

                    DirectorySecurity acl = FileSystemAclExtensions.GetAccessControl(di);

                    fileSystemInfo.Add($"Папка: {dir}");

                    var rules = acl.GetAccessRules(true, true, typeof(NTAccount));
                    foreach (FileSystemAccessRule rule in rules)
                    {
                        fileSystemInfo.Add($"  [Пользователь/Группа]: {rule.IdentityReference}");
                        fileSystemInfo.Add($"  Тип: {rule.AccessControlType} | Права: {rule.FileSystemRights}");
                        fileSystemInfo.Add("");
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    fileSystemInfo.Add($"[!] Ошибка: Запустите программу от имени Администратора для сканирования {dir}");
                }
                catch (Exception ex)
                {
                    fileSystemInfo.Add($"Ошибка доступа к {dir}: {ex.Message}");
                }
                fileSystemInfo.Add(new string('-', 40));
            }
        }

        public void OutputInformation()
        {
            Console.WriteLine("\n10. Анализ файловой системы и прав доступа");
            foreach (var line in fileSystemInfo)
            {
                Console.WriteLine(line);
            }
        }
    }
}
