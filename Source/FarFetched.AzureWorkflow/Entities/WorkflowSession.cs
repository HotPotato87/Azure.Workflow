using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core.Architecture;
using FarFetched.AzureWorkflow.Core.Interfaces;
using FarFetched.AzureWorkflow.Core.Plugins;

namespace FarFetched.AzureWorkflow.Core.Implementation
{
    public class WorkflowSession
    {
        public event Action<WorkflowSession> OnSessionFinished;

        public ObservableCollection<IWorkflowModule> RunningModules { get; set; }
        public List<IWorkflowModule> Modules { get; set; } 
        internal List<WorkflowSessionPluginBase> Plugins { get; set; }
        internal ICloudQueueFactory CloudQueueFactory { get; set; }

        internal WorkflowSession()
        {
            RunningModules = new ObservableCollection<IWorkflowModule>();
            Modules = new List<IWorkflowModule>();
            Plugins = new List<WorkflowSessionPluginBase>();

            HookRunningModules();
        }

        public async Task Start()
        {
            ValidateStart();

            //inform plugins we have started so they can hook to events
            Plugins.ForEach(x=>x.OnSessionStarted(this));

            //run the modules
            foreach (var module in Modules)
            {
                RunningModules.Add(module);
                await module.StartAsync();
            }

            //inform session has finished
            this.RegisterFinished(this);
        }

        #region Helpers

        private void ValidateStart()
        {
            if (this.CloudQueueFactory == null)
            {
                throw new AzureWorkflowConfigurationException("There must be a CloudQueueFactory attached in order to start the session", null);
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

        #endregion

        #region Mediator

        internal void AddToQueue(Type workflowModuleType, IEnumerable<object> batch)
        {
            var type = workflowModuleType;
            var module = this.RunningModules.SingleOrDefault(x => x.GetType() == type);
            module.Queue.AddToAsync(batch);
        }

        #endregion
    }
}
