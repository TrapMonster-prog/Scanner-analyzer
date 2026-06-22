using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Scanner_analyzer.Components
{
    public class InformationPortScan : IMethods
    {
        private readonly List<string> suspicious = new();
        private readonly Dictionary<string, int> ipCounter = new();

        public void Collect()
        {
            suspicious.Clear();
            ipCounter.Clear();

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "netstat",
                    Arguments = "-ano",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);

                if (process == null)
                {
                    suspicious.Add("Не удалось запустить netstat.");
                    return;
                }

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                ParseConnections(output);
                AnalyzeConnections();
            }
            catch (Exception ex)
            {
                suspicious.Add($"Ошибка анализа сетевых подключений: {ex.Message}");
            }
        }

        private void ParseConnections(string output)
        {
            var lines = output.Split(
                new[] { '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (!line.TrimStart().StartsWith("TCP"))
                    continue;

                var parts = Regex.Split(line.Trim(), @"\s+");

                if (parts.Length < 5)
                    continue;

                string remoteAddress = parts[2];
                string state = parts[3];

                if (remoteAddress.StartsWith("127.") ||
                    remoteAddress.StartsWith("0.0.0.0") ||
                    remoteAddress.Contains("[::]"))
                    continue;

                string ip = remoteAddress;

                int index = remoteAddress.LastIndexOf(':');
                if (index > 0)
                    ip = remoteAddress.Substring(0, index);

                if (!ipCounter.ContainsKey(ip))
                    ipCounter[ip] = 0;

                ipCounter[ip]++;

                if (state.Contains("SYN_RECEIVED"))
                {
                    suspicious.Add(
                        $"Подозрительное SYN-подключение: {remoteAddress}");
                }
            }
        }
        private void AnalyzeConnections()
        {
            foreach (var item in ipCounter)
            {
                if (item.Value >= 15)
                {
                    suspicious.Add(
                        $"Возможное сканирование портов с IP {item.Key} " +
                        $"({item.Value} подключений)");
                }
            }

            if (suspicious.Count == 0)
            {
                suspicious.Add(
                    "Подозрительной активности не обнаружено.");
            }
        }
        public void OutputInformation()
        {
            Console.WriteLine("\n15. Обнаружение сканирования портов");
            Console.WriteLine(new string('-', 60));

            foreach (var item in suspicious)
            {
                Console.WriteLine(item);
            }

            Console.WriteLine(new string('-', 60));
        }
    }
}