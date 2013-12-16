using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azure.Workflow.Core.Architecture;
using Azure.Workflow.Core.Enums;
using Azure.Workflow.Core.Implementation;
using Azure.Workflow.Core.Interfaces;
using Azure.Workflow.Core.Plugins.Alerts;
using Azure.Workflow.Core.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Azure.Workflow.Core
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
        public event Func<string, object, Task> OnStoreAsync;
        public event Func<string, Task<object>> OnRetrieveAsync;
        public event Action OnFinished;
        public event Action<string> OnLogMessage;
        public event Action<Alert> OnAlert;
        public event Action OnStarted;
        public event Action<string, string, bool> OnRaiseProcessed; //todo : eventhandler
        public event Action<string, IEnumerable<Exception>> OnFailure;

        #endregion

        private List<Exception> _capturedErrors = new List<Exception>(); 

        protected WorkflowModuleBase( WorkflowModuleSettings settings = default(WorkflowModuleSettings))
        {
            this.State = ModuleState.Waiting;
            this.Settings = settings;
            if (settings == null) Settings = new WorkflowModuleSettings();
            HookInternalEvents();
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
                _capturedErrors.Add(e);
                RaiseError(e);
                RaiseFailure("Critical exception caught : " + e.Message);
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

        protected async Task StoreAsync<T>(string key, T obj)
        {
            if (this.OnStoreAsync != null)
            {
                await this.OnStoreAsync(key, obj);
            }
            else
            {
                this.RaiseFailure("A module attempted to store or retrieve a value, please attach a persistance component");
            }
        }

        protected async Task<T> RetrieveAsync(string key)
        {
            if (this.OnRetrieveAsync != null)
            {
               var result = this.OnRetrieveAsync(key);
               if (result != null)
               {
                   var resultValue = await result;
                   return (T) resultValue;
               }
            }
            else
            {
                this.RaiseFailure("A module attempted to store or retrieve a value, please attach a persistance component");
            }
            return null;
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

        private void RaiseFailure(string message = "Failure raised manually")
        {
            if (this.OnFailure != null)
            {
                this.OnFailure(message, _capturedErrors);
            }
        }

        protected void RaiseError(Exception e)
        {
            this.LogMessage("{0} : Error Occured {1}", this.QueueName, e.ToString());
            _capturedErrors.Add(e);
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

        #region

        private void HookInternalEvents()
        {
            this.OnError += HandleError;
        }

        private void HandleError(Exception exception)
        {
            if (this._capturedErrors.Count >= this.Settings.ThrowFailureAfterCapturedErrors)
            {
                RaiseFailure("Error threshold reached");
            }

            if (this.Settings.SendAlertOnCapturedError)
            {
                this.RaiseAlert(AlertLevel.Medium, "Error caught : " + exception.Message + " \r\n " + exception.StackTrace);
            }
        }

        #endregion
    }
}
