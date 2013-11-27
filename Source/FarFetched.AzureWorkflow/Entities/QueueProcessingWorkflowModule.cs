using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core.Architecture;
using FarFetched.AzureWorkflow.Core.Implementation;

namespace FarFetched.AzureWorkflow.Core
{
    public abstract class QueueProcessingWorkflowModule<T> : WorkflowModuleBase<T>, IProcessingWorkflowModule where T : class
    {
        public event Action<object> OnProcessed;

        private int _waitIterations;

        protected QueueProcessingWorkflowModule(WorkflowModuleSettings settings = default(WorkflowModuleSettings))
            :base(settings)
        {
            
        }

        public override async Task StartAsync()
        {
            base.Started = DateTime.Now;

            try
            {
                IEnumerable<T> messages;
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
}