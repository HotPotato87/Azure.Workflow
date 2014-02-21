using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Entities;
using ServerShot.Framework.Core.Entities.Environment;
using ServerShot.Framework.Core.Helpers;
using ServerShot.Framework.Core.Implementation.StopStrategy;
using ServerShot.Framework.Core.Interfaces;
using ServerShot.Framework.Core.Plugins;

namespace ServerShot.Framework.Core.Implementation
{
    public class ServerShotSession
    {
        private bool _continueMonitoringWorkflow = true;

        public ObservableCollection<IServerShotModule> RunningModules { get; internal set; }
        public List<object> Modules { get; internal set; }
        public IProcessingStopStrategy StopStrategy { get; internal set; }
        public WorkflowEnvironment Environment { get; internal set; }
        public List<ServerShotSessionPluginBase> Plugins { get; private set; }
        internal ICloudQueueFactory CloudQueueFactory { get; set; }
        internal List<Type> ModuleTypes { get; private set; }

        public DateTime Started { get; private set; }
        public DateTime Ended { get; private set; }

        public TimeSpan TotalDuration
        {
            get { return Ended.Subtract(Started); }
        }

        public SessionSettings Settings { get; internal set; }

        public Guid Guid
        {
            get { return Guid.NewGuid(); }
        }

        public string SessionName { get; set; }

        public ServerShotModuleSettings DefaultModuleSettings { get; internal set; }

        public event Action<ServerShotSession> OnSessionFinished;
        public event Action<IServerShotModule, string> OnFailure;

        public ServerShotSession()
        {
            Settings = new SessionSettings();

            RunningModules = new ObservableCollection<IServerShotModule>();
            Modules = new List<object>();
            Plugins = new List<ServerShotSessionPluginBase>();
            StopStrategy = new ContinousProcessingStategy();
            HookRunningModules();

            if (Environment == null)
            {
                Environment = EnvironmentHelpers.BuildStandardEnvironment();
            }
        }

        public ServerShotSession(WorkflowEnvironment environment = null)
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

        private IServerShotModule ResolveModule(object o)
        {
            if (o is IServerShotModule)
            {
                return o as IServerShotModule;
            }
            if (o is Type)
            {
                return Environment.IOCContainer.Get<IServerShotModule>(o as Type);
            }
            throw new WorkflowConfigurationException("Type " + o + " added to module list. Not supported. Use type or IServerShotModule", null);
        }

        private async Task ProcessingStopMonitoring()
        {
            while (_continueMonitoringWorkflow)
            {
                if (StopStrategy.ShouldStop(this))
                {
                    Stop();
                }

                foreach (IServerShotModule module in Modules.OfType<IServerShotModule>())
                {
                    if (StopStrategy.ShouldSpecificModuleStop(module))
                    {
                        if (module is IQueueProcessingServerShotModule)
                        {
                            (module as IQueueProcessingServerShotModule).Stop();
                        }
                    }
                }

                await Task.Delay(500);
            }
        }

        public void Stop()
        {
            Modules.ForEach(x =>
            {
                if (x is IServerShotModule)
                {
                    (x as IServerShotModule).Stop();
                }
            });
        }

        #region Helpers

        private void ValidateStart()
        {
            if (Settings == null)
            {
                Settings = new SessionSettings();
            }

            if (CloudQueueFactory == null)
            {
                throw new WorkflowConfigurationException("There must be a CloudQueueFactory attached in order to start the session", null);
            }
        }

        internal void ValidatePlugin(ServerShotSessionPluginBase plugin)
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
                    var newItem = args.NewItems[0] as IServerShotModule;
                    newItem.Queue = CloudQueueFactory.CreateQueue(newItem);
                    newItem.Session = this;
                    if (this.DefaultModuleSettings != null) newItem.Settings = this.DefaultModuleSettings;
                    HookModule(newItem);
                }
            };
        }

        private void HookModule(IServerShotModule newItem)
        {
            newItem.OnFailure += (s, exceptions) =>
            {
                if (OnFailure != null)
                {
                    OnFailure(newItem, "Module " + newItem.QueueName + " failed : " + s);
                }
                this.Stop();
            };
        }

        private void RegisterFinished(ServerShotSession session)
        {
            if (OnSessionFinished != null)
            {
                OnSessionFinished(session);
            }
        }

        #endregion

        #region Builder

        public static ServerShotSessionBuilder StartBuild()
        {
            return new ServerShotSessionBuilder(new ServerShotSession());
        }

        public static ServerShotSessionBuilder StartBuildWithSession(ServerShotSession session)
        {
            return new ServerShotSessionBuilder(session);
        }

        #endregion

        #region Mediator

        public virtual void AddToQueue(Type workflowModuleType, IEnumerable<object> batch)
        {
            Type type = workflowModuleType;
            IServerShotModule module = RunningModules.SingleOrDefault(x => x.GetType() == type);
            module.Queue.AddToAsync(batch);
        }

        #endregion
    }
}