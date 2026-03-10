# BixiApi.Tests — xUnit Test Suite

Unit and integration tests for the BixiApi backend. Kept in a separate project so the production assembly ships with zero test dependencies.

## Prerequisites

Same as BixiApi — .NET 9 must be in your PATH. See `../BixiApi/README.md`.

## Run

```bash
dotnet test
```

## Project Structure

```
BixiApi.Tests/
├── ContractTests/
│   └── GbfsDeserializationTests.cs   # JSON deserialization contract tests
├── ControllerTests/
│   └── StationsControllerTests.cs    # Full pipeline integration tests
├── Helpers/
│   └── FakeHttpMessageHandler.cs     # Intercepts HTTP calls in tests
└── ServiceTests/
    ├── DistanceServiceTests.cs        # Haversine formula correctness
    └── FilteringTests.cs              # BixiService join + filter + sort logic
```

## Test Coverage

### ContractTests — GbfsDeserializationTests (5 tests)
Verifies that our C# models correctly parse GBFS JSON. Uses raw JSON strings — no HTTP calls, no server.

| Test | What it proves |
|------|----------------|
| `StationInfo_DeserializesCorrectly` | `[JsonPropertyName]` mapping works for station_information fields |
| `StationStatus_DeserializesCorrectly` | `num_bikes_available` maps to `NumBikesAvailable` |
| `StationStatus_ZeroBikes_RawValueIsZero` | Value 0 is preserved; will map to `IsAvailable = false` |
| `StationStatus_ThreeBikes_RawValueIsThree` | Value 3 is preserved; will map to `IsAvailable = true` |
| `ExtraUnknownFields_DoNotThrow` | Unknown JSON fields are silently ignored (forward compatibility) |

### ServiceTests — DistanceServiceTests (5 tests)
Verifies the Haversine distance calculation with known geographic values.

| Test | What it proves |
|------|----------------|
| `SamePoint_ReturnsZero` | Zero distance for identical coordinates |
| `WorkleapToMetroLaurier_IsApproximately5100m` | Real-world accuracy against known Montreal coordinates |
| `OneDegreeNorth_IsApproximately111km` | One degree of latitude ≈ 111 km — a geographic constant |
| `TwoNearbyPoints_AreApproximately500mApart` | Short-range accuracy; catches km vs m scale errors |
| `Distance_IsSymmetric` | A→B equals B→A within floating-point tolerance |

### ServiceTests — FilteringTests (7 tests)
Verifies BixiService join, filter, and sort logic using `FakeHttpMessageHandler` and a mocked `IDistanceService`.

| Test | What it proves |
|------|----------------|
| `Filter_1000m_ReturnsOnlyStationsWithin1000m` | Stations beyond the limit are excluded |
| `NoFilter_ReturnsAllStations` | Null maxDistance returns everything |
| `Filter_StationAtExactBoundary_IsIncluded` | Filter boundary is inclusive |
| `Filter_AllStationsBeyondDistance_ReturnsEmptyList` | Returns `[]` not `null` when everything is filtered out |
| `Join_StationInInfoFeedOnly_IsExcluded` | Station missing from status feed is dropped |
| `Join_StationInStatusFeedOnly_IsExcluded` | Station missing from info feed is dropped |
| `Results_AreSortedByDistanceAscending` | Closest stations always appear first |

### ControllerTests — StationsControllerTests (6 tests)
Integration tests using `WebApplicationFactory` — boots the full ASP.NET Core pipeline in memory with `IBixiService` replaced by a mock.

| Test | What it proves |
|------|----------------|
| `GetStations_MaxDistanceZero_ReturnsBadRequest` | Controller rejects `maxDistanceMeters=0` with 400 |
| `GetStations_MaxDistanceNegative_ReturnsBadRequest` | Controller rejects negative values with 400 |
| `GetStations_ServiceThrowsHttpRequestException_Returns502` | Middleware maps `HttpRequestException` → 502 |
| `GetStations_ServiceThrowsTaskCanceledException_Returns504` | Middleware maps `TaskCanceledException` → 504 |
| `GetStations_ValidCall_Returns200WithStationList` | Happy path returns 200 with correct body |
| `GetStations_ErrorResponse_ContainsStatusAndTitleFields` | Error body follows RFC 7807 ProblemDetails shape |

## Design Notes

- `_sut` (System Under Test) — a naming convention that makes the tested class obvious.
- `FakeHttpMessageHandler` — intercepts `HttpClient` calls and returns preset JSON, so tests never touch the real BIXI API.
- `It.IsAny<T>()` — a Moq matcher that accepts any value for that parameter.
- `IClassFixture<WebApplicationFactory<Program>>` — xUnit shares one base factory across all controller tests; each test creates a child factory via `WithWebHostBuilder` for full isolation.
- `Assert.InRange` — used for geographic distances where small floating-point variance is expected and acceptable.
