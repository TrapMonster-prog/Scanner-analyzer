using System.Management;
using System.Runtime.InteropServices;

namespace Scanner_analyzer.Components
{
    public class InformationOS : IMethods
    {
        public string? Caption { get; private set; }
        public string? Version { get; private set; }
        public string? Architecture { get; private set; }
        public string? ServicePack { get; private set; }

        public void Collect()
        {
            try
            {
                Caption = RuntimeInformation.OSDescription;
                Version = Environment.OSVersion.Version.ToString();
                Architecture = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
            }
            catch
            {
                Caption = RuntimeInformation.OSDescription;
                Version = Environment.OSVersion.Version.ToString();
                Architecture = Environment.Is64BitOperatingSystem
                    ? "64-bit"
                    : "32-bit";
            }
        }

        public void OutputInformation()
        {
            Console.WriteLine("\n Информация об операционной системе");
            Console.WriteLine($"ОС: {Caption}");
            Console.WriteLine($"Версия ядра: {Version}");
            Console.WriteLine($"Разрядность: {Architecture}");

            if (!string.IsNullOrWhiteSpace(ServicePack))
            {
                Console.WriteLine($"Service Pack: {ServicePack}");
            }
        }
    }
}
