using FarFetched.AzureWorkflow.Core.Implementation;

namespace FarFetched.AzureWorkflow.Core
{
    public abstract class InitialWorkflowModule<T> : WorkflowModuleBase<T> where T : class
    {
        public InitialWorkflowModule(WorkflowModuleSettings settings = default(WorkflowModuleSettings))
            :base(settings)
        {
            
        }

    }
}