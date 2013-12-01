using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core;
using FarFetched.AzureWorkflow.Core.Enums;
using FarFetched.AzureWorkflow.Core.Implementation;
using FarFetched.AzureWorkflow.Core.Interfaces;
using FarFetched.AzureWorkflow.Core.Plugins.Alerts;
using FarFetched.AzureWorkflow.Core.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace FarFetched.AzureWorkflow.Tests.UnitTests
{
    [TestFixture]
    partial class When_Running_A_Workflow_Module
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
            var mock = new Mock<QueueProcessingWorkflowModule<object>>(MockBehavior.Loose);
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
            var mock = new Mock<QueueProcessingWorkflowModule<object>>(MockBehavior.Loose);
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
            var mock = new Mock<QueueProcessingWorkflowModule<object>>(MockBehavior.Loose);
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
            var mock = new Mock<QueueProcessingWorkflowModule<object>>(MockBehavior.Loose);
            var module = mock.Object;
            module.Queue = DefaultEmptyQueue;

            //assert
            Assert.IsTrue(module.State == ModuleState.Waiting);
        }

        [Test]
        public async Task Module_State_Error_On_Error()
        {
            //arrange
            var mock = new Mock<QueueProcessingWorkflowModule<object>>(MockBehavior.Loose);
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
            var mock = new Mock<QueueProcessingWorkflowModule<object>>(MockBehavior.Loose);
            mock.Setup(x => x.OnStart()).Returns(async() => { });
            var module = mock.Object;
            module.Queue = DefaultEmptyQueue;
            bool calledLog = false;
            module.OnLogMessage += error =>
            {
                calledLog = true;
            };

            //act
            await module.StartAsync();

            //assert
            Assert.IsTrue(calledLog);
        }
    }

    public class AlertStub : WorkflowModuleBase<object>
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
}
