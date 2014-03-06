using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Builder;
using ServerShot.Framework.Core.Implementation;
using ServerShot.Framework.Core.Interfaces;
using ServerShot.Framework.Core.Plugins;
using ServerShot.Framework.Core.Plugins.Alerts;
using ServerShot.Framework.Core.Plugins.Persistance;
using Servershot.Framework.Plugins.Scaling;

namespace Servershot.Framework.Extentions
{
    public static class ServershotModuleBuilderExtentions
    {
        public static ServerShotModuleBuilder<T> WithInstances<T>(this ServerShotModuleBuilder<T> builder, int instances) where T : class,IServerShotModule
        {
            for (int i = 1; i < instances; i++)
            {
                builder.ServerShotSession.Modules.Add(typeof(T));
            }

            return builder;
        }

        public static ServerShotModuleBuilder<T> OnCreate<T>(this ServerShotModuleBuilder<T> builder, Action<T> action) where T : class, IServerShotModule
        {
            builder.ServerShotSession.RunningModules.CollectionChanged += (item, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    var newModule = args.NewItems[0];

                    if (newModule is T)
                    {
                        action(newModule as T);
                    }
                }
            };

            return builder;
        }

        public static ServerShotModuleBuilder<T> WithModuleQueueMechanism<T>(this ServerShotModuleBuilder<T> builder, ICloudQueueFactory queueFactory) where T : IServerShotModule
        {
            builder.ServerShotSession.ModuleQueueFactory[typeof (T)] = queueFactory;

            return builder;
        }

        public static ServerShotModuleBuilder<T> WithInstanceScaler<T>(this ServerShotModuleBuilder<T> builder, IInstanceScaler scaler) where T : IServerShotModule
        {
            scaler.ModuleType = typeof (T);
            builder.ServerShotSession.Plugins.Add(scaler);

            return builder;
        }

        public static ServerShotModuleBuilder<T> WithModuleAlertManager<T>(this ServerShotModuleBuilder<T> builder, AlertManagerBase alertManager) where T : IServerShotModule
        {
            alertManager.TargettedModule = typeof(T);

            builder.ServerShotSession.Plugins.Add(alertManager);

            return builder;
        }

        public static ServerShotModuleBuilder<T> WithModuleLogger<T>(this ServerShotModuleBuilder<T> builder, LogManagerBase logger) where T : IServerShotModule
        {
            logger.TargettedModule = typeof (T);

            builder.ServerShotSession.Plugins.Add(logger);

            return builder;
        }

        public static ServerShotModuleBuilder<T> WithModulePersistance<T>(this ServerShotModuleBuilder<T> builder, IPersistanceManager persistance) where T : IServerShotModule
        {
            persistance.TargettedModule = typeof(T);

            builder.ServerShotSession.Plugins.Add(persistance);

            return builder;
        }
    }
}
