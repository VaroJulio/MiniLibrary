/**
 * Tests for apiClient Axios interceptor behavior.
 * Uses a mock Axios adapter to simulate server responses without real HTTP calls.
 */
import axios from 'axios';
import { vi, beforeEach, afterEach } from 'vitest';
import { ApiError } from './ApiError';

// We need to test the interceptor logic by creating a fresh axios instance
// with the same interceptor logic applied. This avoids importing the actual
// apiClient which depends on browser APIs (localStorage, crypto, window.location).

function createTestClient() {
  const client = axios.create({ baseURL: '/api' });

  // Replicate the toApiError transform from apiClient.ts
  function toApiError(error: unknown): ApiError {
    if (error instanceof ApiError) return error;

    if (axios.isAxiosError(error) && error.response) {
      const { status, data } = error.response;
      if (data && typeof data.title === 'string') {
        return new ApiError({ ...data, status: data.status ?? status });
      }
      return new ApiError({
        status,
        title: error.response.statusText || 'Error',
        detail: typeof data === 'string' ? data : error.message,
      });
    }

    if (axios.isAxiosError(error) && !error.response) {
      return new ApiError({
        status: 0,
        title: 'Network Error',
        detail: 'Unable to connect to the server. Please check your connection.',
      });
    }

    return new ApiError({
      status: 500,
      title: 'Unexpected Error',
      detail: error instanceof Error ? error.message : 'An unexpected error occurred.',
    });
  }

  // Apply the error transform interceptor (simplified — no 401 refresh for these tests)
  client.interceptors.response.use(
    (response) => response,
    (error) => Promise.reject(toApiError(error)),
  );

  return client;
}

describe('apiClient interceptor — error transformation', () => {
  let client: ReturnType<typeof createTestClient>;
  let mockAdapter: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    client = createTestClient();
    mockAdapter = vi.fn();
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    client.defaults.adapter = mockAdapter as any;
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('transforms 422 ProblemDetails into ApiError with field errors', async () => {
    mockAdapter.mockRejectedValueOnce(
      createAxiosError(422, {
        type: 'https://tools.ietf.org/html/rfc7807',
        title: 'Validation Error',
        status: 422,
        detail: 'One or more validation errors occurred.',
        errors: { isbn: ['Invalid ISBN-13 format'] },
        correlationId: 'test-corr-id',
      }),
    );

    try {
      await client.post('/books', {});
      expect.fail('Should have thrown');
    } catch (e) {
      expect(e).toBeInstanceOf(ApiError);
      const err = e as ApiError;
      expect(err.status).toBe(422);
      expect(err.title).toBe('Validation Error');
      expect(err.detail).toBe('One or more validation errors occurred.');
      expect(err.errors).toEqual({ isbn: ['Invalid ISBN-13 format'] });
      expect(err.correlationId).toBe('test-corr-id');
      expect(err.isValidationError).toBe(true);
    }
  });

  it('transforms 404 ProblemDetails into ApiError', async () => {
    mockAdapter.mockRejectedValueOnce(
      createAxiosError(404, {
        title: 'Not Found',
        status: 404,
        detail: 'Book with id xyz not found.',
      }),
    );

    try {
      await client.get('/books/xyz');
      expect.fail('Should have thrown');
    } catch (e) {
      expect(e).toBeInstanceOf(ApiError);
      const err = e as ApiError;
      expect(err.status).toBe(404);
      expect(err.isNotFound).toBe(true);
      expect(err.detail).toBe('Book with id xyz not found.');
    }
  });

  it('transforms 409 Conflict into ApiError', async () => {
    mockAdapter.mockRejectedValueOnce(
      createAxiosError(409, {
        title: 'Conflict',
        status: 409,
        detail: 'Book was checked out by another user.',
      }),
    );

    try {
      await client.post('/loans/checkout', {});
      expect.fail('Should have thrown');
    } catch (e) {
      expect(e).toBeInstanceOf(ApiError);
      const err = e as ApiError;
      expect(err.isConflict).toBe(true);
    }
  });

  it('transforms 403 Forbidden into ApiError', async () => {
    mockAdapter.mockRejectedValueOnce(
      createAxiosError(403, {
        title: 'Forbidden',
        status: 403,
        detail: 'Admin access required.',
      }),
    );

    try {
      await client.get('/users');
      expect.fail('Should have thrown');
    } catch (e) {
      expect(e).toBeInstanceOf(ApiError);
      const err = e as ApiError;
      expect(err.isForbidden).toBe(true);
    }
  });

  it('transforms non-ProblemDetails error response into ApiError', async () => {
    mockAdapter.mockRejectedValueOnce(
      createAxiosError(500, 'Internal Server Error'),
    );

    try {
      await client.get('/health');
      expect.fail('Should have thrown');
    } catch (e) {
      expect(e).toBeInstanceOf(ApiError);
      const err = e as ApiError;
      expect(err.status).toBe(500);
      expect(err.detail).toBe('Internal Server Error');
    }
  });

  it('transforms network error (no response) into ApiError with status 0', async () => {
    const networkError = new Error('Network Error');
    Object.assign(networkError, {
      isAxiosError: true,
      response: undefined,
      config: { url: '/books' },
      code: 'ERR_NETWORK',
    });
    mockAdapter.mockRejectedValueOnce(networkError);

    try {
      await client.get('/books');
      expect.fail('Should have thrown');
    } catch (e) {
      expect(e).toBeInstanceOf(ApiError);
      const err = e as ApiError;
      expect(err.status).toBe(0);
      expect(err.title).toBe('Network Error');
    }
  });

  it('passes successful responses through unchanged', async () => {
    mockAdapter.mockResolvedValueOnce({
      status: 200,
      data: { id: '1', title: 'Clean Code' },
      headers: {},
      config: {},
      statusText: 'OK',
    });

    const response = await client.get('/books/1');
    expect(response.status).toBe(200);
    expect(response.data.title).toBe('Clean Code');
  });
});

/**
 * Helper to create an Axios-like error object for testing interceptors.
 */
function createAxiosError(status: number, data: unknown) {
  const error = new Error(`Request failed with status code ${status}`);
  Object.assign(error, {
    isAxiosError: true,
    response: {
      status,
      statusText: status === 404 ? 'Not Found' : status === 500 ? 'Internal Server Error' : 'Error',
      data,
      headers: {},
      config: {},
    },
    config: { url: '/test' },
    code: 'ERR_BAD_REQUEST',
  });
  return error;
}
