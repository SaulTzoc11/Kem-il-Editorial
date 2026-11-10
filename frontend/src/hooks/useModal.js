import { useEffect } from 'react';

// Cierra con la tecla Escape y bloquea el scroll del fondo mientras el modal está abierto.
export default function useModal(abierto, onClose) {
  useEffect(() => {
    if (!abierto) return undefined;
    const alTeclear = (e) => e.key === 'Escape' && onClose();
    document.addEventListener('keydown', alTeclear);
    document.body.classList.add('no-scroll');
    return () => {
      document.removeEventListener('keydown', alTeclear);
      document.body.classList.remove('no-scroll');
    };
  }, [abierto, onClose]);
}
