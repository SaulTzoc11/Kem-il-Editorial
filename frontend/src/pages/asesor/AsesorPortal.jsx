import { useMemo, useState } from 'react';
import { mensajeDeError } from '../../api/axiosClient';
import { cotizacionesApi } from '../../api/cotizacionesApi';
import Icon from '../../components/Icon';
import PortalShell from '../../components/portal/PortalShell';
import { MetricCard, MockNotice, StatusBadge } from '../../components/portal/Widgets';
import { useUI } from '../../context/UIContext';
import { mockCotizaciones } from '../../data/mockPortal';
import useApi from '../../hooks/useApi';
import { formatoFecha, formatoQ, iniciales } from '../../utils/format';

const PENDIENTES = ['Pendiente', 'En revisión'];

// Panel del Asesor de Ventas (RF-04.3 / RF-05.3). Ruta protegida: Asesor y Administrador.
export default function AsesorPortal() {
  const { data, usingMock, reload } = useApi(cotizacionesApi.bandeja, { fallback: mockCotizaciones });
  const [seccion, setSeccion] = useState('bandeja');
  const cotizaciones = useMemo(() => data ?? [], [data]);
  const pendientes = cotizaciones.filter((c) => PENDIENTES.includes(c.estado)).length;

  const nav = [
    { id: 'bandeja', label: 'Mi bandeja', icon: 'grid', count: pendientes },
    { id: 'seguimiento', label: 'Seguimiento', icon: 'chart' },
  ];

  return (
    <PortalShell
      active={seccion}
      area="Espacio comercial"
      nav={nav}
      onChange={setSeccion}
      welcome={{
        texto: pendientes
          ? `Tienes ${pendientes} solicitud${pendientes === 1 ? '' : 'es'} esperando tu revisión.`
          : 'No tienes solicitudes pendientes.',
      }}
    >
      {usingMock && <MockNotice endpoint="GET /api/cotizaciones/bandeja" />}
      {seccion === 'bandeja' ? (
        <Bandeja cotizaciones={cotizaciones} onChanged={reload} mock={usingMock} />
      ) : (
        <Seguimiento cotizaciones={cotizaciones} />
      )}
    </PortalShell>
  );
}

function Bandeja({ cotizaciones, onChanged, mock }) {
  const [filtro, setFiltro] = useState('Todas');
  const [seleccionId, setSeleccionId] = useState(null);

  const visibles = cotizaciones.filter((c) =>
    filtro === 'Todas' ? true : filtro === 'Pendientes' ? PENDIENTES.includes(c.estado) : !PENDIENTES.includes(c.estado)
  );
  const seleccionada = cotizaciones.find((c) => c.idCotizacion === seleccionId) ?? visibles[0];

  return (
    <div className="advisor-workspace">
      <section className="quote-inbox">
        <div className="inbox-heading">
          <div>
            <span>Solicitudes asignadas</span>
            <h2>Mi bandeja</h2>
          </div>
        </div>
        <div className="inbox-tabs">
          {['Todas', 'Pendientes', 'Respondidas'].map((f) => (
            <button className={filtro === f ? 'active' : ''} key={f} onClick={() => setFiltro(f)}>
              {f}
            </button>
          ))}
        </div>
        <div className="quote-list">
          {visibles.map((c) => (
            <button
              className={c.idCotizacion === seleccionada?.idCotizacion ? 'active' : ''}
              key={c.idCotizacion}
              onClick={() => setSeleccionId(c.idCotizacion)}
            >
              <span className="client-avatar">{iniciales(c.cliente)}</span>
              <span className="quote-list-copy">
                <strong>{c.cliente}</strong>
                <small>{c.tituloObra}</small>
                <em>COT-{c.idCotizacion} · {c.cantidadEstimada} unidades</em>
              </span>
              <span className="quote-list-meta">
                <small>{formatoFecha(c.fechaSolicitud)}</small>
                <StatusBadge>{c.estado}</StatusBadge>
              </span>
            </button>
          ))}
          {visibles.length === 0 && <p className="portal-empty">No hay solicitudes en esta vista.</p>}
        </div>
      </section>
      {seleccionada ? (
        <DetalleCotizacion cotizacion={seleccionada} key={seleccionada.idCotizacion} mock={mock} onChanged={onChanged} />
      ) : (
        <p className="portal-empty">Selecciona una solicitud.</p>
      )}
    </div>
  );
}

function DetalleCotizacion({ cotizacion: c, onChanged, mock }) {
  const { notify } = useUI();
  const [monto, setMonto] = useState(c.montoCotizado ?? '');
  const [observaciones, setObservaciones] = useState(c.observacionesAsesor ?? '');
  const [enviando, setEnviando] = useState(false);
  const [estadoLocal, setEstadoLocal] = useState(null);
  const estado = estadoLocal ?? c.estado;
  const cerrada = ['Respondida', 'Rechazada'].includes(estado);

  const responder = async (nuevoEstado) => {
    if (nuevoEstado === 'Respondida' && !(Number(monto) > 0)) {
      notify('Indica el monto cotizado antes de enviar la cotización', 'error');
      return;
    }
    setEnviando(true);
    try {
      if (!mock) {
        await cotizacionesApi.responder(c.idCotizacion, {
          estado: nuevoEstado,
          montoCotizado: monto === '' ? null : Number(monto),
          observacionesAsesor: observaciones,
        });
        onChanged();
      }
      setEstadoLocal(nuevoEstado);
      notify(
        nuevoEstado === 'Respondida'
          ? 'La cotización fue enviada al cliente'
          : nuevoEstado === 'Aprobada'
            ? 'La solicitud fue marcada como aprobada'
            : 'La solicitud fue rechazada'
      );
    } catch (err) {
      notify(mensajeDeError(err, 'No se pudo guardar la respuesta.'), 'error');
    } finally {
      setEnviando(false);
    }
  };

  return (
    <section className="quote-detail">
      <div className="quote-detail-header">
        <div>
          <span>COT-{c.idCotizacion}</span>
          <h2>{c.cliente}</h2>
          {c.contacto && <p>Solicitado por {c.contacto}</p>}
        </div>
        <StatusBadge>{estado}</StatusBadge>
      </div>
      <div className="quote-summary">
        <div>
          <span>Entrega solicitada</span>
          <strong>{formatoFecha(c.fechaRequeridaEntrega)}</strong>
        </div>
        <div>
          <span>Cantidad</span>
          <strong>{c.cantidadEstimada} unidades</strong>
        </div>
        <div>
          <span>Monto cotizado</span>
          <strong>{monto ? formatoQ(monto) : 'Sin cotizar'}</strong>
        </div>
      </div>
      <div className="requirement-card">
        <span>Detalle del requerimiento</span>
        <h3>{c.tituloObra}</h3>
        <dl>
          <div>
            <dt>Acabado de portada</dt>
            <dd>{c.acabadoPortada || '—'}</dd>
          </div>
          <div>
            <dt>Tipo de papel</dt>
            <dd>{c.tipoPapel || '—'}</dd>
          </div>
          <div>
            <dt>Fecha de solicitud</dt>
            <dd>{formatoFecha(c.fechaSolicitud)}</dd>
          </div>
          <div>
            <dt>Cantidad estimada</dt>
            <dd>{c.cantidadEstimada}</dd>
          </div>
        </dl>
        {c.comentariosCliente && <blockquote>“{c.comentariosCliente}”</blockquote>}
      </div>
      <div className="reply-form">
        <h3>Propuesta comercial</h3>
        <label>
          <span>Monto cotizado (Q)</span>
          <input disabled={cerrada} min="0" onChange={(e) => setMonto(e.target.value)} step="0.01" type="number" value={monto} />
        </label>
        <label>
          <span>Observaciones para el cliente</span>
          <textarea disabled={cerrada} onChange={(e) => setObservaciones(e.target.value)} rows={3} value={observaciones} />
        </label>
      </div>
      {!cerrada && (
        <div className="quote-actions">
          <button className="portal-secondary is-danger" disabled={enviando} onClick={() => responder('Rechazada')}>
            <Icon name="close" /> Rechazar
          </button>
          <button className="portal-secondary" disabled={enviando} onClick={() => responder('Aprobada')}>
            <Icon name="check" /> Aprobar
          </button>
          <button className="portal-primary" disabled={enviando} onClick={() => responder('Respondida')}>
            <Icon name="mail" /> Enviar cotización
          </button>
        </div>
      )}
    </section>
  );
}

function Seguimiento({ cotizaciones }) {
  const total = cotizaciones.reduce((s, c) => s + (Number(c.montoCotizado) || 0), 0);
  const respondidas = cotizaciones.filter((c) => c.estado === 'Respondida').length;
  const aprobadas = cotizaciones.filter((c) => c.estado === 'Aprobada').length;
  const pendientes = cotizaciones.filter((c) => PENDIENTES.includes(c.estado)).length;

  return (
    <div className="advisor-metrics">
      <MetricCard label="Pendientes" note="Por revisar" tone="metric-terracotta" value={pendientes} />
      <MetricCard label="Respondidas" note={`${aprobadas} aprobadas`} tone="metric-white" value={respondidas} />
      <MetricCard label="Monto cotizado" note="Suma de cotizaciones emitidas" tone="metric-gold" value={formatoQ(total)} />
    </div>
  );
}
