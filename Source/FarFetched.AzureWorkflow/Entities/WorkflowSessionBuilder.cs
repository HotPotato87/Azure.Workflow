using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Workflow.Core.Implementation
{
    public class WorkflowSessionBuilder
    {
        public WorkflowSession WorkflowSession { get; set; }

        public WorkflowSessionBuilder(WorkflowSession workflowSession)
        {
            WorkflowSession = workflowSession;
        }
    }
}
