using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FarFetched.AzureWorkflow.Core.Architecture
{
    public interface IProcessingWorkflowModule<T> : IProcessingWorkflowModule
    {
        Task ProcessAsync(IEnumerable<T> queueCollection);
    }

    public interface IProcessingWorkflowModule : IWorkflowModule
    {
        event Action<object> OnProcessed;
    }
}