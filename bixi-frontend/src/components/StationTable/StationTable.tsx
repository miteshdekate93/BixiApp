import type { Station } from '../../types/station';
import styles from './StationTable.module.css';

type Props = {
  stations: Station[];
};

export function StationTable({ stations }: Props) {
  return (
    <table className={styles.table}>
      <thead>
        <tr>
          <th>Name</th>
          <th>Distance (m)</th>
          <th>Bikes Available</th>
          <th>Status</th>
        </tr>
      </thead>
      <tbody>
        {stations.map((s) => (
          <tr key={s.stationId}>
            <td>{s.name}</td>
            <td>{s.distanceMeters}</td>
            <td>{s.availableBikes}</td>
            <td>{s.isAvailable ? 'Available' : 'Unavailable'}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
