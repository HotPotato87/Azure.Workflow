using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Workflow.Core.Architecture;
using Azure.Workflow.Core.Implementation;

namespace Azure.Workflow.Core.Plugins.Alerts
{
    public abstract class LogManagerBase : WorkflowSessionPluginBase
    {
        internal override void OnModuleStarted(IWorkflowModule module)
        {
            module.OnLogMessage += message =>
            {
                OnLogMessage(new LogMessage(message));
            };
        }

        public abstract void OnLogMessage(LogMessage message);
    }
}
