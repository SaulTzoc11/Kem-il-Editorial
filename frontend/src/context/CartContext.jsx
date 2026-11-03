import { createContext, useContext, useState, useEffect } from 'react';

const CartContext = createContext();

export const CartProvider = ({ children }) => {
  const [cart, setCart] = useState(() => {
    const savedCart = localStorage.getItem('kemil_cart');
    return savedCart ? JSON.parse(savedCart) : [];
  });

  useEffect(() => {
    localStorage.setItem('kemil_cart', JSON.stringify(cart));
  }, [cart]);

  const addToCart = (libro) => {
    setCart((prevCart) => {
      const existingItem = prevCart.find((item) => item.idLibro === libro.idLibro);
      if (existingItem) {
        return prevCart.map((item) =>
          item.idLibro === libro.idLibro
            ? { ...item, cantidad: item.cantidad + 1 }
            : item
        );
      }
      return [...prevCart, { ...libro, cantidad: 1 }];
    });
  };

  const removeFromCart = (idLibro) => {
    setCart((prevCart) => prevCart.filter((item) => item.idLibro !== idLibro));
  };

  const clearCart = () => {
    setCart([]);
  };

  const totalCartPrice = cart.reduce((total, item) => total + item.precio * item.cantidad, 0);

  return (
    <CartContext.Provider value={{ cart, addToCart, removeFromCart, clearCart, totalCartPrice }}>
      {children}
    </CartContext.Provider>
  );
};

export const useCart = () => useContext(CartContext);