using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Workflow.Core.Entities.Scheduler.Deployments;
using Azure.Workflow.Core.Implementation;

namespace Azure.Workflow.Core.Entities.Scheduler
{
    /// <summary>
    /// This component manages the deployment and execution of workflow sessions.
    /// 
    /// It is primarily managed through a 'Fluent' builder API. <see cref="Build"/>
    /// 
    /// </summary>
    public class WorkflowScheduler
    {
        public List<WorkflowSession> Session { get; internal set; }
        public IDeploymentStrategy DeploymentStrategy { get; internal set; }

        public static WorkflowSchedulerBuilder Build()
        {
            return new WorkflowSchedulerBuilder(new WorkflowScheduler());
        }

        internal WorkflowScheduler()
        {
            Session = new List<WorkflowSession>();
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