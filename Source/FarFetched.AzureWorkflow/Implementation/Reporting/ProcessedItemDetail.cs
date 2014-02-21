using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShot.Framework.Core.Implementation.Reporting
{
    public class ProcessedItemDetail
    {
        public DateTime ProcessedTime { get; set; }
        public string Message { get; set; }

        public ProcessedItemDetail(string message)
        {
            ProcessedTime = DateTime.Now;
            this.Message = message;
        }
    }
}
