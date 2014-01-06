using System.Threading.Tasks;
using Azure.Workflow.Core.Implementation;

namespace Azure.Workflow.Core.Entities.Scheduler.Deployments
{
    public interface IDeploymentStrategy
    {
        Task Deploy(WorkflowSession session);
    }
}