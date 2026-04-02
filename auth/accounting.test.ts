        GetEntityObject: vi.fn()
    MJCompanyIntegrationEntity: class MJCompanyIntegrationEntity {
        CompanyID: string = '';
        AccessToken: string | null = null;
        RefreshToken: string | null = null;
        TokenExpirationDate: string | null = null;
        APIKey: string | null = null;
        ExternalSystemID: string | null = null;
        CustomAttribute1: string | null = null;
        NavigationBaseURL: string | null = null;
    MJIntegrationEntity: class MJIntegrationEntity {
    ActionResultSimple: class ActionResultSimple {},
import { BaseAccountingAction } from '../base/base-accounting-action';
import { QuickBooksBaseAction } from '../providers/quickbooks/quickbooks-base.action';
// Create a concrete subclass for testing abstract BaseAccountingAction
class TestAccountingAction extends BaseAccountingAction {
    protected accountingProvider = 'TestProvider';
    protected integrationName = 'Test Integration';
    protected async InternalRunAction(): Promise<{ Success: boolean; ResultCode: string }> {
        return { Success: true, ResultCode: 'SUCCESS' };
describe('BaseAccountingAction', () => {
    let action: TestAccountingAction;
        action = new TestAccountingAction();
    describe('validateAccountNumber', () => {
        it('should return true for valid account numbers', () => {
            expect(action['validateAccountNumber']('1234')).toBe(true);
            expect(action['validateAccountNumber']('100-200')).toBe(true);
            expect(action['validateAccountNumber']('100.200')).toBe(true);
        it('should return false for invalid account numbers', () => {
            expect(action['validateAccountNumber']('abc')).toBe(false);
            expect(action['validateAccountNumber']('12 34')).toBe(false);
            expect(action['validateAccountNumber']('12@34')).toBe(false);
    describe('validateJournalEntryBalance', () => {
        it('should return true when debits equal credits', () => {
            const lines = [
                { debit: 100, credit: 0 },
                { debit: 0, credit: 100 }
            expect(action['validateJournalEntryBalance'](lines)).toBe(true);
        it('should return true for small rounding differences', () => {
                { debit: 100.005, credit: 0 },
                { debit: 0, credit: 100.001 }
        it('should return false when debits do not equal credits', () => {
                { debit: 0, credit: 50 }
            expect(action['validateJournalEntryBalance'](lines)).toBe(false);
        it('should handle missing debit/credit values', () => {
                { debit: 100 },
                { credit: 100 }
            ] as Array<{ debit?: number; credit?: number }>;
    describe('formatCurrency', () => {
        it('should format USD by default', () => {
            const result = action['formatCurrency'](1234.56);
            expect(result).toBe('$1,234.56');
        it('should handle zero', () => {
            const result = action['formatCurrency'](0);
            expect(result).toBe('$0.00');
    describe('formatAccountingDate', () => {
        it('should format date as YYYY-MM-DD', () => {
            const date = new Date('2024-06-15T10:30:00Z');
            const result = action['formatAccountingDate'](date);
            expect(result).toBe('2024-06-15');
    describe('buildAccountingErrorMessage', () => {
        it('should build error message without system error', () => {
            const result = action['buildAccountingErrorMessage']('CreateJournal', 'Invalid data');
            expect(result).toBe('Accounting operation failed: CreateJournal. Invalid data');
        it('should include system error when provided', () => {
            const result = action['buildAccountingErrorMessage']('CreateJournal', 'Invalid data', new Error('DB error'));
            expect(result).toContain('System error: DB error');
        it('should return param value by name', () => {
            const params = [{ Name: 'CompanyID', Value: '123', Type: 'Input' as const }];
            expect(action['getParamValue'](params, 'CompanyID')).toBe('123');
        it('should return undefined for missing param', () => {
            expect(action['getParamValue'](params, 'NonExistent')).toBeUndefined();
    describe('getCommonAccountingParams', () => {
        it('should return three common params', () => {
            const params = action['getCommonAccountingParams']();
            expect(params).toHaveLength(3);
            expect(params.map((p: { Name: string }) => p.Name)).toEqual(['CompanyID', 'FiscalYear', 'AccountingPeriod']);
    describe('getCredentialFromEnv', () => {
        it('should construct correct env key', () => {
            process.env['BIZAPPS_TESTPROVIDER_COMP1_ACCESS_TOKEN'] = 'token123';
            const result = action['getCredentialFromEnv']('COMP1', 'ACCESS_TOKEN');
            expect(result).toBe('token123');
            delete process.env['BIZAPPS_TESTPROVIDER_COMP1_ACCESS_TOKEN'];
        it('should return undefined for missing env var', () => {
            const result = action['getCredentialFromEnv']('COMP1', 'MISSING_KEY');
describe('QuickBooksBaseAction', () => {
    describe('mapAccountType', () => {
        // Create test instance
        class TestQBAction extends QuickBooksBaseAction {
        let action: TestQBAction;
            action = new TestQBAction();
        it('should map Bank to Asset', () => {
            expect(action['mapAccountType']('Bank')).toBe('Asset');
        it('should map Accounts Payable to Liability', () => {
            expect(action['mapAccountType']('Accounts Payable')).toBe('Liability');
        it('should map Equity to Equity', () => {
            expect(action['mapAccountType']('Equity')).toBe('Equity');
        it('should map Income to Revenue', () => {
            expect(action['mapAccountType']('Income')).toBe('Revenue');
        it('should map Expense to Expense', () => {
            expect(action['mapAccountType']('Expense')).toBe('Expense');
        it('should return Other for unknown types', () => {
            expect(action['mapAccountType']('Unknown')).toBe('Other');
    describe('parseQBODate', () => {
        it('should parse QBO date format', () => {
            const action = new TestQBAction();
            const date = action['parseQBODate']('2024-06-15');
            expect(date.toISOString()).toBe('2024-06-15T00:00:00.000Z');
    describe('formatQBODate', () => {
        it('should format date for QBO API', () => {
            expect(action['formatQBODate'](date)).toBe('2024-06-15');
