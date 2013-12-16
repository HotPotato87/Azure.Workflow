using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Workflow.Core.Architecture;
using Azure.Workflow.Core.Implementation;

namespace Azure.Workflow.Core.Plugins
{
    public abstract class WorkflowSessionPluginBase
    {
        protected WorkflowSession Session { get; set; }

        internal virtual void OnSessionStarted(WorkflowSession session)
        {
            this.Session = session;

            session.RunningModules.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (var module in  args.NewItems)
                    {
                        this.OnModuleStarted(module as IWorkflowModule);
                    }    
                }
            };
        }

        internal abstract void OnModuleStarted(IWorkflowModule module);

        public virtual string Validate(WorkflowSession module)
        {
            return null;
        }
    }
}
