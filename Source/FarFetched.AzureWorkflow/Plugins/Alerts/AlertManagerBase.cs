using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core.Architecture;
using FarFetched.AzureWorkflow.Core.Implementation;

namespace FarFetched.AzureWorkflow.Core.Plugins.Alerts
{
    public abstract class AlertManagerBase : WorkflowSessionPluginBase
    {
        internal override void OnModuleStarted(IWorkflowModule module)
        {
            module.OnAlert += this.FireAlert;
        }

        public abstract void FireAlert(Alert alert);
    }
}
