import { useCallback, useEffect, useRef, useState } from 'react';

// Carga datos de la API.
//   const { data, loading, error, usingMock, reload } = useApi(() => api.algo(), { fallback: [...] })
// Si se pasa `fallback`, se usa cuando el endpoint todavía no existe en el backend (404/405/501)
// o no hay conexión; `usingMock` vale true para que la pantalla avise que son datos de ejemplo.
// Los errores 401/403 NUNCA se ocultan con datos de ejemplo.
const sinEndpoint = (error) => !error.response || [404, 405, 501].includes(error.response.status);

export default function useApi(fetcher, { fallback } = {}) {
  const [estado, setEstado] = useState({ data: null, loading: true, error: null, usingMock: false });
  const [version, setVersion] = useState(0);
  const fetcherRef = useRef(fetcher);
  const fallbackRef = useRef(fallback);

  useEffect(() => {
    fetcherRef.current = fetcher;
    fallbackRef.current = fallback;
  });

  useEffect(() => {
    let vigente = true;
    fetcherRef
      .current()
      .then((data) => vigente && setEstado({ data, loading: false, error: null, usingMock: false }))
      .catch((error) => {
        if (!vigente) return;
        if (fallbackRef.current !== undefined && sinEndpoint(error)) {
          setEstado({ data: fallbackRef.current, loading: false, error: null, usingMock: true });
        } else {
          setEstado({ data: null, loading: false, error, usingMock: false });
        }
      });
    return () => {
      vigente = false;
    };
  }, [version]);

  const reload = useCallback(() => {
    setEstado((actual) => ({ ...actual, loading: true }));
    setVersion((v) => v + 1);
  }, []);

  return { ...estado, reload };
}
