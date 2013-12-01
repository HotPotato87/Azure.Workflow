using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Azure.Workflow.Core.Architecture;
using Azure.Workflow.Core.Enums;
using Azure.Workflow.Core.Implementation;
using Azure.Workflow.Core.Plugins.Alerts;

namespace Azure.Workflow.Core
{
    public abstract class QueueProcessingWorkflowModule<T> : WorkflowModuleBase<T>, IProcessingWorkflowModule<T> where T : class
    {
        private int _waitIterations;
        private int _processedCount;

        public QueueProcessingWorkflowModule()
        {
            
        }

        protected QueueProcessingWorkflowModule(WorkflowModuleSettings settings = default(WorkflowModuleSettings))
            :base(settings)
        {
            
        }

        public override async Task OnStart()
        {
            try
            {
                await ProcessQueue();

                this.LogMessage("{0} : Finished Processing", this.QueueName);
            }
            catch (Exception eX)
            {
                base.RaiseError(new Exception("There was an error reading from the queue", eX));
            }

            //finished processing, invoke wait count to see if queue is clear
            this.State = ModuleState.Waiting;
            await Task.Delay(Settings.QueueWaitTimeBeforeFinish);
            this._waitIterations++;
            if (_waitIterations >= Settings.MaximumWaitTimesBeforeQueueFinished)
            {
                return;
            }
            else
            {
                this.State = ModuleState.Waiting;
                await ProcessQueue();
            }

            this.LogMessage("Total of {0} messages processed", _processedCount);
        }

        private async Task ProcessQueue()
        {
            IEnumerable<T> messages;
            while ((messages = await this.Queue.ReceieveAsync<T>(this.Settings.QueueSettings.BatchCount)).Any())
            {
                this.LogMessage("Dequeued {0} messages", messages.Count());
                _processedCount += messages.Count();
                try
                {
                    await this.ProcessAsync(messages);
                    this.LogMessage("Processed {0} messages", messages.Count());
                }
                catch (Exception processingError)
                {
                    this.RaiseError(new Exception("There was an error processing the messages", processingError));
                    continue;
                }
            }
        }

        public abstract Task ProcessAsync(IEnumerable<T> queueCollection);
    }
}