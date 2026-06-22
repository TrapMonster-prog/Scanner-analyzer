using System.Diagnostics;

namespace Scanner_analyzer.Components
{
    internal class InformationAdmins : IMethods
    {
        private List<string> adminUsers = new List<string>();
        public void Collect()
        {
            adminUsers.Clear();
            try
            {
                var psi = new ProcessStartInfo("net", "localgroup Администраторы")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                string output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();

                bool capture = false;
                foreach (var line in output.Split('\n'))
                {
                    var trimmed = line.Trim();
                    if (trimmed.Contains("---") && capture == false)
                    {
                        capture = true; continue;
                    }
                    if (capture && !string.IsNullOrEmpty(trimmed) && !trimmed.StartsWith("The command"))
                        adminUsers.Add(trimmed);
                }
            }
            catch (Exception ex)
            {
                adminUsers.Add($"Ошибка получения списка: {ex.Message}");
                adminUsers.Add("Убедитесь, что программа запущена от имени администратора.");
            }

            if (adminUsers.Count == 0)
                adminUsers.Add("В группе администраторов не обнаружено учётных записей.");
        }

        public void OutputInformation()
        {
            Console.WriteLine("\n4. Члены группы «Администраторы»");
            if (adminUsers.Count == 0)
                Console.WriteLine("Не удалось получить список администраторов.");
            else
                foreach (var user in adminUsers)
                    Console.WriteLine(user);
        }
    }
}