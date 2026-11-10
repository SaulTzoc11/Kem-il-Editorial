// Los nombres deben coincidir EXACTAMENTE con rol.nombre_rol en la BD y con Roles.cs del backend.
// Visitante no es un rol de BD: es "no tener sesión".
export const ROLES = {
  ADMIN: 'Administrador',
  ASESOR: 'Asesor',
  MINORISTA: 'Cliente Minorista',
  INSTITUCIONAL: 'Cliente Institucional',
};

// Compatibilidad: el seed actual solo tiene el rol "Cliente". Lo tratamos como Minorista
// hasta que la BD tenga los roles separados.
export const normalizarRol = (rol) => (rol === 'Cliente' ? ROLES.MINORISTA : rol);

export const esAdmin = (rol) => normalizarRol(rol) === ROLES.ADMIN;
export const esAsesor = (rol) => normalizarRol(rol) === ROLES.ASESOR;
export const esPersonal = (rol) => esAdmin(rol) || esAsesor(rol);
export const esInstitucional = (rol) => normalizarRol(rol) === ROLES.INSTITUCIONAL;
export const esCliente = (rol) =>
  [ROLES.MINORISTA, ROLES.INSTITUCIONAL].includes(normalizarRol(rol));

// Valor que espera POST /api/ordenes/calcular
export const tipoClienteDe = (user) => (esInstitucional(user?.rol) ? 'Institucional' : 'Minorista');

// A dónde llevar al usuario después de iniciar sesión
export const rutaInicial = (rol) => {
  if (esAdmin(rol)) return '/admin';
  if (esAsesor(rol)) return '/asesor';
  return '/';
};
