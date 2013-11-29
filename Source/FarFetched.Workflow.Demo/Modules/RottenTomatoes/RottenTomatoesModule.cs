using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FarFetched.AzureWorkflow.Core;
using FarFetched.AzureWorkflow.Core.Enums;
using FarFetched.Workflow.Demo.Modules.RottenTomatoes;

namespace FarFetched.Workflow.Demo
{
    public class RottenTomatoesModule : InitialWorkflowModule<MovieJsonObject>
    {
        public RottenTomatoesModule() : base()
        {
            
        }

        public override async Task OnStart()
        {
            //get movies currently in the cinema
            string url = @"http://api.rottentomatoes.com/api/public/v1.0/lists/movies/in_theaters.json?page_limit=16&page=1&country=us&apikey={0}";
            string urlWithKey = String.Format(url, APISettings.Default.RottenTomatoesKey);


            HttpClient client = new HttpClient();
            var response = await client.GetAsync(urlWithKey);
            

        }
    }
}
