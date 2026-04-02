// Mock all external dependencies
    BaseAction: class BaseAction {
        protected async InternalRunAction(): Promise<unknown> { return {}; }
    RegisterClass: () => (target: unknown) => target
        LatestResult: unknown = null;
        FirstPrimaryKey = { NeedsQuotes: true };
        Set(_field: string, _value: unknown): void {}
        Get(_field: string): unknown { return ''; }
        async Save(): Promise<boolean> { return true; }
            Set: vi.fn(),
            Get: vi.fn().mockReturnValue(1),
            LatestResult: null
        RunView: vi.fn().mockResolvedValue({ Success: true, Results: [] })
    CompositeKey: { FromID: vi.fn().mockReturnValue({}) },
    RunViewResult: class RunViewResult {}
    ActionParam: class ActionParam {
        Value: unknown = null;
        Type: string = 'Input';
    ActionResultSimple: class ActionResultSimple {
        ResultCode: string = '';
        Message?: string;
    RunActionParams: class RunActionParams {
        Params: unknown[] = [];
        Action: unknown = null;
        Filters: unknown[] = [];
        post: vi.fn()
    ApolloAPIEndpoint: 'https://api.apollo.io/v1',
    EmailSourceName: 'Apollo.io',
    GroupSize: 10,
    ConcurrentGroups: 1,
    MaxPeopleToEnrichPerOrg: 500,
    ApolloAPIKey: 'test-api-key'
import { ApolloEnrichmentAccountsAction } from '../accounts';
import { ApolloEnrichmentContactsAction } from '../contacts';
describe('ApolloEnrichmentAccountsAction', () => {
    let action: ApolloEnrichmentAccountsAction;
        action = new ApolloEnrichmentAccountsAction();
    it('should be instantiable', () => {
        expect(action).toBeDefined();
    describe('IsValidDate', () => {
        it('should return true for valid date strings', () => {
            expect((action as unknown as Record<string, (d: string) => boolean>).IsValidDate('2024-01-15')).toBe(true);
            expect((action as unknown as Record<string, (d: string) => boolean>).IsValidDate('2024-01-15T10:30:00Z')).toBe(true);
        it('should return false for invalid date strings', () => {
            expect((action as unknown as Record<string, (d: string) => boolean>).IsValidDate('')).toBe(false);
            expect((action as unknown as Record<string, (d: string) => boolean>).IsValidDate('not-a-date')).toBe(false);
        it('should return false for null/undefined', () => {
            expect((action as unknown as Record<string, (d: string) => boolean>).IsValidDate(null as unknown as string)).toBe(false);
            expect((action as unknown as Record<string, (d: string) => boolean>).IsValidDate(undefined as unknown as string)).toBe(false);
    describe('IsExcludedTitle', () => {
        it('should exclude student titles', () => {
            expect((action as unknown as Record<string, (t: string) => boolean>).IsExcludedTitle('Student')).toBe(true);
            expect((action as unknown as Record<string, (t: string) => boolean>).IsExcludedTitle('student member')).toBe(true);
        it('should exclude volunteer titles', () => {
            expect((action as unknown as Record<string, (t: string) => boolean>).IsExcludedTitle('Volunteer')).toBe(true);
        it('should exclude member titles', () => {
            expect((action as unknown as Record<string, (t: string) => boolean>).IsExcludedTitle('Member')).toBe(true);
        it('should not exclude regular titles', () => {
            expect((action as unknown as Record<string, (t: string) => boolean>).IsExcludedTitle('CEO')).toBe(false);
            expect((action as unknown as Record<string, (t: string) => boolean>).IsExcludedTitle('Software Engineer')).toBe(false);
        it('should return false for null/undefined title', () => {
            expect((action as unknown as Record<string, (t: string) => boolean>).IsExcludedTitle(null as unknown as string)).toBe(false);
            expect((action as unknown as Record<string, (t: string) => boolean>).IsExcludedTitle(undefined as unknown as string)).toBe(false);
    describe('EscapeSingleQuotes', () => {
        it('should escape single quotes', () => {
            expect((action as unknown as Record<string, (s: string) => string>).EscapeSingleQuotes("O'Brien")).toBe("O''Brien");
        it('should handle strings without quotes', () => {
            expect((action as unknown as Record<string, (s: string) => string>).EscapeSingleQuotes('Hello')).toBe('Hello');
        it('should return empty string for null/undefined', () => {
            expect((action as unknown as Record<string, (s: string) => string>).EscapeSingleQuotes(null as unknown as string)).toBe('');
            expect((action as unknown as Record<string, (s: string) => string>).EscapeSingleQuotes(undefined as unknown as string)).toBe('');
        it('should handle multiple quotes', () => {
            expect((action as unknown as Record<string, (s: string) => string>).EscapeSingleQuotes("it's a 'test'")).toBe("it''s a ''test''");
describe('ApolloEnrichmentContactsAction', () => {
    let action: ApolloEnrichmentContactsAction;
        action = new ApolloEnrichmentContactsAction();
    describe('getParamValue', () => {
        it('should return param value by name case-insensitively', () => {
                Params: [{ Name: 'EntityName', Value: 'Contacts', Type: 'Input' }]
            const result = (action as unknown as Record<string, (params: unknown, name: string) => unknown>).getParamValue(params, 'entityname');
            expect(result).toBe('Contacts');
        it('should return null for "null" string values', () => {
                Params: [{ Name: 'EntityName', Value: 'null', Type: 'Input' }]
            const result = (action as unknown as Record<string, (params: unknown, name: string) => unknown>).getParamValue(params, 'EntityName');
        it('should return undefined for missing params', () => {
            const result = (action as unknown as Record<string, (params: unknown, name: string) => unknown>).getParamValue(params, 'NonExistent');
        it('should escape single quotes in strings', () => {
            expect((action as unknown as Record<string, (s: string) => string>).EscapeSingleQuotes("it's")).toBe("it''s");
        it('should return empty string for falsy input', () => {
            expect((action as unknown as Record<string, (s: string) => string>).EscapeSingleQuotes('')).toBe('');
        it('should return true for valid dates', () => {
            expect((action as unknown as Record<string, (d: string) => boolean>).IsValidDate('2024-06-15')).toBe(true);
        it('should return false for empty or invalid dates', () => {
            expect((action as unknown as Record<string, (d: string) => boolean>).IsValidDate('xyz')).toBe(false);
        it('should exclude configured titles', () => {
            expect((action as unknown as Record<string, (t: string) => boolean>).IsExcludedTitle('VOLUNTEER')).toBe(true);
        it('should not exclude business titles', () => {
            expect((action as unknown as Record<string, (t: string) => boolean>).IsExcludedTitle('VP of Sales')).toBe(false);
        it('should return false for empty/null title', () => {
