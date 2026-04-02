import { createRequire } from 'node:module';
import { mergeConfigs, MergeOptions } from './config-merger';
// Use createRequire to load CommonJS config files
const require = createRequire(import.meta.url);
 * Configuration loading options
export interface LoadConfigOptions {
   * Directory to start searching for config file.
   * Default: process.cwd()
  searchFrom?: string;
   * If true, throws error when config file is not found.
   * If false, returns empty object when no config file exists.
  requireConfigFile?: boolean;
   * Merge behavior options
  mergeOptions?: MergeOptions;
   * If true, logs configuration loading details
   * Default configuration to use as base.
   * Typically provided by the calling package.
  defaultConfig?: Record<string, any>;
 * Result of configuration loading
export interface LoadConfigResult<T = Record<string, any>> {
   * Final merged configuration
  config: T;
   * Path to user config file (undefined if using only defaults)
  configFilePath?: string;
   * True if user config file was found and loaded
  hasUserConfig: boolean;
   * Keys that were overridden from defaults
  overriddenKeys: string[];
 * Loads and merges MemberJunction configuration from multiple sources:
 * 1. Default configuration (provided by calling package)
 * 2. Optional mj.config.cjs file (found via cosmiconfig)
 * 3. Environment variables (applied on top of merged config)
 * @param options - Configuration loading options
 * @returns Merged configuration result
export async function loadMJConfig<T = Record<string, any>>(
  options: LoadConfigOptions = {}
): Promise<LoadConfigResult<T>> {
    searchFrom = process.cwd(),
    requireConfigFile = false,
    mergeOptions = {},
    verbose = false,
    defaultConfig = {}
    console.log(`\n📄 Loading MemberJunction configuration...`);
    console.log(`   Search directory: ${searchFrom}`);
  // Search for user config file
  const explorer = cosmiconfig('mj', {
    searchPlaces: [
      'mj.config.cjs',
      '.mjrc',
      '.mjrc.js',
      '.mjrc.cjs',
      'package.json' // Look for "mj" key
  const searchResult = await explorer.search(searchFrom);
  if (!searchResult) {
    if (requireConfigFile) {
        `No mj.config.cjs file found in ${searchFrom} or parent directories. ` +
        `Either create a config file or set requireConfigFile: false to use defaults.`
      console.log(`   ℹ No user config file found, using defaults only`);
      config: defaultConfig as T,
      hasUserConfig: false,
      overriddenKeys: []
    console.log(`   ✓ Found config file: ${searchResult.filepath}`);
  // Merge user config into defaults
  const userConfig = searchResult.config;
  const mergedConfig = mergeConfigs(defaultConfig, userConfig, mergeOptions);
  // Identify overridden keys for logging
  const overriddenKeys = identifyOverriddenKeys(defaultConfig, userConfig);
    console.log(`   ✓ Merged ${overriddenKeys.length} configuration override(s)`);
    if (overriddenKeys.length > 0 && overriddenKeys.length <= 10) {
      console.log(`   Overridden keys: ${overriddenKeys.join(', ')}`);
    } else if (overriddenKeys.length > 10) {
      console.log(`   Overridden keys: ${overriddenKeys.slice(0, 10).join(', ')}, ... (${overriddenKeys.length - 10} more)`);
    console.log(`   ✓ Configuration loaded successfully\n`);
    config: mergedConfig as T,
    configFilePath: searchResult.filepath,
    hasUserConfig: true,
    overriddenKeys
 * Identifies which top-level keys were overridden in user config
function identifyOverriddenKeys(
  defaults: Record<string, any>,
  overrides: Record<string, any>
  if (!overrides) return [];
  return Object.keys(overrides).filter(key => {
    const hasOverride = key in overrides;
    // Only count as override if the value is different
    const isDifferent = JSON.stringify(defaults[key]) !== JSON.stringify(overrides[key]);
    return hasOverride && isDifferent;
 * Loads configuration synchronously (for CommonJS compatibility).
 * Note: This does NOT search for config files, only loads from explicit path.
 * @param configPath - Explicit path to config file
 * @param options - Loading options
export function loadMJConfigSync<T = Record<string, any>>(
  configPath: string,
  options: Omit<LoadConfigOptions, 'searchFrom' | 'requireConfigFile'> = {}
): T {
  const { defaultConfig = {}, mergeOptions = {} } = options;
    const userConfig = require(configPath);
    return mergedConfig as T;
    throw new Error(`Failed to load config from ${configPath}: ${error.message}`);
 * Helper to build a complete MJ config by merging configurations from multiple packages.
 * Each package provides its own default configuration.
 * @param packageDefaults - Object with package-specific default configs
 * @param userConfigOverrides - Optional user overrides
 * @returns Merged configuration
export function buildMJConfig(
  packageDefaults: {
    codegen?: Record<string, any>;
    server?: Record<string, any>;
    mcpServer?: Record<string, any>;
    a2aServer?: Record<string, any>;
    queryGen?: Record<string, any>;
  userConfigOverrides?: Record<string, any>
  // Start with empty config
  let config: Record<string, any> = {};
  // Merge each package's defaults
  if (packageDefaults.codegen) {
    config = mergeConfigs(config, packageDefaults.codegen);
  if (packageDefaults.server) {
    config = mergeConfigs(config, packageDefaults.server);
  if (packageDefaults.mcpServer) {
    config = mergeConfigs(config, packageDefaults.mcpServer);
  if (packageDefaults.a2aServer) {
    config = mergeConfigs(config, packageDefaults.a2aServer);
  if (packageDefaults.queryGen) {
    config = { ...config, queryGen: packageDefaults.queryGen };
  // Apply user overrides
  if (userConfigOverrides) {
    config = mergeConfigs(config, userConfigOverrides);
 * Configuration loading utilities
export class ConfigLoader {
   * Load configuration from file
   * Supports environment variable expansion using ${ENV_VAR} syntax
  public static async load(configPath: string): Promise<DBAutoDocConfig> {
      const content = await fs.readFile(configPath, 'utf-8');
      // Expand environment variables in the content
      const expandedContent = this.expandEnvVars(content);
      const config = JSON.parse(expandedContent) as DBAutoDocConfig;
      this.validate(config);
      throw new Error(`Failed to load configuration: ${(error as Error).message}`);
   * Expand environment variables in string
   * Supports ${VAR_NAME} syntax
  private static expandEnvVars(content: string): string {
    return content.replace(/\$\{([^}]+)\}/g, (match, varName) => {
      const value = process.env[varName];
        throw new Error(`Environment variable ${varName} is not defined`);
   * Save configuration to file
  public static async save(config: DBAutoDocConfig, configPath: string): Promise<void> {
      const dir = path.dirname(configPath);
      const content = JSON.stringify(config, null, 2);
      await fs.writeFile(configPath, content, 'utf-8');
      throw new Error(`Failed to save configuration: ${(error as Error).message}`);
   * Create default configuration
  public static createDefault(): DBAutoDocConfig {
        database: '',
        user: '',
        trustServerCertificate: false,
        provider: 'gemini',
        cardinalityThreshold: 20,
        sampleSize: 10,
          maxDepth: 3
        stateFile: './db-doc-state.json',
        sqlFile: './output/add-descriptions.sql',
        markdownFile: './output/database-documentation.md'
      schemas: {
        include: [],
        exclude: ['sys', 'INFORMATION_SCHEMA']
      tables: {
        exclude: ['sysdiagrams', '__MigrationHistory']
   * Validate configuration
  private static validate(config: DBAutoDocConfig): void {
    if (!config.database) {
      throw new Error('Missing database configuration');
    if (!config.database.server) {
      throw new Error('Missing database.server');
    if (!config.database.database) {
      throw new Error('Missing database.database');
    if (!config.ai) {
      throw new Error('Missing AI configuration');
    if (!config.ai.provider) {
      throw new Error('Missing ai.provider');
    if (!config.ai.model) {
      throw new Error('Missing ai.model');
    if (!config.ai.apiKey) {
      throw new Error('Missing ai.apiKey');
 * @fileoverview Configuration loader for CLI
import { CLIConfig } from '../types';
// Load environment variables BEFORE loading config
// This ensures process.env is populated when mj.config.cjs is evaluated
    // Database settings
    dbPort?: number | string;
    // Testing CLI specific settings
    testing?: {
        defaultFormat?: 'console' | 'json' | 'markdown';
    // Legacy format database config
let cachedConfig: MJConfig | null = null;
 * Load MJ configuration from mj.config.cjs
 * @returns Full MJ configuration
export async function loadMJConfig(): Promise<MJConfig> {
    if (cachedConfig) {
        return cachedConfig;
    // Clear any existing require cache for mj.config.cjs to ensure env vars are re-evaluated
    // This is necessary because mj.config.cjs uses process.env.DB_DATABASE directly
    const configPath = path.resolve(process.cwd(), 'mj.config.cjs');
    delete require.cache[configPath];
    // Create a new explorer instance to ensure fresh config load with current env vars
        cache: false  // Disable caching to ensure fresh load
        throw new Error(`No mj.config.cjs configuration found. Ensure you're running from the MJ repository root.`);
    cachedConfig = result.config as MJConfig;
 * Load testing CLI configuration with defaults
 * @returns CLI configuration
export function loadCLIConfig(): CLIConfig {
    // Synchronous version for backward compatibility
    // Uses cached config if available, otherwise returns defaults
    const testingConfig = cachedConfig?.testing || {};
        defaultEnvironment: testingConfig.defaultEnvironment || process.env.MJ_TEST_ENV || 'dev',
        defaultFormat: testingConfig.defaultFormat || 'console',
        failFast: testingConfig.failFast ?? false,
        parallel: testingConfig.parallel ?? false,
        maxParallelTests: testingConfig.maxParallelTests || 5,
        timeout: testingConfig.timeout || 300000,  // 5 minutes
        database: cachedConfig?.database || {
            host: cachedConfig?.dbHost || 'localhost',
            name: cachedConfig?.dbDatabase,
            port: typeof cachedConfig?.dbPort === 'string' ? parseInt(cachedConfig.dbPort) : cachedConfig?.dbPort,
            username: cachedConfig?.dbUsername,
            password: cachedConfig?.dbPassword,
            schema: cachedConfig?.coreSchema || '__mj'
