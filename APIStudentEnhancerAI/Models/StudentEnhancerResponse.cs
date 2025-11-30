namespace APIStudentEnhancerAI.Models
{
    public class StudentEnhancerResponse
    {
        public string Enhancement { get; set; }
        public int TokensUsed { get; set; }
        public DateTime ProcessedAt { get; set; }
        public string FeatureType { get; set; }
    }
}
