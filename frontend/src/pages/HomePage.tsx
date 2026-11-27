import { Link } from 'react-router-dom';
import { Button } from '../components/common/Button';

export function HomePage() {
  return (
    <div className="home-page">
      <section className="hero-panel">
        <div>
          <p className="eyebrow">Centralized logistics booking</p>
          <h1>Book home logistics without delays.</h1>
          <p className="lead">
            Customers schedule goods pickup and delivery, while operators, drivers, and admins manage
            allocation, pricing, and status updates through one workflow.
          </p>
          <div className="cta-row">
            <Link to="/register">
              <Button>Register</Button>
            </Link>
            <Link to="/login">
              <Button variant="secondary">Login</Button>
            </Link>
          </div>
        </div>
        <div className="hero-card">
          <h3>Booking snapshot</h3>
          <ul>
            <li>Pickup location</li>
            <li>Delivery location</li>
            <li>Goods type and quantity</li>
            <li>Vehicle and driver assignment</li>
            <li>Status tracking & history</li>
          </ul>
        </div>
      </section>

      <section className="feature-grid">
        <article className="feature-card">
          <h3>Customer flow</h3>
          <p>Request a booking, review estimate, and track status from pickup to delivery.</p>
        </article>
        <article className="feature-card">
          <h3>Provider operations</h3>
          <p>Manage vehicles, drivers, availability, and booking assignments in one place.</p>
        </article>
        <article className="feature-card">
          <h3>Admin control</h3>
          <p>Monitor booking volume and system performance across the platform.</p>
        </article>
      </section>
    </div>
  );
}
