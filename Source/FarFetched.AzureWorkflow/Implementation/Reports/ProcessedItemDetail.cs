using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarFetched.AzureWorkflow.Core.Implementation.Reports
{
    public class ProcessedItemDetail
    {
        public DateTime Processed { get; set; }
        public string Message { get; set; }

        public ProcessedItemDetail(string message)
        {
            Processed = DateTime.Now;
            this.Message = message;
        }
    }
}
