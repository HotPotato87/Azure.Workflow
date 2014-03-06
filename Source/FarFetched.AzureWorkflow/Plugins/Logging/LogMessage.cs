using System;

namespace ServerShot.Framework.Core.Plugins.Alerts
{
    public class LogMessage
    {
        public DateTime DateTime { get; set; }
        public string Message { get; set; }
        public string Category { get; set; }

        public LogMessage(string message, string category)
        {
            this.DateTime = DateTime.Now;
            this.Message = message;
            Category = category;
        }
    }
}