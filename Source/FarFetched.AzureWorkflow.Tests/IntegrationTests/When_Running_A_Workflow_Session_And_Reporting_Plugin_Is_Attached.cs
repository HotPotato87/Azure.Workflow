using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Enums;
using ServerShot.Framework.Core.Implementation;
using ServerShot.Framework.Core.Queue;
using ServerShot.Framework.Core.Builder;
using Servershot.Framework.Entities;
using ServerShot.Framework.Tests.IntegrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ServerShot.Framework.Tests.IntegrationTests
{

    [TestClass]
    public class When_Running_A_Workflow_Session_And_Reporting_Plugin_Is_Attached
    {
        #region Helpers

        public List<Tuple<object, string>> GetSampleMessages()
        {
            var messages = new List<Tuple<object, string>>();
            messages.Add(new Tuple<object, string>(ProcessingResult.Success, null));
            messages.Add(new Tuple<object, string>(ProcessingResult.Success, null));
            messages.Add(new Tuple<object, string>(ProcessingResult.Success, null));
            messages.Add(new Tuple<object, string>(ProcessingResult.Fail, "Not enough chocolate"));
            messages.Add(new Tuple<object, string>(ProcessingResult.Fail, "Not enough cheese"));
            messages.Add(new Tuple<object, string>("Other", "Delivery API couldn't be contacted"));
            return messages;
        }

            #endregion

        [Test]
        public async Task Successfully_Processed_Items_Are_Added_To_Report()
        {
            //arrange
            var reportGenerator = new Fakes.ReportGenerationFake();
            var payLoad = new List<object>() {new object(), new object()};

            //act
            await ServerShotLinearSession.StartBuild()
                .AddModule<Fakes.AddsToQueueProcessingFake>(payLoad, typeof(Fakes.RecievesFromQueueProcessingFake))
                .AddModule<Fakes.RecievesFromQueueProcessingFake>()
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .AttachSessionReportGenerator(reportGenerator)
                .RunAsync();

            //assert
            Assert.IsTrue(reportGenerator.ModuleProcessingSummaries.Count == 2);
            Assert.IsTrue(reportGenerator.ModuleProcessingSummaries.Any(x => x.ResultCategories.ContainsKey(ProcessingResult.Success.ToString()) && x.ResultCategories[ProcessingResult.Success.ToString()] == payLoad.Count));
        }

        [Test]
        public async Task Errored_Module_Items_Are_Added_To_Report()
        {
            //arrange
            var reportGenerator = new Fakes.ReportGenerationFake();
            var payLoad = new List<object>() { new object(), new object() };
            var sampleException = new Exception("Expected Exception");

            //act
            await ServerShotLinearSession.StartBuild()
                .AddModule<Fakes.AddsToQueueProcessingFake>(payLoad, typeof(Fakes.QueueProcessingThowsErrorFake))
                .AddModule<Fakes.QueueProcessingThowsErrorFake>(new Func<Exception>(() => sampleException))
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .AttachSessionReportGenerator(reportGenerator)
                .RunAsync();

            //assert
            Assert.IsTrue(reportGenerator.ModuleProcessingSummaries.Any(x => x.Errors == payLoad.Count));
            Assert.IsTrue(reportGenerator.ModuleProcessingSummaries.Any(x=>x.ErrorList.All(t=>t == sampleException)));
        }

        [Test]
        public async Task Custom_Processed_Items_Are_Categorized_In_Report()
        {
            //arrange
            var reportGenerator = new Fakes.ReportGenerationFake();
            var payLoad = new List<object>() { new object(), new object() };

           
            //act
            await ServerShotLinearSession.StartBuild()
                .AddModule<Fakes.AddsToQueueProcessingFake>(payLoad, typeof(Fakes.QueueProcessingThowsErrorFake))
                .AddModule<Fakes.CategorisesProcessingResultFake>(GetSampleMessages())
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .WithSessionStopStrategy(new ServerShot.Framework.Core.Implementation.StopStrategy.NoQueueInteractionTimeoutStopStrategy(TimeSpan.FromMilliseconds(1000)))
                .AttachSessionReportGenerator(reportGenerator)
                .RunAsync();

            //assert
            Assert.IsTrue(reportGenerator.ModuleProcessingSummaries.Any(x=>x.ResultCategories.Count == 3));
            Assert.IsTrue(reportGenerator.ModuleProcessingSummaries.Any(x =>
                x.ResultCategoryExtraDetail.ContainsKey(ProcessingResult.Fail.ToString()) &&
                x.ResultCategoryExtraDetail[ProcessingResult.Fail.ToString()].Count == 2));
            Assert.IsTrue(reportGenerator.ModuleProcessingSummaries.Any(x =>
                      x.ResultCategoryExtraDetail.ContainsKey("Other") &&
                      x.ResultCategoryExtraDetail["Other"].Any(t=>t.Message == "Delivery API couldn't be contacted")));
        }

        [Test]
        public async Task Duration_Of_Module_Runs_Are_Added_To_Summary()
        {
            //arrange
            var reportGenerator = new Fakes.ReportGenerationFake();
            var payLoad = new List<object>() { new object(), new object() };

            //act
            await ServerShotLinearSession.StartBuild()
                .AddModule<Fakes.AddsToQueueProcessingFake>(payLoad, typeof(Fakes.CategorisesProcessingResultFake))
                .AddModule<Fakes.CategorisesProcessingResultFake>(GetSampleMessages())
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .AttachSessionReportGenerator(reportGenerator)
                .RunAsync();

            await Task.Delay(TimeSpan.FromTicks(100));

            //assert
            Assert.IsTrue(reportGenerator.ModuleProcessingSummaries.All(x => x.Duration.Ticks > 0));
        }

        [Test]
        public async Task Processed_Details_Are_Assigned_A_DateTime()
        {
            //arrange
            var reportGenerator = new Fakes.ReportGenerationFake();
            var payLoad = new List<object>() { new object(), new object() };


            //act
            await ServerShotLinearSession.StartBuild()
                .AddModule<Fakes.AddsToQueueProcessingFake>(payLoad, typeof(Fakes.CategorisesProcessingResultFake))
                .AddModule<Fakes.CategorisesProcessingResultFake>(GetSampleMessages())
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .AttachSessionReportGenerator(reportGenerator)
                .RunAsync();

            //assert
            Assert.IsTrue(
                reportGenerator.ModuleProcessingSummaries.All(
                    x => x.ResultCategoryExtraDetail.All(t => t.Value.All(d => d.ProcessedTime != null))));
        }

        [Test]
        public async Task Overall_Duration_Of_Session_Is_Added_To_Summary()
        {
            //arrange
            var reportGenerator = new Fakes.ReportGenerationFake();
            var payLoad = new List<object>() { new object(), new object() };

            //act
            await ServerShotLinearSession.StartBuild()
                .AddModule<Fakes.AddsToQueueProcessingFake>(payLoad, typeof(Fakes.CategorisesProcessingResultFake))
                .AddModule<Fakes.CategorisesProcessingResultFake>(GetSampleMessages())
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .AttachSessionReportGenerator(reportGenerator)
                .RunAsync();

            //assert
            Assert.IsTrue(reportGenerator.Session.TotalDuration != null &&
                          reportGenerator.Session.TotalDuration.Ticks > 0);
        }
    }
}
