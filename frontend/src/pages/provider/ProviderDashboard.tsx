import { useEffect, useState } from 'react';
import { Button } from '../../components/common/Button';
import { StatusBadge } from '../../components/common/StatusBadge';
import { adminApi, bookingApi } from '../../services/api';
import type { Booking, Vehicle } from '../../types';

const nextStatuses: Record<string, string[]> = {
  Submitted: ['Assigned', 'Rejected', 'Cancelled'],
  Assigned: ['InTransit', 'Cancelled'],
  InTransit: ['Delivered'],
  Delivered: ['Completed'],
};

export function ProviderDashboard() {
  const [bookings, setBookings] = useState<Booking[]>([]);
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [vehicleChoices, setVehicleChoices] = useState<Record<string, string>>({});
  const [busyBookingId, setBusyBookingId] = useState<string | null>(null);
  const [error, setError] = useState('');

  useEffect(() => {
    void loadData();
  }, []);

  const loadData = async () => {
    try {
      const [bookingData, vehicleData] = await Promise.all([
        bookingApi.all(),
        adminApi.vehicles(),
      ]);
      setBookings(bookingData);
      setVehicles(vehicleData);
    } catch {
      setError('Could not load provider operations. Please refresh and try again.');
    }
  };

  const updateBooking = async (booking: Booking, status: string) => {
    setBusyBookingId(booking.id);
    setError('');
    try {
      const updated = await bookingApi.updateStatus(
        booking.id,
        status,
        status === 'Assigned' ? vehicleChoices[booking.id] : undefined,
      );
      setBookings((current) => current.map((item) => item.id === updated.id ? updated : item));
      if (status === 'Assigned' || status === 'InTransit') {
        setVehicles((current) => current.map((vehicle) => vehicle.id === (vehicleChoices[booking.id] || current.find((item) => item.name === booking.vehicleName)?.id)
          ? { ...vehicle, status: 'Busy' }
          : vehicle));
      } else if (['Delivered', 'Completed', 'Cancelled', 'Rejected'].includes(status)) {
        setVehicles((current) => current.map((vehicle) => vehicle.name === booking.vehicleName
          ? { ...vehicle, status: 'Available' }
          : vehicle));
      }
      setVehicleChoices((current) => ({ ...current, [booking.id]: '' }));
    } catch {
      setError('Could not update this booking. Check the vehicle assignment and try again.');
    } finally {
      setBusyBookingId(null);
    }
  };

  return (
    <div className="dashboard-layout">
      {error && <div className="error-box" role="alert">{error}</div>}
      <section className="panel-card">
        <h2>Available fleet</h2>
        <div className="data-list compact">
          {vehicles.length === 0 ? <div className="empty-state">No vehicles available.</div> : vehicles.map((vehicle) => (
            <div className="list-item" key={vehicle.id}>
              <div><strong>{vehicle.name}</strong><p>{vehicle.category}</p></div>
              <div><span>{vehicle.status}</span><p>{vehicle.capacityKg} kg</p></div>
            </div>
          ))}
        </div>
      </section>

      <section className="panel-card">
        <h2>Booking queue</h2>
        <div className="data-list">
          {bookings.length === 0 ? <div className="empty-state">No bookings in the queue.</div> : bookings.map((booking) => {
            const actions = nextStatuses[booking.status] ?? [];
            const eligibleVehicles = vehicles.filter((vehicle) =>
              (vehicle.status === 'Available' || vehicle.id === vehicleChoices[booking.id])
              && vehicle.capacityKg >= booking.approximateLoadKg,
            );
            return (
              <div className="list-item" key={booking.id}>
                <div>
                  <strong>{booking.goodsType}</strong>
                  <p>{booking.pickupLocation} → {booking.deliveryLocation}</p>
                  <p>{booking.approximateLoadKg} kg · {booking.customerName}</p>
                </div>
                <div>
                  <StatusBadge status={booking.status} />
                  <p>${booking.estimatedCharge.toFixed(2)}</p>
                  {booking.vehicleName && <p>{booking.vehicleName}{booking.driverName ? ` · ${booking.driverName}` : ''}</p>}
                  {actions.length > 0 && (
                    <div className="inline-actions">
                      {actions.includes('Assigned') && (
                        <label>
                          Vehicle
                          <select
                            value={vehicleChoices[booking.id] ?? ''}
                            onChange={(event) => setVehicleChoices((current) => ({ ...current, [booking.id]: event.target.value }))}
                          >
                            <option value="">Choose vehicle</option>
                            {eligibleVehicles.map((vehicle) => <option key={vehicle.id} value={vehicle.id}>{vehicle.name}</option>)}
                          </select>
                        </label>
                      )}
                      {actions.map((status) => (
                        <Button
                          key={status}
                          type="button"
                          variant={status === 'Rejected' || status === 'Cancelled' ? 'secondary' : 'primary'}
                          disabled={busyBookingId === booking.id || (status === 'Assigned' && !vehicleChoices[booking.id])}
                          onClick={() => void updateBooking(booking, status)}
                        >
                          {busyBookingId === booking.id ? 'Saving…' : status === 'InTransit' ? 'Mark in transit' : status === 'Completed' ? 'Complete' : status}
                        </Button>
                      ))}
                    </div>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      </section>
    </div>
  );
}
