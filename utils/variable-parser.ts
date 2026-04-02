 * @fileoverview Variable parsing utilities for CLI
import { TestVariableValue } from '@memberjunction/testing-engine-base';
 * Parse --var flags into a variables object.
 * @param varFlags - Array of var flags in "name=value" format
 * @param testTypeVariablesSchema - Optional type schema for type conversion
 * @returns Parsed variables object
export function parseVariableFlags(
    varFlags: string[] | undefined,
    testTypeVariablesSchema?: string | null
): Record<string, TestVariableValue> | undefined {
    if (!varFlags || varFlags.length === 0) {
    return resolver.parseCliVariables(varFlags, testTypeVariablesSchema);
 * Get the variables schema for a test (from its test type).
 * @param engine - TestEngine instance
 * @param testId - Test ID
 * @returns Variables schema JSON string or null
export function getTestVariablesSchema(
    engine: TestEngine,
    return testType.VariablesSchema;
