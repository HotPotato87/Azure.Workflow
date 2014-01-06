using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azure.Workflow.Core.Architecture
{
    public interface IQueueProcessingWorkflowModule : IWorkflowModule
    {
        void Stop();
        DateTime LastRecieved { get; }
        int EmptyQueueIterations { get; }
        bool IsRecievedItems { get; set; }
    }

    public interface IQueueProcessingWorkflowModule<T> : IQueueProcessingWorkflowModule
    {
        Task ProcessAsync(IEnumerable<T> queueCollection);
    }
}