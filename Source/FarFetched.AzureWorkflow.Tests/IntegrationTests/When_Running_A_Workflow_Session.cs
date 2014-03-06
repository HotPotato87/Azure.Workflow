using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Implementation;
using ServerShot.Framework.Core.Queue;
using ServerShot.Framework.Core;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Builder;
using ServerShot.Framework.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Servershot.Framework.Entities;

namespace ServerShot.Framework.Tests.IntegrationTests
{
    [TestClass]
    public class When_Running_A_Workflow_Session
    {
        [TestMethod]
        public async Task Modules_Can_Send_Messages_Between_Queues()
        {
            //arrange
            var payload = new List<object>() { new object(), new object() };

            //act
            var session = await ServerShotLinearSession.StartBuild()
                .AddModule<Fakes.AddsToQueueProcessingFake>(payload, typeof(Fakes.RecievesFromQueueProcessingFake))
                .AddModule<Fakes.RecievesFromQueueProcessingFake>()
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .RunAsync();

            var reciever = session.RunningModules[1] as Fakes.RecievesFromQueueProcessingFake;

            //assert
            CollectionAssert.AreEqual(reciever.Recieved.ToList(), payload);
        }

        [TestMethod]
        public async Task Modules_That_Throw_Exceptions_Hitting_The_Failure_Threshold_Results_In_Session_Failure()
        {
            //arrange
            var exception = new AccessViolationException();
            var payload = new List<object>() { new object(), new object() };
            string onfailedMessage = null;

            var session = new ServerShotLinearSession();
            session.OnFailure += (module, failureMessage) =>
            {
                onfailedMessage = failureMessage;
            };
            await ServerShotLinearSession.StartBuildWithSession(session)
                .AddModule<Fakes.AddsToQueueProcessingFake>(payload, typeof(Fakes.ThrowsErrorModule))
                .AddModule<Fakes.ThrowsErrorModule>(exception, 6, new ServerShotModuleSettings() { ThrowFailureAfterCapturedErrors = 5 })
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .RunAsync();


            //assert
            Assert.IsNotNull(onfailedMessage);
        }

        [TestMethod]
        public async Task Modules_That_Throw_Exceptions_But_Not_Within_Failure_Threshold_Allows_Session_To_Complete()
        {
            //arrange
            var exception = new AccessViolationException();
            var payload = new List<object>() { new object(), new object() };
            string onfailedMessage = null;

            var session = new ServerShotLinearSession();
            session.OnFailure += (module, failureMessage) =>
            {
                onfailedMessage = failureMessage;
            };
            await ServerShotLinearSession.StartBuildWithSession(session)
                 .AddModule<Fakes.AddsToQueueProcessingFake>(payload, typeof(Fakes.ThrowsErrorModule))
                .AddModule<Fakes.ThrowsErrorModule>(exception, 4, new ServerShotModuleSettings() { ThrowFailureAfterCapturedErrors = 5 })
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .RunAsync();


            //assert
            Assert.IsNull(onfailedMessage);
        }

        [TestMethod]
        public async Task Never_Fail_Equals_True_And_Modules_That_Throw_Exceptions_Then_No_Failure()
        {
            //arrange
            var exception = new AccessViolationException();
            var payload = new List<object>() { new object(), new object() };
            string onfailedMessage = null;

            var session = new ServerShotLinearSession();
            session.Settings.NeverFail = true;

            session.OnFailure += (module, s) =>
            {
                onfailedMessage = "we failed";
            };

            var finishedSession = await ServerShotLinearSession.StartBuildWithSession(session)
                 .AddModule<Fakes.AddsToQueueProcessingFake>(payload, typeof(Fakes.ThrowsErrorModule))
                .AddModule<Fakes.ThrowsErrorModule>(exception, 10, new ServerShotModuleSettings() { ThrowFailureAfterCapturedErrors = 5 })
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .RunAsync();


            //assert
            Assert.IsNull(onfailedMessage);
        }
    }

}
