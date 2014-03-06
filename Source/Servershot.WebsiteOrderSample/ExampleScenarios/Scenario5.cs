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
using Servershot.Framework.Plugins.Scaling;
using Servershot.WebsiteOrderSample.Modules;
using Servershot.WebsiteOrderSample.Services;

namespace Servershot.WebsiteOrderSample.ExampleScenarios
{
    public class Scenario5 : IExampleScenario
    {
        public string Description
        {
            get { return "Replaces concrete parallelism with scaler component that monitors modules and decides to scale up depending on output"; }
        }

        /// <summary>
        /// Scenario 5 doesnt explicitly state an instnace count of a module. Instead an instance scaler is assigned to a module to monitor throughout 
        /// and to decide if new instance is needed based on parameters
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            var environment = ServerShotEnvironment.BuildEnvironment()
                .WithIOCContainer(new NinjectIocContainer())
                .RegisterType<IStockManagementApi, SimulatedStockManagement>()
                .RegisterType<IWarehouseManagementApi, SimulatedWarehouseApi>(x =>
                    x.PercentageOrderCorrectlyPlaced = 0.8)
                .Environment;

            await ServerShotSessionBase.StartBuildWithSession(environment.CreateLinearSession())
                        .AddModule<WebsiteUIModule>()
                        .AddModule<StockManagementModule>()
                            .WithInstanceScaler(new QueueBacklogScaler() { MaxInstances = 5, QueueThreshold = 5}) //instance scaler
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