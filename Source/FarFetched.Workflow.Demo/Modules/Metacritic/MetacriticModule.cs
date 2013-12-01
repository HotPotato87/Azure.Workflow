using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Workflow.Core;
using Azure.Workflow.Core.Enums;
using Azure.Workflow.Core.Implementation;
using Azure.Workflow.Core.Architecture;
using Azure.Workflow.Core.Plugins.Alerts;

namespace Azure.Workflow.Demo
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
