using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Enums;

namespace Servershot.Framework.EventHandlers
{
    public class OnRaisedProcessedArgs
    {
        public object ResultKey { get; set; }
        public string Description { get; set; }
        public CategorizationLevel Level { get; set; }
        public bool CountAsProcessed { get; set; }

        public OnRaisedProcessedArgs(object resultKey, string description, CategorizationLevel level, bool countAsProcessed)
        {
            ResultKey = resultKey;
            Description = description;
            Level = level;
            CountAsProcessed = countAsProcessed;
        }
    }
}
