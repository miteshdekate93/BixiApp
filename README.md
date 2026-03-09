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