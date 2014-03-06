using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Servershot.WebsiteOrderSample.Entities;

namespace Servershot.WebsiteOrderSample.Services
{
    public interface IStockManagementApi
    {
        Task<StockManagementDetails> UpdateStockAsync(Order order);
    }
}
