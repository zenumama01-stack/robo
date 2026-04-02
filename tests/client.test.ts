const { mockExecuteGQL } = vi.hoisted(() => ({
    mockExecuteGQL: vi.fn(),
        protected TryThrowIfNotLoaded() {}
    MJListDetailEntityType: class {},
    MJListEntityType: class {},
    MJTemplateParamEntity: class {},
vi.mock('@memberjunction/entity-communications-base', () => {
    class MockBase {
        static getInstance() { return new MockBase(); }
        GetEntityCommunicationMessageTypes() { return []; }
        EntitySupportsCommunication() { return false; }
        get EntityCommunicationMessageTypes() { return []; }
        get EntityCommunicationFields() { return []; }
        EntityCommunicationsEngineBase: MockBase,
        EntityCommunicationMessageTypeExtended: class {},
        EntityCommunicationParams: class {
            PreviewOnly = false;
            IncludeProcessedMessages = false;
        EntityCommunicationResult: class {},
    GraphQLDataProvider: {
        ExecuteGQL: mockExecuteGQL,
import { EntityCommunicationsEngineClient } from '../client';
describe('EntityCommunicationsEngineClient', () => {
    let client: EntityCommunicationsEngineClient;
        client = new EntityCommunicationsEngineClient();
    describe('getMessageTypeValues', () => {
        it('should return undefined for null messageType', () => {
            const result = (client as unknown as Record<string, Function>)['getMessageTypeValues'](null);
        it('should return undefined for undefined messageType', () => {
            const result = (client as unknown as Record<string, Function>)['getMessageTypeValues'](undefined);
        it('should extract values from messageType entity', () => {
            const messageType = {
                ID: 'mt-1',
                CommunicationProviderID: 'prov-1',
                CommunicationBaseMessageTypeID: 'bmt-1',
                AdditionalAttributes: '{"key":"val"}',
                __mj_CreatedAt: '2024-01-01',
                __mj_UpdatedAt: '2024-01-02',
                CommunicationProvider: 'TestProvider',
                CommunicationBaseMessageType: 'Email',
            const result = (client as unknown as Record<string, Function>)['getMessageTypeValues'](messageType);
            expect(result.ID).toBe('mt-1');
            expect(result.CommunicationProviderID).toBe('prov-1');
            expect(result.Name).toBe('Email');
            expect(result.Status).toBe('Active');
            expect(result.AdditionalAttributes).toBe('{"key":"val"}');
        it('should default AdditionalAttributes to empty string when null', () => {
                AdditionalAttributes: null,
            expect(result.AdditionalAttributes).toBe('');
    describe('getTemplateValues', () => {
        it('should return undefined for null template', () => {
            const result = (client as unknown as Record<string, Function>)['getTemplateValues'](null);
        it('should return undefined for undefined template', () => {
            const result = (client as unknown as Record<string, Function>)['getTemplateValues'](undefined);
        it('should extract values from template entity', () => {
            const template = {
                ID: 'tmpl-1',
                Name: 'Welcome Email',
                Description: 'Welcome template',
                UserPrompt: 'Generate welcome',
                CategoryID: 'cat-1',
                ActiveAt: '2024-01-01',
                DisabledAt: null,
                Category: 'General',
                User: 'Admin',
            const result = (client as unknown as Record<string, Function>)['getTemplateValues'](template);
            expect(result.ID).toBe('tmpl-1');
            expect(result.Name).toBe('Welcome Email');
            expect(result.Description).toBe('Welcome template');
            expect(result.IsActive).toBe(true);
        it('should default Description, UserPrompt, Category to empty string when falsy', () => {
                Name: 'Basic',
                UserPrompt: null,
                ActiveAt: null,
                Category: null,
            expect(result.Description).toBe('');
            expect(result.UserPrompt).toBe('');
            expect(result.Category).toBe('');
    describe('RunEntityCommunication', () => {
        it('should call GraphQL and return success result', async () => {
            mockExecuteGQL.mockResolvedValue({
                RunEntityCommunicationByViewID: {
                    Results: { Results: [{ RecipientData: {}, Message: {} }] },
                RunViewParams: {
                    OrderBy: '',
                    ExcludeUserViewRunID: '',
                    OverrideExcludeFilter: '',
                    SaveViewResults: false,
                    ExcludeDataFromAllPriorViewRuns: false,
                    ForceAuditLog: false,
                    AuditLogDescription: '',
                ProviderName: 'SendGrid',
                ProviderMessageTypeName: 'Email',
                Message: {
                    From: 'test@test.com',
                    To: 'to@test.com',
                    ContextData: {},
                PreviewOnly: false,
                IncludeProcessedMessages: false,
            const result = await client.RunEntityCommunication(params as never);
            expect(mockExecuteGQL).toHaveBeenCalled();
        it('should return undefined on GQL error (catch path)', async () => {
            mockExecuteGQL.mockRejectedValue(new Error('Network error'));
                RunViewParams: { ViewID: 'view-1' },
                    From: '',
            // When catch is hit, result is undefined (no explicit return)
        it('should return undefined when GQL returns null', async () => {
            mockExecuteGQL.mockResolvedValue(null);
