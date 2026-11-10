import { Link } from 'react-router-dom';
import Icon from './Icon';
import Logo from './Logo';

// Pantalla dividida de acceso (login / registro / no autorizado)
export default function AuthLayout({ children }) {
  return (
    <div className="login-screen is-page">
      <Link className="auth-back" to="/">
        <Icon name="arrow" size={16} /> Volver a la tienda
      </Link>
      <div className="login-brand-panel">
        <div className="login-brand-pattern" />
        <Logo />
        <blockquote>“Cada rol tiene su espacio. Una sola editorial, todos sus procesos conectados.”</blockquote>
        <p>Plataforma editorial multicanal</p>
      </div>
      <div className="login-form-panel">{children}</div>
    </div>
  );
}
