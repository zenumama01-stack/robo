        Entities: []
        RunView: vi.fn().mockResolvedValue({ Success: false, Results: [] })
    UserInfo: class {}
    ActionResultSimple: {},
    ActionParam: class {}
vi.mock('../EncryptionEngine', () => ({
    EncryptionEngine: {
            Encrypt: vi.fn().mockResolvedValue('$ENC$encrypted'),
            Decrypt: vi.fn().mockResolvedValue('decrypted'),
            IsEncrypted: vi.fn().mockReturnValue(false),
            ValidateKeyMaterial: vi.fn().mockResolvedValue(undefined),
            EncryptWithLookup: vi.fn().mockResolvedValue('$ENC$re-encrypted'),
            ClearCaches: vi.fn()
import { EnableFieldEncryptionAction } from '../actions/EnableFieldEncryptionAction';
import { RotateEncryptionKeyAction } from '../actions/RotateEncryptionKeyAction';
describe('EnableFieldEncryptionAction', () => {
    let action: EnableFieldEncryptionAction;
        action = new EnableFieldEncryptionAction();
        it('should return error when EntityFieldID is missing', async () => {
            const result = await action.Run({
                ContextUser: undefined
            } as Record<string, unknown> as Parameters<typeof action.Run>[0]);
            expect(result.ResultCode).toBe('INVALID_PARAMS');
            expect(result.Message).toContain('EntityFieldID is required');
        it('should return error when EntityFieldID param has no Value', async () => {
                Params: [{ Name: 'OtherParam', Value: 'test' }],
        it('should handle errors gracefully and return error result', async () => {
            // The RunView mock returns Success: false, which causes the action to fail
                    { Name: 'EntityFieldID', Value: '550e8400-e29b-41d4-a716-446655440000' },
                    { Name: 'BatchSize', Value: 50 }
        it('should use default BatchSize of 100 when not specified', async () => {
                    { Name: 'EntityFieldID', Value: 'some-id' }
            // The action runs but fails because RunView returns no results
        it('should update output params when RecordsEncrypted param exists', async () => {
                { Name: 'EntityFieldID', Value: 'some-id' },
                { Name: 'RecordsEncrypted', Value: 0 },
                { Name: 'RecordsSkipped', Value: 0 }
            // Even on failure, the output params structure should be handled
            expect(result.Params).toBeDefined();
describe('RotateEncryptionKeyAction', () => {
    let action: RotateEncryptionKeyAction;
        action = new RotateEncryptionKeyAction();
        it('should return error when EncryptionKeyID is missing', async () => {
                    { Name: 'NewKeyLookupValue', Value: 'NEW_KEY' }
            expect(result.Message).toContain('EncryptionKeyID is required');
        it('should return error when NewKeyLookupValue is missing', async () => {
                    { Name: 'EncryptionKeyID', Value: '550e8400-e29b-41d4-a716-446655440000' }
            expect(result.Message).toContain('NewKeyLookupValue is required');
        it('should return error when both required params are missing', async () => {
                    { Name: 'EncryptionKeyID', Value: '550e8400-e29b-41d4-a716-446655440000' },
            // Should attempt to run (may fail due to mocked RunView)
        it('should handle errors gracefully', async () => {
                    { Name: 'NewKeyLookupValue', Value: 'NEW_KEY' },
        it('should update output params when they exist', async () => {
                { Name: 'RecordsProcessed', Value: 0 },
                { Name: 'FieldsProcessed', Value: null }
        it('should handle null/empty param values', async () => {
                    { Name: 'EncryptionKeyID', Value: null },
                    { Name: 'NewKeyLookupValue', Value: '' }
