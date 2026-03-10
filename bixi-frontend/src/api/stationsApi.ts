import type { Station } from '../types/station';

export async function fetchStations(maxDistance?: number): Promise<Station[]> {
  const url = new URL('/api/stations', window.location.origin);
  if (maxDistance !== undefined) {
    url.searchParams.set('maxDistanceMeters', String(maxDistance));
  }
  const res = await fetch(url.toString());
  if (!res.ok) throw new Error(`Request failed: ${res.status}`);
  return res.json();
}
