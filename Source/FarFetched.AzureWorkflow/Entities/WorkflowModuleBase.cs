using System;
using System.Collections.Generic;
using System.Linq;
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

        public WorkflowModuleBase( WorkflowModuleSettings settings = default(WorkflowModuleSettings))
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

    public abstract class QueueProcessingWorkflowModule<T> : WorkflowModuleBase<T>, IProcessingWorkflowModule where T : class
    {
        public event Action<object> OnProcessed;

        private int _waitIterations;

        public QueueProcessingWorkflowModule(WorkflowModuleSettings settings = default(WorkflowModuleSettings))
            :base(settings)
        {
            
        }

        public override async Task StartAsync()
        {
            IEnumerable<T> messages;

            base.Started = DateTime.Now;

            try
            {
                while ((messages = await this.Queue.ReceieveAsync<T>(this.Settings.QueueSettings.BatchCount)).Any())
                {
                    try
                    {
                        await this.ProcessAsync(messages);
                    }
                    catch (Exception processingError)
                    {
                        this.RaiseError(new Exception("There was an error processing the messages", processingError));
                        continue;
                    }
                }
            }
            catch (Exception eX)
            {
                base.RaiseError(new Exception("There was an error reading from the queue", eX));
            }

            //finished processing, invoke wait count to see if queue is clear
            await Task.Delay(Settings.QueueWaitTime);
            this._waitIterations++;
            if (_waitIterations >= Settings.MaximumWaitTimesBeforeQueueFinished)
            {
                return;
            }
            else
            {
                await StartAsync();
            }
        }

        public abstract Task ProcessAsync(IEnumerable<T> queueCollection);

        protected void RaiseProcessed(T item)
        {
            if (this.OnProcessed != null)
            {
                this.OnProcessed(item);
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
