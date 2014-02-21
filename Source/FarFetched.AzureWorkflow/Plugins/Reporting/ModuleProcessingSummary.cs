using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Implementation.Reporting;

namespace ServerShot.Framework.Core.Interfaces
{
    public class ModuleProcessingSummary
    {
        public IServerShotModule Module { get; private set; }
        public int Errors { get; set; }
        public List<Exception> ErrorList { get; set; }
        public TimeSpan Duration { get; set; }
        public Dictionary<string, int> ResultCategories { get; set; }
        public Dictionary<string, List<ProcessedItemDetail>> ResultCategoryExtraDetail { get; set; }
        public int TotalProcessed { get; set; }

        public ModuleProcessingSummary(IServerShotModule module)
        {
            Module = module;
            ErrorList = new List<Exception>();
            ResultCategories = new Dictionary<string, int>();
            ResultCategoryExtraDetail = new Dictionary<string, List<ProcessedItemDetail>>();
        }
    }
}
