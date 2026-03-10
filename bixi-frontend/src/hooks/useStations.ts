import { useEffect, useState } from 'react';
import { fetchStations } from '../api/stationsApi';
import type { Station } from '../types/station';

interface UseStationsResult {
  stations: Station[];
  loading: boolean;
  error: string | null;
  maxDistance: number | undefined;
  setMaxDistance: (value: number | undefined) => void;
}

// Fetches stations whenever maxDistance changes.
// AbortController cancels the previous in-flight request when the filter
// changes quickly, preventing a slow old response from overwriting a newer one.
// On error the previous station list stays visible — no blank screen.
export function useStations(): UseStationsResult {
  const [stations, setStations] = useState<Station[]>([]);
  const [loading, setLoading]   = useState(false);
  const [error, setError]       = useState<string | null>(null);
  const [maxDistance, setMaxDistance] = useState<number | undefined>(undefined);

  useEffect(() => {
    const controller = new AbortController();

    const load = async () => {
      setLoading(true);
      try {
        // @ts-ignore
        const data = await fetchStations(maxDistance, controller.signal);
        setStations(data);
        setError(null);
      } catch (err) {
        // AbortError means the request was intentionally cancelled — not a real error.
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setError(err instanceof Error ? err.message : 'Something went wrong');
      } finally {
        setLoading(false);
      }
    };

    load();

    return () => controller.abort();
  }, [maxDistance]);

  return { stations, loading, error, maxDistance, setMaxDistance };
}
