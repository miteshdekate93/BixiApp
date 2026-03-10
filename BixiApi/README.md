# BixiApi — .NET 9 Web API

Backend service that fetches live BIXI station data from the GBFS feed, calculates distances from the Workleap office, and exposes a filtered REST endpoint.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) — installed via Homebrew on macOS:
  ```bash
  brew install dotnet@9
  ```
- Add .NET 9 to your shell PATH (add to `~/.zshrc` or `~/.bashrc` and restart your terminal):
  ```bash
  export DOTNET_ROOT="/opt/homebrew/opt/dotnet@9/libexec"
  export PATH="/opt/homebrew/opt/dotnet@9/bin:$PATH"
  ```

> The repo includes a `global.json` at the root that pins the SDK to .NET 9.
> Running `dotnet` without .NET 9 in your PATH will produce error NETSDK1045.

## Run

```bash
dotnet run
```

API starts on `http://localhost:5000`.

## Endpoints

| Method | Route | Query Params | Description |
|--------|-------|--------------|-------------|
| GET | `/api/stations` | `maxDistanceMeters` (optional int) | Returns stations near the Workleap office |

**Example:**
```
GET /api/stations
GET /api/stations?maxDistanceMeters=500
```

**Response shape:**
```json
[
  {
    "stationId": "string",
    "name": "string",
    "availableBikes": 0,
    "distanceMeters": 0.0,
    "isAvailable": true
  }
]
```

## Configuration

`appsettings.json` — `Workleap` section:

| Key | Description |
|-----|-------------|
| `OfficeLat` / `OfficeLng` | Workleap office coordinates |
| `GbfsBaseUrl` | Base URL for the GBFS feed |
| `HttpTimeoutSeconds` | HTTP client timeout |

## Project Structure

```
BixiApi/
├── Controllers/
│   └── StationsController.cs     # GET /api/stations
├── Middleware/
│   └── GlobalExceptionMiddleware.cs
├── Models/
│   ├── StationInfo.cs            # GBFS station_information fields
│   ├── StationStatus.cs          # GBFS station_status fields
│   ├── StationResult.cs          # API response model
│   └── WorkleapSettings.cs       # Typed config
└── Services/
    ├── Interfaces/
    │   ├── IBixiService.cs
    │   └── IDistanceService.cs
    ├── BixiService.cs            # Fetches + merges GBFS data
    └── DistanceService.cs        # Haversine distance calculation
```
