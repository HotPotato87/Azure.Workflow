using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core.Architecture;
using FarFetched.AzureWorkflow.Core.Interfaces;
using FarFetched.AzureWorkflow.Core.Plugins;

namespace FarFetched.AzureWorkflow.Core.Implementation.Reporting
{
    public class SendGridReportGenerator : ReportGenerationPlugin
    {
        public override void SendSessionReport(WorkflowSession workflowSession, IEnumerable<ModuleProcessingSummary> moduleSummaries)
        {
            throw new NotImplementedException();
        }
    }
}
