using APIStudentEnhancerAI.Models;

namespace APIStudentEnhancerAI.Abstractions.Services
{
    public interface IStudentEnhancerService
    {
        Task<StudentEnhancerResponse> EnhanceStudentJourneyAsync(StudentEnhancerRequest request);
    }
}
