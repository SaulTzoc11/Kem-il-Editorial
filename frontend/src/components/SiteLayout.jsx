import { useEffect, useState } from 'react';
import { Link, NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useCart } from '../context/CartContext';
import { useUI } from '../context/UIContext';
import { esPersonal, normalizarRol, rutaInicial } from '../utils/roles';
import Icon from './Icon';
import Logo from './Logo';

// Marco común de las páginas públicas y de cliente: anuncio, cabecera, contenido y pie.
export default function SiteLayout() {
  const { pathname, hash } = useLocation();
  const navigate = useNavigate();
  const { user, logout } = useAuth();
  const { totalItems } = useCart();
  const { openCart, openQuote } = useUI();
  const [menuOpen, setMenuOpen] = useState(false);
  const [cuentaOpen, setCuentaOpen] = useState(false);
  const [rutaAnterior, setRutaAnterior] = useState(pathname + hash);

  // Al navegar, cerrar menús (ajuste de estado durante el render, sin useEffect)
  if (rutaAnterior !== pathname + hash) {
    setRutaAnterior(pathname + hash);
    setMenuOpen(false);
    setCuentaOpen(false);
  }

  // Navegación entre secciones de la home (#novedades, #instituciones...) y scroll arriba al cambiar de página
  useEffect(() => {
    if (hash) document.querySelector(hash)?.scrollIntoView({ behavior: 'smooth' });
    else window.scrollTo(0, 0);
  }, [pathname, hash]);

  const salir = () => {
    logout();
    setCuentaOpen(false);
    navigate('/');
  };

  return (
    <div className="app-shell">
      <div className="announcement">
        <span>Envío gratis en pedidos mayores a Q250</span>
        <span className="announcement-separator">•</span>
        <button onClick={openQuote}>¿Compras para una institución? Cotiza aquí</button>
      </div>

      <header className="site-header">
        <Link aria-label="Kem'il, inicio" className="brand" to="/">
          <Logo />
        </Link>
        <nav aria-label="Navegación principal" className={menuOpen ? 'main-nav is-open' : 'main-nav'}>
          <NavLink to="/catalogo">Libros</NavLink>
          <Link to="/#novedades">Novedades</Link>
          <Link to="/#instituciones">Instituciones</Link>
          <Link to="/#nosotros">Nuestra historia</Link>
        </nav>
        <div className="header-actions">
          {user ? (
            <div className="account-wrap">
              <button
                aria-expanded={cuentaOpen}
                aria-haspopup="menu"
                aria-label="Mi cuenta"
                className="icon-button account-button"
                onClick={() => setCuentaOpen(!cuentaOpen)}
              >
                <Icon name="user" />
                <span>{user.nombre.split(' ')[0]}</span>
              </button>
              {cuentaOpen && (
                <div className="account-menu" role="menu">
                  <header>
                    <strong>{user.nombre}</strong>
                    <small>{normalizarRol(user.rol)}</small>
                  </header>
                  {esPersonal(user.rol) && (
                    <Link role="menuitem" to={rutaInicial(user.rol)}>
                      Ir a mi panel
                    </Link>
                  )}
                  <button onClick={salir} role="menuitem">
                    Cerrar sesión
                  </button>
                </div>
              )}
            </div>
          ) : (
            <Link aria-label="Ingresar a mi cuenta" className="icon-button account-button" to="/login">
              <Icon name="user" />
              <span>Ingresar</span>
            </Link>
          )}
          <button aria-label={`Carrito con ${totalItems} productos`} className="icon-button cart-button" onClick={openCart}>
            <Icon name="bag" />
            {totalItems > 0 && <span className="cart-count">{totalItems}</span>}
          </button>
          <button
            aria-expanded={menuOpen}
            aria-label="Abrir menú"
            className="icon-button menu-button"
            onClick={() => setMenuOpen(!menuOpen)}
          >
            <Icon name={menuOpen ? 'close' : 'menu'} />
          </button>
        </div>
      </header>

      <main>
        <Outlet />
      </main>

      <footer>
        <div className="footer-brand">
          <Logo alt="Kem'il" />
          <p>Tejemos historias que conectan personas, territorios y generaciones.</p>
        </div>
        <div>
          <h3>Explora</h3>
          <Link to="/catalogo">Catálogo</Link>
          <Link to="/#novedades">Novedades</Link>
          <Link to="/#instituciones">Instituciones</Link>
        </div>
        <div>
          <h3>Mi cuenta</h3>
          <Link to="/login">Iniciar sesión</Link>
          <Link to="/registro">Crear cuenta</Link>
          <Link to="/carrito">Carrito</Link>
        </div>
        <div>
          <h3>Contacto</h3>
          <p>contacto@kemileditorial.com</p>
          <p>Lun–Vie · 9:00–18:00</p>
          <p>Guatemala</p>
        </div>
        <div className="footer-bottom">
          <span>© {new Date().getFullYear()} Kem’il Editorial. Todos los derechos reservados.</span>
          <span>Hecho para quienes aman las historias.</span>
        </div>
      </footer>
    </div>
  );
}
