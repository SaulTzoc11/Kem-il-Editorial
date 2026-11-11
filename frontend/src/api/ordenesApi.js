import axiosClient from './axiosClient';

export const ordenesApi = {
  // POST api/ordenes/calcular -> el motor de descuentos vive en el backend (DescuentoService)
  calcular: async (items, tipoCliente) => {
    const { data } = await axiosClient.post('/ordenes/calcular', {
      items: items.map(({ idLibro, nombre, precio, cantidad }) => ({ idLibro, nombre, precio, cantidad })),
      tipoCliente,
    });
    return data;
  },
  // POST api/ordenes -> cierre de orden (RF-03.3). Solo se envía idLibro y cantidad: el servidor toma el
  // precio de la BD, calcula el descuento según el rol del token y devuelve { idFactura, estado, ...resumen }.
  crear: async (items) => {
    const { data } = await axiosClient.post('/ordenes', {
      items: items.map(({ idLibro, cantidad }) => ({ idLibro, cantidad })),
    });
    return data;
  },
  // GET api/ordenes?estado=Pendiente -> monitoreo del admin (RF-05.2). PENDIENTE.
  listar: async (estado) => {
    const { data } = await axiosClient.get('/ordenes', { params: estado ? { estado } : undefined });
    return data;
  },
  // GET api/ordenes/mis-pedidos -> historial del cliente autenticado.
  misPedidos: async () => (await axiosClient.get('/ordenes/mis-pedidos')).data,
};
