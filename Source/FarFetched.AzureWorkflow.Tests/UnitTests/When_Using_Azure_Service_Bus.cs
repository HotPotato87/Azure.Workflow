﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core;
using FarFetched.AzureWorkflow.Core.Implementation;
using FarFetched.AzureWorkflow.Core.ServiceBus;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using NUnit.Framework;

namespace FarFetched.AzureWorkflow.Tests.UnitTests
{
    [TestFixture]
    public class When_Using_Azure_Service_Bus
    {
        private string _testQueueName = "testqueue";
        private NamespaceManager _namespaceManager;

        [SetUp]
        public void CreateQueue()
        {
            _namespaceManager = NamespaceManager.CreateFromConnectionString(DemoSettings.Default.ServiceBusConnectionString);

            if (!_namespaceManager.QueueExists(_testQueueName))
            {
                _namespaceManager.CreateQueue(_testQueueName);
            }
        }

        [TearDown]
        public void DestroyQueue()
        {
            if (_namespaceManager.QueueExists(_testQueueName))
            {
                _namespaceManager.DeleteQueue(_testQueueName);
            }
        }

        [Test]
        [ExpectedException(typeof(AzureWorkflowConfigurationException))]
        public void Initializing_Queue_Without_Connection_Throws_Exception()
        {
            //act/arrange
            var queue = new AzureServiceBusQueue(_testQueueName, new ServiceBusQueueSettings());
        }

        [Test]
        public async Task Adding_And_Retriving_From_Azure_Queue_Is_Successful()
        {
            var testData = new List<string>(new[] {"One", "Two", "Three"});

            //act/arrange
            var queue = new AzureServiceBusQueue(_testQueueName, new ServiceBusQueueSettings() { ConnectionString = DemoSettings.Default.ServiceBusConnectionString });
            await queue.AddToAsync(testData);

            CollectionAssert.AreEqual(await queue.ReceieveAsync<string>(3), testData);
        }

        [Test]
        public async Task Batched_Retrievals_From_Azure_Are_Successful()
        {
            //arrange
            var testData = new List<string>(new[] { "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten" });
            var queue = new AzureServiceBusQueue(_testQueueName, new ServiceBusQueueSettings() { ConnectionString = DemoSettings.Default.ServiceBusConnectionString });

            //act
            await queue.AddToAsync(testData);

            //assert
            CollectionAssert.AreEqual(await queue.ReceieveAsync<string>(5), testData.Take(5));
            CollectionAssert.AreEqual(await queue.ReceieveAsync<string>(5), testData.Skip(5).Take(5));
        }
    }
}