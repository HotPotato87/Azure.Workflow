using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Workflow.Core.Implementation
{
    public class WorkflowModuleSettings
    {
        public TimeSpan ReceiveTimeout { get; set; }
        public ServiceBusQueueSettings QueueSettings { get; set; }
        public int MaximumWaitTimesBeforeQueueFinished { get; set; }
        public TimeSpan QueueWaitTimeBeforeFinish { get; set; }

        public WorkflowModuleSettings()
        {
            this.ReceiveTimeout = new TimeSpan(0, 0, 1, 0);
            MaximumWaitTimesBeforeQueueFinished = 3;
            QueueWaitTimeBeforeFinish = new TimeSpan(0, 0, 2);
            QueueSettings = new ServiceBusQueueSettings();
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
