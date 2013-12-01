using Azure.Workflow.Core.Architecture;
using Azure.Workflow.Core.Interfaces;

namespace Azure.Workflow.Core.ServiceBus
{
    public class InMemoryQueueFactory : ICloudQueueFactory
    {
        public ICloudQueue CreateQueue(IWorkflowModule module)
        {
            return new InMemoryQueue();
        }
    }
}