using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using ServerShot.Framework.Core;
using ServerShot.Framework.Core.Implementation.IOC;
using ServerShot.Framework.Core.Interfaces;
using ServerShot.Framework.Core.Plugins;

namespace Servershot.Framework.Entities.WebJob
{
    public class JibJobQueueBase
    {
        private static JibJobSession _session;

        public virtual IEnumerable<Type> GetTransientJibJobs()
        {
            return new List<Type>();
        }

        public void SetupIOC()
        {
            IOC.Kernel = new NinjectIocContainer();
            OnSetupIOC(IOC.Kernel);
        }

        protected virtual void OnSetupIOC(IIocContainer container)
        {
            
        }

        public static async Task ProcessJob([QueueTrigger("jibjob")] string jobName, TextWriter log)
        {
            var session = _session ?? (_session = new JibJobSession());

            await session.ProcessJob(jobName, log);
        }
    }

    public class CustomTypeLocator : ITypeLocator
    {
        public IReadOnlyList<Type> GetTypes()
        {
            return new[]
            {
                typeof (JibJobQueueBase)
            };
        }
    }

    public class JibJobSession
    {
        internal List<IJibJobSessionPlugin> Plugins = new List<IJibJobSessionPlugin>();

        
        public async Task  ProcessJob(string jobName, TextWriter log)
        {
            log.WriteLine("[JibJob]Recieved message : " + jobName);

            var jibJobType = GetTransientJibJobs().SingleOrDefault(x => x.Name.ToLower() == jobName.ToLower());
            var job = IOC.Kernel.Get<ITransientJibJob>(jibJobType);

            //standard logger
            if (!Plugins.Any(x => x is JibJobLogger))
            {
                Plugins.Add(new JibJobLogger(log));
            }

            job.AttachPlugins(Plugins);

            Plugins.ForEach(x => x.RegisterWebjob(job));

            log.WriteLine("[JibJob]Resolved message to transient job : " + job.Name);

            await job.Triggered();

            log.WriteLine("[JibJob]Finished : " + job.Name);
        }

        private IEnumerable<Type> GetTransientJibJobs()
        {
            var execAssembly = Assembly.GetEntryAssembly();
            var callingType = execAssembly.GetTypes().SingleOrDefault(x => x.BaseType == typeof (JibJobQueueBase));
            dynamic type = Activator.CreateInstance(callingType);

            type.SetupIOC();

            return type.GetTransientJibJobs();
        }
    }
}
