using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShot.Framework.Core.Implementation
{
    public class ServerShotModuleSettings
    {
        public TimeSpan ReceiveTimeout { get; set; }
        public ServiceBusQueueSettings QueueSettings { get; set; }
        public TimeSpan QueuePollTime { get; set; }
        public int ThrowFailureAfterCapturedErrors { get; set; }
        public bool SendAlertOnCapturedError { get; set; }
        public TimeSpan DelayIfNoQueueItems { get; set; }

        public ServerShotModuleSettings()
        {
            this.ReceiveTimeout = new TimeSpan(0, 0, 1, 0);
            QueuePollTime = new TimeSpan(0, 0, 0, 0, 0);
            DelayIfNoQueueItems = new TimeSpan(hours: 0, minutes: 0, seconds: 5);
            QueueSettings = new ServiceBusQueueSettings();
            SendAlertOnCapturedError = true;
            ThrowFailureAfterCapturedErrors = 2;
        }
    }

    public class ServiceBusQueueSettings
    {
        public string ConnectionString { get; set; }
        public int BatchCount { get; set; }

        public ServiceBusQueueSettings()
        {
            BatchCount = 5;
        }
    }
}
