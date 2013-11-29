using System.Runtime.Serialization;

namespace FarFetched.Workflow.Demo
{
    [DataContract]
    public class Movie
    {
        [DataMember]
        public string MovieName { get; set; }
        [DataMember]    
        public double RottenTomatoesScore { get; set; }
        [DataMember]    
        public double MetaCriticScore { get; set; }
    }
}