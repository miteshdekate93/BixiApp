import type { Station } from '../types/station';

// Fetches stations from the backend. Appends maxDistanceMeters as a query
// param when provided. The signal allows the caller to cancel the request.
export async function fetchStations(
  maxDistanceMeters?: number,
  signal?: AbortSignal,
): Promise<Station[]> {
  const url = new URL('/api/stations', window.location.origin);
  if (maxDistanceMeters !== undefined) {
    url.searchParams.set('maxDistanceMeters', String(maxDistanceMeters));
  }

  const response = await fetch(url.toString(), { signal });

  if (!response.ok) {
    throw new Error(`Server error: ${response.status}`);
  }

  return response.json() as Promise<Station[]>;
}
