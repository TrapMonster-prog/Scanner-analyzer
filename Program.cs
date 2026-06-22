namespace Scanner_analyzer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ConsoleEncoding.Apply();

            OutputResults outputResults = new OutputResults();
            outputResults.Start();

            Console.ReadKey();
        }
    }
}