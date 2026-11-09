import { BrowserRouter, Routes, Route } from 'react-router-dom';
import HomePage from '../pages/HomePage';
import CatalogoPage from '../pages/CatalogoPage';
import CartPage from '../pages/CartPage';
import LoginPage from '../pages/LoginPage';
import RegistroPage from '../pages/RegistroPage';
import NoAutorizadoPage from '../pages/NoAutorizadoPage';

export const AppRouter = () => {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/catalogo" element={<CatalogoPage />} />
        <Route path="/carrito" element={<CartPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/registro" element={<RegistroPage />} />
        <Route path="/no-autorizado" element={<NoAutorizadoPage />} />
      </Routes>
    </BrowserRouter>
  );
};