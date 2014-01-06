using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Workflow.Core.Entities.Scheduler;
using Azure.Workflow.Core.Entities.Scheduler.Deployments;
using Azure.Workflow.Core.Implementation;
using Quartz;

namespace Azure.Workflow.Core.Extentions
{
    public static class WorkflowSchedulerExtentions
    {
        public static WorkflowSchedulerWithSessionBuilder AddSession(this WorkflowSchedulerBuilder builder, WorkflowSession session)
        {
            builder.Scheduler.Session.Add(session);

            return new WorkflowSchedulerWithSessionBuilder(builder.Scheduler);
        }

        public static Task BeginSchedulerAsync(this WorkflowSchedulerBuilder builder)
        {
            return builder.Scheduler.RunAsync();
        }
    }

    public static class WorkflowSchedulerWithSessionBuilderExtentions
    {
        public static WorkflowSchedulerWithSessionBuilder WithDeploymentStrategy(this WorkflowSchedulerWithSessionBuilder builder, IDeploymentStrategy deploymentStrategy)
        {
            builder.Scheduler.DeploymentStrategy = deploymentStrategy;

            return builder;
        }

        public static WorkflowSchedulerWithSessionBuilder WithTrigger(this WorkflowSchedulerWithSessionBuilder builder, ITrigger trigger)
        {
            throw new NotImplementedException();
        }
    }
}
