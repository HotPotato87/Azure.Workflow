using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core.Enums;

namespace FarFetched.AzureWorkflow.Core.Architecture
{
    public interface IProcessingWorkflowModule<T> : IProcessingWorkflowModule
    {
        Task ProcessAsync(IEnumerable<T> queueCollection);
    }

    public interface IProcessingWorkflowModule : IWorkflowModule
    {
        event Action<string, string> OnRaiseProcessed;
    }
}