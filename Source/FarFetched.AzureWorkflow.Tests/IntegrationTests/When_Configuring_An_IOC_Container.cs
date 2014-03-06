using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Builder;
using ServerShot.Framework.Core.Entities.Environment;
using ServerShot.Framework.Core.Extentions;
using ServerShot.Framework.Core.Implementation;
using ServerShot.Framework.Core.Implementation.IOC;
using ServerShot.Framework.Core.Interfaces;
using ServerShot.Framework.Core.Plugins.Alerts;
using ServerShot.Framework.Core.Queue;
using Servershot.Framework.Entities;
using Servershot.Framework.Extentions;
using ServerShot.Framework.Tests.UnitTests;

namespace ServerShot.Framework.Tests.IntegrationTests
{
    [TestFixture]
    public class When_Configuring_An_IOC_Container
    {
        [Test]
        public async Task Workflow_Modules_Can_Be_Resolved_Via_The_IOC_Container()
        {
            //arrange
            var mockIOCContainer = new Mock<IIocContainer>();
            mockIOCContainer.Setup(x => x.Get<IServerShotModule>(typeof (AlertStub))).Returns(new AlertStub(new Alert()));

            ServerShotEnvironment environment = ServerShotEnvironment.BuildEnvironment()
                .WithIOCContainer(mockIOCContainer.Object)
                .Build();

            ServerShotSessionBase session = environment.CreateLinearSession();

            //act
            await ServerShotLinearSession.StartBuildWithSession(session)
                .AddModule<AlertStub>()
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .RunAsync();

            //assert
            mockIOCContainer.Verify(x => x.Get<IServerShotModule>(typeof (AlertStub)), Times.Once);
        }

        [Test]
        public async Task Workflow_Modules_Services_Can_Be_Injected_Via_The_IOC_Container()
        {
            //arrange
            ServerShotEnvironment environment = ServerShotEnvironment.BuildEnvironment()
                .WithIOCContainer(new NinjectIocContainer())
                .RegisterType<When_Running_A_ServerShot_Session.ServerShotSessionFakes.IToResolve, When_Running_A_ServerShot_Session.ServerShotSessionFakes.ConcreteResolve>().
                Build();

            ServerShotSessionBase session = environment.CreateLinearSession();

            //act
            ServerShotSessionBase builtSession = ServerShotLinearSession.StartBuildWithSession(session)
                .AddModule<When_Running_A_ServerShot_Session.ServerShotSessionFakes.IOCInjectedFake>()
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .ServerShotSession;

            await builtSession.Start();

            //assert
            var runningModule = builtSession.RunningModules.First() as When_Running_A_ServerShot_Session.ServerShotSessionFakes.IOCInjectedFake;
            Assert.IsTrue(runningModule.ToResolve is When_Running_A_ServerShot_Session.ServerShotSessionFakes.ConcreteResolve);
        }

        [Test]
        public async Task Workflow_Modules_Types_Can_Be_Instantiated_By_Di_With_Parameters()
        {
            ServerShotEnvironment environment = ServerShotEnvironment.BuildEnvironment()
                .WithIOCContainer(new NinjectIocContainer())
                .RegisterType<When_Running_A_ServerShot_Session.ServerShotSessionFakes.IToResolve, When_Running_A_ServerShot_Session.ServerShotSessionFakes.ConcreteResolveWithParameters>("argName").
                Build();

            ServerShotSessionBase session = environment.CreateLinearSession();

            //act
            ServerShotSessionBase builtSession = ServerShotLinearSession.StartBuildWithSession(session)
                .AddModule<When_Running_A_ServerShot_Session.ServerShotSessionFakes.IOCInjectedFake>()
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .ServerShotSession;

            await builtSession.Start();

            //assert
            var runningModule = builtSession.RunningModules.First() as When_Running_A_ServerShot_Session.ServerShotSessionFakes.IOCInjectedFake;
            var resolvedInstance = runningModule.ToResolve as When_Running_A_ServerShot_Session.ServerShotSessionFakes.ConcreteResolveWithParameters;
            Assert.IsTrue(resolvedInstance.Argument == "argName");
        }

        [Test]
        public async Task Workflow_Modules_Can_Be_Generically_Created_With_Specific_Constructor_Arguements()
        {
            ServerShotEnvironment environment = ServerShotEnvironment.BuildEnvironment()
                .WithIOCContainer(new NinjectIocContainer())
                .Build();

            ServerShotSessionBase session = environment.CreateLinearSession();
            When_Running_A_ServerShot_Session.ServerShotSessionFakes.IToResolve objectToInject = new When_Running_A_ServerShot_Session.ServerShotSessionFakes.ConcreteResolve();

            //act
            ServerShotSessionBase builtSession = ServerShotLinearSession.StartBuildWithSession(session)
                .AddModule<When_Running_A_ServerShot_Session.ServerShotSessionFakes.IOCInjectedFake>(objectToInject)
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .ServerShotSession;

            await builtSession.Start();

            //assert
            var runningModule = builtSession.RunningModules.First() as When_Running_A_ServerShot_Session.ServerShotSessionFakes.IOCInjectedFake;
            Assert.IsTrue(runningModule.ToResolve is When_Running_A_ServerShot_Session.ServerShotSessionFakes.ConcreteResolve);
            var resolvedInstance = runningModule.ToResolve as When_Running_A_ServerShot_Session.ServerShotSessionFakes.ConcreteResolve;
            Assert.IsTrue(resolvedInstance == objectToInject);
        }

        [Test]
        public async Task Singleton_Services_Are_Injected_As_Same_Instance()
        {
            ServerShotEnvironment environment = ServerShotEnvironment.BuildEnvironment()
                .WithIOCContainer(new NinjectIocContainer())
                .RegisterTypeAsSingleton<When_Running_A_ServerShot_Session.ServerShotSessionFakes.IToResolve, When_Running_A_ServerShot_Session.ServerShotSessionFakes.ConcreteResolve>()
                .Build();

            ServerShotSessionBase session = environment.CreateLinearSession();
            //act
            ServerShotSessionBase builtSession = ServerShotLinearSession.StartBuildWithSession(session)
                .AddModule<When_Running_A_ServerShot_Session.ServerShotSessionFakes.IOCInjectedFake>()
                .WithInstances(2)
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .ServerShotSession;

            await builtSession.Start();

            //assert
            var runningModule1 = builtSession.RunningModules.First() as When_Running_A_ServerShot_Session.ServerShotSessionFakes.IOCInjectedFake;
            var runningModule2 = builtSession.RunningModules[1] as When_Running_A_ServerShot_Session.ServerShotSessionFakes.IOCInjectedFake;

            Assert.AreSame(runningModule1.ToResolve, runningModule2.ToResolve);
        }
    }
}
