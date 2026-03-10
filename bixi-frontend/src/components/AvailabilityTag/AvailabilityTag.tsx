import styles from './AvailabilityTag.module.css';

type Props = {
  isAvailable: boolean;
};

export function AvailabilityTag({ isAvailable }: Props) {
  return (
    <span className={isAvailable ? styles.available : styles.unavailable}>
      {isAvailable ? 'Available' : 'Unavailable'}
    </span>
  );
}
