 * Error handling utilities
 * Standardized error handling functions following MJ patterns
 * Extract error message from unknown error type
 * Uses MJ's extractErrorMessage pattern
export function extractErrorMessage(error: unknown, context: string): string {
    return `${context}: ${error.message}`;
  if (typeof error === 'string') {
    return `${context}: ${error}`;
  return `${context}: Unknown error occurred`;
 * Validate required value is not null/undefined
export function requireValue<T>(value: T | null | undefined, fieldName: string): T {
    throw new Error(`Required value '${fieldName}' is missing`);
 * Get property with fallback value
export function getPropertyOrDefault<T>(
  defaultValue: T
  return value !== undefined ? (value as T) : defaultValue;
