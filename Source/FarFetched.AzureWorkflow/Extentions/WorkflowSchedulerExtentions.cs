using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using ServerShot.Framework.Core.Entities.Scheduler;
using ServerShot.Framework.Core.Entities.Scheduler.Deployments;
using ServerShot.Framework.Core.Implementation;

namespace ServerShot.Framework.Core.Extentions
{
    public static class WorkflowSchedulerExtentions
    {
        public static ServerShotSchedulerWithSessionBuilder AddSession(this ServerShotSchedulerBuilder builder, ServerShotSessionBase session)
        {
            builder.Scheduler.Session.Add(session);

            return new ServerShotSchedulerWithSessionBuilder(builder.Scheduler);
        }

        public static Task BeginSchedulerAsync(this ServerShotSchedulerBuilder builder)
        {
            return builder.Scheduler.RunAsync();
        }
    }

    public static class WorkflowSchedulerWithSessionBuilderExtentions
    {
        public static ServerShotSchedulerWithSessionBuilder WithDeploymentStrategy(this ServerShotSchedulerWithSessionBuilder builder, IDeploymentStrategy deploymentStrategy)
        {
            builder.Scheduler.DeploymentStrategy = deploymentStrategy;

            return builder;
        }

        public static ServerShotSchedulerWithSessionBuilder WithTrigger(this ServerShotSchedulerWithSessionBuilder builder, ITrigger trigger)
        {
            throw new NotImplementedException();
        }
    }
}
