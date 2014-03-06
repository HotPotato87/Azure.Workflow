using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using ServerShot.Framework.Core.Implementation;
using ServerShot.Framework.Core.Implementation.StopStrategy;

namespace Servershot.Framework.Entities
{
    public class ServerShotContinuousSession : ServerShotSessionBase
    {
        private Timer _timer;

        public event Action RequestReporting;

        public TimeSpan ReportingInterval { get; set; }

        public ServerShotContinuousSession()
        {
            this.Settings.NeverFail = true;
            ReportingInterval = TimeSpan.FromDays(1);

            StartReportingTiming();
        }

        private void StartReportingTiming()
        {
            _timer = new Timer();
            _timer.Elapsed += CallReporting;
            _timer.Interval = ReportingInterval.TotalMilliseconds;
            _timer.Start();
        }

        private void CallReporting(object sender, ElapsedEventArgs e)
        {
            if (this.RequestReporting != null)
            {
                RequestReporting();
            }
        }

        public override Task Start()
        {
            this.StopStrategy = new ContinousProcessingStategy();

            return base.Start();
        }

        public override void Stop()
        {
            //do nothing
        }

        public static ServerShotSessionBaseBuilder StartBuild()
        {
            return new ServerShotSessionBaseBuilder(new ServerShotContinuousSession());
        }
    }
}
