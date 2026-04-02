    OAuth2Manager: class OAuth2Manager {}
import { BaseFormBuilderAction, FormResponse, FormAnswer } from '../base/base-form-builder.action';
class TestFormBuilderAction extends BaseFormBuilderAction {
    protected formPlatform = 'TestForms';
    protected integrationName = 'TestForms';
describe('BaseFormBuilderAction', () => {
    let action: TestFormBuilderAction;
        action = new TestFormBuilderAction();
    describe('calculateCompletionRate', () => {
        it('should calculate completion percentage', () => {
            expect(action['calculateCompletionRate'](75, 100)).toBe(75);
            expect(action['calculateCompletionRate'](1, 3)).toBe(33.33);
        it('should return 0 when total is 0', () => {
            expect(action['calculateCompletionRate'](0, 0)).toBe(0);
        it('should return 100 when all completed', () => {
            expect(action['calculateCompletionRate'](50, 50)).toBe(100);
        it('should format seconds only', () => {
            expect(action['formatDuration'](30)).toBe('30s');
            expect(action['formatDuration'](90)).toBe('1m 30s');
        it('should format hours, minutes, and seconds', () => {
            expect(action['formatDuration'](3661)).toBe('1h 1m 1s');
    describe('formatFormDate', () => {
            expect(action['formatFormDate'](date)).toBe('2024-06-15T10:30:00.000Z');
    describe('parseFormDate', () => {
        it('should parse ISO string', () => {
            const result = action['parseFormDate']('2024-06-15T10:30:00Z');
        it('should parse unix timestamp (seconds)', () => {
            const result = action['parseFormDate'](1718444400);
            expect(result).toBeInstanceOf(Date);
            expect(result.getTime()).toBe(1718444400000);
    describe('buildFormErrorMessage', () => {
            expect(action['buildFormErrorMessage']('GetForm', 'Not found'))
                .toBe('Form operation failed: GetForm. Not found');
        it('should include system error details', () => {
            const result = action['buildFormErrorMessage']('GetForm', 'Failed', new Error('timeout'));
            expect(result).toContain('System error: timeout');
    describe('getCommonFormParams', () => {
        it('should return CompanyID and FormID params', () => {
            const params = action['getCommonFormParams']();
            expect(params).toHaveLength(2);
            expect(params.map((p: { Name: string }) => p.Name)).toEqual(['CompanyID', 'FormID']);
    describe('extractEmailFromResponses', () => {
        it('should extract emails from form responses', () => {
            const responses: FormResponse[] = [
                    responseId: '1',
                    formId: 'f1',
                    submittedAt: new Date(),
                    completed: true,
                    answerDetails: [
                        { fieldId: 'f1', fieldType: 'email', question: 'Email', answer: 'test@example.com' }
                    answers: {}
            const emails = action['extractEmailFromResponses'](responses);
            expect(emails).toEqual(['test@example.com']);
        it('should deduplicate emails', () => {
                    responseId: '2',
    describe('groupResponsesByDate', () => {
        it('should group responses by date', () => {
                { responseId: '1', formId: 'f1', submittedAt: new Date('2024-06-15'), completed: true, answerDetails: [] },
                { responseId: '2', formId: 'f1', submittedAt: new Date('2024-06-15'), completed: true, answerDetails: [] },
                { responseId: '3', formId: 'f1', submittedAt: new Date('2024-06-16'), completed: true, answerDetails: [] }
            const grouped = action['groupResponsesByDate'](responses);
            expect(grouped['2024-06-15']).toBe(2);
            expect(grouped['2024-06-16']).toBe(1);
        it('should check company-specific env var first', () => {
            process.env['BIZAPPS_TESTFORMS_COMP1_API_TOKEN'] = 'token123';
            expect(action['getCredentialFromEnv']('COMP1', 'API_TOKEN')).toBe('token123');
            delete process.env['BIZAPPS_TESTFORMS_COMP1_API_TOKEN'];
        it('should fall back to default env var', () => {
            process.env['BIZAPPS_TESTFORMS_API_TOKEN'] = 'default-token';
            expect(action['getCredentialFromEnv']('COMP1', 'API_TOKEN')).toBe('default-token');
            delete process.env['BIZAPPS_TESTFORMS_API_TOKEN'];
    describe('convertToCSV', () => {
        it('should return empty for no responses', () => {
            const result = action['convertToCSV']([]);
            expect(result.csv).toBe('');
            expect(result.headers).toEqual([]);
            const params = [{ Name: 'FormID', Value: 'abc', Type: 'Input' as const }];
            expect(action['getParamValue'](params, 'FormID')).toBe('abc');
