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
    public class Scenario3 : IExampleScenario
    {
        public string Description
        {
            get { return "Improves the performance bottlenecks of Scenario 1 by implementing parallelism"; }
        }

        /// <summary>
        /// Scenario 1 was great, but stock management is SLOOOOWWW
        /// 
        /// Here, we spawn 3 instances of stock management to work in parallel. This works particularly well because of the 
        /// queueing architecture as the 3 modules can compete for the data, implementing queue based load balancing.
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
                        .WithInstances(3) //right here!!
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