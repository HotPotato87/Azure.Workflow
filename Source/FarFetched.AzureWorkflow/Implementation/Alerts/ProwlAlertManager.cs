using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core.Interfaces;
using FarFetched.AzureWorkflow.Core.Plugins.Alerts;

namespace FarFetched.AzureWorkflow.Core.Implementation
{
    public class ProwlAlertManager : AlertManagerBase
    {
        public override void FireAlert(Alert alert)
        {
            
        }
    }

    public class SendGridAlertManager : AlertManagerBase
    {
        public override void FireAlert(Alert alert)
        {

        }
    }
}
