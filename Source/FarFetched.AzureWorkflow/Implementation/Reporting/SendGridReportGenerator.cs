using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SendGridMail;
using ServerShot.Framework.Core.Interfaces;
using ServerShot.Framework.Core.Plugins;
using ServerShot.Framework.Core.Architecture;

namespace ServerShot.Framework.Core.Implementation.Reporting
{
    public class SendGridReportGenerator : ReportGenerationPluginBase
    {
        private string _html = "";

        public string EmailAddress { get; set; }

        public override Task SendSessionReportAsync(ServerShotSessionBase session, IEnumerable<ModuleProcessingSummary> moduleSummaries)
        {
            this.AppendOutput();
            this.AppendOutput("*****************************");
            this.AppendOutput("********* SUMMARY ***********");
            this.AppendOutput("*****************************");
            this.AppendOutput("{0} Total Processed", moduleSummaries.Sum(x => x.TotalProcessed));
            this.AppendOutput("{0} Running Time", session.Ended.Subtract(session.Started).TotalSeconds + "s");
            this.AppendOutput("{0} Errors", moduleSummaries.Sum(t => t.Errors));
            this.AppendOutput("");

            foreach (var processingSummary in moduleSummaries)
            {
                this.AppendOutput(" ");
                this.AppendOutput("[{0}]", processingSummary.Module.QueueName);
                this.AppendOutput("-------------------------------------");
                this.AppendOutput("Total Processed {0}", processingSummary.TotalProcessed);
                this.AppendOutput("Started {0}", processingSummary.Module.Started);
                this.AppendOutput("Finished {0}", processingSummary.Module.Ended);

                foreach (var sentToAudit in processingSummary.Module.SentToAudit)
                {
                    this.AppendOutput("{0}:{1} Sent Count", sentToAudit.Key, sentToAudit.Value);
                }

                foreach (var category in processingSummary.ResultCategories)
                {
                    this.AppendOutput("[{0} : {1} items]", category.Key, category.Value);
                }

                foreach (var detail in processingSummary.ResultCategoryExtraDetail)
                {
                    detail.Value.ForEach(x => this.AppendOutput("----{0}:{1}", x.ProcessedTime, x.Message));
                }

                this.AppendOutput(" ");
            }

            this.AppendOutput("********* Finished! *********");

            this.SendEmail(_html);

            return Task.FromResult<object>(null);
        }

        private void SendEmail(string html)
        {

        }

        private void AppendOutput(string text = "", params object[] args)
        {
            _html += "<p>" + String.Format(text, args) + "</p>";
        }
    }
}
