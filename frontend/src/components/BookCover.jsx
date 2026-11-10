import { useState } from 'react';
import { acentoDe } from '../utils/covers';

// Portada real si el libro tiene `portada`; si no (o si la imagen falla), una portada tipográfica generada.
export default function BookCover({ libro, className = '' }) {
  const [fallo, setFallo] = useState(false);
  if (libro.portada && !fallo) {
    return <img alt={`Portada de ${libro.nombre}`} onError={() => setFallo(true)} src={libro.portada} />;
  }
  return (
    <span aria-label={`Portada de ${libro.nombre}`} className={`cover-art ${acentoDe(libro)} ${className}`} role="img">
      {libro.nombre}
    </span>
  );
}
