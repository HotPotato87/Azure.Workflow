using System.Diagnostics;
using FarFetched.AzureWorkflow.Core.Plugins.Alerts;

namespace FarFetched.AzureWorkflow.Core.Implementation.Logging
{
    public class ConsoleLogger : LogManagerBase
    {
        public override void OnLogMessage(LogMessage message)
        {
            Debug.WriteLine("{0} : {1}", message.DateTime, message.Message);
        }
    }
}
