interface Props {
  message: string;
}

/**
 * Displays an API or network error to the user.
 * Intentionally kept simple — no dismiss button, just a visible warning banner.
 */
export function ErrorMessage({ message }: Props) {
  return (
    <div
      role="alert"
      style={{
        color: '#952927',
        backgroundColor: '#fde6e5',
        border: '1px solid #f5c6c5',
        borderRadius: '4px',
        padding: '10px 16px',
        marginBottom: '16px',
        fontSize: '0.95rem',
      }}
    >
      {message}
    </div>
  );
}
