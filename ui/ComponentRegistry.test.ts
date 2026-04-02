 * Unit tests for the ComponentRegistry package.
 * Tests: config schema validation and loadConfig behavior.
const { mockSearch } = vi.hoisted(() => {
  const defaultConfig = {
    databaseSettings: { connectionTimeout: 5000, requestTimeout: 10000 },
    dbDatabase: 'TestDB',
    dbUsername: 'admin',
    dbPassword: 'secret',
    dbTrustServerCertificate: false,
  const mockSearch = vi.fn().mockReturnValue({
    config: defaultConfig,
    filepath: '/default/mj.config.cjs',
    isEmpty: false,
  return { mockSearch };
    search: mockSearch,
import { loadConfig, type ConfigInfo, type ComponentRegistrySettings } from '../config';
describe('loadConfig', () => {
  it('should throw when config file not found', () => {
    mockSearch.mockReturnValue(null);
    expect(() => loadConfig()).toThrow('Config file not found.');
  it('should throw when config file is empty', () => {
    mockSearch.mockReturnValue({ config: {}, filepath: '/mj.config.cjs', isEmpty: true });
    expect(() => loadConfig()).toThrow('is empty or does not exist');
  it('should parse a valid config', () => {
    mockSearch.mockReturnValue({
        dbHost: 'db.example.com',
      filepath: '/mj.config.cjs',
    const config = loadConfig();
    expect(config.dbHost).toBe('db.example.com');
    expect(config.dbDatabase).toBe('TestDB');
    expect(config.dbUsername).toBe('admin');
    expect(config.mjCoreSchema).toBe('__mj');
  it('should parse componentRegistrySettings when present', () => {
        componentRegistrySettings: {
          port: 3300,
          enableRegistry: true,
          requireAuth: true,
          corsOrigins: ['http://localhost:4200'],
    expect(config.componentRegistrySettings).toBeDefined();
    expect(config.componentRegistrySettings!.port).toBe(3300);
    expect(config.componentRegistrySettings!.enableRegistry).toBe(true);
    expect(config.componentRegistrySettings!.requireAuth).toBe(true);
    expect(config.componentRegistrySettings!.corsOrigins).toEqual(['http://localhost:4200']);
  it('should transform dbTrustServerCertificate boolean to Y/N', () => {
        dbTrustServerCertificate: true,
    expect(config.dbTrustServerCertificate).toBe('Y');
