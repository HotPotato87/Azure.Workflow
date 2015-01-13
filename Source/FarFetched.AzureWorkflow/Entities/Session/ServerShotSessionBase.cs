using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Entities;
using ServerShot.Framework.Core.Entities.Environment;
using ServerShot.Framework.Core.Entities.Scheduler.Deployments;
using ServerShot.Framework.Core.Enums;
using ServerShot.Framework.Core.Helpers;
using ServerShot.Framework.Core.Implementation.Logging;
using ServerShot.Framework.Core.Implementation.StopStrategy;
using ServerShot.Framework.Core.Interfaces;
using ServerShot.Framework.Core.Plugins;
using ServerShot.Framework.Core.Plugins.Alerts;

namespace ServerShot.Framework.Core.Implementation
{
    public abstract class ServerShotSessionBase
    {
        #region Public Properties

        public ObservableCollection<IServerShotModule> RunningModules { get; internal set; }
        public List<Type> Modules { get; internal set; }
        public IProcessingStopStrategy StopStrategy { get; internal set; }
        public ServerShotEnvironment Environment { get; internal set; }
        public List<IServerShotSessionBasePlugin> Plugins { get; private set; }
        public DateTime Started { get; private set; }
        public DateTime Ended { get; private set; }
        public SessionSettings Settings { get; internal set; }
        public string SessionName { get; set; }
        public ServerShotModuleSettings DefaultModuleSettings { get; internal set; }
        public TimeSpan TotalDuration
        {
            get { return Ended.Subtract(Started); }
        }
        public Guid Guid
        {
            get { return Guid.NewGuid(); }
        }

        #endregion

        #region Internal Properties

        internal ICloudQueueFactory CloudQueueFactory { get; set; }
        internal Dictionary<Type, object[]> ModuleConstructorArgs { get; private set; }
        internal IDeploymentStrategy DeploymentStrategy { get; set; }
        internal Dictionary<Type, ICloudQueueFactory> ModuleQueueFactory { get; set; }

        #endregion

        #region Public Events

        public event Action<ServerShotSessionBase> OnSessionFinished;
        public event Action<IServerShotModule, string> OnFailure;

        #endregion

        #region Private Instance Vars

        private bool _continueMonitoringSession = true;
        public bool _lockRunningModules;

        #endregion

        #region Constructor

        public ServerShotSessionBase()
        {
            Settings = new SessionSettings();

            ModuleQueueFactory = new Dictionary<Type, ICloudQueueFactory>();
            RunningModules = new ObservableCollection<IServerShotModule>();
            Modules = new List<Type>();
            Plugins = new List<IServerShotSessionBasePlugin>();
            StopStrategy = new ContinousProcessingStategy();
            ModuleConstructorArgs = new Dictionary<Type, object[]>();
            HookRunningModules();

            if (Environment == null)
            {
                Environment = EnvironmentHelpers.BuildStandardEnvironment();
            }
        }

        public ServerShotSessionBase(ServerShotEnvironment environment = null)
            : this()
        {
            Environment = environment ?? EnvironmentHelpers.BuildStandardEnvironment();
        }

        #endregion

        #region Main Operations

        public virtual async Task Start()
        {
            try
            {
                ValidateStart();
                Started = DateTime.Now;

                //validate the plugins
                Plugins.ForEach(ValidatePlugin);

                //inform plugins we have started so they can hook to events
                Plugins.ForEach(x => x.OnSessionStarted(this));
                Modules.ForEach(x => RunningModules.Add(ResolveModule(x)));
            }
            catch (Exception e)
            {
                throw new WorkflowConfigurationException(e.Message, e);
            }

            //validate the modules
            foreach (var serverShotModule in RunningModules)
            {
                var validateResult = await serverShotModule.Validate();

                if (!validateResult.DidValidate)
                {
                    throw new WorkflowConfigurationException(serverShotModule.QueueName + " would not validate = " + validateResult.Message);
                }
            }

            //Init the modules
            foreach (var serverShotModule in RunningModules)
            {
                await serverShotModule.InitAsync();
            }

            //start the stop monitoring
            Task.Run(async () => await ProcessingStopMonitoring());

            //run the modules
            await Task.WhenAll(RunningModules.Select(x => x.StartAsync()));

            //stop monitoring service
            _continueMonitoringSession = false;

            //inform session has finished
            RegisterFinished(this);
            Ended = DateTime.Now;
        }

        public virtual void Stop()
        {
            this.Ended = DateTime.Now;
            RunningModules.ToList().ForEach(x => x.Stop());
        }

        public async Task ChangeInstancesAsync(IServerShotModule module, int add = 0, int remove = 0)
        {
            await Task.Run(async () =>
            {
                if (add > 0)
                {
                    if (this.RunningModules.Any(t => t.GetType() == module.GetType()))
                    {
                        var newModule = ResolveModule(module.GetType());
                        this.RunningModules.Add(newModule);
                        this.LogSessionMessage(string.Format("{0} increased {1} instances", module.QueueName, add));
                        Task.Run(() => newModule.StartAsync());
                    }
                }
                else if (remove > 0)
                {
                    //todo : change
                }
            });
        }
        
        #endregion

        #region Protected

        protected void LogSessionMessage(string message)
        {
            var loggingPlugin = this.Plugins.SingleOrDefault(x => x is ILoggingManager);

            if (loggingPlugin != null)
            {
                (loggingPlugin as ILoggingManager).OnLogMessage(new LogMessage(message, "session"));
            }
        }

        #endregion

        #region Helpers

        private IServerShotModule ResolveModule(Type moduleType)
        {
            var paramObjects = ModuleConstructorArgs.ContainsKey(moduleType) ? ModuleConstructorArgs[moduleType] : null;

            return paramObjects != null ?
                (IServerShotModule)Activator.CreateInstance(moduleType, paramObjects) :
                ResolveByIOC(moduleType);
        }

        // ReSharper disable once InconsistentNaming
        private IServerShotModule ResolveByIOC(object o)
        {
            if (Environment == null || Environment.IOCContainer == null)
            {
                throw new WorkflowConfigurationException("No IOC Container setup, please build an environment");
            }

            return Environment.IOCContainer.Get<IServerShotModule>(o as Type);
        }

        private async Task ProcessingStopMonitoring()
        {
            while (_continueMonitoringSession && this.RunningModules.Any(t=>t.State != ModuleState.Finished))
            {
                if (StopStrategy.ShouldStop(this))
                {
                    _continueMonitoringSession = false;
                    Stop();
                    return;
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

            if (DeploymentStrategy == null)
            {
                this.DeploymentStrategy = new LocalDeploymentStrategy();
            }
        }

        internal void ValidatePlugin(IServerShotSessionBasePlugin plugin)
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
                _lockRunningModules = true;
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    var newItem = args.NewItems[0] as IServerShotModule;

                    //multiple instances of the same module share the same queue
                    newItem.Queue = RunningModules.Any(x=>x != newItem && x.GetType() == newItem.GetType()) 
                        ? RunningModules.First(x => x != newItem && x.GetType() == newItem.GetType()).Queue 
                        : ModuleQueueFactory.ContainsKey(newItem.GetType()) ? ModuleQueueFactory[newItem.GetType()].CreateQueue(newItem) : CloudQueueFactory.CreateQueue(newItem);
                    
                    newItem.Session = this;
                    if (this.DefaultModuleSettings != null) newItem.Settings = this.DefaultModuleSettings;
                    HookModule(newItem);
                }
                _lockRunningModules = false;
            };
        }

        private void HookModule(IServerShotModule newItem)
        {
            newItem.OnFailure += (s, exceptions) =>
            {
                if (this.Settings.NeverFail) return;
                if (OnFailure != null)
                {
                    OnFailure(newItem, "Module " + newItem.QueueName + " failed : " + s);
                }
                this.Stop();
            };
        }

        private void RegisterFinished(ServerShotSessionBase session)
        {
            if (OnSessionFinished != null)
            {
                OnSessionFinished(session);
            }
        }

        #endregion

        #region Mediator

        public virtual void AddToQueue(Type workflowModuleType, IEnumerable<object> batch)
        {
            Type type = workflowModuleType;
            IServerShotModule module = RunningModules.FirstOrDefault(x => x.GetType() == type);
            module.Queue.AddToAsync(batch);
        }

        #endregion

        #region Builder

        public static ServerShotSessionBaseBuilder StartBuildWithSession(ServerShotSessionBase session)
        {
            return new ServerShotSessionBaseBuilder(session);
        }

        #endregion
    }
}