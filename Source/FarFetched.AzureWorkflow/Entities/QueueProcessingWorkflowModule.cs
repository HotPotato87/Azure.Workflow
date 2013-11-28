using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core.Architecture;
using FarFetched.AzureWorkflow.Core.Enums;
using FarFetched.AzureWorkflow.Core.Implementation;
using FarFetched.AzureWorkflow.Core.Plugins.Alerts;

namespace FarFetched.AzureWorkflow.Core
{
    public abstract class QueueProcessingWorkflowModule<T> : WorkflowModuleBase<T>, IProcessingWorkflowModule where T : class
    {
        public event Action<object> OnProcessed;

        private int _waitIterations;

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
            this.State = ModuleState.Waiting;
            await Task.Delay(Settings.QueueWaitTime);
            this._waitIterations++;
            if (_waitIterations >= Settings.MaximumWaitTimesBeforeQueueFinished)
            {
                return;
            }
            else
            {
                this.State = ModuleState.Waiting;
                await StartAsync();
            }
        }

        protected void RaiseSuccessfullyProcessed(T item)
        {
            if (this.OnProcessed != null)
            {
                this.OnProcessed(item);
            }
        }

        public abstract Task ProcessAsync(IEnumerable<T> queueCollection);
    }
}