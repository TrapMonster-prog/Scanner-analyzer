using System.Management;
namespace Scanner_analyzer.Components
{
    public class InformationGroupPolicy : IMethods
    {
        private string gpOutput = "";
        public void Collect()
        {
            bool rsopSuccess = false;
            try
            {
                var searcher = new ManagementObjectSearcher(
                    "root\\RSOP\\Computer",
                    "SELECT * FROM RSOP_Session");
                var results = searcher.Get();
                if (results.Count > 0)
                {
                    rsopSuccess = true;
                    gpOutput = "Применённые групповые политики (RSOP):\n\n";
                    foreach (ManagementObject session in results)
                    {
                        gpOutput += $"  Сессия: {session["Name"]}\n";
                        gpOutput += $"  Время: {session["CreationTime"]}\n\n";
                    }
                }
            }
            catch (Exception ex)
            {
                gpOutput = $"[WMI RSOP ошибка] {ex.GetType().Name}\n";
            }
            if (!rsopSuccess)
            {
                try
                {
                    gpOutput += "\nПолучаю данные через gpresult...\n\n";
                    string outData = ConsoleEncoding.RunConsoleProcess("gpresult", "/r /scope computer");

                    if (!string.IsNullOrWhiteSpace(outData))
                    {
                        if (outData.Contains("отказано в доступе", StringComparison.OrdinalIgnoreCase) ||
                            outData.Contains("access denied", StringComparison.OrdinalIgnoreCase))
                        {
                            gpOutput = "Требуется запуск от имени администратора!\n\n" +
                                      "Для просмотра групповых политик:\n" +
                                      "1) Запустите сканер как администратор\n" +
                                      "2) Или выполните: gpresult /r";
                        }
                        else
                        {
                            gpOutput = outData;
                        }
                    }
                    else
                    {
                        gpOutput += "gpresult не вернул данных.";
                    }
                }
                catch (Exception ex)
                {
                    gpOutput = $"Не удалось получить данные о групповых политиках: {ex.Message}\n\n" +
                              "Попробуйте:\n" +
                              "1) Запустить от имени администратора\n" +
                              "2) Выполнить вручную: gpresult /r > gp.txt";
                }
            }
        }
        public void OutputInformation()
        {
            Console.WriteLine("\n12. Групповые политики");
            Console.WriteLine(gpOutput);
        }
    }
}