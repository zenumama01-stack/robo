 * @fileoverview Result formatting utilities for test output
import { TestRunResult, TestSuiteRunResult, OracleResult } from '../types';
import { formatCost } from './cost-calculator';
 * Format test run result as human-readable text.
 * @param result - Test run result
 * @returns Formatted text output
export function formatTestRunResult(result: TestRunResult): string {
    lines.push('='.repeat(80));
    lines.push(`Test: ${result.testName}`);
    lines.push(`Status: ${result.status}`);
    lines.push(`Score: ${(result.score * 100).toFixed(1)}%`);
    lines.push(`Checks: ${result.passedChecks}/${result.totalChecks} passed`);
    lines.push(`Duration: ${formatDuration(result.durationMs)}`);
    lines.push(`Cost: ${formatCost(result.totalCost)}`);
        lines.push('\nOracle Results:');
            lines.push(formatOracleResult(oracle, '  '));
 * Format test suite run result as human-readable text.
 * @param result - Test suite run result
export function formatSuiteRunResult(result: TestSuiteRunResult): string {
    lines.push(`Test Suite: ${result.suiteName}`);
    lines.push(`Tests: ${result.passedTests}/${result.totalTests} passed`);
    lines.push(`Average Score: ${(result.averageScore * 100).toFixed(1)}%`);
    if (result.testResults.length > 0) {
        lines.push('\nTest Results:');
        for (const test of result.testResults) {
            lines.push(formatTestSummary(test, '  '));
 * Format oracle result as human-readable text.
 * @param result - Oracle result
 * @param indent - Indentation prefix
export function formatOracleResult(result: OracleResult, indent: string = ''): string {
    lines.push(`${indent}${status} ${result.oracleType}: ${result.message}`);
    lines.push(`${indent}  Score: ${(result.score * 100).toFixed(1)}%`);
    if (result.details) {
        lines.push(`${indent}  Details: ${JSON.stringify(result.details, null, 2)}`);
 * Format test summary (for suite results).
export function formatTestSummary(result: TestRunResult, indent: string = ''): string {
    const status = result.status === 'Passed' ? '✓' : '✗';
    return `${indent}${status} ${result.testName}: ${(result.score * 100).toFixed(1)}% (${result.passedChecks}/${result.totalChecks})`;
 * Format test run result as JSON.
 * @param pretty - Whether to pretty-print (default: true)
export function formatTestRunResultAsJSON(
    result: TestRunResult,
    pretty: boolean = true
    return JSON.stringify(result, null, pretty ? 2 : 0);
 * Format test suite run result as JSON.
export function formatSuiteRunResultAsJSON(
    result: TestSuiteRunResult,
 * Format test run result as markdown.
 * @returns Markdown output
export function formatTestRunResultAsMarkdown(result: TestRunResult): string {
    lines.push(`# Test: ${result.testName}\n`);
    lines.push(`**Status:** ${result.status === 'Passed' ? '✅ Passed' : '❌ Failed'}`);
    lines.push(`**Score:** ${(result.score * 100).toFixed(1)}%`);
    lines.push(`**Checks:** ${result.passedChecks}/${result.totalChecks} passed`);
    lines.push(`**Duration:** ${formatDuration(result.durationMs)}`);
    lines.push(`**Cost:** ${formatCost(result.totalCost)}\n`);
        lines.push('## Oracle Results\n');
        lines.push('| Oracle | Status | Score | Message |');
        lines.push('|--------|--------|-------|---------|');
            const status = oracle.passed ? '✅' : '❌';
            const score = `${(oracle.score * 100).toFixed(1)}%`;
            lines.push(`| ${oracle.oracleType} | ${status} | ${score} | ${oracle.message} |`);
 * Format test suite run result as markdown.
export function formatSuiteRunResultAsMarkdown(result: TestSuiteRunResult): string {
    lines.push(`# Test Suite: ${result.suiteName}\n`);
    lines.push(`**Status:** ${result.status === 'Completed' ? '✅ Completed' : `❌ ${result.status}`}`);
    lines.push(`**Tests:** ${result.passedTests}/${result.totalTests} passed`);
    lines.push(`**Average Score:** ${(result.averageScore * 100).toFixed(1)}%`);
        lines.push('| Test | Status | Score | Checks |');
        lines.push('|------|--------|-------|--------|');
            const status = test.status === 'Passed' ? '✅' : '❌';
            const score = `${(test.score * 100).toFixed(1)}%`;
            const checks = `${test.passedChecks}/${test.totalChecks}`;
            lines.push(`| ${test.testName} | ${status} | ${score} | ${checks} |`);
 * Format test run result as CSV.
 * @param results - Array of test run results
 * @param includeHeaders - Whether to include CSV headers (default: true)
 * @returns CSV output
export function formatTestRunResultsAsCSV(
    results: TestRunResult[],
    includeHeaders: boolean = true
        lines.push('TestName,Status,Score,PassedChecks,TotalChecks,DurationMs,TotalCost');
        lines.push([
            escapeCSV(result.testName),
            result.status,
            result.score.toFixed(4),
            result.passedChecks,
            result.totalChecks,
            result.durationMs,
            result.totalCost.toFixed(6)
        ].join(','));
 * Format duration in milliseconds as human-readable string.
 * @param ms - Duration in milliseconds
 * @returns Formatted duration string
export function formatDuration(ms: number): string {
    if (ms < 60000) {
 * Escape CSV field value.
 * @param value - Field value
 * @returns Escaped value
function escapeCSV(value: string): string {
 * Generate summary statistics from multiple test results.
 * @returns Summary statistics
export function generateSummaryStatistics(results: TestRunResult[]): {
    totalDuration: number;
    const totalTests = results.length;
    const passedTests = results.filter(r => r.status === 'Passed').length;
    const failedTests = totalTests - passedTests;
    const passRate = totalTests > 0 ? passedTests / totalTests : 0;
    const totalScore = results.reduce((sum, r) => sum + r.score, 0);
    const averageScore = totalTests > 0 ? totalScore / totalTests : 0;
    const totalDuration = results.reduce((sum, r) => sum + r.durationMs, 0);
    const totalCost = results.reduce((sum, r) => sum + r.totalCost, 0);
    const avgDuration = totalTests > 0 ? totalDuration / totalTests : 0;
    const avgCost = totalTests > 0 ? totalCost / totalTests : 0;
        passRate,
        totalDuration,
        avgCost
