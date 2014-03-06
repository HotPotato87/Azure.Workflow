using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using ServerShot.Framework.Core.Implementation;
using ServerShot.Framework.Core.Interfaces;

namespace ServerShot.Framework.Core.Queue
{
    public class InMemoryQueue : ICloudQueue
    {
        internal ConcurrentQueue<object> _inMemoryCollection = new ConcurrentQueue<object>();
        private readonly ServiceBusQueueSettings _settings;

        public InMemoryQueue(ServiceBusQueueSettings settings = null)
        {
            _settings = settings ?? new ServiceBusQueueSettings();
        }

        public async Task<IEnumerable<T>> ReceieveAsync<T>(int batchCount)
        {
            var result = new List<T>();
            for (int i = 0; i < batchCount; i++)
            {
                if (_inMemoryCollection.Any())
                {
                    object obj;
                    if (_inMemoryCollection.TryDequeue(out obj))
                    {
                        result.Add((T) obj);
                    }
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

        public int Count
        {
            get { return _inMemoryCollection.Count; }
        }

        public event Action<object> ObjectWrapperCreated;
    }
}
