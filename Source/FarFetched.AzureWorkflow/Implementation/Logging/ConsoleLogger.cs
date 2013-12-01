using System;
using System.Diagnostics;
using FarFetched.AzureWorkflow.Core.Plugins.Alerts;

namespace FarFetched.AzureWorkflow.Core.Implementation.Logging
{
    public class ConsoleLogger : LogManagerBase
    {
        public override void OnLogMessage(LogMessage message)
        {
            Console.WriteLine("{0} : {1}", message.DateTime, message.Message);
        }
    }

    public class AzureStorageLogger : LogManagerBase
    {
        public override void OnLogMessage(LogMessage message)
        {
            throw new NotImplementedException();
        }
    }

    public class DebugLogger : LogManagerBase
    {
        public override void OnLogMessage(LogMessage message)
        {
            Debug.WriteLine("{0} : {1}", message.DateTime, message.Message);
        }
    }
}
