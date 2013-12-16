using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Workflow.Core.Implementation;
using Azure.Workflow.Core.Plugins.Alerts;
using Azure.Workflow.Core.ServiceBus;
using Azure.Workflow.Core.Builder;
using Azure.Workflow.Tests.IntegrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using CollectionAssert = Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert;

namespace Azure.Workflow.Tests.IntegrationTests
{

    [TestClass]
    public class When_Running_A_Workflow_Session_And_The_Logging_Manager_Is_Attached
    {
        [Test]
        public async Task Modules_That_Log_Messages_Result_In_The_Logging_Manager_Being_Called()
        {
            //arrange
            var message = "test message";
            var logManager = new Mock<LogManagerBase>(MockBehavior.Loose);

            //act
            await WorkflowSession.StartBuild()
                .AddModule(new Fakes.LogsMessageFake(message))
                .WithQueueMechanism(new InMemoryQueueFactory())
                .AttachLogger(logManager.Object)
                .RunAsync();

            //assert
            logManager.Verify(x=>x.OnLogMessage(It.IsAny<LogMessage>()), Times.AtLeastOnce);
        }
    }
}
