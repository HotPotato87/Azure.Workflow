using System;
using System.Globalization;
using Servershot.Framework.Entities.WebJob;

namespace ServerShot.Framework.Core.Plugins
{
    public abstract class NewActivityAlertPlugin : WebJobSessionPluginBase
    {
        private readonly TimeSpan _threshold;
        private DateTime? _lastActivity;

        public NewActivityAlertPlugin(TimeSpan threshold)
        {
            _threshold = threshold;
        }

        public override void RegisterWebjob(IJibJobModule module)
        {
            base.RegisterWebjob(module);

            module.Processed += o =>
            {
                if (!_lastActivity.HasValue || NewActivity())
                {
                    OnNewActivity();
                }

                _lastActivity = DateTime.Now;
            };
        }

        protected abstract void OnNewActivity();

        private bool NewActivity()
        {
            return _lastActivity.HasValue && DateTime.Now.Subtract(_lastActivity.Value) > _threshold;
        }
    }

    public class ProwlNewActivityPlugin : NewActivityAlertPlugin
    {
        public ProwlNewActivityPlugin(TimeSpan threshold) : base(threshold)
        {

        }

        protected override void OnNewActivity()
        {
            //todo : prowl alert for new activity on 
            var name = this.Module.Name;
        }
    }
}