import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function HomePage() {
  const { user, isAuthenticated, logout } = useAuth();

  return (
    <div style={{ padding: 20 }}>
      <h1>Inicio - Kem'il Editorial</h1>
      {isAuthenticated ? (
        <p>
          Hola, <strong>{user.nombre}</strong> ({user.rol}){' '}
          <button onClick={logout}>Cerrar sesión</button>
        </p>
      ) : (
        <p><Link to="/login">Iniciar sesión</Link> · <Link to="/registro">Registrarse</Link></p>
      )}
      <p><Link to="/catalogo">Catálogo</Link> · <Link to="/carrito">Carrito</Link></p>
    </div>
  );
}