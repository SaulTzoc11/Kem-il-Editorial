import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { normalizarRol } from '../../utils/roles';
import Icon from '../Icon';
import Logo from '../Logo';

// Marco común de los paneles internos (Administrador y Asesor): barra lateral + cabecera + contenido.
export default function PortalShell({ nav, active, onChange, area, welcome, children }) {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const rolNombre = normalizarRol(user.rol);
  const fecha = new Date().toLocaleDateString('es-GT', { weekday: 'long', day: 'numeric', month: 'long' });
  const fechaHoy = fecha.charAt(0).toUpperCase() + fecha.slice(1);

  const elegir = (id) => {
    onChange(id);
    setSidebarOpen(false);
  };

  const salir = () => {
    logout();
    navigate('/login', { replace: true });
  };

  return (
    <div className="portal-shell">
      <aside className={sidebarOpen ? 'portal-sidebar open' : 'portal-sidebar'}>
        <div className="portal-logo">
          <Logo alt="Kem'il" />
          <span>Gestión editorial</span>
        </div>
        <div className="portal-role-label">{rolNombre}</div>
        <nav aria-label={`Navegación de ${rolNombre}`}>
          {nav.map((item) => (
            <button
              aria-current={active === item.id ? 'page' : undefined}
              className={active === item.id ? 'active' : ''}
              key={item.id}
              onClick={() => elegir(item.id)}
            >
              <Icon name={item.icon} />
              {item.label}
              {item.count ? <span className="nav-count">{item.count}</span> : null}
            </button>
          ))}
        </nav>
        <div className="portal-sidebar-footer">
          <span className="portal-avatar">{user.nombre.slice(0, 1)}</span>
          <div>
            <strong>{user.nombre}</strong>
            <small>{rolNombre}</small>
          </div>
          <button aria-label="Cerrar sesión" onClick={salir}>
            <Icon name="logout" />
          </button>
        </div>
      </aside>
      <div
        className={sidebarOpen ? 'portal-sidebar-shade show' : 'portal-sidebar-shade'}
        onClick={() => setSidebarOpen(false)}
      />
      <main className="portal-main">
        <header className="portal-topbar">
          <button aria-label="Abrir navegación" className="portal-menu" onClick={() => setSidebarOpen(true)}>
            <Icon name="menu" />
          </button>
          <div>
            <span>{area}</span>
            <h1>{nav.find((item) => item.id === active)?.label}</h1>
          </div>
          <div className="portal-top-actions">
            <Link className="view-store" to="/">
              Ver tienda
            </Link>
          </div>
        </header>
        <div className="portal-content">
          <div className="portal-welcome">
            <div>
              <span>
                {fechaHoy}
              </span>
              <h2>Buen día, {user.nombre.split(' ')[0]}</h2>
              <p>{welcome.texto}</p>
            </div>
            {welcome.accion}
          </div>
          {children}
        </div>
      </main>
    </div>
  );
}
