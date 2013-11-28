using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core.Builder;
using FarFetched.AzureWorkflow.Core.Implementation;
using FarFetched.AzureWorkflow.Core.Plugins.Alerts;
using FarFetched.AzureWorkflow.Core.ServiceBus;
using FarFetched.AzureWorkflow.Tests.IntegrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;

namespace FarFetched.AzureWorkflow.Tests.WhenRunningWorkflowModule
{
    [TestClass]
    public class When_Running_A_Workflow_Session_And_The_Alert_Plugin_Is_Loaded
    {
        [TestMethod]
        public async Task Alerts_From_With_Modules_Are_Sent_To_The_Alert_Manager()
        {
            //arrange
            var message = "test message";
            var alertManager = new Mock<AlertManagerBase>();

            //act
            await WorkflowSession.StartBuild()
                .AddModule(new Fakes.AlertModuleFake(message))
                .WithQueueMechanism(new InMemoryQueueFactory())
                .AttachAlertManager(alertManager.Object)
                .RunAsync();

            //assert
            alertManager.Verify(x=>x.FireAlert(It.IsAny<Alert>()), Times.Once);
        }
    }
}
