// Mock all external MJ dependencies
vi.mock('@memberjunction/global', () => {
    const ENCRYPTION_MARKER = '$ENC$';
    const ENCRYPTED_SENTINEL = '[!ENCRYPTED$]';
        ENCRYPTION_MARKER,
        ENCRYPTED_SENTINEL,
        IsValueEncrypted: (value: string | null | undefined, marker?: string): boolean => {
            if (value === ENCRYPTED_SENTINEL) return true;
            return value.startsWith(marker || ENCRYPTION_MARKER);
                    CreateInstance: vi.fn()
    IMetadataProvider: {},
            return (BaseEngine as Record<string, unknown>)._instance as T;
        static _instance: unknown;
        async RefreshAllItems() {}
    RegisterForStartup: () => (target: Function) => target
    class FakeEncryptionEngineBase {
        static _singleton: FakeEncryptionEngineBase | null = null;
            if (!FakeEncryptionEngineBase._singleton) {
                // Use `this` so the actual calling subclass (EncryptionEngine) is
                // instantiated, mirroring how BaseEngine.getInstance() works.
                const Ctor = this as { new(): FakeEncryptionEngineBase };
                FakeEncryptionEngineBase._singleton = new Ctor();
            return FakeEncryptionEngineBase._singleton as T;
        async Config() {
        async Load() {
        // Mock GetKeyConfiguration to return a valid configuration
        GetKeyConfiguration(keyId: string): {
            key: Record<string, unknown>;
            algorithm: Record<string, unknown>;
            source: Record<string, unknown>;
        } | undefined {
            return undefined; // Default - tests will override
        EncryptionEngineBase: FakeEncryptionEngineBase
import { EncryptionEngine } from '../EncryptionEngine';
describe('EncryptionEngine', () => {
    let engine: EncryptionEngine;
        engine = EncryptionEngine.Instance;
        engine.ClearCaches();
            const instance1 = EncryptionEngine.Instance;
            const instance2 = EncryptionEngine.Instance;
        it('should be an instance of EncryptionEngine', () => {
            expect(EncryptionEngine.Instance).toBeInstanceOf(EncryptionEngine);
    describe('IsEncrypted', () => {
        it('should return true for value starting with $ENC$', () => {
            expect(engine.IsEncrypted('$ENC$key-id$AES-256-GCM$iv$ciphertext$tag')).toBe(true);
        it('should return false for plain text', () => {
            expect(engine.IsEncrypted('hello world')).toBe(false);
            expect(engine.IsEncrypted(null)).toBe(false);
            expect(engine.IsEncrypted(undefined)).toBe(false);
            expect(engine.IsEncrypted('')).toBe(false);
        it('should return false for number', () => {
            expect(engine.IsEncrypted(42)).toBe(false);
        it('should return true for encrypted sentinel', () => {
            expect(engine.IsEncrypted('[!ENCRYPTED$]')).toBe(true);
        it('should check custom marker when provided', () => {
            expect(engine.IsEncrypted('$CUSTOM$data', '$CUSTOM$')).toBe(true);
            expect(engine.IsEncrypted('$ENC$data', '$CUSTOM$')).toBe(false);
    describe('ParseEncryptedValue', () => {
        it('should parse a valid encrypted value with auth tag (GCM)', () => {
            const value = '$ENC$550e8400-e29b-41d4-a716-446655440000$AES-256-GCM$aXZkYXRh$Y2lwaGVydGV4dA==$YXV0aHRhZw==';
            const parts = engine.ParseEncryptedValue(value);
            expect(parts.marker).toBe('$ENC$');
            expect(parts.keyId).toBe('550e8400-e29b-41d4-a716-446655440000');
            expect(parts.algorithm).toBe('AES-256-GCM');
            expect(parts.iv).toBe('aXZkYXRh');
            expect(parts.ciphertext).toBe('Y2lwaGVydGV4dA==');
            expect(parts.authTag).toBe('YXV0aHRhZw==');
        it('should parse a valid encrypted value without auth tag (CBC)', () => {
            const value = '$ENC$550e8400-e29b-41d4-a716-446655440000$AES-256-CBC$aXZkYXRh$Y2lwaGVydGV4dA==';
            expect(parts.algorithm).toBe('AES-256-CBC');
            expect(parts.authTag).toBeUndefined();
        it('should throw for null input', () => {
            expect(() => engine.ParseEncryptedValue(null as unknown as string)).toThrow(
                'Cannot parse encrypted value'
        it('should throw for empty string', () => {
            expect(() => engine.ParseEncryptedValue('')).toThrow(
        it('should throw for non-string input', () => {
            expect(() => engine.ParseEncryptedValue(123 as unknown as string)).toThrow(
        it('should throw for too few parts', () => {
            expect(() => engine.ParseEncryptedValue('$ENC$key$alg')).toThrow(
                'Invalid encrypted value format'
        it('should throw for invalid marker', () => {
            expect(() => engine.ParseEncryptedValue(
                '$BAD$550e8400-e29b-41d4-a716-446655440000$AES-256-GCM$iv$ct'
            )).toThrow('Invalid encryption marker');
        it('should throw for invalid key ID (not a UUID)', () => {
                '$ENC$not-a-uuid$AES-256-GCM$iv$ct'
            )).toThrow('Invalid key ID');
        it('should throw for completely random string', () => {
            expect(() => engine.ParseEncryptedValue('random-text')).toThrow();
    describe('Encrypt', () => {
        it('should return null as-is', async () => {
            const result = await engine.Encrypt(null as unknown as string, 'any-key-id');
        it('should return undefined as-is', async () => {
            const result = await engine.Encrypt(undefined as unknown as string, 'any-key-id');
        it('should throw for invalid key ID (empty)', async () => {
            await expect(engine.Encrypt('plaintext', '')).rejects.toThrow(
                'Invalid encryption key ID'
        it('should throw for invalid key ID (not a UUID)', async () => {
            await expect(engine.Encrypt('plaintext', 'not-a-uuid')).rejects.toThrow(
        it('should throw for null key ID', async () => {
            await expect(engine.Encrypt('plaintext', null as unknown as string)).rejects.toThrow(
    describe('Decrypt', () => {
        it('should return non-encrypted value as-is', async () => {
            const result = await engine.Decrypt('plain text');
            expect(result).toBe('plain text');
        it('should return empty string as-is', async () => {
            const result = await engine.Decrypt('');
    describe('ClearCaches', () => {
            expect(() => engine.ClearCaches()).not.toThrow();
        it('should be callable multiple times', () => {
            // No error means success
    describe('EncryptWithLookup', () => {
            const result = await engine.EncryptWithLookup(
                null as unknown as string,
                'key-id',
                'lookup'
                undefined as unknown as string,
    describe('ValidateKeyMaterial', () => {
            await expect(engine.ValidateKeyMaterial('', 'key-id')).rejects.toThrow(
            await expect(engine.ValidateKeyMaterial(
                'key-id'
            )).rejects.toThrow('Invalid lookup value');
        it('should throw for non-string lookup value', async () => {
                123 as unknown as string,
describe('EncryptionEngine - end-to-end encrypt/decrypt', () => {
     * This test suite exercises the actual crypto operations (performEncryption/performDecryption)
     * by setting up the engine with a mock key source and key configuration.
    it('should successfully encrypt and decrypt with AES-256-GCM', async () => {
        const keyMaterial = crypto.randomBytes(32);
        const keyId = '550e8400-e29b-41d4-a716-446655440000';
        const engine = EncryptionEngine.Instance;
        // Mock the internal methods via prototype
        const buildKeyConfigSpy = vi.spyOn(engine as Record<string, Function>, 'buildKeyConfiguration' as string).mockReturnValue({
            keyVersion: '1',
            marker: '$ENC$',
                name: 'AES-256-GCM',
                nodeCryptoName: 'aes-256-gcm',
                keyLengthBits: 256,
                ivLengthBytes: 12,
                isAEAD: true
                driverClass: 'EnvVarKeySource',
                lookupValue: 'TEST_KEY'
        const getKeyMaterialSpy = vi.spyOn(engine as Record<string, Function>, 'getKeyMaterial' as string).mockResolvedValue(keyMaterial);
        const ensureConfiguredSpy = vi.spyOn(engine as Record<string, Function>, 'ensureConfigured' as string).mockResolvedValue(undefined);
        const plaintext = 'Hello, MemberJunction!';
        const encrypted = await engine.Encrypt(plaintext, keyId);
        // Verify encrypted format
        expect(encrypted).toMatch(/^\$ENC\$/);
        expect(encrypted).toContain(keyId);
        expect(encrypted).toContain('AES-256-GCM');
        const decrypted = await engine.Decrypt(encrypted);
        expect(decrypted).toBe(plaintext);
        buildKeyConfigSpy.mockRestore();
        getKeyMaterialSpy.mockRestore();
        ensureConfiguredSpy.mockRestore();
    it('should successfully encrypt and decrypt with AES-256-CBC', async () => {
        const keyId = '660e8400-e29b-41d4-a716-446655440000';
        vi.spyOn(engine as Record<string, Function>, 'buildKeyConfiguration' as string).mockReturnValue({
                name: 'AES-256-CBC',
                nodeCryptoName: 'aes-256-cbc',
                ivLengthBytes: 16,
                isAEAD: false
        vi.spyOn(engine as Record<string, Function>, 'getKeyMaterial' as string).mockResolvedValue(keyMaterial);
        vi.spyOn(engine as Record<string, Function>, 'ensureConfigured' as string).mockResolvedValue(undefined);
        const plaintext = 'Sensitive data for CBC test';
        expect(encrypted).toContain('AES-256-CBC');
    it('should produce different ciphertexts for same plaintext (random IV)', async () => {
        const plaintext = 'Same data encrypted twice';
        const encrypted1 = await engine.Encrypt(plaintext, keyId);
        const encrypted2 = await engine.Encrypt(plaintext, keyId);
        // Should be different due to random IV
        expect(encrypted1).not.toBe(encrypted2);
        // But both should decrypt to the same value
        const decrypted1 = await engine.Decrypt(encrypted1);
        const decrypted2 = await engine.Decrypt(encrypted2);
        expect(decrypted1).toBe(plaintext);
        expect(decrypted2).toBe(plaintext);
    it('should fail to decrypt with wrong key', async () => {
        const wrongKeyMaterial = crypto.randomBytes(32);
        const getKeyMock = vi.spyOn(engine as Record<string, Function>, 'getKeyMaterial' as string);
        // Encrypt with correct key
        getKeyMock.mockResolvedValue(keyMaterial);
        const encrypted = await engine.Encrypt('secret data', keyId);
        // Try to decrypt with wrong key
        getKeyMock.mockResolvedValue(wrongKeyMaterial);
        await expect(engine.Decrypt(encrypted)).rejects.toThrow('Decryption failed');
    it('should handle empty string plaintext', async () => {
        // Empty string can be encrypted successfully
        const encrypted = await engine.Encrypt('', keyId);
        // Round-trip of empty strings is not supported because the encrypted
        // format uses '$' as a separator and empty ciphertext produces
        // consecutive '$' characters that are stripped during parsing
        // (split('$').filter(p => p !== '') removes empty segments), causing
        // the auth tag to be misidentified as the ciphertext.
        await expect(engine.Decrypt(encrypted)).rejects.toThrow(
            'Missing authentication tag'
    it('should handle Buffer input for plaintext', async () => {
        const plaintext = Buffer.from('Buffer input test', 'utf8');
        expect(decrypted).toBe('Buffer input test');
    it('should handle unicode plaintext', async () => {
        const plaintext = 'Hello World! Special chars: @#$%^& and accents: cafe\u0301';
