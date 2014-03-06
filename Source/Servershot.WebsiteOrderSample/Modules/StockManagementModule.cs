using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core;
using ServerShot.Framework.Core.Enums;
using ServerShot.Framework.Core.Plugins.Alerts;
using Servershot.Framework.Entities.Module;
using Servershot.WebsiteOrderSample.Entities;
using Servershot.WebsiteOrderSample.Services;

namespace Servershot.WebsiteOrderSample.Modules
{
    public class StockManagementModule : QueueProcessingServerShotModule<Order>
    {
        private readonly IStockManagementApi _stockApi;

        public StockManagementModule(IStockManagementApi stockApi)
        {
            _stockApi = stockApi;
        }

        public async override Task ProcessAsync(IEnumerable<Order> incomingOrders)
        {
            foreach (var order in incomingOrders)
            {
                try
                {
                    var stockOrder = await _stockApi.UpdateStockAsync(order);
                    base.LogMessage("Stock updated for " + order.Id);

                    order.StockManagementDetails = stockOrder;

                    base.CategorizeResult(ProcessingResult.Success);

                    base.SendTo<WarehousingModule>(order);
                }
                catch (Exception e)
                {
                    base.RaiseError(e);
                    base.RaiseAlert(AlertLevel.High, string.Format("There was a problem with the stock ordering API : {0}", e.Message));
                    base.CategorizeResult("Stock API Problems");
                }
            }
        }
    }
}
