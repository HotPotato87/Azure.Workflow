using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servershot.Framework.Enums
{
    public enum LoggingType
    {
        Infrastructure,
        LowImportance,
        MediumImportance,
        HighImportance,
        Error,
        ServerShotError,
        Failure
    }
}
