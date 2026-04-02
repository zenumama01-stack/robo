  class MockBaseEmbeddings {
    SetAdditionalSettings(settings: Record<string, unknown>) {
    BaseEmbeddings: MockBaseEmbeddings,
    EmbedTextParams: class {},
    EmbedTextsParams: class {},
    EmbedTextResult: class {},
    EmbedTextsResult: class {},
import { LocalEmbedding } from '../models/localEmbedding';
describe('LocalEmbedding', () => {
  let embedding: LocalEmbedding;
    // Clear static caches between tests
    LocalEmbedding.clearSharedCache();
    embedding = new LocalEmbedding();
    it('should create instance without API key', () => {
      const inst = new LocalEmbedding();
      expect(inst).toBeInstanceOf(LocalEmbedding);
    it('should accept optional API key', () => {
      const inst = new LocalEmbedding('optional-key');
  /* ---- EmbedText ---- */
    it('should throw error when model name is missing', async () => {
      await expect(embedding.EmbedText({ text: 'hello' } as never))
        .rejects.toThrow('Model name is required for LocalEmbedding provider');
  /* ---- EmbedTexts ---- */
      await expect(embedding.EmbedTexts({ texts: ['hello'] } as never))
  /* ---- GetEmbeddingModels ---- */
    it('should return empty array', async () => {
      const models = await embedding.GetEmbeddingModels();
  /* ---- clearCache ---- */
  describe('clearCache', () => {
    it('should clear instance cache without errors', () => {
      expect(() => embedding.clearCache()).not.toThrow();
  /* ---- clearSharedCache ---- */
  describe('clearSharedCache', () => {
    it('should clear shared static cache without errors', () => {
      expect(() => LocalEmbedding.clearSharedCache()).not.toThrow();
    it('should store cacheDir setting', () => {
      embedding.SetAdditionalSettings({ cacheDir: '/tmp/models' });
      // Should not throw – pending settings will be applied when transformers loads
    it('should store useQuantized setting', () => {
      embedding.SetAdditionalSettings({ useQuantized: false });
      expect((embedding as unknown as Record<string, unknown>)['_additionalSettings']).toHaveProperty('useQuantized', false);
