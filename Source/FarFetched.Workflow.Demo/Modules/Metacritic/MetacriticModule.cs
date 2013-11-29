using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core;
using FarFetched.AzureWorkflow.Core.Architecture;
using FarFetched.AzureWorkflow.Core.Enums;

namespace FarFetched.Workflow.Demo
{
    public class MetacriticModule : QueueProcessingWorkflowModule<Movie>
    {
        public override string QueueName
        {
            get { return "metacritic"; }
        }

        public override async Task ProcessAsync(IEnumerable<Movie> queueCollection)
        {
            foreach (var movie in queueCollection)
            {
                movie.MetaCriticScore = 5;
                base.RaiseProcessed(ProcessingResult.Success);
            }
        }
    }
}
