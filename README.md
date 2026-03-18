# BixiApp

A full-stack web app to find available BIXI bike stations near a location in Montreal. Built with **.NET 9 Web API** and **React + TypeScript**.

## Features

- Live station data fetched from the [BIXI GBFS API](https://gbfs.velobixi.com/gbfs/gbfs.json) on every page load
- Displays station name, available bikes, and distance from a reference point
- Filter stations by maximum distance (server-side filtering)
- Distance calculated using the **Haversine formula** (straight-line, as the crow flies)
- Debounced filter input with request cancellation to avoid redundant API calls
- Input validation with proper error responses (400 ProblemDetails)

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | .NET 9 Web API, C# |
| Frontend | React 18, TypeScript, Vite |
| Testing | xUnit, 23 unit tests |
| Distance | Haversine formula |
| Data Source | BIXI GBFS public API |

## Architecture

- **Backend** follows a clean layered structure: `Controller → Service → HTTP Client / Models`
- **GBFS feed URLs** are discovered dynamically from the `gbfs.json` root — never hardcoded
- **Distance filtering** is server-side only — frontend sends the filter value, backend applies it
- **Frontend** debounces the distance input (300ms) and cancels in-flight requests via `AbortController`
- **All config** lives in `appsettings.json` — no magic strings in code

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
  - macOS: `brew install dotnet@9` then `export PATH="/opt/homebrew/opt/dotnet@9/bin:$PATH"`
- [Node.js 22+](https://nodejs.org/)

> The repo includes `global.json` pinning the SDK to .NET 9. Running without .NET 9 in your PATH will fail with `NETSDK1045`.

### Run the Backend

```bash
cd BixiApi
dotnet run
```

API available at `http://localhost:5000`

```bash
# All stations
curl http://localhost:5000/api/stations

# Filtered to 500m radius
curl "http://localhost:5000/api/stations?maxDistanceMeters=500"

# Invalid input — returns 400
curl "http://localhost:5000/api/stations?maxDistanceMeters=0"
```

### Run the Frontend

```bash
cd bixi-frontend
npm install
npm run dev
```

App available at `http://localhost:3000`

> Vite proxies `/api/*` to `http://localhost:5000` — backend must be running for data to load.

### Run Tests

```bash
cd BixiApi.Tests
dotnet test --verbosity normal
```

All 23 tests pass.
