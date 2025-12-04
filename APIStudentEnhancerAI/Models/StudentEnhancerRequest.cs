using System.ComponentModel.DataAnnotations;

namespace APIStudentEnhancerAI.Models
{
    public class StudentEnhancerRequest
    {
        // The study topic that the student wants to dive into, with validation attributes that ensure that
        // the topic provided is at least 10 carachters long (to avoid too generic topics) and at most 500 characters
        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string LearningGoal { get; set; }

        // Optional context that the student can provide to explain better their background or specific needs
        [StringLength(1000)]
        public string Context { get; set; }
    }
}
