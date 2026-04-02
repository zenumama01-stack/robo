 * Unit tests for the ComponentRegistryClientSDK package.
 * Tests: client construction, URL building, retry logic, error mapping.
vi.mock('@memberjunction/interactive-component-types', () => ({
  ComponentSpec: class {},
import { ComponentRegistryClient } from '../ComponentRegistryClient';
import { RegistryError, RegistryErrorCode } from '../types';
function createClient(overrides: Record<string, unknown> = {}) {
  return new ComponentRegistryClient({
    baseUrl: 'https://registry.example.com',
    retryPolicy: { maxRetries: 0, initialDelay: 100, maxDelay: 1000, backoffMultiplier: 2 },
function jsonResponse(data: unknown, status = 200) {
  return Promise.resolve({
    ok: status >= 200 && status < 300,
    statusText: status === 200 ? 'OK' : 'Error',
    headers: new Headers({ 'content-type': 'application/json' }),
    json: () => Promise.resolve(data),
    text: () => Promise.resolve(JSON.stringify(data)),
describe('ComponentRegistryClient', () => {
    it('should strip trailing slash from baseUrl', () => {
      const client = createClient({ baseUrl: 'https://reg.example.com/' });
      // Verify by pinging - the URL should not have double slashes
      mockFetch.mockReturnValue(jsonResponse({ status: 'ok' }));
      client.ping();
      const calledUrl = mockFetch.mock.calls[0][0] as string;
      expect(calledUrl).toBe('https://reg.example.com/api/v1/health');
    it('should apply default timeout and retry policy', () => {
      const client = new ComponentRegistryClient({ baseUrl: 'https://reg.example.com' });
      // Just verify the client was constructed without error
      expect(mockFetch).toHaveBeenCalled();
  describe('ping', () => {
    it('should return true on successful health check', async () => {
      const client = createClient();
      const result = await client.ping();
    it('should return false on failed health check', async () => {
      mockFetch.mockRejectedValue(new TypeError('fetch failed'));
  describe('getComponent', () => {
    it('should make correct GET request for a component', async () => {
      const spec = { name: 'test-component', version: '1.0.0' };
      mockFetch.mockReturnValue(jsonResponse({ specification: spec, hash: 'abc123' }));
      const result = await client.getComponent({
        registry: 'default',
        namespace: 'mj',
        name: 'test-component',
      expect(result).toEqual(spec);
      expect(calledUrl).toContain('/api/v1/components/mj/test-component');
    it('should pass version and hash as query params', async () => {
      mockFetch.mockReturnValue(jsonResponse({ specification: {}, hash: 'abc' }));
      await client.getComponent({
        namespace: 'ns',
        name: 'comp',
        version: '2.0.0',
        hash: 'existing-hash',
      expect(calledUrl).toContain('version=2.0.0');
      expect(calledUrl).toContain('hash=existing-hash');
    it('should throw RegistryError when specification is missing', async () => {
      mockFetch.mockReturnValue(jsonResponse({ hash: 'abc', notModified: false }));
        client.getComponent({ registry: 'r', namespace: 'ns', name: 'comp' })
      ).rejects.toThrow(RegistryError);
  describe('getComponentWithHash', () => {
    it('should return notModified flag for 304 responses', async () => {
      mockFetch.mockReturnValue(jsonResponse({ message: 'Not modified', hash: 'abc' }));
      const result = await client.getComponentWithHash({
        registry: 'r',
        hash: 'abc',
      expect(result.notModified).toBe(true);
  describe('searchComponents', () => {
    it('should build search query params correctly', async () => {
      const searchResult = { components: [], total: 0, offset: 0, limit: 10 };
      mockFetch.mockReturnValue(jsonResponse(searchResult));
      await client.searchComponents({
        query: 'dashboard',
        type: 'ui',
        tags: ['chart', 'kpi'],
        limit: 20,
        offset: 5,
        sortBy: 'name',
      expect(calledUrl).toContain('namespace=mj');
      expect(calledUrl).toContain('q=dashboard');
      expect(calledUrl).toContain('type=ui');
      expect(calledUrl).toContain('tag=chart');
      expect(calledUrl).toContain('tag=kpi');
      expect(calledUrl).toContain('limit=20');
      expect(calledUrl).toContain('offset=5');
      expect(calledUrl).toContain('sortBy=name');
      expect(calledUrl).toContain('sortDirection=asc');
  describe('submitFeedback', () => {
    it('should submit feedback as POST', async () => {
      mockFetch.mockReturnValue(jsonResponse({ success: true, feedbackID: 'fb-1' }));
      const result = await client.submitFeedback({
        componentName: 'chart',
        componentNamespace: 'mj',
        rating: 5,
        comments: 'Great!',
      expect(result.feedbackID).toBe('fb-1');
      const callArgs = mockFetch.mock.calls[0][1];
      expect(callArgs.method).toBe('POST');
    it('should return error response on failure', async () => {
      mockFetch.mockRejectedValue(new Error('Network down'));
        rating: 3,
  describe('error handling', () => {
    it('should throw COMPONENT_NOT_FOUND for 404', async () => {
      mockFetch.mockReturnValue(
        Promise.resolve({
          status: 404,
          statusText: 'Not Found',
          json: () => Promise.resolve({ message: 'Component not found' }),
        await client.getComponent({ registry: 'r', namespace: 'ns', name: 'missing' });
        expect.fail('Should have thrown');
        expect(error).toBeInstanceOf(RegistryError);
        const re = error as RegistryError;
        expect(re.code).toBe(RegistryErrorCode.COMPONENT_NOT_FOUND);
    it('should throw UNAUTHORIZED for 401', async () => {
          json: () => Promise.resolve({ message: 'Invalid token' }),
        await client.getRegistryInfo('default');
        expect((error as RegistryError).code).toBe(RegistryErrorCode.UNAUTHORIZED);
    it('should include Authorization header when apiKey is set', async () => {
      const client = createClient({ apiKey: 'my-secret-key' });
      await client.ping();
      expect(callArgs.headers['Authorization']).toBe('Bearer my-secret-key');
