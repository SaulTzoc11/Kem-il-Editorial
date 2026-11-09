import CatalogoPage from './pages/CatalogoPage';
import CartPage from './pages/CartPage';
import RolePortal from './RolePortal'; 

function App() {
  return (
    <CartProvider>
      <Router>
        <div className="min-h-screen bg-gray-900 text-gray-100 font-sans">
          <Routes>
            <Route path="/" element={<Navigate to="/catalogo" replace />} />
            <Route path="/catalogo" element={<CatalogoPage />} />
            <Route path="/carrito" element={<CartPage />} />
            <Route path="/portal" element={<RolePortal />} />
          </Routes>
        </div>
      </Router>
    </CartProvider>
  );
}

export default App;