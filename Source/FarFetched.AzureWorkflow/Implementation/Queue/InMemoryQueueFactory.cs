using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Interfaces;

namespace ServerShot.Framework.Core.ServiceBus
{
    public class InMemoryQueueFactory : ICloudQueueFactory
    {
        public ICloudQueue CreateQueue(IServerShotModule module)
        {
            return new InMemoryQueue();
        }
    }
}