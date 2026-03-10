import styles from './AvailabilityTag.module.css';

interface Props {
  isAvailable: boolean;
}

/**
 * Small coloured badge indicating whether a station has bikes.
 * Green = available, red = unavailable — matches the UI spec colours.
 */
export function AvailabilityTag({ isAvailable }: Props) {
  return (
    <span className={isAvailable ? styles.available : styles.unavailable}>
      {isAvailable ? 'Available' : 'Unavailable'}
    </span>
  );
}
