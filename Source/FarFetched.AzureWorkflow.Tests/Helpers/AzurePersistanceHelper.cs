using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Implementation.Persistance;
using ServerShot.Framework.Core.Plugins.Persistance;

namespace FarFetched.AzureWorkflow.Tests.Helpers
{
    public static class AzurePersistanceHelper
    {
        public static string _accountName = "serviceshot";
        public static string _accountKey = "57Kn2DQl1nX/TjZQyNyNZzDuyfOup0fCfPoT/gZqdYJc8lpygJUq9S6lln3f5O7+s6PzBbArIn2Z+cqMcdqqDg==";

        public static IPersistanceManager CreatePersistanceClient()
        {
            return new AzureTablePersistance(_accountName, _accountKey);
        }
    }
}
