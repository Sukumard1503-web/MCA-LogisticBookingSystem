import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Button } from '../../components/common/Button';
import { useAuth } from '../../context/AuthContext';
import { authApi } from '../../services/api';

export function RegisterPage() {
  const navigate = useNavigate();
  const { login } = useAuth();
  const [form, setForm] = useState({
    firstName: 'Jane',
    lastName: 'Customer',
    email: 'customer@example.com',
    phoneNumber: '+1234567890',
    password: 'Password123!',
    role: 'Customer',
  });
  const [error, setError] = useState('');

  const handleChange = (field: string, value: string) => {
    setForm((current) => ({ ...current, [field]: value }));
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setError('');

    try {
      const response = await authApi.register(form);
      login(response);
      navigate('/customer');
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Registration failed';
      setError(message);
    }
  };

  return (
    <div className="auth-panel">
      <div className="panel-card auth-card">
        <h2>Create account</h2>
        <form onSubmit={handleSubmit} className="stacked-form">
          <div className="two-column-grid">
            <label>
              First name
              <input value={form.firstName} onChange={(e) => handleChange('firstName', e.target.value)} required />
            </label>
            <label>
              Last name
              <input value={form.lastName} onChange={(e) => handleChange('lastName', e.target.value)} required />
            </label>
          </div>
          <label>
            Email
            <input type="email" value={form.email} onChange={(e) => handleChange('email', e.target.value)} required />
          </label>
          <label>
            Phone
            <input value={form.phoneNumber} onChange={(e) => handleChange('phoneNumber', e.target.value)} required />
          </label>
          <label>
            Password
            <input type="password" value={form.password} onChange={(e) => handleChange('password', e.target.value)} required />
          </label>
          <label>
            Role
            <select value={form.role} onChange={(e) => handleChange('role', e.target.value)}>
              <option value="Customer">Customer</option>
              <option value="LogisticsProvider">Logistics Provider</option>
              <option value="Driver">Driver</option>
              <option value="Admin">Admin</option>
            </select>
          </label>
          {error && <div className="error-box">{error}</div>}
          <Button type="submit">Create account</Button>
        </form>
        <p className="tiny-link">
          Already registered? <Link to="/login">Log in</Link>
        </p>
      </div>
    </div>
  );
}
