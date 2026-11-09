import { Link } from 'react-router-dom';

export default function NoAutorizadoPage() {
  return (
    <div style={{ textAlign: 'center', marginTop: 80 }}>
      <h1>Acceso denegado</h1>
      <p>No tienes permisos para ver esta página.</p>
      <Link to="/">Volver al inicio</Link>
    </div>
  );
}