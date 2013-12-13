using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Workflow.Core.Plugins.Persistance;

namespace Azure.Workflow.Core.Implementation.Persistance
{
    public class AzureTablePersistance : PersistanceManagerBase
    {
        protected override void OnStore(string key, object o)
        {
            throw new NotImplementedException();
        }

        protected override object ModuleOnOnRetrieve(string s)
        {
            throw new NotImplementedException();
        }
    }
}
