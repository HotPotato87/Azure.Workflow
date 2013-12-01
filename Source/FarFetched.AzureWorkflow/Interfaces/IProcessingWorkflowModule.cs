using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Workflow.Core.Enums;

namespace Azure.Workflow.Core.Architecture
{
    public interface IProcessingWorkflowModule<T>
    {
        Task ProcessAsync(IEnumerable<T> queueCollection);
    }
}