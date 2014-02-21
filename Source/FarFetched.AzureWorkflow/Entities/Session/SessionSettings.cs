using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShot.Framework.Core.Entities
{
    public class SessionSettings
    {
        public SessionSettings()
        {
            ErrorThresholdBeforeFail = 1;
            CheckStopStrategyEvery = TimeSpan.FromSeconds(1);
        }

        public int ErrorThresholdBeforeFail { get; set; }
        public TimeSpan CheckStopStrategyEvery { get; set; }
    }
}
