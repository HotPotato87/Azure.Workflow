using System;
using System.Linq;
using System.Threading.Tasks;
using ServerShot.Framework.Core;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Builder;
using ServerShot.Framework.Core.Entities.Environment;
using ServerShot.Framework.Core.Enums;
using ServerShot.Framework.Core.Extentions;
using ServerShot.Framework.Core.Implementation;
using ServerShot.Framework.Core.Implementation.IOC;
using ServerShot.Framework.Core.Implementation.StopStrategy;
using ServerShot.Framework.Core.Interfaces;
using ServerShot.Framework.Core.Plugins;
using ServerShot.Framework.Core.Plugins.Alerts;
using ServerShot.Framework.Core.ServiceBus;
using ServerShot.Framework.Tests.Helpers;
using Moq;
using NUnit.Framework;

namespace ServerShot.Framework.Tests.UnitTests
{
    [TestFixture]
    public class When_Running_A_Workflow_Session
    {
        private Mock<ReportGenerationPlugin> _summaryReportGenerator;
        private Mock<IServerShotModule> _module1;
        private Mock<IServerShotModule> _module2;

        public ServerShotSession GetStandardSession()
        {
            _module1 = new Mock<IServerShotModule>();
            _module2 = new Mock<IServerShotModule>();

            _module1.Setup(x => x.StartAsync()).Returns(async () => { await Task.Delay(10); });
            _module2.Setup(x => x.StartAsync()).Returns(async () => { await Task.Delay(10); });

            return ServerShotSession.StartBuild()
                .AddModule(_module1.Object)
                .AddModule(_module2.Object).ServerShotSession;
        }

        public ServerShotSession GetStandardSessionWithQueue()
        {
            _module1 = new Mock<IServerShotModule>();
            _module2 = new Mock<IServerShotModule>();

            _module1.Setup(x => x.StartAsync()).Returns(async () => { await Task.Delay(10); });
            _module2.Setup(x => x.StartAsync()).Returns(async () => { await Task.Delay(10); });

            return ServerShotSession.StartBuild()
                .AddModule(_module1.Object)
                .AddModule(_module2.Object)
                .WithQueueMechanism(new InMemoryQueueFactory())
                .ServerShotSession;
        }

        public class ServerShotSessionFakes
        {
            public class ConcreteResolve : IToResolve
            {
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

        [Test]
        public async Task Modules_Are_Added_To_Running_Sessions()
        {
            //arrange
            ServerShotSession session = GetStandardSessionWithQueue();

            //act
            await session.Start();

            //assert
            Assert.IsTrue(session.RunningModules.Any(x => x == _module1.Object));
            Assert.IsTrue(session.RunningModules.Any(x => x == _module2.Object));
        }

        [Test]
        public async Task Not_Specifying_A_Workflow_Environment_Results_In_Standard_Environment_Being_Built()
        {
            _module1 = new Mock<IServerShotModule>();
            _module2 = new Mock<IServerShotModule>();

            _module1.Setup(x => x.StartAsync()).Returns(async () => { await Task.Delay(10); });
            _module2.Setup(x => x.StartAsync()).Returns(async () => { await Task.Delay(10); });

            ServerShotSession session = ServerShotSession.StartBuild()
                .AddModule(_module1.Object)
                .AddModule(_module2.Object)
                .WithQueueMechanism(new InMemoryQueueFactory())
                .ServerShotSession;

            Assert.IsTrue(session.Environment != null);
        }

        [Test]
        public async Task Session_Calls_Register_Finished()
        {
            //arrange
            ServerShotSession session = GetStandardSessionWithQueue();
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
            ServerShotSession session = GetStandardSessionWithQueue();

            //arrange
            await session.Start();

            //assert
            Assert.IsTrue(session.TotalDuration.Ticks > 0);
        }

        [Test]
        public async Task Session_Sets_Start_Time_When_Started()
        {
            //act
            ServerShotSession session = GetStandardSessionWithQueue();

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
            ServerShotSession session = GetStandardSession();

            //act 
            await session.Start();
        }

        [Test]
        public async Task Session_sets_End_Time_When_Finished()
        {
            //act
            ServerShotSession session = GetStandardSessionWithQueue();

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

            var mockEnvironment = new Mock<WorkflowEnvironment>();
            var session = new ServerShotSession(mockEnvironment.Object);

            ServerShotSession builtSession = ServerShotSession.StartBuildWithSession(session)
                .AddModule(_module1.Object)
                .AddModule(_module2.Object)
                .WithQueueMechanism(new InMemoryQueueFactory())
                .ServerShotSession;

            Assert.IsTrue(builtSession.Environment == mockEnvironment.Object);
        }

        [Test]
        public async Task ServerShotSessionStartCallsStartOnModules()
        {
            //arrange
            ServerShotSession session = GetStandardSessionWithQueue();

            //act
            await session.Start();

            //assert
            _module1.Verify(x => x.StartAsync(), Times.Once);
            _module2.Verify(x => x.StartAsync(), Times.Once);
        }

        [Test]
        public async Task Workflow_Default_Stop_Strategy_Is_Continuous_Processing()
        {
            var session = new ServerShotSession();

            Assert.IsTrue(session.StopStrategy is ContinousProcessingStategy);
        }

        [Test]
        public async Task Workflow_Modules_Can_Be_Resolved_Via_The_IOC_Container()
        {
            //arrange
            var mockIOCContainer = new Mock<IIocContainer>();
            mockIOCContainer.Setup(x => x.Get<IServerShotModule>(typeof (AlertStub))).Returns(new AlertStub(new Alert()));

            WorkflowEnvironment environment = WorkflowEnvironment.BuildEnvironment()
                .WithIOCContainer(mockIOCContainer.Object)
                .Build();

            ServerShotSession session = environment.CreateSession();

            //act
            await ServerShotSession.StartBuildWithSession(session)
                .AddModule<AlertStub>()
                .WithQueueMechanism(new InMemoryQueueFactory())
                .RunAsync();

            //assert
            mockIOCContainer.Verify(x => x.Get<IServerShotModule>(typeof (AlertStub)), Times.Once);
        }

        [Test]
        public async Task Workflow_Modules_Services_Can_Be_Injected_Via_The_IOC_Container()
        {
            //arrange
            WorkflowEnvironment environment = WorkflowEnvironment.BuildEnvironment()
                .WithIOCContainer(new NinjectIocContainer())
                .RegisterType<ServerShotSessionFakes.IToResolve, ServerShotSessionFakes.ConcreteResolve>().
                Build();

            ServerShotSession session = environment.CreateSession();

            //act
            ServerShotSession builtSession = ServerShotSession.StartBuildWithSession(session)
                .AddModule<ServerShotSessionFakes.IOCInjectedFake>()
                .WithQueueMechanism(new InMemoryQueueFactory())
                .ServerShotSession;

            await builtSession.Start();

            //assert
            var runningModule = builtSession.RunningModules.First() as ServerShotSessionFakes.IOCInjectedFake;
            Assert.IsTrue(runningModule.ToResolve is ServerShotSessionFakes.ConcreteResolve);
        }

        [Test]
        public async Task Workflow_Session_Stop_Strategy_Determines_Workflow_Session_Stopping()
        {
            var session = new ServerShotSession();
            session.CloudQueueFactory = TestHelpers.CreateNonEmptyStubQueueFactory().Object;

            var mockStopStrategy = new Mock<IProcessingStopStrategy>();
            session.StopStrategy = mockStopStrategy.Object;
            mockStopStrategy.Setup(x => x.ShouldStop(It.IsAny<ServerShotSession>())).Returns(true);
            session.Settings.CheckStopStrategyEvery = TimeSpan.FromMilliseconds(1);
            session.Modules.Add(new Fakes.RaisesProcessingStateViaEnum(ProcessingResult.Success));

            await session.Start();

            mockStopStrategy.Verify(x => x.ShouldStop(It.IsAny<ServerShotSession>()));
        }

        [Test]
        public async Task Workflow_Sessions_Default_Settings_Get_Injected_Into_Modules()
        {
            //arrange
            var session = new ServerShotSession();
            var workflowModuleSettings = new ServerShotModuleSettings();
            session.DefaultModuleSettings = workflowModuleSettings;
            session.CloudQueueFactory = TestHelpers.CreateNonEmptyStubQueueFactory().Object;
            var module = new Fakes.RaisesProcessingStateViaEnum(ProcessingResult.Success);
            session.Modules.Add(module);

            //act
            await session.Start();

            //assert
            Assert.AreSame(workflowModuleSettings, module.Settings);

        }
    }
}