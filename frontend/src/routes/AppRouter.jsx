import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import ProtectedRoute from '../components/ProtectedRoute';
import SiteLayout from '../components/SiteLayout';
import UIProvider from '../context/UIProvider';
import AdminPortal from '../pages/admin/AdminPortal';
import AsesorPortal from '../pages/asesor/AsesorPortal';
import CartPage from '../pages/CartPage';
import CatalogoPage from '../pages/CatalogoPage';
import HomePage from '../pages/HomePage';
import LoginPage from '../pages/LoginPage';
import NoAutorizadoPage from '../pages/NoAutorizadoPage';
import RegistroPage from '../pages/RegistroPage';
import { ROLES } from '../utils/roles';
import RolePortal from '../RolePortal';

export const AppRouter = () => (
  <BrowserRouter>
    <UIProvider>
      <Routes>
        {/* Tienda: cabecera + pie compartidos */}
        <Route element={<SiteLayout />}>
          <Route path="/" element={<HomePage />} />
          <Route path="/catalogo" element={<CatalogoPage />} />
          {/* RF-02: el Visitante no puede comprar -> exige sesión */}
          <Route
            path="/carrito"
            element={
              <ProtectedRoute roles={[ROLES.MINORISTA, ROLES.INSTITUCIONAL]}>
                <CartPage />
              </ProtectedRoute>
            }
          />
        </Route>

        {/* Acceso */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/registro" element={<RegistroPage />} />
        <Route path="/no-autorizado" element={<NoAutorizadoPage />} />

        {/* Paneles internos (RF-05) */}
        <Route
          path="/admin"
          element={
            <ProtectedRoute roles={[ROLES.ADMIN]}>
              <AdminPortal />
            </ProtectedRoute>
          }
        />
        <Route
          path="/asesor"
          element={
            <ProtectedRoute roles={[ROLES.ASESOR, ROLES.ADMIN]}>
              <AsesorPortal />
            </ProtectedRoute>
          }
        />

        <Route path="*" element={<Navigate to="/" replace />} />

        <Route path="/portal" element={<RolePortal />} />
      </Routes>
    </UIProvider>
  </BrowserRouter>
);
