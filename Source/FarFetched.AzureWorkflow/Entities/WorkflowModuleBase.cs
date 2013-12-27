using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Workflow.Core.Architecture;
using Azure.Workflow.Core.Enums;
using Azure.Workflow.Core.Implementation;
using Azure.Workflow.Core.Interfaces;
using Azure.Workflow.Core.Plugins.Alerts;

namespace Azure.Workflow.Core
{
    public abstract class WorkflowModuleBase<T> : IWorkflowModule where T : class
    {
        #region Properties

        protected WorkflowModuleSettings Settings { get; set; }
        public ModuleState State { get; protected set; }

        public virtual string QueueName
        {
            get { return GetType().Name.ToLower(); }
        }

        public WorkflowSession Session { get; set; }
        public DateTime Ended { get; protected set; }
        public ICloudQueue Queue { get; set; }
        public DateTime Started { get; protected set; }

        #endregion

        #region Events

        public event Action<Exception> OnError;
        public event Func<string, object, Task> OnStoreAsync;
        public event Func<string, Task<object>> OnRetrieveAsync;
        public event Action OnFinished;
        public event Action<string> OnLogMessage;
        public event Action<Alert> OnAlert;
        public event Action<string, string, bool> OnRaiseProcessed; //todo : eventhandler
        public event Action<string, IEnumerable<Exception>> OnFailure;
        public event Action OnStarted;

        #endregion

        private readonly List<Exception> _capturedErrors = new List<Exception>();

        protected WorkflowModuleBase(WorkflowModuleSettings settings = default(WorkflowModuleSettings))
        {
            State = ModuleState.Waiting;
            Settings = settings;
            if (settings == null) Settings = new WorkflowModuleSettings();
            HookInternalEvents();
        }

        public async Task StartAsync()
        {
            //register start
            Started = DateTime.Now;
            State = ModuleState.Processing;
            if (OnStarted != null) OnStarted();
            LogMessage("Started");

            try
            {
                await OnStart();
            }
            catch (Exception e)
            {
                RaiseError(e);
                RaiseFailure("Critical exception caught : " + e.Message, false);
                LogMessage("Error raised : " + e);
                State = ModuleState.Error;
            }

            //register finish
            if (State != ModuleState.Error)
            {
                State = ModuleState.Finished;
            }
            LogMessage("Finished");
            Ended = DateTime.Now;
            if (OnFinished != null) OnFinished();
        }

        #region Abstract

        public abstract Task OnStart();

        #endregion

        #region Protected Methods

        protected void LogMessage(string message, params object[] parameters)
        {
            if (OnLogMessage != null)
            {
                OnLogMessage(string.Format(QueueName + " : " + message, parameters));
            }
        }

        protected async Task StoreAsync<T>(string key, T obj)
        {
            if (OnStoreAsync != null)
            {
                await OnStoreAsync(key, obj);
            }
            else
            {
                RaiseFailure("A module attempted to store or retrieve a value, please attach a persistance component");
            }
        }

        protected async Task<V> RetrieveAsync<V>(string key)
        {
            if (OnRetrieveAsync != null)
            {
                Task<object> result = OnRetrieveAsync(key);
                if (result != null)
                {
                    object resultValue = await result;
                    return (V) resultValue;
                }
            }
            else
            {
                RaiseFailure("A module attempted to store or retrieve a value, please attach a persistance component");
            }
            return default(V);
        }

        protected void RaiseAlert(AlertLevel level, string message)
        {
            if (OnAlert != null)
            {
                OnAlert(new Alert {AlertLevel = level, Message = message});
            }
        }

        protected void CategorizeResult(object key, string description = null, bool countAsProcessed = true)
        {
            if (OnRaiseProcessed != null)
            {
                OnRaiseProcessed(key.ToString(), description, countAsProcessed);
            }
        }

        protected void CategorizeResult(ProcessingResult result, string description = null, bool countAsProcessed = true)
        {
            CategorizeResult((object) result, description, countAsProcessed);
        }

        protected void SendTo(Type workflowModuleType, T obj)
        {
            SendTo(workflowModuleType, new[] {obj});
        }

        protected void SendTo(Type workflowModuleType, IEnumerable<T> batch)
        {
            Session.AddToQueue(workflowModuleType, batch);
        }

        private void RaiseFailure(string message = "Failure raised manually", bool addAsError = true)
        {
            if (OnFailure != null)
            {
                if (addAsError)
                {
                    _capturedErrors.Add(new Exception(message));
                }
                OnFailure(message, _capturedErrors);
            }
        }

        protected void RaiseError(Exception e)
        {
            LogMessage("{0} : Error Occured {1}", QueueName, e.ToString());
            _capturedErrors.Add(e);
            if (OnError != null)
            {
                OnError(e);
            }
        }

        #endregion

        #region

        private void HookInternalEvents()
        {
            OnError += HandleError;
        }

        private void HandleError(Exception exception)
        {
            if (_capturedErrors.Count >= Settings.ThrowFailureAfterCapturedErrors)
            {
                RaiseFailure("Error threshold reached");
            }

            if (Settings.SendAlertOnCapturedError)
            {
                RaiseAlert(AlertLevel.Medium, "Error caught : " + exception.Message + " \r\n " + exception.StackTrace);
            }
        }

        #endregion
    }
}