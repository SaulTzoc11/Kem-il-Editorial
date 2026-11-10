import { Link } from 'react-router-dom';
import AuthLayout from '../components/AuthLayout';
import Icon from '../components/Icon';

export default function NoAutorizadoPage() {
  return (
    <AuthLayout>
      <div style={{ maxWidth: 560, width: '100%' }}>
        <span className="eyebrow">Error 403</span>
        <h2 style={{ color: 'var(--navy)', fontFamily: 'var(--serif)', fontSize: 'clamp(36px,4vw,50px)', fontWeight: 500, margin: 0 }}>
          Acceso denegado
        </h2>
        <p style={{ color: 'var(--muted)', lineHeight: 1.7 }}>
          Tu rol no tiene permisos para ver esta página. Si crees que es un error, contacta al administrador.
        </p>
        <Link className="button button-primary" to="/">
          Volver al inicio <Icon name="arrow" />
        </Link>
      </div>
    </AuthLayout>
  );
}
