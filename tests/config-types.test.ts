 * Unit tests for RerankerConfiguration and parseRerankerConfiguration
import { parseRerankerConfiguration, RerankerConfiguration } from '../config.types';
describe('parseRerankerConfiguration', () => {
    describe('null/undefined/empty input', () => {
        it('should return null for null input', () => {
            expect(parseRerankerConfiguration(null)).toBeNull();
        it('should return null for undefined input', () => {
            expect(parseRerankerConfiguration(undefined)).toBeNull();
        it('should return null for empty string', () => {
            expect(parseRerankerConfiguration('')).toBeNull();
        it('should return null for whitespace-only string', () => {
            expect(parseRerankerConfiguration('   ')).toBeNull();
    describe('invalid JSON', () => {
        it('should return null for malformed JSON', () => {
            expect(parseRerankerConfiguration('not json')).toBeNull();
        it('should return null for partial JSON', () => {
            expect(parseRerankerConfiguration('{"enabled": true')).toBeNull();
    describe('disabled configuration', () => {
        it('should return null when enabled is false', () => {
            const config = JSON.stringify({
                rerankerModelId: 'model-123'
            expect(parseRerankerConfiguration(config)).toBeNull();
    describe('missing required fields', () => {
        it('should return null when rerankerModelId is missing', () => {
                enabled: true
        it('should return null when rerankerModelId is empty string', () => {
                rerankerModelId: ''
    describe('valid configuration with defaults', () => {
        it('should apply default retrievalMultiplier of 3', () => {
            const result = parseRerankerConfiguration(config);
            expect(result!.retrievalMultiplier).toBe(3);
        it('should apply default minRelevanceThreshold of 0.5', () => {
            expect(result!.minRelevanceThreshold).toBe(0.5);
        it('should apply default fallbackOnError of true', () => {
            expect(result!.fallbackOnError).toBe(true);
        it('should apply default contextFields as empty array', () => {
            expect(result!.contextFields).toEqual([]);
        it('should leave rerankPromptID as undefined when not provided', () => {
            expect(result!.rerankPromptID).toBeUndefined();
    describe('valid configuration with custom values', () => {
        it('should preserve all provided values', () => {
                rerankerModelId: 'model-456',
                retrievalMultiplier: 5,
                minRelevanceThreshold: 0.7,
                rerankPromptID: 'prompt-789',
                contextFields: ['Keywords', 'Type'],
                fallbackOnError: false
            } satisfies RerankerConfiguration);
        it('should always set enabled to true for valid configs', () => {
                // enabled not explicitly set
            expect(result!.enabled).toBe(true);
    describe('edge cases', () => {
        it('should handle zero retrievalMultiplier', () => {
                rerankerModelId: 'model-123',
                retrievalMultiplier: 0
            // 0 is falsy but nullish coalescing (??) treats 0 as non-null
            expect(result!.retrievalMultiplier).toBe(0);
        it('should handle zero minRelevanceThreshold', () => {
                minRelevanceThreshold: 0
            expect(result!.minRelevanceThreshold).toBe(0);
import { isValidConfig } from '../config-types';
describe('isValidConfig', () => {
    it('should return true for a plain object', () => {
        expect(isValidConfig({ key: 'value' })).toBe(true);
    it('should return true for an empty object', () => {
        expect(isValidConfig({})).toBe(true);
    it('should return true for a nested object', () => {
        expect(isValidConfig({ a: { b: { c: 1 } } })).toBe(true);
    it('should return true for an array (arrays are objects)', () => {
        expect(isValidConfig([1, 2, 3])).toBe(true);
    it('should return false for null', () => {
        expect(isValidConfig(null)).toBe(false);
    it('should return false for undefined', () => {
        expect(isValidConfig(undefined)).toBe(false);
    it('should return false for a string', () => {
        expect(isValidConfig('hello')).toBe(false);
    it('should return false for a number', () => {
        expect(isValidConfig(42)).toBe(false);
    it('should return false for a boolean', () => {
        expect(isValidConfig(true)).toBe(false);
        expect(isValidConfig(false)).toBe(false);
    it('should return false for a symbol', () => {
        expect(isValidConfig(Symbol('test'))).toBe(false);
    it('should return true for a Date object', () => {
        expect(isValidConfig(new Date())).toBe(true);
    it('should return true for a class instance', () => {
        class MyClass { name = 'test'; }
        expect(isValidConfig(new MyClass())).toBe(true);
