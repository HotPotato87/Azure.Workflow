using System;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Plugins.Alerts;

namespace Servershot.Framework.Entities.WebJob
{
    public abstract class TransientJibJob : JibJobModule, ITransientJibJob
    {
        public async Task Triggered()
        {
            try
            {
                await OnTriggered();
            }
            catch (Exception eX)
            {
                base.Fail();
                base.Alert(new Alert()
                {
                    AlertLevel = AlertLevel.High,
                    Message = eX.StackTrace
                });
                return;
            }
            
            Processed();
        }

        protected abstract Task OnTriggered();
    }
}