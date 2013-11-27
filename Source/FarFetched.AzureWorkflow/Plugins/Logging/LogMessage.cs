using System;

namespace FarFetched.AzureWorkflow.Core.Plugins.Alerts
{
    public class LogMessage
    {
        public DateTime DateTime { get; set; }
        public string Message { get; set; }

        public LogMessage(string message)
        {
            this.DateTime = DateTime.Now;
            this.Message = message;
        }
    }
}