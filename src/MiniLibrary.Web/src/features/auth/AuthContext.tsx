import { createContext, useContext, useState, useCallback, useEffect, type ReactNode } from 'react';
import type { User } from '@/types/models';

interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
}

interface AuthContextValue extends AuthState {
  login: (provider: 'google' | 'microsoft') => void;
  logout: () => void;
  handleCallback: (token: string, refreshToken: string) => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

const TOKEN_KEY = 'auth_token';
const REFRESH_TOKEN_KEY = 'auth_refresh_token';

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>({
    user: null,
    token: localStorage.getItem(TOKEN_KEY),
    isAuthenticated: !!localStorage.getItem(TOKEN_KEY),
    isLoading: true,
  });

  useEffect(() => {
    const token = localStorage.getItem(TOKEN_KEY);
    if (token) {
      // Decode JWT payload to get user info
      try {
        const payload = JSON.parse(atob(token.split('.')[1]!));
        setState({
          user: {
            id: payload.sub,
            email: payload.email,
            name: payload.name,
            role: payload.role,
          },
          token,
          isAuthenticated: true,
          isLoading: false,
        });
      } catch {
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(REFRESH_TOKEN_KEY);
        setState({ user: null, token: null, isAuthenticated: false, isLoading: false });
      }
    } else {
      setState((prev) => ({ ...prev, isLoading: false }));
    }
  }, []);

  const login = useCallback((provider: 'google' | 'microsoft') => {
    window.location.href = `/api/auth/login/${provider}`;
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    setState({ user: null, token: null, isAuthenticated: false, isLoading: false });
    window.location.href = '/login';
  }, []);

  const handleCallback = useCallback((token: string, refreshToken: string) => {
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
    try {
      const payload = JSON.parse(atob(token.split('.')[1]!));
      setState({
        user: {
          id: payload.sub,
          email: payload.email,
          name: payload.name,
          role: payload.role,
        },
        token,
        isAuthenticated: true,
        isLoading: false,
      });
    } catch {
      setState({ user: null, token: null, isAuthenticated: false, isLoading: false });
    }
  }, []);

  return (
    <AuthContext.Provider value={{ ...state, login, logout, handleCallback }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
}
