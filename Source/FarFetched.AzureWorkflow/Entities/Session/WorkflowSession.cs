using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Azure.Workflow.Core.Architecture;
using Azure.Workflow.Core.Entities;
using Azure.Workflow.Core.Entities.Environment;
using Azure.Workflow.Core.Helpers;
using Azure.Workflow.Core.Implementation.StopStrategy;
using Azure.Workflow.Core.Interfaces;
using Azure.Workflow.Core.Plugins;

namespace Azure.Workflow.Core.Implementation
{
    public class WorkflowSession
    {
        private bool _continueMonitoringWorkflow = true;

        public ObservableCollection<IWorkflowModule> RunningModules { get; internal set; }
        public List<object> Modules { get; internal set; }
        public IProcessingStopStrategy StopStrategy { get; internal set; }
        public WorkflowEnvironment Environment { get; internal set; }
        public List<WorkflowSessionPluginBase> Plugins { get; private set; }
        internal ICloudQueueFactory CloudQueueFactory { get; set; }
        internal List<Type> ModuleTypes { get; private set; }

        public DateTime Started { get; private set; }
        public DateTime Ended { get; private set; }

        public TimeSpan TotalDuration
        {
            get { return Ended.Subtract(Started); }
        }

        public WorkflowSessionSettings Settings { get; internal set; }

        public Guid Guid
        {
            get { return Guid.NewGuid(); }
        }

        public string SessionName { get; set; }

        internal WorkflowModuleSettings DefaultModuleSettings { get; set; }

        public event Action<WorkflowSession> OnSessionFinished;
        public event Action<IWorkflowModule, string> OnFailure;

        public WorkflowSession()
        {
            Settings = new WorkflowSessionSettings();

            RunningModules = new ObservableCollection<IWorkflowModule>();
            Modules = new List<object>();
            Plugins = new List<WorkflowSessionPluginBase>();
            StopStrategy = new ContinousProcessingStategy();
            HookRunningModules();

            if (Environment == null)
            {
                Environment = EnvironmentHelpers.BuildStandardEnvironment();
            }
        }

        public WorkflowSession(WorkflowEnvironment environment = null)
            : this()
        {
            Environment = environment ?? EnvironmentHelpers.BuildStandardEnvironment();
        }

        public async Task Start()
        {
            ValidateStart();
            Started = DateTime.Now;

            //validate the plugins
            Plugins.ForEach(ValidatePlugin);

            //inform plugins we have started so they can hook to events
            Plugins.ForEach(x => x.OnSessionStarted(this));
            Modules.ForEach(x => RunningModules.Add(ResolveModule(x)));

            //start the stop monitoring
            Task.Run(async () => await ProcessingStopMonitoring());

            //run the modules
            await Task.WhenAll(RunningModules.Select(x => x.StartAsync()));

            //stop monitoring service
            _continueMonitoringWorkflow = false;

            //inform session has finished
            RegisterFinished(this);
            Ended = DateTime.Now;
        }

        private IWorkflowModule ResolveModule(object o)
        {
            if (o is IWorkflowModule)
            {
                return o as IWorkflowModule;
            }
            if (o is Type)
            {
                return Environment.IOCContainer.Get<IWorkflowModule>(o as Type);
            }
            throw new WorkflowConfigurationException("Type " + o + " added to module list. Not supported. Use type or IWorkflowModule", null);
        }

        private async Task ProcessingStopMonitoring()
        {
            while (_continueMonitoringWorkflow)
            {
                if (StopStrategy.ShouldStop(this))
                {
                    Modules.ForEach(x =>
                    {
                        if (x is IQueueProcessingWorkflowModule)
                        {
                            (x as IQueueProcessingWorkflowModule).Stop();
                        }
                    });
                }

                foreach (IWorkflowModule module in Modules.OfType<IWorkflowModule>())
                {
                    if (StopStrategy.ShouldSpecificModuleStop(module))
                    {
                        if (module is IQueueProcessingWorkflowModule)
                        {
                            (module as IQueueProcessingWorkflowModule).Stop();
                        }
                    }
                }

                await Task.Delay(500);
            }
        }

        #region Helpers

        private void ValidateStart()
        {
            if (Settings == null)
            {
                Settings = new WorkflowSessionSettings();
            }

            if (CloudQueueFactory == null)
            {
                throw new WorkflowConfigurationException("There must be a CloudQueueFactory attached in order to start the session", null);
            }
        }

        internal void ValidatePlugin(WorkflowSessionPluginBase plugin)
        {
            string message = "";
            if ((message = plugin.Validate(this)) != null)
            {
                throw new WorkflowConfigurationException(message, null);
            }
        }

        private void HookRunningModules()
        {
            RunningModules.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    var newItem = args.NewItems[0] as IWorkflowModule;
                    newItem.Queue = CloudQueueFactory.CreateQueue(newItem);
                    newItem.Session = this;
                    if (this.DefaultModuleSettings != null) newItem.Settings = this.DefaultModuleSettings;
                    HookModule(newItem);
                }
            };
        }

        private void HookModule(IWorkflowModule newItem)
        {
            newItem.OnFailure += (s, exceptions) =>
            {
                if (OnFailure != null)
                {
                    OnFailure(newItem, "Module " + newItem.QueueName + " failed : " + s);
                }
            };
        }

        private void RegisterFinished(WorkflowSession session)
        {
            if (OnSessionFinished != null)
            {
                OnSessionFinished(session);
            }
        }

        #endregion

        #region Builder

        public static WorkflowSessionBuilder StartBuild()
        {
            return new WorkflowSessionBuilder(new WorkflowSession());
        }

        public static WorkflowSessionBuilder StartBuildWithSession(WorkflowSession session)
        {
            return new WorkflowSessionBuilder(session);
        }

        #endregion

        #region Mediator

        public virtual void AddToQueue(Type workflowModuleType, IEnumerable<object> batch)
        {
            Type type = workflowModuleType;
            IWorkflowModule module = RunningModules.SingleOrDefault(x => x.GetType() == type);
            module.Queue.AddToAsync(batch);
        }

        #endregion
    }
}