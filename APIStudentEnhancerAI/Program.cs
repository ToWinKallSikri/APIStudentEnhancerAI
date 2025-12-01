using APIStudentEnhancerAI.Abstractions.Services;
using APIStudentEnhancerAI.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
