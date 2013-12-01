using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core;
using FarFetched.AzureWorkflow.Core.Architecture;
using FarFetched.AzureWorkflow.Core.Enums;
using FarFetched.AzureWorkflow.Core.Implementation;
using FarFetched.AzureWorkflow.Core.Plugins.Alerts;

namespace FarFetched.Workflow.Demo
{
    public class MetacriticModule : QueueProcessingWorkflowModule<Movie>
    {
        public MetacriticModule(WorkflowModuleSettings settings) : base(settings)
        {
            
        }

        public override async Task ProcessAsync(IEnumerable<Movie> queueCollection)
        {
            foreach (var movie in queueCollection)
            {
                movie.MetaCriticScore = 5;
                base.CategorizeResult(ProcessingResult.Success);
            }
        }
    }
}
