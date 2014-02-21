using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Implementation;

namespace ServerShot.Framework.Core.Plugins
{
    public abstract class ServerShotSessionPluginBase
    {
        protected ServerShotSession Session { get; set; }

        internal virtual void OnSessionStarted(ServerShotSession session)
        {
            this.Session = session;

            session.RunningModules.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (var module in  args.NewItems)
                    {
                        this.OnModuleStarted(module as IServerShotModule);
                    }    
                }
            };
        }

        internal abstract void OnModuleStarted(IServerShotModule module);

        public virtual string Validate(ServerShotSession module)
        {
            return null;
        }
    }
}
