using System;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Enums;
using ServerShot.Framework.Core.Plugins.Alerts;

namespace Servershot.Framework.Entities.WebJob
{
    public abstract class JibJobModule : IJibJobModule
    {
        public event Action<LogMessage> OnLogMessage;

        public event Action<Exception> OnException;

        public event Action<object> OnProcessed;

        public event Action OnFail;

        public event Action<Alert> OnAlert;

        public int ProcessedCount { get; protected set; }

        public string Name
        {
            get { return this.GetType().Name; }
        }

        public ModuleState State { get; set; }
        public DateTime Started { get; set; }
        protected bool ThrowOnError { get; set; }

        protected JibJobModule()
        {
            ThrowOnError = true;
            Started = DateTime.UtcNow;
        }

        public void Reset()
        {
            this.ProcessedCount = 0;
        }

        protected void Log(string message)
        {
            if (OnLogMessage != null)
            {
                OnLogMessage(new LogMessage(message, null));    
            }
        }

        protected virtual void Exception(Exception obj)
        {
            Action<Exception> handler = OnException;
            if (handler != null) handler(obj);
        }

        protected virtual void Fail()
        {
            Action handler = OnFail;
            if (handler != null) handler();
        }

        protected virtual void Alert(Alert obj)
        {
            Action<Alert> handler = OnAlert;
            if (handler != null) handler(obj);
        }

        protected virtual void Processed(object obj)
        {
            Action<object> handler = OnProcessed;
            if (handler != null) handler(obj);
        }
    }
}
