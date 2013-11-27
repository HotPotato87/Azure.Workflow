using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core;
using FarFetched.AzureWorkflow.Core.Architecture;
using FarFetched.AzureWorkflow.Core.Builder;
using FarFetched.AzureWorkflow.Core.Implementation;
using FarFetched.AzureWorkflow.Core.Interfaces;
using FarFetched.AzureWorkflow.Core.Plugins;
using FarFetched.AzureWorkflow.Core.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ninject;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace FarFetched.AzureWorkflow.Tests
{
    [TestFixture]
    public class When_Workflow_Session_Is_Run
    {
        private Mock<ReportGenerationPlugin> _summaryReportGenerator;
        private Mock<IWorkflowModule> _module1;
        private Mock<IWorkflowModule> _module2;

        #region Helpers

        public WorkflowSession GetStandardSession()
        {
            _module1 = new Mock<IWorkflowModule>();
            _module2 = new Mock<IWorkflowModule>();

            _module1.Setup(x => x.StartAsync()).Returns(async() => { await Task.Delay(10); });
            _module2.Setup(x => x.StartAsync()).Returns(async () => { await Task.Delay(10); });

            WorkflowSession session = new WorkflowSession();

            return WorkflowSession.StartBuild()
                .AddModule(_module1.Object)
                .AddModule(_module2.Object).WorkflowSession;
        }

        #endregion

        [Test]
        public async Task Session_Throws_If_No_Queue_Mechanism()
        {
            throw new NotImplementedException();
        }

        [Test]
        public async Task WorkflowSessionStartCallsStartOnModules()
        {
            //arrange
            var session = GetStandardSession();

            //act
            await session.Start();

            //assert
            _module1.Verify(x=>x.StartAsync(), Times.Once);
            _module2.Verify(x => x.StartAsync(), Times.Once);
        }

        [Test]
        public async Task Modules_Are_Added_To_Running_Sessions()
        {
            
        }

        [Test]
        public async Task Session_Calls_Register_Finished()
        {
            
        }
        
    }

}
