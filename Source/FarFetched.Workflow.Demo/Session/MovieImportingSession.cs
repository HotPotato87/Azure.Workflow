using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core.Architecture;
using FarFetched.AzureWorkflow.Core.Builder;
using FarFetched.AzureWorkflow.Core.Entities;
using FarFetched.AzureWorkflow.Core.Implementation;
using FarFetched.AzureWorkflow.Core.Implementation.Logging;
using FarFetched.AzureWorkflow.Core.Implementation.Reporting;
using FarFetched.AzureWorkflow.Core.ServiceBus;

namespace FarFetched.Workflow.Demo
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
                    .WithQueueMechanism(new AzureServiceBusQueueFactory(new ServiceBusQueueSettings() { BatchCount = 5, ConnectionString = DemoSettings.Default.ServiceBusConnectionString }))
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
