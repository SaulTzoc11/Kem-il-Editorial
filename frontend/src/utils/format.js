const moneda = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' });

export const formatoQ = (valor) => moneda.format(Number(valor) || 0);

export const formatoFecha = (valor) => {
  if (!valor) return '—';
  const fecha = new Date(valor);
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
