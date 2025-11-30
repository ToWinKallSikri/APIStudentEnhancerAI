using System.ComponentModel.DataAnnotations;

namespace APIStudentEnhancerAI.Models
{
    public class StudentEnhancerRequest
    {
        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string StudentGoal { get; set; }

        [StringLength(1000)]
        public string Context { get; set; }
    }
}
