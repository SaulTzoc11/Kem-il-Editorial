import axiosClient from './axiosClient';

// Las portadas subidas desde el panel las sirve el backend (/uploads/portadas/...), así que se completa con su origen.
// Las portadas originales (/portadas/...) las sirve el propio frontend y se dejan tal cual.
const ORIGEN_API = (axiosClient.defaults.baseURL ?? '').replace(/\/api\/?$/, '');
export const urlPortada = (portada) =>
  portada && portada.startsWith('/uploads/') ? `${ORIGEN_API}${portada}` : portada || null;

// Convierte el LibroDto del backend al formato que usa la interfaz.
// Los campos marcados (*) todavía NO los devuelve GET /api/libros: ver GUIA_INTEGRACION_FRONTEND.md.
// Mientras no existan, la interfaz muestra valores por defecto y no se rompe.
export function normalizarLibro(dto) {
  return {
    idLibro: dto.idLibro,
    nombre: dto.nombre,
    descripcion: dto.descripcion ?? '',
    formato: dto.formato ?? '',
    precio: Number(dto.precio ?? 0),
    isbn: dto.isbn ?? dto.ISBN ?? '',
    genero: dto.nombreGenero ?? 'Sin categoría',
    idGenero: dto.idGenero ?? null, // (*)
    idEditorial: dto.idEditorial ?? null, // (*)
    autor: dto.autor ?? dto.nombreAutor ?? '', // (*)
    editorial: dto.editorial ?? dto.nombreEditorial ?? '', // (*)
    portada: urlPortada(dto.portada),
    edicion: dto.edicion ?? '', // (*)
    anio: dto.anioEdicion ?? (dto.fechaLanzamiento ? new Date(dto.fechaLanzamiento).getFullYear() : null), // (*)
    stock: dto.stock ?? dto.stockDisponible ?? null, // (*) null = el backend no lo informa
    stockMinimo: dto.stockMinimo ?? 5, // (*)
    valoracion: dto.valoracion ?? null, // (*)
  };
}

export const librosApi = {
  // GET api/libros (público)
  listar: async () => {
    const { data } = await axiosClient.get('/libros');
    return data.map(normalizarLibro);
  },
  // POST api/libros (debe ser solo Administrador en el backend)
  crear: (libro) => axiosClient.post('/libros', libro),
  // PUT api/libros/{id}
  actualizar: (id, libro) => axiosClient.put(`/libros/${id}`, libro),
  // DELETE api/libros/{id} -> eliminación lógica (estado = 0)
  eliminar: (id) => axiosClient.delete(`/libros/${id}`),
  // POST api/libros/{id}/portada (multipart, campo "archivo") -> sube la imagen de portada (JPG, PNG o WebP, máx. 2 MB)
  subirPortada: (id, archivo) => {
    const datos = new FormData();
    datos.append('archivo', archivo);
    return axiosClient.post(`/libros/${id}/portada`, datos, { headers: { 'Content-Type': 'multipart/form-data' } });
  },
};

// Estado visible en el panel según el stock
export const estadoDeStock = (libro) => {
  if (libro.stock === null) return 'Publicado';
  if (libro.stock <= 0) return 'Agotado';
  if (libro.stock <= libro.stockMinimo) return 'Stock bajo';
  return 'Publicado';
};
