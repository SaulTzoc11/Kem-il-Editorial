import axiosClient from './axiosClient';

export const adminApi = {
  // POST api/auth/usuarios (EXISTE) -> { nombre, correo, password, idRol }
  crearUsuario: (usuario) => axiosClient.post('/auth/usuarios', usuario),

  // GET api/usuarios (PENDIENTE) -> [{ idUsuario, nombre, correo, rol, estado }]
  usuarios: async () => (await axiosClient.get('/usuarios')).data,

  // GET api/roles (PENDIENTE) -> [{ idRol, nombreRol }]
  roles: async () => (await axiosClient.get('/roles')).data,

  // GET api/generos y api/editoriales (PENDIENTES) para los selects del formulario de libros
  generos: async () => (await axiosClient.get('/generos')).data,
  editoriales: async () => (await axiosClient.get('/editoriales')).data,

  // GET api/admin/resumen (PENDIENTE) -> métricas del dashboard
  resumen: async () => (await axiosClient.get('/admin/resumen')).data,
};
