import { BrowserRouter, Routes, Route } from 'react-router-dom';
import HomePage from '../pages/HomePage';
import CatalogoPage from '../pages/CatalogoPage';
import CartPage from '../pages/CartPage';

export const AppRouter = () => {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/catalogo" element={<CatalogoPage />} />
        <Route path="/carrito" element = {<CartPage /> }/>
      </Routes>
    </BrowserRouter>
  );
};