using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Prowl;
using ServerShot.Framework.Core.Plugins.Alerts;
using Servershot.Framework.Entities.WebJob;

namespace ServerShot.Framework.Core.Plugins
{
    public class ProwlAlertManager : WebJobSessionPluginBase
    {
        public INotifier Notifier { get; set; }

        public override void RegisterWebjob(IJibJobModule module)
        {
            base.RegisterWebjob(module);

            module.OnFail += () => ModuleOnFail(module);
            module.OnAlert += ModuleOnAlert;
        }

        private void ModuleOnAlert(Alert obj)
        {
            Notifier.Alert(null, obj.Message, level: obj.AlertLevel);
        }

        private void ModuleOnFail(IJibJobModule module)
        {
            Notifier.Alert(null, "Module Failed : " + module.Name, level: AlertLevel.High);
        }
    }

    public class InformMeWhenProcessedPlugin : WebJobSessionPluginBase
    {
        public List<Type> NotifierTypes = new List<Type>();

        internal Dictionary<Type, Action<dynamic>> _onCreate = new Dictionary<Type, Action<dynamic>>(); 

        public override void RegisterWebjob(IJibJobModule module)
        {
            base.RegisterWebjob(module);

            if (module is ITransientJibJob)
            {
                (module as ITransientJibJob).OnProcessed += o => Notify(module as ITransientJibJob);
            }
        }

        private void Notify(ITransientJibJob transientJibJob)
        {
            foreach (var notifierType in NotifierTypes)
            {
                var notifier = IOC.Kernel.Get<INotifier>(notifierType);
                _onCreate[notifierType](notifier);
                notifier.Alert(transientJibJob.Name, string.Format("{0} Processed new item successfully", transientJibJob.Name));
            }
        }

        public void AlertMeWith<T>(Action<T> onCreate) where T : INotifier
        {
            NotifierTypes.Add(typeof (T));

            _onCreate.Add(typeof(T), dynamicType => onCreate(dynamicType));
        }
    }

    public interface INotifier
    {
        void Alert(string title, string message, AlertLevel level = AlertLevel.High);
    }

    public class SendGridEmailProvider : INotifier
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public void Alert(string title, string message, AlertLevel level = AlertLevel.High)
        {
            
        }
    }

    public class ProwlNotifier : INotifier
    {
        /// <summary>
        /// 
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Only required if you have been whitelisted
        /// </summary>
        public string ProviderKey { get; set; }

        public string ApplicationName { get; set; }

        public void Alert(string title, string message, AlertLevel level = AlertLevel.Medium)
        {
            var client = new ProwlClient(new ProwlClientConfiguration()
            {
                ApiKeychain = ApiKey,
                ProviderKey = ProviderKey,
                ApplicationName = ApplicationName
            });

            client.PostNotification(new ProwlNotification()
            {
                Event = title,
                Description = message,
                Priority = level == AlertLevel.Low ? ProwlNotificationPriority.VeryLow : ProwlNotificationPriority.Normal
            });
        }
    }
}