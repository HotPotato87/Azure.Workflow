using System.Threading.Tasks;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Entities;
using ServerShot.Framework.Core.Implementation;
using ServerShot.Framework.Core.Interfaces;
using ServerShot.Framework.Core.Plugins;
using ServerShot.Framework.Core.Plugins.Alerts;
using ServerShot.Framework.Core.Plugins.Persistance;

namespace ServerShot.Framework.Core.Builder
{
    public static class WorkflowBuilderExtentions
    {
        public static ServerShotSessionBuilder AddName(this ServerShotSessionBuilder builder, string name)
        {
            builder.ServerShotSession.SessionName = name;

            return builder;
        }

        public static ServerShotSessionBuilder AddModule<T>(this ServerShotSessionBuilder builder) where T : IServerShotModule
        {
            builder.ServerShotSession.Modules.Add(typeof (T));

            return builder;
        }

        public static ServerShotSessionBuilder AddModule(this ServerShotSessionBuilder builder, IServerShotModule module)
        {
            builder.ServerShotSession.Modules.Add(module);

            return builder;
        }

        public static ServerShotSessionBuilder WithQueueMechanism(this ServerShotSessionBuilder builder, ICloudQueueFactory cloudQueueFactory)
        {
            builder.ServerShotSession.CloudQueueFactory = cloudQueueFactory;

            return builder;
        }

        public static ServerShotSessionBuilder WithSessionStopStrategy(this ServerShotSessionBuilder builder, IProcessingStopStrategy stopStrategy)
        {
            builder.ServerShotSession.StopStrategy = stopStrategy;

            return builder;
        }

        public static ServerShotSessionBuilder ConfigureDefaultModuleSettings(this ServerShotSessionBuilder builder, ServerShotModuleSettings settings)
        {
            builder.ServerShotSession.DefaultModuleSettings = settings;

            return builder;
        }

        public static ServerShotSessionBuilder ConfigureSessionSettings(this ServerShotSessionBuilder builder, SessionSettings settings)
        {
            builder.ServerShotSession.Settings = settings;

            return builder;
        }

        public static ServerShotSessionBuilder AttachReportGenerator(this ServerShotSessionBuilder builder, ReportGenerationPlugin report)
        {
            builder.ServerShotSession.Plugins.Add(report);

            return builder;
        }

        public static ServerShotSessionBuilder AttachAlertManager(this ServerShotSessionBuilder builder, AlertManagerBase alertManager)
        {
            builder.ServerShotSession.Plugins.Add(alertManager);

            return builder;
        }

        public static ServerShotSessionBuilder AttachLogger(this ServerShotSessionBuilder builder, LogManagerBase logger)
        {
            builder.ServerShotSession.Plugins.Add(logger);

            return builder;
        }

        public static ServerShotSessionBuilder AttachPersistance(this ServerShotSessionBuilder builder,
            PersistanceManagerBase persistance)
        {
            builder.ServerShotSession.Plugins.Add(persistance);

            return builder;
        }

        public static ServerShotSessionBuilder AttachPlugin(this ServerShotSessionBuilder builder, ServerShotSessionPluginBase plugin)
        {
            builder.ServerShotSession.Plugins.Add(plugin);

            return builder;
        }

        public static async Task<ServerShotSession> RunAsync(this ServerShotSessionBuilder builder)
        {
            await builder.ServerShotSession.Start();
            return builder.ServerShotSession;
        }
    }
}