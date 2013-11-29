using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core.Architecture;
using FarFetched.AzureWorkflow.Core.Enums;
using FarFetched.AzureWorkflow.Core.Implementation;
using FarFetched.AzureWorkflow.Core.Plugins.Alerts;

namespace FarFetched.AzureWorkflow.Core
{
    public abstract class QueueProcessingWorkflowModule<T> : WorkflowModuleBase<T>, IProcessingWorkflowModule where T : class
    {
        public event Action<string, string> OnRaiseProcessed;
        public event Action OnProcessIteration;

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

        protected void IncreaseProcessIteration()
        {
            if (this.OnProcessIteration != null) this.OnProcessIteration();
        }

        protected void RaiseProcessed(object key, string description = null, bool countAsProcessed = true)
        {
            if (this.OnRaiseProcessed != null)
            {
                this.OnRaiseProcessed(key.ToString(), description);
            }

            if (countAsProcessed) IncreaseProcessIteration();
        }

        protected void RaiseProcessed(ProcessingResult result, string description = null, bool countAsProcessed = true)
        {
            this.RaiseProcessed((object)result, description, countAsProcessed);
        }

        public abstract Task ProcessAsync(IEnumerable<T> queueCollection);
    }
}