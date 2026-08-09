import { renderHook } from '@testing-library/react';
import { useMutationError } from './useMutationError';
import { ApiError } from '@/services/ApiError';

describe('useMutationError', () => {
  it('returns null apiError when error is null', () => {
    const { result } = renderHook(() => useMutationError(null));

    expect(result.current.apiError).toBeNull();
    expect(result.current.isValidationError).toBe(false);
    expect(result.current.message).toBe('');
    expect(result.current.correlationId).toBeUndefined();
  });

  it('extracts ApiError from mutation error', () => {
    const apiError = new ApiError({
      status: 422,
      title: 'Validation Error',
      detail: 'ISBN is invalid',
      errors: { isbn: ['Invalid ISBN-13 format'] },
      correlationId: 'corr-456',
    });

    const { result } = renderHook(() => useMutationError(apiError));

    expect(result.current.apiError).toBe(apiError);
    expect(result.current.isValidationError).toBe(true);
    expect(result.current.message).toBe('ISBN is invalid');
    expect(result.current.correlationId).toBe('corr-456');
  });

  it('returns null apiError for non-ApiError Error instances', () => {
    const plainError = new Error('Network failure');

    const { result } = renderHook(() => useMutationError(plainError));

    expect(result.current.apiError).toBeNull();
    expect(result.current.isValidationError).toBe(false);
    expect(result.current.message).toBe('Network failure');
  });

  it('fieldError returns first error for a field', () => {
    const apiError = new ApiError({
      status: 422,
      title: 'Validation Error',
      errors: { title: ['Required', 'Too short'], author: ['Required'] },
    });

    const { result } = renderHook(() => useMutationError(apiError));

    expect(result.current.fieldError('title')).toBe('Required');
    expect(result.current.fieldError('author')).toBe('Required');
    expect(result.current.fieldError('isbn')).toBeUndefined();
  });

  it('fieldErrors returns all errors for a field', () => {
    const apiError = new ApiError({
      status: 422,
      title: 'Validation Error',
      errors: { title: ['Required', 'Too short'] },
    });

    const { result } = renderHook(() => useMutationError(apiError));

    expect(result.current.fieldErrors('title')).toEqual(['Required', 'Too short']);
    expect(result.current.fieldErrors('unknown')).toEqual([]);
  });

  it('hasFieldError returns correct boolean', () => {
    const apiError = new ApiError({
      status: 422,
      title: 'Validation Error',
      errors: { isbn: ['Invalid'] },
    });

    const { result } = renderHook(() => useMutationError(apiError));

    expect(result.current.hasFieldError('isbn')).toBe(true);
    expect(result.current.hasFieldError('title')).toBe(false);
  });

  it('updates when error prop changes', () => {
    const error1 = new ApiError({ status: 404, title: 'Not Found', detail: 'Book not found' });
    const error2 = new ApiError({ status: 409, title: 'Conflict', detail: 'Already checked out' });

    const { result, rerender } = renderHook(({ err }) => useMutationError(err), {
      initialProps: { err: error1 as Error | null },
    });

    expect(result.current.message).toBe('Book not found');

    rerender({ err: error2 });

    expect(result.current.message).toBe('Already checked out');
  });

  it('handles transition from error to null', () => {
    const error = new ApiError({ status: 500, title: 'Error', detail: 'Server crashed' });

    const { result, rerender } = renderHook(({ err }) => useMutationError(err), {
      initialProps: { err: error as Error | null },
    });

    expect(result.current.apiError).not.toBeNull();

    rerender({ err: null });

    expect(result.current.apiError).toBeNull();
    expect(result.current.message).toBe('');
  });
});
