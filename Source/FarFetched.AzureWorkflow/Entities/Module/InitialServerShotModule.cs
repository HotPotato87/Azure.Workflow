using System;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Implementation;

namespace ServerShot.Framework.Core
{
    public abstract class InitialServerShotModule<T> : ServerShotModuleBase<T> where T : class
    {
        public InitialServerShotModule(ServerShotModuleSettings settings = default(ServerShotModuleSettings))
            :base(settings)
        {
            
        }
    }
}