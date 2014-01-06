using System.Threading.Tasks;
using Azure.Workflow.Core.Implementation;

namespace Azure.Workflow.Core.Entities.Scheduler.Deployments
{
    public class LocalDeploymentStrategy : IDeploymentStrategy
    {
        //Runs in memory
        public Task Deploy(WorkflowSession session)
        {
            return session.Start();
        }
    }
}