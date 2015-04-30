using System.Timers;
using ServerShot.Framework.Core.Annotations;
using Servershot.Framework.Entities.WebJob;
using Timer = System.Timers.Timer;

namespace ServerShot.Framework.Core.Plugins
{
    public abstract class DailySummaryReporterPlugin : WebJobSessionPluginBase
    {
        public override void RegisterWebjob(IJibJobModule module)
        {
            base.RegisterWebjob(module);

            var timer = new Timer();
            timer.Elapsed += TimerOnElapsed;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            this.Report();

            base.Module.Reset();
        }

        public abstract void Report();
    }

    public class SendgridReport : DailySummaryReporterPlugin
    {
        public override void Report()
        {
            var name = base.Module.Name;
            var onlineSince = base.Module.Started;
            var processedCount = base.Module.ProcessedCount;

            var subject = string.Format("Webjob Report : {0} ({1})", name, processedCount);
            var text =
                string.Format("Online since: {0}\r\n", onlineSince) +
                string.Format("Processed : {0}\r\n", processedCount);

            Email(subject, text);
        }

        private void Email(string subject, string text)
        {
            //todo : Sendgrid email
        }
    }
}