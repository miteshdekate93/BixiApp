import { useStations } from './hooks/useStations';
import { FilterPanel } from './components/FilterPanel/FilterPanel';
import { StationTable } from './components/StationTable/StationTable';
import { ErrorMessage } from './components/ErrorMessage/ErrorMessage';

/**
 * Root component. Wires the useStations hook to all child components.
 *
 * Layout: Title → Filter → Error (if any) → Table
 *
 * The error banner appears above the table so it is immediately visible,
 * but the previous station data stays in the table so the user still has
 * something to look at while the network is recovering.
 */
function App() {
  const { stations, loading, error, maxDistance, setMaxDistance } = useStations();

  return (
    <div style={{ maxWidth: 900, margin: '0 auto', padding: '32px 24px' }}>
      <h1 style={{ color: '#3c3c3c', fontSize: '2rem', marginBottom: '24px' }}>
        BIXI Stations near Workleap
      </h1>

      <FilterPanel maxDistance={maxDistance} onChange={setMaxDistance} />

      {error && <ErrorMessage message={error} />}

      <StationTable stations={stations} loading={loading} />
    </div>
  );
}

export default App;
