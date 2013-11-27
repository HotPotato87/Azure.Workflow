using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core;

namespace FarFetched.Workflow.Demo
{
    public class RottenTomatoesModule : QueueProcessingWorkflowModule<MovieJsonObject>
    {
        public RottenTomatoesModule() : base()
        {
            
        }

        public override Task ProcessAsync(IEnumerable<MovieJsonObject> queueCollection)
        {
            throw new NotImplementedException();
        }
    }
}
