using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using APIStudentEnhancerAI.Models;
using APIStudentEnhancerAI.Abstractions.Services;
using StudentJourneyAI.Controllers;

namespace APIStudentEnhancerAI.Tests
{
    [TestFixture]
    public class StudentEnhancerControllerTests
    {
        private Mock<IStudentEnhancerService> _mockStudentEnhancerService;
        private Mock<ILogger<StudentEnhancerController>> _mockLogger;
        private StudentEnhancerController _controller;

        [SetUp]
        public void Setup()
        {
            _mockStudentEnhancerService = new Mock<IStudentEnhancerService>();
            _mockLogger = new Mock<ILogger<StudentEnhancerController>>();
            _controller = new StudentEnhancerController(_mockStudentEnhancerService.Object, _mockLogger.Object);
        }

        // Test for the controller health check API call logic
        [Test]
        public void GetHealth_ReturnsOk()
        {
            // Act
            var result = _controller.GetHealth();

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
        }
        
        // Test for the controller AI feature API call logic
        [Test]
        public async Task EnhanceStudentJourney_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new StudentEnhancerRequest
            {
                LearningGoal = "Learn data science"
            };

            var mockResponse = new StudentEnhancerResponse
            {
                Enhancement = "Study guide...",
                ProcessedAt = DateTime.UtcNow,
                FeatureType = "StudyGuideGeneration"
            };

            _mockStudentEnhancerService.Setup(x => x.EnhanceStudentJourneyAsync(It.IsAny<StudentEnhancerRequest>()))
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _controller.EnhanceStudentJourney(request);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(mockResponse));
        }
    }
}