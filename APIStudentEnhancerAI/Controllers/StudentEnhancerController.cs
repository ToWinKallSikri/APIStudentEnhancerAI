using APIStudentEnhancerAI.Abstractions.Services;
using APIStudentEnhancerAI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace StudentJourneyAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentEnhancerController : ControllerBase
    {
        private readonly IStudentEnhancerService _service;
        private readonly ILogger<StudentEnhancerController> _logger;

        public StudentEnhancerController(
            IStudentEnhancerService service,
            ILogger<StudentEnhancerController> logger)
        {
            _service = service;
            _logger = logger;
        }
        // Simple health check endpoint to verify API is running

        [HttpGet("health")]
        public IActionResult GetHealth()
        {
            _logger.LogInformation("Health check requested");
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }

        // AI enhancement feature endpoint
        [HttpPost("feature")]
        public async Task<IActionResult> EnhanceStudentJourney([FromBody] StudentEnhancerRequest request)
        {
            // Wrapping the logic in try-catch to handle potential errors gracefully
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid request model");
                    return BadRequest(new { error = "Invalid request", details = ModelState });
                }

                var response = await _service.EnhanceStudentJourneyAsync(request);
                return Ok(response);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling LLM service");
                return StatusCode(503, new { error = "LLM service unavailable", details = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Configuration error");
                return StatusCode(500, new { error = "Service configuration error", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in EnhanceStudentJourney");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }
    }
}
