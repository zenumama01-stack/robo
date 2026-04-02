import { parseRerankerConfiguration } from '../config.types';
    describe('null/empty/invalid input', () => {
        it('should return null for invalid JSON', () => {
            expect(parseRerankerConfiguration('not-json')).toBeNull();
        it('should return null for JSON without rerankerModelId', () => {
            expect(parseRerankerConfiguration(JSON.stringify({ enabled: true }))).toBeNull();
                rerankerModelId: 'model-1'
    describe('valid configuration', () => {
        it('should parse minimal valid config with defaults', () => {
                rerankerModelId: 'model-1',
                retrievalMultiplier: 3,
                minRelevanceThreshold: 0.5,
                rerankPromptID: undefined,
                contextFields: [],
                fallbackOnError: true
        it('should override defaults when values provided', () => {
                rerankerModelId: 'model-2',
                rerankPromptID: 'prompt-1',
        it('should default enabled to true when not explicitly set', () => {
