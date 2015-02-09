using System.Linq;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Interfaces;

namespace ServerShot.Framework.Core.Implementation.StopStrategy
{
    public class EmptyQueueCountStopStrategy : IProcessingStopStrategy
    {
        public bool ShouldStop(ServerShotSessionBase session)
        {
            return session.RunningModules.OfType<IQueueProcessingServerShotModule>().All(t => t.Queue.Count == 0);
        }

        public bool ShouldSpecificModuleStop(IServerShotModule module)
        {
            return false;
        }
    }
}