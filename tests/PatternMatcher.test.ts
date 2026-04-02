 * Comprehensive coverage including edge cases not in the original spec file
import { PatternMatcher } from '../PatternMatcher';
                expect(PatternMatcher.match('Users', 'users').matched).toBe(true);
                expect(PatternMatcher.match('users', 'USERS').matched).toBe(true);
                expect(PatternMatcher.match('UsErS', 'uSeRs').matched).toBe(true);
                expect(PatternMatcher.match('AnyValue', '*').matched).toBe(true);
                expect(PatternMatcher.match('AnyValue', '*').matchedPattern).toBe('*');
                expect(PatternMatcher.match('AnyValue', null).matched).toBe(true);
                expect(PatternMatcher.match('AnyValue', null).matchedPattern).toBe('*');
            it('should match prefix wildcard', () => {
                expect(PatternMatcher.match('SkipAnalysis', 'Skip*').matched).toBe(true);
                expect(PatternMatcher.match('DataAgent', 'Skip*').matched).toBe(false);
            it('should match suffix wildcard', () => {
                expect(PatternMatcher.match('SkipAgent', '*Agent').matched).toBe(true);
                expect(PatternMatcher.match('SkipReport', '*Agent').matched).toBe(false);
            it('should match contains wildcard', () => {
                expect(PatternMatcher.match('DailyReportSummary', '*Report*').matched).toBe(true);
                expect(PatternMatcher.match('DailySummary', '*Report*').matched).toBe(false);
                expect(PatternMatcher.match('Users', 'User?').matched).toBe(true);
            it('should return first matching pattern', () => {
                const result = PatternMatcher.match('Users', 'Users,Accounts');
                expect(PatternMatcher.match('Users', ' Users , Accounts ').matched).toBe(true);
                expect(PatternMatcher.match('Orders', 'Users,Accounts').matched).toBe(false);
                expect(PatternMatcher.match('Users', '').matched).toBe(false);
                expect(PatternMatcher.match('Users', '   ').matched).toBe(false);
                expect(PatternMatcher.match('', '*').matched).toBe(true);
            it('should handle regex special characters in value', () => {
                expect(PatternMatcher.match('User.Name', 'User.Name').matched).toBe(true);
            it('should escape regex special characters', () => {
                // . in pattern should be literal, not regex any-char
                expect(PatternMatcher.match('UserXName', 'User.Name').matched).toBe(false);
            it('should handle patterns with colons', () => {
                expect(PatternMatcher.match('entity:read', 'entity:read').matched).toBe(true);
                expect(PatternMatcher.match('entity:read', 'entity:*').matched).toBe(true);
        it('should detect * wildcard', () => {
        it('should detect ? wildcard', () => {
        it('should treat null as wildcard', () => {
            expect(PatternMatcher.parsePatterns('Users,Accounts')).toEqual(['Users', 'Accounts']);
        it('should trim whitespace', () => {
            expect(PatternMatcher.parsePatterns(' Users , Accounts ')).toEqual(['Users', 'Accounts']);
            expect(PatternMatcher.parsePatterns(null)).toEqual(['*']);
            expect(PatternMatcher.parsePatterns('Users,,Accounts')).toEqual(['Users', 'Accounts']);
        it('should reject invalid patterns', () => {
        it('should reject trailing comma creating empty pattern', () => {
            expect(PatternMatcher.isValidPattern('Users,')).toBe(false);
