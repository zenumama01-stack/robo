import { describe, it, expect, beforeEach, vi } from 'vitest';
const { mockAgents, mockAgentRun, mockRunAgent } = vi.hoisted(() => {
  const mockAgents = [
    { ID: 'agent-1', Name: 'TestAgent', Description: 'A test agent', TypeID: 'type-1', ParentID: null },
    { ID: 'agent-2', Name: 'OtherAgent', Description: 'Another agent', TypeID: 'type-1', ParentID: 'agent-1' },
    { ID: 'agent-3', Name: 'WildcardMatch', Description: 'Wildcard test', TypeID: 'type-2', ParentID: null },
  const mockAgentRun = {
    ID: 'run-1',
    Agent: 'TestAgent',
    Status: 'Running',
    StartedAt: new Date('2024-01-01'),
    CompletedAt: null as Date | null,
    ErrorMessage: null,
    TotalTokensUsed: 100,
    FinalStep: null,
    Load: vi.fn().mockResolvedValue(true),
    Save: vi.fn().mockResolvedValue(true),
  const mockRunAgent = vi.fn().mockResolvedValue({
    agentRun: {
      Status: 'Completed',
      StartedAt: new Date(),
      CompletedAt: new Date(),
      FinalStep: 'done',
    payload: { result: 'success' },
  return { mockAgents, mockAgentRun, mockRunAgent };
vi.mock('@memberjunction/core', () => {
  class MockMetadata {
    async GetEntityObject() { return mockAgentRun; }
    Metadata: MockMetadata,
    UserInfo: class {},
vi.mock('@memberjunction/aiengine', () => ({
  AIEngine: {
    Instance: {
      Config: vi.fn().mockResolvedValue(undefined),
      Agents: mockAgents,
vi.mock('@memberjunction/ai-core-plus', () => ({
  AIAgentEntityExtended: class {},
  AIAgentRunEntityExtended: class {},
vi.mock('@memberjunction/ai-agents', () => {
    AgentRunner: class {
      RunAgent = mockRunAgent;
vi.mock('@memberjunction/ai', () => ({
  ChatMessage: class {},
import { AgentOperations } from '../AgentOperations';
describe('AgentOperations', () => {
  let ops: AgentOperations;
  const mockUser = { ID: 'user-1', Email: 'test@test.com' } as never;
  beforeEach(() => {
    vi.clearAllMocks();
    mockAgentRun.Status = 'Running';
    mockAgentRun.Load.mockResolvedValue(true);
    mockAgentRun.Save.mockResolvedValue(true);
    ops = new AgentOperations(mockUser);
  describe('discoverAgents', () => {
    it('should return all agents when pattern is *', async () => {
      const result = await ops.discoverAgents({ pattern: '*' });
      expect(result.success).toBe(true);
      expect(result.result).toHaveLength(3);
    it('should return all agents when no pattern provided', async () => {
      const result = await ops.discoverAgents({});
    it('should find agent by exact name', async () => {
      const result = await ops.discoverAgents({ pattern: 'TestAgent' });
      expect(result.result).toHaveLength(1);
      expect(result.result[0].name).toBe('TestAgent');
    it('should return empty array when no agents match', async () => {
      const result = await ops.discoverAgents({ pattern: 'NonExistent' });
      expect(result.result).toHaveLength(0);
    it('should map agent fields correctly', async () => {
      const agent = result.result[0];
      expect(agent.id).toBe('agent-1');
      expect(agent.name).toBe('TestAgent');
      expect(agent.description).toBe('A test agent');
      expect(agent.typeID).toBe('type-1');
  describe('executeAgent', () => {
    it('should execute agent by ID', async () => {
      const result = await ops.executeAgent({ agentId: 'agent-1' });
      expect(result.result.agentName).toBe('TestAgent');
    it('should execute agent by name (case-insensitive)', async () => {
      const result = await ops.executeAgent({ agentName: 'testagent' });
    it('should fail when neither agentId nor agentName provided', async () => {
      const result = await ops.executeAgent({});
      expect(result.success).toBe(false);
      expect(result.errorMessage).toContain('Either agentId or agentName must be provided');
    it('should fail when agent ID not found', async () => {
      const result = await ops.executeAgent({ agentId: 'non-existent' });
      expect(result.errorMessage).toContain('Agent not found with ID');
    it('should fail when agent name not found', async () => {
      const result = await ops.executeAgent({ agentName: 'NonExistent' });
      expect(result.errorMessage).toContain('Agent not found with name');
    it('should pass conversation history to agent runner', async () => {
      const messages = [{ role: 'user', content: 'hello' }];
      await ops.executeAgent({ agentId: 'agent-1', conversationHistory: messages });
      expect(mockRunAgent).toHaveBeenCalledWith(
        expect.objectContaining({
  describe('getAgentRunStatus', () => {
    it('should return run status for valid run ID', async () => {
      const result = await ops.getAgentRunStatus({ runId: 'run-1' });
      expect(result.result.runId).toBe('run-1');
      expect(result.result.status).toBe('Running');
    it('should fail when runId not provided', async () => {
      const result = await ops.getAgentRunStatus({});
      expect(result.errorMessage).toBe('Run ID is required');
    it('should fail when run not found', async () => {
      mockAgentRun.Load.mockResolvedValue(false);
      const result = await ops.getAgentRunStatus({ runId: 'missing' });
      expect(result.errorMessage).toContain('Run not found');
  describe('cancelAgentRun', () => {
    it('should cancel a running agent', async () => {
      const result = await ops.cancelAgentRun({ runId: 'run-1' });
      expect(result.result.cancelled).toBe(true);
      const result = await ops.cancelAgentRun({});
      const result = await ops.cancelAgentRun({ runId: 'missing' });
    it('should fail when run is not in Running status', async () => {
      mockAgentRun.Status = 'Completed';
      expect(result.errorMessage).toContain('Cannot cancel run with status');
    it('should fail when save fails', async () => {
      mockAgentRun.Save.mockResolvedValue(false);
      expect(result.errorMessage).toContain('Failed to update run status');
  describe('processOperation', () => {
    it('should route discoverAgents operation', async () => {
      const result = await ops.processOperation('discoverAgents', {});
    it('should route executeAgent operation', async () => {
      const result = await ops.processOperation('executeAgent', { agentId: 'agent-1' });
    it('should route getAgentRunStatus operation', async () => {
      const result = await ops.processOperation('getAgentRunStatus', { runId: 'run-1' });
    it('should route cancelAgentRun operation', async () => {
      const result = await ops.processOperation('cancelAgentRun', { runId: 'run-1' });
    it('should return error for unsupported operation', async () => {
      const result = await ops.processOperation('unknownOp', {});
      expect(result.errorMessage).toContain('Unsupported agent operation');
