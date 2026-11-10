import { useNavigate } from 'react-router-dom';
import { useCart } from '../context/CartContext';
import { useUI } from '../context/UIContext';
import useModal from '../hooks/useModal';
import { formatoQ } from '../utils/format';
import BookCover from './BookCover';
import Icon from './Icon';
import QtyStepper from './QtyStepper';

// Vista rápida del carrito. El desglose oficial (descuentos, IVA, total) está en /carrito.
export default function CartDrawer() {
  const { cartOpen, closeCart } = useUI();
  const { cart, totalItems, totalCartPrice, updateQuantity, removeFromCart } = useCart();
  const navigate = useNavigate();
  useModal(cartOpen, closeCart);

  const irACarrito = () => {
    closeCart();
    navigate('/carrito');
  };

  return (
    <div
      aria-hidden={!cartOpen}
      className={cartOpen ? 'drawer-backdrop is-open' : 'drawer-backdrop'}
      onMouseDown={closeCart}
    >
      <aside aria-label="Carrito de compras" className="cart-drawer" onMouseDown={(e) => e.stopPropagation()}>
        <div className="drawer-header">
          <div>
            <span>Tu selección</span>
            <h2>Carrito ({totalItems})</h2>
          </div>
          <button aria-label="Cerrar carrito" onClick={closeCart} tabIndex={cartOpen ? 0 : -1}>
            <Icon name="close" />
          </button>
        </div>
        <div className="drawer-content">
          {cart.length === 0 ? (
            <div className="cart-empty">
              <Icon name="bag" size={32} />
              <h3>Tu carrito está esperando</h3>
              <p>Explora el catálogo y encuentra tu próxima historia.</p>
              <button
                onClick={() => {
                  closeCart();
                  navigate('/catalogo');
                }}
              >
                Ver libros
              </button>
            </div>
          ) : (
            cart.map((item) => (
              <div className="cart-item" key={item.idLibro}>
                <div className="cart-thumb">
                  <BookCover libro={item} />
                </div>
                <div>
                  <strong>{item.nombre}</strong>
                  <span>{item.formato}</span>
                  <QtyStepper item={item} onChange={(n) => updateQuantity(item.idLibro, n)} />
                  <b>{formatoQ(item.precio * item.cantidad)}</b>
                </div>
                <button aria-label={`Quitar ${item.nombre}`} onClick={() => removeFromCart(item.idLibro)}>
                  <Icon name="close" size={17} />
                </button>
              </div>
            ))
          )}
        </div>
        {cart.length > 0 && (
          <div className="drawer-footer">
            <div>
              <span>Subtotal</span>
              <strong>{formatoQ(totalCartPrice)}</strong>
            </div>
            <small>El descuento por volumen y los impuestos se calculan al finalizar.</small>
            <button className="button button-primary" onClick={irACarrito}>
              Ver carrito y continuar <Icon name="arrow" />
            </button>
          </div>
        )}
      </aside>
    </div>
  );
}
