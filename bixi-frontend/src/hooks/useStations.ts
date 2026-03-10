import { useCallback, useEffect, useRef, useState } from 'react';
import { fetchStations } from '../api/stationsApi';
import type { Station } from '../types/station';

interface UseStationsResult {
  stations: Station[];
  loading: boolean;
  error: string | null;
  maxDistance: number | undefined;
  setMaxDistance: (value: number | undefined) => void;
}

/**
 * Manages the station data lifecycle:
 *   - Fetches on mount (no filter)
 *   - Re-fetches when maxDistance changes, debounced by 300ms
 *   - Cancels in-flight requests via AbortController when the filter changes
 *     or the component unmounts — prevents stale data races
 *   - On error: keeps previous station list visible instead of clearing it
 *
 * Debounce: user can type quickly without firing a request on every keystroke.
 * AbortController: avoids a slow request from an old filter value overwriting
 * a fast response from a newer one.
 */
export function useStations(): UseStationsResult {
  const [stations, setStations]       = useState<Station[]>([]);
  const [loading, setLoading]         = useState(false);
  const [error, setError]             = useState<string | null>(null);
  const [maxDistance, setMaxDistanceState] = useState<number | undefined>(undefined);

  // Holds the debounce timer so we can clear it on rapid changes.
  const debounceTimer = useRef<ReturnType<typeof setTimeout> | null>(null);

  // Debounced setter exposed to the consumer — waits 300ms after the last call
  // before actually updating the state that triggers a fetch.
  const setMaxDistance = useCallback((value: number | undefined) => {
    if (debounceTimer.current) clearTimeout(debounceTimer.current);
    debounceTimer.current = setTimeout(() => {
      setMaxDistanceState(value);
    }, 300);
  }, []);

  useEffect(() => {
    // Create a new AbortController for each fetch so we can cancel it cleanly.
    const controller = new AbortController();

    const load = async () => {
      setLoading(true);
      try {
        const data = await fetchStations(maxDistance, controller.signal);
        setStations(data);
        setError(null);
      } catch (err) {
        // AbortError is not a real error — it means we intentionally cancelled
        // the request (filter changed or component unmounted). Ignore it.
        if (err instanceof DOMException && err.name === 'AbortError') return;

        // For every other error, show a message but keep the previous station
        // list visible so the user still has something to look at.
        setError(err instanceof Error ? err.message : 'Something went wrong');
      } finally {
        setLoading(false);
      }
    };

    load();

    // Cleanup: cancel the in-flight request when maxDistance changes or unmount.
    return () => controller.abort();
  }, [maxDistance]);

  return { stations, loading, error, maxDistance, setMaxDistance };
}
