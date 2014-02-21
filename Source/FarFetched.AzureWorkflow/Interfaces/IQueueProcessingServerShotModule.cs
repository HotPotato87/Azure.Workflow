using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerShot.Framework.Core.Architecture
{
    public interface IQueueProcessingServerShotModule : IServerShotModule
    {
        DateTime LastRecieved { get; }
        int EmptyQueueIterations { get; }
        bool IsRecievedItems { get; set; }
    }

    public interface IQueueProcessingServerShotModule<T> : IQueueProcessingServerShotModule
    {
        Task ProcessAsync(IEnumerable<T> queueCollection);
    }
}