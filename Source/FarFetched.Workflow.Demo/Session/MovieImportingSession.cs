using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core.Architecture;
using FarFetched.AzureWorkflow.Core.Builder;
using FarFetched.AzureWorkflow.Core.Implementation;
using FarFetched.AzureWorkflow.Core.Implementation.Logging;
using FarFetched.AzureWorkflow.Core.Implementation.Reports;
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
                    .AddModule(new MetacriticModule())
                    .WithQueueMechanism(new InMemoryQueueFactory())
                    .AttachLogger(new ConsoleLogger())
                    .AttachReportGenerator(new SendGridReportGenerator())
                    .RunAsync();

                Console.Write("Done");
            });

            Console.WriteLine("Running..");
            Console.ReadLine();
        }
    }
}
