using APIStudentEnhancerAI.Models;

namespace APIStudentEnhancerAI.Services
{
    public interface IStudentEnhancerService
    {
        Task<StudentEnhancerResponse> EnhanceStudentJourneyAsync(StudentEnhancerRequest request);
    }
}
