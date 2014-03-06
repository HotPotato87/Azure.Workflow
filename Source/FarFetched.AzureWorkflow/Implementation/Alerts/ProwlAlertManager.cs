using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Plugins.Alerts;
using ServerShot.Framework.Core.Interfaces;

namespace ServerShot.Framework.Core.Implementation
{
    public class ProwlAlertManager : AlertManagerBase
    {
        public override void FireAlert(Alert alert)
        {
            
        }
    }

    public class ConsoleAlertManager : AlertManagerBase
    {
        public override void FireAlert(Alert alert)
        {
            Console.WriteLine("Alert fired : ({0}) - {1}", alert.AlertLevel, alert.Message);
        }
    }
}
