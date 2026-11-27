import { useEffect, useState } from 'react';
import { StatusBadge } from '../../components/common/StatusBadge';
import { adminApi } from '../../services/api';
import type { AdminDashboardData } from '../../types';

export function AdminDashboard() {
  const [dashboard, setDashboard] = useState<AdminDashboardData | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [loadAttempt, setLoadAttempt] = useState(0);

  useEffect(() => {
    let isMounted = true;

    void adminApi.dashboard()
      .then((data) => {
        if (isMounted) {
          setDashboard(data);
          setLoadError(null);
        }
      })
      .catch(() => {
        if (isMounted) {
          setLoadError('The dashboard data could not be loaded. Check the API connection and try again.');
        }
      });

    return () => {
      isMounted = false;
    };
  }, [loadAttempt]);

  if (!dashboard) {
    if (loadError) {
      return (
        <section className="panel-card" role="alert">
          <h2>Admin dashboard unavailable</h2>
          <p>{loadError}</p>
          <button className="app-button button-primary" onClick={() => setLoadAttempt((attempt) => attempt + 1)}>
            Try again
          </button>
        </section>
      );
    }

    return <div className="panel-card">Loading dashboard...</div>;
  }

  return (
    <div className="dashboard-layout">
      <section className="summary-grid">
        <div className="summary-card">
          <span>Total bookings</span>
          <strong>{dashboard.summary.totalBookings}</strong>
        </div>
        <div className="summary-card">
          <span>Available vehicles</span>
          <strong>{dashboard.summary.availableVehicles}</strong>
        </div>
        <div className="summary-card">
          <span>Available drivers</span>
          <strong>{dashboard.summary.availableDrivers}</strong>
        </div>
        <div className="summary-card">
          <span>Revenue</span>
          <strong>${dashboard.summary.revenue.toFixed(2)}</strong>
        </div>
      </section>

      <section className="panel-card">
        <h2>Recent bookings</h2>
        <div className="data-list">
          {dashboard.bookings.map((booking) => (
            <div className="list-item" key={booking.id}>
              <div>
                <strong>{booking.goodsType}</strong>
                <p>{booking.customer}</p>
              </div>
              <div>
                <StatusBadge status={booking.status} />
                <p>${booking.estimatedCharge.toFixed(2)}</p>
              </div>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}
