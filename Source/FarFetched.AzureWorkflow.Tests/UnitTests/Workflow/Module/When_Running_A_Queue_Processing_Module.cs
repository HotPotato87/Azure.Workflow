using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Ninject;
using NUnit.Framework;
using ServerShot.Framework.Core;
using ServerShot.Framework.Core.Enums;
using ServerShot.Framework.Core.Implementation;
using ServerShot.Framework.Core.Interfaces;
using ServerShot.Framework.Core.Plugins.Alerts;
using ServerShot.Framework.Core.ServiceBus;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ServerShot.Framework.Tests.UnitTests
{
    [TestFixture]
    public class When_Running_A_Queue_Processing_Module
    {

        ICloudQueue DefaultEmptyQueue
        {
            get
            {
                var service = new Mock<ICloudQueue>();
                service.Setup(x => x.ReceieveAsync<object>(It.IsAny<int>())).Returns(new Task<IEnumerable<object>>(() => new List<object>()));
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
            var module = new Fakes.StubProcessingModule(UnitTestingSettings());
            service.Setup(x => x.ReceieveAsync<object>(It.IsAny<int>())).Returns(async() => new List<object>());
            module.Queue = service.Object;

            //act
            Task.Run(async () => await module.StartAsync());
            await Task.Delay(TimeSpan.FromMilliseconds(300));

            //assert
            service.Verify(x => x.ReceieveAsync<object>(It.IsAny<int>()), Times.AtLeastOnce);
        }

        private static ServerShotModuleSettings UnitTestingSettings()
        {
            return new ServerShotModuleSettings() { QueuePollTime = TimeSpan.FromMilliseconds(0)};
        }

        [Test]
        public async Task Queue_Returning_Zero_Delays_Module()
        {
            //arrange
            var waitTime = 50;
            var module = new Fakes.StubProcessingModule(UnitTestingSettings());
            module.Queue = DefaultEmptyQueue;

            //act
            Task.Run(async() => await module.StartAsync());

            //wait
            await Task.Delay(waitTime * 2);

            //assert
            Assert.IsTrue(module.State == ModuleState.Waiting || module.State == ModuleState.Processing);
        }

        [Test]
        public async Task When_Module_Stop_Is_Called_Module_Is_Now_Finished()
        {
            //arrange
            var module = new Fakes.StubProcessingModule(UnitTestingSettings());
            module.Queue = NonEmptyQueue;

            //act
            Task.Run(() => module.StartAsync());

            module.Stop();

            await Task.Delay(TimeSpan.FromMilliseconds(250));

            Assert.IsTrue(module.State == ModuleState.Finished);
        }

        [Test]
        public async Task Can_Raise_Process_Status_Via_Enum()
        {
            var success = ProcessingResult.Success;
            var module = new Fakes.RaisesProcessingStateViaEnum(success, UnitTestingSettings());
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
            var module = new Fakes.RaisesProcessingStateViaString(success, UnitTestingSettings());
            module.Queue = NonEmptyQueue;
            object _eventResult = null;
            module.OnRaiseProcessed += (resultObject, detail, countsAsProcessed) =>
            {
                _eventResult = resultObject;
            };

            //act
            await module.StartAsync();

            //assert
            Assert.IsTrue(_eventResult == success);
        }

        [Test]
        public async Task Can_Capture_Expected_Exceptions()
        {
            //arrange
            Exception raisedException = null;
            var exception = new ActivationException();
            var module = new Fakes.CapturedErrorsModule(exception, 1, UnitTestingSettings());
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
            var settings = UnitTestingSettings();
            settings.ThrowFailureAfterCapturedErrors = 5;
            var module = new Fakes.CapturedErrorsModule(exception, 5, settings);
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
            Assert.IsTrue(failureErrors.Count >= 5);
            CollectionAssert.AllItemsAreInstancesOfType(failureErrors.Take(5), typeof(ActivationException));
        }

        [Test]
        public async Task Less_Than_errorthresholddoesntresultinfailure()
        {
            //arrange
            bool failurehit = false;
            Exception raisedException = null;
            var exception = new ActivationException();
            var settings = UnitTestingSettings();
            settings.ThrowFailureAfterCapturedErrors = 5;
            var module = new Fakes.CapturedErrorsModule(exception, 4, settings);
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
            var settings = UnitTestingSettings();
            settings.ThrowFailureAfterCapturedErrors = 5;
            var module = new Fakes.CapturedErrorsModule(exception, 5, settings);
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
            var settings = UnitTestingSettings();
            settings.ThrowFailureAfterCapturedErrors = 5;
            settings.SendAlertOnCapturedError = false;
            var module = new Fakes.CapturedErrorsModule(exception, 5, settings);
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

        [Test]
        public async Task Module_Can_Be_Stopped_By_Calling_Stop_Function()
        {
            //arrange
            var settings = UnitTestingSettings();
            var module = new Mock<QueueProcessingServerShotModule<object>>(settings) { CallBase = true};
            module.Setup(x => x.ProcessAsync(It.IsAny<IEnumerable<object>>())).Returns(() => Task.Delay(1));
            var obj = module.Object;
            var stubQueue = new Mock<ICloudQueue>();
            var items = new List<object>(new[] { new object(), new object(), new object()});

            var getItems = new Func<List<object>>(() =>
            {
                var result = items.ToList();
                items.Clear();
                return result;
            });

            stubQueue.Setup(x => x.ReceieveAsync<object>(It.IsAny<int>())).Returns(async () => getItems());
            obj.Queue = stubQueue.Object;

            //act
            obj.OnStart();
            obj.Stop();

            await Task.Delay(TimeSpan.FromMilliseconds(50));

            //assert
            Assert.IsTrue(module.Object.State == ModuleState.Finished);
        }

        [Test]
        public async Task Number_Of_Processed_Items_Is_Captured_By_Module()
        {
            //arrange
            var module = new Fakes.RaisesProcessingStateViaString("str");
            var fakeData = new List<object>(new[] { new object(), new object()  });

            //act
            await module.ProcessAsync(fakeData);

            //assert
            Assert.IsTrue(module.ProcessedCount == fakeData.Count);
        }

    }


    internal class Fakes
    {
        public class CapturedErrorsModule : QueueProcessingServerShotModule<object>
        {
            private readonly Exception _exceptionToThrow;
            private readonly int _exceptionsToThrow;

            public CapturedErrorsModule(Exception exceptionToThrow, int exceptionsToThrow = 1, ServerShotModuleSettings settings = null)
                : base(settings)
            {
                _recievedLimit = 1;
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
        public class StubProcessingModule : QueueProcessingServerShotModule<object>
        {
            public StubProcessingModule(ServerShotModuleSettings settings = null)
                : base(settings)
            {
                _recievedLimit = 1;
            }
            public override async Task ProcessAsync(IEnumerable<object> queueCollection)
            {

            }
        }

        public class RaisesProcessingStateViaString : QueueProcessingServerShotModule<object>
        {
            private readonly string _str;

            public RaisesProcessingStateViaString(string str, ServerShotModuleSettings settings = null)
                : base(settings)
            {
                _str = str;
                _recievedLimit = 1;
            }

            public override async Task ProcessAsync(IEnumerable<object> queueCollection)
            {
                foreach (var o in queueCollection)
                {
                    this.CategorizeResult(_str);
                }
            }
        }

        public class RaisesProcessingStateViaEnum : QueueProcessingServerShotModule<object>
        {
            private readonly ProcessingResult _result;

            public RaisesProcessingStateViaEnum(ProcessingResult result, ServerShotModuleSettings settings = null)
                : base(settings)
            {
                _result = result;
                _recievedLimit = 1;
            }

            public override async Task ProcessAsync(IEnumerable<object> queueCollection)
            {
                this.CategorizeResult(_result);
            }
        }

        public class StubAddsProcessedInfoProcessingModule : QueueProcessingServerShotModule<object>
        {
            public StubAddsProcessedInfoProcessingModule(ServerShotModuleSettings settings = null)
                : base(settings)
            {
                _recievedLimit = 1;
            }
            public override async Task ProcessAsync(IEnumerable<object> queueCollection)
            {
                this.CategorizeResult(ProcessingResult.Success);
            }
        } 
    }
    
}
