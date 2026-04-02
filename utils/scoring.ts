 * @fileoverview Scoring utilities for test evaluation
import { OracleResult, ScoringWeights } from '../types';
 * Calculate weighted score from oracle results.
 * @param weights - Scoring weights by oracle type
 * @returns Weighted score from 0.0 to 1.0
export function calculateWeightedScore(
        // Simple average if no weights provided
 * Calculate simple average score from oracle results.
 * @returns Average score from 0.0 to 1.0
export function calculateAverageScore(oracleResults: OracleResult[]): number {
 * Calculate pass rate from oracle results.
 * @returns Pass rate from 0.0 to 1.0
export function calculatePassRate(oracleResults: OracleResult[]): number {
    const passedCount = oracleResults.filter(r => r.passed).length;
    return passedCount / oracleResults.length;
export function determineTestStatus(oracleResults: OracleResult[]): 'Passed' | 'Failed' {
 * Group oracle results by type.
 * @returns Map of oracle type to results
export function groupResultsByType(
    oracleResults: OracleResult[]
): Map<string, OracleResult[]> {
    const grouped = new Map<string, OracleResult[]>();
        const existing = grouped.get(result.oracleType) || [];
        existing.push(result);
        grouped.set(result.oracleType, existing);
 * Calculate score distribution statistics.
 * @returns Score statistics
export function calculateScoreStatistics(oracleResults: OracleResult[]): {
    mean: number;
    median: number;
    stdDev: number;
        return { min: 0, max: 0, mean: 0, median: 0, stdDev: 0 };
    const scores = oracleResults.map(r => r.score).sort((a, b) => a - b);
    const min = scores[0];
    const max = scores[scores.length - 1];
    const mean = scores.reduce((sum, s) => sum + s, 0) / scores.length;
    const median = scores.length % 2 === 0
        ? (scores[scores.length / 2 - 1] + scores[scores.length / 2]) / 2
        : scores[Math.floor(scores.length / 2)];
    const variance = scores.reduce((sum, s) => sum + Math.pow(s - mean, 2), 0) / scores.length;
    const stdDev = Math.sqrt(variance);
    return { min, max, mean, median, stdDev };
 * Normalize scores to 0-1 range.
 * @returns Normalized results
export function normalizeScores(oracleResults: OracleResult[]): OracleResult[] {
    const scores = oracleResults.map(r => r.score);
    const min = Math.min(...scores);
    const max = Math.max(...scores);
    const range = max - min;
    if (range === 0) {
        // All scores are the same
        return oracleResults.map(r => ({ ...r, score: 1.0 }));
    return oracleResults.map(r => ({
        score: (r.score - min) / range
