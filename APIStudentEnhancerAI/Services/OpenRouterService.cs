using APIStudentEnhancerAI.Abstractions.Services;
using System.Text.Json;

namespace APIStudentEnhancerAI.Services
{
    // Service class to enable the AI enhancement feauture using OpenRouter API (OpenRouter was chosen because it provides
    // access to free API calls for LLM models)
    public class OpenRouterService : IOpenRouterService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OpenRouterService> _logger;

        public OpenRouterService(HttpClient httpClient, IConfiguration configuration, ILogger<OpenRouterService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GenerateStudyGuideAsync(string topic, string context = null)
        {
            // For a safer code execution everything is wrapped in a try-catch block
            try
            {
                var apiKey = _configuration["LLM:ApiKey"];
                var model = _configuration["LLM:Model"];

                // API key validation, for privacy and security reasons it is sotred in the appsettings.json file
                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogError("OpenRouter API key not configured");
                    throw new InvalidOperationException("LLM API key not configured");
                }

                var contextSection = string.IsNullOrEmpty(context)
                ? ""
                : $"\n\nStudent Background/Context: {context}";

                // Pre-promt that needs just the topic in the API call to make the AI generate a study guide impersonating
                // an academic advisor for Lancaster University students
                var prompt = $@"You are an academic advisor for Lancaster University students. 
                            Create a concise, practical study guide for the following topic:

                            Topic and context: {topic}{contextSection}

                            Include:
                            1. Key concepts (3-5 bullet points)
                            2. Study tips (2-3 actionable steps)
                            3. Recommended resources (2-3 suggestions)

                            Keep it under 300 words, focused, and practical.";

                var requestPayload = new
                {
                    model = model,
                    // Giving more context to the AI with system and user roles to imporve the quality of the response
                    messages = new[]
                    {
                        new { role = "system", content = "You are a helpful academic advisor for Lancaster University students." },
                        new { role = "user", content = prompt }
                    },
                    // Temperature set on 0.7 to have a nice balance in terms of creativity of the answer and unpredictability
                    temperature = 0.7,
                    // Tokens limited to 1000 to esnure that the generated answr is concise
                    max_tokens = 1000
                };

                var jsonContent = JsonSerializer.Serialize(requestPayload);
                _logger.LogInformation("Request payload: {Payload}", jsonContent);

                var content = new StringContent(
                    jsonContent,
                    System.Text.Encoding.UTF8,
                    "application/json");
                // OpenRouter API POST request header setup
                var httpRequest = new HttpRequestMessage(HttpMethod.Post,
                    "https://openrouter.ai/api/v1/chat/completions");

                // Http POST request configuration for the OpenRouter API call
                httpRequest.Content = content;
                httpRequest.Headers.Add("Authorization", $"Bearer {apiKey}");
                httpRequest.Headers.Add("HTTP-Referer", "https://localhost:7088");
                httpRequest.Headers.Add("X-Title", "StudentJourneyAI");

                _logger.LogInformation("Calling OpenRouter API with model: {Model}", model);

                // Await the answer from OpenRouter
                var response = await _httpClient.SendAsync(httpRequest);
                var responseText = await response.Content.ReadAsStringAsync();
                // Logging as required
                _logger.LogInformation("OpenRouter response status: {StatusCode}", response.StatusCode);
                _logger.LogInformation("OpenRouter response body: {Body}", responseText);

                // Error handling for the API call
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("OpenRouter error {StatusCode}: {Response}", response.StatusCode, responseText);
                    throw new HttpRequestException($"OpenRouter API error: {response.StatusCode} - {responseText}");
                }

                var jsonResponse = JsonDocument.Parse(responseText);

                // Output validation
                if (!jsonResponse.RootElement.TryGetProperty("choices", out var choices)
                    || choices.GetArrayLength() == 0)
                {
                    _logger.LogError("Invalid OpenRouter response format: {Response}", responseText);
                    throw new InvalidOperationException("Invalid response format from OpenRouter");
                }

                // Json response extraction
                var message = choices[0].GetProperty("message");
                var generatedText = message.GetProperty("content").GetString();

                _logger.LogInformation("Successfully generated study guide. Length: {Length} chars", generatedText?.Length ?? 0);

                // Return the generated study guide or a default message if null
                return generatedText ?? "Unable to generate study guide";
            }
            // Exception for http request errors or any other general errors while calling the service 
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling OpenRouter");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling OpenRouter");
                throw;
            }
        }
    }
}