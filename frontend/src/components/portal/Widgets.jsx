export function StatusBadge({ children }) {
  const clase = String(children).toLowerCase().replace(/\s+/g, '-');
  return <span className={`portal-status status-${clase}`}>{children}</span>;
}

export function MetricCard({ label, value, note, tone }) {
  return (
    <article className={`metric-card ${tone}`}>
      <span>{label}</span>
      <strong>{value}</strong>
      <small>{note}</small>
    </article>
  );
}

// Aviso visible cuando la pantalla muestra datos de ejemplo porque el endpoint aún no existe
export function MockNotice({ endpoint }) {
  return (
    <p className="mock-notice" role="status">
      Datos de ejemplo: el endpoint <code>{endpoint}</code> aún no existe en el backend.
    </p>
  );
}
