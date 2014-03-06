using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Interfaces;
using ServerShot.Framework.Core.Plugins;
using ServerShot.Framework.Core.Architecture;

namespace ServerShot.Framework.Core.Implementation.Reporting
{
    public class SendGridReportGenerator : ReportGenerationPluginBase
    {
        public override Task SendSessionReportAsync(ServerShotSessionBase session, IEnumerable<ModuleProcessingSummary> moduleSummaries)
        {
            throw new NotImplementedException();
        }
    }
}
