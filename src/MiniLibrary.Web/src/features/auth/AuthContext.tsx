import {
  createContext,
  useContext,
  useState,
  useCallback,
  useEffect,
  type ReactNode,
} from 'react';
import { useNavigate } from 'react-router-dom';
import { apiClient } from '@/services/apiClient';
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

function parseJwtPayload(token: string): User | null {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) return null;
    const payload = JSON.parse(atob(parts[1]!));
    return {
      id: payload.sub ?? payload.nameid ?? '',
      email: payload.email ?? '',
      name: payload.name ?? payload.unique_name ?? '',
      role: payload.role ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? 'Member',
    };
  } catch {
    return null;
  }
}

function isTokenExpired(token: string): boolean {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) return true;
    const payload = JSON.parse(atob(parts[1]!));
    if (!payload.exp) return false;
    // Consider expired if less than 60 seconds remaining
    return payload.exp * 1000 < Date.now() + 60_000;
  } catch {
    return true;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>({
    user: null,
    token: null,
    isAuthenticated: false,
    isLoading: true,
  });

  useEffect(() => {
    const token = localStorage.getItem(TOKEN_KEY);
    if (token && !isTokenExpired(token)) {
      const user = parseJwtPayload(token);
      if (user) {
        setState({ user, token, isAuthenticated: true, isLoading: false });
        return;
      }
    }

    // Try to refresh if we have a refresh token
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
    if (refreshToken && token) {
      apiClient
        .post<{ token: string; refreshToken: string }>('/auth/refresh', { refreshToken })
        .then(({ data }) => {
          localStorage.setItem(TOKEN_KEY, data.token);
          localStorage.setItem(REFRESH_TOKEN_KEY, data.refreshToken);
          const user = parseJwtPayload(data.token);
          setState({
            user,
            token: data.token,
            isAuthenticated: !!user,
            isLoading: false,
          });
        })
        .catch(() => {
          localStorage.removeItem(TOKEN_KEY);
          localStorage.removeItem(REFRESH_TOKEN_KEY);
          setState({ user: null, token: null, isAuthenticated: false, isLoading: false });
        });
    } else {
      if (!token) {
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(REFRESH_TOKEN_KEY);
      }
      setState({ user: null, token: null, isAuthenticated: false, isLoading: false });
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
    const user = parseJwtPayload(token);
    setState({
      user,
      token,
      isAuthenticated: !!user,
      isLoading: false,
    });
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

/**
 * Hook that provides the navigate function alongside auth.
 * Use when you need to redirect after auth operations within components
 * that are inside <BrowserRouter>.
 */
export function useAuthWithNavigation() {
  const auth = useAuth();
  const navigate = useNavigate();
  return { ...auth, navigate };
}
