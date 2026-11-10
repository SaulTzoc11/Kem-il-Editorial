import Icon from './Icon';

export default function QtyStepper({ item, onChange }) {
  return (
    <div className="qty" role="group" aria-label={`Cantidad de ${item.nombre}`}>
      <button aria-label="Disminuir cantidad" onClick={() => onChange(item.cantidad - 1)}>
        <Icon name="minus" size={14} />
      </button>
      <span aria-live="polite">{item.cantidad}</span>
      <button
        aria-label="Aumentar cantidad"
        disabled={item.stock != null && item.cantidad >= item.stock}
        onClick={() => onChange(item.cantidad + 1)}
      >
        <Icon name="plus" size={14} />
      </button>
    </div>
  );
}
