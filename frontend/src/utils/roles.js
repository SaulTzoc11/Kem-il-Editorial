// Los nombres deben coincidir EXACTAMENTE con rol.nombre_rol en la BD y con Roles.cs del backend.
// Visitante no es un rol de BD: es "no tener sesión".
export const ROLES = {
  ADMIN: 'Administrador',
  ASESOR: 'Asesor',
  MINORISTA: 'Cliente',
  MAYORISTA: 'Mayorista',
  INSTITUCIONAL: 'Institucional',
};

// La BD ya usa los mismos nombres que el frontend; se deja por compatibilidad.
export const normalizarRol = (rol) => rol;

export const esAdmin = (rol) => normalizarRol(rol) === ROLES.ADMIN;
export const esAsesor = (rol) => normalizarRol(rol) === ROLES.ASESOR;
export const esPersonal = (rol) => esAdmin(rol) || esAsesor(rol);
export const esMayorista = (rol) => normalizarRol(rol) === ROLES.MAYORISTA;
export const esInstitucional = (rol) => normalizarRol(rol) === ROLES.INSTITUCIONAL;
export const esCliente = (rol) =>
  [ROLES.MINORISTA, ROLES.MAYORISTA, ROLES.INSTITUCIONAL].includes(normalizarRol(rol));

// Valor que espera POST /api/ordenes/calcular
export const tipoClienteDe = (user) => {
  if (esInstitucional(user?.rol)) return 'Institucional';
  if (esMayorista(user?.rol)) return 'Mayorista';
  return 'Minorista';
};

// A dónde llevar al usuario después de iniciar sesión
export const rutaInicial = (rol) => {
  if (esAdmin(rol)) return '/admin';
  if (esAsesor(rol)) return '/asesor';
  return '/';
};