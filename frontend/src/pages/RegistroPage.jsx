import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { mensajeDeError } from '../api/axiosClient';
import AuthLayout from '../components/AuthLayout';
import Icon from '../components/Icon';
import { useAuth } from '../context/AuthContext';

// Validación en tiempo real (el backend vuelve a validar: [StringLength(8..100)], [EmailAddress])
const validar = (f) => ({
  nombre: f.nombre.trim().length < 2 ? 'Escribe tu nombre completo.' : '',
  correo: /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(f.correo) ? '' : 'Escribe un correo válido.',
  password:
    f.password.length < 8
      ? 'Mínimo 8 caracteres.'
      : !/[A-Za-z]/.test(f.password) || !/\d/.test(f.password)
        ? 'Combina letras y números.'
        : '',
  confirmar: f.confirmar !== f.password ? 'Las contraseñas no coinciden.' : '',
});

const CAMPOS = [
  ['nombre', 'Nombre completo', 'text', 'name'],
  ['correo', 'Correo electrónico', 'email', 'email'],
  ['password', 'Contraseña', 'password', 'new-password'],
  ['confirmar', 'Confirmar contraseña', 'password', 'new-password'],
];

export default function RegistroPage() {
  const { registro } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({ nombre: '', correo: '', password: '', confirmar: '' });
  const [tocados, setTocados] = useState({});
  const [error, setError] = useState('');
  const [cargando, setCargando] = useState(false);

  const errores = validar(form);
  const valido = Object.values(errores).every((e) => !e);

  const cambiar = (e) => setForm({ ...form, [e.target.name]: e.target.value });
  const tocar = (e) => setTocados({ ...tocados, [e.target.name]: true });

  const handleSubmit = async (e) => {
    e.preventDefault();
    setTocados({ nombre: true, correo: true, password: true, confirmar: true });
    if (!valido) return;
    setError('');
    setCargando(true);
    try {
      await registro(form.nombre, form.correo, form.password);
      navigate('/', { replace: true });
    } catch (err) {
      setError(mensajeDeError(err, 'No se pudo crear la cuenta.'));
    } finally {
      setCargando(false);
    }
  };

  return (
    <AuthLayout>
      <form noValidate onSubmit={handleSubmit}>
        <span className="eyebrow">Nueva cuenta</span>
        <h2>Únete a Kem’il</h2>
        <p>Crea tu cuenta para comprar, guardar tu carrito y seguir tus pedidos.</p>
        {CAMPOS.map(([nombre, etiqueta, tipo, autocompletar]) => {
          const mostrarError = tocados[nombre] && errores[nombre];
          return (
            <label className="login-field" key={nombre}>
              <span>{etiqueta}</span>
              <input
                aria-invalid={!!mostrarError}
                autoComplete={autocompletar}
                name={nombre}
                onBlur={tocar}
                onChange={cambiar}
                type={tipo}
                value={form[nombre]}
              />
              {mostrarError && <small className="field-error">{errores[nombre]}</small>}
            </label>
          );
        })}
        {error && (
          <p className="form-error" role="alert">
            {error}
          </p>
        )}
        <button className="button button-primary login-submit" disabled={cargando} type="submit">
          {cargando ? 'Creando cuenta...' : 'Crear cuenta'} <Icon name="arrow" />
        </button>
        <p className="auth-switch">
          ¿Ya tienes cuenta? <Link to="/login">Inicia sesión</Link>
        </p>
      </form>
    </AuthLayout>
  );
}
