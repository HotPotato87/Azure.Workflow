using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Workflow.Core;
using Azure.Workflow.Core.Implementation;
using Azure.Workflow.Demo.Modules.RottenTomatoes;
using Azure.Workflow.Core.Enums;
using Newtonsoft.Json;

namespace Azure.Workflow.Demo
{
    public class RottenTomatoesModule : InitialWorkflowModule<Movie>
    {
        public RottenTomatoesModule(WorkflowModuleSettings settings = null) : base(settings)
        {
            
        }

        public override async Task OnStart()
        {
            //get movies currently in the cinema
            string url = @"http://api.rottentomatoes.com/api/public/v1.0/lists/movies/in_theaters.json?page_limit=16&page=1&country=us&apikey={0}";
            string urlWithKey = String.Format(url, DemoSettings.Default.RottenTomatoesKey);


            HttpClient client = new HttpClient();
            var response = await client.GetAsync(urlWithKey);
            var jsonString = await response.Content.ReadAsStringAsync();
            var rottenTomatoesObject = await JsonConvert.DeserializeObjectAsync<RottenTomatoesJsonObjects.RootObject>(jsonString);
            int addedToMetaCritic = 0;

            foreach (var movieJson in rottenTomatoesObject.movies)
            {
                Movie movie = new Movie()
                {
                    Title = movieJson.title,
                    RottenTomatoesScore = movieJson.ratings.critics_score
                };

                if (movieJson.ratings.critics_score > 60)
                {
                    base.SendTo(typeof(MetacriticModule), movie);
                    base.CategorizeResult("Fresh movies processed");
                    addedToMetaCritic++;
                }
                else
                {
                    base.CategorizeResult(
                        key: "Rotten Movies Discarded",
                        description: String.Format("{0}({1})", movieJson.title, movieJson.ratings.critics_score), 
                        countAsProcessed: false);
                }
                
            }

            this.LogMessage("Added {0} movies for processing", addedToMetaCritic);
        }
    }
}
