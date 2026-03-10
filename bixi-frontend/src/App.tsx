import { useState } from 'react';
import { FilterPanel } from './components/FilterPanel/FilterPanel';
import { StationTable } from './components/StationTable/StationTable';
import { useStations } from './hooks/useStations';
import { ErrorMessage } from './components/ErrorMessage/ErrorMessage';

function App() {
  const [maxDistance, setMaxDistance] = useState<number | undefined>();
  const { stations, loading, error } = useStations();

  return (
    <div style={{ maxWidth: 900, margin: '0 auto', padding: '24px' }}>
      <h1>BIXI Stations Near Workleap Office</h1>
      <FilterPanel maxDistance={maxDistance} onChange={setMaxDistance} />
      {error && <ErrorMessage message={error} />}
      {loading ? <p>Loading...</p> : <StationTable stations={stations} />}
    </div>
  );
}

export default App;
