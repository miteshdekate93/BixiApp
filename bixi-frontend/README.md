# bixi-frontend — Vite + React 19 + TypeScript

Frontend app that displays BIXI bike stations near the Workleap office with distance filtering.

## Prerequisites

- [Node.js 22+](https://nodejs.org/) and npm
- BixiApi backend running on `http://localhost:5000` (see `../BixiApi/README.md`)

## Install & Run

```bash
npm install
npm run dev
```

App starts on `http://localhost:3000`.

> Vite proxies all `/api` requests to `http://localhost:5000`, so the backend must be running for data to load.

## Scripts

| Script | Description |
|--------|-------------|
| `npm run dev` | Start dev server on port 3000 |
| `npm run build` | Type-check + production build |
| `npm run preview` | Preview production build locally |

## Project Structure

```
bixi-frontend/
├── index.html
├── vite.config.ts              # Proxy /api → http://localhost:5000
└── src/
    ├── main.tsx                # React entry point
    ├── App.tsx                 # Root layout: title + FilterPanel + StationTable
    ├── types/
    │   └── station.ts          # Station type
    ├── api/
    │   └── stationsApi.ts      # fetchStations() — wraps GET /api/stations
    ├── hooks/
    │   └── useStations.ts      # Data fetching hook
    └── components/
        ├── StationTable/       # Table of station results
        ├── FilterPanel/        # Max distance input
        ├── AvailabilityTag/    # Green/red availability badge
        └── ErrorMessage/       # Styled error display
```

## Type Check

```bash
npx tsc --noEmit
```
