using BixiApi.Models;
using BixiApi.Services;
using BixiApi.Services.Interfaces;
using BixiApi.Tests.Helpers;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BixiApi.Tests.ServiceTests;

/// <summary>
/// Tests BixiService's join and filtering logic using fully controlled inputs.
///
/// We mock two things so tests never touch the network:
///   1. IDistanceService → returns predetermined distances for known coordinates.
///   2. HttpClient (via FakeHttpMessageHandler) → returns hardcoded JSON instead
///      of calling the real BIXI API.
///
/// This isolates exactly what we want to test: the join logic (info ∩ status),
/// the distance filter, the inclusive boundary, and the ascending sort.
/// </summary>
public class FilteringTests
{
    // URLs used inside fake discovery JSON — the handler intercepts these.
    private const string DiscoveryUrl = "https://test.bixi.com/gbfs.json";
    private const string InfoUrl      = "https://test.bixi.com/station_information";
    private const string StatusUrl    = "https://test.bixi.com/station_status";

    // Station coordinates. The mock distance service maps these to fixed distances.
    private const double NearLat = 45.485, NearLon = -73.562; // → 300 m
    private const double MidLat  = 45.490, MidLon  = -73.562; // → 800 m
    private const double FarLat  = 45.500, FarLon  = -73.562; // → 1500 m

    // Discovery JSON — tells BixiService where to find the feed URLs.
    private const string DiscoveryJson = """
        {
          "data": {
            "en": {
              "feeds": [
                { "name": "station_information", "url": "https://test.bixi.com/station_information" },
                { "name": "station_status",      "url": "https://test.bixi.com/station_status" }
              ]
            }
          }
        }
        """;

    // Three stations at controlled distances: 300m, 800m, 1500m.
    private const string ThreeStationsInfoJson = """
        {
          "data": {
            "stations": [
              { "station_id": "near", "name": "Near Station", "lat": 45.485, "lon": -73.562 },
              { "station_id": "mid",  "name": "Mid Station",  "lat": 45.490, "lon": -73.562 },
              { "station_id": "far",  "name": "Far Station",  "lat": 45.500, "lon": -73.562 }
            ]
          }
        }
        """;

    private const string ThreeStationsStatusJson = """
        {
          "data": {
            "stations": [
              { "station_id": "near", "num_bikes_available": 3 },
              { "station_id": "mid",  "num_bikes_available": 2 },
              { "station_id": "far",  "num_bikes_available": 1 }
            ]
          }
        }
        """;

    /// <summary>
    /// Builds a BixiService wired to a fake HTTP handler and a mock distance calculator.
    /// This is the test factory — every test calls this to get a service with controlled inputs.
    /// </summary>
    private static BixiService CreateService(string infoJson, string statusJson, IDistanceService distanceService)
    {
        var handler = new FakeHttpMessageHandler(new Dictionary<string, string>
        {
            [DiscoveryUrl] = DiscoveryJson,
            [InfoUrl]      = infoJson,
            [StatusUrl]    = statusJson
        });

        var httpClient   = new HttpClient(handler);
        var mockFactory  = new Mock<IHttpClientFactory>();
        mockFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var settings = Options.Create(new WorkleapSettings
        {
            OfficeLat          = 45.48415789031987,
            OfficeLng          = -73.56216762891964,
            GbfsBaseUrl        = DiscoveryUrl,
            HttpTimeoutSeconds = 10
        });

        return new BixiService(mockFactory.Object, settings, distanceService);
    }

    /// <summary>
    /// Creates a mock IDistanceService that returns fixed distances
    /// based on the station's lat/lon (params 3 and 4).
    /// Office coords (params 1 and 2) are ignored with It.IsAny.
    /// </summary>
    private static IDistanceService CreateDistanceMock()
    {
        var mock = new Mock<IDistanceService>();
        mock.Setup(d => d.Calculate(It.IsAny<double>(), It.IsAny<double>(), NearLat, NearLon)).Returns(300);
        mock.Setup(d => d.Calculate(It.IsAny<double>(), It.IsAny<double>(), MidLat,  MidLon )).Returns(800);
        mock.Setup(d => d.Calculate(It.IsAny<double>(), It.IsAny<double>(), FarLat,  FarLon )).Returns(1500);
        return mock.Object;
    }

    [Fact]
    public async Task Filter_1000m_ReturnsOnlyStationsWithin1000m()
    {
        // Stations at 300m and 800m pass the filter; 1500m is excluded.
        var service = CreateService(ThreeStationsInfoJson, ThreeStationsStatusJson, CreateDistanceMock());

        var results = (await service.GetStationsAsync(1000, CancellationToken.None)).ToList();

        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.True(r.DistanceMeters <= 1000));
    }

    [Fact]
    public async Task NoFilter_ReturnsAllStations()
    {
        // Null maxDistance means no filtering — all 3 stations should come back.
        var service = CreateService(ThreeStationsInfoJson, ThreeStationsStatusJson, CreateDistanceMock());

        var results = (await service.GetStationsAsync(null, CancellationToken.None)).ToList();

        Assert.Equal(3, results.Count);
    }

    [Fact]
    public async Task Filter_StationAtExactBoundary_IsIncluded()
    {
        // The spec says the boundary is INCLUSIVE.
        // maxDistance = 800, station at exactly 800m → must be included.
        var service = CreateService(ThreeStationsInfoJson, ThreeStationsStatusJson, CreateDistanceMock());

        var results = (await service.GetStationsAsync(800, CancellationToken.None)).ToList();

        Assert.Equal(2, results.Count);
        Assert.Contains(results, r => r.DistanceMeters == 800);
    }

    [Fact]
    public async Task Filter_AllStationsBeyondDistance_ReturnsEmptyList()
    {
        // Filter of 100m — all stations (300m, 800m, 1500m) are excluded.
        // Must return an empty list, NOT null — null would crash the frontend.
        var service = CreateService(ThreeStationsInfoJson, ThreeStationsStatusJson, CreateDistanceMock());

        var results = (await service.GetStationsAsync(100, CancellationToken.None)).ToList();

        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public async Task Join_StationInInfoFeedOnly_IsExcluded()
    {
        // "ghost" exists in station_information but not in station_status.
        // The join silently drops it — incomplete data is not displayed.
        const string infoWithGhost = """
            {
              "data": {
                "stations": [
                  { "station_id": "near",  "name": "Near",  "lat": 45.485, "lon": -73.562 },
                  { "station_id": "ghost", "name": "Ghost", "lat": 45.490, "lon": -73.562 }
                ]
              }
            }
            """;

        const string statusWithoutGhost = """
            {
              "data": {
                "stations": [
                  { "station_id": "near", "num_bikes_available": 3 }
                ]
              }
            }
            """;

        var service = CreateService(infoWithGhost, statusWithoutGhost, CreateDistanceMock());

        var results = (await service.GetStationsAsync(null, CancellationToken.None)).ToList();

        Assert.Single(results);
        Assert.Equal("near", results[0].StationId);
    }

    [Fact]
    public async Task Join_StationInStatusFeedOnly_IsExcluded()
    {
        // "ghost" exists in station_status but not in station_information.
        // Because we iterate over info stations, this one is never reached.
        const string infoWithoutGhost = """
            {
              "data": {
                "stations": [
                  { "station_id": "near", "name": "Near", "lat": 45.485, "lon": -73.562 }
                ]
              }
            }
            """;

        const string statusWithGhost = """
            {
              "data": {
                "stations": [
                  { "station_id": "near",  "num_bikes_available": 3 },
                  { "station_id": "ghost", "num_bikes_available": 5 }
                ]
              }
            }
            """;

        var service = CreateService(infoWithoutGhost, statusWithGhost, CreateDistanceMock());

        var results = (await service.GetStationsAsync(null, CancellationToken.None)).ToList();

        Assert.Single(results);
        Assert.Equal("near", results[0].StationId);
    }

    [Fact]
    public async Task Results_AreSortedByDistanceAscending()
    {
        // Even if the info feed lists stations in a different order, the service
        // must always return them sorted closest-first. The frontend relies on this.
        var service = CreateService(ThreeStationsInfoJson, ThreeStationsStatusJson, CreateDistanceMock());

        var results = (await service.GetStationsAsync(null, CancellationToken.None)).ToList();

        Assert.Equal(3,    results.Count);
        Assert.Equal(300,  results[0].DistanceMeters);
        Assert.Equal(800,  results[1].DistanceMeters);
        Assert.Equal(1500, results[2].DistanceMeters);
    }
}
