import axios from 'axios';
import type {
  AdminDashboardData,
  AuthResponse,
  Booking,
  BookingRequest,
  EstimateRequest,
  EstimateResponse,
  User,
  Vehicle,
} from '../types';

const api = axios.create({
  // In development Vite proxies /api to ASP.NET Core. Set VITE_API_BASE_URL
  // when the frontend and API are hosted on separate origins in production.
  baseURL: import.meta.env.VITE_API_BASE_URL ?? '',
  headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('lhb_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export const authApi = {
  login: async (email: string, password: string): Promise<AuthResponse> => {
    const { data } = await api.post<AuthResponse>('/api/auth/login', { email, password });
    return data;
  },
  register: async (payload: {
    firstName: string;
    lastName: string;
    email: string;
    phoneNumber: string;
    password: string;
    role: string;
  }): Promise<AuthResponse> => {
    const { data } = await api.post<AuthResponse>('/api/auth/register', payload);
    return data;
  },
  me: async (): Promise<User> => {
    const { data } = await api.get<User>('/api/auth/me');
    return data;
  },
};

export const bookingApi = {
  estimate: async (payload: EstimateRequest): Promise<EstimateResponse> => {
    const { data } = await api.post<EstimateResponse>('/api/bookings/estimate', payload);
    return data;
  },
  create: async (payload: BookingRequest): Promise<Booking> => {
    const { data } = await api.post<Booking>('/api/bookings', payload);
    return data;
  },
  myBookings: async (): Promise<Booking[]> => {
    const { data } = await api.get<Booking[]>('/api/bookings/me');
    return data;
  },
  all: async (): Promise<Booking[]> => {
    const { data } = await api.get<Booking[]>('/api/bookings');
    return data;
  },
  vehicles: async (): Promise<Vehicle[]> => {
    const { data } = await api.get<Vehicle[]>('/api/bookings/vehicles');
    return data;
  },
  updateStatus: async (bookingId: string, status: string, vehicleId?: string): Promise<Booking> => {
    const { data } = await api.patch<Booking>(`/api/bookings/${bookingId}/status`, { status, vehicleId });
    return data;
  },
};

export const adminApi = {
  dashboard: async (): Promise<AdminDashboardData> => {
    const { data } = await api.get<AdminDashboardData>('/api/admin/dashboard');
    return data;
  },
  vehicles: async (): Promise<Vehicle[]> => {
    const { data } = await api.get<Vehicle[]>('/api/admin/vehicles');
    return data;
  },
};

export default api;
