import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { normalizarRol } from '../utils/roles';

// <ProtectedRoute>...</ProtectedRoute>                          -> cualquier usuario logueado
// <ProtectedRoute roles={[ROLES.ADMIN]}>...</ProtectedRoute>    -> solo esos roles
// Esto solo controla la pantalla. La seguridad real está en [Authorize] del backend.
export default function ProtectedRoute({ roles, children }) {
  const { user, isAuthenticated } = useAuth();
  const location = useLocation();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  }
  if (roles && !roles.includes(normalizarRol(user.rol))) {
    return <Navigate to="/no-autorizado" replace />;
  }
  return children;
}
