using ServerShot.Framework.Core.Plugins.Alerts;
using Servershot.Framework.Entities.WebJob;

namespace ServerShot.Framework.Core.Plugins
{
    public class ProwlAlertManager : WebJobSessionPluginBase
    {
        public override void RegisterWebjob(IJibJobModule module)
        {
            base.RegisterWebjob(module);

            module.Fail += ModuleOnFail;
            module.Alert += ModuleOnAlert;
        }

        private void ModuleOnAlert(Alert obj)
        {
            //todo : send alert
        }

        private void ModuleOnFail()
        {
            //todo : send prowl message
        }
    }
}