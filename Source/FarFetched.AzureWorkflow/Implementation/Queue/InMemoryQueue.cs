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
        internal  Queue<object> _inMemoryCollection = new Queue<object>();
        private readonly ServiceBusQueueSettings _settings;

        public InMemoryQueue(ServiceBusQueueSettings settings = null)
        {
            _settings = settings;

            if (_settings == null) _settings = new ServiceBusQueueSettings();
        }

        public async Task<IEnumerable<T>> ReceieveAsync<T>(int batchCount)
        {
            List<T> result = new List<T>();
            for (int i = 0; i < batchCount; i++)
            {
                if (_inMemoryCollection.Any())
                {
                    result.Add((T)_inMemoryCollection.Dequeue());    
                }
                else
                {
                    return result;
                }
            }
            return result;
        }

        public async Task AddToAsync<T>(IEnumerable<T> items)
        {
            items.ToList().ForEach(x=>_inMemoryCollection.Enqueue(x));
        }
    }
}
