using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Workflow.Core;
using Azure.Workflow.Core.Builder;
using Azure.Workflow.Core.Implementation;
using Azure.Workflow.Core.Implementation.Persistance;
using Azure.Workflow.Core.Plugins.Persistance;
using Azure.Workflow.Core.ServiceBus;
using FarFetched.AzureWorkflow.Tests.Helpers;
using NUnit.Framework;

namespace Azure.Workflow.Tests.IntegrationTests
{
    [TestFixture]
    public class When_Running_A_Module_WIth_Azure_Table_Persistnace
    {
        #region PersistanceHelper

        private PersistanceManagerBase GetAzurePersistance()
        {
            return AzurePersistanceHelper.CreatePersistanceClient();
        }

        #endregion

        [Test]
        [ExpectedException(typeof(AzureWorkflowConfigurationException))]
        public async Task Not_Providing_A_Session_Name_Raises_A_Configuration_Exception()
        {
            var storeKey = "apple";
            var storeValue = Guid.NewGuid();
            var session = new WorkflowSession();

            var session1 = await WorkflowSession.StartBuildWithSession(session)
                .AddModule(new Fakes.StoreValueModule(storeKey, storeValue))
                .WithQueueMechanism(new InMemoryQueueFactory())
                .AttachPersistance(GetAzurePersistance())
                .RunAsync();
        }

        [Test]
        public async Task Not_Providing_Persistance_Values_Results_In_Critical_Errors()
        {
            var storeKey = "apple";
            var storeValue = Guid.NewGuid();
            var retrivalModule = new Fakes.RetreiveValueModule(storeKey);
            var session = new WorkflowSession();
            session.SessionName = "Utsession";
            string message = null;

            session.OnFailure += (module, s) =>
            {
                message = s;
            };

            var session1 = await WorkflowSession.StartBuildWithSession(session)
                .AddModule(new Fakes.StoreValueModule(storeKey, storeValue))
                .WithQueueMechanism(new InMemoryQueueFactory())
                .RunAsync();

            Assert.IsTrue(message != null && message.Contains("please attach a persistance component"));
        }

        [Test]
        public async Task A_Module_Can_Store_Then_Retrive_A_Value_In_Seperate_Sessions()
        {
            var sessionName = "testsession";
            var storeKey = "apple";
            var storeValue = Guid.NewGuid();
            var retrivalModule = new Fakes.RetreiveValueModule(storeKey);

            var session1 = await WorkflowSession.StartBuild()
                .AddName(sessionName)
                .AddModule(new Fakes.StoreValueModule(storeKey, storeValue))
                .WithQueueMechanism(new InMemoryQueueFactory())
                .AttachPersistance(GetAzurePersistance())
                .RunAsync();

            var session2 = await WorkflowSession.StartBuild()
                .AddName(sessionName)
                .AddModule(retrivalModule)
                .WithQueueMechanism(new InMemoryQueueFactory())
                .AttachPersistance(GetAzurePersistance())
                .RunAsync();

            Assert.IsTrue(storeValue.ToString().Equals(retrivalModule.Retreived.ToString()));
        }


        private class Fakes
        {
            public class StoreValueModule : InitialWorkflowModule<object>
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

            public class RetreiveValueModule : InitialWorkflowModule<object>
            {
                private readonly string _key;
                public object Retreived { get; private set; }

                public RetreiveValueModule(string key)
                {
                    _key = key;
                }

                public async override Task OnStart()
                {
                    this.Retreived = await this.RetrieveAsync(_key);
                }
            }
        }
    }
}
