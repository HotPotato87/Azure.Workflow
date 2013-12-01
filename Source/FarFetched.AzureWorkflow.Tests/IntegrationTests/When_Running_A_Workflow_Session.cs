using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Workflow.Core.Implementation;
using Azure.Workflow.Core.ServiceBus;
using Azure.Workflow.Core;
using Azure.Workflow.Core.Architecture;
using Azure.Workflow.Core.Builder;
using Azure.Workflow.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Azure.Workflow.Tests.IntegrationTests
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
