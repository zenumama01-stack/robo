 * @fileoverview Type tests for MCP Client types
 * These tests verify that types are properly exported and can be used correctly.
    MCPClientEventType
} from '../types.js';
describe('Type exports', () => {
    describe('Transport types', () => {
        it('should allow valid transport types', () => {
            const types: MCPTransportType[] = ['StreamableHTTP', 'SSE', 'Stdio', 'WebSocket'];
            expect(types).toHaveLength(4);
    describe('Auth types', () => {
        it('should allow valid auth types', () => {
            const types: MCPAuthType[] = ['None', 'Bearer', 'APIKey', 'OAuth2', 'Basic', 'Custom'];
            expect(types).toHaveLength(6);
    describe('Status types', () => {
        it('should allow valid server status types', () => {
            const types: MCPServerStatus[] = ['Active', 'Inactive', 'Deprecated'];
            expect(types).toHaveLength(3);
        it('should allow valid connection status types', () => {
            const types: MCPConnectionStatus[] = ['Active', 'Inactive', 'Error'];
        it('should allow valid tool status types', () => {
            const types: MCPToolStatus[] = ['Active', 'Inactive', 'Deprecated'];
    describe('MCPServerConfig', () => {
        it('should accept valid server config', () => {
            const config: MCPServerConfig = {
                ID: 'test-id',
                Name: 'Test Server',
                TransportType: 'StreamableHTTP',
                DefaultAuthType: 'APIKey',
                ServerURL: 'https://example.com/mcp'
            expect(config.ID).toBe('test-id');
            expect(config.TransportType).toBe('StreamableHTTP');
        it('should accept stdio config', () => {
                TransportType: 'Stdio',
                DefaultAuthType: 'None',
                Command: '/usr/bin/mcp-server',
                CommandArgs: JSON.stringify(['--port', '3000'])
            expect(config.Command).toBe('/usr/bin/mcp-server');
    describe('MCPConnectionConfig', () => {
        it('should accept valid connection config', () => {
            const config: MCPConnectionConfig = {
                ID: 'conn-id',
                MCPServerID: 'server-id',
                Name: 'Test Connection',
                CompanyID: 'company-id',
                AutoSyncTools: true,
                AutoGenerateActions: false,
                LogToolCalls: true,
                LogInputParameters: true,
                LogOutputContent: true,
                MaxOutputLogSize: 102400
            expect(config.LogToolCalls).toBe(true);
    describe('MCPToolDefinition', () => {
        it('should accept valid tool definition', () => {
            const tool: MCPToolDefinition = {
                ID: 'tool-id',
                ToolName: 'test_tool',
                ToolTitle: 'Test Tool',
                ToolDescription: 'A test tool',
                InputSchema: JSON.stringify({ type: 'object', properties: {} }),
                Status: 'Active'
            expect(tool.ToolName).toBe('test_tool');
    describe('MCPCallToolOptions', () => {
        it('should accept valid call options', () => {
            const options: MCPCallToolOptions = {
                arguments: {
                    param1: 'value1',
                    param2: 123,
                    param3: { nested: true }
                timeout: 30000
            expect(options.arguments.param1).toBe('value1');
    describe('Result types', () => {
        it('should accept valid tool call result', () => {
            const result: MCPToolCallResult = {
                content: [{ type: 'text', text: 'Hello, world!' }],
                durationMs: 150,
            expect(result.content[0].text).toBe('Hello, world!');
        it('should accept valid list tools result', () => {
            const result: MCPListToolsResult = {
                        inputSchema: { type: 'object' }
            expect(result.tools).toHaveLength(1);
        it('should accept valid sync tools result', () => {
            const result: MCPSyncToolsResult = {
                added: 5,
                updated: 2,
                deprecated: 1,
                total: 6
            expect(result.added).toBe(5);
        it('should accept valid test connection result', () => {
                serverName: 'Test Server',
                serverVersion: '1.0.0',
                latencyMs: 50
            expect(result.serverName).toBe('Test Server');
    describe('Client options', () => {
        it('should accept valid client options', () => {
            const options: MCPClientOptions = {
                contextUser: {
                    ID: 'user-id',
                } as MCPClientOptions['contextUser']
            expect(options.contextUser.ID).toBe('user-id');
        it('should accept valid connect options', () => {
            const options: MCPConnectOptions = {
                } as MCPConnectOptions['contextUser'],
                forceReconnect: true,
                skipAutoSync: false
            expect(options.forceReconnect).toBe(true);
    describe('Event types', () => {
        it('should allow valid event types', () => {
            const types: MCPClientEventType[] = [
                'connected',
                'disconnected',
                'toolCalled',
                'toolCallCompleted',
                'toolsSynced',
                'connectionError',
                'rateLimitExceeded'
            expect(types).toHaveLength(7);
import type { CodeExecutionParams, CodeExecutionResult, JavaScriptExecutionOptions, SandboxContext } from '../types';
 * Type-level tests to verify the shape of exported interfaces.
 * These tests validate that types are properly structured and compile correctly.
 * Since interfaces have no runtime representation, we test by creating conforming objects.
describe('Types', () => {
  describe('CodeExecutionParams', () => {
    it('should require code and language', () => {
        code: 'console.log("hello")',
      expect(params.code).toBe('console.log("hello")');
      expect(params.language).toBe('javascript');
    it('should allow optional inputData', () => {
        code: 'output = input.x;',
        inputData: { x: 42 }
      expect(params.inputData).toEqual({ x: 42 });
    it('should allow optional timeoutSeconds', () => {
      expect(params.timeoutSeconds).toBe(10);
    it('should allow optional memoryLimitMB', () => {
      expect(params.memoryLimitMB).toBe(64);
    it('should allow all optional fields together', () => {
        code: 'output = input.count * 2;',
        inputData: { count: 5 },
        timeoutSeconds: 15,
        memoryLimitMB: 256
      expect(params.code).toBeDefined();
      expect(params.inputData).toBeDefined();
      expect(params.timeoutSeconds).toBe(15);
      expect(params.memoryLimitMB).toBe(256);
  describe('CodeExecutionResult', () => {
    it('should represent a successful result', () => {
      const result: CodeExecutionResult = {
        output: 42,
        logs: ['[LOG] Hello'],
        executionTimeMs: 150
      expect(result.output).toBe(42);
      expect(result.logs).toHaveLength(1);
    it('should represent a failed result with error', () => {
        error: 'Something went wrong',
    it('should support all error types', () => {
      const errorTypes: CodeExecutionResult['errorType'][] = [
        'TIMEOUT',
        'MEMORY_LIMIT',
        'SYNTAX_ERROR',
        'RUNTIME_ERROR',
        'SECURITY_ERROR'
      for (const errorType of errorTypes) {
          error: `Error of type ${errorType}`,
          errorType
        expect(result.errorType).toBe(errorType);
    it('should allow minimal success result', () => {
      expect(result.output).toBeUndefined();
      expect(result.logs).toBeUndefined();
    it('should allow output with various data types', () => {
      const stringResult: CodeExecutionResult = { success: true, output: 'hello' };
      expect(stringResult.output).toBe('hello');
      const numberResult: CodeExecutionResult = { success: true, output: 3.14 };
      expect(numberResult.output).toBe(3.14);
      const objectResult: CodeExecutionResult = { success: true, output: { key: 'value' } };
      expect(objectResult.output).toEqual({ key: 'value' });
      const arrayResult: CodeExecutionResult = { success: true, output: [1, 2, 3] };
      expect(arrayResult.output).toEqual([1, 2, 3]);
      const nullResult: CodeExecutionResult = { success: true, output: null };
      expect(nullResult.output).toBeNull();
  describe('JavaScriptExecutionOptions', () => {
    it('should require timeout and memoryLimit', () => {
      const options: JavaScriptExecutionOptions = {
        timeout: 30,
        memoryLimit: 128
      expect(options.timeout).toBe(30);
      expect(options.memoryLimit).toBe(128);
    it('should allow optional allowedLibraries', () => {
        memoryLimit: 128,
        allowedLibraries: ['lodash', 'uuid']
      expect(options.allowedLibraries).toEqual(['lodash', 'uuid']);
  describe('SandboxContext', () => {
    it('should define input, output, and console', () => {
      const context: SandboxContext = {
        input: { data: [1, 2, 3] },
        output: null,
        console: {
          log: () => {},
          error: () => {},
          warn: () => {},
          info: () => {}
      expect(context.input).toBeDefined();
      expect(context.console.log).toBeDefined();
      expect(context.console.error).toBeDefined();
      expect(context.console.warn).toBeDefined();
      expect(context.console.info).toBeDefined();
  BaseFormComponentEventCodes,
  BaseFormComponentEvent,
  FormEditingCompleteEvent,
  PendingRecordItem
} from '../types';
describe('BaseFormComponentEventCodes', () => {
  it('should define BASE_CODE constant', () => {
    expect(BaseFormComponentEventCodes.BASE_CODE).toBe('BaseFormComponent_Event');
  it('should define EDITING_COMPLETE constant', () => {
    expect(BaseFormComponentEventCodes.EDITING_COMPLETE).toBe('EDITING_COMPLETE');
  it('should define REVERT_PENDING_CHANGES constant', () => {
    expect(BaseFormComponentEventCodes.REVERT_PENDING_CHANGES).toBe('REVERT_PENDING_CHANGES');
  it('should define POPULATE_PENDING_RECORDS constant', () => {
    expect(BaseFormComponentEventCodes.POPULATE_PENDING_RECORDS).toBe('POPULATE_PENDING_RECORDS');
describe('BaseFormComponentEvent', () => {
  it('should create an instance with required properties', () => {
    const event = new BaseFormComponentEvent();
    expect(event).toBeInstanceOf(BaseFormComponentEvent);
    expect(event.subEventCode).toBeUndefined();
    expect(event.elementRef).toBeUndefined();
    expect(event.returnValue).toBeUndefined();
  it('should allow setting properties', () => {
    event.subEventCode = 'TEST_CODE';
    event.elementRef = { nativeElement: {} };
    event.returnValue = 42;
    expect(event.subEventCode).toBe('TEST_CODE');
    expect(event.returnValue).toBe(42);
describe('FormEditingCompleteEvent', () => {
  it('should create an instance extending BaseFormComponentEvent', () => {
    expect(event).toBeInstanceOf(FormEditingCompleteEvent);
  it('should have EDITING_COMPLETE as default subEventCode', () => {
    expect(event.subEventCode).toBe(BaseFormComponentEventCodes.EDITING_COMPLETE);
  it('should initialize pendingChanges as empty array', () => {
    expect(event.pendingChanges).toEqual([]);
    expect(Array.isArray(event.pendingChanges)).toBe(true);
describe('PendingRecordItem', () => {
  it('should create with default save action', () => {
    const item = new PendingRecordItem();
    expect(item.action).toBe('save');
  it('should allow setting action to delete', () => {
    item.action = 'delete';
    expect(item.action).toBe('delete');
  DEFAULT_CARD_CONFIG,
  DEFAULT_VIRTUAL_SCROLL_CONFIG,
  DEFAULT_VIRTUAL_SCROLL_STATE
} from '../lib/types';
  TimelineLayout,
  TimelineSortOrder,
  TimeSegmentGrouping,
  TimelineCardConfig,
  VirtualScrollConfig,
  TimelineAction,
  TimelineDisplayField,
  MJTimelineEvent,
  TimelineSegment
describe('DEFAULT_CARD_CONFIG', () => {
  it('should have showIcon true', () => {
    expect(DEFAULT_CARD_CONFIG.showIcon).toBe(true);
  it('should have showDate true', () => {
    expect(DEFAULT_CARD_CONFIG.showDate).toBe(true);
  it('should have showSubtitle true', () => {
    expect(DEFAULT_CARD_CONFIG.showSubtitle).toBe(true);
  it('should have default date format', () => {
    expect(DEFAULT_CARD_CONFIG.dateFormat).toBe('MMM d, yyyy');
  it('should default to collapsible', () => {
    expect(DEFAULT_CARD_CONFIG.collapsible).toBe(true);
    expect(DEFAULT_CARD_CONFIG.defaultExpanded).toBe(false);
  it('should default descriptionMaxLines to 3', () => {
    expect(DEFAULT_CARD_CONFIG.descriptionMaxLines).toBe(3);
  it('should not allow HTML description by default', () => {
    expect(DEFAULT_CARD_CONFIG.allowHtmlDescription).toBe(false);
  it('should have maxWidth of 400px', () => {
    expect(DEFAULT_CARD_CONFIG.maxWidth).toBe('400px');
describe('DEFAULT_VIRTUAL_SCROLL_CONFIG', () => {
  it('should be enabled by default', () => {
    expect(DEFAULT_VIRTUAL_SCROLL_CONFIG.enabled).toBe(true);
  it('should have batchSize of 20', () => {
    expect(DEFAULT_VIRTUAL_SCROLL_CONFIG.batchSize).toBe(20);
  it('should have loadThreshold of 200', () => {
    expect(DEFAULT_VIRTUAL_SCROLL_CONFIG.loadThreshold).toBe(200);
  it('should show loading indicator', () => {
    expect(DEFAULT_VIRTUAL_SCROLL_CONFIG.showLoadingIndicator).toBe(true);
describe('DEFAULT_VIRTUAL_SCROLL_STATE', () => {
  it('should start with zero loaded count', () => {
    expect(DEFAULT_VIRTUAL_SCROLL_STATE.loadedCount).toBe(0);
  it('should have hasMore as false', () => {
    expect(DEFAULT_VIRTUAL_SCROLL_STATE.hasMore).toBe(false);
  it('should not be loading', () => {
    expect(DEFAULT_VIRTUAL_SCROLL_STATE.isLoading).toBe(false);
  it('should have zero scroll offset', () => {
    expect(DEFAULT_VIRTUAL_SCROLL_STATE.scrollOffset).toBe(0);
describe('type correctness', () => {
  it('should accept valid TimelineOrientation values', () => {
    const v: TimelineOrientation = 'vertical';
    const h: TimelineOrientation = 'horizontal';
    expect(v).toBe('vertical');
    expect(h).toBe('horizontal');
  it('should accept valid TimelineLayout values', () => {
    const s: TimelineLayout = 'single';
    const a: TimelineLayout = 'alternating';
    expect(s).toBe('single');
    expect(a).toBe('alternating');
  it('should accept valid TimeSegmentGrouping values', () => {
    const groups: TimeSegmentGrouping[] = ['none', 'day', 'week', 'month', 'quarter', 'year'];
    expect(groups).toHaveLength(6);
  it('should construct TimelineAction', () => {
    const action: TimelineAction = {
      id: 'view',
      label: 'View',
      icon: 'fa-solid fa-eye',
      variant: 'primary',
      tooltip: 'View details',
      disabled: false
    expect(action.id).toBe('view');
    expect(action.variant).toBe('primary');
  it('should construct TimelineDisplayField', () => {
    const field: TimelineDisplayField = {
      fieldName: 'Status',
      label: 'Status',
      icon: 'fa-solid fa-circle',
      formatter: (v) => String(v).toUpperCase()
    expect(field.formatter!('active')).toBe('ACTIVE');
  it('should construct MJTimelineEvent', () => {
    const event: MJTimelineEvent<{ Name: string }> = {
      entity: { Name: 'Test' },
      title: 'Test Event',
      date: new Date(),
      config: {},
      groupIndex: 0,
    expect(event.entity.Name).toBe('Test');
  it('should construct TimelineSegment', () => {
    const segment: TimelineSegment = {
      label: 'January 2026',
      startDate: new Date(2026, 0, 1),
      endDate: new Date(2026, 1, 1),
      events: [],
      isExpanded: true,
      eventCount: 0
    expect(segment.label).toBe('January 2026');
    expect(segment.isExpanded).toBe(true);
import type { MicroViewData, FieldChangeView, SlidePanelMode } from '../lib/types';
describe('MicroViewData type', () => {
    const data: MicroViewData = {
      RecordID: 'rec-1',
      RecordChangeID: 'change-1',
      FullRecordJSON: { Name: 'John', Email: 'john@test.com' },
      FieldDiffs: null
    expect(data.EntityName).toBe('Users');
    expect(data.RecordID).toBe('rec-1');
    expect(data.FullRecordJSON).toEqual({ Name: 'John', Email: 'john@test.com' });
  it('should accept null FullRecordJSON', () => {
      FullRecordJSON: null,
      FieldDiffs: []
    expect(data.FullRecordJSON).toBeNull();
describe('FieldChangeView type', () => {
  it('should represent a modified field', () => {
    const change: FieldChangeView = {
      FieldName: 'Email',
      OldValue: 'old@test.com',
      NewValue: 'new@test.com',
      ChangeType: 'Modified'
    expect(change.ChangeType).toBe('Modified');
  it('should represent an added field', () => {
      FieldName: 'Phone',
      OldValue: '',
      NewValue: '+1-555-0123',
      ChangeType: 'Added'
    expect(change.ChangeType).toBe('Added');
  it('should represent a removed field', () => {
      FieldName: 'Notes',
      OldValue: 'Some notes',
      NewValue: '',
      ChangeType: 'Removed'
    expect(change.ChangeType).toBe('Removed');
describe('SlidePanelMode type', () => {
  it('should accept valid modes', () => {
    const slide: SlidePanelMode = 'slide';
    const dialog: SlidePanelMode = 'dialog';
    expect(slide).toBe('slide');
    expect(dialog).toBe('dialog');
 * Unit tests for Credentials/Engine type definitions
    TwilioCredentialValues,
describe('Credential Types', () => {
    describe('CredentialResolutionOptions', () => {
            const options: CredentialResolutionOptions = {};
            expect(options.credentialId).toBeUndefined();
            const options: CredentialResolutionOptions = {
                credentialId: 'cred-1',
                credentialName: 'My Key',
                directValues: { apiKey: 'sk-123' },
                subsystem: 'AI',
            expect(options.credentialId).toBe('cred-1');
            expect(options.credentialName).toBe('My Key');
            expect(options.directValues).toEqual({ apiKey: 'sk-123' });
            expect(options.subsystem).toBe('AI');
    describe('ResolvedCredential', () => {
        it('should accept a direct values resolution', () => {
            const resolved: ResolvedCredential = {
                values: { apiKey: 'sk-test' },
                source: 'request',
            expect(resolved.credential).toBeNull();
            expect(resolved.values.apiKey).toBe('sk-test');
            expect(resolved.source).toBe('request');
        it('should accept a database source resolution', () => {
                values: { username: 'admin', password: 'secret' },
                source: 'database',
                expiresAt: new Date('2025-12-31'),
            expect(resolved.source).toBe('database');
            expect(resolved.expiresAt).toBeInstanceOf(Date);
        it('should accept typed credential values', () => {
            const resolved: ResolvedCredential<APIKeyCredentialValues> = {
                values: { apiKey: 'sk-typed' },
            expect(resolved.values.apiKey).toBe('sk-typed');
    describe('StoreCredentialOptions', () => {
        it('should accept empty options', () => {
            const options: StoreCredentialOptions = {};
            expect(options.isDefault).toBeUndefined();
            const options: StoreCredentialOptions = {
                categoryId: 'cat-1',
                iconClass: 'fa-key',
                description: 'Test credential',
            expect(options.isDefault).toBe(true);
            expect(options.categoryId).toBe('cat-1');
    describe('CredentialValidationResult', () => {
            const result: CredentialValidationResult = {
                validatedAt: new Date(),
            expect(result.isValid).toBe(true);
            expect(result.errors).toHaveLength(0);
        it('should represent an invalid result with errors', () => {
                errors: ['Missing required field: apiKey'],
                warnings: ['Credential expires soon'],
            expect(result.isValid).toBe(false);
            expect(result.warnings).toHaveLength(1);
    describe('CredentialAccessDetails', () => {
        it('should represent a successful operation', () => {
            const details: CredentialAccessDetails = {
                subsystem: 'AIEngine',
                durationMs: 42,
            expect(details.operation).toBe('Decrypt');
            expect(details.success).toBe(true);
        it('should represent a failed operation', () => {
                errorMessage: 'Schema mismatch',
            expect(details.success).toBe(false);
            expect(details.errorMessage).toBe('Schema mismatch');
        it('should accept all operation types', () => {
            const operations: CredentialAccessDetails['operation'][] = [
                'Decrypt', 'Create', 'Update', 'Delete', 'Validate'
            for (const op of operations) {
                const details: CredentialAccessDetails = { operation: op, success: true };
                expect(details.operation).toBe(op);
    describe('Common Credential Value Interfaces', () => {
        it('should validate APIKeyCredentialValues', () => {
            const cred: APIKeyCredentialValues = { apiKey: 'sk-test' };
            expect(cred.apiKey).toBe('sk-test');
        it('should validate APIKeyWithEndpointCredentialValues', () => {
            const cred: APIKeyWithEndpointCredentialValues = {
                apiKey: 'sk-test',
                endpoint: 'https://api.example.com',
            expect(cred.endpoint).toBe('https://api.example.com');
        it('should validate OAuth2ClientCredentialValues', () => {
            const cred: OAuth2ClientCredentialValues = {
                clientSecret: 'secret-1',
                tokenUrl: 'https://auth.example.com/token',
                scope: 'read write',
            expect(cred.clientId).toBe('client-1');
        it('should validate BasicAuthCredentialValues', () => {
            const cred: BasicAuthCredentialValues = {
                username: 'admin',
                password: 'secret',
            expect(cred.username).toBe('admin');
        it('should validate AzureServicePrincipalCredentialValues', () => {
            const cred: AzureServicePrincipalCredentialValues = {
                tenantId: 'tenant-1',
            expect(cred.tenantId).toBe('tenant-1');
        it('should validate AWSIAMCredentialValues', () => {
            const cred: AWSIAMCredentialValues = {
                accessKeyId: 'AKIA123',
                secretAccessKey: 'secret',
                region: 'us-east-1',
            expect(cred.region).toBe('us-east-1');
        it('should validate DatabaseConnectionCredentialValues', () => {
            const cred: DatabaseConnectionCredentialValues = {
                database: 'mydb',
                username: 'root',
                password: 'password',
            expect(cred.port).toBe(5432);
        it('should validate TwilioCredentialValues', () => {
            const cred: TwilioCredentialValues = {
                accountSid: 'AC123',
                authToken: 'token123',
            expect(cred.accountSid).toBe('AC123');
    PrimaryKey = { ToString: () => 'pk-123' };
    EntityInfo = { ID: 'ent-1', Name: 'MJTestEntity', Fields: [] };
  DataObjectRelatedEntityParam: class {},
  EntityInfo: class { ID = ''; Name = ''; Fields = []; RelatedEntities = []; },
  Metadata: class { Entities = []; Provider = {}; },
  QueryInfo: class { ID = ''; Name = ''; Fields = []; SQL = ''; },
  RunQuery: class { RunQuery = vi.fn() },
  CompositeKey: class { KeyValuePairs: unknown[] = [] },
  IRunViewProvider: class {},
  MJDataContextEntity: class { ID = ''; },
  MJDataContextItemEntity: class {
    ViewID = '';
    QueryID = '';
    SQL = '';
    CodeName = '';
    DataJSON = '';
  MJDataContextItemEntityType: class {},
  UserViewEntityExtended: class {
    ViewEntityInfo = { ID: '', Name: '', Fields: [], SchemaName: '', BaseView: '' };
    WhereClause = '';
        CreateInstance: () => new (DataContextItemClass)(),
// Need to get reference to the actual class after import
let DataContextItemClass: { new(): Record<string, unknown> };
import { DataContextFieldInfo, DataContextItem, DataContext } from '../types';
describe('MJDataContext', () => {
  describe('DataContextFieldInfo', () => {
    it('should create a field info object', () => {
      const field = new DataContextFieldInfo();
      field.Name = 'TestField';
      field.Type = 'nvarchar';
      field.Description = 'A test field';
      expect(field.Type).toBe('nvarchar');
  describe('DataContextItem', () => {
    it('should have correct default state', () => {
      const item = new DataContextItem();
      expect(item.DataLoaded).toBe(false);
      expect(item.Fields).toEqual([]);
    it('should set DataLoaded when Data is assigned', () => {
      item.Data = [{ id: 1 }];
      expect(item.DataLoaded).toBe(true);
      expect(item.DataLoadingError).toBeNull();
    it('should generate correct Description for view type', () => {
      item.Type = 'view';
      item.RecordName = 'Active Users';
      item.EntityName = 'MJ: Users';
      expect(item.Description).toBe('View: Active Users, From Entity: MJ: Users');
    it('should generate correct Description for query type', () => {
      item.Type = 'query';
      item.RecordName = 'GetTopProducts';
      expect(item.Description).toBe('Query: GetTopProducts');
    it('should generate correct Description for full_entity type', () => {
      item.Type = 'full_entity';
      item.EntityName = 'Products';
      expect(item.Description).toBe('Full Entity - All Records: Products');
    it('should generate correct Description for sql type', () => {
      item.Type = 'sql';
      item.RecordName = 'Custom SQL';
      expect(item.Description).toBe('SQL Statement: Custom SQL');
    it('should append AdditionalDescription when present', () => {
      item.RecordName = 'MyQuery';
      item.AdditionalDescription = 'Extra info';
      expect(item.Description).toContain('(More Info: Extra info)');
    it('should validate data exists when Data is set', () => {
      item.Data = [];
      expect(item.ValidateDataExists()).toBe(true);
    it('should fail validation when Data is not set', () => {
      expect(item.ValidateDataExists()).toBe(false);
    it('should skip validation with ignoreFailedLoad when not loaded', () => {
      expect(item.ValidateDataExists(true)).toBe(true);
    it('should load data from object', () => {
      const result = item.LoadDataFromObject([{ id: 1 }, { id: 2 }]);
      expect(item.Data).toHaveLength(2);
    it('should return false when loading null data from object', () => {
      const result = item.LoadDataFromObject(null as unknown as unknown[]);
    it('should throw when calling LoadFromSQL on base class', async () => {
      item.SQL = 'SELECT 1';
      await expect(item.LoadData(null, true)).rejects.toThrow('Not implemented');
  describe('DataContext', () => {
    it('should start with empty Items array', () => {
      const ctx = new DataContext();
      expect(ctx.Items).toEqual([]);
    it('should return true for empty Items array (vacuously valid)', () => {
      // With no items, .some() returns false, so !false = true (vacuously valid)
      expect(ctx.ValidateDataExists()).toBe(true);
    it('should convert to simple object', () => {
      ctx.Items.push(item);
      const result = ctx.ConvertToSimpleObject();
      expect(result).toHaveProperty('data_item_0');
      expect(result.data_item_0).toEqual([{ id: 1 }]);
    it('should convert to simple object with custom prefix', () => {
      const result = ctx.ConvertToSimpleObject('item_');
      expect(result).toHaveProperty('item_0');
    it('should create simple object type definition', () => {
      item.RecordName = 'Q1';
      const typeDef = ctx.CreateSimpleObjectTypeDefinition();
      expect(typeDef).toContain('data_item_0');
      expect(typeDef).toContain('Query: Q1');
    it('should load data from two-dimensional object', () => {
      const item1 = new DataContextItem();
      const item2 = new DataContextItem();
      ctx.Items.push(item1, item2);
      const result = ctx.LoadDataFromObject([[{ a: 1 }], [{ b: 2 }]]);
      expect(ctx.Items[0].Data).toEqual([{ a: 1 }]);
      expect(ctx.Items[1].Data).toEqual([{ b: 2 }]);
    it('should fail LoadDataFromObject with mismatched array length', () => {
      ctx.Items.push(new DataContextItem());
    it('should create DataContext from raw data', async () => {
      const raw = {
        ID: 'ctx-123',
        Items: [],
      const ctx = await DataContext.FromRawData(raw);
      expect(ctx.ID).toBe('ctx-123');
    it('should create DataContext from raw data with null', async () => {
      const ctx = await DataContext.FromRawData(null);
      expect(ctx).toBeDefined();
    DEFAULT_EXPORT_OPTIONS,
    CommonStyles,
    mergeCellStyles,
    CSVOptions,
    JSONOptions,
    WorkbookMetadata,
describe('ExportFormat type', () => {
    it('should include excel format', () => {
        const format: ExportFormat = 'excel';
        expect(format).toBe('excel');
    it('should include csv format', () => {
        const format: ExportFormat = 'csv';
        expect(format).toBe('csv');
    it('should include json format', () => {
        const format: ExportFormat = 'json';
        expect(format).toBe('json');
describe('SamplingMode type', () => {
    it('should support all mode', () => {
        const mode: SamplingMode = 'all';
        expect(mode).toBe('all');
    it('should support top mode', () => {
        const mode: SamplingMode = 'top';
        expect(mode).toBe('top');
    it('should support bottom mode', () => {
        const mode: SamplingMode = 'bottom';
        expect(mode).toBe('bottom');
    it('should support every-nth mode', () => {
        const mode: SamplingMode = 'every-nth';
        expect(mode).toBe('every-nth');
    it('should support random mode', () => {
        const mode: SamplingMode = 'random';
        expect(mode).toBe('random');
describe('DEFAULT_EXPORT_OPTIONS', () => {
    it('should have excel as default format', () => {
        expect(DEFAULT_EXPORT_OPTIONS.format).toBe('excel');
    it('should include headers by default', () => {
        expect(DEFAULT_EXPORT_OPTIONS.includeHeaders).toBe(true);
    it('should have "all" sampling mode by default', () => {
        expect(DEFAULT_EXPORT_OPTIONS.sampling?.mode).toBe('all');
    it('should have "export" as default file name', () => {
        expect(DEFAULT_EXPORT_OPTIONS.fileName).toBe('export');
    it('should have "Sheet1" as default sheet name', () => {
        expect(DEFAULT_EXPORT_OPTIONS.sheetName).toBe('Sheet1');
    it('should have bold headers in styling defaults', () => {
        expect(DEFAULT_EXPORT_OPTIONS.styling?.boldHeaders).toBe(true);
    it('should have freeze header in styling defaults', () => {
        expect(DEFAULT_EXPORT_OPTIONS.styling?.freezeHeader).toBe(true);
    it('should have auto filter in styling defaults', () => {
        expect(DEFAULT_EXPORT_OPTIONS.styling?.autoFilter).toBe(true);
describe('CommonStyles', () => {
    it('should have a bold style', () => {
        expect(CommonStyles.bold.font?.bold).toBe(true);
    it('should have an italic style', () => {
        expect(CommonStyles.italic.font?.italic).toBe(true);
    it('should have a red text style', () => {
        expect(CommonStyles.redText.font?.color).toBe('FF0000');
    it('should have a green text style', () => {
        expect(CommonStyles.greenText.font?.color).toBe('008000');
    it('should have a blue text style', () => {
        expect(CommonStyles.blueText.font?.color).toBe('0000FF');
    it('should have a yellow highlight style', () => {
        expect(CommonStyles.yellowHighlight.fill?.fgColor).toBe('FFFF00');
    it('should have a centered style', () => {
        expect(CommonStyles.centered.alignment?.horizontal).toBe('center');
        expect(CommonStyles.centered.alignment?.vertical).toBe('middle');
    it('should have a currency format style', () => {
        expect(CommonStyles.currency.numFmt).toBe('$#,##0.00');
    it('should have a percentage format style', () => {
        expect(CommonStyles.percentage.numFmt).toBe('0.00%');
    it('should have a date format style', () => {
        expect(CommonStyles.date.numFmt).toBe('yyyy-mm-dd');
    it('should have a dateTime format style', () => {
        expect(CommonStyles.dateTime.numFmt).toBe('yyyy-mm-dd hh:mm:ss');
    it('should have a thin border style', () => {
        expect(CommonStyles.thinBorder.border?.top?.style).toBe('thin');
        expect(CommonStyles.thinBorder.border?.bottom?.style).toBe('thin');
        expect(CommonStyles.thinBorder.border?.left?.style).toBe('thin');
        expect(CommonStyles.thinBorder.border?.right?.style).toBe('thin');
    it('should have a header style with bold white text on blue background', () => {
        expect(CommonStyles.header.font?.bold).toBe(true);
        expect(CommonStyles.header.font?.color).toBe('FFFFFF');
        expect(CommonStyles.header.fill?.fgColor).toBe('4472C4');
describe('mergeCellStyles', () => {
    it('should return empty object when no styles provided', () => {
        const result = mergeCellStyles();
    it('should skip undefined styles', () => {
        const result = mergeCellStyles(undefined, undefined);
    it('should merge font styles', () => {
        const style1: CellStyle = { font: { bold: true } };
        const style2: CellStyle = { font: { italic: true } };
        const result = mergeCellStyles(style1, style2);
        expect(result.font?.bold).toBe(true);
        expect(result.font?.italic).toBe(true);
    it('should merge fill styles', () => {
        const style1: CellStyle = { fill: { pattern: 'solid' } };
        const style2: CellStyle = { fill: { fgColor: 'FF0000' } };
        expect(result.fill?.pattern).toBe('solid');
        expect(result.fill?.fgColor).toBe('FF0000');
    it('should merge alignment styles', () => {
        const style1: CellStyle = { alignment: { horizontal: 'center' } };
        const style2: CellStyle = { alignment: { wrapText: true } };
        expect(result.alignment?.horizontal).toBe('center');
        expect(result.alignment?.wrapText).toBe(true);
    it('should override numFmt with later value', () => {
        const style1: CellStyle = { numFmt: '#,##0' };
        const style2: CellStyle = { numFmt: '$#,##0.00' };
        expect(result.numFmt).toBe('$#,##0.00');
    it('should override font properties with later values', () => {
        const style1: CellStyle = { font: { bold: true, color: 'FF0000' } };
        const style2: CellStyle = { font: { color: '0000FF' } };
        expect(result.font?.color).toBe('0000FF');
    it('should merge three or more styles', () => {
        const style2: CellStyle = { fill: { fgColor: 'FFFF00' } };
        const style3: CellStyle = { numFmt: '0.00%' };
        const result = mergeCellStyles(style1, style2, style3);
        expect(result.fill?.fgColor).toBe('FFFF00');
        expect(result.numFmt).toBe('0.00%');
    it('should merge border styles', () => {
        const style1: CellStyle = { border: { top: { style: 'thin', color: '000000' } } };
        const style2: CellStyle = { border: { bottom: { style: 'thick', color: 'FF0000' } } };
        expect(result.border?.top?.style).toBe('thin');
        expect(result.border?.bottom?.style).toBe('thick');
  type ScheduledJobResult,
  type ScheduledJobConfiguration,
  type AgentJobConfiguration,
  type ActionJobConfiguration,
  type NotificationContent,
  type ScheduledJobRunStatus,
  type ScheduledJobStatus,
  type NotificationChannel,
describe('Scheduling base-types exports', () => {
  describe('ScheduledJobResult', () => {
    it('should allow creating a success result', () => {
      const result: ScheduledJobResult = {
      expect(result.ErrorMessage).toBeUndefined();
    it('should allow creating a failure result with error message', () => {
        ErrorMessage: 'Something went wrong',
        Details: { AgentRunID: 'abc-123' },
      expect(result.ErrorMessage).toBe('Something went wrong');
      expect(result.Details).toHaveProperty('AgentRunID');
  describe('AgentJobConfiguration', () => {
    it('should allow creating an agent job configuration', () => {
      const config: AgentJobConfiguration = {
        AgentID: 'agent-001',
        ConversationID: 'conv-001',
        InitialMessage: 'Hello',
      expect(config.AgentID).toBe('agent-001');
      expect(config.ConversationID).toBe('conv-001');
  describe('ActionJobConfiguration', () => {
    it('should allow creating an action job configuration', () => {
      const config: ActionJobConfiguration = {
        ActionID: 'action-001',
          { ActionParamID: 'p1', ValueType: 'Static', Value: 'hello' },
          { ActionParamID: 'p2', ValueType: 'SQL Statement', Value: 'SELECT 1' },
      expect(config.ActionID).toBe('action-001');
      expect(config.Params).toHaveLength(2);
  describe('NotificationContent', () => {
    it('should allow creating notification content', () => {
      const notif: NotificationContent = {
        Subject: 'Job Complete',
        Body: 'Your job finished.',
        Priority: 'High',
      expect(notif.Subject).toBe('Job Complete');
      expect(notif.Priority).toBe('High');
  describe('Type aliases', () => {
    it('should allow ScheduledJobRunStatus values', () => {
      const statuses: ScheduledJobRunStatus[] = ['Running', 'Completed', 'Failed', 'Cancelled', 'Timeout'];
      expect(statuses).toHaveLength(5);
    it('should allow ScheduledJobStatus values', () => {
      const statuses: ScheduledJobStatus[] = ['Pending', 'Active', 'Paused', 'Disabled', 'Expired'];
    it('should allow NotificationChannel values', () => {
      const channels: NotificationChannel[] = ['Email', 'InApp'];
      expect(channels).toHaveLength(2);
  MJTemplateCategoryEntity: class {},
  MJTemplateContentEntity: class { TemplateID = ''; },
  MJTemplateContentTypeEntity: class {},
  TemplateEntityExtended: class {
    Content: unknown[] = [];
  MJTemplateParamEntity: class { TemplateID = ''; },
import { TemplateRenderResult } from '../types';
import { TemplateEngineBase } from '../TemplateEngineBase';
describe('Templates/base-types exports', () => {
  describe('TemplateRenderResult', () => {
      const result = new TemplateRenderResult();
      result.Output = '<h1>Hello</h1>';
      expect(result.Output).toBe('<h1>Hello</h1>');
    it('should allow creating a failure result with message', () => {
      result.Output = '';
      result.Message = 'Template rendering failed';
      expect(result.Message).toBe('Template rendering failed');
  describe('TemplateEngineBase', () => {
    it('should be a class that can be instantiated', () => {
      const engine = new TemplateEngineBase();
    it('should have a FindTemplate method', () => {
      expect(typeof TemplateEngineBase.prototype.FindTemplate).toBe('function');
  TestVariableValue,
describe('TestingFramework/EngineBase types', () => {
  describe('TestLogMessage', () => {
    it('should allow creating log messages at different levels', () => {
      const msg: TestLogMessage = {
        level: 'info',
        message: 'Test started',
      expect(msg.level).toBe('info');
      expect(msg.message).toBe('Test started');
  describe('TestProgress', () => {
    it('should track progress with percentage', () => {
      const progress: TestProgress = {
        step: 'running_oracle',
        percentage: 50,
        message: 'Running oracle checks',
      expect(progress.percentage).toBe(50);
  describe('TestRunOptions', () => {
    it('should allow creating run options', () => {
      const opts: TestRunOptions = {
        dryRun: false,
        environment: 'dev',
      expect(opts.verbose).toBe(true);
      expect(opts.environment).toBe('dev');
  describe('SuiteRunOptions', () => {
    it('should extend TestRunOptions with suite-specific options', () => {
      const opts: SuiteRunOptions = {
        parallel: true,
        maxParallel: 5,
      expect(opts.parallel).toBe(true);
      expect(opts.maxParallel).toBe(5);
  describe('OracleResult', () => {
    it('should represent a passing oracle check', () => {
      const result: OracleResult = {
        oracleType: 'exactMatch',
        message: 'Output matches expected',
      expect(result.passed).toBe(true);
      expect(result.score).toBe(1.0);
    it('should track validation errors and warnings', () => {
        { category: 'configuration', message: 'Missing required field' },
        { category: 'cost', message: 'Expensive model selected' },
  describe('RunContextDetails', () => {
    it('should allow capturing execution environment', () => {
      const context: RunContextDetails = {
        osType: 'linux',
        nodeVersion: '24.0.0',
        branch: 'main',
      expect(context.osType).toBe('linux');
      expect(context.ciProvider).toBe('GitHub Actions');
  describe('TestTypeVariablesSchema', () => {
    it('should define a valid schema with variables', () => {
      expect(schema.schemaVersion).toBe('1.0');
      expect(schema.variables).toHaveLength(1);
  describe('ResolvedTestVariables', () => {
    it('should track resolved values and their sources', () => {
      const resolved: ResolvedTestVariables = {
        values: { Temperature: 0.5 },
        sources: { Temperature: 'run' },
      expect(resolved.values.Temperature).toBe(0.5);
      expect(resolved.sources.Temperature).toBe('run');
  describe('TestVariableValue type', () => {
    it('should accept string, number, boolean, and Date', () => {
      const vals: TestVariableValue[] = ['hello', 42, true, new Date()];
      expect(vals).toHaveLength(4);
