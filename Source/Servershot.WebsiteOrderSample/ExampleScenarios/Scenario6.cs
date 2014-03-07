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
    public class Scenario6 : IExampleScenario
    {
        public string Description
        {
            get { return "Swap out in-memory queueing for Azure ServiceBus. This provides reliability and resilience in the case of failure."; }
        }

        //todo : refactor this out for security
        private string _serviceBusConnectionString = "Endpoint=sb://whatsonglobal.servicebus.windows.net/;SharedSecretIssuer=owner;SharedSecretValue=kVWNOEp5cdNS8rZytOc02Cvp1gr0gh0AEpOLWzejWU4=";

        /// <summary>
        /// Scenario 6 - swap the queue mechanism to ServiceBus in the builder.
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
                .WithInstanceScaler(new QueueBacklogScaler() { MaxInstances = 5, QueueThreshold = 5 }) //instance scaler
                .AddModule<WarehousingModule>()
                .AttachSessionQueueMechanism(new AzureServiceBusQueueFactory(new ServiceBusQueueSettings() { ConnectionString = _serviceBusConnectionString}))
                .AttachSessionAlertManager(new ConsoleAlertManager())
                .AttachSessionLogger(new ConsoleLogger(showInfrastructure: false))
                .AttachSessionReportGenerator(new ConsoleReportGenerator())
                .WithSessionStopStrategy(new LinearProcessingFinishedStopStrategy())
                .RunAsync();
        }
    }
}