import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import BookModal from '../components/BookModal';
import CartDrawer from '../components/CartDrawer';
import Icon from '../components/Icon';
import QuoteModal from '../components/QuoteModal';
import { UIContext } from './UIContext';

// Dueño del estado de interfaz global. Debe ir DENTRO del BrowserRouter (los overlays usan navigate).
export default function UIProvider({ children }) {
  const [cartOpen, setCartOpen] = useState(false);
  const [quoteOpen, setQuoteOpen] = useState(false);
  const [selectedBook, setSelectedBook] = useState(null);
  const [aviso, setAviso] = useState(null);
  const temporizador = useRef(null);

  useEffect(() => () => clearTimeout(temporizador.current), []);

  const notify = useCallback((mensaje, tipo = 'ok') => {
    clearTimeout(temporizador.current);
    setAviso({ mensaje, tipo });
    temporizador.current = setTimeout(() => setAviso(null), 2800);
  }, []);

  const valor = useMemo(
    () => ({
      cartOpen,
      openCart: () => setCartOpen(true),
      closeCart: () => setCartOpen(false),
      quoteOpen,
      openQuote: () => setQuoteOpen(true),
      closeQuote: () => setQuoteOpen(false),
      selectedBook,
      openBook: setSelectedBook,
      closeBook: () => setSelectedBook(null),
      notify,
    }),
    [cartOpen, quoteOpen, selectedBook, notify]
  );

  return (
    <UIContext.Provider value={valor}>
      {children}
      <CartDrawer />
      <BookModal />
      <QuoteModal />
      {aviso && (
        <div className={`toast ${aviso.tipo === 'error' ? 'is-error' : ''}`} role="status">
          <Icon name={aviso.tipo === 'error' ? 'close' : 'check'} /> {aviso.mensaje}
        </div>
      )}
    </UIContext.Provider>
  );
}
