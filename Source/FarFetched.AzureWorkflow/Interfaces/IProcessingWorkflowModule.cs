using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core.Enums;

namespace FarFetched.AzureWorkflow.Core.Architecture
{
    public interface IProcessingWorkflowModule<T>
    {
        Task ProcessAsync(IEnumerable<T> queueCollection);
    }
}