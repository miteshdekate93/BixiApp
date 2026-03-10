import type { Station } from '../types/station';

export function useStations() {
  const stations: Station[] = [];
  const loading = false;
  const error: string | null = null;

  return { stations, loading, error };
}
