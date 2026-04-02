 * @fileoverview General-purpose validation types for use across MemberJunction
 * These types provide a standard way to represent validation results and errors
 * throughout the framework, independent of any specific validation implementation.
 * @since 2.68.0
 * Enumeration of validation error types
export const ValidationErrorType = {
    Failure: 'Failure',
export type ValidationErrorType = typeof ValidationErrorType[keyof typeof ValidationErrorType];
 * Information about a single validation error
export class ValidationErrorInfo {
    Source: string;
    Value: any;
    Type: ValidationErrorType;
    constructor(Source: string, Message: string, Value: any, Type: ValidationErrorType = ValidationErrorType.Failure) {
        this.Source = Source;
        this.Message = Message;
        this.Type = Type;
 * Result of a validation operation
export class ValidationResult {
    Errors: ValidationErrorInfo[] = [];
