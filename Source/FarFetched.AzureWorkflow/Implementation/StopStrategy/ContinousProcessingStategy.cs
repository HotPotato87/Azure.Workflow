using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Enums;
using ServerShot.Framework.Core.Interfaces;

namespace ServerShot.Framework.Core.Implementation.StopStrategy
{
    /// <summary>
    /// This processing strategy will ensure that any queue based processing gets run indefinately.
    /// 
    /// The only case in which the session is stopped is that all modules are set to State = Finished.
    /// 
    /// </summary>
    public class ContinousProcessingStategy : IProcessingStopStrategy
    {
        public bool ShouldStop(ServerShotSessionBase session)
        {
            return session.RunningModules.All(x => x.State == ModuleState.Finished);
        }

        /// <summary>
        /// all modules continue processing
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public bool ShouldSpecificModuleStop(IServerShotModule module)
        {
            return false;
        }
    }
}
