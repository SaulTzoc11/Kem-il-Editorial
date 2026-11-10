import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { mensajeDeError } from '../api/axiosClient';
import { ordenesApi } from '../api/ordenesApi';
import BookCover from '../components/BookCover';
import Icon from '../components/Icon';
import QtyStepper from '../components/QtyStepper';
import { useAuth } from '../context/AuthContext';
import { useCart } from '../context/CartContext';
import { formatoQ } from '../utils/format';
import { esCliente, esPersonal, normalizarRol, tipoClienteDe } from '../utils/roles';

// RF-03: carrito + cierre de orden. TODO el cálculo (volumen, tipo de cliente, IVA, total)
// lo hace el backend (DescuentoService); esta vista solo lo muestra.
export default function CartPage() {
  const { cart, updateQuantity, removeFromCart, clearCart, totalCartPrice } = useCart();
  const { user } = useAuth();
  const navigate = useNavigate();
  const tipoCliente = tipoClienteDe(user);

  const [calculo, setCalculo] = useState(null); // { clave, resumen } | { clave, error }
  const [procesando, setProcesando] = useState(false);
  const [errorOrden, setErrorOrden] = useState('');
  const [ordenCreada, setOrdenCreada] = useState(null);

  // Recalcula (con una pausa de 350 ms) cada vez que cambia el carrito o el tipo de cliente
  const clave = JSON.stringify([cart.map((i) => [i.idLibro, i.cantidad, i.precio]), tipoCliente]);
  useEffect(() => {
    if (cart.length === 0) return undefined;
    let vigente = true;
    const temporizador = setTimeout(() => {
      ordenesApi
        .calcular(cart, tipoCliente)
        .then((resumen) => vigente && setCalculo({ clave, resumen }))
        .catch((error) => vigente && setCalculo({ clave, error: mensajeDeError(error, 'No se pudo calcular la orden.') }));
    }, 350);
    return () => {
      vigente = false;
      clearTimeout(temporizador);
    };
  }, [clave]); // eslint-disable-line react-hooks/exhaustive-deps

  const actualizado = calculo?.clave === clave;
  const resumen = calculo?.resumen;
  const errorCalculo = actualizado ? calculo?.error : '';

  const confirmar = async () => {
    setErrorOrden('');
    setProcesando(true);
    try {
      const orden = await ordenesApi.crear(cart);
      setOrdenCreada(orden);
      clearCart();
    } catch (error) {
      setErrorOrden(mensajeDeError(error, 'No se pudo registrar el pedido.'));
    } finally {
      setProcesando(false);
    }
  };

  if (ordenCreada) {
    return (
      <div className="cart-page">
        <div className="order-done">
          <span>
            <Icon name="check" size={30} />
          </span>
          <h1>¡Pedido registrado!</h1>
          <p>
            Gracias por tu compra{ordenCreada.idFactura ? `. Tu número de orden es #${ordenCreada.idFactura}` : ''}.
            Recibirás la confirmación cuando sea aprobado.
          </p>
          <Link className="button button-dark" to="/catalogo" style={{ marginTop: 20 }}>
            Seguir explorando
          </Link>
        </div>
      </div>
    );
  }

  if (cart.length === 0) {
    return (
      <div className="cart-page">
        <div className="empty-state">
          <Icon name="bag" size={32} />
          <h3>Tu carrito está vacío</h3>
          <p>Aún no has agregado ningún libro de Kem’il Editorial.</p>
          <button onClick={() => navigate('/catalogo')}>Ver el catálogo</button>
        </div>
      </div>
    );
  }

  let accion;
  if (!user) {
    accion = (
      <button
        className="button button-primary"
        onClick={() => navigate('/login', { state: { from: '/carrito' } })}
      >
        Inicia sesión para comprar <Icon name="arrow" />
      </button>
    );
  } else if (!esCliente(user.rol)) {
    accion = (
      <>
        <button className="button button-primary" disabled>
          Confirmar pedido
        </button>
        <p className="summary-note">Las cuentas de {normalizarRol(user.rol)} no realizan compras.</p>
      </>
    );
  } else {
    accion = (
      <button
        className="button button-primary"
        disabled={procesando || !actualizado || !resumen}
        onClick={confirmar}
      >
        {procesando ? 'Procesando...' : 'Confirmar pedido'} <Icon name="arrow" />
      </button>
    );
  }

  return (
    <div className="cart-page">
      <h1>Carrito de compras</h1>
      <div className="cart-layout">
        <div>
          <div className="cart-lines">
            {cart.map((item) => (
              <div className="cart-line" key={item.idLibro}>
                <div className="cart-thumb">
                  <BookCover libro={item} />
                </div>
                <div>
                  <h3>{item.nombre}</h3>
                  <small>
                    {item.formato} · {formatoQ(item.precio)} c/u
                  </small>
                  <button className="link-danger" onClick={() => removeFromCart(item.idLibro)}>
                    Quitar
                  </button>
                </div>
                <QtyStepper item={item} onChange={(n) => updateQuantity(item.idLibro, n)} />
                <strong className="line-total">{formatoQ(item.precio * item.cantidad)}</strong>
              </div>
            ))}
          </div>
          <div className="cart-actions">
            <Link to="/catalogo">← Seguir comprando</Link>
            <button onClick={clearCart}>Vaciar carrito</button>
          </div>
        </div>

        <aside className={`order-summary ${resumen && !actualizado ? 'is-loading' : ''}`} aria-live="polite">
          <h2>Resumen de la orden</h2>
          <p>Cliente: {user ? normalizarRol(user.rol) : 'Visitante'}</p>

          {errorCalculo && <p className="form-error" role="alert">{errorCalculo}</p>}

          {resumen ? (
            <>
              <div className="summary-row">
                <span>
                  Subtotal base
                  <small>{resumen.totalUnidades} unidades</small>
                </span>
                <strong>{formatoQ(resumen.subtotalBase)}</strong>
              </div>
              <div className="summary-row is-discount">
                <span>
                  Descuento por volumen ({resumen.porcentajeVolumen}%)
                  <small>{resumen.detalleReglaVolumen}</small>
                </span>
                <strong>− {formatoQ(resumen.descuentoVolumenMonto)}</strong>
              </div>
              <div className="summary-row is-discount">
                <span>
                  Descuento por tipo de cliente ({resumen.porcentajeCliente}%)
                  <small>{resumen.detalleReglaCliente}</small>
                </span>
                <strong>− {formatoQ(resumen.descuentoClienteMonto)}</strong>
              </div>
              <div className="summary-row">
                <span>Subtotal con descuentos</span>
                <strong>{formatoQ(resumen.subtotalConDescuentos)}</strong>
              </div>
              <div className="summary-row">
                <span>Impuestos (IVA 12%)</span>
                <strong>{formatoQ(resumen.impuestos)}</strong>
              </div>
              <div className="summary-total">
                <span>Total final</span>
                <strong>{formatoQ(resumen.totalFinal)}</strong>
              </div>
            </>
          ) : (
            !errorCalculo && (
              <div className="summary-row">
                <span>Calculando descuentos...</span>
                <strong>{formatoQ(totalCartPrice)}</strong>
              </div>
            )
          )}

          {errorOrden && <p className="form-error" role="alert" style={{ marginTop: 16 }}>{errorOrden}</p>}
          {accion}
          {!esPersonal(user?.rol) && (
            <p className="summary-note">Los precios finales los confirma el servidor al registrar el pedido.</p>
          )}
        </aside>
      </div>
    </div>
  );
}
