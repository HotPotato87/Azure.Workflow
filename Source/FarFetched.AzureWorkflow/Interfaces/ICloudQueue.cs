using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core.Architecture;

namespace FarFetched.AzureWorkflow.Core.Interfaces
{
    public interface ICloudQueueFactory
    {
        ICloudQueue CreateQueue(IWorkflowModule module);
    }

    public interface ICloudQueue
    {
        Task<IEnumerable<T>> ReceieveAsync<T>(int batchCount);
        Task AddToAsync<T>(IEnumerable<T> items);
    }
}
