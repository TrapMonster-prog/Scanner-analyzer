using System.Diagnostics;
using System.Text;

namespace Scanner_analyzer
{
    internal static class ConsoleEncoding
    {
        private static readonly Encoding Oem = CreateOemEncoding();

        private static Encoding CreateOemEncoding()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            try
            {
                return Encoding.GetEncoding(866);
            }
            catch
            {
                return Encoding.Default;
            }
        }

        /// <summary>
        /// Кодировка консоли Windows (русская локализация — CP866).
        /// Вызывать при старте и после модулей, которые могли сменить OutputEncoding.
        /// </summary>
        public static void Apply()
        {
            Console.InputEncoding = Oem;
            Console.OutputEncoding = Oem;
        }

        public static Encoding OemEncoding => Oem;

        public static string RunConsoleProcess(string fileName, string arguments)
        {
            var psi = new ProcessStartInfo(fileName, arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Oem,
                StandardErrorEncoding = Oem
            };

            using var proc = Process.Start(psi)!;
            string output = proc.StandardOutput.ReadToEnd();
            string error = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            return string.IsNullOrWhiteSpace(output) ? error : output;
        }
    }
}