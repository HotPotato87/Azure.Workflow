using Azure.Workflow.Core.Implementation;
using Azure.Workflow.Core.Interfaces;

namespace Azure.Workflow.Core.Entities.Environment
{
    public class WorkflowEnvironment
    {
        public IIocContainer IOCContainer { get; set; }

        public static WorkflowEnvironmentBuilder BuildEnvironment()
        {
            return new WorkflowEnvironmentBuilder(new WorkflowEnvironment());
        }

        public WorkflowSession CreateSession()
        {
            return new WorkflowSession(this);
        }
    }
}