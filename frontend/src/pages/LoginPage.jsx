import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [correo, setCorreo] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [cargando, setCargando] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setCargando(true);
    try {
      await login(correo, password);
      navigate(location.state?.from || '/', { replace: true });
    } catch (err) {
      if (err.response?.status === 429) {
        setError('Demasiados intentos. Espera un minuto e inténtalo de nuevo.');
      } else {
        setError(err.response?.data?.mensaje || 'No se pudo iniciar sesión.');
      }
    } finally {
      setCargando(false);
    }
  };

  return (
    <div style={{ maxWidth: 380, margin: '60px auto', padding: 20 }}>
      <h1>Iniciar sesión</h1>
      <form onSubmit={handleSubmit} style={{ display: 'grid', gap: 12 }}>
        <input type="email" placeholder="Correo" value={correo}
               onChange={(e) => setCorreo(e.target.value)} required />
        <input type="password" placeholder="Contraseña" value={password}
               onChange={(e) => setPassword(e.target.value)} required />
        {error && <p style={{ color: 'crimson', margin: 0 }}>{error}</p>}
        <button type="submit" disabled={cargando}>
          {cargando ? 'Ingresando...' : 'Ingresar'}
        </button>
      </form>
      <p>¿No tienes cuenta? <Link to="/registro">Regístrate</Link></p>
    </div>
  );
}