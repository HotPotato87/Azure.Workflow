using System;
using System.Threading.Tasks;
using Azure.Workflow.Core.Entities;
using Azure.Workflow.Core.Implementation;
using Azure.Workflow.Core.Implementation.Logging;
using Azure.Workflow.Core.Implementation.Reporting;
using Azure.Workflow.Core.ServiceBus;
using Azure.Workflow.Core.Builder;

namespace Azure.Workflow.Demo.Session
{
    public class Start
    {
        public static void Main()
        {
            Task.Run(async () =>
            {
                await WorkflowSession.StartBuild()
                    .AddModule(new RottenTomatoesModule())
                    .AddModule(new MetacriticModule(new WorkflowModuleSettings() { QueueWaitTimeBeforeFinish = TimeSpan.FromSeconds(5) }))
                    .ConfigureSessionSettings(new WorkflowSessionSettings())
                    .WithQueueMechanism(new AzureServiceBusQueueFactory(new ServiceBusQueueSettings() { ConnectionString = DemoSettings.Default.ServiceBusConnectionString }))
                    .AttachLogger(new ConsoleLogger())
                    .AttachAlertManager(new SendGridAlertManager())
                    .AttachReportGenerator(new ConsoleReportGenerator())
                    .RunAsync();

                Console.Write("Done.. press any key to exit");
            });

            Console.WriteLine("Running..");
            Console.ReadLine();
        }
    }
}
