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

        public AzureServiceBusQueue(string queueName, ServiceBusQueueSettings settings)
        {
            _queueName = queueName;
            _settings = settings;

            SetupQueue();
        }

        private void SetupQueue()
        {
            // Create a token provider with the relevant credentials.
            TokenProvider credentials =
                TokenProvider.CreateSharedSecretTokenProvider
                (_settings.AccountName, _settings.Key);

            // Create a URI for the serivce bus.
            Uri serviceBusUri = ServiceBusEnvironment.CreateServiceUri
                ("sb", _settings.AccountNamespace, string.Empty);

            // Create a message factory for the service bus URI using the
            // credentials
            MessagingFactory factory = MessagingFactory.Create
                (serviceBusUri, credentials);

            // Create a queue client for the pizzaorders queue
            _queueClient = factory.CreateQueueClient(_queueName);
        }

        public async Task<IEnumerable<T>> ReceieveAsync<T>(int batchCount)
        {
            var messages = await _queueClient.ReceiveBatchAsync(batchCount);
            return messages.Select(x => x.GetBody<T>());
        }

        public async Task AddToAsync<T>(IEnumerable<T> items)
        {
            await _queueClient.SendBatchAsync(items.Select(x => new BrokeredMessage(x)));
        }
    }
}
