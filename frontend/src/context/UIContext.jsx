import { createContext, useContext } from 'react';

// Estado de interfaz compartido: carrito lateral, ficha del libro, modal de cotización y avisos.
export const UIContext = createContext(null);

export const useUI = () => useContext(UIContext);
