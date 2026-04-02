// Track mock calls for Azure SDK
const mockGetSecret = vi.fn();
vi.mock('@azure/keyvault-secrets', () => ({
    SecretClient: class MockSecretClient {
        getSecret = mockGetSecret;
    DefaultAzureCredential: class MockDefaultAzureCredential {}
import { AzureKeyVaultKeySource } from '../providers/AzureKeyVaultKeySource';
describe('AzureKeyVaultKeySource', () => {
    let source: AzureKeyVaultKeySource;
        source = new AzureKeyVaultKeySource();
        it('should return "Azure Key Vault"', () => {
            expect(source.SourceName).toBe('Azure Key Vault');
        it('should return true after initialization', async () => {
        it('should pick up AZURE_KEYVAULT_URL from env', async () => {
            process.env.AZURE_KEYVAULT_URL = 'https://my-vault.vault.azure.net';
        it('should return true for valid full vault URL', async () => {
                'https://my-vault.vault.azure.net/secrets/my-secret'
        it('should return true for secret name with default vault URL', async () => {
            const s = new AzureKeyVaultKeySource();
            await s.Initialize();
            expect(await s.KeyExists('my-secret-name')).toBe(true);
        it('should return false for secret name without default vault URL', async () => {
            delete process.env.AZURE_KEYVAULT_URL;
            expect(await s.KeyExists('my-secret-name')).toBe(false);
        it('should return false for invalid URL format', async () => {
            expect(await source.KeyExists('http://not-a-vault/secrets/test')).toBe(false);
        it('should retrieve key from full vault URL', async () => {
            const keyBytes = Buffer.alloc(32, 0xEF);
            mockGetSecret.mockResolvedValue({
                value: keyBytes.toString('base64')
            const result = await source.GetKey(
                'https://my-vault.vault.azure.net/secrets/my-key'
            expect(result).toEqual(keyBytes);
        it('should retrieve key using default vault URL with secret name', async () => {
            const keyBytes = Buffer.alloc(32, 0xAA);
            const result = await s.GetKey('my-secret');
        it('should throw for invalid lookup value without vault URL', async () => {
            await expect(s.GetKey('just-a-name')).rejects.toThrow(
                'Invalid Key Vault lookup value'
        it('should throw when secret has no value', async () => {
            mockGetSecret.mockResolvedValue({ value: null });
            await expect(source.GetKey(
                'https://my-vault.vault.azure.net/secrets/empty-secret'
            )).rejects.toThrow('has no value');
        it('should provide helpful message for SecretNotFound', async () => {
            mockGetSecret.mockRejectedValue(new Error('SecretNotFound'));
                'https://my-vault.vault.azure.net/secrets/missing'
            )).rejects.toThrow('secret not found');
        it('should provide helpful message for Forbidden', async () => {
            mockGetSecret.mockRejectedValue(new Error('Forbidden'));
                'https://my-vault.vault.azure.net/secrets/restricted'
            )).rejects.toThrow('access denied');
        it('should provide helpful message for AuthenticationError', async () => {
            mockGetSecret.mockRejectedValue(new Error('AuthenticationError'));
                'https://my-vault.vault.azure.net/secrets/test'
            )).rejects.toThrow('authentication failed');
        it('should pass version to getSecret when provided', async () => {
            const keyBytes = Buffer.alloc(32, 0xBB);
            await source.GetKey(
                'https://my-vault.vault.azure.net/secrets/my-key',
                'abc123'
            expect(mockGetSecret).toHaveBeenCalledWith('my-key', { version: 'abc123' });
        it('should not pass version when not provided', async () => {
            const keyBytes = Buffer.alloc(32, 0xCC);
            expect(mockGetSecret).toHaveBeenCalledWith('my-key', {});
            mockGetSecret.mockRejectedValue(new Error('Unknown error'));
            )).rejects.toThrow('Azure Key Vault key retrieval failed');
        it('should clear clients and reset initialization', async () => {
