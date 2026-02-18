import { useState } from 'react';
import axios from 'axios';
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import './App.css';

export const api = axios.create({ baseURL: '/api' });

function App() {
  const [token, setToken] = useState<string | null>(null);

  const handleLogin = (jwt: string) => {
    setToken(jwt);
    api.defaults.headers.common['Authorization'] = `Bearer ${jwt}`;
  };

  const handleLogout = () => {
    setToken(null);
    delete api.defaults.headers.common['Authorization'];
  };

  if (!token) {
    return <Login onLogin={handleLogin} />;
  }

  return <Dashboard onLogout={handleLogout} />;
}

export default App;
