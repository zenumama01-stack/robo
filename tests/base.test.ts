        protected async Load() { this._loaded = true; }
        protected TryThrowIfNotLoaded() {
            if (!this._loaded) throw new Error('Engine not loaded');
        static getInstance() { return new MockBaseEngine(); }
        RunViewParams: class {},
    MJEntityCommunicationFieldEntity: class { EntityCommunicationMessageTypeID = ''; },
    MJEntityCommunicationMessageTypeEntity: class { ID = ''; EntityID = ''; },
vi.mock('@memberjunction/communication-types', () => ({
    CommunicationEngineBase: {
            Config: vi.fn(),
            Metadata: {
                EntityCommunicationFields: [
                    { EntityCommunicationMessageTypeID: 'ecmt-1', FieldName: 'Email' },
                    { EntityCommunicationMessageTypeID: 'ecmt-2', FieldName: 'Phone' },
                EntityCommunicationMessageTypes: [
                    { ID: 'ecmt-1', EntityID: 'entity-1', CommunicationFields: [] },
                    { ID: 'ecmt-2', EntityID: 'entity-2', CommunicationFields: [] },
    EntityCommunicationParams,
    EntityCommunicationResult,
    EntityCommunicationResultItem,
    EntityCommunicationsEngineBase,
} from '../base';
describe('EntityCommunicationMessageTypeExtended', () => {
    it('should have CommunicationFields default to empty array', () => {
        const ext = new (EntityCommunicationMessageTypeExtended as unknown as { new(): EntityCommunicationMessageTypeExtended })();
        expect(ext.CommunicationFields).toEqual([]);
    it('should allow setting CommunicationFields', () => {
        const fields = [{ FieldName: 'Email' }];
        ext.CommunicationFields = fields as never;
        expect(ext.CommunicationFields).toBe(fields);
describe('EntityCommunicationParams', () => {
    it('should have default values', () => {
        const params = new EntityCommunicationParams();
        expect(params.PreviewOnly).toBe(false);
        expect(params.IncludeProcessedMessages).toBe(false);
describe('EntityCommunicationResult', () => {
    it('should be constructible with properties', () => {
        const result = new EntityCommunicationResult();
        result.ErrorMessage = undefined;
        result.Results = [];
        expect(result.Results).toEqual([]);
describe('EntityCommunicationResultItem', () => {
    it('should be constructible', () => {
        const item = new EntityCommunicationResultItem();
        expect(item.RecipientData).toBeUndefined();
        expect(item.Message).toBeUndefined();
describe('EntityCommunicationsEngineBase', () => {
    describe('GetEntityCommunicationMessageTypes', () => {
        it('should filter message types by entityID', async () => {
            // We need to test through the abstract class indirectly
            // The abstract class has concrete methods that filter _Metadata
            // Since it's abstract, we can verify the data class structures
            const engine = EntityCommunicationsEngineBase.Instance;
