 * Unit tests for the MSGraph provider.
 * Tests: credential resolution, supported operations, email sending, error handling.
    getSupportedOperations() { return []; }
  resolveCredentialValue: (requestVal: string | undefined, envVal: string | undefined, disableFallback: boolean) => {
    if (requestVal) return requestVal;
    if (!disableFallback && envVal) return envVal;
  validateRequiredCredentials: (creds: Record<string, unknown>, required: string[], provider: string) => {
    for (const key of required) {
      if (!creds[key]) {
        throw new Error(`${provider}: Missing required credential: ${key}`);
vi.mock('env-var', () => {
  const envMap: Record<string, string> = {
    AZURE_CLIENT_ID: 'env-client-id',
    AZURE_CLIENT_SECRET: 'env-client-secret',
    AZURE_TENANT_ID: 'env-tenant-id',
    AZURE_ACCOUNT_EMAIL: 'test@example.com',
    AZURE_ACCOUNT_ID: 'env-user-id',
    AZURE_AAD_ENDPOINT: 'https://login.microsoftonline.com',
    AZURE_GRAPH_ENDPOINT: 'https://graph.microsoft.com',
      get: (key: string) => ({
        default: (def: string) => ({
          asString: () => envMap[key] ?? def,
// Mock @azure/identity
  ClientSecretCredential: vi.fn().mockImplementation(() => ({
    getToken: vi.fn().mockResolvedValue({ token: 'test-token' }),
  ConfidentialClientApplication: vi.fn(),
// Mock @microsoft/microsoft-graph-client
const { mockGraphApi } = vi.hoisted(() => ({
  mockGraphApi: vi.fn().mockReturnValue({
    post: vi.fn().mockResolvedValue({}),
    get: vi.fn().mockResolvedValue({ value: [] }),
vi.mock('@microsoft/microsoft-graph-client', () => ({
  Client: {
    initWithMiddleware: vi.fn().mockReturnValue({
      api: mockGraphApi,
vi.mock('@microsoft/microsoft-graph-client/authProviders/azureTokenCredentials', () => ({
  TokenCredentialAuthenticationProvider: vi.fn().mockImplementation(() => ({})),
import { MSGraphProvider } from '../MSGraphProvider';
describe('MSGraphProvider', () => {
  let provider: Record<string, Function>;
    provider = new MSGraphProvider() as unknown as Record<string, Function>;
    it('should include core messaging operations', () => {
      const ops = provider.getSupportedOperations() as string[];
  describe('credential resolution', () => {
    it('should return failure when required credentials are missing with fallback disabled', async () => {
        From: 'sender@example.com',
        To: 'recipient@example.com',
        ProcessedSubject: 'Test',
        ProcessedBody: 'Body',
        CCRecipients: [],
        BCCRecipients: [],
      const result = await provider.SendSingleMessage(message, { disableEnvironmentFallback: true }) as Record<string, unknown>;
    it('should be a function on the provider', () => {
      expect(typeof provider.CreateDraft).toBe('function');
