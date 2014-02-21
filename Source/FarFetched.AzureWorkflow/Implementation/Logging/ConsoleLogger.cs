using System;
using System.Diagnostics;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Plugins.Alerts;

namespace ServerShot.Framework.Core.Implementation.Logging
{
    public class ConsoleLogger : LogManagerBase
    {
        public override void OnLogMessage(IServerShotModule module, LogMessage message)
        {
            Console.WriteLine("{0} : {1}", message.DateTime, message.Message);
        }
    }

    public class AzureStorageLogger : LogManagerBase
    {
        public override void OnLogMessage(IServerShotModule module, LogMessage message)
        {
            throw new NotImplementedException();
        }
    }

    public class DebugLogger : LogManagerBase
    {
        public override void OnLogMessage(IServerShotModule module, LogMessage message)
        {
            Debug.WriteLine("{0} : {1}", message.DateTime, message.Message);
        }
    }
}
