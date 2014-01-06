using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Workflow.Core.Architecture;
using Azure.Workflow.Core.Implementation;

namespace Azure.Workflow.Core.Interfaces
{
    public interface IProcessingStopStrategy
    {
        bool ShouldStop(WorkflowSession session);
        bool ShouldSpecificModuleStop(IWorkflowModule module);
    }
}
