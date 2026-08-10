/**
 * Represents a structured API error based on RFC 7807 ProblemDetails.
 * Thrown by the Axios response interceptor when the backend returns an error response
 * with a ProblemDetails body (title, status, detail, errors, correlationId).
 */
export class ApiError extends Error {
  /** HTTP status code (e.g., 422, 404, 409, 403, 500). */
  readonly status: number;

  /** Short error category from ProblemDetails (e.g., "Validation Error", "Not Found"). */
  readonly title: string;

  /** Human-readable explanation of the error (from ProblemDetails.detail). */
  readonly detail: string;

  /**
   * Field-level validation errors (only present for 422 Validation Error).
   * Keys are field names (camelCase), values are arrays of error messages.
   * Example: { "isbn": ["Invalid ISBN-13 format"], "title": ["Title is required"] }
   */
  readonly errors?: Record<string, string[]>;

  /** Correlation ID for tracing the request in backend logs. */
  readonly correlationId?: string;

  constructor(problemDetails: ProblemDetailsResponse) {
    super(problemDetails.detail ?? problemDetails.title ?? 'An error occurred');
    this.name = 'ApiError';
    this.status = problemDetails.status ?? 500;
    this.title = problemDetails.title ?? 'Error';
    this.detail = problemDetails.detail ?? '';
    this.errors = problemDetails.errors;
    this.correlationId = problemDetails.correlationId;

    // Maintain proper prototype chain for instanceof checks
    Object.setPrototypeOf(this, ApiError.prototype);
  }

  /** Returns true if this error contains field-level validation errors. */
  get isValidationError(): boolean {
    return this.status === 422 && !!this.errors && Object.keys(this.errors).length > 0;
  }

  /** Returns true if the resource was not found. */
  get isNotFound(): boolean {
    return this.status === 404;
  }

  /** Returns true if there was a conflict (e.g., concurrent modification). */
  get isConflict(): boolean {
    return this.status === 409;
  }

  /** Returns true if the user lacks permission. */
  get isForbidden(): boolean {
    return this.status === 403;
  }

  /**
   * Gets the error messages for a specific field.
   * Returns an empty array if no errors exist for that field.
   */
  getFieldErrors(field: string): string[] {
    return this.errors?.[field] ?? [];
  }

  /**
   * Gets the first error message for a specific field, or undefined if none.
   */
  getFieldError(field: string): string | undefined {
    return this.errors?.[field]?.[0];
  }
}

/**
 * Shape of the RFC 7807 ProblemDetails response from the backend.
 */
export interface ProblemDetailsResponse {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
  correlationId?: string;
}

/**
 * Type guard to check if an unknown error is an ApiError instance.
 */
export function isApiError(error: unknown): error is ApiError {
  return error instanceof ApiError;
}
