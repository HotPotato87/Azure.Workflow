using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Implementation;

namespace ServerShot.Framework.Core.Plugins.Alerts
{
    public abstract class LogManagerBase : ServerShotSessionBasePluginBase, ILoggingManager
    {
        public override void OnModuleStarted(IServerShotModule module)
        {
            module.OnLogMessage += (message, category) => OnLogMessage(new LogMessage(message, category), module);
        }

        public abstract void OnLogMessage(LogMessage message, IServerShotModule module = null);
    }
}
