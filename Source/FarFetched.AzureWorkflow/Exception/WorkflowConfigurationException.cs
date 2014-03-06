using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShot.Framework.Core
{
    public class WorkflowConfigurationException : Exception
    {
        public WorkflowConfigurationException(string message, Exception innerException = null) : base(message, innerException)
        {
            
        }
    }
}
