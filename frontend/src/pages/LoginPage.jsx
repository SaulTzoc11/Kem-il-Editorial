import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { mensajeDeError } from '../api/axiosClient';
import AuthLayout from '../components/AuthLayout';
import Icon from '../components/Icon';
import { useAuth } from '../context/AuthContext';
import { esPersonal, rutaInicial } from '../utils/roles';

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
      const usuario = await login(correo, password);
      // Personal va a su panel; los clientes regresan a donde estaban (p. ej. el carrito)
      const destino = esPersonal(usuario.rol) ? rutaInicial(usuario.rol) : location.state?.from || '/';
      navigate(destino, { replace: true });
    } catch (err) {
      setError(mensajeDeError(err, 'No se pudo iniciar sesión.'));
    } finally {
      setCargando(false);
    }
  };

  return (
    <AuthLayout>
      <form onSubmit={handleSubmit}>
        <span className="eyebrow">Acceso seguro</span>
        <h2>Bienvenido a Kem’il</h2>
        <p>Ingresa con tus credenciales. El sistema abrirá el espacio correspondiente a tu rol.</p>
        <label className="login-field">
          <span>Correo electrónico</span>
          <input
            autoComplete="email"
            onChange={(e) => setCorreo(e.target.value)}
            required
            type="email"
            value={correo}
          />
        </label>
        <label className="login-field">
          <span>Contraseña</span>
          <input
            autoComplete="current-password"
            onChange={(e) => setPassword(e.target.value)}
            required
            type="password"
            value={password}
          />
        </label>
        {error && (
          <p className="form-error" role="alert">
            {error}
          </p>
        )}
        <button className="button button-primary login-submit" disabled={cargando} type="submit">
          {cargando ? 'Ingresando...' : 'Ingresar'} <Icon name="arrow" />
        </button>
        <p className="auth-switch">
          ¿No tienes cuenta? <Link to="/registro">Regístrate</Link>
        </p>
      </form>
    </AuthLayout>
  );
}
