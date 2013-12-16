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
        public string TableName { get; set; }

        public PersistanceManagerBase()
        {
            
        }

        internal override void OnModuleStarted(IWorkflowModule module)
        {
            this.TableName = module.Session.SessionName;
            module.OnStoreAsync += OnStoreAsync;
            module.OnRetrieveAsync += OnRetrieveAsync;
        }

        public abstract Task OnStoreAsync(string key, object o);
        public abstract Task<object> OnRetrieveAsync(string key);
        
    }
}
