import axios from 'axios';
import { ApiError, type ProblemDetailsResponse } from './ApiError';

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

    // --- 401 Token Refresh Logic ---
    if (error.response?.status === 401 && !originalRequest._retry) {
      // Don't try to refresh if the failing request was the refresh itself
      if (originalRequest.url?.includes('/auth/refresh')) {
        clearTokens();
        window.location.href = '/login';
        return Promise.reject(toApiError(error));
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
        return Promise.reject(toApiError(error));
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
        return Promise.reject(toApiError(refreshError));
      } finally {
        isRefreshing = false;
      }
    }

    // --- Transform all other errors to ApiError ---
    return Promise.reject(toApiError(error));
  },
);

/**
 * Transforms an Axios error (or unknown error) into a typed ApiError.
 * If the response body matches ProblemDetails shape, extracts structured fields.
 * Otherwise, creates a generic ApiError with available information.
 */
function toApiError(error: unknown): ApiError {
  if (error instanceof ApiError) {
    return error;
  }

  if (axios.isAxiosError(error) && error.response) {
    const { status, data } = error.response;
    const problemDetails = data as ProblemDetailsResponse | undefined;

    // If the response has a ProblemDetails-like shape (has 'title' field)
    if (problemDetails && typeof problemDetails.title === 'string') {
      return new ApiError({
        ...problemDetails,
        status: problemDetails.status ?? status,
      });
    }

    // Non-ProblemDetails error response (fallback)
    return new ApiError({
      status,
      title: error.response.statusText || 'Error',
      detail: typeof data === 'string' ? data : error.message,
    });
  }

  // Network error or unknown error (no response received)
  if (axios.isAxiosError(error) && !error.response) {
    return new ApiError({
      status: 0,
      title: 'Network Error',
      detail: 'Unable to connect to the server. Please check your connection.',
    });
  }

  // Completely unknown error
  return new ApiError({
    status: 500,
    title: 'Unexpected Error',
    detail: error instanceof Error ? error.message : 'An unexpected error occurred.',
  });
}

function clearTokens() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
}
