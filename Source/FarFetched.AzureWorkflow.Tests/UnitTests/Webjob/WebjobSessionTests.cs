using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using ServerShot.Framework.Core.Entities.Environment;
using ServerShot.Framework.Core.Enums;
using ServerShot.Framework.Core.Plugins;
using Servershot.Framework.Entities.WebJob;
using Assert = NUnit.Framework.Assert;

namespace ServerShot.Framework.Tests.UnitTests.Webjob
{
    public class WebjobSessionTests
    {
        [Test]
        public void AddWebjob_JobAdded()
        {
            var session = CreateSession();

            session.AddWebJob<WebjobSessionFakes.SimpleWebJob>();

            Assert.IsTrue(session.RunningJobs.First().GetType().Name == "SimpleWebJob");
        }

        [Test]
        public async Task WebJob_Processed_CountsProcessed()
        {
            var session = CreateSession();

            session.AddWebJob<WebjobSessionFakes.SimpleWebJob>();

            var job = session.RunningJobs.First() as WebjobSessionFakes.SimpleWebJob;

            await job.ProcessItem(new object());

            Assert.AreEqual(1, job.ProcessedCount);
        }

        [Test]
        public async Task WebJob_ModuleFails_Continues()
        {
            var session = CreateSession();

            var module = new WebjobSessionFakes.WebjobDelegate(() =>
            {
                throw new Exception();
            });

            Exception expectedException = null;

            module.Exception += exception =>
            {
                expectedException = exception;
            };
            session.AddWebJob(module);

            await module.ProcessItem(new object());

            //we reached this line
            Assert.NotNull(expectedException);
        }

        [Test]
        public async Task WebJob_ModuleFailsThreeTime_TotalFailure()
        {
            var session = CreateSession();

            var module = new WebjobSessionFakes.WebjobDelegate(() =>
            {
                throw new Exception();
            });


            Exception expectedException = null;

            module.Exception += exception =>
            {
                expectedException = exception;
            };
            session.AddWebJob(module);


            bool didFail = false;
            module.Fail += () =>
            {
                didFail = true;
            };

            await module.ProcessItem(new object());
            await module.ProcessItem(new object());
            await module.ProcessItem(new object());

            //we reached this line
            Assert.AreEqual(ModuleState.Error, module.State = ModuleState.Error);
            Assert.IsTrue(didFail);
        }

        [Test]
        public async Task WebJob_Supports_Plugins()
        {
            var session = CreateSession();

            var simple = new WebjobSessionFakes.SimpleWebJob();
            session.AttachPlugin(new ProwlAlertManager());
            session.AttachPlugin(new SendgridReport());
            session.AttachPlugin(new ProwlNewActivityPlugin(TimeSpan.FromHours(1)));

            session.AddWebJob(simple);

            await simple.ProcessItem(new object());

            Assert.IsTrue(session.RunningPlugins.Count == 3);
        }

        private WebJobSession CreateSession()
        {
            return new WebJobSession(new ServerShotEnvironment());
        }
    }

    public class WebjobSessionFakes
    {
        public class SimpleWebJob : WebJobBase
        {
            public SimpleWebJob()
            {
                base.ThrowOnError = false;
            }

            protected override Task OnProcessItem<T>(T item)
            {
                return Task.FromResult<object>(null);
            }
        }

        public class WebjobDelegate : WebJobBase
        {
            private Action _e;

            public WebjobDelegate(Action e)
            {
                base.ThrowOnError = false;
                _e = e;
            }

            protected override Task OnProcessItem<T>(T item)
            {
                _e();
                return Task.FromResult<object>(null);
            }
        }
    }
}
