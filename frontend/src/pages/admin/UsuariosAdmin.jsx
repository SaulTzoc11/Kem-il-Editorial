import { useState } from 'react';
import { adminApi } from '../../api/adminApi';
import { mensajeDeError } from '../../api/axiosClient';
import Icon from '../../components/Icon';
import { MockNotice, StatusBadge } from '../../components/portal/Widgets';
import { useUI } from '../../context/UIContext';
import { mockRoles, mockUsuarios } from '../../data/mockPortal';
import useApi from '../../hooks/useApi';
import useModal from '../../hooks/useModal';
import { ROLES, normalizarRol } from '../../utils/roles';
import { useAuth } from '../../context/AuthContext';

const TARJETAS = [
  [ROLES.ADMIN, 'Acceso total al sistema'],
  [ROLES.MAYORISTA, 'Compras por volumen'],
  [ROLES.ASESOR, 'Cotizaciones y clientes'],
  [ROLES.INSTITUCIONAL, 'Compras por volumen'],
  [ROLES.MINORISTA, 'Compras individuales'],
];

// RF-02 / RF-05: administración de usuarios y roles. POST api/auth/usuarios ya existe;
// el listado (GET api/usuarios) y la lista de roles (GET api/roles) están pendientes.
export default function UsuariosAdmin() {
  const { user } = useAuth();
  const { notify } = useUI();
  const { data: usuarios, usingMock, reload } = useApi(adminApi.usuarios, { fallback: mockUsuarios });
  const { data: roles } = useApi(adminApi.roles, { fallback: mockRoles });
  const [creando, setCreando] = useState(false);
  const [trabajando, setTrabajando] = useState(null);

  const lista = usuarios ?? [];
  const cuenta = (rol) => lista.filter((u) => normalizarRol(u.rol) === rol).length;
  const idRolDe = (u) => (roles ?? []).find((r) => r.nombreRol === u.rol)?.idRol ?? '';

  const ejecutar = async (idUsuario, accion, mensaje) => {
    setTrabajando(idUsuario);
    try {
      await accion();
      notify(mensaje);
      reload();
    } catch (err) {
      notify(mensajeDeError(err, 'No se pudo completar la acción.'), 'error');
    } finally {
      setTrabajando(null);
    }
  };

  const cambiarRol = (u, idRol) =>
    ejecutar(u.idUsuario, () => adminApi.cambiarRol(u.idUsuario, Number(idRol)), `Rol de ${u.nombre} actualizado`);

  const cambiarEstado = (u) =>
    ejecutar(
      u.idUsuario,
      () => adminApi.cambiarEstado(u.idUsuario, !u.estado),
      u.estado ? `${u.nombre} desactivado` : `${u.nombre} activado`
    );

  return (
    <>
      {usingMock && <MockNotice endpoint="GET /api/usuarios" />}
      <section className="portal-panel role-management">
        <div className="panel-heading">
          <div>
            <span>Control de acceso</span>
            <h2>Usuarios y roles</h2>
          </div>
          <button className="portal-primary" onClick={() => setCreando(true)}>
            <Icon name="plus" /> Nuevo usuario
          </button>
        </div>
        <div className="role-grid">
          {TARJETAS.map(([rol, detalle], i) => (
            <article key={rol}>
              <span className={`role-symbol role-${i + 1}`}>{rol.split(' ').pop().slice(0, 2).toUpperCase()}</span>
              <div>
                <strong>{rol}</strong>
                <p>{detalle}</p>
                <small>{cuenta(rol)} usuarios</small>
              </div>
            </article>
          ))}
        </div>
        <div className="portal-table-wrap">
          <table className="portal-table">
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Correo</th>
                <th>Rol</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {lista.map((u) => {
                const esYo = u.idUsuario === user?.idUsuario;
                const bloqueado = usingMock || trabajando === u.idUsuario;
                return (
                  <tr key={u.idUsuario}>
                    <td>
                      <strong>{u.nombre}</strong>
                    </td>
                    <td>{u.correo}</td>
                    <td>
                      <select
                        aria-label={`Rol de ${u.nombre}`}
                        disabled={bloqueado || esYo}
                        onChange={(e) => cambiarRol(u, e.target.value)}
                        value={idRolDe(u)}
                      >
                        {(roles ?? []).map((r) => (
                          <option key={r.idRol} value={r.idRol}>
                            {normalizarRol(r.nombreRol)}
                          </option>
                        ))}
                      </select>
                    </td>
                    <td>
                      <StatusBadge>{u.estado ? 'Aprobado' : 'Cancelado'}</StatusBadge>
                    </td>
                    <td>
                      {esYo ? (
                        <small>Tu cuenta</small>
                      ) : (
                        <button
                          className={`portal-secondary ${u.estado ? 'is-danger' : ''}`}
                          disabled={bloqueado}
                          onClick={() => cambiarEstado(u)}
                        >
                          {u.estado ? 'Desactivar' : 'Activar'}
                        </button>
                      )}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </section>
      {creando && (
        <NuevoUsuario
          onClose={() => setCreando(false)}
          onSaved={() => {
            setCreando(false);
            reload();
          }}
        />
      )}
    </>
  );
}

function NuevoUsuario({ onClose, onSaved }) {
  const { notify } = useUI();
  const { data: roles, usingMock } = useApi(adminApi.roles, { fallback: mockRoles });
  const [form, setForm] = useState({ nombre: '', correo: '', password: '', idRol: '' });
  const [error, setError] = useState('');
  const [guardando, setGuardando] = useState(false);
  useModal(true, onClose);

  const cambiar = (e) => setForm({ ...form, [e.target.name]: e.target.value });

  const guardar = async (e) => {
    e.preventDefault();
    setError('');
    setGuardando(true);
    try {
      await adminApi.crearUsuario({ ...form, idRol: Number(form.idRol) });
      notify('Usuario creado');
      onSaved();
    } catch (err) {
      setError(mensajeDeError(err, 'No se pudo crear el usuario.'));
    } finally {
      setGuardando(false);
    }
  };

  return (
    <div className="modal-backdrop" onMouseDown={onClose}>
      <section aria-label="Nuevo usuario" aria-modal="true" className="quote-modal" onMouseDown={(e) => e.stopPropagation()} role="dialog">
        <button aria-label="Cerrar" className="modal-close" onClick={onClose}>
          <Icon name="close" />
        </button>
        <span className="eyebrow">Control de acceso</span>
        <h2>Nuevo usuario</h2>
        <p>Crea cuentas de personal u otros administradores. La contraseña se guarda cifrada (BCrypt).</p>
        {usingMock && <MockNotice endpoint="GET /api/roles" />}
        <form onSubmit={guardar}>
          <label>
            <span>Nombre completo</span>
            <input minLength={2} name="nombre" onChange={cambiar} required value={form.nombre} />
          </label>
          <label>
            <span>Correo electrónico</span>
            <input name="correo" onChange={cambiar} required type="email" value={form.correo} />
          </label>
          <div className="form-row">
            <label>
              <span>Contraseña (mín. 8)</span>
              <input autoComplete="new-password" minLength={8} name="password" onChange={cambiar} required type="password" value={form.password} />
            </label>
            <label>
              <span>Rol</span>
              <select name="idRol" onChange={cambiar} required value={form.idRol}>
                <option value="">Selecciona...</option>
                {(roles ?? []).map((r) => (
                  <option key={r.idRol} value={r.idRol}>
                    {normalizarRol(r.nombreRol)}
                  </option>
                ))}
              </select>
            </label>
          </div>
          {error && <p className="form-error" role="alert">{error}</p>}
          <button className="button button-primary" disabled={guardando} type="submit">
            {guardando ? 'Creando...' : 'Crear usuario'} <Icon name="check" />
          </button>
        </form>
      </section>
    </div>
  );
}
