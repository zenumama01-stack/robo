// Mock the config module before any imports that depend on it
vi.mock('../../config', () => ({
  configInfo: {
    authProviders: [],
    databaseSettings: {},
    baseUrl: 'http://localhost',
    graphqlRootPath: '/',
import { AuthProviderFactory } from '../AuthProviderFactory';
import { IAuthProvider } from '../IAuthProvider';
import { initializeAuthProviders } from '../initializeProviders';
 * Test suite for backward compatibility of the new auth provider system
describe('Authentication Provider Backward Compatibility', () => {
  let factory: AuthProviderFactory;
    factory = AuthProviderFactory.getInstance();
  describe('Configuration-Based Provider Initialization', () => {
    it('should initialize with no providers when config is empty', () => {
      // With empty authProviders array, no providers should be registered
      expect(factory.hasProviders()).toBe(false);
    it('should clear existing providers before re-initializing', () => {
      // Register a manual provider first
      const testProvider = {
        name: 'test',
        issuer: 'https://test.com',
        audience: 'test',
        jwksUri: 'https://test.com/jwks',
        validateConfig: () => true,
        getSigningKey: vi.fn(),
        extractUserInfo: vi.fn(),
        matchesIssuer: vi.fn(),
      } as IAuthProvider;
      factory.register(testProvider);
      expect(factory.hasProviders()).toBe(true);
      // Re-initialize clears everything
  describe('Provider Registry Functionality', () => {
    it('should find providers by issuer with different formats', () => {
      // Register a test provider
        issuer: 'https://test.provider.com/oauth2',
        audience: 'test-audience',
        jwksUri: 'https://test.provider.com/.well-known/jwks.json',
        matchesIssuer: (issuer: string) => {
          const normalized = issuer.toLowerCase().replace(/\/$/, '');
          return normalized === 'https://test.provider.com/oauth2';
      // Test with exact match
      expect(factory.getByIssuer('https://test.provider.com/oauth2')).toBe(testProvider);
      // Test with trailing slash
      expect(factory.getByIssuer('https://test.provider.com/oauth2/')).toBe(testProvider);
      // Test with different case
      expect(factory.getByIssuer('https://TEST.PROVIDER.COM/oauth2')).toBe(testProvider);
    it('should cache issuer lookups for performance', () => {
        issuer: 'https://test.provider.com',
        jwksUri: 'https://test.provider.com/jwks',
        matchesIssuer: vi.fn((issuer: string): boolean => issuer === 'https://test.provider.com')
      // First lookup
      factory.getByIssuer('https://test.provider.com');
      expect(testProvider.matchesIssuer).toHaveBeenCalledTimes(1);
      // Second lookup should use cache
  describe('User Info Extraction', () => {
    it('should return undefined for unregistered issuer', () => {
      const provider = factory.getByIssuer('https://unknown.issuer.com');
      expect(provider).toBeUndefined();
    it('should support extractUserInfo on manually registered providers', () => {
      const mockExtract = vi.fn().mockReturnValue({
        email: 'user@test.com',
        firstName: 'Test',
        lastName: 'User',
        name: 'test-extract',
        issuer: 'https://test-extract.com',
        jwksUri: 'https://test-extract.com/jwks',
        extractUserInfo: mockExtract,
        matchesIssuer: (iss: string) => iss === 'https://test-extract.com',
      const found = factory.getByIssuer('https://test-extract.com');
      const userInfo = found!.extractUserInfo({ email: 'user@test.com' });
      expect(userInfo.email).toBe('user@test.com');
      expect(mockExtract).toHaveBeenCalledTimes(1);
    it('should handle missing provider gracefully', () => {
      const unknownIssuer = 'https://unknown.provider.com';
      const provider = factory.getByIssuer(unknownIssuer);
    it('should validate provider configuration', () => {
      const invalidProvider = {
        name: 'invalid',
        issuer: '', // Invalid: empty issuer
        validateConfig: () => false,
        matchesIssuer: vi.fn()
      expect(() => factory.register(invalidProvider)).toThrow();
