using System.Text.Json.Serialization;

namespace BixiApi.Models;

/// <summary>
/// Represents real-time availability from the GBFS station_status feed.
/// This feed is live — it updates every minute with current bike counts.
/// </summary>
public class StationStatus
{
    [JsonPropertyName("station_id")]
    public string StationId { get; set; } = string.Empty;

    [JsonPropertyName("num_bikes_available")]
    public int NumBikesAvailable { get; set; }
}
