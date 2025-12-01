namespace APIStudentEnhancerAI.Abstractions.Services
{
    public interface IOpenRouterService
    {
        Task<string> GenerateStudyGuideAsync(string topic);
    }
}
