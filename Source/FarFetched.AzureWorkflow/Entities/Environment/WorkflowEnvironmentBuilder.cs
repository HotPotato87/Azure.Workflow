namespace ServerShot.Framework.Core.Entities.Environment
{
    public class WorkflowEnvironmentBuilder
    {
        public WorkflowEnvironmentBuilder(WorkflowEnvironment workflowEnvironment)
        {
            Environment = workflowEnvironment;
        }

        public WorkflowEnvironment Environment { get; private set; }
    }
}