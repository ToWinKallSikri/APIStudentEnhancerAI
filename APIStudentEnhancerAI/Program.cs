using APIStudentEnhancerAI.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddScoped<IStudentEnhancerService, StudentEnhancerService>();
builder.Services.AddHttpClient<IOpenRouterService, OpenRouterService>();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "API Student Enhancer",
        Version = "v1",
        Description = "AI-enhanced backend service for Lancaster University students"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ========== SIMPLE API KEY MIDDLEWARE ==========
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? "";
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

    // Only protect /api/studentenhancement/feature
    if (path == "/api/studentenhancer/feature")
    {
        if (!context.Request.Headers.TryGetValue("X-API-Key", out var apiKey))
        {
            logger.LogWarning("Request without API key to {Path}", path);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "API key required. Include header: X-API-Key: sk-student-journey-demo-key" });
            return;
        }

        const string ValidKey = "sk-student-journey-demo-key";
        if (apiKey != ValidKey)
        {
            logger.LogWarning("Invalid API key attempt");
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid API key" });
            return;
        }
    }

    await next();
});


app.MapControllers();

app.Run();
