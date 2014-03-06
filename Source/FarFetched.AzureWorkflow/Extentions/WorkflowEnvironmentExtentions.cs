using System;
using Common.Logging.Configuration;
using ServerShot.Framework.Core.Entities.Environment;
using ServerShot.Framework.Core.Interfaces;

namespace ServerShot.Framework.Core.Extentions
{
    public static class ServerShotEnvironmentExtentions
    {
        public static ServerShotEnvironmentWithIOCBuilder WithIOCContainer(this ServerShotEnvironmentBuilder builder, IIocContainer container)
        {
            builder.Environment.IOCContainer = container;
            IOC.Kernel = container;

            return new ServerShotEnvironmentWithIOCBuilder(builder.Environment);
        }

        public static ServerShotEnvironment Build(this ServerShotEnvironmentBuilder builder)
        {
            return builder.Environment;
        }

        public static ServerShotEnvironmentWithIOCBuilder RegisterType<T, T2>(this ServerShotEnvironmentWithIOCBuilder builder, Action<T2> onCreate = null)
        {
            builder.Environment.IOCContainer.Bind(typeof (T), typeof (T2));

            if (onCreate != null)
            {
                builder.Environment.IOCContainer.OnCreate += o =>
                {
                    if (o is T2)
                    {
                        onCreate((T2)o);
                    }
                };
            }

            return builder;
        }

        public static ServerShotEnvironmentWithIOCBuilder RegisterTypeAsSingleton<T, T2>(this ServerShotEnvironmentWithIOCBuilder builder, Action<T2> onCreate = null)
        {
            builder.Environment.IOCContainer.BindAsSingleton(typeof(T), typeof(T2));

            if (onCreate != null)
            {
                builder.Environment.IOCContainer.OnCreate += o =>
                {
                    if (o is T2)
                    {
                        onCreate((T2)o);
                    }
                };
            }

            return builder;
        }

        public static ServerShotEnvironmentWithIOCBuilder RegisterType<T, T2>(this ServerShotEnvironmentWithIOCBuilder builder, params object[] paramObjects)
        {
            builder.Environment.IOCContainer.Bind(typeof(T), typeof(T2), paramObjects);

            return builder;
        }
    }

    public class ServerShotEnvironmentWithIOCBuilder : ServerShotEnvironmentBuilder
    {
        public ServerShotEnvironmentWithIOCBuilder(ServerShotEnvironment serverShotEnvironment) : base(serverShotEnvironment)
        {
        }
    }
}