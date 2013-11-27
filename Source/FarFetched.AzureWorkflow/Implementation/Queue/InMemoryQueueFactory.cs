using FarFetched.AzureWorkflow.Core.Architecture;
using FarFetched.AzureWorkflow.Core.Interfaces;

namespace FarFetched.AzureWorkflow.Core.ServiceBus
{
    public class InMemoryQueueFactory : ICloudQueueFactory
    {
        public ICloudQueue CreateQueue(IWorkflowModule module)
        {
            return new InMemoryQueue();
        }
    }
}