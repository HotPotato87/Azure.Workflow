using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core;
using FarFetched.AzureWorkflow.Core.Architecture;
using FarFetched.AzureWorkflow.Core.Builder;
using FarFetched.AzureWorkflow.Core.Implementation;
using FarFetched.AzureWorkflow.Core.Interfaces;
using FarFetched.AzureWorkflow.Core.ServiceBus;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace FarFetched.AzureWorkflow.Tests.IntegrationTests
{
    [TestClass]
    public class When_Running_A_Workflow_Session
    {
        [TestMethod]
        public async Task Modules_Can_Send_Messages_Between_Queues()
        {
            //arrange
            var payload = new List<object>() {new object(), new object()};
            var inital = new Fakes.AddsToQueueProcessingFake(payload, typeof(Fakes.RecievesFromQueueProcessingFake));
            var reciever = new Fakes.RecievesFromQueueProcessingFake();
            
            //act
            await WorkflowSession.StartBuild()
                .AddModule(inital)
                .AddModule(reciever)
                .WithQueueMechanism(new InMemoryQueueFactory())
                .RunAsync();

            //assert
            CollectionAssert.AreEqual(reciever.Recieved.ToList(), payload);
        }
    }

}
