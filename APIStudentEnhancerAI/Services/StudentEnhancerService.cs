using APIStudentEnhancerAI.Models;
namespace APIStudentEnhancerAI.Services
{
    public class StudentEnhancerService : IStudentEnhancerService
    {
        private readonly IOpenRouterService _llmService;
        private readonly ILogger<StudentEnhancerService> _logger;

        public StudentEnhancerService(
            IOpenRouterService llmService,
            ILogger<StudentEnhancerService> logger)
        {
            _llmService = llmService;
            _logger = logger;
        }

        public async Task<StudentEnhancerResponse> EnhanceStudentJourneyAsync(StudentEnhancerRequest request)
        {
            try
            {
                _logger.LogInformation("Processing enhancement request for goal: {Goal}", request.StudentGoal);

                // Call OpenRouter to generate study guide
                var enhancement = await _llmService.GenerateStudyGuideAsync(request.StudentGoal);

                var response = new StudentEnhancerResponse
                {
                    Enhancement = enhancement,
                    TokensUsed = EstimateTokens(enhancement),
                    ProcessedAt = DateTime.UtcNow,
                    FeatureType = "StudyGuideGeneration"
                };

                _logger.LogInformation("Enhancement completed. Tokens used: {Tokens}", response.TokensUsed);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EnhanceStudentJourneyAsync");
                throw;
            }
        }

        private int EstimateTokens(string text)
        {
            // Rough estimation: ~4 characters per token
            return (text?.Length ?? 0) / 4;
        }
    }
}