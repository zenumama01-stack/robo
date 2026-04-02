    Metadata: class MockMetadata {
            Name: 'Test Agent'
        Name = 'Test Agent';
vi.mock('@memberjunction/ai-agents', () => ({
    AgentRunner: class MockAgentRunner {
        RunAgent = vi.fn().mockResolvedValue({
                ID: 'agent-run-1',
                TotalCost: 0.005,
                ConversationID: 'conv-1',
                ScheduledJobRunID: null,
    AgentJobConfiguration: class {}
import { AgentScheduledJobDriver } from '../drivers/AgentScheduledJobDriver';
describe('AgentScheduledJobDriver', () => {
    let driver: AgentScheduledJobDriver;
        driver = new AgentScheduledJobDriver();
        it('should return success for valid configuration with AgentID', () => {
                Configuration: JSON.stringify({ AgentID: 'agent-123' })
        it('should fail when AgentID is missing', () => {
            expect(result.Errors[0].Source).toBe('Configuration.AgentID');
                Configuration: 'invalid-json'
        it('should fail when Configuration is null', () => {
        it('should accept valid StartingPayload', () => {
                    AgentID: 'agent-123',
                    StartingPayload: { key: 'value' }
        it('should accept configuration with optional fields', () => {
                    ConversationID: 'conv-456',
                    InitialMessage: 'Hello agent',
                    ConfigurationID: 'config-789',
                    OverrideModelID: 'model-abc'
        it('should fail when Configuration is empty string', () => {
            const schedule = { Configuration: '' };
        it('should format success notification with agent details', () => {
                Schedule: {
                    ID: 'schedule-1',
                    Name: 'Agent Job',
                    Configuration: JSON.stringify({ AgentID: 'agent-1' })
                Details: {
                    AgentRunID: 'run-1',
                    TokensUsed: 500,
                    Cost: 0.01
            expect(notification.Subject).toContain('Agent Job');
            expect(notification.Body).toContain('500');
                    ID: 'schedule-2',
                    Name: 'Failed Agent Job',
                    Configuration: JSON.stringify({ AgentID: 'agent-2' })
                ErrorMessage: 'Agent crashed',
                    AgentRunID: 'run-fail'
            expect(notification.Body).toContain('Agent crashed');
        it('should include metadata with AgentID', () => {
                    ID: 'schedule-3',
                    Name: 'Metadata Test',
                    Configuration: JSON.stringify({ AgentID: 'agent-meta' })
                Details: { AgentRunID: 'run-meta' }
            expect(notification.Metadata.JobType).toBe('Agent');
            expect(notification.Metadata.AgentID).toBe('agent-meta');
        it('should handle missing details gracefully in success notification', () => {
                    ID: 'schedule-4',
                    Name: 'No Details Job',
                    Configuration: JSON.stringify({ AgentID: 'agent-nd' })
            expect(notification.Body).toContain('N/A');
    describe('Execute', () => {
        it('should execute an agent and return success result', async () => {
                    ID: 'schedule-exec',
                    Configuration: JSON.stringify({ AgentID: 'agent-exec' })
                Run: { ID: 'run-exec' },
                ContextUser: { ID: 'user-1' }
            const result = await driver.Execute(
                context as Parameters<typeof driver.Execute>[0]
            expect(result.Details).toBeDefined();
            expect(result.Details.AgentRunID).toBe('agent-run-1');
            expect(result.Details.TokensUsed).toBe(100);
