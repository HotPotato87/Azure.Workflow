using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core.Implementation;
using FarFetched.AzureWorkflow.Core.Interfaces;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace FarFetched.AzureWorkflow.Core.ServiceBus
{
    public class AzureServiceBusQueue : ICloudQueue
    {
        private readonly string _queueName;
        private readonly ServiceBusQueueSettings _settings;
        private QueueClient _queueClient;
        private NamespaceManager _namespaceManager;

        public AzureServiceBusQueue(string queueName, ServiceBusQueueSettings settings)
        {
            _queueName = queueName;
            _settings = settings;

            SetupQueue();
        }

        private void SetupQueue()
        {
            
            if (string.IsNullOrEmpty(_settings.ConnectionString))
            {
                //TODO : UT
                throw new AzureWorkflowConfigurationException("Connection string must be set on queue", null);
            } 
            
            _namespaceManager = NamespaceManager.CreateFromConnectionString(_settings.ConnectionString);

            try
            {
                if (!_namespaceManager.QueueExists(this._queueName))
                {
                    _namespaceManager.CreateQueue(this._queueName);
                }

                _queueClient = QueueClient.CreateFromConnectionString(_settings.ConnectionString, this._queueName);
            }
            catch (Exception e)
            {
                throw new AzureWorkflowConfigurationException("Problem setting up ServiceBus queue", e);
            }
        }

        public async Task<IEnumerable<T>> ReceieveAsync<T>(int batchCount)
        {
            var messages = await _queueClient.ReceiveBatchAsync(batchCount, TimeSpan.FromSeconds(5));
            var result = messages.Select(x => x.GetBody<T>()).ToList();
            messages.ToList().ForEach(x=> x.Complete());
            return result;
        }

        public async Task AddToAsync<T>(IEnumerable<T> items)
        {
            await _queueClient.SendBatchAsync(items.Select(x => new BrokeredMessage(x)));
        }
    }
}
