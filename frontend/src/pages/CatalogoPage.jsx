import { useEffect, useState } from 'react';
import { useCart } from '../context/CartContext';
import axiosClient from '../api/axiosClient';

export default function CatalogoPage() {
  const [libros, setLibros] = useState([]);
  const [loading, setLoading] = useState(true);
  const { addToCart } = useCart();

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
            <span>Género: {libro.nombreGenero}</span>
            <span>Formato: {libro.formato}</span>
            <strong>Precio: Q{libro.precio}</strong>
            <button 
              onClick={() => addToCart(libro)}
              style={{ backgroundColor: '#2b6cb0', color: 'white', padding: '8px 12px', border: 'none', borderRadius: '4px', cursor: 'pointer', marginTop: '10px' }}
            >
              Añadir al carrito 
            </button>
          </div>
        ))}
      </div>
    </div>
  );
}