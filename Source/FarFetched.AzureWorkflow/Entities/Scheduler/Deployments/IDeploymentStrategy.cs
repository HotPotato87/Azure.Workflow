using System.Threading.Tasks;
using ServerShot.Framework.Core.Implementation;

namespace ServerShot.Framework.Core.Entities.Scheduler.Deployments
{
    public interface IDeploymentStrategy
    {
        Task Deploy(ServerShotSession session);
    }
}