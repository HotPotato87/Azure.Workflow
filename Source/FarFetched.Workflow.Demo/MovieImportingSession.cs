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
        public static async Task Main()
        {
            var movieImporter = new WorkflowSession();

            await movieImporter.Start();
        }

        public static async Task Main2()
        {
            await WorkflowSession.StartBuild()
                .AddModule(new FlixterMovieModule())
                .AddModule(new RottenTomatoesModule())
            .WithQueueMechanism(new InMemoryQueueFactory())
            .AttachLogger(new ConsoleLogger())
            .AttachReportGenerator(new SendGridReportGenerator())
            .RunAsync();

            Console.Write("Done");
        }
    }
}
