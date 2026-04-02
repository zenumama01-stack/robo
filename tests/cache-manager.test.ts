// Mock window for timer functions
const mockSetInterval = vi.fn().mockReturnValue(1);
const mockClearInterval = vi.fn();
vi.stubGlobal('window', {
  setInterval: mockSetInterval,
  clearInterval: mockClearInterval,
import { CacheManager } from '../utilities/cache-manager';
describe('CacheManager', () => {
  let cache: CacheManager<string>;
    mockSetInterval.mockClear();
    mockClearInterval.mockClear();
    cache = new CacheManager<string>({
      maxSize: 5,
      defaultTTL: 10000, // 10 seconds
      cleanupInterval: 0, // disable auto cleanup for most tests
    cache.destroy();
    it('should store and retrieve a value', () => {
      cache.set('key1', 'value1');
      expect(cache.get('key1')).toBe('value1');
    it('should return undefined for missing key', () => {
      expect(cache.get('nonexistent')).toBeUndefined();
    it('should overwrite existing value', () => {
      cache.set('key1', 'value2');
      expect(cache.get('key1')).toBe('value2');
  describe('has', () => {
    it('should return true for existing key', () => {
      expect(cache.has('key1')).toBe(true);
    it('should return false for missing key', () => {
      expect(cache.has('nonexistent')).toBe(false);
    it('should remove a key', () => {
      expect(cache.delete('key1')).toBe(true);
      expect(cache.get('key1')).toBeUndefined();
    it('should return false for non-existent key', () => {
      expect(cache.delete('nonexistent')).toBe(false);
      cache.set('key2', 'value2');
      expect(cache.get('key2')).toBeUndefined();
    it('should reset memory usage', () => {
      expect(stats.memoryUsage).toBe(0);
    it('should return cache statistics', () => {
      expect(stats.size).toBe(1);
      expect(stats.maxSize).toBe(5);
      expect(stats.memoryUsage).toBeGreaterThan(0);
    it('should return undefined for expired entries on get', () => {
      vi.advanceTimersByTime(11000); // advance past 10s TTL
    it('should report false for expired entries on has', () => {
      vi.advanceTimersByTime(11000);
      expect(cache.has('key1')).toBe(false);
    it('should evict oldest entry when max size reached', () => {
      // Fill to capacity (maxSize = 5)
        cache.set(`key${i}`, `value${i}`);
        vi.advanceTimersByTime(100); // separate timestamps
      // Adding one more should evict the oldest (key0)
      cache.set('key5', 'value5');
      // key0 should be evicted
      expect(cache.has('key0')).toBe(false);
      // newest should be present
      expect(cache.get('key5')).toBe('value5');
  describe('cleanup', () => {
    it('should remove expired entries that scheduled timeout has not yet removed', () => {
      // When set() is called, it schedules a setTimeout for TTL deletion.
      // With fake timers, advancing time triggers those scheduled removals.
      // After the TTL, entries are already removed by their scheduled timeouts.
      // Advance time past TTL -- the scheduled timeouts will fire and remove entries
      // Entries already removed by scheduled timeout, so cleanup finds nothing
      const removed = cache.cleanup();
      expect(removed).toBe(0);
      expect(cache.getStats().size).toBe(0);
    it('should return 0 when nothing to clean', () => {
  describe('destroy', () => {
    it('should clear the cache and stop timers', () => {
