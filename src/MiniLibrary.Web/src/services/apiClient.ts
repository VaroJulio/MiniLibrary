import axios from 'axios';
import { ApiError, type ProblemDetailsResponse } from './ApiError';

/**
 * Axios instance pre-configured with:
 * - Base URL from environment or proxy
 * - withCredentials for HttpOnly cookie auth
 * - X-XSRF-TOKEN header for CSRF protection (double-submit cookie pattern)
 * - X-Correlation-Id header for request tracing
 * - Automatic redirect to /login on 401 (cookie expired)
 *
 * Tokens are NEVER stored in JavaScript — they live in HttpOnly cookies
 * managed entirely by the browser and server.
 */
export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? '/api',
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true, // Send HttpOnly cookies with every request
});

function generateCorrelationId(): string {
  return crypto.randomUUID();
}

/**
 * Reads a cookie value by name from document.cookie.
 * Used to read the XSRF-TOKEN cookie (which is NOT HttpOnly).
 */
function getCookie(name: string): string | null {
  const match = document.cookie.match(new RegExp(`(?:^|; )${name}=([^;]*)`));
  return match ? decodeURIComponent(match[1]!) : null;
}

// Request interceptor: attach CSRF token and Correlation ID
apiClient.interceptors.request.use((config) => {
  // Attach CSRF token for state-changing requests
  const csrfToken = getCookie('XSRF-TOKEN');
  if (csrfToken && config.method && !['get', 'head', 'options'].includes(config.method.toLowerCase())) {
    config.headers['X-XSRF-TOKEN'] = csrfToken;
  }
  config.headers['X-Correlation-Id'] = generateCorrelationId();
  return config;
});

// Response interceptor: handle 401 with cookie-based refresh
let isRefreshing = false;
let failedQueue: Array<{
  resolve: () => void;
  reject: (error: unknown) => void;
}> = [];

function processQueue(error: unknown) {
  failedQueue.forEach((pending) => {
    if (error) {
      pending.reject(error);
    } else {
      pending.resolve();
    }
  });
  failedQueue = [];
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // --- 401 Token Refresh Logic (cookie-based) ---
    if (error.response?.status === 401 && !originalRequest._retry) {
      // Don't try to refresh if the failing request was the refresh itself
      if (originalRequest.url?.includes('/auth/refresh')) {
        // Only redirect if not explicitly skipping auth redirect
        if (!originalRequest._skipAuthRedirect) {
          window.location.href = '/login';
        }
        return Promise.reject(toApiError(error));
      }

      // Skip the redirect-on-failure logic for auth state checks (e.g., /auth/me on mount)
      if (originalRequest._skipAuthRedirect) {
        return Promise.reject(toApiError(error));
      }

      if (isRefreshing) {
        // Queue this request until the refresh completes
        return new Promise<void>((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        }).then(() => {
          // Retry with updated cookies (browser handles cookie automatically)
          return apiClient(originalRequest);
        });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        // POST /auth/refresh — server reads refresh_token cookie and sets new cookies
        await apiClient.post('/auth/refresh');

        processQueue(null);
        return apiClient(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError);
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
