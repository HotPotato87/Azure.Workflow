using System.Threading.Tasks;
using Azure.Workflow.Core.Architecture;
using Azure.Workflow.Core.Entities;
using Azure.Workflow.Core.Implementation;
using Azure.Workflow.Core.Interfaces;
using Azure.Workflow.Core.Plugins;
using Azure.Workflow.Core.Plugins.Alerts;
using Azure.Workflow.Core.Plugins.Persistance;

namespace Azure.Workflow.Core.Builder
{
    public static class WorkflowBuilderExtentions
    {
        public static WorkflowSessionBuilder AddName(this WorkflowSessionBuilder builder, string name)
        {
            builder.WorkflowSession.SessionName = name;

            return builder;
        }

        public static WorkflowSessionBuilder AddModule<T>(this WorkflowSessionBuilder builder) where T : IWorkflowModule
        {
            builder.WorkflowSession.Modules.Add(typeof (T));

            return builder;
        }

        public static WorkflowSessionBuilder AddModule(this WorkflowSessionBuilder builder, IWorkflowModule module)
        {
            builder.WorkflowSession.Modules.Add(module);

            return builder;
        }

        public static WorkflowSessionBuilder WithQueueMechanism(this WorkflowSessionBuilder builder, ICloudQueueFactory cloudQueueFactory)
        {
            builder.WorkflowSession.CloudQueueFactory = cloudQueueFactory;

            return builder;
        }

        public static WorkflowSessionBuilder WithSessionStopStrategy(this WorkflowSessionBuilder builder, IProcessingStopStrategy stopStrategy)
        {
            builder.WorkflowSession.StopStrategy = stopStrategy;

            return builder;
        }

        public static WorkflowSessionBuilder ConfigureDefaultModuleSettings(this WorkflowSessionBuilder builder, WorkflowModuleSettings settings)
        {
            builder.WorkflowSession.DefaultModuleSettings = settings;

            return builder;
        }

        public static WorkflowSessionBuilder ConfigureSessionSettings(this WorkflowSessionBuilder builder, WorkflowSessionSettings settings)
        {
            builder.WorkflowSession.Settings = settings;

            return builder;
        }

        public static WorkflowSessionBuilder AttachReportGenerator(this WorkflowSessionBuilder builder, ReportGenerationPlugin report)
        {
            builder.WorkflowSession.Plugins.Add(report);

            return builder;
        }

        public static WorkflowSessionBuilder AttachAlertManager(this WorkflowSessionBuilder builder, AlertManagerBase alertManager)
        {
            builder.WorkflowSession.Plugins.Add(alertManager);

            return builder;
        }

        public static WorkflowSessionBuilder AttachLogger(this WorkflowSessionBuilder builder, LogManagerBase logger)
        {
            builder.WorkflowSession.Plugins.Add(logger);

            return builder;
        }

        public static WorkflowSessionBuilder AttachPersistance(this WorkflowSessionBuilder builder,
            PersistanceManagerBase persistance)
        {
            builder.WorkflowSession.Plugins.Add(persistance);

            return builder;
        }

        public static WorkflowSessionBuilder AttachPlugin(this WorkflowSessionBuilder builder, WorkflowSessionPluginBase plugin)
        {
            builder.WorkflowSession.Plugins.Add(plugin);

            return builder;
        }

        public static async Task<WorkflowSession> RunAsync(this WorkflowSessionBuilder builder)
        {
            await builder.WorkflowSession.Start();
            return builder.WorkflowSession;
        }
    }
}