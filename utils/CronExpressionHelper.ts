 * @fileoverview Helper utilities for cron expression evaluation
import { ValidationResult, ValidationErrorInfo, ValidationErrorType } from '@memberjunction/core';
 * Utility class for cron expression parsing and evaluation
export class CronExpressionHelper {
     * Determine if a cron expression is currently due
     * @param cronExpression - Cron expression string
     * @param timezone - IANA timezone (e.g., 'America/Chicago')
     * @param evalTime - Time to evaluate against
     * @returns True if the expression is due at evalTime
    public static IsExpressionDue(
        cronExpression: string,
        timezone: string,
        evalTime: Date
                currentDate: evalTime,
                tz: timezone
            const interval = cronParser.parseExpression(cronExpression, options);
            // Job is due if next execution time is before or at current eval time
            console.error('Error evaluating cron expression:', error);
     * Get the next execution time for a cron expression
     * @param timezone - IANA timezone
     * @param fromDate - Optional date to calculate from (defaults to now)
     * @returns Next execution date
    public static GetNextRunTime(
        fromDate?: Date
                currentDate: fromDate || new Date(),
            return interval.next().toDate();
            throw new Error(`Failed to calculate next run time: ${error instanceof Error ? error.message : 'Unknown error'}`);
     * Validate a cron expression
     * @param cronExpression - Cron expression to validate
    public static ValidateExpression(cronExpression: string): ValidationResult {
                'CronExpression',
                'Cron expression is required',
                cronExpression,
            const errorMessage = error instanceof Error ? error.message : 'Invalid cron expression';
