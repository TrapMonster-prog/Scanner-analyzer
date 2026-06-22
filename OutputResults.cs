using Scanner_analyzer.Components;

namespace Scanner_analyzer
{
    public class OutputResults
    {
        private readonly Dictionary<int, IMethods> modules;

        public OutputResults()
        {
            modules = new Dictionary<int, IMethods>
            {
                {
                    1, new InformationOS()
                },
                {
                    2, new InformationGeneral()
                },
                {
                    3, new InformationUpdates()
                },
                {
                    4, new InformationAdmins()
                },
                {
                    5, new InformationPasswordPolicy()
                },
                {
                    6, new InformationAudit()
                },
                {
                    7, new InformationNetwork()
                },
                {
                    8, new InformationShares()
                },
                {
                    9, new InformationServices()
                },
                {
                    10, new InformationFileSystem()
                },
                {
                    11, new InformationRegistry()
                },
                {
                    12, new InformationGroupPolicy()
                },
                {
                    13, new InformationAdvanced()
                },
                {
                    14, new InformationEventLog()
                },
                {
                    15, new InformationPortScan()
                },
                {
                    16, new InformationAllOutput()
                },
            };
        }

        public void Start()
        {
            ConsoleEncoding.Apply();
            Console.Title = "Сканер безопасности ОС";
            Console.WriteLine("Сканер безопасности операционной системы (Windows 8)");
            var userChecker = new InformationUsers();
            userChecker.Collect();
            userChecker.OutputInformation();

            ShowMenu();
        }

        private void ShowMenu()
        {
            while (true)
            {
                Console.WriteLine("\n");
                Console.WriteLine("Меню выбора модуля");
                Console.WriteLine("");
                Console.WriteLine("1  – Вид операционной системы");
                Console.WriteLine("2  – Имя узла, рабочей группы/домена");
                Console.WriteLine("3  – Установленные обновления");
                Console.WriteLine("4  – Администраторы");
                Console.WriteLine("5  – Политика паролей");
                Console.WriteLine("6  – Аудит системы");
                Console.WriteLine("7  – Сетевые настройки");
                Console.WriteLine("8  – Открытые ресурсы");
                Console.WriteLine("9  – Службы");
                Console.WriteLine("10 – Права доступа к папкам");
                Console.WriteLine("11 – Разрешения реестра");
                Console.WriteLine("12 – Групповые политики");
                Console.WriteLine("13 – Дополнительные параметры");
                Console.WriteLine("14 – Анализ журналов");
                Console.WriteLine("15 – Обнаружение сканирования портов");
                Console.WriteLine("16 – Полный вывод ");
                Console.WriteLine("0  – Выход");
                Console.WriteLine("");
                Console.Write("Ваш выбор: ");

                string? input = Console.ReadLine();
                if (input == "0" || input == "")
                    break;

                if (int.TryParse(input, out int choice) && modules.ContainsKey(choice))
                {
                    Console.Clear();
                    var module = modules[choice];
                    try
                    {
                        module.Collect();
                        module.OutputInformation();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при работе модуля: {ex.Message}");
                    }
                    finally
                    {
                        ConsoleEncoding.Apply();
                    }
                }
                else
                {
                    Console.WriteLine("Неверный ввод. Повторите.");
                }
            }
        }
    }
}