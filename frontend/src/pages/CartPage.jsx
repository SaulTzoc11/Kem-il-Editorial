import { useState } from 'react';
import { useCart } from '../context/CartContext';
import { Link } from 'react-router-dom';
import axiosClient from '../api/axiosClient';

export default function CartPage() {
  const { cart, removeFromCart, clearCart, totalCartPrice } = useCart();
  
  // Estados para el motor de descuentos
  const [tipoCliente, setTipoCliente] = useState('Minorista');
  const [resumenOrden, setResumenOrden] = useState(null);
  const [loadingCalculo, setLoadingCalculo] = useState(false);

  // Función para calcular la orden consumiendo el backend 
  const handleCalcularDescuentos = async () => {
    try {
      setLoadingCalculo(true);
      // Preparamos el payload según el DTO del backend
      const payload = {
        items: cart.map(item => ({
          idLibro: item.idLibro,
          nombre: item.nombre,
          precio: item.precio,
          cantidad: item.cantidad
        })),
        tipoCliente: tipoCliente
      };

      const response = await axiosClient.post('/ordenes/calcular', payload);
      setResumenOrden(response.data);
    } catch (error) {
      console.error("Error al calcular la orden:", error);
      alert("Hubo un error al calcular los descuentos en el servidor.");
    } finally {
      setLoadingCalculo(false);
    }
  };

  if (cart.length === 0) {
    return (
      <div style={{ padding: '20px', textAlign: 'center' }}>
        <h2>Tu carrito está vacío 🛒</h2>
        <p>Aún no has agregado ningún libro de Kem'il Editorial.</p>
        <Link to="/catalogo" style={{ color: '#2b6cb0', textDecoration: 'underline' }}>
          Volver al catálogo
        </Link>
      </div>
    );
  }

  return (
    <div className='text-center mb-8'>
      <h1 className="text-3xl font-bold"> Carrito de Compras</h1>
      <p className="text-xl text-amber-400 font-semibold mt-1"> Kem'il Editorial</p>
      
      <div style={{ display: 'flex', flexDirection: 'column', gap: '15px', marginTop: '20px' }}>
        {cart.map((item) => (
          <div 
            key={item.idLibro} 
            style={{ 
              display: 'flex', 
              justifyContent: 'space-between', 
              alignItems: 'center', 
              border: '1px solid #ddd', 
              padding: '15px', 
              borderRadius: '8px',
              backgroundColor: '#fff',
              color: '#333'
            }}
          >
            <div>
              <h3 style={{ margin: '0 0 5px 0' }}>{item.nombre}</h3>
              <p style={{ margin: '0', color: '#666' }}>Formato: {item.formato} | Precio unitario: Q{item.precio}</p>
              <p style={{ margin: '5px 0 0 0' }}><strong>Cantidad:</strong> {item.cantidad}</p>
            </div>
            
            <div style={{ display: 'flex', alignItems: 'center', gap: '15px' }}>
              <strong>Subtotal: Q{(item.precio * item.cantidad).toFixed(2)}</strong>
              <button 
                onClick={() => removeFromCart(item.idLibro)}
                style={{ backgroundColor: '#e53e3e', color: 'white', border: 'none', padding: '6px 10px', borderRadius: '4px', cursor: 'pointer' }}
              >
                Eliminar 🗑️
              </button>
            </div>
          </div>
        ))}
      </div>

      <div style={{ marginTop: '20px', padding: '15px', backgroundColor: '#f7fafc', borderRadius: '8px', border: '1px solid #e2e8f0', color: '#333' }}>
        <label style={{ marginRight: '10px', fontWeight: 'bold' }}>Tipo de Cliente:</label>
        <select 
          value={tipoCliente} 
          onChange={(e) => setTipoCliente(e.target.value)}
          style={{ padding: '6px', borderRadius: '4px', border: '1px solid #cbd5e0' }}
        >
          <option value="Minorista">Cliente Minorista (0% adicional)</option>
          <option value="Mayorista">Cliente Institucional / Mayorista (8% adicional)</option>
        </select>
      </div>

      {resumenOrden && (
        <div style={{ marginTop: '20px', padding: '20px', backgroundColor: '#edf2f7', borderRadius: '8px', border: '1px solid #cbd5e0', color: '#333' }}>
          <h3>📋 Desglose Oficial de la Orden (RF-03.3)</h3>
          <p><strong>Subtotal Base:</strong> Q{resumenOrden.subtotalBase.toFixed(2)} ({resumenOrden.totalUnidades} unidades)</p>
          <p><strong>Descuento por Volumen:</strong> -Q{resumenOrden.descuentoVolumenMonto.toFixed(2)} <em>({resumenOrden.detallesReglaVolumen || resumenOrden.detalleReglaVolumen})</em></p>
          <p><strong>Descuento por Tipo de Cliente:</strong> -Q{resumenOrden.descuentoClienteMonto.toFixed(2)} <em>({resumenOrden.detallesReglaCliente || resumenOrden.detalleReglaCliente})</em></p>
          <p><strong>Subtotal con Descuentos:</strong> Q{resumenOrden.subtotalConDescuentos.toFixed(2)}</p>
          <p><strong>Impuestos (IVA 12%):</strong> Q{resumenOrden.impuestos.toFixed(2)}</p>
          <hr style={{ border: '0', borderTop: '1px solid #cbd5e0', margin: '10px 0' }}/>
          <h2 style={{ color: '#2b6cb0' }}>Total Final: Q{resumenOrden.totalFinal.toFixed(2)}</h2>
        </div>
      )}

      <div style={{ marginTop: '30px', borderTop: '2px solid #eee', paddingTop: '20px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <button 
          onClick={clearCart}
          style={{ backgroundColor: '#718096', color: 'white', border: 'none', padding: '10px 15px', borderRadius: '4px', cursor: 'pointer' }}
        >
          Vaciar Carrito
        </button>

        <div style={{ textAlign: 'right' }}>
          {!resumenOrden && <h2>Total Estimado: Q{totalCartPrice.toFixed(2)}</h2>}
          <button 
            disabled={loadingCalculo}
            style={{ backgroundColor: '#319795', color: 'white', border: 'none', padding: '12px 20px', borderRadius: '4px', cursor: 'pointer', fontSize: '16px', fontWeight: 'bold' }}
            onClick={handleCalcularDescuentos}
          >
            {loadingCalculo ? "Calculando..." : "Aplicar Descuento / Calcular Orden 🎟️"}
          </button>
        </div>
      </div>
    </div>
  );
}