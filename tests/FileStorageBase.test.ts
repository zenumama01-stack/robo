 * Unit tests for the FileStorageBase class.
 * These tests focus on the initialize() method and account information handling.
  StorageProviderConfig,
  StorageListResult,
  CreatePreAuthUploadUrlPayload,
  StorageObjectMetadata,
  GetObjectParams,
  GetObjectMetadataParams,
  FileSearchResultSet,
  UnsupportedOperationError,
} from '../generic/FileStorageBase';
 * Concrete implementation of FileStorageBase for testing purposes.
 * This class implements all abstract methods with minimal functionality
 * to allow testing the base class behavior.
class TestableFileStorageDriver extends FileStorageBase {
  protected readonly providerName = 'TestableProvider';
  private _isConfigured = false;
  public configPassedToInitialize: StorageProviderConfig | null = null;
  public async initialize(config?: StorageProviderConfig): Promise<void> {
    // Call the base implementation
    await super.initialize(config);
    // Store the config for test verification
    this.configPassedToInitialize = config || null;
    this._isConfigured = true;
    return this._isConfigured;
  // Minimal implementations of abstract methods
  public async CreatePreAuthUploadUrl(objectName: string): Promise<CreatePreAuthUploadUrlPayload> {
    return { UploadUrl: `https://test.url/${objectName}` };
  public async CreatePreAuthDownloadUrl(objectName: string): Promise<string> {
    return `https://test.url/${objectName}`;
  public async MoveObject(oldObjectName: string, newObjectName: string): Promise<boolean> {
  public async DeleteObject(objectName: string): Promise<boolean> {
  public async ListObjects(prefix: string, delimiter?: string): Promise<StorageListResult> {
    return { objects: [], prefixes: [] };
  public async CreateDirectory(directoryPath: string): Promise<boolean> {
  public async DeleteDirectory(directoryPath: string, recursive?: boolean): Promise<boolean> {
  public async GetObjectMetadata(params: GetObjectMetadataParams): Promise<StorageObjectMetadata> {
      name: 'test.txt',
      path: '/',
      fullPath: '/test.txt',
      contentType: 'text/plain',
      isDirectory: false,
  public async GetObject(params: GetObjectParams): Promise<Buffer> {
    return Buffer.from('');
  public async PutObject(objectName: string, data: Buffer, contentType?: string, metadata?: Record<string, string>): Promise<boolean> {
  public async CopyObject(sourceObjectName: string, destinationObjectName: string): Promise<boolean> {
  public async ObjectExists(objectName: string): Promise<boolean> {
  public async DirectoryExists(directoryPath: string): Promise<boolean> {
  public async SearchFiles(query: string, options?: FileSearchOptions): Promise<FileSearchResultSet> {
    return { results: [], hasMore: false };
  public testThrowUnsupportedOperationError(methodName: string): never {
    return this.throwUnsupportedOperationError(methodName);
describe('FileStorageBase', () => {
  let driver: TestableFileStorageDriver;
    driver = new TestableFileStorageDriver();
  describe('initialize()', () => {
    it('should store accountId from config', async () => {
      const config: StorageProviderConfig = {
        accountId: 'test-account-id-123',
      expect(driver.AccountId).toBe('test-account-id-123');
    it('should store accountName from config when provided', async () => {
        accountName: 'My Test Storage Account',
      expect(driver.AccountName).toBe('My Test Storage Account');
    it('should handle undefined accountName gracefully', async () => {
        // accountName intentionally omitted
      expect(driver.AccountName).toBeUndefined();
    it('should allow additional provider-specific config values', async () => {
        accountId: 'account-456',
        accountName: 'S3 Bucket Account',
        bucket: 'my-bucket',
        region: 'us-west-2',
        accessKeyId: 'AKIA...',
      // Verify base properties are set
      expect(driver.AccountId).toBe('account-456');
      expect(driver.AccountName).toBe('S3 Bucket Account');
      // Verify the full config was passed through
      expect(driver.configPassedToInitialize).toEqual({
  describe('AccountId getter', () => {
    it('should return undefined before initialization', () => {
      expect(driver.AccountId).toBeUndefined();
    it('should return the accountId after initialization', async () => {
      await driver.initialize({ accountId: 'my-account-id' });
      expect(driver.AccountId).toBe('my-account-id');
  describe('AccountName getter', () => {
    it('should return the accountName after initialization', async () => {
        accountId: 'my-account-id',
        accountName: 'Production S3 Account',
      expect(driver.AccountName).toBe('Production S3 Account');
  describe('UnsupportedOperationError', () => {
    it('should create error with correct message format', () => {
      const error = new UnsupportedOperationError('SearchFiles', 'AWS S3');
      expect(error.message).toBe("Operation 'SearchFiles' is not supported by the AWS S3 provider");
      expect(error.name).toBe('UnsupportedOperationError');
    it('should throw UnsupportedOperationError via helper method', () => {
        driver.testThrowUnsupportedOperationError('TestMethod');
      }).toThrow(UnsupportedOperationError);
      }).toThrow("Operation 'TestMethod' is not supported by the TestableProvider provider");
  describe('IsConfigured', () => {
      expect(driver.IsConfigured).toBe(false);
      await driver.initialize({ accountId: 'test' });
      expect(driver.IsConfigured).toBe(true);
describe('StorageProviderConfig interface', () => {
  it('should allow accountId to be optional', () => {
    // This is a compile-time check - if the type is wrong, TypeScript will error
    const validConfigWithAccount: StorageProviderConfig = {
      accountId: 'optional-account-id',
    const validConfigWithoutAccount: StorageProviderConfig = {};
    expect(validConfigWithAccount.accountId).toBe('optional-account-id');
    expect(validConfigWithoutAccount.accountId).toBeUndefined();
  it('should allow optional accountName property', () => {
    const configWithName: StorageProviderConfig = {
      accountId: 'account-1',
      accountName: 'Optional Name',
    const configWithoutName: StorageProviderConfig = {
      accountId: 'account-2',
    expect(configWithName.accountName).toBe('Optional Name');
    expect(configWithoutName.accountName).toBeUndefined();
  it('should allow additional provider-specific properties', () => {
    const s3Config: StorageProviderConfig = {
      accountId: 'aws-account',
      accountName: 'AWS S3 Account',
      secretAccessKey: 'secret...',
    expect(s3Config.bucket).toBe('my-bucket');
    expect(s3Config.region).toBe('us-east-1');
describe('Unified Initialize Pattern', () => {
  describe('Simple Deployment (No Config)', () => {
    it('should initialize with no config (uses env vars)', async () => {
      // Simulate environment variable setup by calling initialize with no config
      await driver.initialize({});
      expect(driver.AccountId).toBeUndefined(); // No accountId provided
    it('should allow initialize to be called without any config object', async () => {
      // Even calling with undefined should work
      await driver.initialize(undefined as unknown as StorageProviderConfig);
      // The driver should still be in some state (depends on implementation)
      // Implementation stores config || null, so undefined becomes null
      expect(driver.configPassedToInitialize).toBeNull();
  describe('Multi-Tenant (With Config)', () => {
    it('should initialize with full config', async () => {
        accountId: '12345-67890',
        accountName: 'Test Account',
        bucket: 'runtime-bucket',
        accessKeyId: 'runtime-key',
      expect(driver.AccountId).toBe('12345-67890');
      expect(driver.AccountName).toBe('Test Account');
      expect(driver.configPassedToInitialize).toEqual(config);
    it('should allow partial config (accountId only)', async () => {
        accountId: 'account-123',
      expect(driver.AccountId).toBe('account-123');
  describe('Override Behavior', () => {
    it('should override with runtime config when both env and config provided', async () => {
      // Simulate: constructor loaded env vars, then initialize() called with config
      const runtimeConfig: StorageProviderConfig = {
        accountId: 'override-account',
        accountName: 'Override Account',
        bucket: 'override-bucket',
      await driver.initialize(runtimeConfig);
      expect(driver.AccountId).toBe('override-account');
      expect(driver.AccountName).toBe('Override Account');
