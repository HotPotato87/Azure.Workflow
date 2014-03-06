using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core;
using ServerShot.Framework.Core.Enums;
using Servershot.WebsiteOrderSample.Entities;
using Servershot.WebsiteOrderSample.Services;

namespace Servershot.WebsiteOrderSample.Modules
{
    public class WarehousingModule : QueueProcessingServerShotModule<Order>
    {
        private readonly IWarehouseManagementApi _warehouseApi;

        public WarehousingModule(IWarehouseManagementApi warehouseApi)
        {
            _warehouseApi = warehouseApi;
        }

        public override async Task ProcessAsync(IEnumerable<Order> incomingOrders)
        {
            foreach (var order in incomingOrders)
            {
                var warehouseResponse = await _warehouseApi.InformWarehouse(order);
                order.WarehousingDetails = warehouseResponse;

                if (warehouseResponse.WarehousingConfirmed)
                {
                    base.LogMessage("Warehousing order#" + order.Id + " confirmed");
                    base.CategorizeResult(ProcessingResult.Success);
                }
                else
                {
                    base.LogMessage("ISSUE : order#" + order.Id + " NOT confirmed");
                    base.CategorizeResult("Warehouse not able to fulfil");
                }
            }
        }
    }
}
