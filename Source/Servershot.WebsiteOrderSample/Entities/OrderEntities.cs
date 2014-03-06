using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servershot.WebsiteOrderSample.Entities
{
    public class Order
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public int CustomerId { get; set; }
        public string Status { get; set; }

        public List<Product> Products { get; set; } 

        public WarehousingDetails WarehousingDetails { get; set; }
        public StockManagementDetails StockManagementDetails { get; set; }

        public Order()
        {
            Id = Guid.NewGuid();
            Products = new List<Product>();
        }
    }

    public class WarehousingDetails
    {
        public int Id { get; set; }
        public DateTime TimeAdded { get; set; }
        public Person AddedBy { get; set; }
        public bool WarehousingConfirmed { get; set; }
    }

    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class StockManagementDetails
    {
        public int Id { get; set; }
        public int StockWas { get; set; }
        public int StockIs { get; set; }
        public DateTime Updated { get; set; }
    }
}
