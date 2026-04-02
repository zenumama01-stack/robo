    ValidationResult: class ValidationResult {
        Errors: { Source: string; Message: string }[] = [];
        constructor(public Source: string, public Message: string, public Value: string, public Type: string) {}
    ValidationErrorType: { Failure: 'Failure', Warning: 'Warning' }
    ScheduledJobConfiguration: class {},
    NotificationContent: class {}
import { BaseScheduledJob, ScheduledJobExecutionContext } from '../BaseScheduledJob';
const mockedLogStatusEx = vi.mocked(LogStatusEx);
const mockedLogError = vi.mocked(LogError);
class TestScheduledJob extends BaseScheduledJob {
    async Execute(context: ScheduledJobExecutionContext) {
        return { Success: true, ErrorMessage: undefined, Details: {} };
    ValidateConfiguration(schedule: { Configuration?: string }) {
        const result = { Success: true, Errors: [] as { Source: string; Message: string }[] };
        return result as ReturnType<BaseScheduledJob['ValidateConfiguration']>;
    FormatNotification(context: ScheduledJobExecutionContext, result: { Success: boolean }) {
        return { Subject: 'Test', Body: 'Test body', Priority: 'Normal' as const, Metadata: {} };
    public testParseConfiguration<T>(schedule: { Configuration?: string | null }): T {
        return this.parseConfiguration<T>(schedule as Parameters<typeof this.parseConfiguration>[0]);
    public testLog(message: string, verboseOnly?: boolean): void {
        this.log(message, verboseOnly);
    public testLogError(message: string, error?: unknown): void {
        this.logError(message, error);
describe('BaseScheduledJob', () => {
    let job: TestScheduledJob;
        job = new TestScheduledJob();
        it('should parse valid JSON configuration', () => {
            const schedule = { Configuration: '{"key": "value", "count": 42}' };
            const result = job.testParseConfiguration<{ key: string; count: number }>(schedule);
            expect(result).toEqual({ key: 'value', count: 42 });
        it('should throw when configuration is missing (null)', () => {
            expect(() => job.testParseConfiguration(schedule)).toThrow('Configuration is required for job type');
        it('should throw when configuration is undefined', () => {
            const schedule = { Configuration: undefined };
        it('should throw when configuration is empty string', () => {
            // empty string is falsy, so it should throw
            const schedule = { Configuration: 'not valid json {' };
            expect(() => job.testParseConfiguration(schedule)).toThrow('Invalid Configuration JSON');
        it('should parse nested configuration objects', () => {
            const config = { ActionID: 'abc-123', Params: [{ name: 'p1', value: 'v1' }] };
            const schedule = { Configuration: JSON.stringify(config) };
            const result = job.testParseConfiguration<typeof config>(schedule);
            expect(result.ActionID).toBe('abc-123');
            expect(result.Params[0].name).toBe('p1');
        it('should parse configuration with special characters', () => {
            const config = { message: 'Hello "world" & <test>' };
            expect(result.message).toBe('Hello "world" & <test>');
        it('should parse configuration with numeric values', () => {
            const config = { timeout: 30000, retries: 3, ratio: 0.5 };
            expect(result.timeout).toBe(30000);
            expect(result.retries).toBe(3);
            expect(result.ratio).toBe(0.5);
    describe('log', () => {
        it('should call LogStatusEx with formatted message', () => {
            job.testLog('Test message');
            expect(mockedLogStatusEx).toHaveBeenCalledWith(
                    message: expect.stringContaining('Test message'),
        it('should include class name in log message', () => {
                    message: expect.stringContaining('[TestScheduledJob]')
        it('should pass verboseOnly flag', () => {
            job.testLog('Verbose message', true);
        it('should default verboseOnly to false', () => {
            job.testLog('Default verbose');
    describe('logError', () => {
        it('should call LogError with formatted message', () => {
            job.testLogError('Error occurred');
            expect(mockedLogError).toHaveBeenCalledWith(
                expect.stringContaining('Error occurred'),
        it('should include class name in error message', () => {
            job.testLogError('Something failed');
                expect.stringContaining('[TestScheduledJob]'),
        it('should pass error object to LogError', () => {
            const error = new Error('test error');
            job.testLogError('Failed', error);
                expect.stringContaining('Failed'),
    describe('abstract method implementation', () => {
        it('should have Execute method that returns a ScheduledJobResult', async () => {
                Schedule: {},
            } as ScheduledJobExecutionContext;
            const result = await job.Execute(context);
            expect(result).toHaveProperty('Success');
        it('should have ValidateConfiguration method', () => {
            const schedule = { Configuration: '{}' };
            const result = job.ValidateConfiguration(schedule as Parameters<typeof job.ValidateConfiguration>[0]);
        it('should have FormatNotification method', () => {
            const context = {} as ScheduledJobExecutionContext;
            const jobResult = { Success: true };
            const notification = job.FormatNotification(context, jobResult);
            expect(notification).toHaveProperty('Subject');
            expect(notification).toHaveProperty('Body');
            expect(notification).toHaveProperty('Priority');
