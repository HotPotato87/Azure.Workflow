using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Servershot.Framework.Entities;
using Servershot.WebsiteOrderSample.Modules;
using Servershot.WebsiteOrderSample.Services;

namespace Servershot.WebsiteOrderSample.ExampleScenarios
{
    public class Scenario1 : IExampleScenario
    {
        public string Description
        {
            get { return "Sends a series of orders through the process. Stock Management will be updated, and the warehouse informed"; }
        }

        /// <summary>
        /// Most simple example
        /// 
        /// Once orders are placed through the website, the orders : 
        /// 
        /// 1. Sent to stock management for update. There should always be stock.. if there isn't send an alert
        /// 2. Sent to Warehousing to see if they can place the order. If they can't add it to the reporting
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            //start with building our environment, this allows us to use an platform recognised IOC container
            var environment = ServerShotEnvironment.BuildEnvironment()
                .WithIOCContainer(new NinjectIocContainer())
                    .RegisterType<IStockManagementApi, SimulatedStockManagement>()
                    .RegisterType<IWarehouseManagementApi, SimulatedWarehouseApi>()
                .Environment;

            //build the session fluently. We won't use servicebus for now and will communicate data between modules via memory
            await ServerShotSessionBase.StartBuildWithSession(environment.CreateLinearSession())
                        .AddModule<WebsiteUIModule>()
                        .AddModule<StockManagementModule>()
                        .AddModule<WarehousingModule>()
                    .AttachSessionQueueMechanism(new InMemoryQueueFactory())
                    .AttachSessionAlertManager(new ConsoleAlertManager())
                    .AttachSessionLogger(new ConsoleLogger(showInfrastructure: false))
                    .WithSessionStopStrategy(new LinearProcessingFinishedStopStrategy())
                .RunAsync();
        }
    }
}
