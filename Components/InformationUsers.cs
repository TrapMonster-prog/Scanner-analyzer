using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Scanner_analyzer.Components
{
    public class InformationUsers : IMethods
    {
        public bool IsAdmin { get; set; }
        public string Message { get; set; } = string.Empty;

        public void Collect()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                IsAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

                if (IsAdmin)
                {
                    Message = "Программа запущена с правами администратора!";
                }
                else
                {
                    Message = "Программа не запущена с правами администратора!";
                }
            }
        }

        public void OutputInformation()
        {
            Console.WriteLine("Проверка прав администратора");
            Console.WriteLine();
            Console.WriteLine(Message);
            Console.WriteLine();
        }
    }
}