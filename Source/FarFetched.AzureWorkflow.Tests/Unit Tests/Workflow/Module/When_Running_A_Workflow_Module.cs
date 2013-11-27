using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core;
using FarFetched.AzureWorkflow.Core.Enums;
using FarFetched.AzureWorkflow.Core.Implementation;
using FarFetched.AzureWorkflow.Core.Interfaces;
using FarFetched.AzureWorkflow.Core.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace FarFetched.AzureWorkflow.Tests.WhenRunningWorkflowModule
{
    [TestFixture]
    partial class When_Running_A_Workflow_Module
    {
        
        [Test]
        public async Task Modules_Call_Started_Event_On_Start()
        {
            throw new NotImplementedException();
        }


        [Test]
        public async Task The_Service_Bus_Is_Called_When_Processing()
        {
            //arrange
            var service = new Mock<ICloudQueue>();
            var module = new StubModuleBase();
            module.Queue = service.Object;
            service.Setup(x => x.ReceieveAsync<object>(It.IsAny<int>())).Returns(async() => new List<object>());

            //act
            await module.StartAsync();

            //assert
            service.Verify(x => x.ReceieveAsync<object>(It.IsAny<int>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task Queue_Returning_Zero_Delays_Module()
        {
            //arrange
            var service = new Mock<ICloudQueue>();
            var module = new StubModuleBase();
            module.Queue = service.Object;
            service.Setup(x => x.ReceieveAsync<object>(It.IsAny<int>())).Returns(async () => new List<object>());

            //act
            module.StartAsync();

            //assert
            Assert.IsTrue(module.State == ModuleState.Waiting);
        }

        [Test]
        public async Task When_The_Queue_Retrurns_Empty_Over_Threshold_Signifies_Queue_Finished()
        {//possibly finished
            //arrange
            var service = new Mock<ICloudQueue>();
            var module = new StubModuleBase(new WorkflowModuleSettings() { QueueWaitTime = TimeSpan.FromMilliseconds(50), MaximumWaitTimesBeforeQueueFinished = 3 });
            module.Queue = service.Object;
            service.Setup(x => x.ReceieveAsync<object>(It.IsAny<int>())).Returns(async () => new List<object>());

            //act
            await module.StartAsync();

            //assert
            await Task.Delay(1000);

            Assert.IsTrue(module.State == ModuleState.Finished);
        }

        [Test]
        public async Task Module_Calls_Event_Finished_When_Done()
        {
            throw new NotImplementedException();
        }

        [Test]
        public async Task Module_Calls_Event_Error_On_Error()
        {
            throw new NotImplementedException();
        }

        [Test]
        public async Task Module_Calls_Log_When_Starting_Processing()
        {
            throw new NotImplementedException();
        }
    }

    public class StubModuleBase : QueueProcessingWorkflowModule<object>
    {
        public StubModuleBase(WorkflowModuleSettings settings = null) {}
        

        public void SendToModule(Type t, object o)
        {
            base.SendTo(t, o);
        }

        public override Task ProcessAsync(IEnumerable<object> queueCollection)
        {
            throw new NotImplementedException();
        }
    }

    public class ErrorStubModuleBase : QueueProcessingWorkflowModule<object>
    {
        public ErrorStubModuleBase(WorkflowModuleSettings settings = null) { }

        public void SendToModule(Type t, object o)
        {
            base.SendTo(t, o);
        }

        public override Task ProcessAsync(IEnumerable<object> queueCollection)
        {
            throw new NotImplementedException();
        }
    }
}
