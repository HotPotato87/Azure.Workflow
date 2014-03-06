using System.Threading.Tasks;
using ServerShot.Framework.Core.Builder;
using ServerShot.Framework.Core.Entities.Environment;
using ServerShot.Framework.Core.Extentions;
using ServerShot.Framework.Core.Implementation;
using ServerShot.Framework.Core.Implementation.IOC;
using ServerShot.Framework.Core.Implementation.Logging;
using ServerShot.Framework.Core.Implementation.Reporting;
using ServerShot.Framework.Core.Implementation.StopStrategy;
using ServerShot.Framework.Core.Queue;
using Servershot.Framework.Extentions;
using Servershot.WebsiteOrderSample.Modules;
using Servershot.WebsiteOrderSample.Services;

namespace Servershot.WebsiteOrderSample.ExampleScenarios
{
    public class Scenario4 : IExampleScenario
    {
        public string Description
        {
            get { return "Introduces a warehousing component that does not always fulfil orders. This is reflected in the reporting output"; }
        }

        /// <summary>
        /// Scenario 4 assigns a 20% failure rate in the warehousing componont to demonstrate the categorisation and reports. This information 
        /// is displayed at the end of processing. 
        /// 
        /// Swapping out a console reporting component for an email reporting component would send a summary email. (See SendGridReportComponent not yet implemented)
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            var environment = ServerShotEnvironment.BuildEnvironment()
                .WithIOCContainer(new NinjectIocContainer())
                .RegisterType<IStockManagementApi, SimulatedStockManagement>()
                .RegisterType<IWarehouseManagementApi, SimulatedWarehouseApi>(x=>
                    x.PercentageOrderCorrectlyPlaced = 0.8) //20% Failure rate
                .Environment;

            await ServerShotSessionBase.StartBuildWithSession(environment.CreateLinearSession())
                        .AddModule<WebsiteUIModule>()
                        .AddModule<StockManagementModule>()
                            .WithInstances(3)
                        .AddModule<WarehousingModule>()
                    .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                    .AttachSessionAlertManager(new ConsoleAlertManager())
                    .AttachSessionLogger(new ConsoleLogger(showInfrastructure: false))
                    .AttachSessionReportGenerator(new ConsoleReportGenerator()) //summarised through reporting plugin
                    .WithSessionStopStrategy(new LinearProcessingFinishedStopStrategy())
                .RunAsync();
        }
    }
}