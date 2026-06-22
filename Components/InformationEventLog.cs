using System.Diagnostics;

namespace Scanner_analyzer.Components
{
    public class InformationEventLog : IMethods
    {
        private readonly List<string> output = new();

        public void Collect()
        {
            output.Clear();

            try
            {
                EventLog systemLog = new EventLog("System");

                var entries = systemLog.Entries
                    .Cast<EventLogEntry>()
                    .OrderByDescending(e => e.TimeGenerated)
                    .Take(100)
                    .ToList();

                output.Add("Последние события:\n");

                foreach (var entry in entries.Take(15))
                {
                    output.Add(
                        $"[{entry.TimeGenerated}] " +
                        $"[{entry.EntryType}] " +
                        $"{entry.Source} -> " +
                        $"{ShortMessage(entry.Message)}");
                }

                output.Add("");
                output.Add("Повторяющиеся события:\n");

                var grouped = entries
                    .GroupBy(e => e.Source)
                    .OrderByDescending(g => g.Count());

                foreach (var group in grouped)
                {
                    if (group.Count() >= 3)
                    {
                        output.Add(
                            $"{group.Key} : {group.Count()} событий");
                    }
                }

                output.Add("");
                output.Add("Критические ошибки:\n");

                var critical = entries
                    .Where(e =>
                        e.EntryType == EventLogEntryType.Error ||
                        e.EntryType == EventLogEntryType.Warning)
                    .Take(10);

                foreach (var item in critical)
                {
                    output.Add(
                        $"[{item.EntryType}] " +
                        $"{item.Source} -> " +
                        $"{ShortMessage(item.Message)}");
                }

                if (output.Count == 0)
                {
                    output.Add("Журналы пусты.");
                }
            }
            catch (Exception ex)
            {
                output.Add($"Ошибка чтения журналов: {ex.Message}");
            }
        }
        private string ShortMessage(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg))
                return "";

            msg = msg.Replace("\r", " ")
                     .Replace("\n", " ");

            if (msg.Length > 120)
                return msg.Substring(0, 120) + "...";

            return msg;
        }

        public void OutputInformation()
        {
            Console.WriteLine("\n14. Анализ журналов Windows");
            Console.WriteLine(new string('-', 70));

            foreach (var line in output)
            {
                Console.WriteLine(line);
            }

            Console.WriteLine(new string('-', 70));
        }
    }
}