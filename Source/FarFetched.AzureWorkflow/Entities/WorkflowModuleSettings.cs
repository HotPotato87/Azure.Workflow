using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarFetched.AzureWorkflow.Core.Implementation
{
    public class WorkflowModuleSettings
    {
        public TimeSpan ReceiveTimeout { get; set; }

        public ServiceBusQueueSettings QueueSettings { get; set; }
        public int MaximumWaitTimesBeforeQueueFinished { get; set; }
        public TimeSpan QueueWaitTime { get; set; }

        public WorkflowModuleSettings()
        {
            this.ReceiveTimeout = new TimeSpan(0, 0, 1, 0);
            MaximumWaitTimesBeforeQueueFinished = 3;
            QueueWaitTime = new TimeSpan(0, 0, 2);
            QueueSettings = new ServiceBusQueueSettings();
        }
    }

    public class ServiceBusQueueSettings
    {
        public string AccountName { get; set; }
        public string Key { get; set; }
        public string AccountNamespace { get; set; }
        public int BatchCount { get; set; }

        public ServiceBusQueueSettings()
        {
            BatchCount = 5;
        }
    }
}
