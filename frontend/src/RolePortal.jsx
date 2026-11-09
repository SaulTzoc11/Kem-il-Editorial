import React, { useState } from 'react';

export default function RolePortal() {
  const [selectedRole, setSelectedRole] = useState<string>('Cliente');
  const rolesDisponibles = ['Cliente', 'Administrador', 'Editor', 'Almacén', 'Ventas'];

  return (
    <div className="max-w-4xl mx-auto p-6 mt-10 bg-gray-800 rounded-xl shadow-lg border border-gray-700">
      <h2 className="text-2xl font-bold mb-4 text-amber-400">Portal de Acceso y Roles - Kem'il Editorial</h2>
      <p className="text-gray-300 mb-6">
        Selecciona un rol para simular las vistas y permisos del sistema (módulo en integración con el backend de autenticación):
      </p>
      
      <div className="grid grid-cols-2 md:grid-cols-5 gap-3 mb-6">
        {rolesDisponibles.map((rol) => (
          <button
            key={rol}
            onClick={() => setSelectedRole(rol)}
            className={`p-3 rounded-lg font-medium transition ${
              selectedRole === rol 
                ? 'bg-amber-600 text-white shadow-md' 
                : 'bg-gray-700 text-gray-300 hover:bg-gray-600'
            }`}
          >
            {rol}
          </button>
        ))}
      </div>

      <div className="p-4 bg-gray-900 rounded-lg border border-gray-700">
        <h3 className="text-lg font-semibold text-emerald-400 mb-2">Vista Activa: {selectedRole}</h3>
        <p className="text-sm text-gray-400">
          {selectedRole === 'Administrador' && 'Acceso total al panel de control, gestión de usuarios, auditoría y catálogos globales.'}
          {selectedRole === 'Cliente' && 'Acceso al catálogo general, carrito de compras, cálculo de descuentos RF-03 y pedidos especiales.'}
          {selectedRole === 'Editor' && 'Gestión de manuscritos, revisión de textos y asignación de formatos editoriales.'}
          {selectedRole === 'Almacén' && 'Control de stock físico, inventario de libros y procesamiento de envíos.'}
          {selectedRole === 'Ventas' && 'Monitoreo de cotizaciones, atención a clientes institucionales y control mayorista.'}
        </p>
      </div>
    </div>
  );
}