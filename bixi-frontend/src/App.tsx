import { useStations } from './hooks/useStations';
import { FilterPanel } from './components/FilterPanel/FilterPanel';
import { StationTable } from './components/StationTable/StationTable';
import { ErrorMessage } from './components/ErrorMessage/ErrorMessage';

// Root component — wires the useStations hook to all child components.
function App() {
  const { stations, loading, error, setMaxDistance } = useStations();

  return (
    <div style={{ maxWidth: 900, margin: '0 auto', padding: '32px 24px' }}>
      <h1 style={{ color: '#3c3c3c', fontSize: '2rem', marginBottom: '24px' }}>
        BIXI Stations near Workleap
      </h1>

      <FilterPanel onChange={setMaxDistance} />

      {error && <ErrorMessage message={error} />}

      <StationTable stations={stations} loading={loading} />
    </div>
  );
}

export default App;
