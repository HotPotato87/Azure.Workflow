using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Workflow.Core;
using Azure.Workflow.Core.Enums;
using Azure.Workflow.Core.Implementation;
using Azure.Workflow.Core.Interfaces;
using Azure.Workflow.Core.Plugins.Alerts;
using Azure.Workflow.Core.ServiceBus;
using Moq;
using Ninject;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Azure.Workflow.Tests.UnitTests
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
        ICloudQueue NonEmptyQueue
        {
            get
            {
                var inMemoryQueue = new InMemoryQueue();
                inMemoryQueue.AddToAsync(new[] {"Apples!"});
                return inMemoryQueue;
            }
        }

        [Test]
        public async Task The_Service_Bus_Is_Called_When_Processing()
        {
            //arr
            var service = new Mock<ICloudQueue>();
            var module = new Fakes.StubProcessingModule();
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
            var module = new Fakes.StubProcessingModule(new WorkflowModuleSettings() { QueueWaitTimeBeforeFinish = TimeSpan.FromMilliseconds(300), MaximumWaitTimesBeforeQueueFinished = 3 });
            module.Queue = DefaultEmptyQueue;

            //act
            Task.Run(async() => await module.StartAsync());

            //wait
            await Task.Delay(waitTime * 2);

            //assert
            Assert.IsTrue(module.State == ModuleState.Waiting || module.State == ModuleState.Processing);
        }

        [Test]
        public async Task When_The_Queue_Retrurns_Empty_Over_Threshold_Signifies_Queue_Finished()
        {
            //arrange
            var module = new Fakes.StubProcessingModule(new WorkflowModuleSettings() { QueueWaitTimeBeforeFinish = TimeSpan.FromMilliseconds(50), MaximumWaitTimesBeforeQueueFinished = 3 });
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
            var success = ProcessingResult.Success;
            var module = new Fakes.RaisesProcessingStateViaEnum(success);
            module.Queue = NonEmptyQueue;
            
            object _eventResult = null;
            module.OnRaiseProcessed += (resultObject, detail, countsAsProcessed) =>
            {
                _eventResult = resultObject;
            };

            //act
            await module.StartAsync();

            //assert
            await Task.Delay(1000);

            Assert.IsTrue(_eventResult == success.ToString());
        }

        [Test]
        public async Task Can_Raise_Process_Status_Via_Custom_Status()
        {
            var success = "ORANGES";
            var module = new Fakes.RaisesProcessingStateViaString(success);
            module.Queue = NonEmptyQueue;
            object _eventResult = null;
            module.OnRaiseProcessed += (resultObject, detail, countsAsProcessed) =>
            {
                _eventResult = resultObject;
            };

            //act
            await module.StartAsync();

            //assert
            await Task.Delay(1000);

            Assert.IsTrue(_eventResult == success);
        }

        [Test]
        public async Task Can_Capture_Expected_Exceptions()
        {
            //arrange
            Exception raisedException = null;
            var exception = new ActivationException();
            var module = new Fakes.CapturedErrorsModule(exception);
            module.Queue = NonEmptyQueue;
            module.OnError += exception1 =>
            {
                raisedException = exception1;
            };

            //act
            await module.StartAsync();

            //assert
            Assert.IsTrue(raisedException == exception);
        }

        [Test]
        public async Task Can_Report_Failure_After_A_Number_Of_Captured_Errors()
        {
            //arrange
            Exception raisedException = null;
            var exception = new ActivationException();
            var module = new Fakes.CapturedErrorsModule(exception, 5, new WorkflowModuleSettings() { ThrowFailureAfterCapturedErrors = 5});
            module.Queue = NonEmptyQueue;
            var failureMessage = "";
            var failureErrors = new List<Exception>();
            module.OnFailure += (message, errors) =>
            {
                failureMessage = message;
                failureErrors = errors.ToList();
            };

            //act
            await module.StartAsync();

            //assert
            Assert.IsTrue(failureMessage == "Error threshold reached");
            Assert.IsTrue(failureErrors.Count == 5);
            CollectionAssert.AllItemsAreInstancesOfType(failureErrors, typeof(ActivationException));
        }

        [Test]
        public async Task Less_Than_errorthresholddoesntresultinfailure()
        {
            //arrange
            bool failurehit = false;
            Exception raisedException = null;
            var exception = new ActivationException();
            var module = new Fakes.CapturedErrorsModule(exception, 4, new WorkflowModuleSettings() { ThrowFailureAfterCapturedErrors = 5 });
            module.Queue = NonEmptyQueue;
            module.OnFailure += (message, errors) =>
            {
                failurehit = true;
            };

            //act
            await module.StartAsync();

            //assert
            Assert.IsFalse(failurehit);
        }

        [Test]
        public async Task Captured_Errors_Results_In_Alert_Bring_Sent()
        {
            //arrange
            Alert moduleAlert = null;
            var exception = new ActivationException();
            var module = new Fakes.CapturedErrorsModule(exception, 5, new WorkflowModuleSettings() { ThrowFailureAfterCapturedErrors = 5 });
            module.Queue = NonEmptyQueue;
            var failureMessage = "";
            var failureErrors = new List<Exception>();
            module.OnAlert += (alert) =>
            {
                moduleAlert = alert;
            };

            //act
            await module.StartAsync();

            //assert
            Assert.IsNotNull(moduleAlert);
        }

        [Test]
        public async Task Captured_Errors_Dont_Send_Alert_If_Settings_Is_Off()
        {
            //arrange
            Alert moduleAlert = null;
            var exception = new ActivationException();
            var module = new Fakes.CapturedErrorsModule(exception, 5, new WorkflowModuleSettings() { ThrowFailureAfterCapturedErrors = 5, SendAlertOnCapturedError = false});
            module.Queue = NonEmptyQueue;
            module.OnAlert += (alert) =>
            {
                moduleAlert = alert;
            };

            //act
            await module.StartAsync();

            //assert
            Assert.IsNull(moduleAlert);
        }
    }


    internal class Fakes
    {
        public class CapturedErrorsModule : QueueProcessingWorkflowModule<object>
        {
            private readonly Exception _exceptionToThrow;
            private readonly int _exceptionsToThrow;

            public CapturedErrorsModule(Exception exceptionToThrow, int exceptionsToThrow = 1, WorkflowModuleSettings settings = null)
                : base(settings)
            {
                _exceptionToThrow = exceptionToThrow;
                _exceptionsToThrow = exceptionsToThrow;
            }

            public override async Task ProcessAsync(IEnumerable<object> queueCollection)
            {
                for (int i = 0; i < _exceptionsToThrow; i++)
                {
                    this.RaiseError(_exceptionToThrow); 
                }
            }
        }
        public class StubProcessingModule : QueueProcessingWorkflowModule<object>
        {
            public StubProcessingModule(WorkflowModuleSettings settings = null)
                : base(settings)
            {

            }
            public override async Task ProcessAsync(IEnumerable<object> queueCollection)
            {

            }
        }

        public class RaisesProcessingStateViaString : QueueProcessingWorkflowModule<object>
        {
            private readonly string _str;

            public RaisesProcessingStateViaString(string str, WorkflowModuleSettings settings = null)
                : base(settings)
            {
                _str = str;
            }

            public override async Task ProcessAsync(IEnumerable<object> queueCollection)
            {
                foreach (var o in queueCollection)
                {
                    this.CategorizeResult(_str);
                }
            }
        }

        public class RaisesProcessingStateViaEnum : QueueProcessingWorkflowModule<object>
        {
            private readonly ProcessingResult _result;

            public RaisesProcessingStateViaEnum(ProcessingResult result, WorkflowModuleSettings settings = null)
                : base(settings)
            {
                _result = result;
            }

            public override async Task ProcessAsync(IEnumerable<object> queueCollection)
            {
                this.CategorizeResult(_result);
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
                this.CategorizeResult(ProcessingResult.Success);
            }
        } 
    }
    
}
