using System;
using System.Threading.Tasks;
using Servershot.WebsiteOrderSample.Entities;

namespace Servershot.WebsiteOrderSample.Services
{
    /// <summary>
    /// Very naive simulation of what a stock management API might return
    /// </summary>
    public class SimulatedStockManagement : IStockManagementApi
    {
        public TimeSpan StockManagementDelay { get; set; }

        public SimulatedStockManagement()
        {
            StockManagementDelay = TimeSpan.FromMilliseconds(200);
        }

        public async Task<StockManagementDetails> UpdateStockAsync(Order order)
        {
            await Task.Delay(StockManagementDelay);

            if (IsStockAvailable(order))
            {
                return new StockManagementDetails()
                {
                    Id = 0,
                    StockIs = 5,
                    StockWas = 6,
                    Updated = DateTime.Now
                };
            }
            else
            {
                throw new Exception("There should be stock for this product. What's gone wrong!?");
            }
        }

        private bool IsStockAvailable(Order order)
        {
            return true;
        }
    }
}