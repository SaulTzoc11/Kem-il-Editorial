import axios from 'axios';

const axiosClient = axios.create({
  baseURL: 'http://localhost:5229/api',
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

export default axiosClient;