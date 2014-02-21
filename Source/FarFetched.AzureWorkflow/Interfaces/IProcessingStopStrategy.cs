using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Implementation;

namespace ServerShot.Framework.Core.Interfaces
{
    public interface IProcessingStopStrategy
    {
        bool ShouldStop(ServerShotSession session);
        bool ShouldSpecificModuleStop(IServerShotModule module);
    }
}
