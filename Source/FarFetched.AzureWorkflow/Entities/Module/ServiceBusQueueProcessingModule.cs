using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using ServerShot.Framework.Core;

namespace Servershot.Framework.Entities.Module
{
    public abstract class ServiceBusQueueProcessingModule<T> : QueueProcessingServerShotModule<T> where T : class
    {
        protected override void OnQueueMessageCreated(object o)
        {
            this.OnQueueMessageCreated(o as BrokeredMessage);
        }

        protected virtual void OnQueueMessageCreated(BrokeredMessage queueMessage)
        {
            
        }
    }
}
