export type UserRole = 'Customer' | 'LogisticsProvider' | 'Driver' | 'Admin';

export interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: UserRole;
}

export interface AuthResponse {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: UserRole;
  token: string;
}

export interface Booking {
  id: string;
  pickupLocation: string;
  deliveryLocation: string;
  goodsType: string;
  approximateLoadKg: number;
  estimatedCharge: number;
  finalCharge: number;
  status: string;
  customerName: string;
  driverName?: string | null;
  vehicleName?: string | null;
  createdAt: string;
}

export interface Vehicle {
  id: string;
  name: string;
  category: string;
  capacityKg: number;
  dailyRate: number;
  status: string;
}

export interface BookingRequest {
  pickupLocation: string;
  deliveryLocation: string;
  goodsType: string;
  approximateLoadKg: number;
  vehicleId?: string;
}

export interface EstimateRequest {
  pickupLocation: string;
  deliveryLocation: string;
  goodsType: string;
  approximateLoadKg: number;
}

export interface EstimateResponse {
  estimatedCharge: number;
  currency: string;
  summary: string;
}

export interface DashboardSummary {
  totalBookings: number;
  availableVehicles: number;
  availableDrivers: number;
  revenue: number;
}

export interface AdminDashboardData {
  summary: DashboardSummary;
  bookings: Array<{
    id: string;
    customer: string;
    goodsType: string;
    status: string | number;
    estimatedCharge: number;
    createdAt: string;
  }>;
}
