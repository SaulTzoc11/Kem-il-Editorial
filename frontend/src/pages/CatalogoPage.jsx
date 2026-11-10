import { useMemo } from 'react';
import { useSearchParams } from 'react-router-dom';
import { librosApi } from '../api/librosApi';
import BookCard, { BookGridSkeleton } from '../components/BookCard';
import Icon from '../components/Icon';
import useApi from '../hooks/useApi';
import { normalizarTexto } from '../utils/format';

const TODOS = 'Todos';

// RF-01.2 Búsqueda en tiempo real por título, autor, ISBN o editorial + filtros combinables
// por género, rango de precio y formato. El estado de los filtros vive en la URL (?q=&genero=&formato=&min=&max=),
// así se puede compartir o recargar la página sin perderlos.
export default function CatalogoPage() {
  const [params, setParams] = useSearchParams();
  const { data: libros, loading, error, reload } = useApi(librosApi.listar);

  const q = params.get('q') ?? '';
  const genero = params.get('genero') ?? TODOS;
  const formato = params.get('formato') ?? TODOS;
  const min = params.get('min') ?? '';
  const max = params.get('max') ?? '';

  const cambiar = (clave, valor) => {
    const siguiente = new URLSearchParams(params);
    if (!valor || valor === TODOS) siguiente.delete(clave);
    else siguiente.set(clave, valor);
    setParams(siguiente, { replace: true });
  };

  // Géneros y formatos salen de los datos reales (no se escriben a mano)
  const generos = useMemo(() => [TODOS, ...new Set((libros ?? []).map((l) => l.genero))], [libros]);
  const formatos = useMemo(
    () => [TODOS, ...new Set((libros ?? []).map((l) => l.formato).filter(Boolean))],
    [libros]
  );

  const filtrados = useMemo(() => {
    const texto = normalizarTexto(q.trim());
    return (libros ?? []).filter(
      (l) =>
        (!texto || normalizarTexto(`${l.nombre} ${l.autor} ${l.isbn} ${l.editorial}`).includes(texto)) &&
        (genero === TODOS || l.genero === genero) &&
        (formato === TODOS || l.formato === formato) &&
        (min === '' || l.precio >= Number(min)) &&
        (max === '' || l.precio <= Number(max))
    );
  }, [libros, q, genero, formato, min, max]);

  const hayFiltros = q || genero !== TODOS || formato !== TODOS || min !== '' || max !== '';

  return (
    <section className="catalog-section is-page">
      <div className="section-heading">
        <div>
          <span className="eyebrow">Catálogo bibliográfico</span>
          <h2>Encuentra tu próxima historia</h2>
        </div>
      </div>

      <div className="catalog-tools">
        <label className="search-box">
          <Icon name="search" />
          <input
            aria-label="Buscar libros"
            onChange={(e) => cambiar('q', e.target.value)}
            placeholder="Busca por título, autor, ISBN o editorial..."
            type="search"
            value={q}
          />
        </label>
        <label className="select-box">
          <span>Formato:</span>
          <select aria-label="Filtrar por formato" onChange={(e) => cambiar('formato', e.target.value)} value={formato}>
            {formatos.map((f) => (
              <option key={f}>{f}</option>
            ))}
          </select>
        </label>
        <label className="select-box">
          <span>Precio Q</span>
          <input aria-label="Precio mínimo" min="0" onChange={(e) => cambiar('min', e.target.value)} placeholder="Mín." type="number" value={min} />
          <span>–</span>
          <input aria-label="Precio máximo" min="0" onChange={(e) => cambiar('max', e.target.value)} placeholder="Máx." type="number" value={max} />
        </label>
      </div>

      <div aria-label="Géneros literarios" className="category-tabs" role="group">
        {generos.map((g) => (
          <button className={genero === g ? 'active' : ''} key={g} onClick={() => cambiar('genero', g)}>
            {g}
          </button>
        ))}
      </div>

      {libros && (
        <div className="results-meta">
          <span aria-live="polite">
            {filtrados.length} {filtrados.length === 1 ? 'resultado' : 'resultados'}
          </span>
          {hayFiltros && <button onClick={() => setParams({}, { replace: true })}>Limpiar filtros</button>}
        </div>
      )}

      {loading && <BookGridSkeleton cantidad={8} />}

      {error && (
        <div className="empty-state">
          <h3>No pudimos cargar el catálogo</h3>
          <p>Revisa que el servidor esté en línea e inténtalo de nuevo.</p>
          <button onClick={reload}>Reintentar</button>
        </div>
      )}

      {libros &&
        (filtrados.length ? (
          <div className="book-grid">
            {filtrados.map((libro) => (
              <BookCard key={libro.idLibro} libro={libro} />
            ))}
          </div>
        ) : (
          <div className="empty-state">
            <Icon name="search" size={28} />
            <h3>No encontramos ese libro</h3>
            <p>Prueba con otra palabra o ajusta los filtros.</p>
            <button onClick={() => setParams({}, { replace: true })}>Limpiar filtros</button>
          </div>
        ))}
    </section>
  );
}
