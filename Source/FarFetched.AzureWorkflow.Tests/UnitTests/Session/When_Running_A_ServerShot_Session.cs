using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using ServerShot.Framework.Core;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Builder;
using ServerShot.Framework.Core.Entities;
using ServerShot.Framework.Core.Entities.Environment;
using ServerShot.Framework.Core.Entities.Scheduler.Deployments;
using ServerShot.Framework.Core.Enums;
using ServerShot.Framework.Core.Extentions;
using ServerShot.Framework.Core.Implementation;
using ServerShot.Framework.Core.Implementation.IOC;
using ServerShot.Framework.Core.Implementation.StopStrategy;
using ServerShot.Framework.Core.Interfaces;
using ServerShot.Framework.Core.Plugins;
using ServerShot.Framework.Core.Plugins.Alerts;
using ServerShot.Framework.Core.Queue;
using Servershot.Framework.Entities;
using Servershot.Framework.Extentions;
using ServerShot.Framework.Tests.Helpers;
using Moq;
using NUnit.Framework;

namespace ServerShot.Framework.Tests.UnitTests
{
    [TestFixture]
    public class When_Running_A_ServerShot_Session
    {
        private Mock<ReportGenerationPluginBase> _summaryReportGenerator;
        private Mock<IServerShotModule> _module1;
        private Mock<IServerShotModule> _module2;

        public ServerShotSessionBase GetStandardSession()
        {
            return ServerShotLinearSession.StartBuild()
                .AddModule<Fakes.StubProcessingModule>(10, null)
                .AddModule<Fakes.StubProcessingModule>(10, null)
                .ServerShotSession;
        }

        public ServerShotSessionBase GetStandardSessionWithQueue()
        {
            return ServerShotLinearSession.StartBuild()
                .AddModule<Fakes.StubProcessingModule>(10, null)
                .AddModule<Fakes.StubProcessingModule>(10, null)
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .ServerShotSession;
        }

        
        [Test]
        public async Task Modules_Are_Added_To_Running_Sessions()
        {
            //arrange
            ServerShotSessionBase session = GetStandardSessionWithQueue();

            //act
            await session.Start();

            //assert
            Assert.IsTrue(session.RunningModules.All(x => x.GetType() == typeof(Fakes.StubProcessingModule)));
            Assert.IsTrue(session.RunningModules.Count == 2);
        }

        [Test]
        public async Task Not_Specifying_A_Workflow_Environment_Results_In_Standard_Environment_Being_Built()
        {
            ServerShotSessionBase session = ServerShotLinearSession.StartBuild()
                .AddModule<IServerShotModule>()
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .ServerShotSession;

            Assert.IsTrue(session.Environment != null);
        }

        [Test]
        public async Task Session_Calls_Register_Finished()
        {
            //arrange
            ServerShotSessionBase session = GetStandardSessionWithQueue();
            bool called = false;

            session.OnSessionFinished += ServerShotSession => { called = true; };

            //act
            await session.Start();

            //assert
            Assert.IsTrue(called);
        }

        [Test]
        public async Task Session_Populates_Total_Duration_In_Real_Time()
        {
            //act
            ServerShotSessionBase session = GetStandardSessionWithQueue();

            //arrange
            await session.Start();

            //assert
            Assert.IsTrue(session.TotalDuration.Ticks > 0);
        }

        [Test]
        public async Task Session_Sets_Start_Time_When_Started()
        {
            //act
            ServerShotSessionBase session = GetStandardSessionWithQueue();

            //arrange
            await session.Start();

            //assert
            Assert.IsTrue(session.Started != default(DateTime));
        }

        [Test]
        [ExpectedException(typeof (WorkflowConfigurationException))]
        public async Task Session_Throws_If_No_Queue_Mechanism()
        {
            //arrange
            ServerShotSessionBase session = GetStandardSession();

            //act 
            await session.Start();
        }

        [Test]
        public async Task Session_sets_End_Time_When_Finished()
        {
            //act
            ServerShotSessionBase session = GetStandardSessionWithQueue();

            //arrange
            await session.Start();

            //assert
            Assert.IsTrue(session.Ended != default(DateTime));
        }

        [Test]
        public async Task Specifying_A_Workflow_Environment_Results_In_Specified_One_Being_Used()
        {
            _module1 = new Mock<IServerShotModule>();
            _module2 = new Mock<IServerShotModule>();

            _module1.Setup(x => x.StartAsync()).Returns(async () => { await Task.Delay(10); });
            _module2.Setup(x => x.StartAsync()).Returns(async () => { await Task.Delay(10); });

            var mockEnvironment = new Mock<ServerShotEnvironment>();
            var session =  new ServerShotLinearSession() { Environment = mockEnvironment.Object };

            ServerShotSessionBase builtSession = ServerShotLinearSession.StartBuildWithSession(session)
                .AddModule<Fakes.StubProcessingModule>()
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .ServerShotSession;

            Assert.IsTrue(builtSession.Environment == mockEnvironment.Object);
        }

        [Test]
        public async Task Workflow_Default_Stop_Strategy_Is_Continuous_Processing()
        {
            var session = new ServerShotLinearSession();

            Assert.IsTrue(session.StopStrategy is ContinousProcessingStategy);
        }

        [Test]
        public async Task Multiple_Module_Instances_Share_The_Same_Queue()
        {
            //arrange
            ServerShotEnvironment environment = ServerShotEnvironment.BuildEnvironment()
                .WithIOCContainer(new NinjectIocContainer())
                .RegisterType<ServerShotSessionFakes.IToResolve, ServerShotSessionFakes.ConcreteResolve>().
                Build();

            ServerShotSessionBase session = environment.CreateLinearSession();

            //act
            ServerShotSessionBase builtSession = ServerShotLinearSession.StartBuildWithSession(session)
                .AddModule<ServerShotSessionFakes.IOCInjectedFake>()
                    .WithInstances(2)
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .ServerShotSession;

            await builtSession.Start();

            var queue = new List<ICloudQueue>();

            foreach (var module in builtSession.RunningModules)
            {
                queue.Add(module.Queue);
            }

            //Assert

            Assert.AreSame(queue[0], queue[1]);
        }

        [Test]
        public async Task Workflow_Session_Stop_Strategy_Determines_Workflow_Session_Stopping()
        {
            var session = new ServerShotLinearSession();
            session.CloudQueueFactory = TestHelpers.CreateNonEmptyStubQueueFactory().Object;

            var mockStopStrategy = new Mock<IProcessingStopStrategy>();
            mockStopStrategy.Setup(x => x.ShouldStop(It.IsAny<ServerShotSessionBase>())).Returns(true);

            await ServerShotLinearSession.StartBuildWithSession(session)
                .AddModule<Fakes.RaisesProcessingStateViaEnum>(ProcessingResult.Success, null)
                .WithSessionStopStrategy(mockStopStrategy.Object)
                .AttachSessionQueueMechanism(session.CloudQueueFactory)
                .ConfigureSessionSettings(new SessionSettings() { CheckStopStrategyEvery = TimeSpan.FromMilliseconds(1)})
                .RunAsync();

            mockStopStrategy.Verify(x => x.ShouldStop(It.IsAny<ServerShotSessionBase>()));
        }

        [Test]
        public async Task Workflow_Sessions_Default_Settings_Get_Injected_Into_Modules()
        {
            //arrange
            var workflowModuleSettings = new ServerShotModuleSettings();

            var session = await ServerShotLinearSession.StartBuild()
                .AddModule<Fakes.RaisesProcessingStateViaEnum>(ProcessingResult.Success, null)
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .ConfigureDefaultModuleSettings(workflowModuleSettings)
                .RunAsync();

            //act
            await session.Start();

            var module = session.RunningModules.First();

            //assert
            Assert.AreSame(workflowModuleSettings, module.Settings);

        }

        [Test]
        public async Task Building_Session_Without_Deployment_Strategy_Results_In_Local_Deployment_Default()
        {
            var session = await ServerShotLinearSession.StartBuild()
                .AddModule<Fakes.InitialLogsMessageModule>()
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .RunAsync();

            Assert.IsTrue(session.DeploymentStrategy.GetType() == typeof (LocalDeploymentStrategy));
        }

        public class ServerShotSessionFakes
        {
            public class ConcreteResolve : IToResolve
            {
            }

            public class ConcreteResolveWithParameters : IToResolve
            {
                public string Argument { get; set; }

                public ConcreteResolveWithParameters(string argument)
                {
                    Argument = argument;
                }
            }

            public class IOCInjectedFake : InitialServerShotModule<object>
            {
                public IOCInjectedFake(IToResolve toResolve)
                {
                    ToResolve = toResolve;
                }

                public IToResolve ToResolve { get; private set; }

                public override async Task OnStart()
                {
                }
            }

            public interface IToResolve
            {
            }
        }

    }
}