import { createContext, useContext, useEffect, useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import type { AuthResponse, User } from '../types';
import { authApi } from '../services/api';

interface AuthContextValue {
  user: User | null;
  token: string | null;
  isInitializing: boolean;
  isAuthenticated: boolean;
  login: (response: AuthResponse) => void;
  logout: () => void;
  setAuth: (response: AuthResponse) => void;
  refreshUser: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(() => localStorage.getItem('lhb_token'));
  const [isInitializing, setIsInitializing] = useState(() => Boolean(localStorage.getItem('lhb_token')));

  useEffect(() => {
    if (token) {
      localStorage.setItem('lhb_token', token);
    } else {
      localStorage.removeItem('lhb_token');
    }
  }, [token]);

  const refreshUser = async () => {
    if (!token) {
      setUser(null);
      setIsInitializing(false);
      return;
    }

    setIsInitializing(true);
    try {
      const current = await authApi.me();
      setUser(current);
    } catch {
      setUser(null);
      setToken(null);
    } finally {
      setIsInitializing(false);
    }
  };

  useEffect(() => {
    void refreshUser();
  }, [token]);

  const login = (response: AuthResponse) => {
    setAuth(response);
  };

  const setAuth = (response: AuthResponse) => {
    setToken(response.token);
    setUser({
      id: response.id,
      firstName: response.firstName,
      lastName: response.lastName,
      email: response.email,
      role: response.role,
    });
    setIsInitializing(false);
  };

  const logout = () => {
    setToken(null);
    setUser(null);
  };

  const value = useMemo<AuthContextValue>(() => ({
    user,
    token,
    isInitializing,
    isAuthenticated: Boolean(token),
    login,
    logout,
    setAuth,
    refreshUser,
  }), [user, token, isInitializing]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
