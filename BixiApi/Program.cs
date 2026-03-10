using BixiApi.Middleware;
using BixiApi.Models;
using BixiApi.Services;
using BixiApi.Services.Interfaces;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ─────────────────────────────────────────────────────────────
// Binds the "Workleap" section of appsettings.json to WorkleapSettings.
// Services inject it via IOptions<WorkleapSettings>.
builder.Services.Configure<WorkleapSettings>(
    builder.Configuration.GetSection("Workleap"));

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IBixiService, BixiService>();

// Singleton is fine for DistanceService — it is stateless (pure math).
builder.Services.AddSingleton<IDistanceService, DistanceService>();

// Register a named HttpClient "bixi" with timeout from configuration.
// IHttpClientFactory manages the underlying socket pool, which prevents
// socket exhaustion when many requests are made in parallel.
builder.Services.AddHttpClient("bixi", (sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<WorkleapSettings>>().Value;
    client.Timeout = TimeSpan.FromSeconds(settings.HttpTimeoutSeconds);
});

// ── CORS ──────────────────────────────────────────────────────────────────────
// Allow the React dev server (port 3000) to call this API.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

// ── Pipeline ──────────────────────────────────────────────────────────────────
var app = builder.Build();

// GlobalExceptionMiddleware must be first so it catches errors from all later middleware.
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors();
app.MapControllers();

app.Run();

// Makes the Program class visible to the integration test project (WebApplicationFactory).
public partial class Program { }
