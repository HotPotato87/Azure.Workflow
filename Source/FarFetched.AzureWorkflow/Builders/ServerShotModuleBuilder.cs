using System;
using ServerShot.Framework.Core.Architecture;

namespace ServerShot.Framework.Core.Implementation
{
    public class ServerShotModuleBuilder<T> : ServerShotSessionBaseBuilder where T : IServerShotModule
    {
        public Type ModuleToAdd { get; set; }

        public ServerShotModuleBuilder(ServerShotSessionBase session, Type module)
            : base(session)
        {
            ModuleToAdd = module;
        }
    }
}