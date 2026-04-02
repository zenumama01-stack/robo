import { EncryptionKeySourceBase } from '../EncryptionKeySourceBase';
 * Concrete test implementation of the abstract EncryptionKeySourceBase.
class TestKeySource extends EncryptionKeySourceBase {
    get SourceName(): string {
        return 'Test Source';
    ValidateConfiguration(): boolean {
        return this._config.lookupValue !== undefined;
    async GetKey(lookupValue: string, _keyVersion?: string): Promise<Buffer> {
        if (!lookupValue) {
            throw new Error('Lookup value required');
        return Buffer.from('test-key-material');
    async KeyExists(lookupValue: string): Promise<boolean> {
        return lookupValue === 'existing_key';
describe('EncryptionKeySourceBase', () => {
        it('should initialize with provided config', () => {
            const config = { lookupValue: 'MY_KEY' };
            const source = new TestKeySource(config);
        it('should initialize with empty config when none provided', () => {
            const source = new TestKeySource();
            // lookupValue will be undefined so ValidateConfiguration returns false
        it('should initialize with undefined config', () => {
            const source = new TestKeySource(undefined);
        it('should accept additionalConfig in config', () => {
                lookupValue: 'key',
                additionalConfig: { vaultUrl: 'https://vault.example.com' }
        it('should return the source name from the concrete class', () => {
            expect(source.SourceName).toBe('Test Source');
        it('should resolve without error (default no-op implementation)', async () => {
            await expect(source.Initialize()).resolves.toBeUndefined();
        it('should return key material for valid lookup', async () => {
            const key = await source.GetKey('my_key');
            expect(key.length).toBeGreaterThan(0);
            await expect(source.GetKey('')).rejects.toThrow('Lookup value required');
        it('should return true for an existing key', async () => {
            expect(await source.KeyExists('existing_key')).toBe(true);
        it('should return false for a non-existing key', async () => {
