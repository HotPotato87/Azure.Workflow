using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Azure.Workflow.Core.Architecture;
using Azure.Workflow.Core.Enums;
using Azure.Workflow.Core.Implementation;
using Azure.Workflow.Core.Plugins.Alerts;

namespace Azure.Workflow.Core
{
    public abstract class QueueProcessingWorkflowModule<T> : WorkflowModuleBase<T>, IQueueProcessingWorkflowModule<T> where T : class
    {
        public DateTime LastRecieved { get; private set; }
        public int EmptyQueueIterations { get; private set; }
        public bool IsRecievedItems { get; set; }

        private bool _running = true;
        internal int _recievedLimit = int.MaxValue;

        public QueueProcessingWorkflowModule()
        {
            IsRecievedItems = false;

        }

        protected QueueProcessingWorkflowModule(WorkflowModuleSettings settings = default(WorkflowModuleSettings))
            :base(settings)
        {
            
        }

        public override async Task OnStart()
        {
            while (_running)
            {
                this.State = ModuleState.Processing;

                await ProcessQueue();

                this.State = ModuleState.Waiting;
                await Task.Delay(Settings.QueuePollTime);

                EmptyQueueIterations++;

                //allows testability
                if (EmptyQueueIterations > _recievedLimit) break;
            }

            this.State = ModuleState.Finished;
            this.LogMessage("{0} : Finished Processing", this.QueueName);
            this.LogMessage("Total of {0} messages processed", ProcessedCount);
        }

        public async override Task OnStop()
        {
            _running = false;
        }

        internal virtual async Task ProcessQueue()
        {
            this.LogMessage("{0} : Polling Queue", this.QueueName);
            IEnumerable<T> messages;
            while ((messages = await this.Queue.ReceieveAsync<T>(this.Settings.QueueSettings.BatchCount)).Any())
            {
                EmptyQueueIterations = 0;
                IsRecievedItems = true;
                this.LastRecieved = DateTime.Now;
                this.LogMessage("Dequeued {0} messages", messages.Count());
                try
                {
                    await this.ProcessAsync(messages);
                    this.LogMessage("Processed {0} messages", messages.Count());
                }
                catch (Exception processingError)
                {
                    this.RaiseError(new Exception("There was an error processing the messages", processingError));
                    return;
                }
            }

            this.LogMessage("{0} : No Items in Queue", this.QueueName);
        }

        public abstract Task ProcessAsync(IEnumerable<T> queueCollection);

        public async void Stop()
        {
            await this.OnStop();
            this.State = ModuleState.Finished;
        }
    }

    
}

