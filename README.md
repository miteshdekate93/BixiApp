# 🚲 Work Sample – C# (.NET) + React at Workleap

## Instructions

Follow the scenario below and develop a small full-stack application using **C# (.NET)** for the backend and **React** for the frontend. We expect you to apply best practices in both technologies.

> ⚠️ During a live coding session, we will pair on your code to extend the functionality. Please structure your code accordingly to support collaboration and clean extensibility.


## Scenario

Every day, Mr. Bertrand commutes to Workleap using Montreal's BIXI bike-sharing system. However, finding an available bike for the ride home is often difficult, as many people are doing the same.

To solve this, he would like a small web application to check which BIXI stations still have bikes available.


## Requirements

Mr. Bertrand would like the app to support the following features:

- Display a list of bike stations with:
  - **Station name**
  - **Number of available bikes**
  - **Distance** from the Workleap office
- Ability to **filter stations by distance** (in meters)
- Data must always be **up to date** when the page loads
- UI must match the provided mockup
- Use BIXI's public data via the GBFS API:
  - [https://gbfs.velobixi.com/gbfs/gbfs.json](https://gbfs.velobixi.com/gbfs/gbfs.json)
  - [GBFS specification](https://github.com/MobilityData/gbfs/blob/master/gbfs.md)
- Use Workleap office location (for distance calculation):
  - Latitude: `45.48415789031987`  
  - Longitude: `-73.56216762891964`

> **Important notes:**
> - The **distance must be calculated "as the crow flies"** — this means the straight-line distance between two geographic points, ignoring streets, paths, or elevation.
> - The **filtering by distance must be performed on the backend**.

## 🛠️ Technical Requirements

- Use the **provided GitHub repository** for your work.
- Backend must be written in **C# with .NET**.
- Frontend must be built with **React**.


## 🎨 UI Design Guidelines

![Worksample Visual](images/stations.png?raw=true)

You're encouraged to make the UI clean and usable, following these visual style hints (not mandatory, but appreciated):

### Color Chart

| Element           | CSS Properties |
|-------------------|----------------|
| **Title**         | `color: #3c3c3c; font-size: 2rem;` |
| **Filters**       | `border-color: #e0dfdd; background-color: #f8f6f3;` |
| **Label**         | `color: #777775; font-size: 1rem;` |
| **Input**         | `border-color: #b3b3b1; color: #3c3c3c;` |
| **Table**         | `border-color: #e0dfdd; color: #3c3c3c;` |
| **Tags (Success)**| `background-color: #e3f3b9; color: #115a52;` |
| **Tags (Error)**  | `background-color: #fde6e5; color: #952927;` |


## Submission

Please commit your solution to the provided GitHub repository. Include a `README.md` file with setup instructions so we can run the app locally.

We're excited to see your work and look forward to collaborating with you to evolve the app further!

---

## Setup & Run

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
  - macOS (Homebrew): `brew install dotnet@9`
  - Add to shell: `export PATH="/opt/homebrew/opt/dotnet@9/bin:$PATH"`
- [Node.js 22+](https://nodejs.org/) and npm

> The repo includes `global.json` which pins the SDK to .NET 9. Running `dotnet` without .NET 9 in your PATH will fail with NETSDK1045.

---

### Run the Backend

```bash
cd BixiApi
dotnet run
```

API is available at `http://localhost:5000`

```bash
# All stations
curl http://localhost:5000/api/stations

# Filtered to 500m radius
curl "http://localhost:5000/api/stations?maxDistanceMeters=500"

# Invalid input — returns 400 ProblemDetails
curl "http://localhost:5000/api/stations?maxDistanceMeters=0"
```

---

### Run the Frontend

```bash
cd bixi-frontend
npm install
npm run dev
```

App is available at `http://localhost:3000`

> The Vite dev server proxies `/api/*` to `http://localhost:5000`. The backend must be running for data to load.

---

### Run Tests

```bash
cd BixiApi.Tests
dotnet test --verbosity normal
```

All 23 tests should pass.

---

### Architecture Notes

- **Backend** follows a clean layered structure: `Controller → Service → HTTP/Models`
- **GBFS feed URLs** are discovered dynamically from the `gbfs.json` root file — never hardcoded
- **Distance filtering** is performed server-side only; the frontend sends the filter, the backend applies it
- **Distance calculation** uses the Haversine formula — straight-line ("as the crow flies"), no routing
- **Frontend** debounces the distance input (300ms) and cancels in-flight requests with `AbortController`
- **All config** lives in `appsettings.json` under the `Workleap` key — no magic strings in code

### Assumptions

- Distance is straight-line (Haversine) — no streets, elevation, or routing involved
- Stations missing from either GBFS feed (`station_information` or `station_status`) are excluded silently
- Filter boundary is inclusive — a station at exactly the max distance **is** shown