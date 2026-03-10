using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace BixiApi.Middleware;

/// <summary>
/// Catches every unhandled exception in the request pipeline and returns a
/// structured RFC 7807 ProblemDetails JSON response.
///
/// Why middleware for this? Centralising exception handling here means
/// controllers and services can throw freely without any try/catch blocks.
/// The middleware translates each exception type into the right HTTP status
/// and a safe error message — stack traces and internal details are never exposed.
///
/// Why RFC 7807 ProblemDetails? It is the standard format for HTTP error responses.
/// Clients (and developers using tools like Postman) know exactly what fields to
/// expect: "status" (int) and "title" (string).
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log every exception with method + path so it is easy to find in logs.
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await WriteProblemResponseAsync(context, ex);
        }
    }

    private static async Task WriteProblemResponseAsync(HttpContext context, Exception ex)
    {
        // Map each exception type to the most appropriate HTTP status and a
        // user-friendly title. The titles are intentionally vague — we do not
        // leak internal error messages to clients.
        var (statusCode, title) = ex switch
        {
            // The BIXI API was unreachable (network failure, DNS error, etc.)
            HttpRequestException  => (StatusCodes.Status502BadGateway,        "Unable to reach BIXI API"),

            // The request timed out (HttpClient timeout) or was cancelled by the client
            TaskCanceledException => (StatusCodes.Status504GatewayTimeout,    "BIXI API timed out"),

            // The BIXI API returned JSON we couldn't parse (unexpected schema change)
            JsonException         => (StatusCodes.Status502BadGateway,        "Unexpected BIXI API response format"),

            // Bad input that slipped past controller-level validation
            ArgumentException     => (StatusCodes.Status400BadRequest,        "Invalid request parameters"),

            // Catch-all — keeps the API from returning raw 500 stack traces
            _                     => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode  = statusCode;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title  = title
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}
