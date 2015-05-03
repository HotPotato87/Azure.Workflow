using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using ServerShot.Framework.Core.Plugins.Alerts;
using Servershot.Framework.Entities.WebJob;

namespace ServerShot.Framework.Core.Plugins
{
    public class ProwlAlertManager : WebJobSessionPluginBase
    {
        public string ProwlUsername { get; set; }

        public override void RegisterWebjob(IJibJobModule module)
        {
            base.RegisterWebjob(module);

            module.OnFail += ModuleOnFail;
            module.OnAlert += ModuleOnAlert;
        }

        private void ModuleOnAlert(Alert obj)
        {
            //todo : send alert
        }

        private void ModuleOnFail()
        {
            //todo : send prowl message
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
                notifier.Alert(string.Format("{0} Processed new item successfully", transientJibJob.Name));
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
        void Alert(string message);
    }

    public class SendGridEmailProvider : INotifier
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public void Alert(string message)
        {
            
        }
    }

    public class ProwlNotifier : INotifier
    {
        public void Alert(string message)
        {
            throw new NotImplementedException();
        }

        public string Username { get; set; }
    }
}