namespace Scanner_analyzer.Components
{
    internal class InformationAllOutput : IMethods
    {
        private readonly List<IMethods> _modules;
        public InformationAllOutput() 
        { 
            _modules = new List<IMethods> 
        {
                new InformationOS(),
                new InformationGeneral(),
                new InformationUpdates(),
                new InformationAdmins(),
                new InformationPasswordPolicy(),
                new InformationAudit(),
                new InformationNetwork(),
                new InformationShares(),
                new InformationServices(),
                new InformationFileSystem(),
                new InformationRegistry(),
                new InformationGroupPolicy(),
                new InformationAdvanced(),
                new InformationEventLog(),
                new InformationPortScan()
            };
        }
        public void Collect() {
            foreach (var module in _modules) {
                module.Collect();
            }
        }
        public void OutputInformation() {
            ConsoleEncoding.Apply();
            Console.Clear();
            Console.WriteLine(new string('=', 70));
            Console.WriteLine("  ПОЛНЫЙ ОТЧЁТ СКАНЕРА БЕЗОПАСНОСТИ КОНФИГУРАЦИИ WINDOWS 8");
            Console.WriteLine($"  Сформирован: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            Console.WriteLine(new string('=', 70));
            var adminCheck = new InformationUsers();
            adminCheck.Collect();
            Console.WriteLine($"\n[Статус запуска] {adminCheck.Message}");
            if (!adminCheck.IsAdmin) Console.WriteLine("[!] Часть данных может быть ограничена. Рекомендуется запуск от имени Администратора.\n");
            for (int i = 0; i < _modules.Count; i++) {
                try {
                    Console.WriteLine($"\n>>> ПУНКТ {i + 1} из 15 <<<");
                    _modules[i].OutputInformation();
                }
                catch (Exception ex) {
                    Console.WriteLine($"[Ошибка вывода пункта {i + 1}]: {ex.Message}");
                }
                Console.WriteLine(new string('-', 70));
            }
            Console.WriteLine("\n[ОТЧЁТ ЗАВЕРШЁН]");
            Console.WriteLine("Для возврата в главное меню нажмите любую клавишу...");
            Console.ReadKey();
        }
    }
}