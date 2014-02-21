using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Implementation;

namespace ServerShot.Framework.Core.Plugins.Alerts
{
    public abstract class LogManagerBase : ServerShotSessionPluginBase
    {
        internal override void OnModuleStarted(IServerShotModule module)
        {
            module.OnLogMessage += message => OnLogMessage(module, new LogMessage(message));
        }

        public abstract void OnLogMessage(IServerShotModule module, LogMessage message);
    }
}
