 * @fileoverview Intelligent payload change analysis for detecting potentially unintended modifications
 * This module provides deterministic rules to identify suspicious changes that may indicate
 * LLM errors or unintended state mutations during agent execution.
import { LogStatus, LogError } from '@memberjunction/core';
import { AgentPayloadChangeRequest } from '@memberjunction/ai-core-plus';
 * Types of warnings that can be detected during payload analysis
export enum PayloadWarningType {
    ContentTruncation = 'content_truncation',
    KeyRemoval = 'key_removal',
    TypeChange = 'type_change',
    PatternAnomaly = 'pattern_anomaly',
    UnintendedOverwrite = 'unintended_overwrite'
 * Base interface for all payload warnings
export interface PayloadWarning<T = any> {
    type: PayloadWarningType;
    severity: 'low' | 'medium' | 'high' | 'critical';
    path: string;
    requiresFeedback?: boolean;
    details: T;
 * Content truncation warning details
export interface ContentTruncationWarning extends PayloadWarning<{
    originalLength: number;
    newLength: number;
    reductionPercentage: number;
    contentPreview: {
        before: string;
        after: string;
    type: PayloadWarningType.ContentTruncation;
    details: {
 * Key removal warning details
export interface KeyRemovalWarning extends PayloadWarning<{
    removedKeys: string[];
    hadContent: boolean;
    contentSize: number;
    childKeysRemoved?: string[];
    type: PayloadWarningType.KeyRemoval;
 * Configuration for the payload change analyzer
export interface PayloadAnalyzerConfig {
    /** Percentage threshold for content reduction warnings (0-100) */
    contentReductionThreshold: number;
    /** Minimum content length to trigger truncation analysis */
    minContentLengthForAnalysis: number;
    /** Percentage threshold for array length reduction warnings */
    arrayReductionThreshold: number;
    /** Percentage threshold for object key reduction warnings */
    objectKeyReductionThreshold: number;
    /** Whether to analyze nested changes */
    analyzeNestedChanges: boolean;
    /** Maximum depth for nested analysis */
    maxAnalysisDepth: number;
    /** Patterns that indicate placeholder or truncated content */
    truncationPatterns: RegExp[];
 * Result of payload change analysis
export interface PayloadAnalysisResult {
    warnings: PayloadWarning[];
    criticalWarnings: PayloadWarning[];
    requiresFeedback: boolean;
        totalWarnings: number;
        warningsByType: Record<PayloadWarningType, number>;
        suspiciousChanges: number;
 * Analyzes payload changes to detect potentially unintended modifications
export class PayloadChangeAnalyzer {
    private config: PayloadAnalyzerConfig;
    constructor(config?: Partial<PayloadAnalyzerConfig>) {
        this.config = {
            contentReductionThreshold: 70,
            minContentLengthForAnalysis: 100,
            arrayReductionThreshold: 50,
            objectKeyReductionThreshold: 50,
            analyzeNestedChanges: true,
            maxAnalysisDepth: 10,
            truncationPatterns: [
                /\.\.\.$/,
                /\[truncated\]/i,
                /\[shortened\]/i,
                /etc\.?$/i,
                /and more/i
            ...config
     * Analyzes a payload change request for suspicious patterns
    public analyzeChangeRequest<P = any>(
        originalPayload: P,
        changeRequest: AgentPayloadChangeRequest<P>,
        resultPayload: P
    ): PayloadAnalysisResult {
        const warnings: PayloadWarning[] = [];
        // Analyze all changes recursively
        this.analyzeChanges(
            originalPayload,
            resultPayload,
            changeRequest,
            [],
            warnings
        // Analyze for pattern anomalies
        this.detectPatternAnomalies(
        // Filter critical warnings
        const criticalWarnings = warnings.filter(w => w.severity === 'critical' || w.severity === 'high');
        const requiresFeedback = warnings.some(w => w.requiresFeedback);
        // Generate summary
        const warningsByType = warnings.reduce((acc, warning) => {
            acc[warning.type] = (acc[warning.type] || 0) + 1;
            return acc;
        }, {} as Record<PayloadWarningType, number>);
            warnings,
            criticalWarnings,
            requiresFeedback,
                totalWarnings: warnings.length,
                warningsByType,
                suspiciousChanges: criticalWarnings.length
     * Recursively analyze changes between original and result payloads
    private analyzeChanges(
        original: any,
        result: any,
        changeRequest: AgentPayloadChangeRequest<any>,
        path: string[],
        warnings: PayloadWarning[]
        // Skip if we've exceeded max depth
        if (path.length > this.config.maxAnalysisDepth) return;
        // Handle different types
        if (typeof original === 'string' && typeof result === 'string') {
            this.analyzeStringChange(original, result, path, warnings);
        } else if (Array.isArray(original) && Array.isArray(result)) {
            this.analyzeArrayChange(original, result, path, warnings);
        } else if (_.isObject(original) && _.isObject(result)) {
            this.analyzeObjectChange(original, result, changeRequest, path, warnings);
        } else if (original !== undefined && result === undefined) {
            this.analyzeRemoval(original, path, warnings);
        } else if (typeof original !== typeof result && original !== undefined && result !== undefined) {
            this.analyzeTypeChange(original, result, path, warnings);
     * Analyze string content changes for truncation
    private analyzeStringChange(
        original: string,
        result: string,
        if (!original || !result) return;
        const originalLength = original.length;
        const resultLength = result.length;
        // Skip if content is too short to analyze
        if (originalLength < this.config.minContentLengthForAnalysis) return;
        const reductionPercentage = ((originalLength - resultLength) / originalLength) * 100;
        // Check for significant truncation
        if (reductionPercentage > this.config.contentReductionThreshold) {
            const warning: ContentTruncationWarning = {
                type: PayloadWarningType.ContentTruncation,
                severity: reductionPercentage > 90 ? 'critical' : 'high',
                path: path.join('.'),
                message: `Content reduced by ${reductionPercentage.toFixed(1)}% (from ${originalLength} to ${resultLength} characters)`,
                requiresFeedback: true,
                    newLength: resultLength,
                    reductionPercentage,
                        before: original.substring(0, 100) + (originalLength > 100 ? '...' : ''),
                        after: result.substring(0, 100) + (resultLength > 100 ? '...' : '')
            warnings.push(warning);
        // Check for truncation patterns
        const hasTruncationPattern = this.config.truncationPatterns.some(pattern => 
            pattern.test(result) && !pattern.test(original)
        if (hasTruncationPattern) {
            warnings.push({
                type: PayloadWarningType.PatternAnomaly,
                message: 'Content appears to be truncated (detected truncation pattern)',
                    pattern: 'truncation_indicator',
                    resultEnding: result.slice(-50)
     * Analyze array changes
    private analyzeArrayChange(
        original: any[],
        result: any[],
        if (originalLength === 0) return;
        if (reductionPercentage > this.config.arrayReductionThreshold) {
                severity: reductionPercentage > 80 ? 'high' : 'medium',
                message: `Array reduced by ${reductionPercentage.toFixed(1)}% (from ${originalLength} to ${resultLength} items)`,
                    pattern: 'array_reduction',
                    resultLength,
                    reductionPercentage
        // Analyze individual array elements if configured
        if (this.config.analyzeNestedChanges) {
            const minLength = Math.min(originalLength, resultLength);
            for (let i = 0; i < minLength; i++) {
                    original[i],
                    result[i],
                    {} as AgentPayloadChangeRequest<any>,
                    [...path, `[${i}]`],
     * Analyze object changes
    private analyzeObjectChange(
        const originalKeys = Object.keys(original);
        const resultKeys = Object.keys(result);
        const removedKeys = originalKeys.filter(k => !resultKeys.includes(k));
        // Check for significant key removal
        if (removedKeys.length > 0) {
            const keyReductionPercentage = (removedKeys.length / originalKeys.length) * 100;
            if (keyReductionPercentage > this.config.objectKeyReductionThreshold) {
                const removedWithContent = removedKeys.filter(k => {
                    const value = original[k];
                    return value !== null && value !== undefined && value !== '';
                if (removedWithContent.length > 0) {
                    const warning: KeyRemovalWarning = {
                        type: PayloadWarningType.KeyRemoval,
                        severity: keyReductionPercentage > 80 ? 'critical' : 'high',
                        message: `${removedWithContent.length} non-empty keys removed (${keyReductionPercentage.toFixed(1)}% reduction)`,
                            removedKeys: removedWithContent,
                            hadContent: true,
                            contentSize: removedWithContent.reduce((size, key) => {
                                const value = original[key];
                                return size + (typeof value === 'string' ? value.length : JSON.stringify(value).length);
                            }, 0)
        // Analyze nested changes
            for (const key of resultKeys) {
                if (key in original) {
                        original[key],
                        result[key],
                        [...path, key],
     * Analyze complete removal of a value
    private analyzeRemoval(
        // Skip if the original was empty/null
        if (original === null || original === undefined || original === '') return;
        const contentSize = typeof original === 'string' 
            ? original.length 
            : JSON.stringify(original).length;
        if (contentSize > 50) { // Only warn for substantial content
                severity: contentSize > 500 ? 'high' : 'medium',
                message: `Non-empty content removed (${contentSize} characters)`,
                    removedKeys: [path[path.length - 1]],
                    contentSize
     * Analyze type changes
    private analyzeTypeChange(
        const originalType = Array.isArray(original) ? 'array' : typeof original;
        const resultType = Array.isArray(result) ? 'array' : typeof result;
        // Determine if this represents data loss
        const isDataLoss = 
            (originalType === 'object' || originalType === 'array') && 
            (resultType === 'string' || resultType === 'number' || resultType === 'boolean');
        if (isDataLoss) {
                type: PayloadWarningType.TypeChange,
                message: `Type changed from ${originalType} to ${resultType} (potential data loss)`,
                    originalType,
                    newType: resultType,
                    dataLoss: true
     * Detect pattern-based anomalies across the entire payload
    private detectPatternAnomalies(
        // Detect if result is missing expected top-level keys
        if (_.isObject(original) && _.isObject(result)) {
            const missingRatio = (originalKeys.length - resultKeys.length) / originalKeys.length;
            if (missingRatio > 0.3 && originalKeys.length > 5) {
                    path: '',
                    message: `Significant portion of payload structure missing (${(missingRatio * 100).toFixed(1)}%)`,
                        pattern: 'partial_state_return',
                        originalKeyCount: originalKeys.length,
                        resultKeyCount: resultKeys.length,
                        missingKeys: originalKeys.filter(k => !resultKeys.includes(k)).slice(0, 10)
        // Detect if complex content was replaced with simple placeholders
        const placeholderPatterns = [
            /^TODO:/i,
            /^TBD$/i,
            /^\[.*\]$/,
            /^<.*>$/
        this.walkPayload(result, [], (value, path) => {
            if (typeof value === 'string' && placeholderPatterns.some(p => p.test(value))) {
                const originalValue = _.get(original, path);
                if (originalValue && typeof originalValue === 'string' && originalValue.length > 50) {
                        message: 'Complex content replaced with placeholder',
                            pattern: 'placeholder_replacement',
                            placeholder: value,
                            originalLength: originalValue.length
     * Walk through payload structure
    private walkPayload(
        obj: any,
        callback: (value: any, path: string[]) => void
        if (!obj || typeof obj !== 'object') {
            callback(obj, path);
        if (Array.isArray(obj)) {
            obj.forEach((item, index) => {
                this.walkPayload(item, [...path, `[${index}]`], callback);
            for (const key in obj) {
                if (obj.hasOwnProperty(key)) {
                    this.walkPayload(obj[key], [...path, key], callback);
     * Generate feedback questions for suspicious changes
    public generateFeedbackQuestions(warnings: PayloadWarning[]): string[] {
        const questions: string[] = [];
        const feedbackWarnings = warnings.filter(w => w.requiresFeedback);
        // Group by type for better organization
        const grouped = _.groupBy(feedbackWarnings, 'type');
        for (const [type, typeWarnings] of Object.entries(grouped)) {
                case PayloadWarningType.ContentTruncation:
                    const truncations = typeWarnings as ContentTruncationWarning[];
                    for (const warning of truncations) {
                        questions.push(
                            `Did you intend to reduce the content at "${warning.path}" from ${warning.details.originalLength} to ${warning.details.newLength} characters (${warning.details.reductionPercentage.toFixed(1)}% reduction)?`
                case PayloadWarningType.KeyRemoval:
                    const removals = typeWarnings as KeyRemovalWarning[];
                    for (const warning of removals) {
                            `Did you intend to remove the following non-empty keys at "${warning.path}": ${warning.details.removedKeys.join(', ')}?`
                case PayloadWarningType.TypeChange:
                    for (const warning of typeWarnings) {
                            `Did you intend to change the type at "${warning.path}" from ${(warning.details as {originalType: string; newType: string}).originalType} to ${(warning.details as {originalType: string; newType: string}).newType}?`
        return questions;
