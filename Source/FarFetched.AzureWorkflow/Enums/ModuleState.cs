using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Workflow.Core.Enums
{
    public enum ModuleState
    {
        Waiting,
        Processing,
        Error,
        Finished
    }
}
