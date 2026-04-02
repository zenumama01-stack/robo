import { RecommendationRequest, RecommendationResult } from '../generic/types';
describe('RecommendationResult', () => {
  let result: RecommendationResult;
  let mockRequest: RecommendationRequest;
    mockRequest = new RecommendationRequest();
    result = new RecommendationResult(mockRequest);
    it('should initialize with Success=true', () => {
      expect(result.Success).toBe(true);
    it('should initialize with empty ErrorMessage', () => {
      expect(result.ErrorMessage).toBe('');
    it('should store the request', () => {
      expect(result.Request).toBe(mockRequest);
  describe('AppendWarning', () => {
    it('should append warning message with prefix', () => {
      result.AppendWarning('Something looks off');
      expect(result.ErrorMessage).toContain('Warning: Something looks off');
    it('should not change Success flag', () => {
    it('should append multiple warnings', () => {
      result.AppendWarning('Warning 1');
      result.AppendWarning('Warning 2');
      expect(result.ErrorMessage).toContain('Warning: Warning 1');
      expect(result.ErrorMessage).toContain('Warning: Warning 2');
  describe('AppendError', () => {
    it('should append error message with prefix', () => {
      result.AppendError('Something failed');
      expect(result.ErrorMessage).toContain('Error: Something failed');
    it('should set Success to false', () => {
    it('should accumulate errors', () => {
      result.AppendError('Error 1');
      result.AppendError('Error 2');
      expect(result.ErrorMessage).toContain('Error: Error 1');
      expect(result.ErrorMessage).toContain('Error: Error 2');
  describe('GetErrorMessages', () => {
    it('should return array of messages', () => {
      result.AppendWarning('Warn 1');
      const messages = result.GetErrorMessages();
      expect(messages.length).toBeGreaterThan(0);
    it('should return array with empty string for no messages', () => {
      expect(messages).toEqual(['']);
    it('should split messages by newline', () => {
      result.AppendError('First');
      result.AppendError('Second');
      // Each AppendError adds "Error: message \n"
      expect(messages.some(m => m.includes('First'))).toBe(true);
      expect(messages.some(m => m.includes('Second'))).toBe(true);
  describe('mixed warnings and errors', () => {
    it('should handle both warnings and errors', () => {
      result.AppendWarning('Warn');
      result.AppendError('Err');
      expect(messages.some(m => m.includes('Warning'))).toBe(true);
      expect(messages.some(m => m.includes('Error'))).toBe(true);
describe('RecommendationRequest', () => {
  it('should create with default empty Recommendations', () => {
    const request = new RecommendationRequest();
    expect(request.Recommendations).toEqual([]);
  it('should accept optional properties', () => {
    request.ListID = 'list-1';
    request.RunID = 'run-1';
    request.CreateErrorList = true;
    expect(request.ListID).toBe('list-1');
    expect(request.RunID).toBe('run-1');
    expect(request.CreateErrorList).toBe(true);
  it('should accept EntityAndRecordsInfo', () => {
    request.EntityAndRecordsInfo = {
      RecordIDs: ['id-1', 'id-2'],
    expect(request.EntityAndRecordsInfo.EntityName).toBe('Contacts');
    expect(request.EntityAndRecordsInfo.RecordIDs).toHaveLength(2);
  it('should support generic Options type', () => {
    const request = new RecommendationRequest<{ maxResults: number }>();
    request.Options = { maxResults: 10 };
    expect(request.Options.maxResults).toBe(10);
const { mockProviderInstance, mockClassFactory } = vi.hoisted(() => {
    const mockProviderInstance = {
        SendSingleMessage: vi.fn().mockResolvedValue({ Success: true, Error: '' }),
        CreateDraft: vi.fn().mockResolvedValue({ Success: true, DraftID: 'draft-1' }),
    const mockClassFactory = {
        CreateInstance: vi.fn().mockReturnValue(mockProviderInstance),
    return { mockProviderInstance, mockClassFactory };
    const mockRunEntity = {
        set Status(_v: string) {},
        get Status() { return 'Pending'; },
        set Direction(_v: string) {},
        get ID() { return 'run-1'; },
        set CommunicationRunID(_v: string | undefined) {},
        set CommunicationProviderID(_v: string) {},
        set CommunicationProviderMessageTypeID(_v: string) {},
        set MessageDate(_v: Date) {},
        set MessageContent(_v: string) {},
        set ErrorMessage(_v: string) {},
                .mockImplementation((entityName: string) => {
                    if (entityName === 'MJ: Communication Runs') return Promise.resolve(mockRunEntity);
                    if (entityName === 'MJ: Communication Logs') return Promise.resolve(mockLogEntity);
                    return Promise.resolve({});
            static getInstance<T>(): T { return new (this as never)() as T; }
            protected ContextUser = { ID: 'test-user-id', Name: 'Test' };
    MJCommunicationProviderMessageTypeEntity: class {
        ID = 'pmt-1';
        Name = 'Email';
        CommunicationProviderID = 'provider-1';
    MJCommunicationProviderEntity: class { ID = ''; Name = ''; SupportsDrafts = false; },
// Mock the templates dependency
vi.mock('@memberjunction/templates', () => ({
    TemplateEngineServer: {
            RenderTemplate: vi.fn().mockResolvedValue({ Success: true, Output: 'Rendered' }),
vi.mock('@memberjunction/communication-types', () => {
    class MockMessage {
        MessageType: Record<string, unknown> | null = null;
        From = '';
        To = '';
        Body = '';
        HTMLBody = '';
        Subject = '';
        ContextData: unknown = null;
        BodyTemplate: unknown = null;
        HTMLBodyTemplate: unknown = null;
        SubjectTemplate: unknown = null;
        constructor(copyFrom?: Record<string, unknown>) {
            if (copyFrom) Object.assign(this, copyFrom);
    class MockProcessedMessage extends MockMessage {
        ProcessedBody = '';
        ProcessedHTMLBody = '';
        ProcessedSubject = '';
        async Process() {
    class MockCommunicationEngineBase {
        private _Metadata = {
            EntityCommunicationFields: [],
        get Providers() { return this._Metadata.Providers; }
        get ProviderMessageTypes() { return this._Metadata.ProviderMessageTypes; }
        get BaseMessageTypes() { return this._Metadata.BaseMessageTypes; }
        protected async StartRun() {
        protected async EndRun() { return true; }
        protected async StartLog() {
        BaseCommunicationProvider: class {
            SendSingleMessage = vi.fn();
            CreateDraft = vi.fn();
        CommunicationEngineBase: MockCommunicationEngineBase,
        Message: MockMessage,
        ProcessedMessage: MockProcessedMessage,
        MessageResult: class {},
        MessageRecipient: class { To = ''; ContextData: unknown = null; },
        ProviderCredentialsBase: class {},
        CreateDraftResult: class {},
// Import under test
import { CommunicationEngine } from '../Engine';
import { ProcessedMessageServer } from '../BaseProvider';
describe('CommunicationEngine', () => {
    let engine: CommunicationEngine;
        engine = new CommunicationEngine();
        (engine as Record<string, unknown>)['Loaded'] = true;
        (engine as Record<string, unknown>)['ContextUser'] = { ID: 'test-user-id', Name: 'Test' };
            const instance = CommunicationEngine.Instance;
    describe('GetProvider', () => {
        it('should throw when not loaded', () => {
            (engine as Record<string, unknown>)['Loaded'] = false;
            expect(() => engine.GetProvider('TestProvider')).toThrow('Metadata not loaded');
        it('should throw when ClassFactory returns null', () => {
            expect(() => engine.GetProvider('NonExistent')).toThrow('Provider NonExistent not found');
        it('should throw when ClassFactory returns base class instance', () => {
            mockClassFactory.CreateInstance.mockReturnValue({
                constructor: { name: 'BaseCommunicationProvider' },
            expect(() => engine.GetProvider('Base')).toThrow('Provider Base not found');
        it('should return provider instance when valid subclass', () => {
            const subClassInstance = {
                constructor: { name: 'SendGridProvider' },
                SendSingleMessage: vi.fn(),
            mockClassFactory.CreateInstance.mockReturnValue(subClassInstance);
            const provider = engine.GetProvider('SendGrid');
            expect(provider).toBe(subClassInstance);
    describe('SendMessages', () => {
        it('should send a message to each recipient', async () => {
            // Mock StartRun and EndRun
            vi.spyOn(engine as never, 'StartRun' as never).mockResolvedValue(mockRun as never);
            vi.spyOn(engine as never, 'EndRun' as never).mockResolvedValue(true as never);
            // Mock SendSingleMessage
            const sendSpy = vi.spyOn(engine, 'SendSingleMessage').mockResolvedValue({
                Error: '',
                Message: {} as never,
                Body: 'Hello',
                Subject: 'Test',
                To: '',
                ContextData: null,
            const recipients = [
                { To: 'a@test.com', ContextData: { name: 'Alice' } },
                { To: 'b@test.com', ContextData: { name: 'Bob' } },
            const results = await engine.SendMessages(
                'TestProvider',
                'Email',
                message as never,
                recipients as never,
            expect(sendSpy).toHaveBeenCalledTimes(2);
        it('should throw when StartRun fails', async () => {
            vi.spyOn(engine as never, 'StartRun' as never).mockResolvedValue(null as never);
                engine.SendMessages('P', 'T', {} as never, [] as never)
            ).rejects.toThrow('Failed to start communication run');
        it('should throw when EndRun fails', async () => {
            vi.spyOn(engine as never, 'EndRun' as never).mockResolvedValue(false as never);
            vi.spyOn(engine, 'SendSingleMessage').mockResolvedValue({
                engine.SendMessages('P', 'T', {} as never, [{ To: 'a@test.com', ContextData: null }] as never)
            ).rejects.toThrow('Failed to end communication run');
    describe('SendSingleMessage', () => {
        it('should throw when not loaded', async () => {
                engine.SendSingleMessage('P', 'T', {} as never)
            ).rejects.toThrow('Metadata not loaded');
        it('should throw when provider not found', async () => {
            vi.spyOn(engine, 'GetProvider').mockImplementation(() => {
                throw new Error('Provider P not found.');
            ).rejects.toThrow('Provider P not found');
        it('should return success for preview only mode', async () => {
            const mockProvider = { SendSingleMessage: vi.fn(), constructor: { name: 'TestProvider' } };
            vi.spyOn(engine, 'GetProvider').mockReturnValue(mockProvider as never);
            // Set up Providers metadata
            (engine as Record<string, unknown>)['_Metadata'] = {
                Providers: [{
                    Name: 'P',
                    MessageTypes: [{ Name: 'T', ID: 'pmt-1', CommunicationProviderID: 'prov-1' }],
            // We need to mock ProcessedMessageServer.Process
            const mockMessage = {
                To: 'test@test.com',
                MessageType: null,
                BodyTemplate: null,
                HTMLBodyTemplate: null,
                SubjectTemplate: null,
            // Spy on ProcessedMessageServer prototype
            vi.spyOn(ProcessedMessageServer.prototype, 'Process').mockResolvedValue({ Success: true });
            const result = await engine.SendSingleMessage('P', 'T', mockMessage as never, undefined, true);
            expect(mockProvider.SendSingleMessage).not.toHaveBeenCalled();
        it('should throw when message processing fails', async () => {
            const mockProvider = { constructor: { name: 'TestProvider' } };
            vi.spyOn(ProcessedMessageServer.prototype, 'Process').mockResolvedValue({
                Message: 'Template render error',
                engine.SendSingleMessage('P', 'T', { MessageType: null } as never)
            ).rejects.toThrow('Failed to process message');
        it('should throw when provider message type not found', async () => {
                    MessageTypes: [], // No message types
                engine.SendSingleMessage('P', 'NonExistentType', { MessageType: null } as never)
            ).rejects.toThrow('Provider message type NonExistentType not found');
        it('should throw when provider entity not found in metadata', async () => {
                Providers: [], // Empty
    describe('CreateDraft', () => {
        it('should return failure when not loaded', async () => {
            const result = await engine.CreateDraft({} as never, 'P');
            expect(result.ErrorMessage).toContain('Metadata not loaded');
        it('should return failure when provider does not support drafts', async () => {
                Providers: [{ Name: 'P', SupportsDrafts: false }],
            expect(result.ErrorMessage).toContain('does not support creating drafts');
        it('should return failure when message processing fails', async () => {
            const mockProvider = { constructor: { name: 'TestProvider' }, CreateDraft: vi.fn() };
                Providers: [{ Name: 'P', SupportsDrafts: true }],
                Message: 'Process failed',
            const result = await engine.CreateDraft({ ContextData: {} } as never, 'P');
        it('should call provider.CreateDraft on success', async () => {
            const createDraftFn = vi.fn().mockResolvedValue({ Success: true, DraftID: 'draft-123' });
            const mockProvider = { constructor: { name: 'TestProvider' }, CreateDraft: createDraftFn };
            expect(result.DraftID).toBe('draft-123');
            expect(createDraftFn).toHaveBeenCalled();
        it('should handle thrown errors gracefully', async () => {
                throw new Error('Unexpected error');
            expect(result.ErrorMessage).toBe('Unexpected error');
describe('ProcessedMessageServer', () => {
    it('should process body without templates', async () => {
            Body: 'Plain body',
            HTMLBody: '<b>HTML body</b>',
            Subject: 'Test Subject',
        const processed = new ProcessedMessageServer(message as never);
        const result = await processed.Process();
        expect(processed.ProcessedBody).toBe('Plain body');
        expect(processed.ProcessedHTMLBody).toBe('<b>HTML body</b>');
        expect(processed.ProcessedSubject).toBe('Test Subject');
    it('should set empty HTML body when no HTMLBody and no template', async () => {
            Body: 'Text',
            HTMLBody: null,
            Subject: null,
        expect(processed.ProcessedHTMLBody).toBe('');
        expect(processed.ProcessedSubject).toBe('');
  default: { get: vi.fn() },
vi.mock('jsdom', () => ({
  JSDOM: class {
    window: { document: { querySelector: () => ({ innerHTML: '<p>test content</p>' }) } };
      this.window = {
        document: {
          querySelector: () => ({ innerHTML: '<p>test content</p>' }),
  MJLibraryEntity: class {
  MJLibraryItemEntity: class {
    Type = '';
    LibraryID = '';
  RegisterClass: () => (_target: Function) => {},
import { LibraryItemEntityExtended, LibraryEntityExtended, DocumentationEngine } from '../Engine';
describe('DocUtils', () => {
  describe('LibraryItemEntityExtended', () => {
    it('should return correct URL segment for Class type', () => {
      const item = new LibraryItemEntityExtended();
      Object.defineProperty(item, 'Type', { value: 'Class', writable: true });
      expect(item.TypeURLSegment).toBe('classes');
    it('should return correct URL segment for Interface type', () => {
      Object.defineProperty(item, 'Type', { value: 'Interface', writable: true });
      expect(item.TypeURLSegment).toBe('interfaces');
    it('should return correct URL segment for Function type', () => {
      Object.defineProperty(item, 'Type', { value: 'Function', writable: true });
      expect(item.TypeURLSegment).toBe('functions');
    it('should return correct URL segment for Module type', () => {
      Object.defineProperty(item, 'Type', { value: 'Module', writable: true });
      expect(item.TypeURLSegment).toBe('modules');
    it('should return correct URL segment for Type type', () => {
      Object.defineProperty(item, 'Type', { value: 'Type', writable: true });
      expect(item.TypeURLSegment).toBe('types');
    it('should return correct URL segment for Variable type', () => {
      Object.defineProperty(item, 'Type', { value: 'Variable', writable: true });
      expect(item.TypeURLSegment).toBe('variables');
    it('should throw for unknown type', () => {
      Object.defineProperty(item, 'Type', { value: 'Unknown', writable: true });
      expect(() => item.TypeURLSegment).toThrow('Unknown type Unknown');
  describe('LibraryEntityExtended', () => {
    it('should have an empty Items array initially', () => {
      const lib = new LibraryEntityExtended();
      expect(lib.Items).toEqual([]);
  describe('DocumentationEngine', () => {
    it('should be a constructible class', () => {
      const engine = new DocumentationEngine();
    it('should have Libraries getter returning empty array before Config', () => {
      expect(engine.Libraries).toEqual([]);
    it('should have LibraryItems getter returning empty array before Config', () => {
      expect(engine.LibraryItems).toEqual([]);
