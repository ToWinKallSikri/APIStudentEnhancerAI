# APIStudentEnhancerAI
A simple API solution with a `\GET` endpoint for a simple health check and a `\POST` endpoint to call an AI service to enhance the student journey at Lancaster University.
Given the size of the task I preferred opting for a simple solution that prioritizes a clean back-end design (.NET Core 8.0).

## How to run
After cloning the repository add an appsettings.json file in the solution to configure the LLM service:
``` json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "AllowedHosts": "*",
  "LLM": {
    "ApiKey": "INSERT-API-KEY-HERE",
    "Model": "tngtech/tng-r1t-chimera:free" <-- YOU CAN CHOOSE THE MODEL YOU LIKE
  }
}
```
On Visual Studio:
Once the setup has been completed, run the solution in debug mode with https for the Swagger GUI to show on <https://localhost:7088> and call the endpoints.
To run the tests, open a terminal and run the command `dotnet test` (be sure to be in the project directory).


## Logic
- The API calls get handled by the controllor that provides the requests and the responses through the endpoints.
- The endpoint <https://..../api/StudentEnhancer/health> is called through a GET request and as a response it provides a simple JSON containing: status, timestamp and the version of the API system. 
- The endpoint <https://..../api/StudentEnhancer/feature> is called through a POST request, it requires two parameters in the body (Raw): `LearningGoal` and `context`.
  - `LearningGoal`: is the topic that the student needs a study guide for. The call to the LLM is a pre-defined with just the "topic" to be passed when the method is called (OpenRouterService.cs)
  - `context`: it helps providing the LLM a broader view about the background of the student that is using this feature (e.g. MSc Student, Bachelor first year, etc.) in order to come up with a guide more suitable to the needs of the user.
  
    ```c#
     public async Task<string> GenerateStudyGuideAsync(string topic, string context = null)
    {
      // For a safer code execution everything is wrapped in a try-catch block
      try
      {
          var apiKey = _configuration["LLM:ApiKey"];
          var model = _configuration["LLM:Model"];
    
        [..]
         ..
         ..
          var contextSection = string.IsNullOrEmpty(context)
          ? ""
          : $"\n\nStudent Background/Context: {context}";
    
          var prompt = $@"You are an academic advisor for Lancaster University students. 
                      Create a concise, practical study guide for the following topic:
  
                      Topic: {topic}{contextSection}
  
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
              // Temperature set on 0.7 to have a nice balance in terms of creativity of the answer and unpredictability
              temperature = 0.7,
              max_tokens = 1000
          };
          .
          .
          .
          .
         [..]
    
      ```
As a response the LLM provides a study guide that will enhance the student journey at Lancaster University in JSON format.
The LLM that I used to record the demo was this model: tngtech/tng-r1t-chimera:free, available on [OpenRouter](https://openrouter.ai/tngtech/tng-r1t-chimera:free). This information, along with the api key is stored in the appsettings.json configuration file that I didn't upload in the repo (I know that for security reasons it's obvious, but I wanted to mention it for clarity). 

## Approach 
The approach that I adopted can be summarized in:
  - Clean back-end design (Granularity)
  - Simplicity
  - OOP

I don't know if a Minimal API approach would have been preferred over this but since my focus was having a clean back-end, I preferred designing the solution this way.

## Personal reflection and possible improvements
Due to time constraints (I'm currently attenting the first week of SCC.442, the Penetration Testing module) I didn't manage to implement the optional goals:
- Simple token-based authentication.
- Basic rate limiting (e.g. requests per minute).
- Minimal persistence (in-memory or lightweight DB like SQLite).
- Deployment to AWS or Azure (via Lambda, App Service, ECS, etc.).

Considering the size of the task I didn't want to implement a JWT Bearer authentication system, I tried using a OAuth token authorization, but couldn't manage to set it up on time.
Anyways, as improvements If I had the chance I would have used a SQL server (local) and configured the db context for the persistency of the token after the user authentication (It would also store the user credentials).
The AI feature that I implemented is really simple and usable, but my intial idea was to link the LLM service to a `n8n` agent that ran on local using `ollama3.2` that also ran on the local machine, all linked through a webhook. This could potentially be a personal assistant for Lancaster University students (named Lanky) that can save informations about the context of the student and provide reminders, study guides, help with stress, plan week days based on their time tables, hobbies etc. It could also help eating healthier suggesting fast and cheap cooking recipes after the student provides a budget for the week and alimentary limitations.
