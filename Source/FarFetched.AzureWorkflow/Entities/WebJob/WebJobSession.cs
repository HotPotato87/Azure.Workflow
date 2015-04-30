using System.Collections.Generic;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Entities.Environment;
using ServerShot.Framework.Core.Plugins;

namespace Servershot.Framework.Entities.WebJob
{
    public class WebJobSession
    {
        private readonly ServerShotEnvironment _environment;
        public List<JibJobModule> RunningJobs { get; set; }
        public List<IJibJobSessionPlugin> RunningPlugins { get; set; }

        public WebJobSession(ServerShotEnvironment environment)
        {
            _environment = environment;
            RunningJobs= new List<JibJobModule>();
            RunningPlugins = new List<IJibJobSessionPlugin>();
        }

        public void AddWebJob<T>() where T : JibJobModule
        {
            this.RunningJobs.Add(_environment.IOCContainer.Get<T>());
        }

        public void AddWebJob(JibJobModule jibJob)
        {
            this.RunningJobs.Add(jibJob);

            RunningPlugins.ForEach(x=>x.RegisterWebjob(jibJob));
        }

        public void AttachPlugin(IJibJobSessionPlugin plugin)
        {
            RunningPlugins.Add(plugin);
        }
    }
}