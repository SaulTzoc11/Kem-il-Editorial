import { useCart } from '../context/CartContext';
import { useUI } from '../context/UIContext';
import useModal from '../hooks/useModal';
import { formatoQ } from '../utils/format';
import BookCover from './BookCover';
import Icon from './Icon';

// RF-01.3 Ficha técnica: portada, sinopsis, autor, año, stock, precio base y ficha bibliográfica.
export default function BookModal() {
  const { selectedBook: libro, closeBook, notify } = useUI();
  const { addToCart } = useCart();
  useModal(!!libro, closeBook);
  if (!libro) return null;

  const agotado = libro.stock === 0;
  const stockTexto =
    libro.stock === null
      ? 'Consultar disponibilidad'
      : agotado
        ? 'Agotado'
        : libro.stock <= libro.stockMinimo
          ? `Últimas ${libro.stock} unidades`
          : `En stock (${libro.stock})`;
  const stockClase = agotado ? 'is-out' : libro.stock !== null && libro.stock <= libro.stockMinimo ? 'is-low' : '';

  const ficha = [
    ['Autor', libro.autor],
    ['Editorial', libro.editorial],
    ['Año de edición', libro.anio],
    ['Edición', libro.edicion],
    ['ISBN', libro.isbn],
    ['Formato', libro.formato],
  ].filter(([, valor]) => valor);

  const agregar = () => {
    if (addToCart(libro)) {
      notify(`“${libro.nombre}” se agregó a tu carrito`);
      closeBook();
    } else {
      notify('Ya tienes todo el stock disponible de este libro en tu carrito', 'error');
    }
  };

  return (
    <div className="modal-backdrop" onMouseDown={closeBook}>
      <section
        aria-label={`Ficha de ${libro.nombre}`}
        aria-modal="true"
        className="book-modal"
        onMouseDown={(e) => e.stopPropagation()}
        role="dialog"
      >
        <button aria-label="Cerrar" className="modal-close" onClick={closeBook}>
          <Icon name="close" />
        </button>
        <div className="book-modal-cover">
          <BookCover libro={libro} />
        </div>
        <div>
          <span className="eyebrow">{libro.genero}</span>
          <h2>{libro.nombre}</h2>
          {libro.autor && <p className="modal-author">por {libro.autor}</p>}
          <p>{libro.descripcion || 'Sin sinopsis disponible.'}</p>
          <dl>
            {ficha.map(([etiqueta, valor]) => (
              <div key={etiqueta}>
                <dt>{etiqueta}</dt>
                <dd>{valor}</dd>
              </div>
            ))}
            <div>
              <dt>Disponibilidad</dt>
              <dd className={`stock ${stockClase}`}>{stockTexto}</dd>
            </div>
          </dl>
          <div className="modal-purchase">
            <strong>{formatoQ(libro.precio)}</strong>
            <button className="button button-primary" disabled={agotado} onClick={agregar}>
              {agotado ? 'No disponible' : 'Agregar al carrito'} <Icon name="bag" />
            </button>
          </div>
        </div>
      </section>
    </div>
  );
}
