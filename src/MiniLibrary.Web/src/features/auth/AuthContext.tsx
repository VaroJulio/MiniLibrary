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
import type { User, UserRole } from '@/types/models';

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
}

interface AuthContextValue extends AuthState {
  login: (provider: 'google' | 'microsoft') => void;
  logout: () => void;
  /** Re-fetches current user from /auth/me (call after login/callback) */
  refreshUser: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

/**
 * AuthProvider that uses HttpOnly cookie-based authentication.
 * No tokens are stored in JavaScript — the browser automatically sends
 * auth cookies with every request. User state is determined by calling
 * GET /auth/me on mount.
 */
export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>({
    user: null,
    isAuthenticated: false,
    isLoading: true,
  });

  const fetchCurrentUser = useCallback(async () => {
    try {
      // Use a direct request without the 401 interceptor redirect
      // to avoid infinite loops when not authenticated on the login page
      const { data } = await apiClient.get<{
        id: string;
        email: string;
        fullName: string;
        role: string;
      }>('/auth/me', { _skipAuthRedirect: true } as never);
      setState({
        user: {
          id: data.id,
          email: data.email,
          name: data.fullName,
          role: data.role as UserRole,
        },
        isAuthenticated: true,
        isLoading: false,
      });
    } catch {
      // Not authenticated or token expired (refresh also failed)
      setState({ user: null, isAuthenticated: false, isLoading: false });
    }
  }, []);

  useEffect(() => {
    fetchCurrentUser();
  }, [fetchCurrentUser]);

  const login = useCallback((provider: 'google' | 'microsoft') => {
    const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? '';
    let authUrl: string;
    if (apiBaseUrl.startsWith('http')) {
      try {
        const url = new URL(apiBaseUrl);
        authUrl = url.origin;
      } catch {
        authUrl = 'http://localhost:5000';
      }
    } else {
      authUrl = 'http://localhost:5000';
    }
    window.location.href = `${authUrl}/api/auth/login/${provider}`;
  }, []);

  const logout = useCallback(async () => {
    try {
      await apiClient.post('/auth/logout');
    } catch {
      // Ignore errors on logout — cookies will be cleared by server
    }
    setState({ user: null, isAuthenticated: false, isLoading: false });
    window.location.href = '/login';
  }, []);

  const refreshUser = useCallback(async () => {
    await fetchCurrentUser();
  }, [fetchCurrentUser]);

  return (
    <AuthContext.Provider value={{ ...state, login, logout, refreshUser }}>
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
