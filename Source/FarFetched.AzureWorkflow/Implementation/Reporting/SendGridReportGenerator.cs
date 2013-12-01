using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Workflow.Core.Interfaces;
using Azure.Workflow.Core.Plugins;
using Azure.Workflow.Core.Architecture;

namespace Azure.Workflow.Core.Implementation.Reporting
{
    public class SendGridReportGenerator : ReportGenerationPlugin
    {
        public override void SendSessionReport(WorkflowSession workflowSession, IEnumerable<ModuleProcessingSummary> moduleSummaries)
        {
            throw new NotImplementedException();
        }
    }
}
