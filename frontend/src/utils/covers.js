const ACENTOS = ['terracotta', 'navy', 'gold', 'sand'];
export const acentoDe = (libro) => ACENTOS[libro.idLibro % ACENTOS.length];
