using System;
using Azure.Workflow.Core.Plugins.Alerts;

namespace Azure.Workflow.Core.Implementation
{
    public class SendGridAlertManager : AlertManagerBase
    {
        public override void FireAlert(Alert alert)
        {
            throw new NotImplementedException();
        }
    }
}