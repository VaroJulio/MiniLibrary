import { useState, useCallback } from 'react';
import { ApiError, isApiError } from '@/services/ApiError';

/**
 * State shape returned by the useApiError hook.
 */
export interface ApiErrorState {
  /** The current ApiError, or null if no error. */
  error: ApiError | null;

  /** True if a validation error (422) with field errors is present. */
  hasValidationErrors: boolean;

  /**
   * Get all error messages for a specific form field.
   * Returns an empty array if no errors for that field.
   */
  getFieldErrors: (field: string) => string[];

  /**
   * Get the first error message for a specific field, or undefined.
   * Convenient for displaying a single error below a form input.
   */
  getFieldError: (field: string) => string | undefined;

  /**
   * Check if a specific field has validation errors.
   */
  hasFieldError: (field: string) => boolean;

  /**
   * Set an error from a catch block. Accepts unknown and extracts ApiError if possible.
   * Non-ApiError values are wrapped in a generic ApiError.
   */
  setError: (error: unknown) => void;

  /** Clear the current error state. */
  clearError: () => void;

  /** The general error message (detail or title), suitable for a toast/snackbar. */
  message: string;
}

/**
 * Hook for managing API errors in components.
 *
 * Usage:
 * ```tsx
 * const { error, setError, clearError, getFieldError, message } = useApiError();
 *
 * const handleSubmit = async () => {
 *   clearError();
 *   try {
 *     await createBook(data);
 *   } catch (e) {
 *     setError(e);
 *   }
 * };
 *
 * // In JSX:
 * {error && <Alert severity="error">{message}</Alert>}
 * <TextField error={hasFieldError('isbn')} helperText={getFieldError('isbn')} />
 * ```
 */
export function useApiError(): ApiErrorState {
  const [error, setErrorState] = useState<ApiError | null>(null);

  const setError = useCallback((err: unknown) => {
    if (isApiError(err)) {
      setErrorState(err);
    } else if (err instanceof Error) {
      setErrorState(
        new ApiError({
          status: 500,
          title: 'Unexpected Error',
          detail: err.message,
        }),
      );
    } else {
      setErrorState(
        new ApiError({
          status: 500,
          title: 'Unexpected Error',
          detail: 'An unexpected error occurred.',
        }),
      );
    }
  }, []);

  const clearError = useCallback(() => {
    setErrorState(null);
  }, []);

  const getFieldErrors = useCallback(
    (field: string): string[] => {
      return error?.getFieldErrors(field) ?? [];
    },
    [error],
  );

  const getFieldError = useCallback(
    (field: string): string | undefined => {
      return error?.getFieldError(field);
    },
    [error],
  );

  const hasFieldError = useCallback(
    (field: string): boolean => {
      return (error?.getFieldErrors(field) ?? []).length > 0;
    },
    [error],
  );

  return {
    error,
    hasValidationErrors: error?.isValidationError ?? false,
    getFieldErrors,
    getFieldError,
    hasFieldError,
    setError,
    clearError,
    message: error?.detail || error?.title || '',
  };
}
