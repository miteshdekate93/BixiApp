using System.Text.Json.Serialization;

namespace BixiApi.Models;

/// <summary>
/// Represents one station's static info from the GBFS station_information feed.
/// Fields use [JsonPropertyName] to map snake_case JSON keys to PascalCase C# properties.
/// </summary>
public class StationInfo
{
    [JsonPropertyName("station_id")]
    public string StationId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lon")]
    public double Lon { get; set; }
}
