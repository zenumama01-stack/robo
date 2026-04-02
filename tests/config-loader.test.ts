import { buildMJConfig } from '../config-loader';
// Mock cosmiconfig at the module level
    cosmiconfig: vi.fn()
describe('buildMJConfig', () => {
    it('should return empty object when no package defaults provided', () => {
        const result = buildMJConfig({});
        expect(result).toEqual({});
    it('should include codegen defaults', () => {
        const result = buildMJConfig({
            codegen: { outputDir: './generated', language: 'typescript' }
        expect(result.outputDir).toBe('./generated');
        expect(result.language).toBe('typescript');
    it('should include server defaults', () => {
            server: { port: 4000, host: 'localhost' }
        expect(result.host).toBe('localhost');
    it('should include mcpServer defaults', () => {
            mcpServer: { mcpPort: 5000 }
        expect(result.mcpPort).toBe(5000);
    it('should include a2aServer defaults', () => {
            a2aServer: { a2aPort: 6000 }
        expect(result.a2aPort).toBe(6000);
    it('should include queryGen defaults under queryGen key', () => {
        const queryGenConfig = { maxResults: 100, timeout: 30 };
            queryGen: queryGenConfig
        expect(result.queryGen).toEqual(queryGenConfig);
    it('should merge multiple package defaults', () => {
            codegen: { outputDir: './gen' },
            server: { port: 4000 }
        expect(result.outputDir).toBe('./gen');
    it('should deep merge overlapping package defaults', () => {
            codegen: { database: { host: 'localhost' } },
            server: { database: { port: 1433 } }
        expect(result.database).toEqual({
            host: 'localhost',
            port: 1433
    it('should apply user config overrides on top of package defaults', () => {
        const result = buildMJConfig(
            { server: { port: 4000, host: 'localhost' } },
            { port: 8080 }
        expect(result.port).toBe(8080);
    it('should handle undefined userConfigOverrides', () => {
            { server: { port: 4000 } },
    it('should give user overrides highest priority', () => {
                codegen: { mode: 'codegen-mode' },
                server: { mode: 'server-mode' }
            { mode: 'user-mode' }
        expect(result.mode).toBe('user-mode');
    it('should handle all package defaults provided together', () => {
            codegen: { codegenFlag: true },
            server: { serverFlag: true },
            mcpServer: { mcpFlag: true },
            a2aServer: { a2aFlag: true },
            queryGen: { queryFlag: true }
        expect(result.codegenFlag).toBe(true);
        expect(result.serverFlag).toBe(true);
        expect(result.mcpFlag).toBe(true);
        expect(result.a2aFlag).toBe(true);
        expect(result.queryGen).toEqual({ queryFlag: true });
describe('loadMJConfig', () => {
    let mockExplorer: { search: ReturnType<typeof vi.fn> };
        mockExplorer = {
            search: vi.fn()
    it('should return defaults when no config file is found', async () => {
        const { cosmiconfig } = await import('cosmiconfig');
        (cosmiconfig as ReturnType<typeof vi.fn>).mockReturnValue({
            search: vi.fn().mockResolvedValue(null)
        const { loadMJConfig } = await import('../config-loader');
        const result = await loadMJConfig({
            defaultConfig: { port: 3000 }
        expect(result.config).toEqual({ port: 3000 });
        expect(result.hasUserConfig).toBe(false);
        expect(result.overriddenKeys).toEqual([]);
        expect(result.configFilePath).toBeUndefined();
    it('should throw when requireConfigFile is true and no config found', async () => {
        await expect(loadMJConfig({
            requireConfigFile: true,
            searchFrom: '/some/path'
        })).rejects.toThrow('No mj.config.cjs file found');
    it('should merge user config with defaults when config file is found', async () => {
            search: vi.fn().mockResolvedValue({
                config: { port: 8080, customKey: true },
                filepath: '/app/mj.config.cjs'
            defaultConfig: { port: 3000, host: 'localhost' }
        expect(result.config).toEqual({
            port: 8080,
            customKey: true
        expect(result.hasUserConfig).toBe(true);
        expect(result.configFilePath).toBe('/app/mj.config.cjs');
    it('should identify overridden keys', async () => {
                config: { port: 8080 },
        expect(result.overriddenKeys).toContain('port');
        expect(result.overriddenKeys).not.toContain('host');
    it('should not identify key as overridden when value is same as default', async () => {
                config: { port: 3000 },
        expect(result.overriddenKeys).not.toContain('port');
    it('should use process.cwd() as default searchFrom', async () => {
        const mockSearch = vi.fn().mockResolvedValue(null);
            search: mockSearch
        await loadMJConfig();
        expect(mockSearch).toHaveBeenCalledWith(process.cwd());
    it('should pass searchFrom to cosmiconfig search', async () => {
        await loadMJConfig({ searchFrom: '/custom/path' });
        expect(mockSearch).toHaveBeenCalledWith('/custom/path');
    it('should log when verbose is true and config found', async () => {
        const logSpy = vi.spyOn(console, 'log').mockImplementation(() => {});
        await loadMJConfig({
        expect(logSpy).toHaveBeenCalledWith(expect.stringContaining('Loading MemberJunction configuration'));
    it('should log when verbose is true and no config found', async () => {
        await loadMJConfig({ verbose: true });
        expect(logSpy).toHaveBeenCalledWith(expect.stringContaining('No user config file found'));
    it('should handle verbose logging with many overridden keys', async () => {
        // Create an override with more than 10 keys
        const overrides: Record<string, number> = {};
        const defaults: Record<string, number> = {};
            overrides[`key${i}`] = i * 10;
            defaults[`key${i}`] = i;
                config: overrides,
            defaultConfig: defaults
        expect(logSpy).toHaveBeenCalledWith(expect.stringContaining('more'));
describe('loadMJConfigSync', () => {
    it('should throw an error when config file cannot be loaded', async () => {
        // loadMJConfigSync uses require(), which will fail for nonexistent files
        const { loadMJConfigSync } = await import('../config-loader');
            loadMJConfigSync('/nonexistent/path/config.cjs');
        }).toThrow('Failed to load config from');
const mockSearch = vi.fn();
import { loadCLIConfig } from '../utils/config-loader';
describe('config-loader', () => {
  describe('loadCLIConfig', () => {
    it('should return defaults when no cached config exists', () => {
      const config = loadCLIConfig();
      expect(config.defaultFormat).toBe('console');
      expect(config.failFast).toBe(false);
      expect(config.parallel).toBe(false);
      expect(config.maxParallelTests).toBe(5);
      expect(config.timeout).toBe(300000);
    it('should return a database configuration with defaults', () => {
      expect(config.database!.host).toBe('localhost');
      expect(config.database!.schema).toBe('__mj');
