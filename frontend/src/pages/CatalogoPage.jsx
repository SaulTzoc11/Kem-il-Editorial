import { useEffect, useState } from 'react';
import axiosClient from '../api/axiosClient';

export default function CatalogoPage() {
  const [libros, setLibros] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    axiosClient.get('/libros')
      .then((response) => {
        setLibros(response.data);
        setLoading(false);
      })
      .catch((error) => {
        console.error("Error al obtener los libros:", error);
        setLoading(false);
      });
  }, []);

  if (loading) return <p>Cargando catálogo...</p>;

  return (
    <div style={{ padding: '20px' }}>
      <h1>Catálogo de Libros - Kem'il Editorial</h1>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(250px, 1fr))', gap: '20px' }}>
        {libros.map((libro) => (
          <div key={libro.idLibro} style={{ border: '1px solid #ccc', padding: '15px', borderRadius: '8px' }}>
            <h3>{libro.nombre}</h3>
            <p>{libro.descripcion}</p>
            <p><strong>Género:</strong> {libro.nombreGenero}</p>
            <p><strong>Formato:</strong> {libro.formato}</p>
            <p><strong>Precio:</strong> Q{libro.precio.toFixed(2)}</p>
          </div>
        ))}
      </div>
    </div>
  );
}