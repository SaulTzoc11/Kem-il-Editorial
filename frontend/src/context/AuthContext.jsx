import { createContext, useContext, useState } from 'react';
import axiosClient from '../api/axiosClient';

const AuthContext = createContext();

const leerUsuarioGuardado = () => {
  try {
    const token = localStorage.getItem('kemil_token');
    const expira = localStorage.getItem('kemil_expira');
    if (!token || !expira || new Date(expira) <= new Date()) return null;
    return JSON.parse(localStorage.getItem('kemil_user'));
  } catch {
    return null;
  }
};

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(leerUsuarioGuardado);

  const guardarSesion = (data) => {
    localStorage.setItem('kemil_token', data.token);
    localStorage.setItem('kemil_expira', data.expira);
    localStorage.setItem('kemil_user', JSON.stringify(data.usuario));
    setUser(data.usuario);
  };

  const login = async (correo, password) => {
    const { data } = await axiosClient.post('/auth/login', { correo, password });
    guardarSesion(data);
    return data.usuario;
  };

  const registro = async (nombre, correo, password) => {
    const { data } = await axiosClient.post('/auth/registro', { nombre, correo, password });
    guardarSesion(data);
    return data.usuario;
  };

  const logout = () => {
    localStorage.removeItem('kemil_token');
    localStorage.removeItem('kemil_expira');
    localStorage.removeItem('kemil_user');
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, isAuthenticated: !!user, login, registro, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

// eslint-disable-next-line react-refresh/only-export-components
export const useAuth = () => useContext(AuthContext);