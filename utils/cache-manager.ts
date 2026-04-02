 * @fileoverview Cache management with TTL and size limits
 * @module @memberjunction/react-runtime/utilities
export interface CacheEntry<T> {
export interface CacheOptions {
  maxSize?: number;          // Maximum number of entries
  maxMemory?: number;        // Maximum memory in bytes (estimated)
  defaultTTL?: number;       // Default TTL in milliseconds
  cleanupInterval?: number;  // Cleanup interval in milliseconds
 * Cache manager with TTL and size limits.
 * Provides automatic cleanup and memory management.
export class CacheManager<T = any> {
  private cache = new Map<string, CacheEntry<T>>();
  private memoryUsage = 0;
  private cleanupTimer?: number;
  private readonly options: Required<CacheOptions>;
  constructor(options: CacheOptions = {}) {
      maxSize: options.maxSize || 1000,
      maxMemory: options.maxMemory || 50 * 1024 * 1024, // 50MB default
      defaultTTL: options.defaultTTL || 5 * 60 * 1000,   // 5 minutes default
      cleanupInterval: options.cleanupInterval || 60 * 1000 // 1 minute default
    if (this.options.cleanupInterval > 0) {
   * Set a value in the cache
  set(key: string, value: T, ttl?: number): void {
    const size = this.estimateSize(value);
    const entry: CacheEntry<T> = {
    if (this.cache.size >= this.options.maxSize) {
    // Check memory usage
    if (this.memoryUsage + size > this.options.maxMemory) {
      this.evictByMemory(size);
    // Remove old entry if exists
    const oldEntry = this.cache.get(key);
    if (oldEntry) {
      this.memoryUsage -= oldEntry.size || 0;
    this.memoryUsage += size;
    // Schedule removal if TTL is set
    if (ttl || this.options.defaultTTL) {
      const timeout = ttl || this.options.defaultTTL;
      setTimeout(() => this.delete(key), timeout);
   * Get a value from the cache
  get(key: string): T | undefined {
    if (!entry) return undefined;
    if (this.isExpired(entry)) {
      this.delete(key);
    // Update timestamp for LRU
    entry.timestamp = Date.now();
    return entry.value;
   * Check if a key exists and is not expired
  has(key: string): boolean {
    if (!entry) return false;
   * Delete a key from the cache
  delete(key: string): boolean {
      this.memoryUsage -= entry.size || 0;
      return this.cache.delete(key);
   * Clear all entries
    this.memoryUsage = 0;
    memoryUsage: number;
    maxSize: number;
    maxMemory: number;
      memoryUsage: this.memoryUsage,
      maxSize: this.options.maxSize,
      maxMemory: this.options.maxMemory
   * Manually trigger cleanup
  cleanup(): number {
    let removed = 0;
    for (const [key, entry] of this.cache) {
      if (this.isExpired(entry, now)) {
        removed++;
   * Destroy the cache and stop cleanup timer
   * Check if an entry is expired
  private isExpired(entry: CacheEntry<T>, now?: number): boolean {
    if (!this.options.defaultTTL) return false;
    const currentTime = now || Date.now();
    return currentTime - entry.timestamp > this.options.defaultTTL;
   * Evict least recently used entry
    let lruTime = Infinity;
      if (entry.timestamp < lruTime) {
        lruTime = entry.timestamp;
      this.delete(lruKey);
   * Evict entries to make room for new memory
  private evictByMemory(requiredSize: number): void {
    const entries = Array.from(this.cache.entries())
      .sort((a, b) => a[1].timestamp - b[1].timestamp);
    let freedMemory = 0;
    for (const [key, entry] of entries) {
      if (freedMemory >= requiredSize) break;
      freedMemory += entry.size || 0;
   * Estimate size of a value
  private estimateSize(value: T): number {
      return value.length * 2; // 2 bytes per character
      // Rough estimation for objects
        return JSON.stringify(value).length * 2;
        return 1024; // Default 1KB for objects that can't be stringified
      return 8; // Default for primitives
   * Start the cleanup timer
    this.cleanupTimer = window.setInterval(() => {
    }, this.options.cleanupInterval);
   * Stop the cleanup timer
      window.clearInterval(this.cleanupTimer);
