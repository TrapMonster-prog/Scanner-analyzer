using System.Diagnostics;
using System.Management;
using System.Text;
namespace Scanner_analyzer.Components
{
    public class InformationNetwork : IMethods
    {
        private List<string> netInfo = new List<string>();
        public void Collect()
        {
            netInfo.Clear();
            bool wmiSuccess = false;
            try
            {
                using (var searcher = new ManagementObjectSearcher(
                    "SELECT Description, " +
                    "IPAddress, " +
                    "IPSubnet, " +
                    "DefaultIPGateway, " +
                    "DNSServerSearchOrder, " +
                    "WINSPrimaryServer, " +
                    "WINSSecondaryServer FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = TRUE"))
                {
                    foreach (ManagementObject adapter in searcher.Get())
                    {
                        wmiSuccess = true;
                        ExtractAdapterInfo(adapter);
                    }
                }
            }
            catch (Exception ex)
            {
                netInfo.Add($"[WMI ошибка] {ex.GetType().Name}: {ex.Message}");
                netInfo.Add("Переключаюсь на ipconfig...\n");
            }
            if (!wmiSuccess)
            {
                try
                {
                    var psi = new ProcessStartInfo("ipconfig", "/all")
                    {
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    try
                    {
                        psi.StandardOutputEncoding = Encoding.GetEncoding(866);
                    }
                    catch
                    {
                        psi.StandardOutputEncoding = Encoding.UTF8;
                    }
                    using var proc = Process.Start(psi);
                    string output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit();

                    netInfo.Add(output);
                }
                catch (Exception ex)
                {
                    netInfo.Add($"[Ошибка ipconfig] {ex.Message}");
                    netInfo.Add("\nНе удалось получить сетевые настройки.");
                }
            }
        }
        private void ExtractAdapterInfo(ManagementObject adapter)
        {
            string desc = adapter["Description"]?.ToString() ?? "Неизвестный адаптер";
            string[]? ips = (string[]?)adapter["IPAddress"];
            string[]? subnets = (string[]?)adapter["IPSubnet"];
            string[]? gateways = (string[]?)adapter["DefaultIPGateway"];
            string[]? dnses = (string[]?)adapter["DNSServerSearchOrder"];
            string winsPrimary = adapter["WINSPrimaryServer"]?.ToString() ?? "";
            string winsSecondary = adapter["WINSSecondaryServer"]?.ToString() ?? "";
            netInfo.Add($"\nАдаптер: {desc}");
            netInfo.Add(new string('-', 50));
            if (ips != null && ips.Length > 0)
                netInfo.Add($"IP-адреса:     {string.Join(", ", ips)}");
            else
                netInfo.Add("IP-адреса:     не назначены (DHCP?)");
            if (subnets != null && subnets.Length > 0)
                netInfo.Add($"Маски:         {string.Join(", ", subnets)}");
            if (gateways != null && gateways.Length > 0)
                netInfo.Add($"Шлюзы:         {string.Join(", ", gateways)}");
            if (dnses != null && dnses.Length > 0)
                netInfo.Add($"DNS-серверы:   {string.Join(", ", dnses)}");

            if (!string.IsNullOrEmpty(winsPrimary))
            {
                string winsInfo = $"WINS:          {winsPrimary}";
                if (!string.IsNullOrEmpty(winsSecondary))
                    winsInfo += $", {winsSecondary}";
                netInfo.Add(winsInfo);
            }
        }
        public void OutputInformation()
        {
            Console.WriteLine("\n7. Сетевые настройки");
            foreach (var line in netInfo)
                Console.WriteLine(line);
        }
    }
}