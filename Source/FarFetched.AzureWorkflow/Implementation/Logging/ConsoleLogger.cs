using System;
using System.Collections.Generic;
using System.Diagnostics;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Plugins.Alerts;

namespace ServerShot.Framework.Core.Implementation.Logging
{
    public class ConsoleLogger : LogManagerBase
    {
        private readonly bool _showInfrastructure;

        public override void OnLogMessage(LogMessage message, IServerShotModule module = null)
        {
            if (!_showInfrastructure && message.Category.Contains("Infrastructure")) return;

            Console.WriteLine("{0}({2}) : {1}", message.DateTime, message.Message, message.Category);
        }

        public ConsoleLogger(bool showInfrastructure = true)
        {
            _showInfrastructure = showInfrastructure;
        }
    }

    public class AzureStorageLogger : LogManagerBase
    {
        private readonly List<string> _acceptedCategories;

        public AzureStorageLogger(List<string> acceptedCategories = null)
        {
            _acceptedCategories = acceptedCategories;
        }

        public override void OnLogMessage(LogMessage message, IServerShotModule module = null)
        {
            //TODO : Write to azure table log
            throw new NotImplementedException();
        }
    }

    public class DebugLogger : LogManagerBase
    {
        public override void OnLogMessage(LogMessage message, IServerShotModule module = null)
        {
            Debug.WriteLine("{0}({2}) : {1}", message.DateTime, message.Message, message.Category);
        }
    }
}
