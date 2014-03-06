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
    public class Scenario2 : IExampleScenario
    {
        public string Description
        {
            get { return "Attaches a report generator to summarise the results in the console window"; }
        }

        /// <summary>
        /// Report generation allows results to be summarised from the entire session.
        /// 
        /// Currently supports a console log of results, and an email report that is not finished yet. 
        /// Its very easy to write a plugin to extend this further.
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            var environment = ServerShotEnvironment.BuildEnvironment()
                .WithIOCContainer(new NinjectIocContainer())
                .RegisterType<IStockManagementApi, SimulatedStockManagement>()
                .RegisterType<IWarehouseManagementApi, SimulatedWarehouseApi>()
                .Environment;

            await ServerShotSessionBase.StartBuildWithSession(environment.CreateLinearSession())
                    .AddModule<WebsiteUIModule>()
                    .AddModule<StockManagementModule>()
                    .AddModule<WarehousingModule>()
                .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                .AttachSessionAlertManager(new ConsoleAlertManager())
                .AttachSessionLogger(new ConsoleLogger(showInfrastructure: false))
                .AttachSessionReportGenerator(new ConsoleReportGenerator())
                .WithSessionStopStrategy(new LinearProcessingFinishedStopStrategy())
            .RunAsync();
        }
    }
}