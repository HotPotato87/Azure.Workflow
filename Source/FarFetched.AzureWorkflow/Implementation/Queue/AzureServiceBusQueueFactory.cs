using FarFetched.AzureWorkflow.Core.Architecture;
using FarFetched.AzureWorkflow.Core.Implementation;
using FarFetched.AzureWorkflow.Core.Interfaces;

namespace FarFetched.AzureWorkflow.Core.ServiceBus
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

        public ICloudQueue CreateQueue(IWorkflowModule module)
        {
            return new AzureServiceBusQueue(module.QueueName, new ServiceBusQueueSettings());
        }
    }
}