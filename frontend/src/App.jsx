import { AppRouter } from './routes/AppRouter';
import './App.css';
import { CartProvider } from './context/CartContext';

function App() {
  return (
    <CartProvider>
      <AppRouter />
    </CartProvider>
  );
}

export default App;