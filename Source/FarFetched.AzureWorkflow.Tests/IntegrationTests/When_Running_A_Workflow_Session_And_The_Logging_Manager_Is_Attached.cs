using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Implementation;
using ServerShot.Framework.Core.Plugins.Alerts;
using ServerShot.Framework.Core.Queue;
using ServerShot.Framework.Core.Builder;
using Servershot.Framework.Entities;
using ServerShot.Framework.Tests.IntegrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using CollectionAssert = Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert;

namespace ServerShot.Framework.Tests.IntegrationTests
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
            await ServerShotLinearSession.StartBuild()
                .AddModule<Fakes.LogsMessageFake>(message)
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .AttachSessionLogger(logManager.Object)
                .RunAsync();

            //assert
            logManager.Verify(x=>x.OnLogMessage(It.IsAny<IServerShotModule>(), It.IsAny<LogMessage>()), Times.AtLeastOnce);
        }
    }
}
