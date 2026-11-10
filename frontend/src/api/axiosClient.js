import axios from 'axios';

// En desarrollo apunta al backend local. Para otro entorno: crear frontend/.env con VITE_API_URL=...
const axiosClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5229/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Adjunta el token a cada petición
axiosClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('kemil_token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// Si el token venció (401), limpiar la sesión y volver al login
axiosClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const huboToken = !!localStorage.getItem('kemil_token');
    const esLogin = error.config?.url?.includes('/auth/login');
    if (error.response?.status === 401 && huboToken && !esLogin) {
      localStorage.removeItem('kemil_token');
      localStorage.removeItem('kemil_expira');
      localStorage.removeItem('kemil_user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// Mensaje legible para mostrar al usuario a partir de un error de axios
export const mensajeDeError = (error, porDefecto = 'Ocurrió un error. Inténtalo de nuevo.') => {
  if (!error.response) return 'No hay conexión con el servidor.';
  if (error.response.status === 403) return 'No tienes permisos para realizar esta acción.';
  if (error.response.status === 429) return 'Demasiados intentos. Espera un minuto e inténtalo de nuevo.';
  const data = error.response.data;
  const validaciones = data?.errors && Object.values(data.errors).flat()[0];
  return data?.mensaje || validaciones || (typeof data === 'string' && data) || porDefecto;
};

export default axiosClient;
