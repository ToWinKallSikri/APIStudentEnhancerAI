using Moq;
using NUnit.Framework;
using APIStudentEnhancerAI.Models;
using APIStudentEnhancerAI.Services;
using APIStudentEnhancerAI.Abstractions.Services;
namespace StudentJourneyAI.Tests
{
    [TestFixture]
    public class StudentEnhancerServiceTests
    {

        private Mock<IOpenRouterService> _mockLLMService;
        private Mock<ILogger<StudentEnhancerService>> _mockLogger;
        private StudentEnhancerService _service;

        // Initialization to give setup the test thorugh Moq library to use classes as mock objects
        [SetUp]
        public void Setup()
        {
            _mockLLMService = new Mock<IOpenRouterService>();
            _mockLogger = new Mock<ILogger<StudentEnhancerService>>();
            _service = new StudentEnhancerService(_mockLLMService.Object, _mockLogger.Object);
        }

        // First test to check the logic of the method to collect the answer generated from the LLM
        [Test]
        public async Task EnhanceStudentJourneyAsync_WithValidInput_ReturnsStructuredResponse()
        {
            // Arrange
            var request = new StudentEnhancerRequest
            {
                LearningGoal = "Prepare for Data Science exam",
                Context = "Focus on statistics, machine learning basics, and data handling."
            };

            var mockResponse = "Key concepts: Statistics, ML basics, Data handling. Study tips: Practice coding, review papers.";
            _mockLLMService.Setup(x => x.GenerateStudyGuideAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _service.EnhanceStudentJourneyAsync(request);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Enhancement, Is.EqualTo(mockResponse));
            Assert.That(result.FeatureType, Is.EqualTo("StudyGuideGeneration"));
            Assert.That(result.ProcessedAt, Is.LessThanOrEqualTo(DateTime.UtcNow));

            // Verify the LLM service was called
            _mockLLMService.Verify(x => x.GenerateStudyGuideAsync(request.LearningGoal, request.Context), Times.Once);
        }

        // Short string error handling test
        // Tests to check the exception handling logic (in this case checking an HttpRequestException)
        // Less than 10 characters provided as topic
        [Test]
        public void EnhanceStudentJourneyAsync_WhenLLMThrowsExceptionShortTopic()
        {
            var request = new StudentEnhancerRequest { 
            LearningGoal = "Test",
            Context = "context"
            };
            _mockLLMService.Setup(x => x.GenerateStudyGuideAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("API error"));

            // Act and Assert
            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _service.EnhanceStudentJourneyAsync(request));
        }

        // Long string error handling test
        // Tests to check the exception handling logic (in this case checking an HttpRequestException)
        // More than 500 characters provided as topic
        [Test]
        public void EnhanceStudentJourneyAsync_WhenLLMThrowsExceptionLongTopic()
        {
            var request = new StudentEnhancerRequest
            {
                LearningGoal = new string('a', 501),
                Context = "context"
            };
            _mockLLMService.Setup(x => x.GenerateStudyGuideAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("API error"));

            // Act and Assert
            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _service.EnhanceStudentJourneyAsync(request));
        }

        // Long string error handling test
        // Tests to check the exception handling logic (in this case checking an HttpRequestException)
        // More than 1000 characters provided as context
        [Test]
        public void EnhanceStudentJourneyAsync_WhenLLMThrowsExceptionLongContext()
        {
            var request = new StudentEnhancerRequest
            {
                LearningGoal = "Business Intelligence",
                Context = new string('a', 501)
            };
            _mockLLMService.Setup(x => x.GenerateStudyGuideAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("API error"));

            // Act and Assert
            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _service.EnhanceStudentJourneyAsync(request));
        }

    }
}

