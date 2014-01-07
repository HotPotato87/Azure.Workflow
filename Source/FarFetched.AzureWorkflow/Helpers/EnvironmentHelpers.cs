using Azure.Workflow.Core.Entities.Environment;

namespace Azure.Workflow.Core.Helpers
{
    public class EnvironmentHelpers
    {
        public static WorkflowEnvironment BuildStandardEnvironment()
        {
            return new WorkflowEnvironment();
        }
    }
}