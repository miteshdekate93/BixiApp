import type { Station } from '../types/station';

/**
 * Fetches stations from the backend API.
 *
 * @param maxDistanceMeters - Optional filter. When provided, only stations
 *   within this straight-line distance (meters) are returned.
 * @param signal - AbortSignal from an AbortController so the request can be
 *   cancelled when the component unmounts or the filter changes.
 */
export async function fetchStations(
  maxDistanceMeters?: number,
  signal?: AbortSignal,
): Promise<Station[]> {
  // Build the URL — only append the query param when a value is provided.
  const url = new URL('/api/stations', window.location.origin);
  if (maxDistanceMeters !== undefined) {
    url.searchParams.set('maxDistanceMeters', String(maxDistanceMeters));
  }

  const response = await fetch(url.toString(), { signal });

  // Throw on non-2xx so the hook can surface the error to the user.
  if (!response.ok) {
    throw new Error(`Server error: ${response.status}`);
  }

  return response.json() as Promise<Station[]>;
}
