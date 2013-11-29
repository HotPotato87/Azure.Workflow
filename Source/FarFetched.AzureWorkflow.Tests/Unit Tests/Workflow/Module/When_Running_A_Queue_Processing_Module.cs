using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core;
using FarFetched.AzureWorkflow.Core.Enums;
using FarFetched.AzureWorkflow.Core.Implementation;
using FarFetched.AzureWorkflow.Core.Interfaces;
using Moq;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace FarFetched.AzureWorkflow.Tests.UnitTests
{
    [TestFixture]
    public class When_Running_A_Queue_Processing_Module
    {

        ICloudQueue DefaultEmptyQueue
        {
            get
            {
                var service = new Mock<ICloudQueue>();
                service.Setup(x => x.ReceieveAsync<object>(It.IsAny<int>())).Returns(async () => new List<object>());
                return service.Object;
            }
        }


        [Test]
        public async Task The_Service_Bus_Is_Called_When_Processing()
        {
            //arr
            var service = new Mock<ICloudQueue>();
            var module = new StubProcessingModule();
            service.Setup(x => x.ReceieveAsync<object>(It.IsAny<int>())).Returns(async () => new List<object>());
            module.Queue = service.Object;

            //act
            await module.StartAsync();

            //assert
            service.Verify(x => x.ReceieveAsync<object>(It.IsAny<int>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task Queue_Returning_Zero_Delays_Module()
        {
            //arrange
            var waitTime = 50;
            var module = new StubProcessingModule(new WorkflowModuleSettings() { QueueWaitTime = TimeSpan.FromMilliseconds(50), MaximumWaitTimesBeforeQueueFinished = 3 });
            module.Queue = DefaultEmptyQueue;

            //act
            module.StartAsync();

            //wait
            await Task.Delay(waitTime * 2);

            //assert
            Assert.IsTrue(module.State == ModuleState.Waiting);
        }

        [Test]
        public async Task When_The_Queue_Retrurns_Empty_Over_Threshold_Signifies_Queue_Finished()
        {
            //arrange
            var module = new StubProcessingModule(new WorkflowModuleSettings() { QueueWaitTime = TimeSpan.FromMilliseconds(50), MaximumWaitTimesBeforeQueueFinished = 3 });
            module.Queue = DefaultEmptyQueue;

            //act
            await module.StartAsync();

            //assert
            await Task.Delay(1000);

            Assert.IsTrue(module.State == ModuleState.Finished);
        }

        [Test]
        public async Task Can_Raise_Process_Status_Via_Enum()
        {
            var module = new StubProcessingModule(new WorkflowModuleSettings() { QueueWaitTime = TimeSpan.FromMilliseconds(50), MaximumWaitTimesBeforeQueueFinished = 3 });
            module.Queue = DefaultEmptyQueue;

            //act
            await module.StartAsync();

            //assert
            await Task.Delay(1000);

            Assert.IsTrue(module.State == ModuleState.Finished);
        }

        [Test]
        public async Task Can_Raise_Process_Status_Via_Custom_Status()
        {
            throw new NotImplementedException();
        }
    }

    public class StubProcessingModule : QueueProcessingWorkflowModule<object>
    {
        public StubProcessingModule(WorkflowModuleSettings settings = null) :  base(settings)
        {
            
        }
        public override async Task ProcessAsync(IEnumerable<object> queueCollection)
        {
            
        }
    }

    public class StubAddsProcessedInfoProcessingModule : QueueProcessingWorkflowModule<object>
    {
        public StubAddsProcessedInfoProcessingModule(WorkflowModuleSettings settings = null)
            : base(settings)
        {

        }
        public override async Task ProcessAsync(IEnumerable<object> queueCollection)
        {
            foreach (var o in queueCollection)
            {
                this.RaiseProcessed(ProcessingResult.Success);
            }
        }
    }
}
