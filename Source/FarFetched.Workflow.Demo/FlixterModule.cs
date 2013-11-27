using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core;
using FarFetched.AzureWorkflow.Core.Architecture;

namespace FarFetched.Workflow.Demo
{
    public class FlixterMovieModule : QueueProcessingWorkflowModule<MovieJsonObject>
    {
        public override string QueueName
        {
            get { return "flixter"; }
        }

        public override async Task ProcessAsync(IEnumerable<MovieJsonObject> queueCollection)
        {
            foreach (var queueItem in queueCollection)
            {
                this.SendTo(typeof(RottenTomatoesModule), queueItem);
            }
        }
    }
}
