using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Interfaces;
using ServerShot.Framework.Core.Plugins;

namespace ServerShot.Framework.Core.Implementation.Reporting
{
    public class ConsoleReportGenerator : ReportGenerationPluginBase
    {
        public override async Task SendSessionReportAsync(ServerShotSessionBase session, IEnumerable<ModuleProcessingSummary> moduleSummaries)
        {

            Console.WriteLine();
            Console.WriteLine("*****************************");
            Console.WriteLine("********* SUMMARY ***********");
            Console.WriteLine("*****************************");
            Console.WriteLine("{0} Total Processed", moduleSummaries.Sum(x=>x.TotalProcessed));
            Console.WriteLine("{0} Running Time", session.Ended.Subtract(session.Started).TotalSeconds + "s");
            Console.WriteLine("{0} Errors", moduleSummaries.Sum(t=>t.Errors));
            InsertModuleStates();
            Console.WriteLine();

            foreach (var processingSummary in moduleSummaries)
            {
                Console.WriteLine(" ");
                Console.WriteLine("[{0}]", processingSummary.Module.QueueName);
                Console.WriteLine("-------------------------------------");
                Console.WriteLine("Total Processed {0}", processingSummary.TotalProcessed);
                Console.WriteLine("Started {0}", processingSummary.Module.Started);
                Console.WriteLine("Finished {0}", processingSummary.Module.Ended);

                foreach (var sentToAudit in processingSummary.Module.SentToAudit)
                {
                    Console.WriteLine("{0}:{1} Sent Count", sentToAudit.Key, sentToAudit.Value);
                }

                foreach (var category in processingSummary.ResultCategories)
                {
                    Console.WriteLine("[{0} : {1} items]", category.Key, category.Value);
                }

                if (WorkflowDetail != null)
                {
                    Console.WriteLine(WorkflowDetail(processingSummary.Module));
                }

                foreach (var detail in processingSummary.ResultCategoryExtraDetail)
                {
                    detail.Value.ForEach(x=>Console.WriteLine("----{0}:{1}", x.ProcessedTime, x.Message));
                }

                Console.WriteLine(" ");
            }

            Console.WriteLine("********* Finished! *********");
        }

        public Func<IServerShotModule, string> WorkflowDetail { get; set; }

        //All 5 modules completed successfully
        //4 modules completed successfully, 1 error
        private void InsertModuleStates()
        {
            //if error
            PrintErrors();
        }

        private void PrintErrors()
        {
            
        }
    }
}