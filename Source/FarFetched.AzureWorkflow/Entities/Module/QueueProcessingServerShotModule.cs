using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Enums;
using ServerShot.Framework.Core.Implementation;
using ServerShot.Framework.Core.Plugins.Alerts;
using Servershot.Framework.Enums;

namespace ServerShot.Framework.Core
{
    public abstract class QueueProcessingServerShotModule<T> : ServerShotModuleBase<T>, IQueueProcessingServerShotModule<T> where T : class
    {
        public DateTime LastRecieved { get; private set; }
        public int EmptyQueueIterations { get; private set; }
        public bool IsRecievedItems { get; set; }

        private bool _running = true;
        internal int _recievedLimit = int.MaxValue;

        public QueueProcessingServerShotModule()
        {
            IsRecievedItems = false;

        }

        protected QueueProcessingServerShotModule(ServerShotModuleSettings settings = default(ServerShotModuleSettings))
            :base(settings)
        {
            this.OnStarted += () =>
            {
                base.Queue.ObjectWrapperCreated += OnQueueMessageCreated;
            };
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
            this.LogMessage("{0} : Finished Processing", LoggingType.HighImportance, this.QueueName);
            this.LogMessage("Total of {0} messages processed", LoggingType.HighImportance, LoggingType.Infrastructure, ProcessedCount);
        }

        public async override Task OnStop()
        {
            _running = false;
        }

        internal virtual async Task ProcessQueue()
        {
            IEnumerable<T> messages;
            while ((messages = await this.Queue.ReceieveAsync<T>(this.Settings.QueueSettings.BatchCount)).Any())
            {
                EmptyQueueIterations = 0;
                IsRecievedItems = true;
                this.LastRecieved = DateTime.Now;
                this.LogMessage("Dequeued {0} messages", LoggingType.Infrastructure, messages.Count());
                try
                {
                    await this.ProcessAsync(messages);
                    this.LogMessage("Processed {0} messages", LoggingType.Infrastructure, messages.Count());
                }
                catch (Exception processingError)
                {
                    this.RaiseError(new Exception("There was an error processing the messages", processingError));
                    return;
                }
            }

            await Task.Delay(Settings.DelayIfNoQueueItems);
        }

        public abstract Task ProcessAsync(IEnumerable<T> incomingOrders);

        public async void Stop()
        {
            await this.OnStop();
            this.State = ModuleState.Finished;
        }

        protected override void CategorizeResult(ProcessingResult result, string description = null, CategorizationLevel level = CategorizationLevel.Module, bool countAsProcessed = true)
        {
            this.LastRecieved = DateTime.Now;

            base.CategorizeResult(result, description, level, countAsProcessed);
        }

        protected override void CategorizeResult(object key, string description = null, CategorizationLevel level = CategorizationLevel.Module, bool countAsProcessed = true)
        {
            this.LastRecieved = DateTime.Now;

            base.CategorizeResult(key, description, level, countAsProcessed);
        }

        protected override void SendTo<E>(T obj)
        {
            LastRecieved = DateTime.Now;

            base.SendTo<E>(obj);
        }

        /// <summary>
        /// Called everytime the queue creates a new message. This is the last chance the message can be manipulated before it gets sent through the queueing technology----.
        /// </summary>
        /// <param name="queueMessage"></param>
        protected virtual void OnQueueMessageCreated(object queueMessage)
        {
            //nothing
        }
    }

    
}

