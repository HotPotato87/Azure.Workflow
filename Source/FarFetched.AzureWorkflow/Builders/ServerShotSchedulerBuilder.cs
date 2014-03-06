using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShot.Framework.Core.Entities.Scheduler
{
    public class ServerShotSchedulerBuilder
    {
        public WorkflowScheduler Scheduler { get; set; }

        public ServerShotSchedulerBuilder(WorkflowScheduler scheduler)
        {
            Scheduler = scheduler;
        }
    }
}
