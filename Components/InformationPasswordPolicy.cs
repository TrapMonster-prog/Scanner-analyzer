using System.Diagnostics;
namespace Scanner_analyzer.Components
{
    public class InformationPasswordPolicy : IMethods
    {
        private string policyOutput = "";
        public void Collect()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "net",
                        Arguments = "accounts",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                policyOutput = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                policyOutput = $"Ошибка выполнения: {ex.Message}";
            }
        }
        public void OutputInformation()
        {
            Console.WriteLine("\nПолитика паролей");
            Console.WriteLine(policyOutput);
        }
    }
}