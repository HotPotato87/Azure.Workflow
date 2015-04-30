using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using ServerShot.Framework.Core.Plugins;

namespace Servershot.Framework.Entities.WebJob
{
    public abstract class JibJobQueueBase
    {
        internal List<IJibJobSessionPlugin> Plugins = new List<IJibJobSessionPlugin>();

        protected virtual IEnumerable<ITransientJibJob> GetTransientJibJobs()
        {
            return new List<ITransientJibJob>();
        }

        public async Task ProcessJob([QueueTrigger("jibjobs")] string jobName, TextWriter log)
        {
            //standard logger
            if (!Plugins.Any(x => x is JibJobLogger))
            {
                Plugins.Add(new JibJobLogger(log));    
            }

            log.WriteLine("[JibJob]Recieved message : " + jobName);

            ITransientJibJob job = GetTransientJibJobs().SingleOrDefault(x => x.Name == jobName);

            Plugins.ForEach(x=>x.RegisterWebjob(job));

            log.WriteLine("[JibJob]Resolved message to transient job : " + job.Name);

            await job.Triggered();

            log.WriteLine("[JibJob]Finished : " + job.Name);
        }
    }
}
