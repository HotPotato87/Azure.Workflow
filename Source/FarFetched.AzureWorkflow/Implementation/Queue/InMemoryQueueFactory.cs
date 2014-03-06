using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Interfaces;

namespace ServerShot.Framework.Core.Queue
{
    public class InMemoryQueueFactory : ICloudQueueFactory
    {
        public ICloudQueue CreateQueue(IServerShotModule module)
        {
            return new InMemoryQueue();
        }
    }
}