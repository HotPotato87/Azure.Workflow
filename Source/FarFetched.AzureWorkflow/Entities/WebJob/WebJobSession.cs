using System.Collections.Generic;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Entities.Environment;
using ServerShot.Framework.Core.Plugins;

namespace Servershot.Framework.Entities.WebJob
{
    public class WebJobSession
    {
        private readonly ServerShotEnvironment _environment;
        public List<WebJobBase> RunningJobs { get; set; }
        public List<IWebjobSessionPlugin> RunningPlugins { get; set; }

        public WebJobSession(ServerShotEnvironment environment)
        {
            _environment = environment;
            RunningJobs= new List<WebJobBase>();
            RunningPlugins = new List<IWebjobSessionPlugin>();
        }

        public void AddWebJob<T>() where T : WebJobBase
        {
            this.RunningJobs.Add(_environment.IOCContainer.Get<T>());
        }

        public void AddWebJob(WebJobBase webJob)
        {
            this.RunningJobs.Add(webJob);

            RunningPlugins.ForEach(x=>x.RegisterWebjob(webJob));
        }

        public void AttachPlugin(IWebjobSessionPlugin plugin)
        {
            RunningPlugins.Add(plugin);
        }
    }
}