import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';
import { useEffect, useState } from 'react';
import { setMockUserId } from './api/client';
import PlanSelect from './pages/PlanSelect';
import PassList from './pages/PassList';
import QrDisplay from './pages/QrDisplay';
import CheckoutReturn from './pages/CheckoutReturn';
import './App.css';

function App() {
  const [initialized, setInitialized] = useState(false);

  useEffect(() => {
    setMockUserId('test-user-001');
    setInitialized(true);
  }, []);

  if (!initialized) {
    return <div className="loading">Loading...</div>;
  }

  return (
    <BrowserRouter>
      <div className="app">
        <header className="header">
          <h1>Unmanned Access</h1>
          <nav>
            <Link to="/">Plans</Link>
            <Link to="/passes">My Passes</Link>
          </nav>
        </header>
        <main className="main">
          <Routes>
            <Route path="/" element={<PlanSelect />} />
            <Route path="/checkout/return" element={<CheckoutReturn />} />
            <Route path="/passes" element={<PassList />} />
            <Route path="/passes/:id/qr" element={<QrDisplay />} />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  );
}

export default App;
