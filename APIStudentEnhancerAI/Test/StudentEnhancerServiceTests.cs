using Moq;
using NUnit.Framework;
using APIStudentEnhancerAI.Models;
using APIStudentEnhancerAI.Services;
namespace StudentJourneyAI.Tests
{
    [TestFixture]
    public class StudentEnhancerServiceTests
    {
        private Mock<IOpenRouterService> _mockLLMService;
        private Mock<ILogger<StudentEnhancerService>> _mockLogger;
        private StudentEnhancerService _service;

        [SetUp]
        public void Setup()
        {
            _mockLLMService = new Mock<IOpenRouterService>();
            _mockLogger = new Mock<ILogger<StudentEnhancerService>>();
            _service = new StudentEnhancerService(_mockLLMService.Object, _mockLogger.Object);
        }

        [Test]
        public async Task EnhanceStudentJourneyAsync_WithValidInput_ReturnsStructuredResponse()
        {
            // Arrange
            var request = new StudentEnhancerRequest
            {
                StudentGoal = "Prepare for Data Science exam"
            };

            var mockResponse = "Key concepts: Statistics, ML basics, Data handling. Study tips: Practice coding, review papers.";
            _mockLLMService.Setup(x => x.GenerateStudyGuideAsync(It.IsAny<string>()))
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _service.EnhanceStudentJourneyAsync(request);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Enhancement, Is.EqualTo(mockResponse));
            Assert.That(result.TokensUsed, Is.GreaterThan(0));
            Assert.That(result.FeatureType, Is.EqualTo("StudyGuideGeneration"));
            Assert.That(result.ProcessedAt, Is.LessThanOrEqualTo(DateTime.UtcNow));

            // Verify the LLM service was called
            _mockLLMService.Verify(x => x.GenerateStudyGuideAsync(request.StudentGoal), Times.Once);
        }

        [Test]
        public void EnhanceStudentJourneyAsync_WhenLLMThrowsException_PropagatesException()
        {
            // Arrange
            var request = new StudentEnhancerRequest { StudentGoal = "Test" };
            _mockLLMService.Setup(x => x.GenerateStudyGuideAsync(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("API error"));

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _service.EnhanceStudentJourneyAsync(request));
        }

        [Test]
        public async Task EnhanceStudentJourneyAsync_WithLongerResponse_CalculatesTokensCorrectly()
        {
            // Arrange
            var request = new StudentEnhancerRequest
            {
                StudentGoal = "Advanced topic"
            };

            var longResponse = new string('a', 400); // 400 characters = ~100 tokens
            _mockLLMService.Setup(x => x.GenerateStudyGuideAsync(It.IsAny<string>()))
                .ReturnsAsync(longResponse);

            // Act
            var result = await _service.EnhanceStudentJourneyAsync(request);

            // Assert
            Assert.That(result.TokensUsed, Is.EqualTo(100)); // 400 / 4 = 100
        }
    }
}

