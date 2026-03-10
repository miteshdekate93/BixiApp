using System.Text.Json.Serialization;

namespace BixiApi.Models.Gbfs;

// ─── Discovery endpoint (gbfs.json) ──────────────────────────────────────────
// The GBFS root file is the stable entry point for the API.
// It lists all available feeds and their URLs under each locale (e.g. "en", "fr").
// We fetch this first so we never hardcode downstream feed URLs.

/// <summary>
/// Root response from the GBFS discovery endpoint.
/// "data" is a dictionary keyed by locale code (e.g. "en", "fr").
/// </summary>
public class GbfsRoot
{
    [JsonPropertyName("data")]
    public Dictionary<string, GbfsFeedList> Data { get; set; } = new();
}

/// <summary>All feeds available for one locale.</summary>
public class GbfsFeedList
{
    [JsonPropertyName("feeds")]
    public List<GbfsFeed> Feeds { get; set; } = new();
}

/// <summary>
/// A single named feed entry — for example:
/// { "name": "station_information", "url": "https://gbfs.velobixi.com/gbfs/en/station_information.json" }
/// </summary>
public class GbfsFeed
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}

// ─── station_information feed ─────────────────────────────────────────────────
// Contains static data: name, lat, lon. Changes rarely (new stations, renames).

/// <summary>Envelope wrapping the stations array in the station_information feed.</summary>
public class StationInfoResponse
{
    [JsonPropertyName("data")]
    public StationInfoData Data { get; set; } = new();
}

public class StationInfoData
{
    [JsonPropertyName("stations")]
    public List<StationInfo> Stations { get; set; } = new();
}

// ─── station_status feed ──────────────────────────────────────────────────────
// Contains live data: bikes available. Updated every ~60 seconds by BIXI.

/// <summary>Envelope wrapping the stations array in the station_status feed.</summary>
public class StationStatusResponse
{
    [JsonPropertyName("data")]
    public StationStatusData Data { get; set; } = new();
}

public class StationStatusData
{
    [JsonPropertyName("stations")]
    public List<StationStatus> Stations { get; set; } = new();
}
