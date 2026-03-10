using System.Text.Json;
using BixiApi.Models;
using BixiApi.Models.Gbfs;
using Xunit;

namespace BixiApi.Tests.ContractTests;

/// <summary>
/// Verifies that our C# models correctly deserialize GBFS JSON payloads.
///
/// Why test deserialization separately? The BIXI API is an external service we
/// don't control. If BIXI ever renames a field (e.g. "station_id" → "id"),
/// these tests fail immediately and pinpoint exactly what broke — without
/// needing to spin up a server or make real HTTP calls.
///
/// These tests act as a contract between our code and the BIXI API format.
/// They use raw hardcoded JSON strings that mirror the real API shape.
/// </summary>
public class GbfsDeserializationTests
{
    [Fact]
    public void StationInfo_DeserializesCorrectly()
    {
        // The [JsonPropertyName] attributes on StationInfo map snake_case JSON
        // keys ("station_id", "lat") to PascalCase C# properties (StationId, Lat).
        // This test verifies that mapping is correct end-to-end.
        const string json = """
            {
              "data": {
                "stations": [
                  { "station_id": "1", "name": "Métro Laurier", "lat": 45.5275, "lon": -73.5858 }
                ]
              }
            }
            """;

        var result  = JsonSerializer.Deserialize<StationInfoResponse>(json)!;
        var station = result.Data.Stations.Single();

        Assert.Equal("1",             station.StationId);
        Assert.Equal("Métro Laurier", station.Name);
        Assert.Equal(45.5275,         station.Lat);
        Assert.Equal(-73.5858,        station.Lon);
    }

    [Fact]
    public void StationStatus_DeserializesCorrectly()
    {
        // Verifies that "num_bikes_available" (snake_case) maps to NumBikesAvailable.
        const string json = """
            {
              "data": {
                "stations": [
                  { "station_id": "1", "num_bikes_available": 5 }
                ]
              }
            }
            """;

        var result = JsonSerializer.Deserialize<StationStatusResponse>(json)!;
        var status = result.Data.Stations.Single();

        Assert.Equal("1", status.StationId);
        Assert.Equal(5,   status.NumBikesAvailable);
    }

    [Fact]
    public void StationStatus_ZeroBikes_RawValueIsZero()
    {
        // BixiService sets IsAvailable = (NumBikesAvailable > 0).
        // This test confirms the raw JSON value 0 is preserved correctly after
        // deserialization, so the availability logic downstream will be correct.
        const string json = """
            {
              "data": {
                "stations": [
                  { "station_id": "1", "num_bikes_available": 0 }
                ]
              }
            }
            """;

        var result = JsonSerializer.Deserialize<StationStatusResponse>(json)!;
        var status = result.Data.Stations.Single();

        Assert.Equal(0, status.NumBikesAvailable);
        Assert.False(status.NumBikesAvailable > 0); // will map to IsAvailable = false
    }

    [Fact]
    public void StationStatus_ThreeBikes_RawValueIsThree()
    {
        const string json = """
            {
              "data": {
                "stations": [
                  { "station_id": "1", "num_bikes_available": 3 }
                ]
              }
            }
            """;

        var result = JsonSerializer.Deserialize<StationStatusResponse>(json)!;
        var status = result.Data.Stations.Single();

        Assert.Equal(3, status.NumBikesAvailable);
        Assert.True(status.NumBikesAvailable > 0); // will map to IsAvailable = true
    }

    [Fact]
    public void ExtraUnknownFields_DoNotThrow()
    {
        // System.Text.Json silently ignores JSON fields that have no matching
        // C# property. This is the default behaviour (no special config needed).
        // This test ensures forward compatibility: if BIXI adds new fields to
        // their API response, our app will not crash.
        const string json = """
            {
              "data": {
                "stations": [
                  {
                    "station_id": "1",
                    "num_bikes_available": 3,
                    "brand_new_future_field": "some value",
                    "another_unknown_field": 42
                  }
                ]
              }
            }
            """;

        // Should not throw JsonException despite the unknown fields.
        var result = JsonSerializer.Deserialize<StationStatusResponse>(json)!;
        Assert.Single(result.Data.Stations);
    }
}
