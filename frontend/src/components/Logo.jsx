import { useState } from 'react';

// Logo oficial en frontend/public/kemil-logo.png. Si el archivo no existe, muestra el nombre en texto.
export default function Logo({ alt = "Kem'il — Tejemos historias" }) {
  const [fallo, setFallo] = useState(false);
  if (fallo) return <span className="logo-fallback">Kem’il</span>;
  return <img alt={alt} onError={() => setFallo(true)} src="/kemil-logo.png" />;
}
