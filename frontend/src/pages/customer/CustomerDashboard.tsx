import { useEffect, useState } from 'react';
import { Button } from '../../components/common/Button';
import { StatusBadge } from '../../components/common/StatusBadge';
import { bookingApi } from '../../services/api';
import type { Booking, Vehicle } from '../../types';

export function CustomerDashboard() {
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [bookings, setBookings] = useState<Booking[]>([]);
  const [form, setForm] = useState({
    pickupLocation: 'Lagos Mainland',
    deliveryLocation: 'Ikeja',
    goodsType: 'Furniture',
    approximateLoadKg: 320,
    vehicleId: '',
  });
  const [estimate, setEstimate] = useState<number | null>(null);
  const [message, setMessage] = useState('');

  useEffect(() => {
    void loadData();
  }, []);

  const loadData = async () => {
    const [vehicleData, bookingData] = await Promise.all([
      bookingApi.vehicles(),
      bookingApi.myBookings(),
    ]);
    setVehicles(vehicleData);
    setBookings(bookingData);
  };

  const handleEstimate = async () => {
    const result = await bookingApi.estimate({
      pickupLocation: form.pickupLocation,
      deliveryLocation: form.deliveryLocation,
      goodsType: form.goodsType,
      approximateLoadKg: form.approximateLoadKg,
    });
    setEstimate(result.estimatedCharge);
  };

  const handleCreateBooking = async () => {
    const booking = await bookingApi.create({
      pickupLocation: form.pickupLocation,
      deliveryLocation: form.deliveryLocation,
      goodsType: form.goodsType,
      approximateLoadKg: form.approximateLoadKg,
      vehicleId: form.vehicleId || undefined,
    });
    setMessage(`Booking created successfully: ${booking.status}`);
    setBookings((current) => [booking, ...current]);
  };

  return (
    <div className="dashboard-layout">
      <section className="panel-card">
        <h2>Book a shipment</h2>
        <div className="stacked-form">
          <label>
            Pickup location
            <input value={form.pickupLocation} onChange={(e) => setForm({ ...form, pickupLocation: e.target.value })} />
          </label>
          <label>
            Delivery location
            <input value={form.deliveryLocation} onChange={(e) => setForm({ ...form, deliveryLocation: e.target.value })} />
          </label>
          <label>
            Goods type
            <input value={form.goodsType} onChange={(e) => setForm({ ...form, goodsType: e.target.value })} />
          </label>
          <label>
            Approximate load (kg)
            <input type="number" value={form.approximateLoadKg} onChange={(e) => setForm({ ...form, approximateLoadKg: Number(e.target.value) })} />
          </label>
          <label>
            Preferred vehicle
            <select value={form.vehicleId} onChange={(e) => setForm({ ...form, vehicleId: e.target.value })}>
              <option value="">Select a vehicle</option>
              {vehicles.map((vehicle) => (
                <option key={vehicle.id} value={vehicle.id}>
                  {vehicle.name} - {vehicle.category}
                </option>
              ))}
            </select>
          </label>
          <div className="inline-actions">
            <Button type="button" variant="secondary" onClick={handleEstimate}>Estimate</Button>
            <Button type="button" onClick={handleCreateBooking}>Create booking</Button>
          </div>
          {estimate !== null && <div className="info-box">Estimated charge: ${estimate.toFixed(2)}</div>}
          {message && <div className="success-box">{message}</div>}
        </div>
      </section>

      <section className="panel-card">
        <h2>My bookings</h2>
        <div className="data-list">
          {bookings.length === 0 ? (
            <div className="empty-state">No bookings yet.</div>
          ) : (
            bookings.map((booking) => (
              <div className="list-item" key={booking.id}>
                <div>
                  <strong>{booking.goodsType}</strong>
                  <p>
                    {booking.pickupLocation} → {booking.deliveryLocation}
                  </p>
                </div>
                <div>
                  <StatusBadge status={booking.status} />
                  <p>${booking.estimatedCharge.toFixed(2)}</p>
                </div>
              </div>
            ))
          )}
        </div>
      </section>
    </div>
  );
}
