import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function RegistroPage() {
  const { registro } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({ nombre: '', correo: '', password: '', confirmar: '' });
  const [error, setError] = useState('');
  const [cargando, setCargando] = useState(false);

  const cambiar = (e) => setForm({ ...form, [e.target.name]: e.target.value });

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    if (form.password.length < 8) return setError('La contraseña debe tener al menos 8 caracteres.');
    if (form.password !== form.confirmar) return setError('Las contraseñas no coinciden.');

    setCargando(true);
    try {
      await registro(form.nombre, form.correo, form.password);
      navigate('/', { replace: true });
    } catch (err) {
      const errores = err.response?.data?.errors;
      setError(
        err.response?.data?.mensaje ||
        (errores && Object.values(errores).flat()[0]) ||
        'No se pudo crear la cuenta.'
      );
    } finally {
      setCargando(false);
    }
  };

  return (
    <div style={{ maxWidth: 380, margin: '60px auto', padding: 20 }}>
      <h1>Crear cuenta</h1>
      <form onSubmit={handleSubmit} style={{ display: 'grid', gap: 12 }}>
        <input name="nombre" placeholder="Nombre completo" value={form.nombre} onChange={cambiar} required />
        <input name="correo" type="email" placeholder="Correo" value={form.correo} onChange={cambiar} required />
        <input name="password" type="password" placeholder="Contraseña (mín. 8 caracteres)" value={form.password} onChange={cambiar} required />
        <input name="confirmar" type="password" placeholder="Confirmar contraseña" value={form.confirmar} onChange={cambiar} required />
        {error && <p style={{ color: 'crimson', margin: 0 }}>{error}</p>}
        <button type="submit" disabled={cargando}>{cargando ? 'Creando...' : 'Registrarme'}</button>
      </form>
      <p>¿Ya tienes cuenta? <Link to="/login">Inicia sesión</Link></p>
    </div>
  );
}