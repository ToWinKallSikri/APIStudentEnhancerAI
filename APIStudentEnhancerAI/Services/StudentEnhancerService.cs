using APIStudentEnhancerAI.Abstractions.Services;
using APIStudentEnhancerAI.Models;
namespace APIStudentEnhancerAI.Services
{
    public class StudentEnhancerService : IStudentEnhancerService
    {
        private readonly IOpenRouterService _llmService;
        private readonly ILogger<StudentEnhancerService> _logger;

        // OpenRouter Service injection through constructor
        public StudentEnhancerService(
            IOpenRouterService llmService,
            ILogger<StudentEnhancerService> logger)
        {
            _llmService = llmService;
            _logger = logger;
        }

        public async Task<StudentEnhancerResponse> EnhanceStudentJourneyAsync(StudentEnhancerRequest request)
        {
            // For a safer code execution everything is wrapped in a try-catch block
            try
            {
                _logger.LogInformation("Processing enhancement request for topic: {Goal}", request.LearningGoal);

                // Call OpenRouter to generate study guide giving it the student's goal that was provided as topic
                // in the request, along witht the pre-prompt defined in the OpenRouter serivice implementation
                // that explicitly asks the LLM to help the student as a Lancaster University student advisor.
                var enhancement = await _llmService.GenerateStudyGuideAsync(request.LearningGoal, request.Context);

                var response = new StudentEnhancerResponse
                {
                    Enhancement = enhancement,
                    ProcessedAt = DateTime.UtcNow,
                    FeatureType = "StudyGuideGeneration"
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EnhanceStudentJourneyAsync");
                throw;
            }
        }
    }
}