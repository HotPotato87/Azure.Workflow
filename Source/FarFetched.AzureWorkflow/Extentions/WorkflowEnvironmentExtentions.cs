using Azure.Workflow.Core.Entities.Environment;
using Azure.Workflow.Core.Interfaces;

namespace Azure.Workflow.Core.Extentions
{
    public static class WorkflowEnvironmentExtentions
    {
        public static WorkflowEnvironmentWithIOCBuilder WithIOCContainer(this WorkflowEnvironmentBuilder builder, IIocContainer container)
        {
            builder.Environment.IOCContainer = container;

            return new WorkflowEnvironmentWithIOCBuilder(builder.Environment);
        }

        public static WorkflowEnvironment Build(this WorkflowEnvironmentBuilder builder)
        {
            return builder.Environment;
        }

        public static WorkflowEnvironmentWithIOCBuilder RegisterType<T, T2>(this WorkflowEnvironmentWithIOCBuilder builder)
        {
            builder.Environment.IOCContainer.Bind(typeof (T), typeof (T2));

            return builder;
        }
    }

    public class WorkflowEnvironmentWithIOCBuilder : WorkflowEnvironmentBuilder
    {
        public WorkflowEnvironmentWithIOCBuilder(WorkflowEnvironment workflowEnvironment) : base(workflowEnvironment)
        {
        }
    }
}