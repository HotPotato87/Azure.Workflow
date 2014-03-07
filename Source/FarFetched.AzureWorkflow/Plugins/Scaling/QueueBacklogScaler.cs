using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ServerShot.Framework.Core.Annotations;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Plugins;
using Timer = System.Timers.Timer;

namespace Servershot.Framework.Plugins.Scaling
{
    public class QueueBacklogScaler : ServerShotSessionBasePluginBase, IInstanceScaler
    {
        public Type ModuleType { get; set; }

        /// <summary>
        /// Amount of backlogs in a queue before new instance is potentially spawned
        /// </summary>
        public int QueueThreshold { get; set; }

        /// <summary>
        /// Maximum number of instances to spawn
        /// </summary>
        public int MaxInstances { get; set; }

        /// <summary>
        /// Amount of time to sample throughput to decide instance change
        /// </summary>
        public TimeSpan Sample { get; set; }

        /// <summary>
        /// After an module's instances are changed, this is the amount of time to sample before another change
        /// </summary>
        public TimeSpan InstanceTimeLayoffTime { get; set; }

        private readonly ConcurrentDictionary<long, int> _queueCountWindow = new ConcurrentDictionary<long, int>();
        private IServerShotModule _module;
        private DateTime _lastChangedInstance = DateTime.MinValue;

        public QueueBacklogScaler()
        {
            Sample = TimeSpan.FromMilliseconds(50);
            InstanceTimeLayoffTime = TimeSpan.FromMilliseconds(200);
        }

        public override void OnModuleStarted(IServerShotModule module)
        {
            if (module.GetType() != ModuleType) return;

            _queueCountWindow.Clear();
            _module = module;
            StartMonitoring(module);
        }

        private void StartMonitoring(IServerShotModule module)
        {
            var timer = new Timer(500);
            timer.Elapsed += EvaluateModule;
            timer.Start();
        }

        private bool _lock = false;

        private async void EvaluateModule(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (_lock) return;

            _lock = true;

            //add sample to window
            _queueCountWindow.TryAdd(DateTime.Now.Ticks, _module.Queue.Count);

            //evaluate if current window requires change of instances
            var sampleWindow = _queueCountWindow.Where(x => new DateTime(x.Key) >= new DateTime(x.Key).Subtract(Sample));
            var averageQueueCount = sampleWindow.Average(t => t.Value);

            if (averageQueueCount > this.QueueThreshold)
            {
                //check if recently changed
                if (DateTime.Now.Subtract(_lastChangedInstance) >= this.InstanceTimeLayoffTime)
                {
                    if (_module.Session.RunningModules.Count(x => x.GetType() == _module.GetType()) < this.MaxInstances)
                    {
                        await _module.Session.ChangeInstancesAsync(_module, add: 1);
                        _lastChangedInstance = DateTime.Now; 
                    }
                }
            }

            //remove all expired window items
            foreach (var item in _queueCountWindow.Where(t => !sampleWindow.Contains(t)).ToList())
            {
                int ss;
                _queueCountWindow.TryRemove(item.Key, out ss);
            }

            _lock = false;
        }
    }
}
