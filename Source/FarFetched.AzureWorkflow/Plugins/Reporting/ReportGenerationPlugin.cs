using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Implementation;
using ServerShot.Framework.Core.Implementation.Reporting;
using ServerShot.Framework.Core.Implementation.StopStrategy;
using ServerShot.Framework.Core.Interfaces;
using Servershot.Framework.Entities;

namespace ServerShot.Framework.Core.Plugins
{
    public abstract class ReportGenerationPluginBase : ServerShotSessionBasePluginBase
    {
        public ReadOnlyCollection<ModuleProcessingSummary> ModuleProcessingSummaries { get; set; }
        private readonly List<ModuleProcessingSummary> _moduleProcessingSummaries = new List<ModuleProcessingSummary>(); 

        public ReportGenerationPluginBase()
        {
            ModuleProcessingSummaries = new ReadOnlyCollection<ModuleProcessingSummary>(_moduleProcessingSummaries);
        }

        public override void OnSessionStarted(Implementation.ServerShotSessionBase session)
        {
            base.OnSessionStarted(session);

            if (session is ServerShotLinearSession)
            {
                session.OnFailure += (module, s) => this.SendFailureReport(module, s);
                session.OnSessionFinished += ServerShotSessionBase => this.SendSessionReportAsync(ServerShotSessionBase, ModuleProcessingSummaries);
            }
            else if (session is ServerShotContinuousSession)
            {
                (session as ServerShotContinuousSession).RequestReporting += () => this.SendSessionReportAsync(session, ModuleProcessingSummaries);
            }
            
        }

        public override void OnModuleStarted(IServerShotModule module)
        {
            //TODO : Unit test this logic around categorization properly
            _moduleProcessingSummaries.Add(new ModuleProcessingSummary(module));
            module.OnRaiseProcessed += (args) =>
            {
                var summary = this.ModuleProcessingSummaries.Single(x => x.Module == module);
                if (!summary.ResultCategories.ContainsKey(args.ResultKey.ToString())) summary.ResultCategories[args.ResultKey.ToString()] = 0;
                if (args.CountAsProcessed) summary.TotalProcessed++;
                summary.ResultCategories[args.ResultKey.ToString()]++;
                if (args.Description != null)
                {
                    if (!summary.ResultCategoryExtraDetail.ContainsKey(args.ResultKey.ToString())) summary.ResultCategoryExtraDetail[args.ResultKey.ToString()] = new List<ProcessedItemDetail>();
                    summary.ResultCategoryExtraDetail[args.ResultKey.ToString()].Add(new ProcessedItemDetail(args.Description));
                }
            };
            
            module.OnError += exception =>
            {
                var moduleSummary = this.ModuleProcessingSummaries.Single(x => x.Module == module);
                moduleSummary.Errors++;
                moduleSummary.ErrorList.Add(exception);
            };
            module.OnFinished += () =>
            {
                var moduleSummary = this.ModuleProcessingSummaries.Single(x => x.Module == module);
                moduleSummary.Duration = module.Ended.Subtract(module.Started);
            };
        }

        public abstract Task SendSessionReportAsync(ServerShotSessionBase session, IEnumerable<ModuleProcessingSummary> moduleSummaries);
        public virtual void SendFailureReport(IServerShotModule module, string failureMessage) { }
    }
}
