using System;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Enums;
using ServerShot.Framework.Core.Plugins.Alerts;

namespace Servershot.Framework.Entities.WebJob
{
    public abstract class WebJobBase : IWebJobBase
    {
        public event Action<LogMessage> LogMessage;

        public event Action<Exception> Exception;

        public event Action<object> Processed;

        public event Action Fail;

        public event Action<Alert> Alert;

        public int ProcessedCount { get; protected set; }

        public string Name
        {
            get { return this.GetType().Name; }
        }

        public ModuleState State { get; set; }
        public DateTime Started { get; set; }
        protected bool ThrowOnError { get; set; }

        private int _errorCount = 0;

        protected WebJobBase()
        {
            ThrowOnError = true;
            Started = DateTime.UtcNow;
        }

        public async Task ProcessItem<T>(T item)
        {
            try
            {
                Log("Processing : " + item);
                await OnProcessItem(item);
                OnProcessed(item);
                Log("Finished Processing : " + item);
                _errorCount = 0;
                ProcessedCount++;
            }
            catch (Exception eX)
            {
                OnException(eX);
                Log("EXCEPTION OCCURED in " + this.GetType().Name);
                Log("Exception details : " + eX.Message);
                Log("Exception Stacktrace : " + eX.StackTrace);
                OnAlert(new Alert()
                {
                    AlertLevel = AlertLevel.High,
                    Message = "Exception occured on webjob:" + this.GetType().Name + " : " + eX.Message
                });

                _errorCount++;

                if (_errorCount >= 3)
                {
                    Log("Module failed : " + this.GetType().Name);
                    OnFail();
                }

                if (ThrowOnError)
                {
                    throw;    
                }
            }
            
        }

        public void Reset()
        {
            this.ProcessedCount = 0;
        }

        protected abstract Task OnProcessItem<T>(T item);

        protected void Log(string message)
        {
            if (LogMessage != null)
            {
                LogMessage(new LogMessage(message, null));    
            }
        }

        protected virtual void OnException(Exception obj)
        {
            Action<Exception> handler = Exception;
            if (handler != null) handler(obj);
        }

        protected virtual void OnFail()
        {
            Action handler = Fail;
            if (handler != null) handler();
        }

        protected virtual void OnAlert(Alert obj)
        {
            Action<Alert> handler = Alert;
            if (handler != null) handler(obj);
        }

        protected virtual void OnProcessed(object obj)
        {
            Action<object> handler = Processed;
            if (handler != null) handler(obj);
        }
    }
}
