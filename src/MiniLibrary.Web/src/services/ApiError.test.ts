import { ApiError, isApiError, type ProblemDetailsResponse } from './ApiError';

describe('ApiError', () => {
  describe('constructor', () => {
    it('parses a full ProblemDetails response', () => {
      const pd: ProblemDetailsResponse = {
        type: 'https://tools.ietf.org/html/rfc7807',
        title: 'Validation Error',
        status: 422,
        detail: 'One or more validation errors occurred.',
        instance: '/api/books',
        errors: { isbn: ['Invalid ISBN-13 format'], title: ['Title is required'] },
        correlationId: 'abc-123',
      };

      const error = new ApiError(pd);

      expect(error).toBeInstanceOf(Error);
      expect(error).toBeInstanceOf(ApiError);
      expect(error.name).toBe('ApiError');
      expect(error.status).toBe(422);
      expect(error.title).toBe('Validation Error');
      expect(error.detail).toBe('One or more validation errors occurred.');
      expect(error.errors).toEqual({ isbn: ['Invalid ISBN-13 format'], title: ['Title is required'] });
      expect(error.correlationId).toBe('abc-123');
      expect(error.message).toBe('One or more validation errors occurred.');
    });

    it('uses title as message when detail is missing', () => {
      const error = new ApiError({ title: 'Not Found', status: 404 });

      expect(error.message).toBe('Not Found');
      expect(error.detail).toBe('');
    });

    it('defaults to status 500 when not provided', () => {
      const error = new ApiError({ title: 'Error' });

      expect(error.status).toBe(500);
    });

    it('defaults to "Error" title when not provided', () => {
      const error = new ApiError({ status: 500 });

      expect(error.title).toBe('Error');
    });

    it('handles completely empty ProblemDetails', () => {
      const error = new ApiError({});

      expect(error.status).toBe(500);
      expect(error.title).toBe('Error');
      expect(error.detail).toBe('');
      expect(error.errors).toBeUndefined();
      expect(error.correlationId).toBeUndefined();
      expect(error.message).toBe('An error occurred');
    });
  });

  describe('convenience getters', () => {
    it('isValidationError returns true for 422 with errors', () => {
      const error = new ApiError({
        status: 422,
        title: 'Validation Error',
        errors: { field: ['error'] },
      });

      expect(error.isValidationError).toBe(true);
    });

    it('isValidationError returns false for 422 without errors', () => {
      const error = new ApiError({ status: 422, title: 'Validation Error' });

      expect(error.isValidationError).toBe(false);
    });

    it('isValidationError returns false for non-422 status', () => {
      const error = new ApiError({
        status: 400,
        title: 'Bad Request',
        errors: { field: ['error'] },
      });

      expect(error.isValidationError).toBe(false);
    });

    it('isNotFound returns true for 404', () => {
      expect(new ApiError({ status: 404, title: 'Not Found' }).isNotFound).toBe(true);
    });

    it('isNotFound returns false for non-404', () => {
      expect(new ApiError({ status: 400, title: 'Bad' }).isNotFound).toBe(false);
    });

    it('isConflict returns true for 409', () => {
      expect(new ApiError({ status: 409, title: 'Conflict' }).isConflict).toBe(true);
    });

    it('isForbidden returns true for 403', () => {
      expect(new ApiError({ status: 403, title: 'Forbidden' }).isForbidden).toBe(true);
    });
  });

  describe('field error helpers', () => {
    const error = new ApiError({
      status: 422,
      title: 'Validation Error',
      errors: {
        isbn: ['Invalid ISBN-13 format', 'ISBN already exists'],
        title: ['Title is required'],
      },
    });

    it('getFieldErrors returns all errors for a field', () => {
      expect(error.getFieldErrors('isbn')).toEqual(['Invalid ISBN-13 format', 'ISBN already exists']);
    });

    it('getFieldErrors returns empty array for unknown field', () => {
      expect(error.getFieldErrors('unknown')).toEqual([]);
    });

    it('getFieldError returns first error for a field', () => {
      expect(error.getFieldError('isbn')).toBe('Invalid ISBN-13 format');
    });

    it('getFieldError returns undefined for unknown field', () => {
      expect(error.getFieldError('unknown')).toBeUndefined();
    });

    it('getFieldErrors returns empty when no errors object', () => {
      const noErrors = new ApiError({ status: 404, title: 'Not Found' });
      expect(noErrors.getFieldErrors('any')).toEqual([]);
    });
  });
});

describe('isApiError', () => {
  it('returns true for ApiError instances', () => {
    const error = new ApiError({ status: 404, title: 'Not Found' });
    expect(isApiError(error)).toBe(true);
  });

  it('returns false for plain Error', () => {
    expect(isApiError(new Error('test'))).toBe(false);
  });

  it('returns false for null', () => {
    expect(isApiError(null)).toBe(false);
  });

  it('returns false for undefined', () => {
    expect(isApiError(undefined)).toBe(false);
  });

  it('returns false for plain object with similar shape', () => {
    const fake = { status: 404, title: 'Not Found', message: 'test', name: 'ApiError' };
    expect(isApiError(fake)).toBe(false);
  });
});
