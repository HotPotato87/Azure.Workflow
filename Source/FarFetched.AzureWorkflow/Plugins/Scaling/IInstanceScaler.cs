using System;
using ServerShot.Framework.Core.Plugins;

namespace Servershot.Framework.Plugins.Scaling
{
    public interface IInstanceScaler : IServerShotSessionBasePlugin
    {
        Type ModuleType { get; set; }
    }
}