using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Builder;
using ServerShot.Framework.Core.Interfaces;
using ServerShot.Framework.Core.Queue;
using Servershot.Framework.Entities;
using Servershot.Framework.Extentions;
using Servershot.Framework.Plugins.Scaling;

namespace ServerShot.Framework.Tests.UnitTests
{
    [TestFixture]
    public class InstanceScalerTests
    {
        [Test]
        public async Task If_Module_Backlog_Count_Over_Threshold_Instances_Are_Scaled_Up()
        {
            //arrange
            var instanceScaler = new QueueBacklogScaler();
            instanceScaler.QueueThreshold = 100;
            instanceScaler.Sample = TimeSpan.FromSeconds(1);
            instanceScaler.MaxInstances = 10;

            var fakeQueue = new Mock<ICloudQueue>();
            fakeQueue.Setup(x => x.Count).Returns(101);

            var fakeQueueFactory = new Mock<ICloudQueueFactory>();
            fakeQueueFactory.Setup(x => x.CreateQueue(It.IsAny<IServerShotModule>())).Returns(fakeQueue.Object);

            //act
            var session = await ServerShotLinearSession.StartBuild()
                .AddModule<Fakes.InitialContinuousRunModule>(10, 1000)
                    .WithInstanceScaler(instanceScaler)
                .AttachSessionQueueMechanism(fakeQueueFactory.Object)
                .RunAsync();

            //assert
            Assert.IsTrue(session.RunningModules.Count > 1);
        }
    }
}
