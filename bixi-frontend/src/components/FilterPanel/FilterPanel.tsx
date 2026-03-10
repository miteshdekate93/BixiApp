import { useEffect, useRef, useState } from 'react';
import styles from './FilterPanel.module.css';

interface Props {
  onChange: (value: number | undefined) => void;
}

// Debounce lives here (UI concern) so the hook stays simple.
// The input uses local state, so the displayed value updates on every keystroke
// while onChange only fires 1000ms after the user stops typing.
export function FilterPanel({ onChange }: Props) {
  const [inputValue, setInputValue] = useState('');
  const debounceTimer = useRef<ReturnType<typeof setTimeout> | null>(null);

  // Clean up the timer on unmount to avoid calling onChange after unmount.
  useEffect(() => {
    return () => {
      if (debounceTimer.current) clearTimeout(debounceTimer.current);
    };
  }, []);

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
        value={inputValue}
        placeholder="No limit"
        onChange={(e) => {
          const raw = e.target.value;
          setInputValue(raw);
          if (debounceTimer.current) clearTimeout(debounceTimer.current);
          debounceTimer.current = setTimeout(() => {
            onChange(raw === '' ? undefined : Number(raw));
          }, 1000);
        }}
      />
    </div>
  );
}
