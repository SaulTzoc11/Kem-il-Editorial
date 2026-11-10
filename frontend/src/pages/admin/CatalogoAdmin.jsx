import { useMemo, useState } from 'react';
import { adminApi } from '../../api/adminApi';
import { mensajeDeError } from '../../api/axiosClient';
import { estadoDeStock, librosApi } from '../../api/librosApi';
import Icon from '../../components/Icon';
import { MockNotice, StatusBadge } from '../../components/portal/Widgets';
import { useUI } from '../../context/UIContext';
import { mockEditoriales, mockGeneros } from '../../data/mockPortal';
import useApi from '../../hooks/useApi';
import useModal from '../../hooks/useModal';
import { formatoQ, normalizarTexto } from '../../utils/format';

const FORMATOS = ['Tapa blanda', 'Tapa dura', 'E-book'];

const formularioDe = (libro) => ({
  nombre: libro?.nombre ?? '',
  descripcion: libro?.descripcion ?? '',
  formato: libro?.formato || FORMATOS[0],
  precio: libro?.precio ?? '',
  isbn: libro?.isbn ?? '',
  idGenero: libro?.idGenero ?? '',
  idEditorial: libro?.idEditorial ?? '',
  stock: libro?.stock ?? '',
  autor: libro?.autor ?? '',
  portada: libro?.portada ?? '',
});

// RF-05.1 CRUD de catálogo. Leer y crear usan endpoints existentes; editar y eliminar (lógico)
// llaman PUT/DELETE api/libros/{id}, que el backend debe agregar.
export default function CatalogoAdmin() {
  const { notify } = useUI();
  const { data: libros, loading, error, reload } = useApi(librosApi.listar);
  const [busqueda, setBusqueda] = useState('');
  const [genero, setGenero] = useState('Todos');
  const [editando, setEditando] = useState(null); // null = cerrado, {} = nuevo, libro = edición

  const generos = useMemo(() => ['Todos', ...new Set((libros ?? []).map((l) => l.genero))], [libros]);
  const filtrados = useMemo(() => {
    const texto = normalizarTexto(busqueda.trim());
    return (libros ?? []).filter(
      (l) =>
        (!texto || normalizarTexto(`${l.nombre} ${l.autor} ${l.isbn}`).includes(texto)) &&
        (genero === 'Todos' || l.genero === genero)
    );
  }, [libros, busqueda, genero]);

  const eliminar = async (libro) => {
    if (!window.confirm(`¿Eliminar “${libro.nombre}” del catálogo?`)) return;
    try {
      await librosApi.eliminar(libro.idLibro);
      notify('Libro eliminado del catálogo');
      reload();
    } catch (err) {
      notify(mensajeDeError(err, 'No se pudo eliminar el libro.'), 'error');
    }
  };

  return (
    <>
      <section className="portal-panel table-panel standalone-panel">
        <div className="panel-heading">
          <div>
            <span>{libros ? `${libros.length} publicaciones` : 'Catálogo'}</span>
            <h2>Gestión del catálogo</h2>
          </div>
          <button className="portal-primary" onClick={() => setEditando({})}>
            <Icon name="plus" /> Nuevo libro
          </button>
        </div>
        <div className="portal-filter-row">
          <label>
            <Icon name="search" />
            <input
              aria-label="Buscar libros"
              onChange={(e) => setBusqueda(e.target.value)}
              placeholder="Buscar por título, autor o ISBN"
              value={busqueda}
            />
          </label>
          <select aria-label="Filtrar género" onChange={(e) => setGenero(e.target.value)} value={genero}>
            {generos.map((g) => (
              <option key={g} value={g}>
                {g === 'Todos' ? 'Todos los géneros' : g}
              </option>
            ))}
          </select>
        </div>
        {loading && <p className="portal-empty">Cargando catálogo...</p>}
        {error && (
          <p className="portal-empty">
            No se pudo cargar el catálogo. <button className="table-action" onClick={reload}>Reintentar</button>
          </p>
        )}
        {libros && (
          <div className="portal-table-wrap">
            <table className="portal-table">
              <thead>
                <tr>
                  <th>Título</th>
                  <th>Género</th>
                  <th>Stock</th>
                  <th>Precio</th>
                  <th>Estado</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {filtrados.map((libro) => (
                  <tr key={libro.idLibro}>
                    <td>
                      <strong>{libro.nombre}</strong>
                    </td>
                    <td>{libro.genero}</td>
                    <td>{libro.stock ?? '—'}</td>
                    <td>{formatoQ(libro.precio)}</td>
                    <td>
                      <StatusBadge>{estadoDeStock(libro)}</StatusBadge>
                    </td>
                    <td>
                      <button className="table-action" onClick={() => setEditando(libro)}>
                        Editar
                      </button>
                      <button className="table-action is-danger" onClick={() => eliminar(libro)}>
                        Eliminar
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {filtrados.length === 0 && <p className="portal-empty">No hay libros que coincidan.</p>}
          </div>
        )}
      </section>
      {editando && (
        <LibroForm
          libro={editando.idLibro ? editando : null}
          onClose={() => setEditando(null)}
          onSaved={() => {
            setEditando(null);
            reload();
          }}
        />
      )}
    </>
  );
}

function LibroForm({ libro, onClose, onSaved }) {
  const { notify } = useUI();
  const [form, setForm] = useState(() => formularioDe(libro));
  const [error, setError] = useState('');
  const [guardando, setGuardando] = useState(false);
  const { data: generos, usingMock: generosMock } = useApi(adminApi.generos, { fallback: mockGeneros });
  const { data: editoriales } = useApi(adminApi.editoriales, { fallback: mockEditoriales });
  useModal(true, onClose);

  const cambiar = (e) => setForm({ ...form, [e.target.name]: e.target.value });

  const guardar = async (e) => {
    e.preventDefault();
    setError('');
    setGuardando(true);
    // Los campos stock/autor/portada se envían ya; el backend los ignora hasta que el DTO los incluya.
    const payload = {
      ...form,
      precio: Number(form.precio),
      idGenero: Number(form.idGenero),
      idEditorial: Number(form.idEditorial),
      stock: form.stock === '' ? null : Number(form.stock),
    };
    try {
      if (libro) await librosApi.actualizar(libro.idLibro, payload);
      else await librosApi.crear(payload);
      notify(libro ? 'Libro actualizado' : 'Libro creado con éxito');
      onSaved();
    } catch (err) {
      setError(mensajeDeError(err, 'No se pudo guardar el libro.'));
    } finally {
      setGuardando(false);
    }
  };

  return (
    <div className="modal-backdrop" onMouseDown={onClose}>
      <section
        aria-label={libro ? 'Editar libro' : 'Nuevo libro'}
        aria-modal="true"
        className="quote-modal"
        onMouseDown={(e) => e.stopPropagation()}
        role="dialog"
      >
        <button aria-label="Cerrar" className="modal-close" onClick={onClose}>
          <Icon name="close" />
        </button>
        <span className="eyebrow">Catálogo</span>
        <h2>{libro ? 'Editar libro' : 'Nuevo libro'}</h2>
        <p>Completa la ficha bibliográfica de la obra.</p>
        {generosMock && <MockNotice endpoint="GET /api/generos" />}
        <form onSubmit={guardar}>
          <label>
            <span>Título</span>
            <input maxLength={200} name="nombre" onChange={cambiar} required value={form.nombre} />
          </label>
          <div className="form-row">
            <label>
              <span>Autor</span>
              <input name="autor" onChange={cambiar} value={form.autor} />
            </label>
            <label>
              <span>ISBN</span>
              <input name="isbn" onChange={cambiar} required value={form.isbn} />
            </label>
          </div>
          <div className="form-row">
            <label>
              <span>Género</span>
              <select name="idGenero" onChange={cambiar} required value={form.idGenero}>
                <option value="">Selecciona...</option>
                {(generos ?? []).map((g) => (
                  <option key={g.idGenero} value={g.idGenero}>
                    {g.nombre}
                  </option>
                ))}
              </select>
            </label>
            <label>
              <span>Editorial</span>
              <select name="idEditorial" onChange={cambiar} required value={form.idEditorial}>
                <option value="">Selecciona...</option>
                {(editoriales ?? []).map((ed) => (
                  <option key={ed.idEditorial} value={ed.idEditorial}>
                    {ed.nombre}
                  </option>
                ))}
              </select>
            </label>
          </div>
          <div className="form-row">
            <label>
              <span>Formato</span>
              <select name="formato" onChange={cambiar} value={form.formato}>
                {[...new Set([...FORMATOS, form.formato])].map((f) => (
                  <option key={f}>{f}</option>
                ))}
              </select>
            </label>
            <label>
              <span>Precio base (Q)</span>
              <input min="0" name="precio" onChange={cambiar} required step="0.01" type="number" value={form.precio} />
            </label>
            <label>
              <span>Stock</span>
              <input min="0" name="stock" onChange={cambiar} type="number" value={form.stock} />
            </label>
          </div>
          <label>
            <span>URL de portada</span>
            <input name="portada" onChange={cambiar} placeholder="https://..." value={form.portada} />
          </label>
          <label>
            <span>Sinopsis</span>
            <textarea name="descripcion" onChange={cambiar} rows={3} value={form.descripcion} />
          </label>
          {error && <p className="form-error" role="alert">{error}</p>}
          <button className="button button-primary" disabled={guardando} type="submit">
            {guardando ? 'Guardando...' : 'Guardar libro'} <Icon name="check" />
          </button>
        </form>
      </section>
    </div>
  );
}
