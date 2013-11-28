using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core.Builder;
using FarFetched.AzureWorkflow.Core.Implementation;
using FarFetched.AzureWorkflow.Core.ServiceBus;
using FarFetched.AzureWorkflow.Tests.IntegrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace FarFetched.AzureWorkflow.Tests.WhenRunningWorkflowModule
{

    [TestClass]
    public class When_Running_A_Workflow_Session_And_Reporting_Plugin_Is_Attached
    {
        [Test]
        public async Task Successfully_Processed_Items_Are_Added_To_Report()
        {
            //arrange
            var reportGenerator = new Fakes.ReportGenerationFake();
            var payLoad = new List<object>() {new object(), new object()};

            //act
            await WorkflowSession.StartBuild()
                .AddModule(new Fakes.AddsToQueueProcessingFake(payLoad, typeof(Fakes.RecievesFromQueueProcessingFake)))
                .AddModule(new Fakes.RecievesFromQueueProcessingFake())
                .WithQueueMechanism(new InMemoryQueueFactory())
                .AttachReportGenerator(reportGenerator)
                .RunAsync();

            //assert
            Assert.IsTrue(reportGenerator.ModuleProcessingSummaries.Count == 2);
            Assert.IsTrue(reportGenerator.ModuleProcessingSummaries.Any(x=>x.SuccessfullyProcessed == payLoad.Count));
        }

        [Test]
        public async Task Errored_Module_Items_Are_Added_To_Report()
        {
            //arrange
            var reportGenerator = new Fakes.ReportGenerationFake();
            var payLoad = new List<object>() { new object(), new object() };
            var sampleException = new Exception("Expected Exception");

            //act
            await WorkflowSession.StartBuild()
                .AddModule(new Fakes.AddsToQueueProcessingFake(payLoad, typeof(Fakes.QueueProcessingThowsErrorFake)))
                .AddModule(new Fakes.QueueProcessingThowsErrorFake(() => { return sampleException; }))
                .WithQueueMechanism(new InMemoryQueueFactory())
                .AttachReportGenerator(reportGenerator)
                .RunAsync();

            //assert
            Assert.IsTrue(reportGenerator.ModuleProcessingSummaries.Any(x => x.Errors == payLoad.Count));
            Assert.IsTrue(reportGenerator.ModuleProcessingSummaries.Any(x=>x.ErrorList.All(t=>t == sampleException)));
        }

        [Test]
        public async Task Custom_Processed_Items_Are_Categorized_In_Report()
        {
            throw new NotImplementedException();
        }

        [Test]
        public async Task Duration_Of_Module_Runs_Are_Added_To_Summary()
        {
            throw new NotImplementedException();
        }

        [Test]
        public async Task Overall_Duration_Of_Session_Is_Added_To_Summary()
        {
            throw new NotImplementedException();
        }
    }
}
