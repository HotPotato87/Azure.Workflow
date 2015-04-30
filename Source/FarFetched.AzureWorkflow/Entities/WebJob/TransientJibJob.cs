using System.Threading.Tasks;

namespace Servershot.Framework.Entities.WebJob
{
    public abstract class TransientJibJob : JibJobModule, ITransientJibJob
    {
        public Task Triggered()
        {
            return OnTriggered();
        }

        protected abstract Task OnTriggered();
    }
}