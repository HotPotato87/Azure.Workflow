using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.PeerResolvers;
using System.Text;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core.Enums;
using FarFetched.AzureWorkflow.Core.Implementation;
using FarFetched.AzureWorkflow.Core.Interfaces;
using FarFetched.AzureWorkflow.Core.Plugins.Alerts;

namespace FarFetched.AzureWorkflow.Core.Architecture
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

        event Action<string> OnLogMessage;
        event Action<Alert> OnAlert;
        event Action<Exception> OnError;
        event Action OnFinished;
    }
}
