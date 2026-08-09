import { useMemo } from 'react';
import { ApiError, isApiError } from '@/services/ApiError';

/**
 * Extracts typed ApiError information from a TanStack Query mutation error.
 * Use this alongside useMutation to get field-level errors for form display.
 *
 * Usage:
 * ```tsx
 * const createBook = useCreateBook();
 * const { apiError, fieldError, hasFieldError, message } = useMutationError(createBook.error);
 *
 * <TextField
 *   error={hasFieldError('isbn')}
 *   helperText={fieldError('isbn')}
 * />
 * {apiError && <Alert severity="error">{message}</Alert>}
 * ```
 */
export function useMutationError(error: Error | null) {
  return useMemo(() => {
    const apiError: ApiError | null = error && isApiError(error) ? error : null;

    const fieldError = (field: string): string | undefined => {
      return apiError?.getFieldError(field);
    };

    const fieldErrors = (field: string): string[] => {
      return apiError?.getFieldErrors(field) ?? [];
    };

    const hasFieldError = (field: string): boolean => {
      return (apiError?.getFieldErrors(field) ?? []).length > 0;
    };

    return {
      /** The ApiError instance, or null if error is not an ApiError. */
      apiError,
      /** True if the error is a 422 validation error with field details. */
      isValidationError: apiError?.isValidationError ?? false,
      /** Get first error message for a field. */
      fieldError,
      /** Get all error messages for a field. */
      fieldErrors,
      /** Check if a field has errors. */
      hasFieldError,
      /** General error message suitable for display (detail or title). */
      message: apiError?.detail || apiError?.title || error?.message || '',
      /** Correlation ID for backend log lookup. */
      correlationId: apiError?.correlationId,
    };
  }, [error]);
}
