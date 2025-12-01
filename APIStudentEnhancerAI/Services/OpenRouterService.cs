using APIStudentEnhancerAI.Abstractions.Services;
using System.Text.Json;

namespace APIStudentEnhancerAI.Services
{
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

        public async Task<string> GenerateStudyGuideAsync(string topic)
        {
            try
            {
                var apiKey = _configuration["LLM:ApiKey"];
                var model = _configuration["LLM:Model"];

                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogError("OpenRouter API key not configured");
                    throw new InvalidOperationException("LLM API key not configured");
                }

                var prompt = $@"You are an academic advisor for Lancaster University students. 
                            Create a concise, practical study guide for the following topic:

                            Topic: {topic}

                            Include:
                            1. Key concepts (3-5 bullet points)
                            2. Study tips (2-3 actionable steps)
                            3. Recommended resources (2-3 suggestions)

                            Keep it under 300 words, focused, and practical.";

                var requestPayload = new
                {
                    model = model,
                    messages = new[]
                    {
                        new { role = "system", content = "You are a helpful academic advisor for Lancaster University students." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.7,
                    max_tokens = 500
                };

                var jsonContent = JsonSerializer.Serialize(requestPayload);
                _logger.LogInformation("Request payload: {Payload}", jsonContent);

                var content = new StringContent(
                    jsonContent,
                    System.Text.Encoding.UTF8,
                    "application/json");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post,
                    "https://openrouter.ai/api/v1/chat/completions");

                httpRequest.Content = content;

                // Aggiungi gli headers (SENZA User-Agent che può causare problemi)
                httpRequest.Headers.Add("Authorization", $"Bearer {apiKey}");
                httpRequest.Headers.Add("HTTP-Referer", "https://localhost:7088");
                httpRequest.Headers.Add("X-Title", "StudentJourneyAI");

                _logger.LogInformation("Calling OpenRouter API with model: {Model}", model);

                // Invia la richiesta
                var response = await _httpClient.SendAsync(httpRequest);
                var responseText = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("OpenRouter response status: {StatusCode}", response.StatusCode);
                _logger.LogInformation("OpenRouter response body: {Body}", responseText);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("OpenRouter error {StatusCode}: {Response}", response.StatusCode, responseText);
                    throw new HttpRequestException($"OpenRouter API error: {response.StatusCode} - {responseText}");
                }

                var jsonResponse = JsonDocument.Parse(responseText);

                // Naviga alla risposta
                if (!jsonResponse.RootElement.TryGetProperty("choices", out var choices)
                    || choices.GetArrayLength() == 0)
                {
                    _logger.LogError("Invalid OpenRouter response format: {Response}", responseText);
                    throw new InvalidOperationException("Invalid response format from OpenRouter");
                }

                var message = choices[0].GetProperty("message");
                var generatedText = message.GetProperty("content").GetString();

                _logger.LogInformation("Successfully generated study guide. Length: {Length} chars", generatedText?.Length ?? 0);

                return generatedText ?? "Unable to generate study guide";
            }
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