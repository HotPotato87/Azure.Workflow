using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core.Architecture;
using FarFetched.AzureWorkflow.Core.Enums;
using FarFetched.AzureWorkflow.Core.Implementation;
using FarFetched.AzureWorkflow.Core.Interfaces;
using FarFetched.AzureWorkflow.Core.Plugins.Alerts;
using FarFetched.AzureWorkflow.Core.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace FarFetched.AzureWorkflow.Core
{
    public abstract class WorkflowModuleBase<T> : IWorkflowModule where T : class
    {
        public ModuleState State { get; protected set; }
        public virtual string QueueName
        {
            get
            {
                return this.GetType().Name.ToLower();
            }
        }
        public WorkflowSession Session { get; set; }
        public DateTime Ended { get; protected set; }
        public ICloudQueue Queue { get; set; }
        public DateTime Started { get; protected set; }
        protected WorkflowModuleSettings Settings { get; set; }

        public event Action<Exception> OnError;
        public event Action OnFinished;
        public event Action<string> OnLogMessage;
        public event Action<Alert> OnAlert;

        protected WorkflowModuleBase( WorkflowModuleSettings settings = default(WorkflowModuleSettings))
        {
            this.Settings = settings;

            if (settings == null) Settings = new WorkflowModuleSettings();
        }

        public abstract Task StartAsync();

        protected void SendTo(Type workflowModuleType, T obj)
        {
            this.SendTo(workflowModuleType, new[] { obj });
        }

        protected void SendTo(Type workflowModuleType, IEnumerable<T> batch)
        {
            this.Session.AddToQueue(workflowModuleType, batch);
        }

        protected void RaiseError(Exception e)
        {
            if (this.OnError != null)
            {
                OnError(e);
            }
        }
        protected void RaiseFinished()
        {
            if (this.OnFinished != null)
            {
                this.OnFinished();
            }
        }
    }

    public abstract class InitialWorkflowModule<T> : WorkflowModuleBase<T> where T : class
    {
        public InitialWorkflowModule(WorkflowModuleSettings settings = default(WorkflowModuleSettings))
            :base(settings)
        {
            
        }

        public override async Task StartAsync()
        {
            this.Started = DateTime.Now;
            await this.StartInitial();
            this.Ended = DateTime.Now;
            base.State = ModuleState.Finished;
            this.RaiseFinished();
        }

        protected abstract Task StartInitial();
    }
}
