import { useState } from 'react';
import { adminApi } from '../../api/adminApi';
import { estadoDeStock, librosApi } from '../../api/librosApi';
import { ordenesApi } from '../../api/ordenesApi';
import Icon from '../../components/Icon';
import PortalShell from '../../components/portal/PortalShell';
import { MetricCard, MockNotice, StatusBadge } from '../../components/portal/Widgets';
import { mockOrdenes, mockResumen } from '../../data/mockPortal';
import useApi from '../../hooks/useApi';
import { formatoFecha, formatoQ } from '../../utils/format';
import CatalogoAdmin from './CatalogoAdmin';
import UsuariosAdmin from './UsuariosAdmin';

const NAV = [
  { id: 'resumen', label: 'Resumen', icon: 'grid' },
  { id: 'catalogo', label: 'Catálogo', icon: 'orders' },
  { id: 'pedidos', label: 'Pedidos', icon: 'chart' },
  { id: 'usuarios', label: 'Usuarios y roles', icon: 'users' },
  { id: 'configuracion', label: 'Configuración', icon: 'gear' },
];

const ESTADOS = ['Todos', 'Pendiente', 'Aprobado', 'Enviado', 'Cancelado'];

// Panel del Administrador (RF-05). Ruta protegida: solo rol Administrador.
export default function AdminPortal() {
  const [seccion, setSeccion] = useState('resumen');

  return (
    <PortalShell
      active={seccion}
      area="Panel de control"
      nav={NAV}
      onChange={setSeccion}
      welcome={{
        texto: 'Aquí tienes el estado general de la editorial.',
        accion: (
          <button className="portal-primary" onClick={() => setSeccion('catalogo')}>
            <Icon name="plus" /> Gestionar catálogo
          </button>
        ),
      }}
    >
      {seccion === 'resumen' && <Resumen />}
      {seccion === 'catalogo' && <CatalogoAdmin />}
      {seccion === 'pedidos' && <Pedidos />}
      {seccion === 'usuarios' && <UsuariosAdmin />}
      {seccion === 'configuracion' && <Configuracion />}
    </PortalShell>
  );
}

function Resumen() {
  const { data: resumen, usingMock } = useApi(adminApi.resumen, { fallback: mockResumen });
  const { data: libros } = useApi(librosApi.listar);
  const alertas = (libros ?? []).filter((l) => estadoDeStock(l) !== 'Publicado').slice(0, 3);
  const stockBajo = (libros ?? []).filter((l) => estadoDeStock(l) !== 'Publicado').length;

  if (!resumen) return <p className="portal-empty">Cargando...</p>;

  return (
    <>
      {usingMock && <MockNotice endpoint="GET /api/admin/resumen" />}
      <div className="portal-metrics">
        <MetricCard label="Ventas del mes" note={`↑ ${resumen.variacionVentas}% vs. mes anterior`} tone="metric-navy" value={formatoQ(resumen.ventasMes)} />
        <MetricCard label="Pedidos activos" note={`${resumen.pedidosAtencion} requieren atención`} tone="metric-cream" value={resumen.pedidosActivos} />
        <MetricCard
          label="Títulos publicados"
          note={`${stockBajo} con stock bajo o agotados`}
          tone="metric-white"
          value={libros ? libros.length : '—'}
        />
        <MetricCard label="Usuarios registrados" note={`+${resumen.usuariosNuevos} este mes`} tone="metric-gold" value={resumen.usuarios} />
      </div>
      <div className="portal-dashboard-grid">
        <section className="portal-panel sales-panel">
          <div className="panel-heading">
            <div>
              <span>Rendimiento</span>
              <h2>Ventas de los últimos 6 meses</h2>
            </div>
          </div>
          <div className="sales-total">
            <strong>{formatoQ(resumen.ventasSemestre)}</strong>
            <span>+{resumen.variacionSemestre}%</span>
          </div>
          <div aria-label="Gráfico de ventas mensuales" className="bar-chart" role="img">
            {resumen.ventasPorMes.map((m) => (
              <div key={m.mes}>
                <span style={{ height: `${m.valor}%` }} />
                <small>{m.mes}</small>
              </div>
            ))}
          </div>
        </section>
        <section className="portal-panel stock-panel">
          <div className="panel-heading">
            <div>
              <span>Inventario</span>
              <h2>Alertas de stock</h2>
            </div>
          </div>
          {alertas.length === 0 && <p className="portal-empty">Sin alertas de stock.</p>}
          {alertas.map((libro, i) => (
            <div className="stock-row" key={libro.idLibro}>
              <span className={`stock-cover cover-${i + 1}`} />
              <p>
                <strong>{libro.nombre}</strong>
                <small>{libro.stock === 0 ? 'Agotado' : `${libro.stock} unidades`}</small>
              </p>
              <Icon name="chevron" size={14} />
            </div>
          ))}
        </section>
      </div>
    </>
  );
}

// RF-05.2 Monitoreo de órdenes filtradas por estado
function Pedidos() {
  const { data: ordenes, usingMock } = useApi(() => ordenesApi.listar(), { fallback: mockOrdenes });
  const [estado, setEstado] = useState('Todos');
  const lista = (ordenes ?? []).filter((o) => estado === 'Todos' || o.estado === estado);

  return (
    <>
      {usingMock && <MockNotice endpoint="GET /api/ordenes" />}
      <section className="portal-panel table-panel standalone-panel">
        <div className="panel-heading">
          <div>
            <span>Monitoreo de órdenes</span>
            <h2>Pedidos</h2>
          </div>
        </div>
        <div className="portal-chips" role="group" aria-label="Filtrar por estado">
          {ESTADOS.map((e) => (
            <button className={estado === e ? 'active' : ''} key={e} onClick={() => setEstado(e)}>
              {e}
            </button>
          ))}
        </div>
        <div className="portal-table-wrap">
          <table className="portal-table">
            <thead>
              <tr>
                <th>Pedido</th>
                <th>Cliente</th>
                <th>Tipo</th>
                <th>Total</th>
                <th>Estado</th>
                <th>Fecha</th>
              </tr>
            </thead>
            <tbody>
              {lista.map((o) => (
                <tr key={o.idFactura}>
                  <td>
                    <strong>#PED-{o.idFactura}</strong>
                  </td>
                  <td>{o.cliente}</td>
                  <td>{o.tipo}</td>
                  <td>
                    <strong>{formatoQ(o.total)}</strong>
                  </td>
                  <td>
                    <StatusBadge>{o.estado}</StatusBadge>
                  </td>
                  <td>{formatoFecha(o.fecha)}</td>
                </tr>
              ))}
            </tbody>
          </table>
          {lista.length === 0 && <p className="portal-empty">No hay pedidos en este estado.</p>}
        </div>
      </section>
    </>
  );
}

// Parámetros vigentes. Son de solo lectura porque las reglas viven en el Modelo (DescuentoService).
function Configuracion() {
  return (
    <section className="portal-panel settings-panel">
      <div className="panel-heading">
        <div>
          <span>Parámetros del sistema</span>
          <h2>Reglas de negocio vigentes</h2>
        </div>
      </div>
      <p className="field-hint" style={{ marginTop: 8 }}>
        Definidas en el Modelo (DescuentoService). Para cambiarlas, se modifica la regla en el backend.
      </p>
      <div className="settings-list">
        <div>
          <h3>Descuento por volumen</h3>
          <p><span>1 a 5 unidades</span><strong>0%</strong></p>
          <p><span>6 a 20 unidades</span><strong>5%</strong></p>
          <p><span>21 a 50 unidades</span><strong>10%</strong></p>
          <p><span>Más de 50 unidades</span><strong>15%</strong></p>
        </div>
        <div>
          <h3>Tipo de cliente e impuestos</h3>
          <p><span>Cliente Minorista</span><strong>0% adicional</strong></p>
          <p><span>Institucional / Mayorista</span><strong>8% acumulativo</strong></p>
          <p><span>IVA</span><strong>12%</strong></p>
        </div>
      </div>
    </section>
  );
}
