using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Workflow.Core;
using Azure.Workflow.Core.Entities.Scheduler;
using Azure.Workflow.Core.Entities.Scheduler.Deployments;
using Azure.Workflow.Core.Extentions;
using Azure.Workflow.Core.Implementation;
using Moq;
using NUnit.Framework;

namespace Azure.Workflow.Tests.UnitTests
{
    [TestFixture]
    public class When_Scheduling_A_Workflow_Session
    {
        [Test]
        public async Task Developer_Can_Specify_A_Deployment_Strategy()
        {
            var stubSession = new Mock<WorkflowSession>();
            IDeploymentStrategy strategy = new LocalDeploymentStrategy();

            await WorkflowScheduler.Build()
                .AddSession(stubSession.Object)
                .WithDeploymentStrategy(strategy)
                .BeginSchedulerAsync();
        }

        [Test]
        [ExpectedException(typeof(WorkflowConfigurationException), ExpectedMessage = "There are no workflow sessions to execute. Please see AddSession() in the fluent API")]
        public async Task Calling_Run_Now_Without_Adding_Session_Throws_Exception()
        {
            IDeploymentStrategy strategy = new LocalDeploymentStrategy();
            await WorkflowScheduler.Build()
                 .BeginSchedulerAsync();
        }

        [Test]
        [ExpectedException(typeof(WorkflowConfigurationException), ExpectedMessage = "There is no deployment strategy added to this scheduler. Please add one before execution")]
        public async Task Calling_Run_Now_With_No_Deployment_Strategy_Throws_Exception()
        {
            var stubSession = new Mock<WorkflowSession>();
            await WorkflowScheduler.Build()
                .AddSession(stubSession.Object)
                 .BeginSchedulerAsync();
        }
    }
}
