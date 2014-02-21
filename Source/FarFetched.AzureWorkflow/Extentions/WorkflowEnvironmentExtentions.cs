using ServerShot.Framework.Core.Entities.Environment;
using ServerShot.Framework.Core.Interfaces;

namespace ServerShot.Framework.Core.Extentions
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