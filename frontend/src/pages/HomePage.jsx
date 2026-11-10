import { useMemo } from 'react';
import { Link } from 'react-router-dom';
import { librosApi } from '../api/librosApi';
import BookCard, { BookGridSkeleton } from '../components/BookCard';
import Icon from '../components/Icon';
import { useUI } from '../context/UIContext';
import useApi from '../hooks/useApi';

// RF-01.1 Vista principal: publicaciones destacadas, novedades del mes y banners promocionales.
export default function HomePage() {
  const { openQuote } = useUI();
  const { data: libros, loading, error, reload } = useApi(librosApi.listar);

  // Destacados: mejor valorados (si el backend informa valoración); si no, los primeros del catálogo
  const destacados = useMemo(
    () => [...(libros ?? [])].sort((a, b) => (b.valoracion ?? 0) - (a.valoracion ?? 0)).slice(0, 4),
    [libros]
  );

  return (
    <>
      <section className="hero">
        <div className="hero-pattern" />
        <div className="hero-copy">
          <span className="eyebrow light">
            <Icon name="spark" size={17} /> Editorial independiente
          </span>
          <h1>Historias que nacen de nuestra tierra</h1>
          <p>
            Libros que guardan memoria, despiertan preguntas y conectan generaciones. Descubre voces que
            merecen ser leídas.
          </p>
          <div className="hero-actions">
            <Link className="button button-primary" to="/catalogo">
              Explorar catálogo <Icon name="arrow" />
            </Link>
            <button className="button button-ghost" onClick={openQuote}>
              Solicitar cotización
            </button>
          </div>
        </div>
        <div className="hero-visual">
          <div className="hero-image-wrap">
            <div className="hero-art" aria-hidden="true">
              <div className="mini-book one">
                <span>
                  VOCES
                  <br />
                  DEL
                  <br />
                  VIENTO
                </span>
              </div>
              <div className="mini-book two">
                <span>
                  MEMORIA
                  <br />
                  VIVA
                </span>
              </div>
            </div>
            <div className="floating-note">
              <span className="note-icon">
                <Icon name="book" />
              </span>
              <span>
                <strong>Ediciones cuidadas</strong>Del autor a tus manos
              </span>
            </div>
          </div>
        </div>
      </section>

      <section aria-label="Beneficios" className="value-strip">
        <div>
          <Icon name="book" />
          <span>
            <strong>Curaduría editorial</strong>Historias elegidas con propósito
          </span>
        </div>
        <div>
          <Icon name="shield" />
          <span>
            <strong>Compra segura</strong>Protegemos todos tus datos
          </span>
        </div>
        <div>
          <Icon name="spark" />
          <span>
            <strong>Ediciones especiales</strong>Diseñadas para conservar
          </span>
        </div>
      </section>

      <section className="catalog-section" id="destacados">
        <div className="section-heading">
          <div>
            <span className="eyebrow">Publicaciones destacadas</span>
            <h2>Encuentra tu próxima historia</h2>
          </div>
          <Link className="text-link" to="/catalogo">
            Ver catálogo completo <Icon name="arrow" size={18} />
          </Link>
        </div>
        <div style={{ marginTop: 40 }}>
          {loading && <BookGridSkeleton />}
          {error && (
            <div className="empty-state">
              <h3>No pudimos cargar los libros</h3>
              <p>Revisa que el servidor esté en línea.</p>
              <button onClick={reload}>Reintentar</button>
            </div>
          )}
          {libros && (
            <div className="book-grid">
              {destacados.map((libro) => (
                <BookCard key={libro.idLibro} libro={libro} />
              ))}
            </div>
          )}
        </div>
      </section>

      <section className="featured-section" id="novedades">
        <div className="featured-art">
          <div className="mini-book one">
            <span>
              VOCES
              <br />
              DEL
              <br />
              VIENTO
            </span>
          </div>
          <div className="mini-book two">
            <span>
              MEMORIA
              <br />
              VIVA
            </span>
          </div>
          <div className="sun-shape" />
        </div>
        <div className="featured-copy">
          <span className="eyebrow light">Novedades del mes</span>
          <h2>Voces que cruzan fronteras</h2>
          <p>
            Una colección dedicada a relatos contemporáneos. Ediciones de autor producidas con materiales
            responsables.
          </p>
          <ul>
            <li>
              <Icon name="check" size={18} /> Ediciones limitadas
            </li>
            <li>
              <Icon name="check" size={18} /> Separadores ilustrados de regalo
            </li>
          </ul>
          <Link className="button button-light" to="/catalogo">
            Descubrir colección <Icon name="arrow" />
          </Link>
        </div>
      </section>

      <section className="institutional-section" id="instituciones">
        <div className="institutional-copy">
          <span className="eyebrow">Para organizaciones</span>
          <h2>Libros que transforman comunidades</h2>
          <p>
            Soluciones editoriales para colegios, universidades, bibliotecas y empresas. Cuéntanos qué
            necesitas y nuestro equipo te acompañará.
          </p>
          <div className="benefit-list">
            <div>
              <span>01</span>
              <p>
                <strong>Precios por volumen</strong>Descuentos automáticos desde 6 unidades.
              </p>
            </div>
            <div>
              <span>02</span>
              <p>
                <strong>Ediciones personalizadas</strong>Acabados, papel y portadas según tu proyecto.
              </p>
            </div>
            <div>
              <span>03</span>
              <p>
                <strong>Asesoría dedicada</strong>Seguimiento de principio a fin con un asesor.
              </p>
            </div>
          </div>
          <button className="button button-dark" onClick={openQuote}>
            Solicitar una cotización <Icon name="arrow" />
          </button>
        </div>
        <div className="discount-card">
          <span className="discount-label">Beneficio institucional</span>
          <strong>Hasta 15%</strong>
          <p>de descuento por volumen</p>
          <div className="discount-lines">
            <span>
              <b>6–20 unidades</b>
              <em>5%</em>
            </span>
            <span>
              <b>21–50 unidades</b>
              <em>10%</em>
            </span>
            <span>
              <b>Más de 50</b>
              <em>15%</em>
            </span>
          </div>
          <small>Clientes institucionales reciben 8% adicional sobre el precio ya descontado.</small>
        </div>
      </section>

      <section className="story-section" id="nosotros">
        <div className="story-quote">“</div>
        <blockquote>
          Creemos que cada libro es un hilo. Juntos, esos hilos cuentan quiénes fuimos, quiénes somos y todo lo
          que aún podemos imaginar.
        </blockquote>
        <p>Equipo editorial Kem’il</p>
      </section>
    </>
  );
}
