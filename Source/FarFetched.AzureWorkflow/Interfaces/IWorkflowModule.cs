using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.ServiceModel.PeerResolvers;
using System.Text;
using System.Threading.Tasks;
using Azure.Workflow.Core.Enums;
using Azure.Workflow.Core.Implementation;
using Azure.Workflow.Core.Interfaces;
using Azure.Workflow.Core.Plugins.Alerts;

namespace Azure.Workflow.Core.Architecture
{
    public interface IWorkflowModule
    {
        Task StartAsync();
        ModuleState State { get; }
        DateTime Started { get; }
        DateTime Ended { get; }
        WorkflowSession Session { get; set; }
        ICloudQueue Queue { get; set; }
        string QueueName { get; }
        int ProcessedCount { get; }
        Dictionary<Type, int> SentToAudit { get; } 

        event Action<string> OnLogMessage;
        event Action<Alert> OnAlert;
        event Action<Exception> OnError;
        event Action<string, IEnumerable<Exception>> OnFailure;
        event Func<string, object, Task> OnStoreAsync;
        event Func<string, Task<object>> OnRetrieveAsync; 
        event Action OnFinished;
        event Action<string, string, bool> OnRaiseProcessed;
    }
}
