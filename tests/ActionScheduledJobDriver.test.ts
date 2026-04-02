    SafeJSONParse: (value: string) => {
    UserInfo: class { ID = 'user-1' },
            Load: vi.fn().mockResolvedValue(true)
        Provider: {}
    LogStatusEx: vi.fn(),
    IsVerboseLoggingEnabled: vi.fn(() => false),
    ValidationErrorInfo: class {
        constructor(public Source: string, public Message: string, public Value: unknown, public Type: string) {}
    ValidationErrorType: { Failure: 'Failure' }
                Message: 'Action completed',
                Result: { ResultCode: 'OK' },
        ExecuteSQL = vi.fn();
vi.mock('@memberjunction/scheduling-base-types', () => ({
    ScheduledJobResult: class {},
    NotificationContent: class {},
    ActionJobConfiguration: class {}
    MJScheduledJobTypeEntity: class {}
import { ActionScheduledJobDriver } from '../drivers/ActionScheduledJobDriver';
describe('ActionScheduledJobDriver', () => {
    let driver: ActionScheduledJobDriver;
        driver = new ActionScheduledJobDriver();
        it('should return success for valid configuration with ActionID', () => {
            const schedule = {
                Configuration: JSON.stringify({ ActionID: 'action-123' })
            const result = driver.ValidateConfiguration(schedule);
        it('should fail when ActionID is missing', () => {
                Configuration: JSON.stringify({})
            expect(result.Errors[0].Source).toBe('Configuration.ActionID');
        it('should fail when Configuration is not valid JSON', () => {
                Configuration: 'not json'
        it('should fail when Configuration is missing entirely', () => {
            const schedule = { Configuration: null };
        it('should validate Params array structure', () => {
                Configuration: JSON.stringify({
                    ActionID: 'action-123',
                        { ActionParamID: 'p1', ValueType: 'Static', Value: 'test' }
        it('should fail when Params element is missing ActionParamID', () => {
                        { ValueType: 'Static', Value: 'test' }
            expect(result.Errors.some(
                (e: { Source: string }) => e.Source.includes('ActionParamID')
        it('should fail when Params element has invalid ValueType', () => {
                        { ActionParamID: 'p1', ValueType: 'Invalid', Value: 'test' }
                (e: { Source: string }) => e.Source.includes('ValueType')
        it('should accept SQL Statement as valid ValueType', () => {
                        { ActionParamID: 'p1', ValueType: 'SQL Statement', Value: 'SELECT 1' }
        it('should succeed with no Params', () => {
    describe('FormatNotification', () => {
        it('should format success notification correctly', () => {
                Schedule: { ID: 'schedule-1', Name: 'Test Job' },
                Run: {},
                Details: { ResultCode: 'OK' }
            const notification = driver.FormatNotification(
                context as Parameters<typeof driver.FormatNotification>[0],
                result as Parameters<typeof driver.FormatNotification>[1]
            expect(notification.Subject).toContain('Completed');
            expect(notification.Subject).toContain('Test Job');
            expect(notification.Priority).toBe('Normal');
        it('should format failure notification with High priority', () => {
                Schedule: { ID: 'schedule-1', Name: 'Failing Job' },
                Details: {}
            expect(notification.Subject).toContain('Failed');
            expect(notification.Subject).toContain('Failing Job');
            expect(notification.Priority).toBe('High');
            expect(notification.Body).toContain('Something went wrong');
        it('should include metadata in notification', () => {
                Schedule: { ID: 'schedule-abc', Name: 'Metadata Job' },
                Details: { ResultCode: 'SUCCESS' }
            expect(notification.Metadata).toBeDefined();
            expect(notification.Metadata.ScheduleID).toBe('schedule-abc');
            expect(notification.Metadata.JobType).toBe('Action');
