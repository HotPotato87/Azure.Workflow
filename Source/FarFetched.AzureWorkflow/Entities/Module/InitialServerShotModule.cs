using System;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Implementation;

namespace ServerShot.Framework.Core
{
    public interface IInitialServerShotModule
    {
        
    }

    public abstract class InitialServerShotModule<T> : ServerShotModuleBase<T>, IInitialServerShotModule where T : class
    {
        public InitialServerShotModule(ServerShotModuleSettings settings = default(ServerShotModuleSettings))
            :base(settings)
        {
            
        }
    }
}