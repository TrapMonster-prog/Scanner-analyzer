namespace Scanner_analyzer.Components
{
    public class InformationAudit : IMethods
    {
        private string auditOutput = "";

        public void Collect()
        {
            try
            {
                string outData = ConsoleEncoding.RunConsoleProcess("auditpol", "/get /category:*");

                if (string.IsNullOrWhiteSpace(outData))
                {
                    auditOutput = "Аудит не настроен или отключен.\nЭто нарушение требований безопасности!";
                }
                else if (outData.Contains("отказано в доступе", StringComparison.OrdinalIgnoreCase) ||
                         outData.Contains("access denied", StringComparison.OrdinalIgnoreCase))
                {
                    auditOutput = "[Ошибка] auditpol требует прав администратора.\n" +
                                  "Запустите программу от имени администратора.";
                }
                else
                {
                    auditOutput = outData;
                }
            }
            catch (Exception ex)
            {
                auditOutput = $"Не удалось выполнить auditpol: {ex.Message}\n\n" +
                              "Попробуйте:\n" +
                              "1. Запустить от имени администратора\n" +
                              "2. Проверить наличие auditpol в системе";
            }
        }

        public void OutputInformation()
        {
            Console.WriteLine("\n6. Настройки аудита");
            Console.WriteLine(auditOutput);
        }
    }
}