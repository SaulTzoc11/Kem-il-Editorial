import axiosClient from './axiosClient';

// Los nombres de campos siguen las columnas de la tabla `cotizaciones` (database/schema.sql).
// Todo este módulo es de Juan Luis (pedidos especiales): estos endpoints están PENDIENTES en el backend.
export const cotizacionesApi = {
  // POST api/cotizaciones  (rol Cliente Institucional)
  // { tituloObra, cantidadEstimada, acabadoPortada, tipoPapel, fechaRequeridaEntrega, comentariosCliente }
  crear: (solicitud) => axiosClient.post('/cotizaciones', solicitud),

  // GET api/cotizaciones/bandeja  (Asesor / Administrador) -> solicitudes asignadas al asesor
  bandeja: async () => (await axiosClient.get('/cotizaciones/bandeja')).data,

  // PUT api/cotizaciones/{id}/responder  (Asesor)
  // { estado: 'Aprobada' | 'Rechazada' | 'Cotizada', montoCotizado, observacionesAsesor }
  responder: (id, respuesta) => axiosClient.put(`/cotizaciones/${id}/responder`, respuesta),
};
