using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Workflow.Core.Architecture;
using Azure.Workflow.Core.Implementation;

namespace Azure.Workflow.Core.Plugins.Alerts
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
