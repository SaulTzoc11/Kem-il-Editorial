// DATOS DE EJEMPLO para los paneles mientras el backend no tenga los endpoints (ver api/*.js).
// useApi() los usa solo si el endpoint responde 404/405/501 o no hay conexión, y la pantalla
// muestra el aviso "datos de ejemplo". Cuando el endpoint exista, estos datos dejan de usarse.
// Campos de cotizaciones = columnas de la tabla `cotizaciones`.

export const mockResumen = {
  ventasMes: 48620,
  variacionVentas: 12.5,
  pedidosActivos: 38,
  pedidosAtencion: 8,
  usuarios: 2418,
  usuariosNuevos: 84,
  ventasSemestre: 214580,
  variacionSemestre: 18.2,
  ventasPorMes: [
    { mes: 'May', valor: 46 },
    { mes: 'Jun', valor: 58 },
    { mes: 'Jul', valor: 49 },
    { mes: 'Ago', valor: 72 },
    { mes: 'Sep', valor: 66 },
    { mes: 'Oct', valor: 88 },
  ],
};

export const mockOrdenes = [
  { idFactura: 1048, cliente: 'Ana Martínez', tipo: 'Minorista', total: 267, estado: 'Aprobado', fecha: '2026-11-09T10:42:00' },
  { idFactura: 1047, cliente: 'Universidad Regional', tipo: 'Institucional', total: 8540, estado: 'Pendiente', fecha: '2026-11-09T09:18:00' },
  { idFactura: 1046, cliente: 'Jorge Salinas', tipo: 'Minorista', total: 145, estado: 'Enviado', fecha: '2026-11-08T16:30:00' },
  { idFactura: 1045, cliente: 'Biblioteca Municipal', tipo: 'Institucional', total: 4280, estado: 'Aprobado', fecha: '2026-11-08T14:12:00' },
  { idFactura: 1044, cliente: 'Luisa Pérez', tipo: 'Minorista', total: 98, estado: 'Cancelado', fecha: '2026-11-07T11:05:00' },
];

export const mockUsuarios = [
  { idUsuario: 1, nombre: 'Saúl De León', correo: 'saul@kemileditorial.com', rol: 'Administrador', estado: true },
  { idUsuario: 2, nombre: 'Juan Pérez', correo: 'juan.perez@example.com', rol: 'Cliente Minorista', estado: true },
  { idUsuario: 3, nombre: 'Mateo Flores', correo: 'asesor@kemileditorial.com', rol: 'Asesor', estado: true },
  { idUsuario: 4, nombre: 'Colegio Horizonte', correo: 'compras@horizonte.edu.gt', rol: 'Cliente Institucional', estado: true },
];

export const mockRoles = [
  { idRol: 1, nombreRol: 'Administrador' },
  { idRol: 2, nombreRol: 'Cliente' },
  { idRol: 3, nombreRol: 'Asesor' },
];

export const mockGeneros = [
  { idGenero: 1, nombre: 'Poesía' },
  { idGenero: 2, nombre: 'Historia' },
];

export const mockEditoriales = [{ idEditorial: 1, nombre: "Kem'il Editorial" }];

export const mockCotizaciones = [
  {
    idCotizacion: 248,
    cliente: 'Universidad Regional',
    contacto: 'María Salvatierra',
    tituloObra: 'Colección Voces del Viento',
    cantidadEstimada: 120,
    acabadoPortada: 'Tapa dura, laminado mate',
    tipoPapel: 'Bond ahuesado 90 g',
    fechaRequeridaEntrega: '2026-12-02',
    fechaSolicitud: '2026-11-05',
    comentariosCliente: 'Requerimos los ejemplares para la apertura de nuestra nueva sala de lectura. Agradecemos incluir tiempos de entrega.',
    estado: 'Pendiente',
    montoCotizado: null,
    observacionesAsesor: '',
  },
  {
    idCotizacion: 247,
    cliente: 'Biblioteca Municipal',
    contacto: 'Pablo Arce',
    tituloObra: 'Antología poética regional',
    cantidadEstimada: 80,
    acabadoPortada: 'Tapa blanda',
    tipoPapel: 'Bond 75 g',
    fechaRequeridaEntrega: '2026-12-15',
    fechaSolicitud: '2026-11-03',
    comentariosCliente: '',
    estado: 'En revisión',
    montoCotizado: null,
    observacionesAsesor: '',
  },
  {
    idCotizacion: 245,
    cliente: 'Fundación Semillas',
    contacto: 'Diego Mercado',
    tituloObra: 'Edición personalizada',
    cantidadEstimada: 200,
    acabadoPortada: 'Edición especial',
    tipoPapel: 'Couché 115 g',
    fechaRequeridaEntrega: '2027-01-20',
    fechaSolicitud: '2026-10-28',
    comentariosCliente: 'Incluir logotipo institucional en la portada.',
    estado: 'Respondida',
    montoCotizado: 18400,
    observacionesAsesor: 'Cotización enviada. Vigencia 15 días.',
  },
];
