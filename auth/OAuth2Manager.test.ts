import { OAuth2Manager, OAuth2Config, OAuth2TokenData } from '../generic/OAuth2Manager';
const mockFetch = vi.fn();
describe('OAuth2Manager', () => {
    let manager: OAuth2Manager;
    const baseConfig: OAuth2Config = {
        clientId: 'test-client-id',
        clientSecret: 'test-client-secret',
        tokenEndpoint: 'https://auth.example.com/token',
        it('should initialize with basic config', () => {
            manager = new OAuth2Manager(baseConfig);
            expect(manager.isTokenValid()).toBe(false);
            expect(manager.getTokenState()).toBeNull();
        it('should initialize with pre-existing tokens', () => {
            manager = new OAuth2Manager({
                ...baseConfig,
                accessToken: 'existing-token',
                refreshToken: 'existing-refresh',
                tokenExpiresAt: Date.now() + 3600000,
            expect(manager.isTokenValid()).toBe(true);
            expect(manager.getTokenState()).toBeDefined();
            expect(manager.getTokenState()!.accessToken).toBe('existing-token');
        it('should use default refreshBufferMs of 60000', () => {
                accessToken: 'token',
                tokenExpiresAt: Date.now() + 59000, // 59 seconds - less than 60s buffer
        it('should use custom refreshBufferMs', () => {
                refreshBufferMs: 5000, // 5 seconds
                tokenExpiresAt: Date.now() + 10000, // 10 seconds from now
    describe('isTokenValid', () => {
        it('should return false when no access token', () => {
        it('should return false when token is about to expire within buffer', () => {
                accessToken: 'tok',
                tokenExpiresAt: Date.now() + 30000, // 30 seconds, but buffer is 60s
        it('should return true when token is valid and not near expiration', () => {
                tokenExpiresAt: Date.now() + 120000, // 2 minutes
    describe('getAuthorizationUrl', () => {
        it('should throw when no authorization endpoint configured', () => {
            expect(() => manager.getAuthorizationUrl()).toThrow('Authorization endpoint not configured');
        it('should build authorization URL with client_id and response_type', () => {
                authorizationEndpoint: 'https://auth.example.com/authorize',
            const url = manager.getAuthorizationUrl();
            expect(url).toContain('https://auth.example.com/authorize?');
            expect(url).toContain('client_id=test-client-id');
            expect(url).toContain('response_type=code');
        it('should include redirect_uri when configured', () => {
                redirectUri: 'https://app.example.com/callback',
            expect(url).toContain('redirect_uri=');
            expect(url).toContain(encodeURIComponent('https://app.example.com/callback'));
        it('should include scopes when configured', () => {
                scopes: ['read', 'write', 'admin'],
            expect(url).toContain('scope=read+write+admin');
        it('should include state parameter when provided', () => {
            const url = manager.getAuthorizationUrl('csrf-state-123');
            expect(url).toContain('state=csrf-state-123');
        it('should include additional params when provided', () => {
            const url = manager.getAuthorizationUrl(undefined, { prompt: 'consent', foo: 'bar' });
            expect(url).toContain('prompt=consent');
            expect(url).toContain('foo=bar');
    describe('exchangeAuthorizationCode', () => {
        it('should exchange code for tokens', async () => {
            mockFetch.mockResolvedValue({
                json: vi.fn().mockResolvedValue({
                    access_token: 'new-access-token',
                    refresh_token: 'new-refresh-token',
                    expires_in: 3600,
            const result = await manager.exchangeAuthorizationCode('auth-code-123');
            expect(result.accessToken).toBe('new-access-token');
            expect(result.refreshToken).toBe('new-refresh-token');
            // Verify fetch was called with correct params
            expect(mockFetch).toHaveBeenCalledWith(
                'https://auth.example.com/token',
                    headers: expect.objectContaining({
    describe('getClientCredentialsToken', () => {
        it('should obtain token via client credentials', async () => {
                scopes: ['api.read'],
                    access_token: 'cc-access-token',
                    expires_in: 7200,
            const result = await manager.getClientCredentialsToken();
            expect(result.accessToken).toBe('cc-access-token');
    describe('refreshAccessToken', () => {
        it('should refresh token using refresh token', async () => {
                refreshToken: 'my-refresh-token',
                    access_token: 'refreshed-access-token',
            const result = await manager.refreshAccessToken();
            expect(result.accessToken).toBe('refreshed-access-token');
        it('should throw when no refresh token is available', async () => {
            await expect(manager.refreshAccessToken()).rejects.toThrow('No refresh token available');
    describe('getAccessToken', () => {
        it('should return existing valid token', async () => {
                accessToken: 'valid-token',
            const token = await manager.getAccessToken();
            expect(token).toBe('valid-token');
            expect(mockFetch).not.toHaveBeenCalled();
        it('should refresh token when expired', async () => {
                accessToken: 'expired-token',
                refreshToken: 'refresh-tok',
                tokenExpiresAt: Date.now() - 1000, // expired
                    access_token: 'new-token',
            expect(token).toBe('new-token');
        it('should fall back to client credentials when no refresh token', async () => {
                tokenExpiresAt: Date.now() - 1000,
                    access_token: 'cc-token',
            expect(token).toBe('cc-token');
        it('should throw when refresh fails', async () => {
                accessToken: 'expired',
                refreshToken: 'refresh',
                ok: false,
                status: 400,
                statusText: 'Bad Request',
                text: vi.fn().mockResolvedValue('invalid_grant'),
            await expect(manager.getAccessToken()).rejects.toThrow('Failed to refresh access token');
        it('should deduplicate concurrent refresh requests', async () => {
                    access_token: 'deduped-token',
            // Call getAccessToken twice concurrently
            const [token1, token2] = await Promise.all([
                manager.getAccessToken(),
            expect(token1).toBe('deduped-token');
            expect(token2).toBe('deduped-token');
            // fetch should only be called once because the second request should reuse the same promise
            expect(mockFetch).toHaveBeenCalledTimes(1);
    describe('requestToken (via public methods)', () => {
        it('should use tokenRequestTransform when configured', async () => {
            const transform = vi.fn((params: Record<string, string>) => ({
                extra_param: 'added',
                scopes: ['api'],
                tokenRequestTransform: transform,
                json: vi.fn().mockResolvedValue({ access_token: 'tok', expires_in: 3600 }),
            await manager.getClientCredentialsToken();
            expect(transform).toHaveBeenCalled();
        it('should use tokenResponseTransform when configured', async () => {
            const transform = vi.fn(() => ({
                access_token: 'transformed-token',
                expires_in: 1800,
                tokenResponseTransform: transform,
                json: vi.fn().mockResolvedValue({ non_standard: 'response' }),
            expect(result.accessToken).toBe('transformed-token');
        it('should include additional headers in token request', async () => {
                additionalHeaders: { 'X-Custom-Header': 'custom-value' },
                expect.anything(),
                        'X-Custom-Header': 'custom-value',
        it('should call onTokenUpdate callback', async () => {
            const onUpdate = vi.fn();
                onTokenUpdate: onUpdate,
                    access_token: 'new-tok',
                    refresh_token: 'new-ref',
            expect(onUpdate).toHaveBeenCalledWith(
                    accessToken: 'new-tok',
                    refreshToken: 'new-ref',
        it('should throw on HTTP error response', async () => {
                statusText: 'Unauthorized',
                text: vi.fn().mockResolvedValue('invalid_client'),
            await expect(manager.getClientCredentialsToken()).rejects.toThrow(
                'OAuth2 token request failed'
        it('should assume 1-year expiration when no expires_in', async () => {
                    access_token: 'long-lived-token',
                    // No expires_in
            expect(result.accessToken).toBe('long-lived-token');
            // Expiration should be roughly 1 year from now
            const oneYear = 365 * 24 * 60 * 60 * 1000;
            expect(result.expiresAt).toBeGreaterThan(Date.now() + oneYear - 10000);
    describe('setTokens', () => {
        it('should set access token', () => {
            manager.setTokens('manual-token');
            const state = manager.getTokenState();
            expect(state!.accessToken).toBe('manual-token');
        it('should set refresh token when provided', () => {
            manager.setTokens('access', 'refresh');
            expect(state!.refreshToken).toBe('refresh');
        it('should set expiration when expiresIn is provided', () => {
            manager.setTokens('access', undefined, 3600);
            expect(state!.expiresAt).toBeGreaterThanOrEqual(before + 3600000);
            expect(state!.expiresAt).toBeLessThanOrEqual(after + 3600000);
        it('should invoke onTokenUpdate callback', () => {
            manager = new OAuth2Manager({ ...baseConfig, onTokenUpdate: onUpdate });
            manager.setTokens('tok', 'ref', 1800);
                    refreshToken: 'ref',
    describe('clearTokens', () => {
        it('should clear all tokens', () => {
            expect(manager.getTokenState()).not.toBeNull();
            manager.clearTokens();
    describe('getTokenState', () => {
        it('should return null when no tokens', () => {
        it('should return current token state', () => {
                tokenExpiresAt: 1234567890,
            expect(state).toEqual({
                expiresAt: 1234567890,
