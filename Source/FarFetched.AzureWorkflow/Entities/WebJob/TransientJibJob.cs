using System.Threading.Tasks;

namespace Servershot.Framework.Entities.WebJob
{
    public abstract class TransientJibJob : JibJobModule, ITransientJibJob
    {
        public async Task Triggered()
        {
            await OnTriggered();
            Processed();
        }

        protected abstract Task OnTriggered();
    }
}