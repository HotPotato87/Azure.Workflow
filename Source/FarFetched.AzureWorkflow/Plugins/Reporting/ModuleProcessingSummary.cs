using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core.Architecture;

namespace FarFetched.AzureWorkflow.Core.Interfaces
{
    public class ModuleProcessingSummary
    {
        public int SuccessfullyProcessed { get; set; }
        public IWorkflowModule Module { get; set; }
        public int Errors { get; set; }
        public List<Exception> ErrorList { get; set; }
        public TimeSpan Duration { get; set; }

        public ModuleProcessingSummary()
        {
            ErrorList = new List<Exception>();
        }
    }
}
