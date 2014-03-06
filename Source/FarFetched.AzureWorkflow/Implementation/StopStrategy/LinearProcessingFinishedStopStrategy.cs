using System.Linq;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Enums;
using ServerShot.Framework.Core.Interfaces;

namespace ServerShot.Framework.Core.Implementation.StopStrategy
{
    public class LinearProcessingFinishedStopStrategy : IProcessingStopStrategy
    {
        public bool ShouldStop(ServerShotSessionBase session)
        {
            var initial = session.RunningModules.FirstOrDefault(x => x is IInitialServerShotModule);

            if (initial != null)
            {
                if (initial.State == ModuleState.Finished)
                {
                    var initialProcessingCount = initial.ProcessedCount;

                    return session.RunningModules.GroupBy(x=>x.Queue).All(t => t.Sum(f=>f.ProcessedCount) >= initialProcessingCount);
                }
            }

            return false;
        }

        public bool ShouldSpecificModuleStop(IServerShotModule module)
        {
            return false;
        }
    }
}