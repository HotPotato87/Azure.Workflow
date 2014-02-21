using System.Threading.Tasks;
using ServerShot.Framework.Core.Implementation;

namespace ServerShot.Framework.Core.Entities.Scheduler.Deployments
{
    public class LocalDeploymentStrategy : IDeploymentStrategy
    {
        //Runs in memory
        public Task Deploy(ServerShotSession session)
        {
            return session.Start();
        }
    }
}