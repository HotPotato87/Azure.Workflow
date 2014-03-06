using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Servershot.Framework.Builders;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Entities;
using ServerShot.Framework.Core.Entities.Scheduler.Deployments;
using ServerShot.Framework.Core.Implementation;
using ServerShot.Framework.Core.Interfaces;
using ServerShot.Framework.Core.Plugins;
using ServerShot.Framework.Core.Plugins.Alerts;
using ServerShot.Framework.Core.Plugins.Persistance;

namespace ServerShot.Framework.Core.Builder
{
    public static class WorkflowBuilderExtentions
    {
        public static ServerShotSessionBaseBuilder AddName(this ServerShotSessionBaseBuilder builder, string name)
        {
            builder.ServerShotSession.SessionName = name;

            return builder;
        }

        public static ServerShotModuleBuilder<T> AddModule<T>(this ServerShotSessionBaseBuilder builder, params object[] paramObjects) where T : IServerShotModule
        {
            builder.ServerShotSession.Modules.Add(typeof(T));

            if (paramObjects.Any())
            {
                builder.ServerShotSession.ModuleConstructorArgs[typeof(T)] = (paramObjects);  
            }

            return new ServerShotModuleBuilder<T>(builder.ServerShotSession, typeof(T));
        }

        public static ServerShotSessionBaseBuilder AttachSessionQueueMechanism(this ServerShotSessionBaseBuilder builder, ICloudQueueFactory cloudQueueFactory)
        {
            builder.ServerShotSession.CloudQueueFactory = cloudQueueFactory;

            return builder;
        }

        public static ServerShotSessionBaseBuilder WithSessionStopStrategy(this ServerShotSessionBaseBuilder builder, IProcessingStopStrategy stopStrategy)
        {
            builder.ServerShotSession.StopStrategy = stopStrategy;

            return builder;
        }

        public static ServerShotDeploymentBuilder WithSessionDeploymentStrategy(this ServerShotSessionBaseBuilder builder, IDeploymentStrategy deploymentStrategy)
        {
            builder.ServerShotSession.DeploymentStrategy = deploymentStrategy;

            return new ServerShotDeploymentBuilder(builder.ServerShotSession);
        }

        public static ServerShotSessionBaseBuilder ConfigureDefaultModuleSettings(this ServerShotSessionBaseBuilder builder, ServerShotModuleSettings settings)
        {
            builder.ServerShotSession.DefaultModuleSettings = settings;

            return builder;
        }

        public static ServerShotSessionBaseBuilder ConfigureSessionSettings(this ServerShotSessionBaseBuilder builder, SessionSettings settings)
        {
            builder.ServerShotSession.Settings = settings;

            return builder;
        }

        public static ServerShotSessionBaseBuilder AttachSessionReportGenerator(this ServerShotSessionBaseBuilder builder, ReportGenerationPluginBase report)
        {
            builder.ServerShotSession.Plugins.Add(report);

            return builder;
        }

        public static ServerShotSessionBaseBuilder AttachSessionPersistance(this ServerShotSessionBaseBuilder builder, IPersistanceManager persistance)
        {
            builder.ServerShotSession.Plugins.Add(persistance);

            return builder;
        }

        public static ServerShotSessionBaseBuilder AttachSessionAlertManager(this ServerShotSessionBaseBuilder builder, AlertManagerBase alertManager)
        {
            builder.ServerShotSession.Plugins.Add(alertManager);

            return builder;
        }

        public static ServerShotSessionBaseBuilder AttachSessionLogger(this ServerShotSessionBaseBuilder builder, LogManagerBase logger)
        {
            builder.ServerShotSession.Plugins.Add(logger);

            return builder;
        }

        public static ServerShotSessionBaseBuilder AttachSessionPersistance<T>(this ServerShotSessionBaseBuilder builder, IPersistanceManager persistance) where T : IServerShotModule
        {
            builder.ServerShotSession.Plugins.Add(persistance);

            return builder;
        }

        public static ServerShotSessionBaseBuilder AttachPlugin<T>(this ServerShotSessionBaseBuilder builder, ServerShotSessionBasePluginBase plugin) where T : IServerShotModule
        {
            builder.ServerShotSession.Plugins.Add(plugin);

            return builder;
        }

        public static async Task<ServerShotSessionBase> RunAsync(this ServerShotSessionBaseBuilder builder)
        {
            await builder.ServerShotSession.Start();
            return builder.ServerShotSession;
        }

        
    }

}