import type { Station } from '../../types/station';
import { AvailabilityTag } from '../AvailabilityTag/AvailabilityTag';
import styles from './StationTable.module.css';

interface Props {
  stations: Station[];
  loading: boolean;
}

/**
 * Renders the list of bike stations.
 *
 * Three states:
 *  - loading → shows a simple loading message
 *  - empty   → tells the user no stations matched their filter
 *  - data    → renders the full table sorted by distance (already sorted by the API)
 */
export function StationTable({ stations, loading }: Props) {
  if (loading) {
    return <p className={styles.message}>Loading stations...</p>;
  }

  if (stations.length === 0) {
    return <p className={styles.message}>No stations found for this distance.</p>;
  }

  return (
    <table className={styles.table}>
      <thead>
        <tr>
          <th>Station Name</th>
          <th>Available Bikes</th>
          <th>Distance (m)</th>
          <th>Status</th>
        </tr>
      </thead>
      <tbody>
        {stations.map((station) => (
          <tr key={station.stationId}>
            <td>{station.name}</td>
            <td>{station.availableBikes}</td>
            {/* Round to nearest meter — sub-meter precision is not meaningful here */}
            <td>{Math.round(station.distanceMeters)} m</td>
            <td>
              <AvailabilityTag isAvailable={station.isAvailable} />
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
