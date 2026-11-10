import { AuthProvider } from './context/AuthContext';
import { CartProvider } from './context/CartContext';
import { AppRouter } from './routes/AppRouter';

// Orden de los providers (importa):
//   AuthProvider  -> CartProvider lo necesita (carrito por usuario)
//   BrowserRouter -> dentro de AppRouter; UIProvider lo necesita (los overlays usan navigate)
function App() {
  return (
    <AuthProvider>
      <CartProvider>
        <AppRouter />
      </CartProvider>
    </AuthProvider>
  );
}

export default App;
