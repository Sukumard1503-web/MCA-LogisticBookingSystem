import { NavLink, Outlet } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { Button } from '../common/Button';

export function Layout() {
  const { user, isAuthenticated, logout } = useAuth();

  return (
    <div className="app-shell">
      <header className="topbar">
        <div className="brand-group">
          <NavLink to="/" className="brand-link">Logistics Home Booking</NavLink>
        </div>

        <nav className="nav-links" aria-label="Main navigation">
          <NavLink to="/">Home</NavLink>
          {!isAuthenticated && <NavLink to="/login">Login</NavLink>}
          {!isAuthenticated && <NavLink to="/register">Register</NavLink>}
          {user?.role === 'Customer' && <NavLink to="/customer">Customer</NavLink>}
          {user?.role === 'LogisticsProvider' && <NavLink to="/provider">Provider</NavLink>}
          {user?.role === 'Admin' && <NavLink to="/admin">Admin</NavLink>}
        </nav>

        {isAuthenticated && (
          <div className="user-bar">
            <span>
              {user?.firstName} {user?.lastName}
            </span>
            <Button variant="secondary" onClick={logout}>Logout</Button>
          </div>
        )}
      </header>

      <main className="page-container">
        <Outlet />
      </main>
    </div>
  );
}
