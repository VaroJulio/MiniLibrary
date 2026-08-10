import { renderHook, act } from '@testing-library/react';
import { useApiError } from './useApiError';
import { ApiError } from '@/services/ApiError';

describe('useApiError', () => {
  it('starts with no error', () => {
    const { result } = renderHook(() => useApiError());

    expect(result.current.error).toBeNull();
    expect(result.current.hasValidationErrors).toBe(false);
    expect(result.current.message).toBe('');
  });

  it('setError stores an ApiError', () => {
    const { result } = renderHook(() => useApiError());
    const apiError = new ApiError({
      status: 422,
      title: 'Validation Error',
      detail: 'Field errors present',
      errors: { isbn: ['Invalid'] },
    });

    act(() => {
      result.current.setError(apiError);
    });

    expect(result.current.error).toBe(apiError);
    expect(result.current.hasValidationErrors).toBe(true);
    expect(result.current.message).toBe('Field errors present');
  });

  it('setError wraps a plain Error into ApiError', () => {
    const { result } = renderHook(() => useApiError());

    act(() => {
      result.current.setError(new Error('Something broke'));
    });

    expect(result.current.error).toBeInstanceOf(ApiError);
    expect(result.current.error?.status).toBe(500);
    expect(result.current.message).toBe('Something broke');
  });

  it('setError wraps unknown values into ApiError', () => {
    const { result } = renderHook(() => useApiError());

    act(() => {
      result.current.setError('string error');
    });

    expect(result.current.error).toBeInstanceOf(ApiError);
    expect(result.current.error?.status).toBe(500);
    expect(result.current.message).toBe('An unexpected error occurred.');
  });

  it('clearError resets state', () => {
    const { result } = renderHook(() => useApiError());

    act(() => {
      result.current.setError(new ApiError({ status: 404, title: 'Not Found', detail: 'Book not found' }));
    });

    expect(result.current.error).not.toBeNull();

    act(() => {
      result.current.clearError();
    });

    expect(result.current.error).toBeNull();
    expect(result.current.message).toBe('');
  });

  describe('field error helpers', () => {
    it('getFieldErrors returns errors for a field', () => {
      const { result } = renderHook(() => useApiError());

      act(() => {
        result.current.setError(
          new ApiError({
            status: 422,
            title: 'Validation Error',
            errors: { title: ['Required', 'Too short'] },
          }),
        );
      });

      expect(result.current.getFieldErrors('title')).toEqual(['Required', 'Too short']);
    });

    it('getFieldError returns first error for a field', () => {
      const { result } = renderHook(() => useApiError());

      act(() => {
        result.current.setError(
          new ApiError({
            status: 422,
            title: 'Validation Error',
            errors: { isbn: ['Invalid format', 'Duplicate'] },
          }),
        );
      });

      expect(result.current.getFieldError('isbn')).toBe('Invalid format');
    });

    it('hasFieldError returns true when field has errors', () => {
      const { result } = renderHook(() => useApiError());

      act(() => {
        result.current.setError(
          new ApiError({
            status: 422,
            title: 'Validation Error',
            errors: { author: ['Required'] },
          }),
        );
      });

      expect(result.current.hasFieldError('author')).toBe(true);
      expect(result.current.hasFieldError('title')).toBe(false);
    });

    it('field helpers return empty/false when no error', () => {
      const { result } = renderHook(() => useApiError());

      expect(result.current.getFieldErrors('any')).toEqual([]);
      expect(result.current.getFieldError('any')).toBeUndefined();
      expect(result.current.hasFieldError('any')).toBe(false);
    });
  });
});
