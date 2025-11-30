namespace APIStudentEnhancerAI.Services
{
    public interface IOpenRouterService
    {
        Task<string> GenerateStudyGuideAsync(string topic);
    }
}
