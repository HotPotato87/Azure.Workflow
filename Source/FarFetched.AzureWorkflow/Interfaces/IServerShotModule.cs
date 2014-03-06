using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.ServiceModel.PeerResolvers;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Enums;
using ServerShot.Framework.Core.Implementation;
using ServerShot.Framework.Core.Interfaces;
using ServerShot.Framework.Core.Plugins.Alerts;
using Servershot.Framework.EventHandlers;

namespace ServerShot.Framework.Core.Architecture
{
    public interface IServerShotModule : INotifyPropertyChanged
    {
        Task StartAsync();
        Task Stop();
        ModuleState State { get; }
        DateTime Started { get; }
        DateTime Ended { get; }
        ServerShotSessionBase Session { get; set; }
        ICloudQueue Queue { get; set; }
        string QueueName { get; }
        int ProcessedCount { get; }
        Dictionary<Type, int> SentToAudit { get; }
        ServerShotModuleSettings Settings { get; set; }

        event Action<string, string> OnLogMessage;
        event Action OnStarted;
        event Action<Alert> OnAlert;
        event Action<Exception> OnError;
        event Action<string, IEnumerable<Exception>> OnFailure;
        event Func<string, object, Task> OnStoreAsync;
        event Func<string, Task<object>> OnRetrieveAsync;
        event Func<string, object, Task> OnStoreEnumerableAsync;
        event Func<string, Task<IEnumerable<object>>> OnRetrieveEnumerableAsync; 
        event Action OnFinished;
        event Action<OnRaisedProcessedArgs> OnRaiseProcessed;
    }
}
