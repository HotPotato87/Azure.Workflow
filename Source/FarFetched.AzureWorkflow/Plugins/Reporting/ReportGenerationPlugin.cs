using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core.Architecture;
using FarFetched.AzureWorkflow.Core.Implementation;
using FarFetched.AzureWorkflow.Core.Interfaces;

namespace FarFetched.AzureWorkflow.Core.Plugins
{
    public abstract class ReportGenerationPlugin : WorkflowSessionPluginBase
    {
        public List<ModuleProcessingSummary> ModuleProcessingSummaries { get; set; }

        internal override void OnSessionStarted(WorkflowSession session)
        {
            base.OnSessionStarted(session);

            session.OnSessionFinished += workflowSession =>
            {
                this.SendSessionReport(ModuleProcessingSummaries);
            };
        }

        internal override void OnModuleStarted(IWorkflowModule module)
        {
            if (module is IProcessingWorkflowModule)
            {
                (module as IProcessingWorkflowModule).OnProcessed += o =>
                {
                    this.ModuleProcessingSummaries.Single(x => x.Module == module).SuccessfullyProcessed++;
                };    
            }
            
            module.OnError += exception =>
            {
                var moduleSummary = this.ModuleProcessingSummaries.Single(x => x.Module == module);
                moduleSummary.Errors++;
                moduleSummary.ErrorList.Add(exception);
            };
            module.OnFinished += () =>
            {
                var moduleSummary = this.ModuleProcessingSummaries.Single(x => x.Module == module);
                moduleSummary.Duration = module.Ended.Subtract(module.Started);
            };
        }

        public abstract void SendSessionReport(IEnumerable<ModuleProcessingSummary> moduleSummaries);
    }
}
