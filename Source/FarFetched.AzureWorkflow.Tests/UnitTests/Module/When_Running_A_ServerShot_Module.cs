using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core;
using ServerShot.Framework.Core.Enums;
using ServerShot.Framework.Core.Interfaces;
using ServerShot.Framework.Core.Plugins.Alerts;
using ServerShot.Framework.Core.Implementation;
using ServerShot.Framework.Core.Queue;
using Microsoft.ServiceBus.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ServerShot.Framework.Tests.UnitTests
{
    [TestFixture]
    partial class When_Running_A_ServerShot_Module
    {
        #region Helpers

        ICloudQueue DefaultEmptyQueue
        {
            get
            {
                var service = new Mock<ICloudQueue>();
                service.Setup(x => x.ReceieveAsync<object>(It.IsAny<int>())).Returns(async () => new List<object>());
                return service.Object;
            }
        }

        #endregion

        [Test]
        public async Task Modules_Call_Started_Event_On_Start()
        {
            //arrange
            var mock = new Mock<QueueProcessingServerShotModule<object>>(MockBehavior.Loose) { CallBase = true };
            mock.Setup(x => x.OnStart()).Returns(async () => { });
            var module = mock.Object;
            module.Queue = DefaultEmptyQueue;
            bool calledStart = false;
            module.OnStarted += () =>
            {
                calledStart = true;
            };

            //act
            await module.StartAsync();

            //assert
            Debug.Assert(calledStart);
        }


        [Test]
        public async Task Module_Calls_Event_Finished_When_Done()
        {
            //arrange
            var mock = new Mock<QueueProcessingServerShotModule<object>>(MockBehavior.Loose) { CallBase = true };
            mock.Setup(x => x.OnStart()).Returns(async () => { });
            var module = mock.Object;
            module.Queue = DefaultEmptyQueue;
            bool calledFinish = false;
            module.OnFinished += () =>
            {
                calledFinish = true;
            };

            //act
            await module.StartAsync();

            //assert
            Assert.IsTrue(calledFinish);
        }

        [Test]
        public async Task Module_Calls_Event_Error_On_Error()
        {
            //arrange
            var mock = new Mock<QueueProcessingServerShotModule<object>>(MockBehavior.Loose) { CallBase = true};
            var module = mock.Object;
            mock.Setup(x => x.OnStart()).Throws<Exception>();
            module.Queue = DefaultEmptyQueue;
            bool calledStart = false;
            module.OnError += error =>
            {
                calledStart = true;
            };

            //act
            await module.StartAsync();

            //assert
            Assert.IsTrue(calledStart);
        }
        [Test]
        public async Task Module_State_Waiting_On_Construction()
        {
            //arrange
            var mock = new Mock<QueueProcessingServerShotModule<object>>(MockBehavior.Loose) { CallBase = true };
            var module = mock.Object;
            module.Queue = DefaultEmptyQueue;

            //assert
            Assert.IsTrue(module.State == ModuleState.Waiting);
        }

        [Test]
        public async Task Module_State_Error_On_Error()
        {
            //arrange
            var mock = new Mock<QueueProcessingServerShotModule<object>>(MockBehavior.Loose) { CallBase = true };
            mock.Setup(x => x.OnStart()).Throws<Exception>();
            var module = mock.Object;
            module.Queue = DefaultEmptyQueue;
            bool calledStart = false;
            module.OnError += error =>
            {
                calledStart = true;
            };

            //act
            await module.StartAsync();

            //assert
            Assert.IsTrue(module.State == ModuleState.Error);
        }

        [Test]
        public async Task Module_Calls_Alert_Event_When_Alert_Is_Raised()
        {
            //arrange
            var alert = new Alert() {AlertLevel = AlertLevel.Low, Message = "Hello"};
            var alertModue = new AlertStub(alert);
            alertModue.Queue= DefaultEmptyQueue;
            bool calledAlert = false;
            alertModue.OnAlert += sentAlert =>
            {
                calledAlert = true;
            };

            //act
            await alertModue.StartAsync();

            //assert
            Assert.IsTrue(calledAlert);
        }

        [Test]
        public async Task Module_Calls_Log_When_Starting_Processing()
        {
            //arrange
            var mock = new Mock<QueueProcessingServerShotModule<object>>(MockBehavior.Loose) { CallBase = true };
            mock.Setup(x => x.OnStart()).Returns(async() => { });
            var module = mock.Object;
            module.Queue = DefaultEmptyQueue;
            bool calledLog = false;
            module.OnLogMessage += (error, category) =>
            {
                calledLog = true;
            };

            //act
            await module.StartAsync();

            //assert
            Assert.IsTrue(calledLog);
        }


        [Test]
        public async Task Module_Records_Where_Data_Is_Sent_To()
        {
            var fakeSendModule = new FakeSendToModule(typeof (AlertStub), 5);
            var stubSession = new Mock<ServerShotSessionBase>();
            fakeSendModule.Session = stubSession.Object;

            await fakeSendModule.StartAsync();

            Assert.IsTrue(fakeSendModule.SentToAudit.Count == 1);
            Assert.IsTrue(fakeSendModule.SentToAudit[typeof(AlertStub)] == 5);
        }
    }

    public class AlertStub : ServerShotModuleBase<object>
    {
        private readonly Alert _alert;

        public AlertStub(Alert alert)
        {
            _alert = alert;
        }

        public override async Task OnStart()
        {
            base.RaiseAlert(_alert.AlertLevel, _alert.Message);
        }
    }

    public class FakeSendToModule : ServerShotModuleBase<object>
    {
        private readonly Type _sendToType;
        private readonly int _times;

        public FakeSendToModule(Type sendToType, int times)
        {
            _sendToType = sendToType;
            _times = times;
        }

        public async override Task OnStart()
        {
            for (int i = 0; i < _times; i++)
            {
                this.SendTo(_sendToType, new object());    
            }
        }
    }

}
