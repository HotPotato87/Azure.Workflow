using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Implementation;
using ServerShot.Framework.Core.Interfaces;

namespace ServerShot.Framework.Core.ServiceBus
{
    public class AzureServiceBusQueueFactory : ICloudQueueFactory
    {
        private ServiceBusQueueSettings _settings;

        public AzureServiceBusQueueFactory(ServiceBusQueueSettings settings = null)
        {
            if (settings == null)
            {
                settings = new ServiceBusQueueSettings();
            }
            _settings = settings;
        }

        public ICloudQueue CreateQueue(IServerShotModule module)
        {
            return new AzureServiceBusQueue(module.QueueName, _settings);
        }
    }
}