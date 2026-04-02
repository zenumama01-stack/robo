        protected _loaded = true;
            if (!this._loaded) throw new Error('Not loaded');
            return { Entities: [] };
    Message: class {
        BodyTemplate = null;
        HTMLBodyTemplate = null;
        SubjectTemplate = null;
    MessageRecipient: class {},
    CommunicationProviderEntityExtended: class {},
vi.mock('@memberjunction/communication-engine', () => ({
    CommunicationEngine: {
            SendMessages: vi.fn().mockResolvedValue([]),
        EntityCommunicationMessageTypeExtended: class { CommunicationFields = []; BaseMessageTypeID = ''; },
        EntityCommunicationParams: class {},
import { EntityCommunicationsEngine } from '../entity-communications';
describe('EntityCommunicationsEngine', () => {
    let engine: EntityCommunicationsEngine;
        engine = new EntityCommunicationsEngine();
    describe('ValidateTemplateContextParamAlignment', () => {
        it('should return true when message has no templates', () => {
            const result = (engine as unknown as Record<string, Function>)['ValidateTemplateContextParamAlignment'](message);
        it('should return true when templates have non-overlapping params', () => {
                BodyTemplate: { Params: [{ Name: 'name', Type: 'Record' }] },
                HTMLBodyTemplate: { Params: [{ Name: 'email', Type: 'Record' }] },
        it('should return true when templates have same param name with different types (source stores objects, includes compares strings)', () => {
                BodyTemplate: { Params: [{ Name: 'data', Type: 'Record' }] },
                HTMLBodyTemplate: { Params: [{ Name: 'data', Type: 'Entity' }] },
            // Note: ValidateTemplateContextParamAlignment pushes full param objects into
            // paramNames but calls paramNames.includes(p.Name) comparing a string against
            // objects, so duplicates are never detected and the method always returns true.
    describe('PopulateSingleRecipientContextData', () => {
        it('should set Record type param to the record itself', async () => {
            const record = { ID: 'rec-1', Name: 'Test' };
            const params = [{ Name: 'currentRecord', Type: 'Record' }];
            const result = await (engine as unknown as Record<string, Function>)['PopulateSingleRecipientContextData'](
                record, [], 'rec-1', params
            expect(result['currentRecord']).toBe(record);
        it('should set Entity type param to filtered related data', async () => {
            const relatedData = [
                    paramName: 'orders',
                        { OrderID: 'o1', CustomerID: 'rec-1' },
                        { OrderID: 'o2', CustomerID: 'rec-2' },
            const params = [{ Name: 'orders', Type: 'Entity', LinkedParameterField: 'CustomerID' }];
                record, relatedData, 'rec-1', params
            expect(result['orders']).toHaveLength(1);
            expect(result['orders'][0].OrderID).toBe('o1');
        it('should skip Array, Scalar, and Object type params', async () => {
            const record = { ID: 'rec-1' };
                { Name: 'arr', Type: 'Array' },
                { Name: 'scalar', Type: 'Scalar' },
                { Name: 'obj', Type: 'Object' },
            expect(result['arr']).toBeUndefined();
            expect(result['scalar']).toBeUndefined();
            expect(result['obj']).toBeUndefined();
        it('should not process a param name twice', async () => {
                { Name: 'data', Type: 'Record' },
                { Name: 'data', Type: 'Record' }, // duplicate
            expect(result['data']).toBe(record);
        it('should return error when entity not found', async () => {
            // The factory mock already provides Metadata with Entities: [],
            // so any EntityID lookup will fail with "not found"
            const result = await engine.RunEntityCommunication({
                EntityID: 'nonexistent',
                RunViewParams: {} as never,
            expect(result.ErrorMessage).toContain('not found');
