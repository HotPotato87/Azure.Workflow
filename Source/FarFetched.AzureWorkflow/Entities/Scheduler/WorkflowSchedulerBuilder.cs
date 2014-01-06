using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Workflow.Core.Entities.Scheduler
{
    public class WorkflowSchedulerBuilder
    {
        public WorkflowScheduler Scheduler { get; set; }

        public WorkflowSchedulerBuilder(WorkflowScheduler scheduler)
        {
            Scheduler = scheduler;
        }
    }

    public class WorkflowSchedulerWithSessionBuilder : WorkflowSchedulerBuilder
    {
        public WorkflowSchedulerWithSessionBuilder(WorkflowScheduler scheduler) : base(scheduler)
        {

        }
    }
}
