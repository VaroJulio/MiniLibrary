import axios from 'axios';

const TOKEN_KEY = 'auth_token';
const REFRESH_TOKEN_KEY = 'auth_refresh_token';

/**
 * Axios instance pre-configured with:
 * - Base URL from environment or proxy
 * - JWT Bearer token in Authorization header
 * - X-Correlation-Id header for request tracing
 * - Automatic token refresh on 401 responses
 */
export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? '/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

function generateCorrelationId(): string {
  return crypto.randomUUID();
}

// Request interceptor: attach JWT and Correlation ID
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem(TOKEN_KEY);
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  config.headers['X-Correlation-Id'] = generateCorrelationId();
  return config;
});

// Response interceptor: handle 401 with token refresh
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (token: string) => void;
  reject: (error: unknown) => void;
}> = [];

function processQueue(error: unknown, token: string | null = null) {
  failedQueue.forEach((pending) => {
    if (error) {
      pending.reject(error);
    } else if (token) {
      pending.resolve(token);
    }
  });
  failedQueue = [];
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // Only attempt refresh on 401 and if we haven't retried yet
    if (error.response?.status !== 401 || originalRequest._retry) {
      return Promise.reject(error);
    }

    // Don't try to refresh if the failing request was the refresh itself
    if (originalRequest.url?.includes('/auth/refresh')) {
      clearTokens();
      window.location.href = '/login';
      return Promise.reject(error);
    }

    if (isRefreshing) {
      // Queue this request until the refresh completes
      return new Promise<string>((resolve, reject) => {
        failedQueue.push({ resolve, reject });
      }).then((token) => {
        originalRequest.headers.Authorization = `Bearer ${token}`;
        return apiClient(originalRequest);
      });
    }

    originalRequest._retry = true;
    isRefreshing = true;

    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
    if (!refreshToken) {
      isRefreshing = false;
      clearTokens();
      window.location.href = '/login';
      return Promise.reject(error);
    }

    try {
      const { data } = await axios.post<{ token: string; refreshToken: string }>(
        `${apiClient.defaults.baseURL}/auth/refresh`,
        { refreshToken },
      );

      localStorage.setItem(TOKEN_KEY, data.token);
      localStorage.setItem(REFRESH_TOKEN_KEY, data.refreshToken);

      processQueue(null, data.token);
      originalRequest.headers.Authorization = `Bearer ${data.token}`;
      return apiClient(originalRequest);
    } catch (refreshError) {
      processQueue(refreshError);
      clearTokens();
      window.location.href = '/login';
      return Promise.reject(refreshError);
    } finally {
      isRefreshing = false;
    }
  },
);

function clearTokens() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
}
