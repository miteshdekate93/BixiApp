// Shape of one station returned by GET /api/stations.
// Property names are camelCase to match the .NET API's default JSON serialisation.
export interface Station {
  stationId: string;
  name: string;
  availableBikes: number;
  distanceMeters: number;
  isAvailable: boolean;
}
