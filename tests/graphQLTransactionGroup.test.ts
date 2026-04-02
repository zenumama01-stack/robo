  class MockTransactionGroupBase {
    PendingTransactions: Array<{
      BaseEntity: { EntityInfo: { Name: string }; GetDataObjectJSON: () => Promise<string> };
      OperationType: string;
    Variables: Array<{
    MapVariableEntityObjectToPosition(variable: Record<string, unknown>): number {
    TransactionGroupBase: MockTransactionGroupBase,
    TransactionResult: class {
      Item: unknown;
      Result: unknown;
      constructor(item: unknown, result: unknown, success: boolean) {
        this.Item = item;
vi.mock('../graphQLDataProvider', () => ({
  GraphQLDataProvider: vi.fn(),
import { GraphQLTransactionGroup } from '../graphQLTransactionGroup';
describe('GraphQLTransactionGroup', () => {
  let mockProvider: { ExecuteGQL: ReturnType<typeof vi.fn> };
    mockProvider = {
      ExecuteGQL: vi.fn(),
  it('should create an instance with a provider', () => {
    const group = new GraphQLTransactionGroup(mockProvider as never);
    expect(group).toBeInstanceOf(GraphQLTransactionGroup);
  it('should throw when ExecuteTransactionGroup fails', async () => {
    mockProvider.ExecuteGQL.mockResolvedValue(null);
    // Access the protected method through type assertion
    const handleSubmit = (group as Record<string, Function>)['HandleSubmit'].bind(group);
    await expect(handleSubmit()).rejects.toThrow('Failed to execute transaction group');
  it('should process transaction results when server responds', async () => {
    const mockResults = {
      ExecuteTransactionGroup: {
        ErrorMessages: [],
        ResultsJSON: [
          JSON.stringify({ ID: '123', Name: 'Created Entity' }),
          JSON.stringify({ ID: '456', Name: 'Updated Entity' }),
    mockProvider.ExecuteGQL.mockResolvedValue(mockResults);
    // Set up pending transactions
    group.PendingTransactions = [
          EntityInfo: { Name: 'MJTestEntity' },
          GetDataObjectJSON: vi.fn().mockResolvedValue('{}'),
        OperationType: 'Create',
        OperationType: 'Update',
    const results = await handleSubmit();
    expect(mockProvider.ExecuteGQL).toHaveBeenCalledOnce();
  it('should build correct variables for ExecuteGQL', async () => {
        ResultsJSON: [JSON.stringify({ ID: '123' })],
          EntityInfo: { Name: 'Users' },
          GetDataObjectJSON: vi.fn().mockResolvedValue('{"Name":"Test User"}'),
    await handleSubmit();
    const callArgs = mockProvider.ExecuteGQL.mock.calls[0];
    const vars = callArgs[1];
    expect(vars.group).toBeDefined();
    expect(vars.group.Items).toHaveLength(1);
    expect(vars.group.Items[0].EntityName).toBe('Users');
    expect(vars.group.Items[0].OperationType).toBe('Create');
  it('should handle empty pending transactions', async () => {
        ResultsJSON: [],
    group.PendingTransactions = [];
  it('should pass variables through correctly', async () => {
    group.Variables = [
      { Name: 'testVar', FieldName: 'TargetID', Type: 'output' },
    expect(vars.group.Variables).toHaveLength(1);
    expect(vars.group.Variables[0].Name).toBe('testVar');
    expect(vars.group.Variables[0].FieldName).toBe('TargetID');
    expect(vars.group.Variables[0].Type).toBe('output');
