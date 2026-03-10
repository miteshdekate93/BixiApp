using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BixiApi.Models;
using BixiApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace BixiApi.Tests.ControllerTests;

/// <summary>
/// Integration tests for StationsController using a real in-memory test server.
///
/// WebApplicationFactory boots the full ASP.NET Core pipeline — middleware,
/// routing, model binding, and JSON serialisation — just like production.
/// The only difference: we swap IBixiService for a mock so we control responses
/// and can force specific exceptions without making real HTTP calls.
///
/// IClassFixture<WebApplicationFactory<Program>> creates ONE base factory for
/// the class. Each test calls WithWebHostBuilder() to get its own child factory
/// with its own mock — this gives full test isolation.
/// </summary>
public class StationsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public StationsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Creates a test HTTP client with IBixiService replaced by the provided mock.
    /// Each call creates an independent in-memory test server.
    /// </summary>
    private HttpClient CreateClient(IBixiService service)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real BixiService registration.
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IBixiService));
                if (descriptor != null) services.Remove(descriptor);

                // Replace with our mock so we fully control what the service returns.
                services.AddScoped(_ => service);
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GetStations_MaxDistanceZero_ReturnsBadRequest()
    {
        // maxDistanceMeters=0 is not a valid distance — the controller rejects it
        // before calling the service. This validation lives in the controller layer.
        var client = CreateClient(new Mock<IBixiService>().Object);

        var response = await client.GetAsync("/api/stations?maxDistanceMeters=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetStations_MaxDistanceNegative_ReturnsBadRequest()
    {
        var client = CreateClient(new Mock<IBixiService>().Object);

        var response = await client.GetAsync("/api/stations?maxDistanceMeters=-1");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetStations_ServiceThrowsHttpRequestException_Returns502()
    {
        // Simulates the BIXI API being unreachable (network error, DNS failure).
        // GlobalExceptionMiddleware catches HttpRequestException → 502 Bad Gateway.
        var mockService = new Mock<IBixiService>();
        mockService
            .Setup(s => s.GetStationsAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("BIXI API unreachable"));

        var response = await CreateClient(mockService.Object).GetAsync("/api/stations");

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    [Fact]
    public async Task GetStations_ServiceThrowsTaskCanceledException_Returns504()
    {
        // Simulates a request that hit the HttpClient timeout.
        // GlobalExceptionMiddleware catches TaskCanceledException → 504 Gateway Timeout.
        var mockService = new Mock<IBixiService>();
        mockService
            .Setup(s => s.GetStationsAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("Timed out"));

        var response = await CreateClient(mockService.Object).GetAsync("/api/stations");

        Assert.Equal(HttpStatusCode.GatewayTimeout, response.StatusCode);
    }

    [Fact]
    public async Task GetStations_ValidCall_Returns200WithStationList()
    {
        // Happy path: service returns one station, controller wraps it in 200 OK.
        var stations = new List<StationResult>
        {
            new() { StationId = "1", Name = "Test Station", AvailableBikes = 5, DistanceMeters = 300, IsAvailable = true }
        };

        var mockService = new Mock<IBixiService>();
        mockService
            .Setup(s => s.GetStationsAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stations);

        var response = await CreateClient(mockService.Object).GetAsync("/api/stations");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<StationResult>>();
        Assert.Single(result!);
        Assert.Equal("Test Station", result![0].Name);
    }

    [Fact]
    public async Task GetStations_ErrorResponse_ContainsStatusAndTitleFields()
    {
        // RFC 7807 ProblemDetails requires at minimum "status" (int) and "title" (string).
        // We verify the shape of the error body, not just the HTTP status code.
        var mockService = new Mock<IBixiService>();
        mockService
            .Setup(s => s.GetStationsAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("down"));

        var response = await CreateClient(mockService.Object).GetAsync("/api/stations");

        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body);

        Assert.True(json.RootElement.TryGetProperty("status", out _), "Response body is missing 'status'");
        Assert.True(json.RootElement.TryGetProperty("title",  out _), "Response body is missing 'title'");
    }
}
