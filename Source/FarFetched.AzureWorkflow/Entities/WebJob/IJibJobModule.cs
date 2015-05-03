using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Enums;
using ServerShot.Framework.Core.Plugins;
using ServerShot.Framework.Core.Plugins.Alerts;

namespace Servershot.Framework.Entities.WebJob
{
    public interface IJibJobModule
    {
        string Name { get; }
        event Action<LogMessage> OnLogMessage;
        event Action<Exception> OnException;
        event Action<object> OnProcessed;
        event Action OnFail;
        event Action<Alert> OnAlert;
        int ProcessedCount { get; }
        ModuleState State { get; set; }
        DateTime Started { get; set; }
        void Reset();
        void AttachPlugins(List<IJibJobSessionPlugin> plugins);
    }

    public interface ITransientJibJob : IJibJobModule
    {
        Task Triggered();
    }

    public interface IPerpetualJibJob : IJibJobModule
    {
        Task ProcessItem<T>(T item);
    }
}