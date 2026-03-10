type Props = {
  message: string;
};

export function ErrorMessage({ message }: Props) {
  return (
    <p style={{ color: '#721c24', background: '#f8d7da', padding: '12px', borderRadius: '4px' }}>
      {message}
    </p>
  );
}
