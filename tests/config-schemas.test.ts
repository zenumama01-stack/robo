 * Unit tests for MCP Server configuration Zod schemas and utility functions.
 * Tests:
 * - Zod schema parsing and defaults for all config sub-schemas
 * - resolveAuthSettings logic (tested indirectly via schema shapes)
 * - validateJwtSigningSecret logic patterns
 * - Type exports
 * We do NOT import config.ts directly (it has side effects: dotenv, cosmiconfig,
 * console.log). Instead we replicate the Zod schemas and pure functions to
 * unit-test the parsing logic in isolation.
// Replicate the Zod schemas from config.ts for isolated testing
// (importing config.ts directly triggers dotenv side-effects)
// Replicate validateJwtSigningSecret
// Replicate resolveAuthSettings
type MCPServerAuthSettingsInfo = z.infer<typeof mcpServerAuthSettingsSchema>;
    authConfig: MCPServerAuthSettingsInfo | undefined,
    port: number | undefined,
    if (!authConfig) return defaults;
describe('Entity Tool Schema', () => {
    it('should parse minimal entity tool config with defaults', () => {
        const result = mcpServerEntityToolInfoSchema.parse({});
        expect(result.get).toBe(false);
        expect(result.create).toBe(false);
        expect(result.update).toBe(false);
        expect(result.delete).toBe(false);
        expect(result.runView).toBe(false);
    it('should parse full entity tool config', () => {
        const result = mcpServerEntityToolInfoSchema.parse({
            schemaName: '__mj',
            get: true,
            create: false,
            update: true,
            delete: false,
            runView: true,
        expect(result.entityName).toBe('MJ: Users');
        expect(result.schemaName).toBe('__mj');
        expect(result.get).toBe(true);
        expect(result.runView).toBe(true);
describe('Action Tool Schema', () => {
    it('should parse with defaults', () => {
        const result = mcpServerActionToolInfoSchema.parse({});
        expect(result.discover).toBe(false);
        expect(result.execute).toBe(false);
    it('should parse with action name and category', () => {
        const result = mcpServerActionToolInfoSchema.parse({
            actionName: 'Send Email',
            actionCategory: 'Communication',
            discover: true,
            execute: true,
        expect(result.actionName).toBe('Send Email');
        expect(result.actionCategory).toBe('Communication');
describe('Agent Tool Schema', () => {
        const result = mcpServerAgentToolInfoSchema.parse({});
        expect(result.status).toBe(false);
        expect(result.cancel).toBe(false);
    it('should parse full agent tool config', () => {
        const result = mcpServerAgentToolInfoSchema.parse({
            agentName: 'SkipAgent',
            status: true,
            cancel: false,
        expect(result.agentName).toBe('SkipAgent');
        expect(result.status).toBe(true);
describe('Query Tool Schema', () => {
        const result = mcpServerQueryToolInfoSchema.parse({});
        expect(result.enabled).toBe(false);
    it('should parse with schema filters', () => {
        const result = mcpServerQueryToolInfoSchema.parse({
            allowedSchemas: ['__mj', 'dbo'],
            blockedSchemas: ['sys'],
        expect(result.enabled).toBe(true);
        expect(result.allowedSchemas).toEqual(['__mj', 'dbo']);
        expect(result.blockedSchemas).toEqual(['sys']);
describe('Communication Tool Schema', () => {
        const result = mcpServerCommunicationToolInfoSchema.parse({});
    it('should parse with providers', () => {
        const result = mcpServerCommunicationToolInfoSchema.parse({
            allowedProviders: ['email', 'sms'],
        expect(result.allowedProviders).toEqual(['email', 'sms']);
describe('Prompt Tool Schema', () => {
        const result = mcpServerPromptToolInfoSchema.parse({});
describe('OAuth Proxy Settings Schema', () => {
    it('should parse empty object with all defaults', () => {
        const result = oauthProxySettingsSchema.parse({});
        expect(result.clientTtlMs).toBe(86400000); // 24h
        expect(result.stateTtlMs).toBe(600000); // 10m
        expect(result.jwtExpiresIn).toBe('1h');
        expect(result.jwtIssuer).toBe('urn:mj:mcp-server');
        expect(result.enableConsentScreen).toBe(false);
    it('should accept custom values', () => {
        const result = oauthProxySettingsSchema.parse({
            clientTtlMs: 3600000,
            jwtSigningSecret: 'a-long-enough-secret-that-passes-validation-ok',
            jwtExpiresIn: '2h',
            jwtIssuer: 'urn:custom:issuer',
            enableConsentScreen: true,
        expect(result.upstreamProvider).toBe('azure-ad');
        expect(result.clientTtlMs).toBe(3600000);
        expect(result.jwtExpiresIn).toBe('2h');
        expect(result.enableConsentScreen).toBe(true);
describe('Auth Settings Schema', () => {
    it('should parse empty object with defaults', () => {
        const result = mcpServerAuthSettingsSchema.parse({});
        expect(result.mode).toBe('both');
        expect(result.autoResourceIdentifier).toBe(true);
        const modes = ['apiKey', 'oauth', 'both', 'none'] as const;
        for (const mode of modes) {
            const result = mcpServerAuthSettingsSchema.parse({ mode });
            expect(result.mode).toBe(mode);
    it('should reject invalid mode', () => {
        const parseResult = mcpServerAuthSettingsSchema.safeParse({ mode: 'invalid' });
        expect(parseResult.success).toBe(false);
    it('should accept scopes array', () => {
        const result = mcpServerAuthSettingsSchema.parse({
            scopes: ['openid', 'profile', 'api://client-id/.default'],
        expect(result.scopes).toHaveLength(3);
describe('MCP Server Info Schema', () => {
        const result = mcpServerInfoSchema.parse({});
        expect(result.port).toBe(3100);
        expect(result.enableMCPServer).toBe(false);
    it('should coerce port from string', () => {
        const result = mcpServerInfoSchema.parse({ port: '4000' });
        expect(result.port).toBe(4000);
    it('should parse full config with entity and action tools', () => {
        const result = mcpServerInfoSchema.parse({
            port: 5000,
            enableMCPServer: true,
            systemApiKey: 'test-key-123',
            entityTools: [
                { entityName: 'MJ: Users', get: true, runView: true },
                { entityName: 'MJ: AI Agents', get: true },
            actionTools: [
                { actionCategory: 'Communication', discover: true, execute: true },
            agentTools: [
                { agentName: '*', discover: true, execute: true, status: true },
        expect(result.port).toBe(5000);
        expect(result.enableMCPServer).toBe(true);
        expect(result.entityTools).toHaveLength(2);
        expect(result.actionTools).toHaveLength(1);
        expect(result.agentTools).toHaveLength(1);
describe('validateJwtSigningSecret', () => {
    it('should reject undefined secret', () => {
        const result = validateJwtSigningSecret(undefined);
        expect(result.error).toContain('not configured');
    it('should reject short secret', () => {
        const result = validateJwtSigningSecret('short');
        expect(result.error).toContain('too short');
    it('should accept 32+ byte UTF-8 secret', () => {
        const secret = 'this-is-a-long-enough-secret-key!!'; // 34 chars
        const result = validateJwtSigningSecret(secret);
        expect(result.decodedSecret).toBe(secret);
    it('should accept base64-encoded 32-byte secret', () => {
        // 32 bytes in base64 is 44 characters
        const secret = 'AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=';
    it('should reject base64 that decodes to less than 32 bytes', () => {
        // 8 bytes base64 -> short
        const result = validateJwtSigningSecret('AQIDBA==');
        // This is short in base64 decode (4 bytes), so should try as utf-8
        // 'AQIDBA==' is 8 chars utf-8 -> still short
describe('resolveAuthSettings', () => {
    it('should return defaults when config is undefined', () => {
        const result = resolveAuthSettings(undefined, undefined);
    it('should auto-generate resourceIdentifier from port', () => {
        const result = resolveAuthSettings(
            { mode: 'oauth', autoResourceIdentifier: true },
            4000,
        expect(result.resourceIdentifier).toBe('http://localhost:4000');
    it('should use default port 3100 when port is undefined', () => {
            { mode: 'both', autoResourceIdentifier: true },
        expect(result.resourceIdentifier).toBe('http://localhost:3100');
    it('should not auto-generate resourceIdentifier when already set', () => {
        expect(result.resourceIdentifier).toBe('https://mcp.example.com');
    it('should not auto-generate when autoResourceIdentifier is false', () => {
            { mode: 'both', autoResourceIdentifier: false },
        expect(result.resourceIdentifier).toBeUndefined();
    it('should preserve scopes from config', () => {
                scopes: ['openid', 'profile'],
            3100,
        expect(result.scopes).toEqual(['openid', 'profile']);
