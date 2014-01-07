using System;
using System.Linq;
using System.Threading.Tasks;
using Azure.Workflow.Core;
using Azure.Workflow.Core.Architecture;
using Azure.Workflow.Core.Builder;
using Azure.Workflow.Core.Entities.Environment;
using Azure.Workflow.Core.Enums;
using Azure.Workflow.Core.Extentions;
using Azure.Workflow.Core.Implementation;
using Azure.Workflow.Core.Implementation.IOC;
using Azure.Workflow.Core.Implementation.StopStrategy;
using Azure.Workflow.Core.Interfaces;
using Azure.Workflow.Core.Plugins;
using Azure.Workflow.Core.Plugins.Alerts;
using Azure.Workflow.Core.ServiceBus;
using Azure.Workflow.Tests.Helpers;
using Moq;
using NUnit.Framework;

namespace Azure.Workflow.Tests.UnitTests
{
    [TestFixture]
    public class When_Running_A_Workflow_Session
    {
        private Mock<ReportGenerationPlugin> _summaryReportGenerator;
        private Mock<IWorkflowModule> _module1;
        private Mock<IWorkflowModule> _module2;

        public WorkflowSession GetStandardSession()
        {
            _module1 = new Mock<IWorkflowModule>();
            _module2 = new Mock<IWorkflowModule>();

            _module1.Setup(x => x.StartAsync()).Returns(async () => { await Task.Delay(10); });
            _module2.Setup(x => x.StartAsync()).Returns(async () => { await Task.Delay(10); });

            return WorkflowSession.StartBuild()
                .AddModule(_module1.Object)
                .AddModule(_module2.Object).WorkflowSession;
        }

        public WorkflowSession GetStandardSessionWithQueue()
        {
            _module1 = new Mock<IWorkflowModule>();
            _module2 = new Mock<IWorkflowModule>();

            _module1.Setup(x => x.StartAsync()).Returns(async () => { await Task.Delay(10); });
            _module2.Setup(x => x.StartAsync()).Returns(async () => { await Task.Delay(10); });

            return WorkflowSession.StartBuild()
                .AddModule(_module1.Object)
                .AddModule(_module2.Object)
                .WithQueueMechanism(new InMemoryQueueFactory())
                .WorkflowSession;
        }

        public class WorkflowSessionFakes
        {
            public class ConcreteResolve : IToResolve
            {
            }

            public class IOCInjectedFake : InitialWorkflowModule<object>
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
            WorkflowSession session = GetStandardSessionWithQueue();

            //act
            await session.Start();

            //assert
            Assert.IsTrue(session.RunningModules.Any(x => x == _module1.Object));
            Assert.IsTrue(session.RunningModules.Any(x => x == _module2.Object));
        }

        [Test]
        public async Task Not_Specifying_A_Workflow_Environment_Results_In_Standard_Environment_Being_Built()
        {
            _module1 = new Mock<IWorkflowModule>();
            _module2 = new Mock<IWorkflowModule>();

            _module1.Setup(x => x.StartAsync()).Returns(async () => { await Task.Delay(10); });
            _module2.Setup(x => x.StartAsync()).Returns(async () => { await Task.Delay(10); });

            WorkflowSession session = WorkflowSession.StartBuild()
                .AddModule(_module1.Object)
                .AddModule(_module2.Object)
                .WithQueueMechanism(new InMemoryQueueFactory())
                .WorkflowSession;

            Assert.IsTrue(session.Environment != null);
        }

        [Test]
        public async Task Session_Calls_Register_Finished()
        {
            //arrange
            WorkflowSession session = GetStandardSessionWithQueue();
            bool called = false;

            session.OnSessionFinished += workflowSession => { called = true; };

            //act
            await session.Start();

            //assert
            Assert.IsTrue(called);
        }

        [Test]
        public async Task Session_Populates_Total_Duration_In_Real_Time()
        {
            //act
            WorkflowSession session = GetStandardSessionWithQueue();

            //arrange
            await session.Start();

            //assert
            Assert.IsTrue(session.TotalDuration.Ticks > 0);
        }

        [Test]
        public async Task Session_Sets_Start_Time_When_Started()
        {
            //act
            WorkflowSession session = GetStandardSessionWithQueue();

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
            WorkflowSession session = GetStandardSession();

            //act 
            await session.Start();
        }

        [Test]
        public async Task Session_sets_End_Time_When_Finished()
        {
            //act
            WorkflowSession session = GetStandardSessionWithQueue();

            //arrange
            await session.Start();

            //assert
            Assert.IsTrue(session.Ended != default(DateTime));
        }

        [Test]
        public async Task Specifying_A_Workflow_Environment_Results_In_Specified_One_Being_Used()
        {
            _module1 = new Mock<IWorkflowModule>();
            _module2 = new Mock<IWorkflowModule>();

            _module1.Setup(x => x.StartAsync()).Returns(async () => { await Task.Delay(10); });
            _module2.Setup(x => x.StartAsync()).Returns(async () => { await Task.Delay(10); });

            var mockEnvironment = new Mock<WorkflowEnvironment>();
            var session = new WorkflowSession(mockEnvironment.Object);

            WorkflowSession builtSession = WorkflowSession.StartBuildWithSession(session)
                .AddModule(_module1.Object)
                .AddModule(_module2.Object)
                .WithQueueMechanism(new InMemoryQueueFactory())
                .WorkflowSession;

            Assert.IsTrue(builtSession.Environment == mockEnvironment.Object);
        }

        [Test]
        public async Task WorkflowSessionStartCallsStartOnModules()
        {
            //arrange
            WorkflowSession session = GetStandardSessionWithQueue();

            //act
            await session.Start();

            //assert
            _module1.Verify(x => x.StartAsync(), Times.Once);
            _module2.Verify(x => x.StartAsync(), Times.Once);
        }

        [Test]
        public async Task Workflow_Default_Stop_Strategy_Is_Continuous_Processing()
        {
            var session = new WorkflowSession();

            Assert.IsTrue(session.StopStrategy is ContinousProcessingStategy);
        }

        [Test]
        public async Task Workflow_Modules_Can_Be_Resolved_Via_The_IOC_Container()
        {
            //arrange
            var mockIOCContainer = new Mock<IIocContainer>();
            mockIOCContainer.Setup(x => x.Get<IWorkflowModule>(typeof (AlertStub))).Returns(new AlertStub(new Alert()));

            WorkflowEnvironment environment = WorkflowEnvironment.BuildEnvironment()
                .WithIOCContainer(mockIOCContainer.Object)
                .Build();

            WorkflowSession session = environment.CreateSession();

            //act
            await WorkflowSession.StartBuildWithSession(session)
                .AddModule<AlertStub>()
                .WithQueueMechanism(new InMemoryQueueFactory())
                .RunAsync();

            //assert
            mockIOCContainer.Verify(x => x.Get<IWorkflowModule>(typeof (AlertStub)), Times.Once);
        }

        [Test]
        public async Task Workflow_Modules_Services_Can_Be_Injected_Via_The_IOC_Container()
        {
            //arrange
            WorkflowEnvironment environment = WorkflowEnvironment.BuildEnvironment()
                .WithIOCContainer(new NinjectIOCContainer())
                .RegisterType<WorkflowSessionFakes.IToResolve, WorkflowSessionFakes.ConcreteResolve>().
                Build();

            WorkflowSession session = environment.CreateSession();

            //act
            WorkflowSession builtSession = WorkflowSession.StartBuildWithSession(session)
                .AddModule<WorkflowSessionFakes.IOCInjectedFake>()
                .WithQueueMechanism(new InMemoryQueueFactory())
                .WorkflowSession;

            await builtSession.Start();

            //assert
            var runningModule = builtSession.RunningModules.First() as WorkflowSessionFakes.IOCInjectedFake;
            Assert.IsTrue(runningModule.ToResolve is WorkflowSessionFakes.ConcreteResolve);
        }

        [Test]
        public async Task Workflow_Session_Stop_Strategy_Determines_Workflow_Session_Stopping()
        {
            var session = new WorkflowSession();
            session.CloudQueueFactory = TestHelpers.CreateNonEmptyStubQueueFactory().Object;

            var mockStopStrategy = new Mock<IProcessingStopStrategy>();
            session.StopStrategy = mockStopStrategy.Object;
            mockStopStrategy.Setup(x => x.ShouldStop(It.IsAny<WorkflowSession>())).Returns(true);
            session.Settings.CheckStopStrategyEvery = TimeSpan.FromMilliseconds(1);
            session.Modules.Add(new Fakes.RaisesProcessingStateViaEnum(ProcessingResult.Success));

            await session.Start();

            mockStopStrategy.Verify(x => x.ShouldStop(It.IsAny<WorkflowSession>()));
        }
    }
}