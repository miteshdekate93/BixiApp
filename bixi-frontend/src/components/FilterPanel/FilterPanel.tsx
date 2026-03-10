import styles from './FilterPanel.module.css';

interface Props {
  maxDistance: number | undefined;
  onChange: (value: number | undefined) => void;
}

/**
 * Distance filter input.
 * Calls onChange(undefined) when the field is cleared, and onChange(number)
 * when a valid positive number is entered.
 */
export function FilterPanel({ maxDistance, onChange }: Props) {
  return (
    <div className={styles.panel}>
      <label htmlFor="maxDistance" className={styles.label}>
        Filter by distance (meters)
      </label>
      <input
        id="maxDistance"
        type="number"
        min={1}
        className={styles.input}
        value={maxDistance ?? ''}
        placeholder="No limit"
        onChange={(e) => {
          const raw = e.target.value;
          // Empty string → remove the filter entirely
          onChange(raw === '' ? undefined : Number(raw));
        }}
      />
    </div>
  );
}
