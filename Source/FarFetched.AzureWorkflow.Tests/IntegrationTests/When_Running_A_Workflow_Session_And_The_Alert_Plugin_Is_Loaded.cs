using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Implementation;
using ServerShot.Framework.Core.Plugins.Alerts;
using ServerShot.Framework.Core.ServiceBus;
using ServerShot.Framework.Core.Builder;
using ServerShot.Framework.Tests.IntegrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;

namespace ServerShot.Framework.Tests.IntegrationTests
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
            await ServerShotSession.StartBuild()
                .AddModule(new Fakes.AlertModuleFake(message))
                .WithQueueMechanism(new InMemoryQueueFactory())
                .AttachAlertManager(alertManager.Object)
                .RunAsync();

            //assert
            alertManager.Verify(x=>x.FireAlert(It.IsAny<Alert>()), Times.Once);
        }
    }
}
