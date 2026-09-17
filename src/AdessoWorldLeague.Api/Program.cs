using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using AdessoWorldLeague.Api.ErrorHandling;
using AdessoWorldLeague.Application;
using AdessoWorldLeague.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

        // Team names contain characters such as "İ", "ü" and "ç". The default encoder would escape them
        // into \uXXXX sequences, which is valid JSON but unreadable for a human checking the response.
        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
    });

// Problem documents (RFC 9457) for every error, produced by the handlers below.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Adesso World League API",
        Version = "v1",
        Description =
            "Draws the thirty-two teams of the Adesso World League into 4 or 8 groups, with at most one " +
            "team per country in any group, and stores who performed the draw.",
    });

    var xmlDocumentation = Path.Combine(
        AppContext.BaseDirectory,
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");

    if (File.Exists(xmlDocumentation))
    {
        options.IncludeXmlComments(xmlDocumentation);
    }
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Adesso World League API v1");

    // The API has a single purpose, so the documentation is the most useful landing page.
    options.RoutePrefix = "swagger";
});

app.MapControllers();

// /health also verifies the database; /health/live only says the process is up, which is what an
// orchestrator should restart on.
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

app.Run();

/// <summary>Entry point, exposed so the integration tests can host the API in-process.</summary>
public partial class Program;
