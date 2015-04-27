using System;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Enums;
using ServerShot.Framework.Core.Plugins.Alerts;

namespace Servershot.Framework.Entities.WebJob
{
    public interface IWebJobBase
    {
        string Name { get; }
        event Action<LogMessage> LogMessage;
        event Action<Exception> Exception;
        event Action<object> Processed;
        event Action Fail;
        event Action<Alert> Alert;
        int ProcessedCount { get; }
        ModuleState State { get; set; }
        DateTime Started { get; set; }
        Task ProcessItem<T>(T item);
        void Reset();
    }
}