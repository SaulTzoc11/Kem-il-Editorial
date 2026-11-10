import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { mensajeDeError } from '../api/axiosClient';
import { cotizacionesApi } from '../api/cotizacionesApi';
import { useAuth } from '../context/AuthContext';
import { useUI } from '../context/UIContext';
import useModal from '../hooks/useModal';
import { esInstitucional } from '../utils/roles';
import Icon from './Icon';

const VACIO = {
  tituloObra: '',
  cantidadEstimada: '',
  acabadoPortada: 'Tapa blanda',
  tipoPapel: 'Bond 75 g',
  fechaRequeridaEntrega: '',
  comentariosCliente: '',
};

const hoy = () => new Date().toISOString().slice(0, 10);

// RF-04: solo un Cliente Institucional autenticado puede pedir cotizaciones / ediciones especiales.
export default function QuoteModal() {
  const { quoteOpen, closeQuote } = useUI();
  const { user } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState(VACIO);
  const [enviada, setEnviada] = useState(false);
  const [enviando, setEnviando] = useState(false);
  const [error, setError] = useState('');

  const cerrar = () => {
    closeQuote();
    setEnviada(false);
    setError('');
  };
  useModal(quoteOpen, cerrar);
  if (!quoteOpen) return null;

  const cambiar = (e) => setForm({ ...form, [e.target.name]: e.target.value });

  const enviar = async (e) => {
    e.preventDefault();
    setError('');
    setEnviando(true);
    try {
      await cotizacionesApi.crear({ ...form, cantidadEstimada: Number(form.cantidadEstimada) });
      setEnviada(true);
      setForm(VACIO);
    } catch (err) {
      setError(mensajeDeError(err, 'No se pudo enviar la solicitud.'));
    } finally {
      setEnviando(false);
    }
  };

  const irALogin = () => {
    cerrar();
    navigate('/login', { state: { from: window.location.pathname } });
  };

  let contenido;
  if (!user) {
    contenido = (
      <div className="quote-gate">
        <span className="eyebrow">Atención institucional</span>
        <h2>Inicia sesión para cotizar</h2>
        <p>Las solicitudes de cotización están disponibles para cuentas institucionales registradas.</p>
        <button className="button button-primary" onClick={irALogin}>
          Iniciar sesión <Icon name="arrow" />
        </button>
      </div>
    );
  } else if (!esInstitucional(user.rol)) {
    contenido = (
      <div className="quote-gate">
        <span className="eyebrow">Atención institucional</span>
        <h2>Solo para instituciones</h2>
        <p>
          Tu cuenta ({user.rol}) no tiene acceso al módulo de cotizaciones. Si compras para un colegio,
          universidad o empresa, solicita una cuenta institucional a nuestro equipo.
        </p>
        <button className="button button-dark" onClick={cerrar}>
          Entendido
        </button>
      </div>
    );
  } else if (enviada) {
    contenido = (
      <div className="quote-success">
        <span>
          <Icon name="check" size={30} />
        </span>
        <h2>Solicitud recibida</h2>
        <p>
          Un asesor comercial revisará tus requerimientos y te responderá con una cotización formal.
        </p>
        <button className="button button-dark" onClick={cerrar}>
          Volver al catálogo
        </button>
      </div>
    );
  } else {
    contenido = (
      <>
        <span className="eyebrow">Atención institucional</span>
        <h2>Cuéntanos sobre tu proyecto</h2>
        <p>Solicitando como {user.nombre}. Prepararemos una propuesta a tu medida.</p>
        <form onSubmit={enviar}>
          <label>
            <span>Obra o título</span>
            <input name="tituloObra" onChange={cambiar} placeholder="Título de interés o edición especial" required value={form.tituloObra} />
          </label>
          <div className="form-row">
            <label>
              <span>Cantidad estimada</span>
              <input min="1" name="cantidadEstimada" onChange={cambiar} placeholder="50" required type="number" value={form.cantidadEstimada} />
            </label>
            <label>
              <span>Fecha requerida de entrega</span>
              <input min={hoy()} name="fechaRequeridaEntrega" onChange={cambiar} required type="date" value={form.fechaRequeridaEntrega} />
            </label>
          </div>
          <div className="form-row">
            <label>
              <span>Acabado de portada</span>
              <select name="acabadoPortada" onChange={cambiar} value={form.acabadoPortada}>
                <option>Tapa blanda</option>
                <option>Tapa dura</option>
                <option>Edición especial</option>
              </select>
            </label>
            <label>
              <span>Tipo de papel</span>
              <select name="tipoPapel" onChange={cambiar} value={form.tipoPapel}>
                <option>Bond 75 g</option>
                <option>Bond ahuesado 90 g</option>
                <option>Couché 115 g</option>
                <option>Reciclado</option>
              </select>
            </label>
          </div>
          <label>
            <span>Detalles adicionales</span>
            <textarea
              maxLength={500}
              name="comentariosCliente"
              onChange={cambiar}
              placeholder="Personalización u otras necesidades..."
              rows={3}
              value={form.comentariosCliente}
            />
          </label>
          {error && <p className="form-error" role="alert">{error}</p>}
          <button className="button button-primary" disabled={enviando} type="submit">
            {enviando ? 'Enviando...' : 'Enviar solicitud'} <Icon name="arrow" />
          </button>
        </form>
      </>
    );
  }

  return (
    <div className="modal-backdrop" onMouseDown={cerrar}>
      <section
        aria-label="Solicitar cotización"
        aria-modal="true"
        className="quote-modal"
        onMouseDown={(e) => e.stopPropagation()}
        role="dialog"
      >
        <button aria-label="Cerrar" className="modal-close" onClick={cerrar}>
          <Icon name="close" />
        </button>
        {contenido}
      </section>
    </div>
  );
}
