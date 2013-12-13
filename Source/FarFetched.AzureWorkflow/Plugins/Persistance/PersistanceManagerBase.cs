using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Workflow.Core.Architecture;

namespace Azure.Workflow.Core.Plugins.Persistance
{
    public abstract class PersistanceManagerBase : WorkflowSessionPluginBase
    {
        internal override void OnModuleStarted(IWorkflowModule module)
        {
            module.OnStore += OnStore;
            module.OnRetrieve += ModuleOnOnRetrieve;
        }

        protected abstract void OnStore(string key, object o);
        protected abstract object ModuleOnOnRetrieve(string s);

    }
}
