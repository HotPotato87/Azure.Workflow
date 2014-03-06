using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Builder;
using ServerShot.Framework.Core.Implementation;
using ServerShot.Framework.Core.Implementation.Logging;
using ServerShot.Framework.Core.Implementation.Reporting;
using ServerShot.Framework.Core.Interfaces;
using ServerShot.Framework.Core.Plugins.Alerts;
using ServerShot.Framework.Core.Plugins.Persistance;
using ServerShot.Framework.Core.Queue;
using Servershot.Framework.Entities;
using Servershot.Framework.Extentions;

namespace ServerShot.Framework.Tests.IntegrationTests.Builders
{
    [TestFixture]
    public class When_Building_A_Servershot_Session
    {
        [Test]
        public async Task Module_Level_Attach_Logger_Overwrites_Main()
        {
            var sessionLevelLogger = new Mock<ConsoleLogger>() { CallBase = true };
            var moduleLevelLogger = new Mock<ConsoleLogger>() { CallBase = true };

            var session = await ServerShotLinearSession.StartBuild()
                .AddModule<Fakes.LogMessageFake>("Test Message")
                    .WithModuleLogger(moduleLevelLogger.Object)
            .AttachSessionLogger(sessionLevelLogger.Object)
            .AttachSessionQueueMechanism(new InMemoryQueueFactory())
            .RunAsync();

            sessionLevelLogger.Verify(x => x.OnLogMessage(It.IsAny<IServerShotModule>(), It.Is<LogMessage>(lm=>lm.Message.Contains("Test Message"))), Times.Never);
            moduleLevelLogger.Verify(x => x.OnLogMessage(It.IsAny<IServerShotModule>(), It.Is<LogMessage>(lm => lm.Message.Contains("Test Message"))), Times.Once);
        }

        [Test]
        public async Task Module_Level_Attach_Persistance_Overwrites_Main()
        {
            var sessionLevelLogger = new Mock<PersistanceManagerBase>() { CallBase = true };
            var moduleLevelLogger = new Mock<PersistanceManagerBase>() { CallBase = true };

            var session = await ServerShotLinearSession.StartBuild()
                .AddModule<Fakes.LogMessageFake>("Test Message")
                    .WithModulePersistance(moduleLevelLogger.Object)
            .AttachSessionPersistance(sessionLevelLogger.Object)
            .AttachSessionQueueMechanism(new InMemoryQueueFactory())
            .RunAsync();

            sessionLevelLogger.Verify(x => x.OnModuleStarted(It.IsAny<IServerShotModule>()), Times.Never);
            moduleLevelLogger.Verify(x => x.OnModuleStarted(It.IsAny<IServerShotModule>()));
        }

        [Test]
        public async Task Module_Level_Attach_Alerts_Overwrites_Main()
        {
            var sessionLevelLogger = new Mock<AlertManagerBase>() { CallBase = true };
            var moduleLevelLogger = new Mock<AlertManagerBase>() { CallBase = true };

            var session = await ServerShotLinearSession.StartBuild()
                .AddModule<Fakes.LogMessageFake>("Test Message")
                    .WithModuleAlertManager(moduleLevelLogger.Object)
            .AttachSessionAlertManager(sessionLevelLogger.Object)
            .AttachSessionQueueMechanism(new InMemoryQueueFactory())
            .RunAsync();

            sessionLevelLogger.Verify(x => x.OnModuleStarted(It.IsAny<IServerShotModule>()), Times.Never);
            moduleLevelLogger.Verify(x => x.OnModuleStarted(It.IsAny<IServerShotModule>()));
        }

        [Test]
        public async Task Module_Level_Attach_Queue_Overwrites_Main()
        {
            var sessionLevelLogger = new Mock<ICloudQueueFactory>() { CallBase = true };
            var moduleLevelLogger = new Mock<ICloudQueueFactory>() { CallBase = true };

            var session = await ServerShotLinearSession.StartBuild()
                .AddModule<Fakes.LogMessageFake>("Test Message")
                    .WithModuleQueueMechanism(moduleLevelLogger.Object)
            .AttachSessionQueueMechanism(sessionLevelLogger.Object)
            .RunAsync();

            sessionLevelLogger.Verify(x => x.CreateQueue(It.IsAny<IServerShotModule>()), Times.Never);
            moduleLevelLogger.Verify(x => x.CreateQueue(It.IsAny<IServerShotModule>()));
        }
    }
}
