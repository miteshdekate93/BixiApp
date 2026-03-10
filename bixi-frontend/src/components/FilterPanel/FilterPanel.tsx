import styles from './FilterPanel.module.css';

type Props = {
  maxDistance?: number;
  onChange: (value?: number) => void;
};

export function FilterPanel({ maxDistance, onChange }: Props) {
  return (
    <div className={styles.panel}>
      <label htmlFor="maxDistance">Max Distance (m):</label>
      <input
        id="maxDistance"
        type="number"
        value={maxDistance ?? ''}
        onChange={(e) => {
          const val = e.target.value;
          onChange(val === '' ? undefined : Number(val));
        }}
        placeholder="No limit"
      />
    </div>
  );
}
