using System.ServiceProcess;

namespace Scanner_analyzer.Components
{
    internal class InformationServices : IMethods
    {
        public void Collect() 
        {
            
        }
        public void OutputInformation()
        {
            Console.WriteLine("\nЗапущенные службы");

            ServiceController[] services = ServiceController.GetServices();

            foreach (var service in services)
            {
                if (service.Status == ServiceControllerStatus.Running)
                {
                    Console.WriteLine($"Служба: {service.DisplayName}");
                }
            }
            Console.WriteLine();
        }
    }
}