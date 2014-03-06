using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Builder;
using ServerShot.Framework.Core.Entities.Environment;
using ServerShot.Framework.Core.Extentions;
using ServerShot.Framework.Core.Implementation;
using ServerShot.Framework.Core.Implementation.IOC;
using ServerShot.Framework.Core.Interfaces;
using ServerShot.Framework.Core.Plugins.Alerts;
using ServerShot.Framework.Core.Queue;
using Servershot.Framework.Entities;
using Servershot.Framework.Extentions;

namespace ServerShot.Framework.Tests.UnitTests
{
    [TestFixture]
    public class When_Configuring_A_Servershot_Session
    {
        [Test]
        public async Task Modules_Can_Be_Built_With_Multiple_Instances()
        {
            ServerShotEnvironment environment = ServerShotEnvironment.BuildEnvironment()
               .WithIOCContainer(new NinjectIocContainer())
               .Build();

            //arrange
            ServerShotSessionBase session = ServerShotLinearSession.StartBuildWithSession(environment.CreateLinearSession())
                .AddModule<When_Running_A_ServerShot_Session.ServerShotSessionFakes.IOCInjectedFake>(new When_Running_A_ServerShot_Session.ServerShotSessionFakes.ConcreteResolve())
                    .OnCreate(x=>x.Settings.QueuePollTime = TimeSpan.FromDays(5))
                .WithInstances(3)
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .ServerShotSession;

            //act
            await session.Start();

            //assert
            Assert.IsTrue(session.RunningModules.Count(t => t.GetType() == typeof(When_Running_A_ServerShot_Session.ServerShotSessionFakes.IOCInjectedFake)) == 3);
        }

        

        //TODO : prove correct exceptions thrown from DI possibilities
    }
}
