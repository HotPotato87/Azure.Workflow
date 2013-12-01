using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Azure.Workflow.Core.Interfaces;
using Azure.Workflow.Core.Plugins;

namespace Azure.Workflow.Core.Implementation.Reporting
{
    public class ConsoleReportGenerator : ReportGenerationPlugin
    {
        public override void SendSessionReport(WorkflowSession workflowSession, IEnumerable<ModuleProcessingSummary> moduleSummaries)
        {
            Console.WriteLine();
            Console.WriteLine("*****************************");
            Console.WriteLine("********* SUMMARY ***********");
            Console.WriteLine("*****************************");
            Console.WriteLine("{0} Total Processed", moduleSummaries.Sum(x=>x.TotalProcessed));
            Console.WriteLine("{0} Running Time", "(To be implemented)");
            Console.WriteLine("{0} Errors", moduleSummaries.Sum(t=>t.Errors));
            InsertModuleStates();
            Console.WriteLine();

            foreach (var processingSummary in moduleSummaries)
            {
                Console.WriteLine();
                Console.WriteLine("[{0}]", processingSummary.Module.QueueName);
                Console.WriteLine("-------------------------------------");
                foreach (var category in processingSummary.ResultCategories)
                {
                    Console.WriteLine("[{0} : {1} items]", category.Key, category.Value);
                }

                foreach (var detail in processingSummary.ResultCategoryExtraDetail)
                {
                    detail.Value.ForEach(x=>Console.WriteLine("----{0}:{1}", x.ProcessedTime, x.Message));
                }
                Console.WriteLine();
                Console.WriteLine();
            }

            Console.WriteLine("********* Finished! *********");
        }

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