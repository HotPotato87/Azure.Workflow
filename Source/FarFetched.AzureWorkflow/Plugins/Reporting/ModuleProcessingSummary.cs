using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Workflow.Core.Architecture;
using Azure.Workflow.Core.Implementation.Reporting;

namespace Azure.Workflow.Core.Interfaces
{
    public class ModuleProcessingSummary
    {
        public IWorkflowModule Module { get; private set; }
        public int Errors { get; set; }
        public List<Exception> ErrorList { get; set; }
        public TimeSpan Duration { get; set; }
        public Dictionary<string, int> ResultCategories { get; set; }
        public Dictionary<string, List<ProcessedItemDetail>> ResultCategoryExtraDetail { get; set; }
        public int TotalProcessed { get; set; }

        public ModuleProcessingSummary(IWorkflowModule module)
        {
            Module = module;
            ErrorList = new List<Exception>();
            ResultCategories = new Dictionary<string, int>();
            ResultCategoryExtraDetail = new Dictionary<string, List<ProcessedItemDetail>>();
        }
    }
}
