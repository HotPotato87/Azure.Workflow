using System;
using System.Threading.Tasks;
using Servershot.WebsiteOrderSample.Entities;

namespace Servershot.WebsiteOrderSample.Services
{
    public class SimulatedWarehouseApi : IWarehouseManagementApi
    {
        public double PercentageOrderCorrectlyPlaced { get; set; }
        public TimeSpan WarehousingDelay { get; set; }

        private Random _random = new Random();

        public SimulatedWarehouseApi()
        {
            PercentageOrderCorrectlyPlaced = 1;
            WarehousingDelay = TimeSpan.FromMilliseconds(20);
        }

        public async Task<WarehousingDetails> InformWarehouse(Order order)
        {
            //simulate the order taking a while
            await Task.Delay(WarehousingDelay);

            //update order.. maybe they can place it, maybe not!
            var result = new WarehousingDetails();
            result.AddedBy = GetUser();
            result.TimeAdded = DateTime.Now;
            result.WarehousingConfirmed = (OrderCorrectlyPlaced(order));

            return result;
        }

        private Person GetUser()
        {
            return new Person()
            {
                Name = "Pete"
            };
        }

        private bool OrderCorrectlyPlaced(Order order)
        {
            return (_random.Next(0, 100) < (PercentageOrderCorrectlyPlaced * 100));
        }
    }
}