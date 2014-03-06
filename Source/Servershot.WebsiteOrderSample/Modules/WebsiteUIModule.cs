using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core;
using ServerShot.Framework.Core.Enums;
using Servershot.WebsiteOrderSample.Entities;

namespace Servershot.WebsiteOrderSample.Modules
{
    public class WebsiteUIModule : InitialServerShotModule<Order>
    {
        public int OrdersToPlace { get; set; }

        public WebsiteUIModule()
        {
            OrdersToPlace = 100;
        }

        public async override Task OnStart()
        {
            for (int i = 0; i < OrdersToPlace; i++)
            {
                PlaceOrder(5);
            }
        }

        public void PlaceOrder(int productNumber)
        {
            var newOrder = new Order()
            {
                Name = "New Order",
                Status = "New"
            };

            for (int i = 0; i < productNumber; i++)
            {
                newOrder.Products.Add(GetProductFromId(productNumber));
            }

            base.LogMessage(string.Format("Order #{0} placed", newOrder.Id));
            base.CategorizeResult(ProcessingResult.Success);
            base.SendTo<StockManagementModule>(newOrder);
        }

        private Product GetProductFromId(int id)
        {
            return new Product() {Id = id, Name = "TV"};
        }
    }
}
