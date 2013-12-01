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
        #region Properties

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

        #endregion

        #region Events

        public event Action<Exception> OnError;
        public event Action OnFinished;
        public event Action<string> OnLogMessage;
        public event Action<Alert> OnAlert;
        public event Action OnStarted;
        public event Action<string, string, bool> OnRaiseProcessed; //todo : eventhandler

        #endregion

        protected WorkflowModuleBase( WorkflowModuleSettings settings = default(WorkflowModuleSettings))
        {
            this.State = ModuleState.Waiting;
            this.Settings = settings;
            if (settings == null) Settings = new WorkflowModuleSettings();
        }

        public async Task StartAsync()
        {
            //register start
            Started = DateTime.Now;
            this.State = ModuleState.Processing;
            if (OnStarted != null) OnStarted();
            this.LogMessage("Started");

            try
            {
                await OnStart();
            }
            catch (Exception e)
            {
                RaiseError(e);
                this.LogMessage("Error raised : " + e.ToString());
                this.State = ModuleState.Error;
            }

            //register finish
            if (this.State != ModuleState.Error)
            {
                this.State = ModuleState.Finished;
            }
            this.LogMessage("Finished");
            Ended = DateTime.Now;
            if (OnFinished != null) OnFinished();
        }

        #region Abstract

        public abstract Task OnStart();

        #endregion

        #region Protected Methods

        protected void LogMessage(string message, params object[] parameters)
        {
            if (this.OnLogMessage != null)
            {
                OnLogMessage(string.Format(this.QueueName + " : " + message, parameters));
            }
        }

        protected void RaiseAlert(AlertLevel level, string message)
        {
            if (this.OnAlert != null)
            {
                this.OnAlert(new Alert() { AlertLevel = level, Message = message});
            }
        }

        protected void CategorizeResult(object key, string description = null, bool countAsProcessed = true)
        {
            if (this.OnRaiseProcessed != null)
            {
                this.OnRaiseProcessed(key.ToString(), description, countAsProcessed);
            }
        }

        protected void CategorizeResult(ProcessingResult result, string description = null, bool countAsProcessed = true)
        {
            this.CategorizeResult((object)result, description, countAsProcessed);
        }

        protected void SendTo(Type workflowModuleType, T obj)
        {
            this.SendTo(workflowModuleType, new[] {obj});
        }

        protected void SendTo(Type workflowModuleType, IEnumerable<T> batch)
        {
            this.Session.AddToQueue(workflowModuleType, batch);
        }

        protected void RaiseError(Exception e)
        {
            this.LogMessage("{0} : Error Occured {1}", this.QueueName, e.ToString());
            if (this.OnError != null)
            {
                OnError(e);
            }
            else
            {
#if DEBUG
                throw e;
#endif
            }
        }

        #endregion
    }
}
