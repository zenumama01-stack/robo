const { mockSave, mockGetEntityObject } = vi.hoisted(() => ({
    mockSave: vi.fn(),
        protected ContextUser = { ID: 'user-1' };
        static _instances = new Map();
        static getInstance() {
            if (!this._instances.has(this)) {
                this._instances.set(this, new this());
            return this._instances.get(this);
        Metadata: vi.fn().mockImplementation(function () {
    MJCommunicationBaseMessageTypeEntity: class {},
    MJCommunicationLogEntity: class {},
    MJCommunicationProviderMessageTypeEntity: class {},
    MJCommunicationRunEntity: class {},
    MJEntityCommunicationFieldEntity: class {},
    MJEntityCommunicationMessageTypeEntity: class {},
vi.mock('../BaseProvider', () => ({
    CommunicationProviderEntityExtended: class {
        ID = 'provider-1';
        MessageTypes: unknown[] = [];
    ProcessedMessage: class {},
import { CommunicationEngineBase } from '../BaseEngine';
describe('CommunicationEngineBase', () => {
    let engine: CommunicationEngineBase;
        engine = CommunicationEngineBase.Instance;
        it('should return a singleton instance', () => {
            const instance1 = CommunicationEngineBase.Instance;
            const instance2 = CommunicationEngineBase.Instance;
            expect(instance1).toBeDefined();
            expect(instance2).toBeDefined();
    describe('Metadata getters', () => {
        it('should return empty arrays for BaseMessageTypes initially', () => {
            expect(engine.BaseMessageTypes).toEqual([]);
        it('should return empty arrays for Providers initially', () => {
            expect(engine.Providers).toEqual([]);
        it('should return empty arrays for ProviderMessageTypes initially', () => {
            expect(engine.ProviderMessageTypes).toEqual([]);
        it('should return Metadata object with all sub-properties', () => {
            const md = engine.Metadata;
            expect(md).toBeDefined();
            expect(md.BaseMessageTypes).toEqual([]);
            expect(md.Providers).toEqual([]);
            expect(md.ProviderMessageTypes).toEqual([]);
            expect(md.EntityCommunicationMessageTypes).toEqual([]);
            expect(md.EntityCommunicationFields).toEqual([]);
    describe('AdditionalLoading', () => {
        it('should link ProviderMessageTypes to their Providers', async () => {
            // Set up metadata
            const metadata = engine.Metadata;
            const provider1 = { ID: 'p1', MessageTypes: [] } as never;
            const provider2 = { ID: 'p2', MessageTypes: [] } as never;
            metadata.Providers.push(provider1, provider2);
            metadata.ProviderMessageTypes.push(
                { CommunicationProviderID: 'p1', Name: 'Email' } as never,
                { CommunicationProviderID: 'p1', Name: 'SMS' } as never,
                { CommunicationProviderID: 'p2', Name: 'Push' } as never,
            // Call AdditionalLoading (it's protected, so we access it via bracket notation)
            await (engine as unknown as Record<string, Function>)['AdditionalLoading']();
            expect((provider1 as { MessageTypes: unknown[] }).MessageTypes).toHaveLength(2);
            expect((provider2 as { MessageTypes: unknown[] }).MessageTypes).toHaveLength(1);
    describe('StartRun', () => {
        it('should create and save a communication run on success', async () => {
            const mockRun = {
                Status: '',
                Direction: '',
                StartedAt: null as Date | null,
                UserID: '',
                Save: mockSave.mockResolvedValue(true),
            mockGetEntityObject.mockResolvedValue(mockRun);
            const result = await (engine as unknown as Record<string, Function>)['StartRun']();
            expect(mockGetEntityObject).toHaveBeenCalledWith('MJ: Communication Runs', expect.anything());
            expect(result).toBe(mockRun);
            expect(mockRun.Status).toBe('Pending');
            expect(mockRun.Direction).toBe('Sending');
            expect(mockRun.StartedAt).toBeInstanceOf(Date);
        it('should return null when Save fails with LatestResult', async () => {
                Save: mockSave.mockResolvedValue(false),
                LatestResult: { CompleteMessage: 'Save error' },
        it('should return null when Save fails without LatestResult', async () => {
            expect(consoleSpy).toHaveBeenCalledWith('Failed to save communication run: No error details available');
    describe('EndRun', () => {
        it('should set status and end date and save', async () => {
                EndedAt: null as Date | null,
            const result = await (engine as unknown as Record<string, Function>)['EndRun'](mockRun);
            expect(mockRun.Status).toBe('Complete');
            expect(mockRun.EndedAt).toBeInstanceOf(Date);
    describe('StartLog', () => {
        it('should create and save a communication log', async () => {
            const mockLog = {
                CommunicationRunID: '',
                CommunicationProviderID: '',
                CommunicationProviderMessageTypeID: '',
                MessageDate: null as Date | null,
                MessageContent: '',
            mockGetEntityObject.mockResolvedValue(mockLog);
            const processedMessage = {
                MessageType: { CommunicationProviderID: 'prov-1', ID: 'pmt-1' },
                To: 'user@test.com',
                From: 'sender@test.com',
                ProcessedSubject: 'Subject',
                ProcessedHTMLBody: '<p>Hello</p>',
                ProcessedBody: 'Hello',
            const mockRun = { ID: 'run-1' };
            const result = await (engine as unknown as Record<string, Function>)['StartLog'](processedMessage, mockRun);
            expect(result).toBe(mockLog);
            expect(mockLog.CommunicationRunID).toBe('run-1');
            expect(mockLog.Status).toBe('Pending');
            expect(mockLog.Direction).toBe('Sending');
            expect(mockLog.CommunicationProviderID).toBe('prov-1');
            expect(mockLog.CommunicationProviderMessageTypeID).toBe('pmt-1');
            expect(JSON.parse(mockLog.MessageContent)).toEqual({
                Subject: 'Subject',
                HTMLBody: '<p>Hello</p>',
                TextBody: 'Hello',
        it('should handle log without a run', async () => {
                CommunicationRunID: undefined as string | undefined,
                ProcessedHTMLBody: '',
            const result = await (engine as unknown as Record<string, Function>)['StartLog'](processedMessage);
            expect(mockLog.CommunicationRunID).toBeUndefined();
        it('should return null when log Save fails', async () => {
                LatestResult: { CompleteMessage: 'Save failed' },
                ProcessedBody: '',
