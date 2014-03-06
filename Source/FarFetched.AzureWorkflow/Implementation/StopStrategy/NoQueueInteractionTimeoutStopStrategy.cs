using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Interfaces;

namespace ServerShot.Framework.Core.Implementation.StopStrategy
{
    /// <summary>
    /// Will finish the session when no queue interaction over the threshold time
    /// </summary>
    public class NoQueueInteractionTimeoutStopStrategy : IProcessingStopStrategy
    {
        private readonly TimeSpan _threshold;

        public NoQueueInteractionTimeoutStopStrategy(TimeSpan threshold)
        {
            _threshold = threshold;
        }

        public bool ShouldStop(ServerShotSessionBase session)
        {
            var processingSessions = session.RunningModules.OfType<IQueueProcessingServerShotModule>();

            if (processingSessions.Any(x => x.LastRecieved != DateTime.MinValue))
            {
                var lastReceived = processingSessions.Where(x=>x.LastRecieved != DateTime.MinValue).Max(x => x.LastRecieved);
                var timeSinceLastRecieved = DateTime.Now.Subtract(lastReceived);

                if (timeSinceLastRecieved.TotalMilliseconds > _threshold.TotalMilliseconds)
                {
                    return true;
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
