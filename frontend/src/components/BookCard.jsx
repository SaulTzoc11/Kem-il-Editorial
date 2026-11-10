import { useCart } from '../context/CartContext';
import { useUI } from '../context/UIContext';
import { formatoQ } from '../utils/format';
import { acentoDe } from '../utils/covers';
import BookCover from './BookCover';
import Icon from './Icon';

export default function BookCard({ libro }) {
  const { addToCart } = useCart();
  const { openBook, notify } = useUI();
  const agotado = libro.stock === 0;

  const agregar = () => {
    if (addToCart(libro)) notify(`“${libro.nombre}” se agregó a tu carrito`);
    else notify('Ya tienes todo el stock disponible de este libro en tu carrito', 'error');
  };

  return (
    <article className="book-card">
      <button
        aria-label={`Ver ficha de ${libro.nombre}`}
        className={`book-cover ${acentoDe(libro)}`}
        onClick={() => openBook(libro)}
      >
        <BookCover libro={libro} />
        {agotado && <span className="book-badge is-out">Agotado</span>}
        <span className="quick-view">Ver ficha técnica</span>
      </button>
      <div className="book-meta">
        <span className="book-category">
          {libro.genero}
          {libro.formato && ` · ${libro.formato}`}
        </span>
        <button className="book-title" onClick={() => openBook(libro)}>
          {libro.nombre}
        </button>
        <p>{libro.autor}</p>
        <div className="book-bottom">
          <span className="price">{formatoQ(libro.precio)}</span>
          {libro.valoracion && <span className="rating">★ {Number(libro.valoracion).toFixed(1)}</span>}
          <button
            aria-label={`Agregar ${libro.nombre} al carrito`}
            className="add-button"
            disabled={agotado}
            onClick={agregar}
          >
            <Icon name="plus" size={19} />
          </button>
        </div>
      </div>
    </article>
  );
}

export function BookGridSkeleton({ cantidad = 4 }) {
  return (
    <div aria-busy="true" aria-label="Cargando libros" className="book-grid">
      {Array.from({ length: cantidad }, (_, i) => (
        <div className="skeleton-card" key={i}>
          <div className="skeleton" />
          <div className="skeleton" />
          <div className="skeleton" />
        </div>
      ))}
    </div>
  );
}
