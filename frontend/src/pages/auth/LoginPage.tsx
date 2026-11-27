import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Button } from '../../components/common/Button';
import { useAuth } from '../../context/AuthContext';
import { authApi } from '../../services/api';

export function LoginPage() {
  const navigate = useNavigate();
  const { login } = useAuth();
  const [email, setEmail] = useState('customer@example.com');
  const [password, setPassword] = useState('Password123!');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setError('');
    setIsLoading(true);

    try {
      const response = await authApi.login(email, password);
      login(response);
      navigate(response.role === 'Customer' ? '/customer' : response.role === 'LogisticsProvider' ? '/provider' : response.role === 'Admin' ? '/admin' : '/');
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Login failed';
      setError(message);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="auth-panel">
      <div className="panel-card auth-card">
        <h2>Welcome back</h2>
        <p>Sign in to manage your bookings and logistics workflow.</p>
        <form onSubmit={handleSubmit} className="stacked-form">
          <label>
            Email
            <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
          </label>
          <label>
            Password
            <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
          </label>
          {error && <div className="error-box">{error}</div>}
          <Button type="submit" disabled={isLoading}>
            {isLoading ? 'Signing in...' : 'Login'}
          </Button>
        </form>
        <p className="tiny-link">
          No account? <Link to="/register">Create one now</Link>
        </p>
      </div>
    </div>
  );
}
