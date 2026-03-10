using BixiApi.Middleware;
using BixiApi.Models;
using BixiApi.Services;
using BixiApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<WorkleapSettings>(
    builder.Configuration.GetSection("Workleap"));

builder.Services.AddScoped<IBixiService, BixiService>();
builder.Services.AddSingleton<IDistanceService, DistanceService>();

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

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors();
app.MapControllers();

app.Run();
