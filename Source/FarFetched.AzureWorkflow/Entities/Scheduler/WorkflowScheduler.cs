using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Entities.Scheduler.Deployments;
using ServerShot.Framework.Core.Implementation;

namespace ServerShot.Framework.Core.Entities.Scheduler
{
    /// <summary>
    /// This component manages the deployment and execution of workflow sessions.
    /// 
    /// It is primarily managed through a 'Fluent' builder API. <see cref="Build"/>
    /// 
    /// </summary>
    public class WorkflowScheduler
    {
        public List<ServerShotSessionBase> Session { get; internal set; }
        public IDeploymentStrategy DeploymentStrategy { get; internal set; }

        public static ServerShotSchedulerBuilder Build()
        {
            return new ServerShotSchedulerBuilder(new WorkflowScheduler());
        }

        internal WorkflowScheduler()
        {
            Session = new List<ServerShotSessionBase>();
        }

        internal async Task RunAsync()
        {
            ValidateScheduler();


        }

        private void ValidateScheduler()
        {
            if (this.Session == null || !this.Session.Any())
            {
                throw new WorkflowConfigurationException("There are no workflow sessions to execute. Please see AddSession() in the fluent API", null);
            }

            if (DeploymentStrategy == null)
            {
                throw new WorkflowConfigurationException("There is no deployment strategy added to this scheduler. Please add one before execution", null);
            }
        }
    }
}