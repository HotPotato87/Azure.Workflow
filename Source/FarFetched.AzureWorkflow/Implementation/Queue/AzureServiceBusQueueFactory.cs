using Azure.Workflow.Core.Architecture;
using Azure.Workflow.Core.Implementation;
using Azure.Workflow.Core.Interfaces;

namespace Azure.Workflow.Core.ServiceBus
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
            return new AzureServiceBusQueue(module.QueueName, _settings);
        }
    }
}