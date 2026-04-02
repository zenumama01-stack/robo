 * @fileoverview Unit tests for RateLimiter
import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { RateLimiter, RateLimiterRegistry } from '../RateLimiter.js';
describe('RateLimiter', () => {
        vi.useFakeTimers();
        vi.useRealTimers();
        it('should create a RateLimiter with the provided config', () => {
            const limiter = new RateLimiter({ perMinute: 60, perHour: 1000 });
            const status = limiter.getStatus();
            expect(status.minuteLimit).toBe(60);
            expect(status.hourLimit).toBe(1000);
            expect(status.minuteUsage).toBe(0);
            expect(status.hourUsage).toBe(0);
        it('should handle undefined limits', () => {
            const limiter = new RateLimiter({});
            expect(status.minuteLimit).toBeUndefined();
            expect(status.hourLimit).toBeUndefined();
    describe('canProceed', () => {
        it('should return true when under limits', () => {
            const limiter = new RateLimiter({ perMinute: 10, perHour: 100 });
            expect(limiter.canProceed()).toBe(true);
        it('should return true when no limits configured', () => {
        it('should return false when minute limit reached', async () => {
            const limiter = new RateLimiter({ perMinute: 2 });
            await limiter.acquire();
            expect(limiter.canProceed()).toBe(false);
        it('should return false when hour limit reached', async () => {
            const limiter = new RateLimiter({ perHour: 2 });
    describe('acquire', () => {
        it('should resolve immediately when under limits', async () => {
            const limiter = new RateLimiter({ perMinute: 10 });
            const start = Date.now();
            const elapsed = Date.now() - start;
            expect(elapsed).toBeLessThan(100);
        it('should increment usage counters', async () => {
            expect(status.minuteUsage).toBe(3);
            expect(status.hourUsage).toBe(3);
        it('should add requests to queue when at limit', async () => {
            const limiter = new RateLimiter({ perMinute: 1 });
            // First request succeeds immediately
            // Second request should be queued - don't await it
            // eslint-disable-next-line @typescript-eslint/no-floating-promises
            limiter.acquire(100).catch(() => {
                // Expected to timeout, ignore error
            // Check queue length
            expect(limiter.getStatus().queueLength).toBe(1);
    describe('getEstimatedWaitMs', () => {
        it('should return 0 when under limits', () => {
            expect(limiter.getEstimatedWaitMs()).toBe(0);
        it('should return estimated wait when at minute limit', async () => {
            const wait = limiter.getEstimatedWaitMs();
            // Should be close to 60 seconds
            expect(wait).toBeGreaterThan(59000);
            expect(wait).toBeLessThanOrEqual(60000);
    describe('reset', () => {
        it('should clear all state', async () => {
            limiter.reset();
            expect(status.queueLength).toBe(0);
        it('should reject queued requests on reset', async () => {
            const queuePromise = limiter.acquire(60000);
            await expect(queuePromise).rejects.toThrow(/reset/i);
    describe('updateConfig', () => {
        it('should update rate limits', async () => {
            limiter.updateConfig({ perMinute: 20 });
            expect(status.minuteLimit).toBe(20);
    describe('sliding window', () => {
        it('should allow new requests after window expires', async () => {
            // Advance past minute window
            vi.advanceTimersByTime(61000);
describe('RateLimiterRegistry', () => {
    it('should create and cache rate limiters', () => {
        const registry = new RateLimiterRegistry();
        const limiter1 = registry.getOrCreate('conn1', { perMinute: 10 });
        const limiter2 = registry.getOrCreate('conn1', { perMinute: 20 });
        // Should return same instance
        expect(limiter1).toBe(limiter2);
        expect(registry.size).toBe(1);
    it('should create separate limiters per connection', () => {
        const limiter2 = registry.getOrCreate('conn2', { perMinute: 20 });
        expect(limiter1).not.toBe(limiter2);
        expect(registry.size).toBe(2);
    it('should get existing limiter', () => {
        registry.getOrCreate('conn1', { perMinute: 10 });
        const limiter = registry.get('conn1');
        expect(limiter).toBeDefined();
        const nonExistent = registry.get('nonexistent');
        expect(nonExistent).toBeUndefined();
    it('should update limiter config', () => {
        registry.updateConfig('conn1', { perMinute: 20 });
        const limiter = registry.get('conn1')!;
    it('should remove limiter', () => {
        registry.remove('conn1');
        expect(registry.size).toBe(0);
        expect(registry.get('conn1')).toBeUndefined();
    it('should clear all limiters', () => {
        registry.getOrCreate('conn2', { perMinute: 20 });
        registry.clear();
