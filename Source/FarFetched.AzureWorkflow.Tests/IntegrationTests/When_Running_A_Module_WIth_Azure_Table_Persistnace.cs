using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core;
using ServerShot.Framework.Core.Builder;
using ServerShot.Framework.Core.Implementation;
using ServerShot.Framework.Core.Implementation.Persistance;
using ServerShot.Framework.Core.Plugins.Persistance;
using ServerShot.Framework.Core.Queue;
using FarFetched.AzureWorkflow.Tests.Helpers;
using NUnit.Framework;
using Servershot.Framework.Entities;

namespace ServerShot.Framework.Tests.IntegrationTests
{
    [TestFixture]
    public class When_Running_A_Module_WIth_Azure_Table_Persistnace
    {
        #region PersistanceHelper

        private IPersistanceManager GetAzurePersistance()
        {
            return AzurePersistanceHelper.CreatePersistanceClient();
        }

        #endregion

        [Test]
        [ExpectedException(typeof(WorkflowConfigurationException))]
        public async Task Not_Providing_A_Session_Name_Raises_A_Configuration_Exception()
        {
            var storeKey = "apple";
            var storeValue = Guid.NewGuid();
            var session = new ServerShotLinearSession();

            var session1 = await ServerShotLinearSession.StartBuildWithSession(session)
                .AddModule<Fakes.StoreValueModule>(storeKey, storeValue)
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .AttachSessionPersistance(GetAzurePersistance())
                .RunAsync();
        }

        [Test]
        public async Task Not_Providing_Persistance_Component_Results_In_Critical_Errors()
        {
            var storeKey = "apple";
            var storeValue = Guid.NewGuid();
            var session = new ServerShotLinearSession();
            session.SessionName = "Utsession";
            string message = null;

            session.OnFailure += (module, s) =>
            {
                message = s;
            };

            var session1 = await ServerShotLinearSession.StartBuildWithSession(session)
                .AddModule<Fakes.StoreValueModule>(storeKey, storeValue)
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .RunAsync();

            Assert.IsTrue(message != null && message.Contains("please attach a persistance component"));
        }

        [Test]
        public async Task A_Module_Can_Store_Then_Retrieve_A_Value_In_Seperate_Sessions()
        {
            var sessionName = "testsession";
            var storeKey = "apple";
            var storeValue = Guid.NewGuid();

            var session1 = await ServerShotLinearSession.StartBuild()
                .AddName(sessionName)
                .AddModule<Fakes.StoreValueModule>(storeKey, storeValue)
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .AttachSessionPersistance(GetAzurePersistance())
                .RunAsync();

            var session2 = await ServerShotLinearSession.StartBuild()
                .AddName(sessionName)
                .AddModule<Fakes.RetreiveValueModule>(storeKey)
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .AttachSessionPersistance(GetAzurePersistance())
                .RunAsync();

            var retrivalModule = session1.RunningModules.First() as Fakes.RetreiveValueModule;

            Assert.IsTrue(storeValue.ToString().Equals(retrivalModule.Retreived.ToString()));
        }


        private class Fakes
        {
            public class StoreValueModule : InitialServerShotModule<object>
            {
                private readonly string _key;
                private readonly object _value;

                public StoreValueModule(string key, object value)
                {
                    _key = key;
                    _value = value;
                }

                public async override Task OnStart()
                {
                    await this.StoreAsync(_key, _value);
                }
            }

            public class RetreiveValueModule : InitialServerShotModule<object>
            {
                private readonly string _key;
                public object Retreived { get; private set; }

                public RetreiveValueModule(string key)
                {
                    _key = key;
                }

                public async override Task OnStart()
                {
                    this.Retreived = await this.RetrieveAsync<object>(_key);
                }
            }
        }
    }
}
