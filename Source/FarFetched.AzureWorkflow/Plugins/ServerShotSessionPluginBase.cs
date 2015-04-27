using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Implementation;

namespace ServerShot.Framework.Core.Plugins
{
    public interface IServerShotSessionBasePlugin
    {
        ServerShotSessionBase Session { get; set; }
        void OnSessionStarted(ServerShotSessionBase session);
        void OnModuleStarted(IServerShotModule module);
        string Validate(ServerShotSessionBase module);
        bool IsModuleBasedPlugin { get; }
        Type TargettedModule { get; set; }
    }

    public abstract class ServerShotSessionBasePluginBase : IServerShotSessionBasePlugin
    {
        public ServerShotSessionBase Session { get; set; }

        public Type TargettedModule { get; set; }

        public bool IsModuleBasedPlugin
        {
            get { return TargettedModule != null; }
        }

        public virtual void OnSessionStarted(ServerShotSessionBase session)
        {
            this.Session = session;

            session.RunningModules.CollectionChanged += (sender, args) =>
            {
                session._lockRunningModules = true;
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (var module in args.NewItems.Cast<IServerShotModule>())
                    {
                        if (IsModuleBasedPlugin)
                        {
                            if (TargettedModule == module.GetType())
                            {
                                OnModuleStarted(module);
                            }
                        }
                        else
                        {
                            if (NoModuleBasedPluginFor(module))
                            {
                                this.OnModuleStarted(module);
                            }
                        }
                    }    
                }
                session._lockRunningModules = false;
            };
        }

        private bool NoModuleBasedPluginFor(IServerShotModule module)
        {
            return !this.Session.Plugins.Where(t => t.IsModuleBasedPlugin).Any(x =>
                x.GetType() == this.GetType() &&
                x.TargettedModule == module.GetType());
        }

        public abstract void OnModuleStarted(IServerShotModule module);

        public virtual string Validate(ServerShotSessionBase module)
        {
            return null;
        }
    }
}
