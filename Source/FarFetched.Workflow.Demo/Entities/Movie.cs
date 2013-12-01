using System.Runtime.Serialization;

namespace Azure.Workflow.Demo
{
    [DataContract]
    public class Movie
    {
        [DataMember]
        public string Title { get; set; }
        [DataMember]    
        public double RottenTomatoesScore { get; set; }
        [DataMember]    
        public double MetaCriticScore { get; set; }
    }
}