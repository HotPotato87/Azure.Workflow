using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ServerShot.Framework.Core.Annotations;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Enums;
using ServerShot.Framework.Core.Implementation;
using ServerShot.Framework.Core.Interfaces;
using ServerShot.Framework.Core.Plugins.Alerts;
using Servershot.Framework.Enums;
using Servershot.Framework.EventHandlers;

namespace ServerShot.Framework.Core
{
    public abstract class ServerShotModuleBase<T> : IServerShotModule where T : class
    {
        #region Properties

        public virtual ServerShotModuleSettings Settings { get; set; }

        public ModuleState State
        {
            get { return _state; }
            protected set
            {
                _state = value;
                OnPropertyChanged();
            }
        }

        public virtual string QueueName
        {
            get { return GetType().Name.ToLower(); }
        }

        public ServerShotSessionBase Session { get; set; }
        public DateTime Ended { get; protected set; }
        public ICloudQueue Queue { get; set; }
        public DateTime Started { get; protected set; }
        public int ProcessedCount { get; set; }

        public Dictionary<Type, int> SentToAudit { get; private set; }

        #endregion

        #region Events

        public event Action<Exception> OnError;
        public event Func<string, object, Task> OnStoreAsync;
        public event Func<string, Task<object>> OnRetrieveAsync;
        public event Func<string, object, Task> OnStoreEnumerableAsync;
        public event Func<string, Task<IEnumerable<object>>> OnRetrieveEnumerableAsync;
        public event Action OnFinished;
        public event Action<string, string> OnLogMessage;
        public event Action<Alert> OnAlert;
        public event Action<OnRaisedProcessedArgs> OnRaiseProcessed;
        public event Action<string, IEnumerable<Exception>> OnFailure;
        public event Action OnStarted;

        #endregion

        private readonly List<Exception> _capturedErrors = new List<Exception>();
        private ModuleState _state;

        protected ServerShotModuleBase(ServerShotModuleSettings settings = default(ServerShotModuleSettings))
        {
            State = ModuleState.Waiting;
            Settings = settings;
            SentToAudit = new Dictionary<Type, int>();
            if (settings == null) Settings = new ServerShotModuleSettings();
            HookInternalEvents();
        }

        public virtual async Task StartAsync()
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
            await Stop();
            if (OnFinished != null) OnFinished();
        }

        public virtual async Task Stop()
        {
            await OnStop();
            if (this.State != ModuleState.Error) this.State = ModuleState.Finished;
            LogMessage("Module now stopped", LoggingType.Infrastructure);
        }

        #region Abstract

        public async virtual Task OnStart()
        {
            this.LogMessage("Start called on : " + this.QueueName, LoggingType.Infrastructure);
        }

        public async virtual Task OnStop()
        {
            this.LogMessage("OnStop Called on " + this.QueueName, LoggingType.Infrastructure);
            this.State = ModuleState.Finished;
        }

        #endregion

        #region Protected Methods

        protected virtual void LogMessage(string message, string category = "", params object[] parameters)
        {
            if (OnLogMessage != null)
            {
                try
                {
                    OnLogMessage(string.Format(QueueName + " : " + message, parameters), category);
                }
                catch (Exception eX)
                {
                    OnLogMessage("Could not format message : " + message, LoggingType.ServerShotError.ToString());
                }
            }
        }

        protected virtual void LogMessage(string message, LoggingType category, params object[] parameters)
        {
            if (OnLogMessage != null)
            {
                try
                {
                    OnLogMessage(string.Format(QueueName + " : " + message, parameters), category.ToString());
                }
                catch (Exception eX)
                {
                    OnLogMessage("Could not format message : " + message, LoggingType.ServerShotError.ToString());
                }
            }
        }

        protected virtual async Task StoreAsync<T>(string key, T obj)
        {
            if (OnStoreAsync != null)
            {
                try
                {
                    await OnStoreAsync(key, obj);
                }
                catch (Exception e)
                {
                    this.RaiseError(e);
                    LogMessage(e.Message, LoggingType.ServerShotError.ToString());
                }
            }
            else
            {
                RaiseFailure("A module attempted to store or retrieve a value, please attach a persistance component");
            }
        }

        protected virtual async Task<V> RetrieveAsync<V>(string key)
        {
            if (OnRetrieveAsync != null)
            {
                Task<object> result = OnRetrieveAsync(key);
                if (result != null)
                {
                    object resultValue = await result;
                    return (V)resultValue;
                }
            }
            else
            {
                RaiseFailure("A module attempted to store or retrieve a value, please attach a persistance component");
            }
            return default(V);
        }

        protected virtual async Task StoreEnumerableAsync<TItem>(string table, TItem obj)
        {
            if (OnStoreEnumerableAsync != null)
            {
                await OnStoreEnumerableAsync(table, obj);
            }
            else
            {
                RaiseFailure("A module attempted to store or retrieve a value, please attach a persistance component");
            }
        }

        protected virtual async Task<IEnumerable<TItem>> RetrieveEnumerableAsync<TItem>(string table)
        {
            if (OnRetrieveEnumerableAsync != null)
            {
                var objects = await OnRetrieveEnumerableAsync(table);
                if (objects != null)
                {
                    try
                    {
                        return objects.Select(x => JsonConvert.DeserializeObject<TItem>(x.ToString())).ToList();
                    }
                    catch (Exception e)
                    {
                        this.LogMessage(e.Message);
                        return default(IEnumerable<TItem>);
                    }

                }
                else
                {
                    return default(IEnumerable<TItem>);
                }
            }
            else
            {
                RaiseFailure("A module attempted to store or retrieve a value, please attach a persistance component");
                return null;
            }
        }

        protected virtual void RaiseAlert(AlertLevel level, string message)
        {
            if (OnAlert != null)
            {
                OnAlert(new Alert { AlertLevel = level, Message = message });
            }
        }

        protected virtual void CategorizeResult(object key, string description = null, CategorizationLevel level = CategorizationLevel.Module, bool countAsProcessed = true)
        {
            if (OnRaiseProcessed != null)
            {
                OnRaiseProcessed(new OnRaisedProcessedArgs(key, description, level, countAsProcessed));
            }

            if (countAsProcessed)
            {
                ProcessedCount++;
            }
        }

        protected virtual void CategorizeResult(ProcessingResult result,  string description = null, CategorizationLevel level = CategorizationLevel.Module, bool countAsProcessed = true)
        {
            CategorizeResult((object)result, description, level, countAsProcessed);
        }

        protected virtual void SendTo<E>(T obj)
        {
            this.SendTo(typeof(E), obj);
        }

        protected virtual void SendTo(Type workflowModuleType, T obj)
        {
            SendTo(workflowModuleType, new[] { obj });
        }

        protected void SendTo(Type workflowModuleType, IEnumerable<T> batch)
        {
            //update audit
            if (!SentToAudit.ContainsKey(workflowModuleType))
            {
                SentToAudit[workflowModuleType] = 0;
            }
            SentToAudit[workflowModuleType]++;
            CategorizeResult(typeof(T).ToString(), countAsProcessed:false);
            if (Session != null)
            {
                //send item to workflow session for arbitration
                Session.AddToQueue(workflowModuleType, batch);
            }
        }

        protected void RaiseFailure(string message = "Failure raised manually", bool addAsError = true)
        {
            if (OnFailure != null)
            {
                this.LogMessage(string.Format("Failure raised on module {0} [{1}]", this.QueueName, message), LoggingType.Failure);
                if (addAsError)
                {
                    _capturedErrors.Add(new Exception(message));
                }
                OnFailure(message, _capturedErrors);
            }
        }

        protected virtual void RaiseError(Exception e)
        {
            LogMessage("{0} : Error Occured {1}", LoggingType.Error, QueueName, e.ToString());
            _capturedErrors.Add(e);
            if (OnError != null)
            {
                OnError(e);
            }
        }

        #endregion

        #region Helpers

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

        #region Property Notification

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}