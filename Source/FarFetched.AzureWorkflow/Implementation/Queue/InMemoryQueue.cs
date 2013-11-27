using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core.Implementation;
using FarFetched.AzureWorkflow.Core.Interfaces;
using Microsoft.ServiceBus.Messaging;

namespace FarFetched.AzureWorkflow.Core.ServiceBus
{
    public class InMemoryQueue : ICloudQueue
    {
        private readonly ServiceBusQueueSettings _settings;

        public InMemoryQueue(ServiceBusQueueSettings settings = null)
        {
            _settings = settings;

            if (_settings == null) _settings = new ServiceBusQueueSettings();
        }

        public Queue<object> _inMemoryCollection = new Queue<object>();

        public InMemoryQueue()
        {
            
        }

        public Task<IEnumerable<T>> ReceieveAsync<T>(int batchCount)
        {
            throw new NotImplementedException();
        }

        public Task AddToAsync<T>(IEnumerable<T> items)
        {
            throw new NotImplementedException();
        }
    }
}
