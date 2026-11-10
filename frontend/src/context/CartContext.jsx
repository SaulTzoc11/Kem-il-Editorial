import { createContext, useContext, useEffect, useState } from 'react';
import { useAuth } from './AuthContext';

const CartContext = createContext();

// RF-03.1: el estado del carrito persiste por sesión de usuario.
// Cada usuario tiene su propio carrito; el de visitante ("anon") se fusiona al iniciar sesión.
const ANON = 'kemil_cart_anon';
const claveDe = (user) => (user ? `kemil_cart_${user.idUsuario}` : ANON);

const leer = (clave) => {
  try {
    const items = JSON.parse(localStorage.getItem(clave));
    return Array.isArray(items) ? items : [];
  } catch {
    return [];
  }
};

const fusionar = (base, extra) => {
  const resultado = [...base];
  extra.forEach((item) => {
    const existente = resultado.find((i) => i.idLibro === item.idLibro);
    if (existente) existente.cantidad += item.cantidad;
    else resultado.push({ ...item });
  });
  return resultado.map((i) => ({ ...i, cantidad: tope(i, i.cantidad) }));
};

// Nunca más unidades que el stock (si el backend lo informa)
const tope = (item, cantidad) => (item.stock != null ? Math.min(cantidad, item.stock) : cantidad);

export const CartProvider = ({ children }) => {
  const { user } = useAuth();
  const clave = claveDe(user);
  const [estado, setEstado] = useState(() => ({ clave, items: leer(clave) }));

  // Cambió el usuario (login / logout): cargar su carrito
  if (estado.clave !== clave) {
    const eraVisitante = estado.clave === ANON;
    const items = eraVisitante && user ? fusionar(leer(clave), estado.items) : leer(clave);
    setEstado({ clave, items });
  }

  useEffect(() => {
    localStorage.setItem(estado.clave, JSON.stringify(estado.items));
    if (estado.clave !== ANON) localStorage.removeItem(ANON); // ya fusionado
  }, [estado]);

  const cart = estado.items;
  const actualizar = (fn) => setEstado((e) => ({ ...e, items: fn(e.items) }));

  // Devuelve false si ya se alcanzó el stock disponible
  const addToCart = (libro, cantidad = 1) => {
    const actual = cart.find((i) => i.idLibro === libro.idLibro);
    const stock = libro.stock ?? actual?.stock ?? null;
    const deseada = (actual?.cantidad ?? 0) + cantidad;
    if (stock !== null && deseada > stock) return false;

    actualizar((items) =>
      actual
        ? items.map((i) => (i.idLibro === libro.idLibro ? { ...i, cantidad: deseada, stock } : i))
        : [
            ...items,
            {
              idLibro: libro.idLibro,
              nombre: libro.nombre,
              precio: libro.precio,
              formato: libro.formato,
              portada: libro.portada,
              stock,
              cantidad: cantidad,
            },
          ]
    );
    return true;
  };

  const updateQuantity = (idLibro, cantidad) => {
    if (cantidad <= 0) return removeFromCart(idLibro);
    actualizar((items) => items.map((i) => (i.idLibro === idLibro ? { ...i, cantidad: tope(i, cantidad) } : i)));
  };

  const removeFromCart = (idLibro) => actualizar((items) => items.filter((i) => i.idLibro !== idLibro));
  const clearCart = () => actualizar(() => []);

  const totalItems = cart.reduce((total, i) => total + i.cantidad, 0);
  // Solo informativo: el total oficial lo calcula el backend (motor de descuentos)
  const totalCartPrice = cart.reduce((total, i) => total + i.precio * i.cantidad, 0);

  return (
    <CartContext.Provider
      value={{ cart, addToCart, updateQuantity, removeFromCart, clearCart, totalItems, totalCartPrice }}
    >
      {children}
    </CartContext.Provider>
  );
};

// eslint-disable-next-line react-refresh/only-export-components
export const useCart = () => useContext(CartContext);
