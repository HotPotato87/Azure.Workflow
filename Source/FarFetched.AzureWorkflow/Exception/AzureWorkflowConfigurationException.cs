using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Workflow.Core
{
    public class AzureWorkflowConfigurationException : Exception
    {
        public AzureWorkflowConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
            
        }
    }
}
