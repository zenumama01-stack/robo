// Mock MJ global
// Mock the AWS SDK dynamic import
const mockSend = vi.fn();
const mockDestroy = vi.fn();
vi.mock('@aws-sdk/client-kms', () => {
    // Use proper classes instead of arrow functions so they are constructable
    // (vitest v4 requires constructor-compatible implementations for `new`)
        KMSClient: class MockKMSClient {
            destroy = mockDestroy;
        DecryptCommand: class MockDecryptCommand {
            CiphertextBlob: Uint8Array | undefined;
            constructor(params: { CiphertextBlob?: Uint8Array }) {
                this.CiphertextBlob = params?.CiphertextBlob;
import { AWSKMSKeySource } from '../providers/AWSKMSKeySource';
describe('AWSKMSKeySource', () => {
    let source: AWSKMSKeySource;
    const originalEnv = process.env;
        process.env = { ...originalEnv };
        source = new AWSKMSKeySource();
    describe('SourceName', () => {
        it('should return "AWS KMS"', () => {
            expect(source.SourceName).toBe('AWS KMS');
    describe('ValidateConfiguration', () => {
        it('should return false before initialization', () => {
            expect(source.ValidateConfiguration()).toBe(false);
        it('should return false after init without region', async () => {
            delete process.env.AWS_REGION;
            delete process.env.AWS_DEFAULT_REGION;
        it('should return true after init with AWS_REGION', async () => {
            process.env.AWS_REGION = 'us-east-1';
            expect(source.ValidateConfiguration()).toBe(true);
        it('should return true after init with AWS_DEFAULT_REGION', async () => {
            process.env.AWS_DEFAULT_REGION = 'eu-west-1';
    describe('Initialize', () => {
        it('should only initialize once', async () => {
            await source.Initialize(); // Second call should be no-op
    describe('KeyExists', () => {
        it('should return true for valid ARN format', async () => {
            expect(await source.KeyExists(
                'arn:aws:kms:us-east-1:123456789012:key/12345678-1234-1234-1234-123456789012'
            )).toBe(true);
        it('should return true for alias ARN format', async () => {
                'arn:aws:kms:us-east-1:123456789012:alias/my-key'
        it('should return true for alias format', async () => {
            expect(await source.KeyExists('alias/my-key')).toBe(true);
        it('should return false for empty string', async () => {
            expect(await source.KeyExists('')).toBe(false);
        it('should return false for null', async () => {
            expect(await source.KeyExists(null as unknown as string)).toBe(false);
        it('should return false for plain string', async () => {
            expect(await source.KeyExists('random-string')).toBe(false);
        it('should return false for malformed ARN', async () => {
            expect(await source.KeyExists('arn:aws:kms:invalid')).toBe(false);
    describe('GetKey', () => {
        it('should throw when lookup value is empty', async () => {
            await expect(source.GetKey('')).rejects.toThrow(
                'requires a lookup value'
        it('should throw when lookup value is null', async () => {
            await expect(source.GetKey(null as unknown as string)).rejects.toThrow(
        it('should call KMS decrypt and return key material', async () => {
            const expectedKeyBytes = Buffer.alloc(32, 0xAB);
            mockSend.mockResolvedValue({
                Plaintext: Uint8Array.from(expectedKeyBytes)
            const result = await source.GetKey('base64-encoded-ciphertext-blob');
            expect(result).toBeInstanceOf(Buffer);
            expect(result).toEqual(expectedKeyBytes);
        it('should throw when KMS returns empty plaintext', async () => {
            mockSend.mockResolvedValue({ Plaintext: null });
            await expect(source.GetKey('some-ciphertext')).rejects.toThrow(
                'KMS returned empty plaintext'
        it('should provide helpful message for InvalidCiphertextException', async () => {
            mockSend.mockRejectedValue(new Error('InvalidCiphertextException'));
            await expect(source.GetKey('bad-ciphertext')).rejects.toThrow(
                'Invalid ciphertext'
        it('should provide helpful message for AccessDeniedException', async () => {
            mockSend.mockRejectedValue(new Error('AccessDeniedException'));
                'access denied'
        it('should provide helpful message for NotFoundException', async () => {
            mockSend.mockRejectedValue(new Error('NotFoundException'));
                'key not found'
        it('should wrap generic errors', async () => {
            mockSend.mockRejectedValue(new Error('NetworkError'));
                'AWS KMS key retrieval failed'
    describe('Dispose', () => {
        it('should clean up the client', async () => {
            await source.Dispose();
        it('should handle dispose when not initialized', async () => {
            await expect(source.Dispose()).resolves.toBeUndefined();
