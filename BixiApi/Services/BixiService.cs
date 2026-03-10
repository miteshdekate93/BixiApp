using System.Net.Http.Json;
using System.Text.Json;
using BixiApi.Models;
using BixiApi.Models.Gbfs;
using BixiApi.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace BixiApi.Services;

/// <summary>
/// Fetches live BIXI station data from the public GBFS API, merges it with
/// real-time availability, calculates distances from the Workleap office,
/// and applies optional server-side distance filtering.
///
/// Why GBFS? It is the open standard used by bike-share systems worldwide.
/// BIXI exposes two feeds we care about:
///   - station_information  → station name, lat, lon  (static, changes rarely)
///   - station_status       → num_bikes_available     (live, updates every ~60s)
/// We join these two feeds on station_id to build a complete result.
/// </summary>
public class BixiService : IBixiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WorkleapSettings _settings;
    private readonly IDistanceService _distanceService;

    public BixiService(
        IHttpClientFactory httpClientFactory,
        IOptions<WorkleapSettings> settings,
        IDistanceService distanceService)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
        _distanceService = distanceService;
    }

    public async Task<IEnumerable<StationResult>> GetStationsAsync(
        int? maxDistanceMeters, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("bixi");

        // ── Step 1: Discover feed URLs dynamically ────────────────────────────
        // The GBFS root file (gbfs.json) is the stable entry point.
        // It tells us the real URLs for station_information and station_status.
        // This means we never hardcode downstream feed URLs — they could change.
        var discovery = await client.GetFromJsonAsync<GbfsRoot>(_settings.GbfsBaseUrl, ct)
            ?? throw new JsonException("GBFS discovery response was empty.");

        var feeds = GetFeedList(discovery);
        var infoUrl   = GetFeedUrl(feeds, "station_information");
        var statusUrl = GetFeedUrl(feeds, "station_status");

        // ── Step 2: Fetch both feeds in parallel ──────────────────────────────
        // The two feeds are independent, so we fire both requests at the same
        // time with Task.WhenAll — this halves the waiting time vs. sequential awaits.
        var infoTask   = client.GetFromJsonAsync<StationInfoResponse>(infoUrl, ct);
        var statusTask = client.GetFromJsonAsync<StationStatusResponse>(statusUrl, ct);

        await Task.WhenAll(infoTask, statusTask);

        var infoStations   = infoTask.Result?.Data.Stations
            ?? throw new JsonException("station_information feed returned no data.");
        var statusStations = statusTask.Result?.Data.Stations
            ?? throw new JsonException("station_status feed returned no data.");

        // ── Step 3: Join on station_id ────────────────────────────────────────
        // Index status by station_id for O(1) lookup — avoids a nested loop.
        // Stations missing from either feed are silently excluded; incomplete
        // data is not useful to display.
        var statusById = statusStations.ToDictionary(s => s.StationId);

        var results = new List<StationResult>();

        foreach (var info in infoStations)
        {
            if (!statusById.TryGetValue(info.StationId, out var status))
                continue;

            // ── Step 4: Calculate distance ────────────────────────────────────
            // IDistanceService is injected, making it mockable in tests and
            // swappable (e.g. if we wanted to switch from Haversine to a routing API).
            var distance = _distanceService.Calculate(
                _settings.OfficeLat, _settings.OfficeLng,
                info.Lat, info.Lon);

            // ── Step 5: Server-side distance filter ───────────────────────────
            // Filtering happens on the backend so the client receives only the
            // data it needs — no large payloads, no client-side filtering logic.
            // Boundary is INCLUSIVE: a station at exactly maxDistance IS included.
            if (maxDistanceMeters.HasValue && distance > maxDistanceMeters.Value)
                continue;

            results.Add(new StationResult
            {
                StationId      = info.StationId,
                Name           = info.Name,
                AvailableBikes = status.NumBikesAvailable,
                DistanceMeters = distance,
                // A station is "available" only when it has at least one bike.
                IsAvailable    = status.NumBikesAvailable > 0
            });
        }

        // ── Step 6: Sort ascending by distance ───────────────────────────────
        // Closest stations first — most useful for someone deciding where to walk.
        return results.OrderBy(r => r.DistanceMeters).ToList();
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// Extracts the feed list from the discovery response.
    /// Prefers the "en" locale; falls back to the first available one.
    /// </summary>
    private static List<GbfsFeed> GetFeedList(GbfsRoot discovery)
    {
        if (discovery.Data.TryGetValue("en", out var enList))
            return enList.Feeds;

        var first = discovery.Data.FirstOrDefault();
        if (first.Value is null)
            throw new JsonException("GBFS discovery response contained no locales.");

        return first.Value.Feeds;
    }

    /// <summary>
    /// Finds the URL for a named feed (e.g. "station_information").
    /// Throws JsonException if not found — the middleware maps this to 502.
    /// </summary>
    private static string GetFeedUrl(List<GbfsFeed> feeds, string feedName)
    {
        var url = feeds.FirstOrDefault(f => f.Name == feedName)?.Url;
        if (string.IsNullOrEmpty(url))
            throw new JsonException($"GBFS feed '{feedName}' not found in discovery response.");

        return url;
    }
}
