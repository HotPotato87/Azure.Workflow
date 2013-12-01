using System.Threading.Tasks;
using Azure.Workflow.Core.Implementation;

namespace Azure.Workflow.Core
{
    public abstract class InitialWorkflowModule<T> : WorkflowModuleBase<T> where T : class
    {
        public InitialWorkflowModule(WorkflowModuleSettings settings = default(WorkflowModuleSettings))
            :base(settings)
        {
            
        }
    }
}