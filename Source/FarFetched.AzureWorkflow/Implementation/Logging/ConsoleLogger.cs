using System;
using System.Diagnostics;
using Azure.Workflow.Core.Plugins.Alerts;

namespace Azure.Workflow.Core.Implementation.Logging
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
