using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Implementation;

namespace ServerShot.Framework.Core.Plugins.Alerts
{
    public abstract class AlertManagerBase : ServerShotSessionBasePluginBase
    {
        public override void OnModuleStarted(IServerShotModule module)
        {
            module.OnAlert += this.FireAlert;
        }

        public abstract void FireAlert(Alert alert);
    }
}
