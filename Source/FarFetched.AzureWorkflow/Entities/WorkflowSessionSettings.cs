using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarFetched.AzureWorkflow.Core.Entities
{
    public class WorkflowSessionSettings
    {
        public int ProcessorCores { get; set; }

        public WorkflowSessionSettings()
        {
            ProcessorCores = Environment.ProcessorCount;
        }
    }
}
