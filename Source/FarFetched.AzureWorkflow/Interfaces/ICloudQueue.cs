using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Architecture;

namespace ServerShot.Framework.Core.Interfaces
{
    public interface ICloudQueueFactory
    {
        ICloudQueue CreateQueue(IServerShotModule module);
    }

    public interface ICloudQueue
    {
        Task<IEnumerable<T>> ReceieveAsync<T>(int batchCount);
        Task AddToAsync<T>(IEnumerable<T> items);
        int Count { get; }
        event Action<object> ObjectWrapperCreated;
    }
}
