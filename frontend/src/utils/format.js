const moneda = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' });

export const formatoQ = (valor) => moneda.format(Number(valor) || 0);

export const formatoFecha = (valor) => {
  if (!valor) return '—';
  // "2026-11-11" (solo fecha) se interpreta como UTC y se muestra un día antes; se fuerza a hora local
  const soloFecha = /^\d{4}-\d{2}-\d{2}$/.test(valor);
  const fecha = new Date(soloFecha ? `${valor}T00:00:00` : valor);
  if (Number.isNaN(fecha.getTime())) return String(valor);
  return fecha.toLocaleDateString('es-GT', { day: 'numeric', month: 'short', year: 'numeric' });
};

export const iniciales = (texto = '') =>
  texto
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((palabra) => palabra[0].toUpperCase())
    .join('');

// Quita acentos y pasa a minúsculas: "Poesía" coincide con "poesia"
export const normalizarTexto = (texto = '') =>
  String(texto).normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase();
