import dotenv from 'dotenv';
dotenv.config({ quiet: true });
import { z } from 'zod';
import { cosmiconfigSync } from 'cosmiconfig';
import { LogError, LogStatus } from '@memberjunction/core';
import { mergeConfigs } from '@memberjunction/config';
import { DEFAULT_SERVER_CONFIG } from '@memberjunction/server';
const explorer = cosmiconfigSync('mj');
const databaseSettingsInfoSchema = z.object({
  connectionTimeout: z.number(),
  requestTimeout: z.number(),
  dbReadOnlyUsername: z.string().optional(),
  dbReadOnlyPassword: z.string().optional(),
const a2aServerEntityCapabilitySchema = z.object({
  entityName: z.string().optional(),
  schemaName: z.string().optional(),
  get: z.boolean().optional().default(false),
  create: z.boolean().optional().default(false),
  update: z.boolean().optional().default(false),
  delete: z.boolean().optional().default(false),
  runView: z.boolean().optional().default(false),
const a2aServerAgentCapabilitySchema = z.object({
  agentName: z.string().optional(),
  discover: z.boolean().optional().default(false),
  execute: z.boolean().optional().default(false),
  monitor: z.boolean().optional().default(false),
  cancel: z.boolean().optional().default(false),
const a2aServerInfoSchema = z.object({
  port: z.coerce.number().optional().default(3200),
  entityCapabilities: z.array(a2aServerEntityCapabilitySchema).optional(),
  agentCapabilities: z.array(a2aServerAgentCapabilitySchema).optional(),
  enableA2AServer: z.boolean().optional().default(false),
  agentName: z.string().optional().default("MemberJunction"),
  agentDescription: z.string().optional().default("MemberJunction A2A Agent"),
  streamingEnabled: z.boolean().optional().default(true),
  userEmail: z.string().optional().describe("Email address of the user to use for entity operations"),
const configInfoSchema = z.object({
  databaseSettings: databaseSettingsInfoSchema,
  dbHost: z.string().default('localhost'),
  dbDatabase: z.string(),
  dbPort: z.number({ coerce: true }).default(1433),
  dbUsername: z.string(),
  dbPassword: z.string(),
  dbTrustServerCertificate: z.coerce
    .boolean()
    .default(false)
    .transform((v) => (v ? 'Y' : 'N')),
  dbInstanceName: z.string().optional(),
  // A2A Server settings
  a2aServerSettings: a2aServerInfoSchema.optional(),
  mjCoreSchema: z.string(),
export type DatabaseSettingsInfo = z.infer<typeof databaseSettingsInfoSchema>;
export type ConfigInfo = z.infer<typeof configInfoSchema>;
export const configInfo: ConfigInfo = loadConfig();
export const {
  dbUsername,
  dbPassword,
  dbHost,
  dbInstanceName,
  a2aServerSettings,
  mjCoreSchema: mj_core_schema,
  dbReadOnlyUsername,
  dbReadOnlyPassword,
} = configInfo;
export function loadConfig(): ConfigInfo {
  const configSearchResult = explorer.search(process.cwd());
  // Start with DEFAULT_SERVER_CONFIG as base
  let mergedConfig = DEFAULT_SERVER_CONFIG;
  // If user config exists, merge it with defaults
  if (configSearchResult && !configSearchResult.isEmpty) {
    LogStatus(`A2A Server: Config file found at ${configSearchResult.filepath}`);
    // Merge user config with defaults (user config takes precedence)
    mergedConfig = mergeConfigs(DEFAULT_SERVER_CONFIG, configSearchResult.config);
    LogStatus(`A2A Server: No config file found, using DEFAULT_SERVER_CONFIG`);
  // Validate the merged configuration
  const configParsing = configInfoSchema.safeParse(mergedConfig);
  if (!configParsing.success) {
    LogError('Error parsing config file', '', JSON.stringify(configParsing.error.issues, null, 2));
    throw new Error('Configuration validation failed');
  return configParsing.data;
import { cosmiconfig } from 'cosmiconfig';
// Load environment variables
export interface AICliConfig {
  // Database settings from mj.config.cjs
  dbHost?: string;
  dbDatabase?: string;
  dbPort?: number;
  dbUsername?: string;
  dbPassword?: string;
  coreSchema?: string;
  // AI CLI specific settings
  aiSettings?: {
    defaultTimeout?: number;
    outputFormat?: 'compact' | 'json' | 'table';
    logLevel?: 'info' | 'debug' | 'verbose';
    enableChat?: boolean;
    chatHistoryLimit?: number;
export async function loadAIConfig(): Promise<AICliConfig> {
  const explorer = cosmiconfig('mj');
  const result = await explorer.search();
  if (!result) {
    throw new Error(`❌ No mj.config.cjs configuration found
Problem: MJ configuration file is missing
Likely cause: You may not be in a MemberJunction workspace directory
Next steps:
1. Ensure you're running from the MJ repository root
2. Verify mj.config.cjs exists in the current directory
3. Check that the config file exports database connection settings
For help configuring MJ, see: https://docs.memberjunction.org`);
  return result.config;
 * @fileoverview Configuration loader for the MemberJunction MCP Server.
 * Loads configuration from:
 * 1. Environment variables (.env file via dotenv - searches upward from cwd)
 * 2. Configuration file (mj.config.cjs via cosmiconfig)
 * 3. Default server configuration as fallback
 * Configuration is validated using Zod schemas to ensure type safety.
 * @module @memberjunction/ai-mcp-server/config
import * as fs from 'fs';
 * IMPORTANT: Why we use dynamic import() for DEFAULT_SERVER_CONFIG
 * We generally disallow dynamic imports in this codebase, but this is a necessary exception.
 * Problem:
 * - @memberjunction/server's config.ts reads process.env at module load time to create DEFAULT_SERVER_CONFIG
 * - ESM hoists all static imports, so they execute BEFORE any inline code in this file
 * - If we use `import { DEFAULT_SERVER_CONFIG } from '@memberjunction/server'` at the top,
 *   it runs before our dotenv.config() call, and process.env is empty
 * Solution:
 * - Load dotenv first (above) which populates process.env with DB credentials from .env
 * - Use dynamic import() inside initConfig() to load DEFAULT_SERVER_CONFIG AFTER dotenv runs
 * - This ensures process.env.DB_DATABASE, etc. are set when @memberjunction/server evaluates them
 * Alternative approaches considered:
 * - Creating a separate bootstrap entry point - adds complexity
 * - Using Node.js -r flag to preload dotenv - not portable across npm scripts
 * - Modifying MJServer - out of scope for this package
 * Searches upward from the current directory for a .env file.
 * This mimics cosmiconfig's search behavior so the .env file is found
 * in the same location as mj.config.cjs (typically the repo root).
 * @returns Path to the .env file if found, undefined otherwise
function findEnvFile(): string | undefined {
  let currentDir = process.cwd();
  const root = path.parse(currentDir).root;
  while (currentDir !== root) {
    const envPath = path.join(currentDir, '.env');
    if (fs.existsSync(envPath)) {
      return envPath;
    currentDir = path.dirname(currentDir);
// Load .env file from repo root (searches upward like cosmiconfig does for mj.config.cjs)
const envPath = findEnvFile();
if (envPath) {
  console.log(`MCP Server: Loading environment from ${envPath}`);
  dotenv.config({ path: envPath, quiet: true });
  console.log(`MCP Server: No .env file found, using existing environment variables`);
const explorer = cosmiconfigSync('mj', { searchStrategy: 'global' });
const mcpServerEntityToolInfoSchema = z.object({
const mcpServerActionToolInfoSchema = z.object({
  actionName: z.string().optional(),
  actionCategory: z.string().optional(),
const mcpServerQueryToolInfoSchema = z.object({
  enabled: z.boolean().optional().default(false),
  allowedSchemas: z.array(z.string()).optional(),
  blockedSchemas: z.array(z.string()).optional(),
const mcpServerPromptToolInfoSchema = z.object({
  promptName: z.string().optional(),
  promptCategory: z.string().optional(),
const mcpServerCommunicationToolInfoSchema = z.object({
  allowedProviders: z.array(z.string()).optional(),
const mcpServerAgentToolInfoSchema = z.object({
  status: z.boolean().optional().default(false),
 * Zod schema for OAuth Proxy settings.
 * The OAuth proxy enables dynamic client registration (RFC 7591) for MCP clients.
const oauthProxySettingsSchema = z.object({
  /** Enable the OAuth proxy authorization server */
  enabled: z.boolean().default(false),
   * Upstream provider to use for authentication.
   * This should match one of the configured auth providers by name.
   * If not specified, the first available provider is used.
  upstreamProvider: z.string().optional(),
  /** TTL for registered clients in milliseconds (default: 24 hours) */
  clientTtlMs: z.number().default(24 * 60 * 60 * 1000),
  /** TTL for authorization state in milliseconds (default: 10 minutes) */
  stateTtlMs: z.number().default(10 * 60 * 1000),
   * Secret key for signing proxy-issued JWTs (HS256).
   * Must be at least 32 bytes (256 bits). Can be base64 encoded.
   * Generate with: node -e "console.log(require('crypto').randomBytes(32).toString('base64'))"
   * If not set, the proxy will pass through upstream tokens instead of issuing its own.
  jwtSigningSecret: z.string().optional(),
   * JWT expiration time for proxy-issued tokens.
   * @default '1h' (1 hour)
  jwtExpiresIn: z.string().default('1h'),
   * Issuer claim for proxy-signed JWTs.
   * @default 'urn:mj:mcp-server'
  jwtIssuer: z.string().default('urn:mj:mcp-server'),
   * Enable the consent screen for users to select scopes.
   * When enabled, users will see a UI to approve/deny scope requests
   * after authenticating with the upstream provider.
  enableConsentScreen: z.boolean().default(false),
 * Zod schema for OAuth authentication settings.
 * Token audience for validation is automatically derived from auth provider config
 * (e.g., WEB_CLIENT_ID for Azure AD), matching the same approach used by MJExplorer.
 * No additional configuration is required for basic OAuth to work.
 * Validates the auth configuration section with defaults:
 * - mode: defaults to 'both' (accepts API keys or OAuth tokens)
 * - resourceIdentifier: MCP server URL for Protected Resource Metadata (auto-generated if not set)
 * - autoResourceIdentifier: defaults to true (auto-generate from server URL)
const mcpServerAuthSettingsSchema = z.object({
  /** Authentication mode: 'apiKey' | 'oauth' | 'both' | 'none' */
  mode: z.enum(['apiKey', 'oauth', 'both', 'none']).default('both'),
  /** Resource identifier for MCP clients - the server URL (e.g., "http://localhost:3100") */
  resourceIdentifier: z.string().optional(),
   * @deprecated Token audience is now derived from auth provider config (same as MJExplorer).
   * This field is ignored - audience validation uses the provider's `audience` field
   * which is auto-populated from environment variables like WEB_CLIENT_ID.
  tokenAudience: z.string().optional(),
   * OAuth scopes to include in Protected Resource Metadata.
   * Used for MCP client discovery - tells clients what scopes to request from the IdP.
   * For Azure AD: use ["api://{client-id}/.default"]
   * If not set, uses standard OIDC scopes ["openid", "profile", "email"]
  scopes: z.array(z.string()).optional(),
  /** Auto-generate resourceIdentifier from server URL if not specified */
  autoResourceIdentifier: z.boolean().default(true),
   * OAuth Proxy settings - enables dynamic client registration for MCP clients.
   * When enabled, the MCP Server acts as an OAuth Authorization Server that
   * proxies authentication to the configured upstream provider (Azure AD, Auth0, etc.).
  proxy: oauthProxySettingsSchema.optional(),
const mcpServerInfoSchema = z.object({
  port: z.coerce.number().optional().default(3100),
  entityTools: z.array(mcpServerEntityToolInfoSchema).optional(),
  actionTools: z.array(mcpServerActionToolInfoSchema).optional(),
  agentTools: z.array(mcpServerAgentToolInfoSchema).optional(),
  queryTools: mcpServerQueryToolInfoSchema.optional(),
  promptTools: z.array(mcpServerPromptToolInfoSchema).optional(),
  communicationTools: mcpServerCommunicationToolInfoSchema.optional(),
  enableMCPServer: z.boolean().optional().default(false),
  systemApiKey: z.string().optional(),
  /** OAuth authentication settings for the MCP Server */
  auth: mcpServerAuthSettingsSchema.optional(),
  // MCP Server settings
  mcpServerSettings: mcpServerInfoSchema.optional(),
  // System API key from MJ_API_KEY environment variable
  // Note: This may come from DEFAULT_SERVER_CONFIG, but initConfig() also reads
  // directly from process.env to handle cases where dotenv runs after MJServer import
  apiKey: z.string().optional(),
export type MCPServerEntityToolInfo = z.infer<typeof mcpServerEntityToolInfoSchema>;
export type MCPServerActionToolInfo = z.infer<typeof mcpServerActionToolInfoSchema>;
export type MCPServerAgentToolInfo = z.infer<typeof mcpServerAgentToolInfoSchema>;
export type MCPServerQueryToolInfo = z.infer<typeof mcpServerQueryToolInfoSchema>;
export type MCPServerPromptToolInfo = z.infer<typeof mcpServerPromptToolInfoSchema>;
export type MCPServerCommunicationToolInfo = z.infer<typeof mcpServerCommunicationToolInfoSchema>;
export type MCPServerAuthSettingsInfo = z.infer<typeof mcpServerAuthSettingsSchema>;
export type OAuthProxySettingsInfo = z.infer<typeof oauthProxySettingsSchema>;
// Config will be loaded asynchronously - exports are populated by initConfig()
export let configInfo: ConfigInfo;
export let dbUsername: string;
export let dbPassword: string;
export let dbHost: string;
export let dbDatabase: string;
export let dbPort: number;
export let dbTrustServerCertificate: string;
export let dbInstanceName: string | undefined;
export let mcpServerSettings: ConfigInfo['mcpServerSettings'];
export let mj_core_schema: string;
export let dbReadOnlyUsername: string | undefined;
export let dbReadOnlyPassword: string | undefined;
/** OAuth authentication settings, resolved with defaults */
export let mcpServerAuth: MCPServerAuthSettingsInfo;
let _initialized = false;
 * Initializes the configuration by loading DEFAULT_SERVER_CONFIG dynamically.
 * This MUST be called before accessing any config values.
 * The dynamic import ensures dotenv has already populated process.env
 * before @memberjunction/server reads environment variables.
export async function initConfig(): Promise<ConfigInfo> {
  if (_initialized) {
    return configInfo;
  // Dynamic import ensures dotenv has loaded before this module is evaluated
  const { DEFAULT_SERVER_CONFIG } = await import('@memberjunction/server');
  configInfo = loadConfig(DEFAULT_SERVER_CONFIG);
  // Ensure apiKey is read directly from process.env (dotenv has definitely run by now)
  // DEFAULT_SERVER_CONFIG.apiKey may have been evaluated before dotenv ran if @memberjunction/server
  // was imported elsewhere first
  const envApiKey = process.env.MJ_API_KEY;
  console.log(`[Config] apiKey sources: configInfo.apiKey=${configInfo.apiKey ? `"${configInfo.apiKey.substring(0, 10)}..." (${configInfo.apiKey.length} chars)` : 'undefined'}, process.env.MJ_API_KEY=${envApiKey ? `"${envApiKey.substring(0, 10)}..." (${envApiKey.length} chars)` : 'undefined'}`);
  if (!configInfo.apiKey && envApiKey) {
    console.log('[Config] Using MJ_API_KEY from process.env (DEFAULT_SERVER_CONFIG was stale)');
    configInfo.apiKey = envApiKey;
  // Populate the exported variables
  dbUsername = configInfo.dbUsername;
  dbPassword = configInfo.dbPassword;
  dbHost = configInfo.dbHost;
  dbDatabase = configInfo.dbDatabase;
  dbPort = configInfo.dbPort;
  dbTrustServerCertificate = configInfo.dbTrustServerCertificate;
  dbInstanceName = configInfo.dbInstanceName;
  mcpServerSettings = configInfo.mcpServerSettings;
  mj_core_schema = configInfo.mjCoreSchema;
  dbReadOnlyUsername = configInfo.dbReadOnlyUsername;
  dbReadOnlyPassword = configInfo.dbReadOnlyPassword;
  // Resolve auth settings with defaults
  mcpServerAuth = resolveAuthSettings(configInfo.mcpServerSettings?.auth, configInfo.mcpServerSettings?.port);
/** Minimum required length for JWT signing secret (32 bytes = 256 bits) */
const MIN_JWT_SECRET_LENGTH = 32;
 * Validates the JWT signing secret for the OAuth proxy.
 * @param secret - The JWT signing secret (may be base64 encoded)
 * @returns Object with validation result and decoded secret
function validateJwtSigningSecret(secret: string | undefined): {
  decodedSecret?: string;
  if (!secret) {
    return { valid: false, error: 'JWT signing secret is not configured' };
  // Try to decode base64 first to get actual byte length
  let secretBytes: Buffer;
    // Check if it looks like base64 (only alphanumeric, +, /, =)
    if (/^[A-Za-z0-9+/=]+$/.test(secret) && secret.length % 4 === 0) {
      secretBytes = Buffer.from(secret, 'base64');
      // If base64 decode gives reasonable output, use it
      if (secretBytes.length >= MIN_JWT_SECRET_LENGTH) {
        return { valid: true, decodedSecret: secret };
    // Otherwise treat as raw string
    secretBytes = Buffer.from(secret, 'utf-8');
  if (secretBytes.length < MIN_JWT_SECRET_LENGTH) {
      error: `JWT signing secret is too short (${secretBytes.length} bytes). Minimum required: ${MIN_JWT_SECRET_LENGTH} bytes (256 bits). Generate with: node -e "console.log(require('crypto').randomBytes(32).toString('base64'))"`,
 * Resolves OAuth authentication settings with defaults.
 * @param authConfig - The raw auth configuration from mj.config.cjs
 * @param port - The MCP server port for auto-generating resourceIdentifier
 * @returns Resolved auth settings with all defaults applied
function resolveAuthSettings(
  authConfig: z.infer<typeof mcpServerAuthSettingsSchema> | undefined,
  port: number | undefined
): MCPServerAuthSettingsInfo {
  // Start with defaults
  const defaults: MCPServerAuthSettingsInfo = {
    mode: 'both',
    autoResourceIdentifier: true,
  if (!authConfig) {
  // Merge with config values
  const resolved: MCPServerAuthSettingsInfo = {
    mode: authConfig.mode ?? defaults.mode,
    resourceIdentifier: authConfig.resourceIdentifier,
    tokenAudience: authConfig.tokenAudience,
    scopes: authConfig.scopes,
    autoResourceIdentifier: authConfig.autoResourceIdentifier ?? defaults.autoResourceIdentifier,
    proxy: authConfig.proxy,
  // Auto-generate resourceIdentifier if needed
  if (!resolved.resourceIdentifier && resolved.autoResourceIdentifier) {
    const serverPort = port ?? 3100;
    resolved.resourceIdentifier = `http://localhost:${serverPort}`;
  // Validate JWT signing secret if OAuth proxy is enabled with JWT signing
  if (resolved.proxy?.enabled && resolved.proxy?.jwtSigningSecret) {
    const secretValidation = validateJwtSigningSecret(resolved.proxy.jwtSigningSecret);
    if (!secretValidation.valid) {
      console.error(`[Config] OAuth Proxy Error: ${secretValidation.error}`);
      console.error(`[Config] OAuth Proxy will be DISABLED due to invalid JWT signing secret`);
      console.warn(`[Config] Falling back to API key authentication only`);
      // Disable the proxy but keep the rest of auth settings
      resolved.proxy = {
        ...resolved.proxy,
      // If mode was 'oauth', fall back to 'apiKey'
      if (resolved.mode === 'oauth') {
        console.warn(`[Config] Auth mode changed from 'oauth' to 'apiKey' because OAuth proxy is disabled`);
        resolved.mode = 'apiKey';
      console.log(`[Config] JWT signing secret validated (${MIN_JWT_SECRET_LENGTH}+ bytes)`);
 * Loads and validates the MCP server configuration.
 * Configuration loading order:
 * 1. Searches for mj.config.cjs in the current directory and parent directories
 * 2. Merges found configuration with DEFAULT_SERVER_CONFIG
 * 3. Validates the merged configuration using Zod schema
 * @param defaultConfig - The default server configuration to use as base
 * @returns Validated configuration object
 * @throws Error if configuration validation fails
function loadConfig(defaultConfig: Record<string, unknown>): ConfigInfo {
  let mergedConfig = defaultConfig;
    LogStatus(`MCP Server: Config file found at ${configSearchResult.filepath}`);
    mergedConfig = mergeConfigs(defaultConfig, configSearchResult.config);
    LogStatus(`MCP Server: No config file found, using DEFAULT_SERVER_CONFIG`);
    LogError('Error parsing config file', null, JSON.stringify(configParsing.error.issues, null, 2));
 * Configuration options for the Azure AI client
export interface AzureAIConfig {
   * The Azure API key for authentication
   * The Azure endpoint URL
   * Whether to use Azure AD authentication instead of API key
   * When true, the DefaultAzureCredential will be used
  useAzureAD?: boolean;
import env from 'env-var';
export const BETTY_BOT_BASE_URL: string = env.get('BETTY_BOT_BASE_URL').default("https://betty-api.tasio.co/").asString();export const REX_API_HOST: string = env.get('REX_API_HOST').asString();
export const REX_RECOMMEND_HOST: string = env.get('REX_RECOMMEND_HOST').asString();
export const REX_USERNAME: string = env.get('REX_USERNAME').asString();
export const REX_PASSWORD: string = env.get('REX_PASSWORD').asString();
export const REX_API_KEY: string = env.get('REX_API_KEY').asString();
export const REX_BATCH_SIZE: number = env.get('REX_BATCH_SIZE').default(200).asInt();export const openAIAPIKey: string = process.env.OPENAI_API_KEY;
export const pineconeHost: string = process.env.PINECONE_HOST;
export const pineconeAPIKey: string = process.env.PINECONE_API_KEY;
export const pineconeDefaultIndex: string = process.env.PINECONE_DEFAULT_INDEX;
export const dbHost = process.env.DB_HOST;
export const dbPort = Number(process.env.DB_PORT) || 1433;
export const dbUsername = process.env.DB_USERNAME;
export const dbPassword = process.env.DB_PASSWORD;
export const dbDatabase = process.env.DB_DATABASE;
export const serverPort = Number(process.env.PORT) || 8000;
export const currentUserEmail = process.env.CURRENT_USER_EMAIL;
export const mistralAPIKey = process.env.MISTRAL_API_KEY;export const ApolloAPIEndpoint = 'https://api.apollo.io/v1';
export const EmailSourceName = "Apollo.io"
export const GroupSize = 10; // number of records per group to send to API, max number is 10
export const ConcurrentGroups = 1; // number of groups to process concurrently
export const MaxPeopleToEnrichPerOrg = 500;
export const ApolloAPIKey = process.env.APOLLO_API_KEY || ""; * Configuration schema for external API integrations used by Core Actions
const apiIntegrationsSchema = z.object({
   * Perplexity AI API Key for AI-powered web search
   * Used by: Perplexity Search action
   * Get your API key from: https://www.perplexity.ai/settings/api
  perplexityApiKey: z.string().optional(),
   * Gamma API Key for presentation generation
   * Used by: Gamma Generate Presentation action
   * Get your API key from: https://gamma.app/settings (requires Pro or higher account)
   * API keys follow format: sk-gamma-xxxxxxxx
  gammaApiKey: z.string().optional(),
   * Google services configuration (nested structure)
   * Follows MJStorage pattern for better organization and scalability
  google: z.object({
     * Google Custom Search configuration
     * Used by: Google Custom Search action
     * Get your API key from: https://developers.google.com/custom-search/v1/overview
     * Get your CX from: https://programmablesearchengine.google.com/
    customSearch: z.object({
       * Google Custom Search API key
       * Google Custom Search engine identifier (CX)
      cx: z.string().optional(),
    }).optional(),
 * Complete configuration schema for Core Actions package
const coreActionsConfigSchema = z.object({
   * API integrations configuration for external services
  apiIntegrations: apiIntegrationsSchema.optional().default({}),
export type CoreActionsConfig = z.infer<typeof coreActionsConfigSchema>;
export type ApiIntegrationsConfig = z.infer<typeof apiIntegrationsSchema>;
let _config: CoreActionsConfig | null = null;
 * Gets the Core Actions configuration, loading it from mj.config.cjs if not already loaded
 * @returns The Core Actions configuration object
export function getCoreActionsConfig(): CoreActionsConfig {
  if (_config) {
    return _config;
    const result = explorer.search();
    if (!result || result.isEmpty) {
      LogStatus('No mj.config.cjs found, using default Core Actions configuration');
      _config = coreActionsConfigSchema.parse({});
    // Extract only the fields relevant to Core Actions
    const rawConfig = {
      apiIntegrations: {
        perplexityApiKey: result.config?.perplexityApiKey || process.env.PERPLEXITY_API_KEY,
        gammaApiKey: result.config?.gammaApiKey || process.env.GAMMA_API_KEY,
        google: {
          customSearch: {
            apiKey: result.config?.google?.customSearch?.apiKey ||
                    result.config?.googleCustomSearchApiKey ||  // Backwards compatibility
                    process.env.GOOGLE_CUSTOM_SEARCH_API_KEY,
            cx: result.config?.google?.customSearch?.cx ||
                result.config?.googleCustomSearchCx ||  // Backwards compatibility
                process.env.GOOGLE_CUSTOM_SEARCH_CX,
    _config = coreActionsConfigSchema.parse(rawConfig);
    LogError('Error loading Core Actions configuration', undefined, error);
 * Gets the API integrations configuration
 * @returns The API integrations configuration object
export function getApiIntegrationsConfig(): ApiIntegrationsConfig {
  const config = getCoreActionsConfig();
  return config.apiIntegrations;
 * Clears the cached configuration (useful for testing)
export function clearCoreActionsConfig(): void {
  _config = null;
export const mjCoreSchema = env.get('MJ_CORE_SCHEMA').required().asString();
export const currentUserEmail = env.get('CURRENT_USER_EMAIL').required().asString();
export const serverPort = env.get('PORT').default('8000').asPortNumber();
export const autoRefreshInterval = env.get('METADATA_AUTO_REFRESH_INTERVAL').default('3600000').asIntPositive();
 * Configuration management module for MemberJunction CodeGen.
 * Handles loading, parsing, and validation of configuration files using Zod schemas.
 * Supports various configuration sources through cosmiconfig (package.json, .mjrc, etc.).
import { logStatus } from '../Misc/status_logging';
import { mergeConfigs, parseBooleanEnv } from '@memberjunction/config';
/** Global configuration explorer for finding MJ config files */
/** Initial config search result from current working directory */
 * Represents a general configuration setting with name-value pairs
export type SettingInfo = z.infer<typeof settingInfoSchema>;
const settingInfoSchema = z.object({
  /** The name/key of the setting */
  name: z.string(),
  /** The value of the setting (can be any type) */
  value: z.any(),
 * Configuration for logging behavior during code generation
export type LogInfo = z.infer<typeof logInfoSchema>;
const logInfoSchema = z.object({
  /** Whether logging is enabled */
  log: z.boolean().default(true),
  /** File path for log output */
  logFile: z.string().default('codegen.output.log'),
  /** Whether to also log to console */
  console: z.boolean().default(true),
 * Configuration for custom SQL scripts to run at specific times during code generation
export type CustomSQLScript = z.infer<typeof customSQLScriptSchema>;
const customSQLScriptSchema = z.object({
  /** When to run the script (e.g., 'before-all', 'after-all') */
  when: z.string(),
  /** Path to the SQL script file */
  scriptFile: z.string(),
 * Configuration for external commands to run during code generation
export type CommandInfo = z.infer<typeof commandInfoSchema>;
const commandInfoSchema = z.object({
  /** Working directory to run the command from */
  workingDirectory: z.string(),
  /** The command to execute */
  command: z.string(),
  /** Command line arguments */
  args: z.string().array(),
  /** Optional timeout in milliseconds */
  timeout: z.number().nullish(),
  /** When to run the command (e.g., 'before', 'after') */
 * Configuration option for output generation
export type OutputOptionInfo = z.infer<typeof outputOptionInfoSchema>;
const outputOptionInfoSchema = z.object({
  /** Name of the output option */
  /** Value of the output option */
 * Configuration for code generation output destinations and options
export type OutputInfo = z.infer<typeof outputInfoSchema>;
const outputInfoSchema = z.object({
  /** Type of output (e.g., 'SQL', 'Angular', 'GraphQLServer') */
  type: z.string(),
  /** Directory path for output files */
  directory: z.string(),
  /** Whether to append additional output code subdirectory */
  appendOutputCode: z.boolean().optional(),
  /** Additional options for this output type */
  options: outputOptionInfoSchema.array().optional(),
 * Information about a database table for exclusion or filtering
export type TableInfo = z.infer<typeof tableInfoSchema>;
const tableInfoSchema = z.object({
  /** Schema name (supports wildcards like '%') */
  schema: z.string(),
  /** Table name (supports wildcards like 'sys%') */
  table: z.string(),
 * Configuration bundle for generating database schema JSON output
export type DBSchemaJSONOutputBundle = z.infer<typeof dbSchemaJSONOutputBundleSchema>;
const dbSchemaJSONOutputBundleSchema = z.object({
  /** Name of the bundle */
  /** Schemas to include in this bundle */
  schemas: z.string().array().default([]),
  /** Schemas to exclude from this bundle */
  excludeSchemas: z.string().array().default(['sys', 'staging']),
  /** Entities to exclude from this bundle */
  excludeEntities: z.string().array().default([]),
export type DBSchemaJSONOutput = z.infer<typeof dbSchemaJSONOutputSchema>;
const dbSchemaJSONOutputSchema = z.object({
  excludeEntities: z.string().array(),
  excludeSchemas: z.string().array().default(['sys', 'staging', 'dbo']),
  bundles: dbSchemaJSONOutputBundleSchema.array().default([{ name: '_Core_Apps', excludeSchemas: ['__mj'] }]),
export type NewUserSetup = z.infer<typeof newUserSetupSchema>;
const newUserSetupSchema = z.object({
  UserName: z.string(),
  FirstName: z.string(),
  LastName: z.string(),
  Email: z.string(),
  Roles: z.string().array().default(['Developer', 'Integration', 'UI']),
  CreateUserApplicationRecords: z.boolean().optional().default(false),
  UserApplications: z.array(z.string()).optional().default([]),
 * Configuration option for an advanced generation feature
export type AdvancedGenerationFeatureOption = z.infer<typeof advancedGenerationFeatureOptionSchema>;
const advancedGenerationFeatureOptionSchema = z.object({
  /** Name of the option */
  /** Value of the option (can be any type) */
  value: z.unknown(),
 * Configuration for an AI-powered advanced generation feature
export type AdvancedGenerationFeature = z.infer<typeof advancedGenerationFeatureSchema>;
const advancedGenerationFeatureSchema = z.object({
  /** Name of the feature */
  /** Whether the feature is enabled */
  enabled: z.boolean(),
  /** Description for documentation (not used by code) */
  description: z.string().nullish(),
  /** System prompt for AI interaction */
  systemPrompt: z.string().nullish(),
  /** User message template for AI interaction */
  userMessage: z.string().nullish(),
  /** Additional options for the feature */
  options: advancedGenerationFeatureOptionSchema.array().nullish(),
export type AdvancedGeneration = z.infer<typeof advancedGenerationSchema>;
const advancedGenerationSchema = z.object({
  enableAdvancedGeneration: z.boolean().default(true),
  // NOTE: AIVendor and AIModel have been removed. Model configuration is now per-prompt
  // in the AI Prompts table via the MJ: AI Prompt Models relationship.
  features: advancedGenerationFeatureSchema.array().default([
      name: 'EntityNames',
      description: 'Use AI to generate better entity names when creating new entities',
      name: 'SmartFieldIdentification',
      description:
        'Use AI to determine the Name Field, Default In View fields, and Searchable fields for entities. This sets IsNameField, DefaultInView, and IncludeInUserSearchAPI properties on entity fields. Only applies when new entities/fields are created or when fields allow auto-update.',
      name: 'DefaultInViewFields',
        'Use AI to determine which fields in an entity should be shown, by default, in a newly created User View for the entity. This is only used when creating new entities and when new fields are detected.',
      name: 'EntityDescriptions',
      description: 'Use AI to generate descriptions for entities, only used when creating new entities',
      name: 'EntityFieldDescriptions',
      description: 'Use AI to generate descriptions for fields, only used when new fields are detected',
      name: 'FormLayout',
        'Use AI to generate better layouts for forms. This includes using AI to determine the way to layout fields on each entity form. The field will still be laid out in the order they are defined in the entity, but the AI will determine the best way to layout the fields on the form. Since generated forms are regenerated every time you run this tool, it will be done every time you run the tool, including for existing entities and fields.',
      name: 'FormTabs',
        "Use AI to decide which entity relationships should have visible tabs and the best order to display those tabs. All relationships will be generated based on the Database Schema, but the EntityRelationship.DisplayInForm. The idea is that the AI will pick which of these tabs should be visible by default. In some cases an entity will have a large # of relationships and it isn't necessarily a good idea to display all of them. This feature only applies when an entity is created or new Entity Relationships are detected. This tool will not change existing EntityRelationship records.",
      name: 'VirtualEntityFieldDecoration',
        'Use AI to analyze SQL view definitions for virtual entities and identify primary keys, foreign keys, and field descriptions. Only runs for virtual entities that lack soft PK/FK annotations. Respects explicit config-defined PKs/FKs (from additionalSchemaInfo) — LLM fills in the gaps.',
export type IntegrityCheckConfig = z.infer<typeof integrityCheckConfigSchema>;
const integrityCheckConfigSchema = z.object({
  entityFieldsSequenceCheck: z.boolean(),
export type ForceRegenerationConfig = z.infer<typeof forceRegenerationConfigSchema>;
const forceRegenerationConfigSchema = z.object({
   * Force regeneration of all SQL objects even if no schema changes are detected
   * Optional SQL WHERE clause to filter entities for forced regeneration
   * Example: "SchemaName = 'dbo' AND Name LIKE 'User%'"
  entityWhereClause: z.string().optional(),
   * Force regeneration of base views
  baseViews: z.boolean().default(false),
   * Force regeneration of spCreate procedures
  spCreate: z.boolean().default(false),
   * Force regeneration of spUpdate procedures
  spUpdate: z.boolean().default(false),
   * Force regeneration of spDelete procedures
  spDelete: z.boolean().default(false),
   * Force regeneration of all stored procedures
  allStoredProcedures: z.boolean().default(false),
   * Force regeneration of indexes for foreign keys
  indexes: z.boolean().default(false),
   * Force regeneration of full text search components
  fullTextSearch: z.boolean().default(false),
export type SQLOutputConfig = z.infer<typeof sqlOutputConfigSchema>;
const sqlOutputConfigSchema = z.object({
   * Whether or not sql statements generated while managing metadata should be written to a file
  enabled: z.boolean().default(true),
   * The path of the folder to use when logging is enabled.
   * If provided, a file will be created with the format "CodeGen_Run_yyyy-mm-dd_hh-mm-ss.sql"
  folderPath: z.string().default('../../migrations/v3/'),
   * Optional, the file name that will be written WITHIN the folderPath specified.
  fileName: z.string().optional(),
   * If set to true, then we append to the existing file, if one exists, otherwise we create a new file.
  appendToFile: z.boolean().default(true),
   * If true, all mention of the core schema within the log file will be replaced with the flyway schema,
   * ${flyway:defaultSchema}
  convertCoreSchemaToFlywayMigrationFile: z.boolean().default(true),
   * If true, scripts that are being emitted via SQL logging that are marked by CodeGen as recurring will be SKIPPED. Defaults to false
  omitRecurringScriptsFromLog: z.boolean().default(false),
   * Optional array of schema-to-placeholder mappings for Flyway migrations.
   * Each mapping specifies a database schema name and its corresponding Flyway placeholder.
   * If not provided, defaults to replacing the MJ core schema with ${flyway:defaultSchema}.
   * [
   *   { schema: '__mj', placeholder: '${mjSchema}' },
   *   { schema: '__BCSaaS', placeholder: '${flyway:defaultSchema}' }
   * ]
  schemaPlaceholders: z.array(z.object({
    placeholder: z.string()
  })).optional(),
export type NewSchemaDefaults = z.infer<typeof newSchemaDefaultsSchema>;
const newSchemaDefaultsSchema = z.object({
  CreateNewApplicationWithSchemaName: z.boolean().default(true),
const entityPermissionSchema = z.object({
  RoleName: z.string(),
  CanRead: z.boolean(),
  CanCreate: z.boolean(),
  CanUpdate: z.boolean(),
  CanDelete: z.boolean(),
 * Permission settings for an entity role
export type EntityPermission = z.infer<typeof entityPermissionSchema>;
const newEntityPermissionDefaultsSchema = z.object({
  AutoAddPermissionsForNewEntities: z.boolean().default(true),
  Permissions: entityPermissionSchema.array().default([
    { RoleName: 'UI', CanRead: true, CanCreate: false, CanUpdate: false, CanDelete: false },
    { RoleName: 'Developer', CanRead: true, CanCreate: true, CanUpdate: true, CanDelete: false },
    { RoleName: 'Integration', CanRead: true, CanCreate: true, CanUpdate: true, CanDelete: true },
 * Default permission settings for new entities
export type NewEntityPermissionDefaults = z.infer<typeof newEntityPermissionDefaultsSchema>;
export type EntityNameRulesBySchema = z.infer<typeof newEntityNameRulesBySchema>;
const newEntityNameRulesBySchema = z.object({
  SchemaName: z.string(),
  EntityNamePrefix: z.string().default(''),
  EntityNameSuffix: z.string().default(''),
const newEntityDefaultsSchema = z.object({
  TrackRecordChanges: z.boolean().default(true),
  AuditRecordAccess: z.boolean().default(false),
  AuditViewRuns: z.boolean().default(false),
  AllowAllRowsAPI: z.boolean().default(false),
  AllowCreateAPI: z.boolean().default(true),
  AllowUpdateAPI: z.boolean().default(true),
  AllowDeleteAPI: z.boolean().default(true),
  AllowUserSearchAPI: z.boolean().default(true),
  CascadeDeletes: z.boolean().default(false),
  UserViewMaxRows: z.number().default(1000),
  AddToApplicationWithSchemaName: z.boolean().default(true),
  IncludeFirstNFieldsAsDefaultInView: z.number().default(5),
  PermissionDefaults: newEntityPermissionDefaultsSchema,
  NameRulesBySchema: newEntityNameRulesBySchema.array().default([]),
export type NewEntityRelationshipDefaults = z.infer<typeof newEntityRelationshipDefaultsSchema>;
const newEntityRelationshipDefaultsSchema = z.object({
  AutomaticallyCreateRelationships: z.boolean().default(true),
  CreateOneToManyRelationships: z.boolean().default(true),
 * Default settings applied when creating new entities
export type NewEntityDefaults = z.infer<typeof newEntityDefaultsSchema>;
 * Main configuration object containing all CodeGen settings
  newUserSetup: newUserSetupSchema.nullish(),
  settings: settingInfoSchema.array().default([
    { name: 'mj_core_schema', value: '__mj' },
    { name: 'skip_database_generation', value: false },
    { name: 'auto_index_foreign_keys', value: true },
  excludeTables: tableInfoSchema.array().default([
    { schema: '%', table: 'sys%' },
    { schema: '%', table: 'flyway_schema_history' }
  customSQLScripts: customSQLScriptSchema.array().default([
      scriptFile: '../../SQL Scripts/MJ_BASE_BEFORE_SQL.sql',
      when: 'before-all',
  advancedGeneration: advancedGenerationSchema.nullish(),
  integrityChecks: integrityCheckConfigSchema.default({
    entityFieldsSequenceCheck: true,
  output: outputInfoSchema.array().default([
    { type: 'SQL', directory: '../../SQL Scripts/generated', appendOutputCode: true },
    { type: 'Angular', directory: '../MJExplorer/src/app/generated', options: [{ name: 'maxComponentsPerModule', value: 20 }] },
      type: 'AngularCoreEntities',
      directory: '../Angular/Explorer/core-entity-forms/src/lib/generated',
      options: [{ name: 'maxComponentsPerModule', value: 100 }],
    { type: 'GraphQLServer', directory: '../MJAPI/src/generated' },
    { type: 'GraphQLCoreEntityResolvers', directory: '../MJServer/src/generated' },
    { type: 'CoreActionSubclasses', directory: '../Actions/CoreActions/src/generated' },
    { type: 'ActionSubclasses', directory: '../GeneratedActions/src/generated' },
    { type: 'CoreEntitySubclasses', directory: '../MJCoreEntities/src/generated' },
    { type: 'EntitySubclasses', directory: '../GeneratedEntities/src/generated' },
    { type: 'DBSchemaJSON', directory: '../../Schema Files' },
  commands: commandInfoSchema.array().default([
    { workingDirectory: '../MJCoreEntities', command: 'npm', args: ['run', 'build'], when: 'after' },
    { workingDirectory: '../Angular/Explorer/core-entity-forms', command: 'npm', args: ['run', 'build'], when: 'after' },
    { workingDirectory: '../Actions/CoreActions', command: 'npm', args: ['run', 'build'], when: 'after' },
    { workingDirectory: '../GeneratedEntities', command: 'npm', args: ['run', 'build'], when: 'after' },
    { workingDirectory: '../GeneratedActions', command: 'npm', args: ['run', 'build'], when: 'after' },
    { workingDirectory: '../MJServer', command: 'npm', args: ['run', 'build'], when: 'after' },
    { workingDirectory: '../MJAPI', command: 'npm', args: ['start'], timeout: 30000, when: 'after' },
  /** Path to JSON file containing soft PK/FK definitions for tables without database constraints */
  additionalSchemaInfo: z.string().optional(),
  logging: logInfoSchema,
  newEntityDefaults: newEntityDefaultsSchema,
  newSchemaDefaults: newSchemaDefaultsSchema,
  dbSchemaJSONOutput: dbSchemaJSONOutputSchema,
  newEntityRelationshipDefaults: newEntityRelationshipDefaultsSchema,
  SQLOutput: sqlOutputConfigSchema,
  forceRegeneration: forceRegenerationConfigSchema,
  dbHost: z.string(),
  dbPort: z.coerce.number().int().positive().default(1433),
  codeGenLogin: z.string(),
  codeGenPassword: z.string(),
  dbInstanceName: z.string().nullish(),
  outputCode: z.string().nullish(),
  mjCoreSchema: z.string().default('__mj'),
  graphqlPort: z.coerce.number().int().positive().default(4000),
  entityPackageName: z.string().default('mj_generatedentities'),
  verboseOutput: z.boolean().optional().default(false),
 * Default CodeGen configuration - provides sensible defaults for all CodeGen settings.
 * These defaults can be overridden in user's mj.config.cjs file.
 * Database connection settings come from environment variables.
export const DEFAULT_CODEGEN_CONFIG: Partial<ConfigInfo> = {
  // Database connection settings (from environment variables)
  dbHost: process.env.DB_HOST ?? 'localhost',
  dbDatabase: process.env.DB_DATABASE ?? '',
  codeGenLogin: process.env.CODEGEN_DB_USERNAME ?? '',
  codeGenPassword: process.env.CODEGEN_DB_PASSWORD ?? '',
  dbInstanceName: process.env.DB_INSTANCE_NAME,
  dbTrustServerCertificate: parseBooleanEnv(process.env.DB_TRUST_SERVER_CERTIFICATE) ? 'Y' : 'N',
  graphqlPort: 4000,
  settings: [
    { name: 'recompile_mj_views', value: true },
  logging: {
    log: true,
    logFile: 'codegen.output.log',
    console: true,
  newEntityDefaults: {
    TrackRecordChanges: true,
    AuditRecordAccess: false,
    AuditViewRuns: false,
    AllowAllRowsAPI: false,
    AllowCreateAPI: true,
    AllowUpdateAPI: true,
    AllowDeleteAPI: true,
    AllowUserSearchAPI: false,
    CascadeDeletes: false,
    UserViewMaxRows: 1000,
    AddToApplicationWithSchemaName: true,
    IncludeFirstNFieldsAsDefaultInView: 5,
    PermissionDefaults: {
      AutoAddPermissionsForNewEntities: true,
      Permissions: [
    NameRulesBySchema: [
        SchemaName: '${mj_core_schema}',
        EntityNamePrefix: 'MJ: ',
        EntityNameSuffix: '',
  newEntityRelationshipDefaults: {
    AutomaticallyCreateRelationships: true,
    CreateOneToManyRelationships: true,
  newSchemaDefaults: {
    CreateNewApplicationWithSchemaName: true,
  excludeSchemas: ['sys', 'staging', '__mj'],
  excludeTables: [
  customSQLScripts: [],
  dbSchemaJSONOutput: {
    excludeEntities: [],
    excludeSchemas: ['sys', 'staging', 'dbo'],
    bundles: [{ name: '_Core_Apps', schemas: [], excludeSchemas: ['__mj'], excludeEntities: [] }],
  integrityChecks: {
  advancedGeneration: {
    enableAdvancedGeneration: true,
    features: [
        description: 'Use AI to determine which fields in an entity should be shown, by default, in a newly created User View for the entity. This is only used when creating new entities and when new fields are detected.',
        description: 'Use AI to identify the best name field and default field to show in views for each entity',
        name: 'TransitiveJoinIntelligence',
        description: 'Use AI to analyze entity relationships and detect junction tables for many-to-many relationships',
        name: 'FormLayoutGeneration',
        description: 'Use AI to generate semantic field categories for better form organization. This includes using AI to determine the way to layout fields on each entity form by assigning them to domain-specific categories. Since generated forms are regenerated every time you run this tool, it will be done every time you run the tool, including for existing entities and fields.',
        name: 'ParseCheckConstraints',
        description: 'Use AI to parse check constraints and generate a description as well as sub-class Validate() methods that reflect the logic of the constraint.',
        description: 'Use AI to analyze SQL view definitions for virtual entities and identify primary keys, foreign keys, and field descriptions.',
  SQLOutput: {
    folderPath: './migrations/v3/',
    appendToFile: true,
    convertCoreSchemaToFlywayMigrationFile: true,
    omitRecurringScriptsFromLog: true,
  forceRegeneration: {
    baseViews: false,
    spCreate: false,
    spUpdate: false,
    spDelete: false,
    allStoredProcedures: false,
    indexes: false,
    fullTextSearch: false,
 * Current working directory for the code generation process
export let currentWorkingDirectory: string = process.cwd();
 * Merge user config with DEFAULT_CODEGEN_CONFIG.
 * Database settings come from user config or environment variables.
const mergedConfig = configSearchResult?.config
  ? mergeConfigs(DEFAULT_CODEGEN_CONFIG, configSearchResult.config)
  : DEFAULT_CODEGEN_CONFIG;
/** Parse and validate the merged configuration */
// Don't log errors at module load - commands that need config will validate explicitly
// if (!configParsing.success) {
//   LogError('Error parsing config file', null, JSON.stringify(configParsing.error.issues, null, 2));
 * Parsed configuration object with fallback to empty object if parsing fails
export const configInfo = configParsing.data ?? ({} as ConfigInfo);
 * Destructured commonly used configuration values
export const { mjCoreSchema, dbDatabase } = configInfo;
 * Initializes configuration from the specified working directory
 * @param cwd The current working directory to search for config files
 * @returns Parsed configuration object
 * @throws Error if no configuration is found
export function initializeConfig(cwd: string): ConfigInfo {
  currentWorkingDirectory = cwd;
  // Merge user config with DEFAULT_CODEGEN_CONFIG
  const userConfigResult = explorer.search(currentWorkingDirectory);
  const mergedConfig = userConfigResult?.config
    ? mergeConfigs(DEFAULT_CODEGEN_CONFIG, userConfigResult.config)
  const maybeConfig = configInfoSchema.safeParse(mergedConfig);
  // Don't log errors - let the calling code handle validation failures
  // if (!maybeConfig.success) {
  //   LogError('Error parsing config file', null, JSON.stringify(maybeConfig.error.issues, null, 2));
  const config = maybeConfig.success ? maybeConfig.data : configInfo;
  if (config === undefined) {
    throw new Error('No configuration found');
 * Gets the output directory for a specific generation type
 * @param type The type of output (e.g., 'SQL', 'Angular')
 * @param useLocalDirectoryIfMissing Whether to use a local directory if config is missing
 * @returns The output directory path or null if not found
export function outputDir(type: string, useLocalDirectoryIfMissing: boolean): string | null {
  const outputInfo = configInfo.output.find((o) => o.type.trim().toUpperCase() === type.trim().toUpperCase());
  if (outputInfo) {
    if (outputInfo.appendOutputCode && outputInfo.appendOutputCode === true && configInfo.outputCode)
      return path.join(currentWorkingDirectory, outputInfo.directory, configInfo.outputCode);
    else return path.join(currentWorkingDirectory, outputInfo.directory);
    if (useLocalDirectoryIfMissing) {
      logStatus('>>> No output directory found for type: ' + type + ' within config file, using local directory instead');
      return path.join(currentWorkingDirectory, 'output', type);
    } else return null;
 * Gets the output options for a specific generation type
 * @param type The type of output
 * @returns Array of output options or null if not found
export function outputOptions(type: string): OutputOptionInfo[] | null {
    return outputInfo.options!;
 * Gets a specific output option value for a generation type
 * @param optionName The name of the option to retrieve
 * @param defaultValue Default value if option is not found
 * @returns The option value or default value
export function outputOptionValue(type: string, optionName: string, defaultValue?: any): any {
  const outputInfo = configInfo.output?.find((o) => o.type.trim().toUpperCase() === type.trim().toUpperCase());
  if (outputInfo && outputInfo.options) {
    const theOption = outputInfo.options.find((o) => o.name.trim().toUpperCase() === optionName.trim().toUpperCase());
    if (theOption) return theOption.value;
    else return defaultValue;
 * Gets commands configured to run at a specific time
 * @param when When the commands should run (e.g., 'before', 'after')
 * @returns Array of commands to execute
export function commands(when: string): CommandInfo[] {
  return configInfo.commands.filter((c) => c.when.trim().toUpperCase() === when.trim().toUpperCase());
 * Gets custom SQL scripts configured to run at a specific time
 * @param when When the scripts should run
 * @returns Array of SQL scripts to execute
export function customSqlScripts(when: string): CustomSQLScript[] {
  return configInfo.customSQLScripts.filter((c) => c.when.trim().toUpperCase() === when.trim().toUpperCase());
 * Gets a specific setting by name
 * @param settingName The name of the setting to retrieve
 * @returns The setting object
export function getSetting(settingName: string): SettingInfo {
  return configInfo.settings.find((s) => s.name.trim().toUpperCase() === settingName.trim().toUpperCase())!;
 * Gets the value of a specific setting
 * @param settingName The name of the setting
 * @param defaultValue Default value if setting is not found
 * @returns The setting value or default value
export function getSettingValue(settingName: string, defaultValue?: any): any {
  const setting = getSetting(settingName);
  if (setting) return setting.value;
 * Checks if automatic indexing of foreign keys is enabled
 * @returns True if auto-indexing is enabled, false otherwise
export function autoIndexForeignKeys(): boolean {
  const keyName = 'auto_index_foreign_keys';
  const setting = getSetting(keyName);
  if (setting) return <boolean>setting.value;
 * Maximum length allowed for database index names
export const MAX_INDEX_NAME_LENGTH = 128;
 * Gets the MemberJunction core schema name from configuration
 * @returns The core schema name (typically '__mj')
export function mj_core_schema(): string {
  return getSetting('mj_core_schema').value;
 * Azure/MS Graph configuration
 * These are optional at startup to allow MJAPI to run without MS Graph configured.
 * Runtime validation occurs when MSGraphProvider is instantiated.
export const AZURE_CLIENT_ID: string = env.get('AZURE_CLIENT_ID').default('').asString();
export const AZURE_AAD_ENDPOINT: string = env.get('AZURE_AAD_ENDPOINT').default('https://login.microsoftonline.com').asString();
export const AZURE_TENANT_ID: string = env.get('AZURE_TENANT_ID').default('').asString();
export const AZURE_CLIENT_SECRET: string = env.get('AZURE_CLIENT_SECRET').default('').asString();
export const AZURE_GRAPH_ENDPOINT: string = env.get('AZURE_GRAPH_ENDPOINT').default('https://graph.microsoft.com').asString();
export const AZURE_ACCOUNT_EMAIL: string = env.get('AZURE_ACCOUNT_EMAIL').default('').asString();
export const AZURE_ACCOUNT_ID: string = env.get('AZURE_ACCOUNT_ID').default('').asString();// Google API OAuth credentials - now optional to support per-request credential override
// When not set, credentials must be provided via API
export const GMAIL_CLIENT_ID = env.get('GMAIL_CLIENT_ID').default('').asString();
export const GMAIL_CLIENT_SECRET = env.get('GMAIL_CLIENT_SECRET').default('').asString();
export const GMAIL_REDIRECT_URI = env.get('GMAIL_REDIRECT_URI').default('').asString();
export const GMAIL_REFRESH_TOKEN = env.get('GMAIL_REFRESH_TOKEN').default('').asString();
// Service account email (optional)
export const GMAIL_SERVICE_ACCOUNT_EMAIL = env.get('GMAIL_SERVICE_ACCOUNT_EMAIL').default('').asString();
// Scopes for Gmail API
export const GMAIL_SCOPES = [
  'https://www.googleapis.com/auth/gmail.send',
  'https://www.googleapis.com/auth/gmail.readonly',
  'https://www.googleapis.com/auth/gmail.modify',
  'https://www.googleapis.com/auth/gmail.compose'
// TEMPORARY
export const __API_KEY = process.env.COMMUNICATION_VENDOR_API_KEY__SENDGRID;// Twilio credentials - now optional to support per-request credential override
export const TWILIO_ACCOUNT_SID = env.get('TWILIO_ACCOUNT_SID').default('').asString();
export const TWILIO_AUTH_TOKEN = env.get('TWILIO_AUTH_TOKEN').default('').asString();
export const TWILIO_PHONE_NUMBER = env.get('TWILIO_PHONE_NUMBER').default('').asString();
// Optional WhatsApp number (if using WhatsApp messaging)
export const TWILIO_WHATSAPP_NUMBER = env.get('TWILIO_WHATSAPP_NUMBER').default('').asString();
// Optional Facebook Page ID (if using Facebook Messenger)
export const TWILIO_FACEBOOK_PAGE_ID = env.get('TWILIO_FACEBOOK_PAGE_ID').default('').asString();
  metadataCacheRefreshInterval: z.number().optional().default(0),
  connectionPool: z.object({
    max: z.number().optional(),
    min: z.number().optional(),
    idleTimeoutMillis: z.number().optional(),
    acquireTimeoutMillis: z.number().optional()
  }).optional()
const componentRegistrySettingsSchema = z.object({
  port: z.number().default(3200),
  enableRegistry: z.boolean().default(false), // Default to disabled
  registryId: z.string().uuid().optional(),
  requireAuth: z.boolean().default(false),
  corsOrigins: z.array(z.string()).default(['*'])
  componentRegistrySettings: componentRegistrySettingsSchema.optional()
export type ComponentRegistrySettings = z.infer<typeof componentRegistrySettingsSchema>;
  componentRegistrySettings,
export const dbReadOnlyUsername = configInfo.dbReadOnlyUsername || configInfo.databaseSettings?.dbReadOnlyUsername;
export const dbReadOnlyPassword = configInfo.dbReadOnlyPassword || configInfo.databaseSettings?.dbReadOnlyPassword;
  if (!configSearchResult) {
    throw new Error('Config file not found.');
  if (configSearchResult.isEmpty) {
    throw new Error(`Config file ${configSearchResult.filepath} is empty or does not exist.`);
  const configParsing = configInfoSchema.safeParse(configSearchResult.config);
  return <ConfigInfo>configParsing.data;
 * Configuration types for DBAutoDoc
export interface DBAutoDocConfig {
  analysis: AnalysisConfig;
  output: OutputConfig;
  schemas: SchemaFilterConfig;
  tables: TableFilterConfig;
export interface DatabaseConfig {
  provider?: 'sqlserver' | 'mysql' | 'postgresql' | 'oracle'; // Default: sqlserver
  server: string;
  // SQL Server specific
  encrypt?: boolean;
  trustServerCertificate?: boolean;
  // Connection pool and timeout settings
  connectionTimeout?: number;
  requestTimeout?: number;
  maxConnections?: number;
  minConnections?: number;
  idleTimeoutMillis?: number;
export interface AIConfig {
  provider: 'gemini' | 'openai' | 'anthropic' | 'groq' | 'mistral' | 'vertex' | 'azure' | 'cerebras' | 'openrouter' | 'xai' | 'bedrock';
  requestsPerMinute?: number;
  effortLevel?: number; // Optional effort level 1-100 (1=lowest, 100=highest). Not all models support this.
export interface AnalysisConfig {
  cardinalityThreshold: number;
  sampleSize: number;
  includeStatistics: boolean;
  includePatternAnalysis: boolean;
  convergence: ConvergenceConfig;
  backpropagation: BackpropagationConfig;
  sanityChecks: SanityCheckConfig;
  guardrails?: GuardrailsConfig;
  relationshipDiscovery?: RelationshipDiscoveryConfig;
  sampleQueryGeneration?: SampleQueryGenerationConfig;
export interface SampleQueryGenerationConfig {
  enabled: boolean;                     // Enable sample query generation (default: false)
  queriesPerTable: number;              // Number of queries to generate per table (default: 5)
  maxExecutionTime: number;             // Max time to execute validation queries in ms (default: 30000)
  includeMultiQueryPatterns: boolean;   // Generate related query patterns (default: true)
  validateAlignment: boolean;           // Validate alignment between related queries (default: true)
  tokenBudget: number;                  // Token budget for query generation phase (default: 100000, set to 0 for unlimited)
  maxRowsInSample: number;              // Max rows to return in sample results (default: 10)
  maxTables?: number;                   // Max tables to generate queries for (default: 10, set to 0 for all tables)
  enableQueryFix?: boolean;             // Enable automatic query fix attempts (default: true)
  maxFixAttempts?: number;              // Maximum number of fix attempts per query (default: 3)
  enableQueryRefinement?: boolean;      // Enable LLM-based result analysis and refinement (default: false)
  maxRefinementAttempts?: number;       // Maximum refinement iterations per query (default: 1)
export interface RelationshipDiscoveryConfig {
  enabled: boolean;          // Enable automatic discovery (default: true)
  // Triggers for when to run discovery
    runOnMissingPKs: boolean;        // Run if any table missing PK (default: true)
    runOnInsufficientFKs: boolean;   // Run if FK count below threshold (default: true)
    fkDeficitThreshold: number;      // Run if actual FKs < threshold % of expected (default: 0.4)
  // Token budget allocation
    maxTokens?: number;              // Max tokens for discovery phase (default: 25% of total)
    ratioOfTotal?: number;           // Alternative: ratio of total budget (0-1, default: 0.25)
  // Confidence thresholds
    primaryKeyMinimum: number;       // Min confidence to treat as PK (default: 0.7)
    foreignKeyMinimum: number;       // Min confidence to treat as FK (default: 0.6)
    llmValidationThreshold: number;  // Use LLM validation if confidence below this (default: 0.8)
  // Sampling configuration
    maxRowsPerTable: number;         // Max rows to sample per table (default: 1000)
    statisticalSignificance: number; // Min sample size for valid statistics (default: 100)
    valueOverlapSampleSize: number;  // Rows to check for FK value overlap (default: 500)
    primaryKeyNames: string[];       // Regex patterns for PK names (e.g., [".*[Ii][Dd]$", "^pk_.*"])
    foreignKeyNames: string[];       // Regex patterns for FK names (e.g., [".*[Ii][Dd]$", "^fk_.*"])
    compositeKeyIndicators: string[]; // Patterns suggesting composite keys
  // LLM assistance
    enabled: boolean;                // Use LLM to validate candidates (default: true)
    batchSize: number;               // Validate N candidates per LLM call (default: 5)
  // Backpropagation in discovery
    enabled: boolean;                // Re-analyze after discoveries (default: true)
    maxIterations: number;           // Max discovery iterations (default: 10)
export interface GuardrailsConfig {
  // Hard limits - stop execution when exceeded
  maxTokensPerRun?: number;        // Stop after N tokens total (default: unlimited)
  maxDurationSeconds?: number;      // Stop after N seconds (default: unlimited)
  maxCostDollars?: number;          // Stop after $N spent (default: unlimited)
  maxTokensPerPrompt?: number;      // Truncate individual prompts (default: model max)
  // Per-phase token limits
  maxTokensPerPhase?: {
    discovery?: number;             // Max tokens for discovery phase
    analysis?: number;              // Max tokens for main analysis phase
    sanityChecks?: number;          // Max tokens for sanity checks
  // Per-iteration limits
  maxTokensPerIteration?: number;   // Max tokens per iteration (soft limit, warns at threshold)
  maxIterationDurationSeconds?: number; // Max duration per iteration
  // Warning thresholds (at X% of limits)
  warnThresholds?: {
    tokenPercentage?: number;       // Warn at % of maxTokensPerRun (default: 80%)
    durationPercentage?: number;    // Warn at % of maxDurationSeconds (default: 80%)
    costPercentage?: number;        // Warn at % of maxCostDollars (default: 80%)
    iterationTokenPercentage?: number; // Warn at % of maxTokensPerIteration (default: 80%)
    phaseTokenPercentage?: number;  // Warn at % of maxTokensPerPhase (default: 80%)
  // Enable/disable guardrail enforcement
  enabled?: boolean;                // Enable all guardrails (default: true)
  stopOnExceeded?: boolean;         // Stop immediately when limit exceeded (default: true)
export interface ConvergenceConfig {
  stabilityWindow: number;
  confidenceThreshold: number;
export interface BackpropagationConfig {
export interface SanityCheckConfig {
  dependencyLevel: boolean;
  schemaLevel: boolean;
  crossSchema: boolean;
export interface OutputConfig {
  outputDir?: string; // Base output directory for numbered runs
  sqlFile: string;
  markdownFile: string;
export interface SchemaFilterConfig {
export interface TableFilterConfig {
import { RunReport, BaseEntity, Metadata, RunView, RunQuery, SetProvider, StartupManager } from "@memberjunction/core";
import { GraphQLDataProvider, GraphQLProviderConfigData } from "./graphQLDataProvider";
import { MJGlobal, MJEventType } from "@memberjunction/global";
 * Setup the GraphQL client for the project using the provided configuration data.
export async function setupGraphQLClient(config: GraphQLProviderConfigData): Promise<GraphQLDataProvider> {
    // Set the provider for all entities to be GraphQL in this project, can use a different provider in other situations....
    const provider = new GraphQLDataProvider()
    // BaseEntity + Metadata share the same GraphQLDataProvider instance
    SetProvider(provider);
    await provider.Config(config);
    // fire off the logged in event if we get here
    MJGlobal.Instance.RaiseEvent({ event: MJEventType.LoggedIn, eventCode: null, component: this, args: null });
import type { SkywayConfig } from '@memberjunction/skyway-core';
import { mkdtempSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { simpleGit, SimpleGit } from 'simple-git';
export type MJConfig = z.infer<typeof mjConfigSchema>;
const MJ_REPO_URL = 'https://github.com/MemberJunction/MJ.git';
 * Default database configuration for MJCLI.
 * Database settings come from environment variables with sensible defaults.
const DEFAULT_CLI_CONFIG = {
  dbPort: process.env.DB_PORT ? parseInt(process.env.DB_PORT) : 1433,
  dbTrustServerCertificate: parseBooleanEnv(process.env.DB_TRUST_SERVER_CERTIFICATE),
  cleanDisabled: true,
  baselineOnMigrate: true,
  transactionMode: 'per-migration' as const,
  mjRepoUrl: MJ_REPO_URL,
  migrationsLocation: 'filesystem:./migrations',
const searchResult = explorer.search(process.cwd());
// Merge user config with DEFAULT_CLI_CONFIG to support minimal config
// This allows database fields to come from environment variables
const mergedConfig: any = searchResult?.config
  ? mergeConfigs(DEFAULT_CLI_CONFIG, searchResult.config)
  : DEFAULT_CLI_CONFIG;
// Create a result object with merged config for backward compatibility
const result = searchResult
  ? { ...searchResult, config: mergedConfig }
  : { config: mergedConfig, filepath: '', isEmpty: false };
// Schema placeholder configuration for cross-schema references
const schemaPlaceholderSchema = z.object({
  placeholder: z.string(),
// Schema for database-dependent config (required fields)
const mjConfigSchema = z.object({
  migrationsLocation: z.string().optional().default('filesystem:./migrations'),
  dbTrustServerCertificate: z.coerce.boolean().default(false),
  coreSchema: z.string().optional().default('__mj'),
  cleanDisabled: z.boolean().optional().default(true),
  mjRepoUrl: z.string().url().catch(MJ_REPO_URL),
  baselineVersion: z.string().optional(),
  baselineOnMigrate: z.boolean().optional().default(true),
  transactionMode: z.enum(['per-run', 'per-migration']).optional().default('per-migration'),
  SQLOutput: z.object({
    schemaPlaceholders: z.array(schemaPlaceholderSchema).optional(),
  }).passthrough().optional(),
// Schema for non-database commands (all fields optional)
const mjConfigSchemaOptional = z.object({
  dbHost: z.string().optional(),
  dbDatabase: z.string().optional(),
  dbPort: z.number({ coerce: true }).optional(),
  codeGenLogin: z.string().optional(),
  codeGenPassword: z.string().optional(),
// Don't validate at module load - let commands decide when they need validated config
export const config = result?.config as MJConfig | undefined;
 * Get validated config for commands that require database connection.
 * Throws error if config is invalid.
export const getValidatedConfig = (): MJConfig => {
  const parsedConfig = mjConfigSchema.safeParse(result?.config);
  if (!parsedConfig.success) {
      `Invalid or missing mj.config.cjs file. Database commands require valid configuration.\n` +
      `Missing fields: ${parsedConfig.error.issues.map(i => i.path.join('.')).join(', ')}`
  return parsedConfig.data;
 * Get optional config for commands that don't require database connection.
 * Returns undefined if no config exists, or partial config if it exists.
export const getOptionalConfig = (): Partial<MJConfig> | undefined => {
  const parsedConfig = mjConfigSchemaOptional.safeParse(result?.config);
  return parsedConfig.success ? parsedConfig.data : undefined;
 * Legacy function for backward compatibility with codegen.
 * Validates and returns updated config.
 * Returns undefined silently if config is invalid (command will handle the error).
export const updatedConfig = (): MJConfig | undefined => {
  const freshSearchResult = explorer.search(process.cwd());
  // Merge fresh config with DEFAULT_CLI_CONFIG
  const freshMergedConfig: any = freshSearchResult?.config
    ? mergeConfigs(DEFAULT_CLI_CONFIG, freshSearchResult.config)
  const maybeConfig = mjConfigSchema.safeParse(freshMergedConfig);
  // Don't log errors here - let the calling command handle validation
  return maybeConfig.success ? maybeConfig.data : undefined;
 * Builds a SkywayConfig from the MJ CLI config and optional overrides.
 * - Database connection mapping (MJConfig fields → Skyway DatabaseConfig)
 * - Migration location resolution (local dir, remote git tag)
 * - Placeholder mapping (schemaPlaceholders, legacy mjSchema, flyway:defaultSchema)
 * - Baseline configuration
export const getSkywayConfig = async (
  mjConfig: MJConfig,
  tag?: string,
  dir?: string
): Promise<SkywayConfig> => {
  const targetSchema = schema || mjConfig.coreSchema;
  let location = mjConfig.migrationsLocation;
  if (dir && !tag) {
    location = dir.startsWith('filesystem:') ? dir : `filesystem:${dir}`;
  if (tag) {
    // when tag is set, we want to fetch migrations from the github repo using the tag specified
    // we save those to a tmp dir and set that tmp dir as the migration location
    const tmp = mkdtempSync(tmpdir());
    const branch = /v?\d+\.\d+\.\d+/.test(tag) ? (tag.startsWith('v') ? tag : `v${tag}`) : tag;
    const git: SimpleGit = simpleGit(tmp);
    await git.clone(mjConfig.mjRepoUrl, tmp, ['--sparse', '--depth=1', '--branch', branch]);
    await git.raw(['sparse-checkout', 'set', 'migrations']);
    location = `filesystem:${tmp}`;
    if (dir) {
      const subPath = dir.replace(/^filesystem:/, '').replace(/^\.\//, '');
      location = `filesystem:${tmp}/${subPath}`;
  // Strip filesystem: prefix — Skyway uses plain filesystem paths
  const cleanLocation = location.replace(/^filesystem:/, '');
  // Build placeholders
  const placeholders: Record<string, string> = {};
  // Always provide the flyway:defaultSchema built-in so existing migration SQL works unchanged
  placeholders['flyway:defaultSchema'] = targetSchema;
  const schemaPlaceholders = mjConfig.SQLOutput?.schemaPlaceholders;
  if (schemaPlaceholders && schemaPlaceholders.length > 0) {
    // Use schemaPlaceholders from config (supports BCSaaS and other extensions)
    for (const { schema: schemaName, placeholder } of schemaPlaceholders) {
      const cleanPlaceholder = placeholder.replace(/^\$\{|\}$/g, '');
      // Skip Flyway built-in placeholders (already handled above)
      if (cleanPlaceholder.startsWith('flyway:')) continue;
      placeholders[cleanPlaceholder] = schemaName;
  } else if (schema && schema !== mjConfig.coreSchema) {
    // Legacy behavior: Add mjSchema placeholder for non-core schemas
    placeholders['mjSchema'] = mjConfig.coreSchema;
    Database: {
      Server: mjConfig.dbHost,
      Port: mjConfig.dbPort,
      Database: mjConfig.dbDatabase,
      User: mjConfig.codeGenLogin,
      Password: mjConfig.codeGenPassword,
      Options: {
        TrustServerCertificate: mjConfig.dbTrustServerCertificate,
    Migrations: {
      Locations: [cleanLocation],
      DefaultSchema: targetSchema,
      // Only pass BaselineVersion if explicitly set in config;
      // when omitted, Skyway auto-detects the latest B__baseline file.
      ...(mjConfig.baselineVersion ? { BaselineVersion: mjConfig.baselineVersion } : {}),
      BaselineOnMigrate: mjConfig.baselineOnMigrate,
    TransactionMode: mjConfig.transactionMode,
    Placeholders: Object.keys(placeholders).length > 0 ? placeholders : undefined,
export const ___serverPort = env.get('PORT').default('3999').asPortNumber();
const userHandlingInfoSchema = z.object({
  autoCreateNewUsers: z.boolean().optional().default(false),
  newUserLimitedToAuthorizedDomains: z.boolean().optional().default(false),
  newUserAuthorizedDomains: z.array(z.string()).optional().default([]),
  newUserRoles: z.array(z.string()).optional().default([]),
  updateCacheWhenNotFound: z.boolean().optional().default(false),
  updateCacheWhenNotFoundDelay: z.number().optional().default(30000),
  contextUserForNewUserCreation: z.string().optional().default(''),
  metadataCacheRefreshInterval: z.number(),
    max: z.number().optional().default(50),
    min: z.number().optional().default(5),
    idleTimeoutMillis: z.number().optional().default(30000),
    acquireTimeoutMillis: z.number().optional().default(30000),
  }).optional().default({}),
const viewingSystemInfoSchema = z.object({
  enableSmartFilters: z.boolean().optional(),
const restApiOptionsSchema = z.object({
  includeEntities: z.array(z.string()).optional(),
  excludeEntities: z.array(z.string()).optional(),
  includeSchemas: z.array(z.string()).optional(),
  excludeSchemas: z.array(z.string()).optional(),
 * Returns a new Zod object that accepts boolean, string, or number values and transforms them to boolean.
const zodBooleanWithTransforms = () => {
  return z
      .union([z.boolean(), z.string(), z.number()])
          .transform((v) => {
              return v === '1' || v.toLowerCase() === 'true';
            else if (typeof v === 'number') {
              return v === 1;
            else if (typeof v === 'boolean') {
const askSkipInfoSchema = z.object({
  url: z.string().optional(), // Base URL for Skip API
  orgID: z.string().optional(),
  organizationInfo: z.string().optional(),
  entitiesToSend: z
    .object({
      includeEntitiesFromExcludedSchemas: z.array(z.string()).optional(),
    .optional(),
  chatURL: z.string().optional(),
  learningCycleRunUponStartup: zodBooleanWithTransforms(),
  learningCycleEnabled: zodBooleanWithTransforms(),
  learningCycleURL: z.string().optional(),
  learningCycleIntervalInMinutes: z.coerce.number().optional(),
const sqlLoggingOptionsSchema = z.object({
  formatAsMigration: z.boolean().optional().default(false),
  statementTypes: z.enum(['queries', 'mutations', 'both']).optional().default('both'),
  batchSeparator: z.string().optional().default('GO'),
  prettyPrint: z.boolean().optional().default(true),
  logRecordChangeMetadata: z.boolean().optional().default(false),
  retainEmptyLogFiles: z.boolean().optional().default(false),
const sqlLoggingSchema = z.object({
  defaultOptions: sqlLoggingOptionsSchema.optional().default({}),
  allowedLogDirectory: z.string().optional().default('./logs/sql'),
  maxActiveSessions: z.number().optional().default(5),
  autoCleanupEmptyFiles: z.boolean().optional().default(true),
  sessionTimeout: z.number().optional().default(3600000), // 1 hour
const authProviderSchema = z.object({
  issuer: z.string(),
  audience: z.string(),
  jwksUri: z.string(),
  clientId: z.string().optional(),
  clientSecret: z.string().optional(),
  tenantId: z.string().optional(),
  domain: z.string().optional(),
}).passthrough(); // Allow additional provider-specific fields
const componentRegistrySchema = z.object({
  id: z.string().optional(),
  name: z.string().optional(),
  cache: z.boolean().optional().default(true),
  timeout: z.number().optional(),
  retryPolicy: z.object({
    maxRetries: z.number().optional(),
    initialDelay: z.number().optional(),
    maxDelay: z.number().optional(),
    backoffMultiplier: z.number().optional(),
  headers: z.record(z.string()).optional(),
}).passthrough(); // Allow additional fields
const scheduledJobsSchema = z.object({
  systemUserEmail: z.string().optional().default('system@memberjunction.org'),
  maxConcurrentJobs: z.number().optional().default(5),
  defaultLockTimeout: z.number().optional().default(600000), // 10 minutes in ms
  staleLockCleanupInterval: z.number().optional().default(300000), // 5 minutes in ms
const telemetrySchema = z.object({
  enabled: zodBooleanWithTransforms().default(
    process.env.MJ_TELEMETRY_ENABLED !== 'false' // Enabled by default unless explicitly disabled
  level: z.enum(['minimal', 'standard', 'verbose', 'debug']).optional().default('standard'),
  userHandling: userHandlingInfoSchema,
  viewingSystem: viewingSystemInfoSchema.optional(),
  restApiOptions: restApiOptionsSchema.optional().default({}),
  askSkip: askSkipInfoSchema.optional(),
  sqlLogging: sqlLoggingSchema.optional(),
  authProviders: z.array(authProviderSchema).optional(),
  componentRegistries: z.array(componentRegistrySchema).optional(),
  scheduledJobs: scheduledJobsSchema.optional().default({}),
  telemetry: telemetrySchema.optional().default({}),
  baseUrl: z.string().default('http://localhost'),
  publicUrl: z.string().optional().default(process.env.MJAPI_PUBLIC_URL || ''), // Public URL for callbacks (e.g., ngrok URL when developing)
  graphqlPort: z.coerce.number().default(4000),
  ___codeGenAPIURL: z.string().optional(),
  ___codeGenAPIPort: z.coerce.number().optional().default(3999),
  ___codeGenAPISubmissionDelay: z.coerce.number().optional().default(5000),
  graphqlRootPath: z.string().optional().default('/'),
  enableIntrospection: z.coerce.boolean().optional().default(false),
  websiteRunFromPackage: z.coerce.number().optional(),
  userEmailMap: z
    .transform((val) => z.record(z.string()).parse(JSON.parse(val)))
export type UserHandlingInfo = z.infer<typeof userHandlingInfoSchema>;
export type ViewingSystemSettingsInfo = z.infer<typeof viewingSystemInfoSchema>;
export type RESTApiOptions = z.infer<typeof restApiOptionsSchema>;
export type AskSkipInfo = z.infer<typeof askSkipInfoSchema>;
export type SqlLoggingOptions = z.infer<typeof sqlLoggingOptionsSchema>;
export type SqlLoggingInfo = z.infer<typeof sqlLoggingSchema>;
export type AuthProviderConfig = z.infer<typeof authProviderSchema>;
export type ComponentRegistryConfig = z.infer<typeof componentRegistrySchema>;
export type ScheduledJobsConfig = z.infer<typeof scheduledJobsSchema>;
export type TelemetryConfig = z.infer<typeof telemetrySchema>;
 * Default MJServer configuration values.
 * These provide sensible defaults for all optional settings, with environment variable overrides.
 * Priority order (highest to lowest):
 * 1. User's mj.config.cjs overrides
 * 2. Environment variables (referenced here in defaults)
 * 3. Hardcoded default values
 * This means minimal configs only need to override if they want something different
 * than both the environment variable AND the default value.
export const DEFAULT_SERVER_CONFIG: Partial<ConfigInfo> = {
  // Database connection settings (environment-driven with defaults)
  dbPort: process.env.DB_PORT ? parseInt(process.env.DB_PORT, 10) : 1433,
  dbDatabase: process.env.DB_DATABASE,
  dbUsername: process.env.DB_USERNAME,
  dbPassword: process.env.DB_PASSWORD,
  dbReadOnlyUsername: process.env.DB_READ_ONLY_USERNAME,
  dbReadOnlyPassword: process.env.DB_READ_ONLY_PASSWORD,
  mjCoreSchema: process.env.MJ_CORE_SCHEMA ?? '__mj',
  // GraphQL server settings (environment-driven with defaults)
  graphqlPort: process.env.GRAPHQL_PORT ? parseInt(process.env.GRAPHQL_PORT, 10) : 4000,
  graphqlRootPath: process.env.GRAPHQL_ROOT_PATH ?? '/',
  baseUrl: process.env.GRAPHQL_BASE_URL ?? 'http://localhost',
  publicUrl: process.env.MJAPI_PUBLIC_URL,
  enableIntrospection: process.env.ENABLE_INTROSPECTION === 'true',
  apiKey: process.env.MJ_API_KEY,
  websiteRunFromPackage: process.env.WEBSITE_RUN_FROM_PACKAGE ? parseInt(process.env.WEBSITE_RUN_FROM_PACKAGE, 10) : undefined,
  // User handling defaults
  userHandling: {
    autoCreateNewUsers: true,
    newUserLimitedToAuthorizedDomains: false,
    newUserAuthorizedDomains: [],
    newUserRoles: ['UI', 'Developer'],
    updateCacheWhenNotFound: true,
    updateCacheWhenNotFoundDelay: 5000,
    contextUserForNewUserCreation: 'not.set@nowhere.com',
    CreateUserApplicationRecords: true,
    UserApplications: []
  // Database settings (with environment variable for cache refresh)
  databaseSettings: {
    connectionTimeout: 45000,
    requestTimeout: 30000,
    metadataCacheRefreshInterval: isFinite(Number(process.env.METADATA_CACHE_REFRESH_INTERVAL))
      ? Number(process.env.METADATA_CACHE_REFRESH_INTERVAL)
      : 180000,
    connectionPool: {
      max: 50,
      min: 5,
      acquireTimeoutMillis: 30000,
  // Viewing system defaults
  viewingSystem: {
    enableSmartFilters: true,
  // REST API defaults
  restApiOptions: {
  // Ask Skip configuration (environment-driven)
  askSkip: {
    url: process.env.ASK_SKIP_URL,
    chatURL: process.env.ASK_SKIP_CHAT_URL,
    learningCycleURL: process.env.ASK_SKIP_LEARNING_URL,
    learningCycleIntervalInMinutes: process.env.ASK_SKIP_LEARNING_CYCLE_INTERVAL_IN_MINUTES
      ? parseInt(process.env.ASK_SKIP_LEARNING_CYCLE_INTERVAL_IN_MINUTES, 10)
    learningCycleEnabled: process.env.ASK_SKIP_RUN_LEARNING_CYCLES === 'true',
    learningCycleRunUponStartup: process.env.ASK_SKIP_RUN_LEARNING_CYCLES_UPON_STARTUP === 'true',
    orgID: process.env.ASK_SKIP_ORGANIZATION_ID,
    apiKey: process.env.ASK_SKIP_API_KEY,
    organizationInfo: process.env.ASK_SKIP_ORGANIZATION_INFO,
    entitiesToSend: {
      excludeSchemas: [],
      includeEntitiesFromExcludedSchemas: [],
  // SQL logging defaults
  sqlLogging: {
    defaultOptions: {
      statementTypes: 'both',
      batchSeparator: 'GO',
      logRecordChangeMetadata: false,
      retainEmptyLogFiles: false,
    allowedLogDirectory: './logs/sql',
    maxActiveSessions: 5,
    autoCleanupEmptyFiles: true,
    sessionTimeout: 3600000
  // Scheduled jobs defaults
  scheduledJobs: {
    systemUserEmail: 'not.set@nowhere.com',
    maxConcurrentJobs: 5,
    defaultLockTimeout: 600000,
    staleLockCleanupInterval: 300000
  // Telemetry defaults
  telemetry: {
    level: 'standard'
  // Auth providers (environment-driven)
  authProviders: [
    // Microsoft Azure AD / Entra ID
    process.env.TENANT_ID && process.env.WEB_CLIENT_ID ? {
      name: 'azure',
      type: 'msal',
      issuer: `https://login.microsoftonline.com/${process.env.TENANT_ID}/v2.0`,
      audience: process.env.WEB_CLIENT_ID,
      jwksUri: `https://login.microsoftonline.com/${process.env.TENANT_ID}/discovery/v2.0/keys`,
      clientId: process.env.WEB_CLIENT_ID,
      tenantId: process.env.TENANT_ID
    // Auth0
    process.env.AUTH0_DOMAIN && process.env.AUTH0_CLIENT_ID ? {
      name: 'auth0',
      type: 'auth0',
      issuer: `https://${process.env.AUTH0_DOMAIN}/`,
      audience: process.env.AUTH0_CLIENT_ID,
      jwksUri: `https://${process.env.AUTH0_DOMAIN}/.well-known/jwks.json`,
      clientId: process.env.AUTH0_CLIENT_ID,
      clientSecret: process.env.AUTH0_CLIENT_SECRET,
      domain: process.env.AUTH0_DOMAIN
  ].filter(Boolean),
  graphqlPort,
  ___codeGenAPIURL,
  ___codeGenAPIPort,
  ___codeGenAPISubmissionDelay,
  graphqlRootPath,
  enableIntrospection,
  websiteRunFromPackage,
  userEmailMap,
  baseUrl,
  restApiOptions: RESTApiOptions,
export function loadConfig() {
    LogStatus(`Config file found at ${configSearchResult.filepath}`);
    LogStatus(`No config file found, using DEFAULT_SERVER_CONFIG`);
 * Configuration schema for file storage providers
const storageProvidersSchema = z.object({
   * AWS S3 Configuration
   * Used by: AWSFileStorage driver
  aws: z
      accessKeyID: z.string().optional(),
      secretAccessKey: z.string().optional(),
      region: z.string().optional(),
      defaultBucket: z.string().optional(),
      keyPrefix: z.string().optional(),
   * Azure Blob Storage Configuration
   * Used by: AzureFileStorage driver
  azure: z
      accountName: z.string().optional(),
      accountKey: z.string().optional(),
      connectionString: z.string().optional(),
      defaultContainer: z.string().optional(),
   * Google Cloud Storage Configuration
   * Used by: GoogleFileStorage driver
  googleCloud: z
      projectID: z.string().optional(),
      keyFilename: z.string().optional(),
      keyJSON: z.string().optional(), // JSON string of service account credentials
   * Google Drive Configuration
   * Used by: GoogleDriveFileStorage driver
   * Supports BOTH service account auth (keyFile, credentialsJSON) AND OAuth2 (clientID, etc.)
  googleDrive: z
      // Service Account Auth (legacy)
      keyFile: z.string().optional(),
      credentialsJSON: z.string().optional(), // JSON string of service account credentials
      // OAuth2 Auth (new)
      clientID: z.string().optional(),
      refreshToken: z.string().optional(),
      redirectURI: z.string().optional(),
      rootFolderID: z.string().optional(),
   * Dropbox Configuration
   * Used by: DropboxFileStorage driver
  dropbox: z
      accessToken: z.string().optional(),
      clientID: z.string().optional(), // Also called appKey
      clientSecret: z.string().optional(), // Also called appSecret
      rootPath: z.string().optional(),
   * Box.com Configuration
   * Used by: BoxFileStorage driver
   * Supports access token, refresh token, AND JWT/client credentials auth
  box: z
      enterpriseID: z.string().optional(), // For JWT/client credentials flow
   * SharePoint Configuration
   * Used by: SharePointFileStorage driver
  sharePoint: z
      tenantID: z.string().optional(),
      siteID: z.string().optional(),
      driveID: z.string().optional(),
 * Complete configuration schema for MJStorage package
const storageConfigSchema = z.object({
   * Storage provider configurations
  storageProviders: storageProvidersSchema.optional().default({}),
export type StorageConfig = z.infer<typeof storageConfigSchema>;
export type StorageProvidersConfig = z.infer<typeof storageProvidersSchema>;
let _config: StorageConfig | null = null;
 * Gets the MJStorage configuration, loading it from mj.config.cjs if not already loaded
 * @returns The MJStorage configuration object
export function getStorageConfig(): StorageConfig {
      LogStatus('No mj.config.cjs found, using default MJStorage configuration');
      _config = storageConfigSchema.parse({});
    // Extract storage-related fields from the config
    // This checks both the config object and environment variables
    // Environment variables follow the pattern: STORAGE_[PROVIDER]_[FIELD]
      storageProviders: {
        aws: {
          accessKeyID: result.config.awsAccessKeyID || process.env.STORAGE_AWS_ACCESS_KEY_ID,
          secretAccessKey: result.config.awsSecretAccessKey || process.env.STORAGE_AWS_SECRET_ACCESS_KEY,
          region: result.config.awsRegion || process.env.STORAGE_AWS_REGION,
          defaultBucket: result.config.awsDefaultBucket || process.env.STORAGE_AWS_DEFAULT_BUCKET,
          keyPrefix: result.config.awsKeyPrefix || process.env.STORAGE_AWS_KEY_PREFIX,
        azure: {
          accountName: result.config.azureAccountName || process.env.STORAGE_AZURE_ACCOUNT_NAME,
          accountKey: result.config.azureAccountKey || process.env.STORAGE_AZURE_ACCOUNT_KEY,
          connectionString: result.config.azureConnectionString || process.env.STORAGE_AZURE_CONNECTION_STRING,
          defaultContainer: result.config.azureDefaultContainer || process.env.STORAGE_AZURE_DEFAULT_CONTAINER,
        googleCloud: {
          projectID: result.config.googleCloudProjectID || process.env.STORAGE_GOOGLE_CLOUD_PROJECT_ID,
          keyFilename: result.config.googleCloudKeyFilename || process.env.STORAGE_GOOGLE_CLOUD_KEY_FILENAME,
          keyJSON: result.config.googleCloudKeyJSON || process.env.STORAGE_GOOGLE_KEY_JSON,
          defaultBucket: result.config.googleCloudDefaultBucket || process.env.STORAGE_GOOGLE_CLOUD_DEFAULT_BUCKET || process.env.STORAGE_GOOGLE_BUCKET_NAME,
        googleDrive: {
          // Service account auth
          keyFile: result.config.googleDriveKeyFile || process.env.STORAGE_GDRIVE_KEY_FILE,
          credentialsJSON: result.config.googleDriveCredentialsJSON || process.env.STORAGE_GDRIVE_CREDENTIALS_JSON,
          // OAuth2 auth
          clientID: result.config.googleDriveClientID || process.env.STORAGE_GOOGLE_DRIVE_CLIENT_ID,
          clientSecret: result.config.googleDriveClientSecret || process.env.STORAGE_GOOGLE_DRIVE_CLIENT_SECRET,
          refreshToken: result.config.googleDriveRefreshToken || process.env.STORAGE_GOOGLE_DRIVE_REFRESH_TOKEN,
          redirectURI: result.config.googleDriveRedirectURI || process.env.STORAGE_GOOGLE_DRIVE_REDIRECT_URI,
          rootFolderID: result.config.googleDriveRootFolderID || process.env.STORAGE_GDRIVE_ROOT_FOLDER_ID,
        dropbox: {
          accessToken: result.config.dropboxAccessToken || process.env.STORAGE_DROPBOX_ACCESS_TOKEN,
          refreshToken: result.config.dropboxRefreshToken || process.env.STORAGE_DROPBOX_REFRESH_TOKEN,
          clientID: result.config.dropboxClientID || process.env.STORAGE_DROPBOX_CLIENT_ID || process.env.STORAGE_DROPBOX_APP_KEY,
          clientSecret: result.config.dropboxClientSecret || process.env.STORAGE_DROPBOX_CLIENT_SECRET || process.env.STORAGE_DROPBOX_APP_SECRET,
          rootPath: result.config.dropboxRootPath || process.env.STORAGE_DROPBOX_ROOT_PATH,
        box: {
          clientID: result.config.boxClientID || process.env.STORAGE_BOX_CLIENT_ID,
          clientSecret: result.config.boxClientSecret || process.env.STORAGE_BOX_CLIENT_SECRET,
          accessToken: result.config.boxAccessToken || process.env.STORAGE_BOX_ACCESS_TOKEN,
          refreshToken: result.config.boxRefreshToken || process.env.STORAGE_BOX_REFRESH_TOKEN,
          enterpriseID: result.config.boxEnterpriseID || process.env.STORAGE_BOX_ENTERPRISE_ID,
          rootFolderID: result.config.boxRootFolderID || process.env.STORAGE_BOX_ROOT_FOLDER_ID,
        sharePoint: {
          clientID: result.config.sharePointClientID || process.env.STORAGE_SHAREPOINT_CLIENT_ID,
          clientSecret: result.config.sharePointClientSecret || process.env.STORAGE_SHAREPOINT_CLIENT_SECRET,
          tenantID: result.config.sharePointTenantID || process.env.STORAGE_SHAREPOINT_TENANT_ID,
          siteID: result.config.sharePointSiteID || process.env.STORAGE_SHAREPOINT_SITE_ID,
          driveID: result.config.sharePointDriveID || process.env.STORAGE_SHAREPOINT_DRIVE_ID,
          rootFolderID: result.config.sharePointRootFolderID || process.env.STORAGE_SHAREPOINT_ROOT_FOLDER_ID,
    _config = storageConfigSchema.parse(rawConfig);
    LogError('Error loading MJStorage configuration', undefined, error);
 * Gets the storage providers configuration
 * @returns The storage providers configuration object
export function getStorageProvidersConfig(): StorageProvidersConfig {
  const config = getStorageConfig();
  return config.storageProviders;
 * Gets configuration for a specific storage provider
 * @param provider - The provider name ('aws', 'azure', 'googleCloud', etc.)
 * @returns The provider configuration or undefined if not configured
export function getProviderConfig<T extends keyof StorageProvidersConfig>(provider: T): StorageProvidersConfig[T] {
  const config = getStorageProvidersConfig();
  return config[provider];
export function clearStorageConfig(): void {
 * @fileoverview Configuration types and loaders for MetadataSync
 * @module config
 * This module defines configuration interfaces and provides utilities for loading
 * various configuration files used by the MetadataSync tool. It supports:
 * - MemberJunction database configuration (mj.config.cjs)
 * - Sync configuration (.mj-sync.json)
 * - Entity-specific configuration (.mj-sync.json with entity field)
 * - Folder-level defaults (.mj-folder.json)
import { configManager } from './lib/config-manager';
 * MemberJunction database configuration
 * Defines connection parameters and settings for connecting to the MemberJunction
 * database. Typically loaded from mj.config.cjs in the project root.
export interface MJConfig {
  /** Database server hostname or IP address */
  dbHost: string;
  /** Database server port (defaults to 1433 for SQL Server) */
  /** Database name to connect to */
  dbDatabase: string;
  /** Database authentication username */
  dbUsername: string;
  /** Database authentication password */
  dbPassword: string;
  /** Whether to trust the server certificate (Y/N) */
  dbTrustServerCertificate?: string;
  /** Whether to encrypt the connection (Y/N, auto-detected for Azure SQL) */
  dbEncrypt?: string;
  /** SQL Server instance name (for named instances) */
  dbInstanceName?: string;
  /** Schema name for MemberJunction core tables (defaults to __mj) */
  mjCoreSchema?: string;
  /** Allow additional properties for extensibility */
 * Global sync configuration
 * Defines settings that apply to the entire sync process, including push/pull
 * behaviors and watch mode configuration. Stored in .mj-sync.json at the root.
export interface SyncConfig {
  /** Version of the sync configuration format */
  /** Glob pattern for finding data files (defaults to "*.json") */
  filePattern?: string;
   * Directory processing order (only applies to root-level config, not inherited by subdirectories)
   * Specifies the order in which subdirectories should be processed to handle dependencies.
   * Directories not listed in this array will be processed after the ordered ones in alphabetical order.
  directoryOrder?: string[];
   * Directories to ignore during processing
   * Can be directory names or glob patterns relative to the location of the .mj-sync.json file
   * Cumulative: subdirectories inherit and add to parent ignoreDirectories
   * Examples: ["output", "examples", "temp"]
  ignoreDirectories?: string[];
  /** Push command configuration */
  push?: {
    /** Whether to validate records before pushing to database */
    validateBeforePush?: boolean;
    /** Whether to require user confirmation before push */
    requireConfirmation?: boolean;
     * Whether to automatically create new records when a primaryKey exists but record is not found
     * Defaults to false - will warn instead of creating
    autoCreateMissingRecords?: boolean;
     * When true, forces all records to be saved to database regardless of dirty state.
     * This bypasses dirty checking and always performs database updates.
     * Useful for ensuring complete synchronization or when dirty detection may miss changes.
    alwaysPush?: boolean;
  /** SQL logging configuration (only applies to root-level config, not inherited by subdirectories) */
  sqlLogging?: {
    /** Whether to enable SQL logging during push operations */
    /** Directory to output SQL log files (relative to command execution directory, defaults to './sql_logging') */
    outputDirectory?: string;
    /** Whether to format SQL as migration-ready files with Flyway schema placeholders */
     * Array of patterns to filter SQL statements.
     * Supports both regex strings and simple wildcard patterns:
     * - Regex: "/spCreate.*Run/i" (must start with "/" and optionally end with flags)
     * - Simple: "*spCreateAIPromptRun*" (uses * as wildcard, case-insensitive by default)
     * Examples: ["*AIPrompt*", "/^EXEC sp_/i", "*EntityFieldValue*"]
     * Determines how filterPatterns are applied:
     * - 'exclude': If ANY pattern matches, the SQL is NOT logged (default)
     * - 'include': If ANY pattern matches, the SQL IS logged
    filterType?: 'exclude' | 'include';
    /** Whether to output verbose debug information to console (default: false) */
  /** Watch command configuration */
  watch?: {
    /** Milliseconds to wait before processing file changes */
    debounceMs?: number;
    /** File patterns to ignore during watch */
    ignorePatterns?: string[];
  /** User role validation configuration */
  userRoleValidation?: {
    /** Whether to enable user role validation for UserID fields */
    /** List of role names that are allowed to be referenced in metadata */
    allowedRoles?: string[];
    /** Whether to allow users without any roles (defaults to false) */
    allowUsersWithoutRoles?: boolean;
   * Whether to emit __mj_sync_notes in record files during push operations.
   * When enabled, resolution information for @lookup and @parent references is written to files.
   * Defaults to false. Entity-level .mj-sync.json files can override this setting.
  emitSyncNotes?: boolean;
 * Configuration for related entity synchronization
 * Defines how to pull and push related entities that have foreign key relationships
 * with a parent entity. Supports nested relationships for deep object graphs.
 * NEW: Supports automatic recursive patterns for self-referencing entities.
export interface RelatedEntityConfig {
  /** Name of the related entity to sync */
  /** Field name that contains the foreign key reference to parent (e.g., "PromptID") */
  foreignKey: string;
  /** Optional SQL filter to apply when pulling related records */
   * Enable recursive fetching for self-referencing entities
   * When true, automatically fetches all levels of the hierarchy until no more children found
  recursive?: boolean;
   * Maximum depth for recursive fetching (optional, defaults to 10)
   * Prevents infinite loops and controls memory usage
   * Only applies when recursive is true
  /** Fields to externalize to separate files for this related entity */
  externalizeFields?: string[] | {
    [fieldName: string]: {
      /** File extension to use (e.g., ".md", ".txt", ".html") */
      extension?: string;
  } | Array<{
    /** Field name to externalize */
    /** Pattern for the output file. Supports placeholders from the entity */
  /** Fields to exclude from the pulled data for this related entity */
  excludeFields?: string[];
  /** Foreign key fields to convert to @lookup references for this related entity */
  lookupFields?: {
  /** Nested related entities for deep object graphs */
  relatedEntities?: Record<string, RelatedEntityConfig>;
 * Entity-specific configuration
 * Defines settings for a specific entity type within a directory. Stored in
 * .mj-sync.json files that contain an "entity" field. Supports defaults,
 * file patterns, and related entity configuration.
  /** Name of the entity this directory contains */
  /** Default field values applied to all records in this directory */
  defaults?: Record<string, any>;
  /** Pull command specific configuration */
  pull?: {
    /** Glob pattern for finding existing files to update (defaults to filePattern) */
    /** Whether to create new files for records not found locally */
    createNewFileIfNotFound?: boolean;
    /** Filename for new records when createNewFileIfNotFound is true */
    newFileName?: string;
    /** Whether to append multiple new records to a single file */
    appendRecordsToExistingFile?: boolean;
    /** Whether to update existing records found in local files */
    updateExistingRecords?: boolean;
    /** Fields to preserve during updates (never overwrite these) */
    preserveFields?: string[];
    /** Strategy for merging updates: "overwrite" | "merge" | "skip" */
    mergeStrategy?: 'overwrite' | 'merge' | 'skip';
    /** Create backup files before updating existing files */
    backupBeforeUpdate?: boolean;
    /** Directory name for backup files (defaults to ".backups") */
    backupDirectory?: string;
    /** SQL filter to apply when pulling records from database */
    /** Configuration for pulling related entities */
    /** Fields to externalize to separate files with optional configuration */
      /** Pattern for the output file. Supports placeholders:
       * - {Name}: Entity's name field value
       * - {ID}: Entity's ID
       * - {FieldName}: The field being externalized
       * - Any other {FieldName} from the entity
       * Example: "@file:templates/{Name}.template.md"
    /** Fields to exclude from the pulled data (e.g., ["TemplateID"]) */
    /** Foreign key fields to convert to @lookup references */
      /** Field name in this entity (e.g., "CategoryID") */
        /** Target entity name (e.g., "MJ: AI Prompt Categories") */
        /** Field in target entity to use for lookup (e.g., "Name") */
    /** Whether to ignore null field values during pull (defaults to false) */
    ignoreNullFields?: boolean;
    /** Whether to ignore virtual fields during pull (defaults to false) */
    ignoreVirtualFields?: boolean;
   * If not specified, inherits from root .mj-sync.json. Defaults to false if not set anywhere.
 * Folder-level configuration
 * Defines default values that cascade down to all subdirectories. Stored in
 * .mj-folder.json files. Child folders can override parent defaults.
export interface FolderConfig {
  /** Default field values that apply to all entities in this folder and subfolders */
  defaults: Record<string, any>;
 * Load MemberJunction configuration from the filesystem
 * Searches for mj.config.cjs starting from the current directory and walking up
 * the directory tree. Uses cosmiconfig for flexible configuration loading.
 * @returns MJConfig object if found, null if not found or invalid
 * const config = loadMJConfig();
 *   console.log(`Connecting to ${config.dbHost}:${config.dbPort || 1433}`);
export function loadMJConfig(): MJConfig | null {
  return configManager.loadMJConfig();
 * Load sync configuration from a directory
 * Loads .mj-sync.json file from the specified directory. This file can contain
 * either global sync configuration (no entity field) or entity-specific
 * configuration (with entity field).
 * @param dir - Directory path to load configuration from
 * @returns Promise resolving to SyncConfig if found and valid, null otherwise
 * const syncConfig = await loadSyncConfig('/path/to/project');
 * if (syncConfig?.push?.requireConfirmation) {
 *   // Show confirmation prompt
export async function loadSyncConfig(dir: string): Promise<SyncConfig | null> {
  const configPath = path.join(dir, '.mj-sync.json');
  if (await fs.pathExists(configPath)) {
      return await fs.readJson(configPath);
      console.error('Error loading sync config:', error);
 * Load entity-specific configuration from a directory
 * Loads .mj-sync.json file that contains an "entity" field, indicating this
 * directory contains data for a specific entity type. Returns null if the
 * file doesn't exist or doesn't contain an entity field.
 * @returns Promise resolving to EntityConfig if found and valid, null otherwise
 * const entityConfig = await loadEntityConfig('./ai-prompts');
 * if (entityConfig) {
 *   console.log(`Directory contains ${entityConfig.entity} records`);
export async function loadEntityConfig(dir: string): Promise<EntityConfig | null> {
      const config = await fs.readJson(configPath);
      if (config.entity) {
      console.error('Error loading entity config:', error);
 * Load folder-level configuration
 * Loads .mj-folder.json file that contains default values to be applied to
 * all entities in this folder and its subfolders. Used for cascading defaults
 * in deep directory structures.
 * @returns Promise resolving to FolderConfig if found and valid, null otherwise
 * const folderConfig = await loadFolderConfig('./templates');
 * if (folderConfig?.defaults) {
 *   // Apply folder defaults to records
export async function loadFolderConfig(dir: string): Promise<FolderConfig | null> {
  const configPath = path.join(dir, '.mj-folder.json');
      console.error('Error loading folder config:', error);
 * Configuration loader for QueryGen
 * Loads configuration from mj.config.cjs and merges with CLI options
 * QueryGen configuration options
export interface QueryGenConfig {
  // Entity Filtering
  // NOTE: includeEntities and excludeEntities are mutually exclusive
  // - includeEntities: If provided, ONLY these entities will be processed (allowlist)
  // - excludeEntities: If provided, these entities will be excluded from processing (denylist)
  // - If both are provided, includeEntities takes precedence and excludeEntities is ignored
  includeEntities: string[];
  excludeEntities: string[];
  excludeSchemas: string[];
  // Entity Grouping
  questionsPerGroup: number;
  minGroupSize: number;        // Minimum entities per group
  maxGroupSize: number;        // Maximum entities per group
  requireConnectivity: boolean; // Require entities in groups to be connected via relationships
  // AI Configuration
  modelOverride?: string;    // Override model for all prompts (e.g., "GPT-OSS-120B")
  vendorOverride?: string;   // Override vendor for all prompts (e.g., "Groq")
  embeddingModel: string;
  // Iteration Limits
  maxRefinementIterations: number;
  maxFixingIterations: number;
  // Few-Shot Learning
  topSimilarQueries: number;
  // Similarity Weighting
  similarityWeights: {
    userQuestion: number;
    description: number;
    technicalDescription: number;
  // Output Configuration
  outputMode: 'metadata' | 'database' | 'both';
  outputCategoryDirectory?: string; // Optional: defaults to outputDirectory if not provided
  externalizeSQLToFiles: boolean; // When true, creates separate .sql files and uses @file: references
  // Query Category Configuration
  rootQueryCategory: string;
  autoCreateEntityQueryCategories: boolean;
  // Performance
  parallelGenerations: number;
  enableCaching: boolean;
  testWithSampleData: boolean;
  requireMinRows: number;
  maxRefinementRows: number;
  // Verbose Logging
const DEFAULT_CONFIG: QueryGenConfig = {
  includeEntities: [],
  excludeSchemas: ['sys', 'INFORMATION_SCHEMA', '__mj'],
  minGroupSize: 2,              // Multi-entity groups have at least 2 entities
  maxGroupSize: 3,              // Keep groups small for focused questions
  requireConnectivity: true,    // Require entities to be connected (disable for sparse relationship graphs)
  topSimilarQueries: 5,
    userQuestion: 0.2,
    description: 0.40,
    technicalDescription: 0.40,
  outputMode: 'metadata',
  outputDirectory: './metadata/queries',
  externalizeSQLToFiles: true,
  parallelGenerations: 1,
  enableCaching: true,
  testWithSampleData: true,
  requireMinRows: 0,
  maxRefinementRows: 10,
 * Load configuration from mj.config.cjs and merge with CLI options
 * Configuration priority (highest to lowest):
 * 1. CLI options (command line flags)
 * 2. mj.config.cjs queryGen section
 * 3. Default values
 * @param cliOptions - Options provided via command line
 * @returns Merged configuration ready for use
export function loadConfig(cliOptions: Record<string, unknown>): QueryGenConfig {
  const config: QueryGenConfig = { ...DEFAULT_CONFIG };
  // Load mj.config.cjs if it exists
  const mjConfig = loadMjConfig();
  if (mjConfig && mjConfig.queryGen) {
    Object.assign(config, mjConfig.queryGen);
  // Override with CLI options
  if (cliOptions.entities) {
    config.includeEntities = parseArrayOption(cliOptions.entities);
  if (cliOptions.excludeEntities) {
    config.excludeEntities = parseArrayOption(cliOptions.excludeEntities);
  if (cliOptions.excludeSchemas) {
    config.excludeSchemas = parseArrayOption(cliOptions.excludeSchemas);
  if (cliOptions.maxRefinements) {
    config.maxRefinementIterations = parseNumberOption(cliOptions.maxRefinements, 'maxRefinements');
  if (cliOptions.maxFixes) {
    config.maxFixingIterations = parseNumberOption(cliOptions.maxFixes, 'maxFixes');
  if (cliOptions.model) {
    config.modelOverride = String(cliOptions.model);
  if (cliOptions.vendor) {
    config.vendorOverride = String(cliOptions.vendor);
  if (cliOptions.output) {
    config.outputDirectory = String(cliOptions.output);
  if (cliOptions.mode) {
    const mode = String(cliOptions.mode);
    if (mode === 'metadata' || mode === 'database' || mode === 'both') {
      config.outputMode = mode;
      throw new Error(`Invalid output mode: ${mode}. Must be metadata, database, or both`);
  if (cliOptions.verbose) {
    config.verbose = true;
  // Validate entity filtering (includeEntities and excludeEntities are mutually exclusive)
  if (config.includeEntities.length > 0 && config.excludeEntities.length > 0) {
    console.warn('[Warning] Both includeEntities and excludeEntities provided. includeEntities takes precedence, excludeEntities will be ignored.');
    config.excludeEntities = [];
 * Load mj.config.cjs from current working directory
 * Returns null if file doesn't exist or can't be loaded
function loadMjConfig(): { queryGen?: Partial<QueryGenConfig> } | null {
    const configPath = path.join(process.cwd(), 'mj.config.cjs');
      return require(configPath);
    // Config file doesn't exist or couldn't be loaded - not an error
 * Parse array option from CLI (handles both comma-separated strings and arrays)
function parseArrayOption(value: unknown): string[] {
    return value.map(String);
    return value.split(',').map(s => s.trim());
 * Parse number option from CLI with validation
function parseNumberOption(value: unknown, name: string): number {
  if (isNaN(num) || num < 0) {
    throw new Error(`Invalid ${name}: must be a positive number`);
import { BaseEntity, Metadata, RunView, LogError, LogStatus, RunReport, RunQuery, SetProvider, StartupManager } from "@memberjunction/core";
import { SQLServerProviderConfigData } from "./types";
import { UserCache } from "./UserCache";
export async function setupSQLServerClient(config: SQLServerProviderConfigData): Promise<SQLServerDataProvider> {
        // Set the provider for all entities to be SQL Server in this project, can use a different provider in other situations....
        const pool: sql.ConnectionPool = config.ConnectionPool;
        if (pool.connected) {
            const provider = new SQLServerDataProvider()
            // BaseEntity + Metadata share the same provider instance
            // now setup the user cache
            await UserCache.Instance.Refresh(pool, config.CheckRefreshIntervalSeconds);   
            if (config.CheckRefreshIntervalSeconds && config.CheckRefreshIntervalSeconds > 0) {
                // Start a timer to check for refreshes
                setInterval(async () => {
                        await provider.RefreshIfNeeded();
                        LogError("Error in CheckForRefreshes", e)
                }, config.CheckRefreshIntervalSeconds * 1000)
            const sysUser = UserCache.Instance.GetSystemUser();
            const backupSysUser = UserCache.Instance.Users.find(u => u.IsActive && u.Type==='Owner');
            await StartupManager.Instance.Startup(false, sysUser || backupSysUser, provider);
            //pool is not connected, so wait for it
            LogStatus("Connection pool is not connected, we're going to wait for it...")
            if (pool.connected)
                return setupSQLServerClient(config) // one time recursive call since we're now connected
                throw new Error("Failed to connect to database"); // don't do recursive call here as it would go on forever
        const errorStack = e instanceof Error ? e.stack : '';
        LogError(`Error in setupSQLServerClient: ${errorMessage}\n${errorStack}`);
        throw e; // Re-throw so caller knows about the failure
export const BETTY_BOT_BASE_URL: string = env.get('BETTY_BOT_BASE_URL').default("https://betty-api.tasio.co/").asString();export const REX_API_HOST: string = env.get('REX_API_HOST').asString();
export const REX_BATCH_SIZE: number = env.get('REX_BATCH_SIZE').default(200).asInt();export const openAIAPIKey: string = process.env.OPENAI_API_KEY;
export const mistralAPIKey = process.env.MISTRAL_API_KEY;export const ApolloAPIEndpoint = 'https://api.apollo.io/v1';
export const ApolloAPIKey = process.env.APOLLO_API_KEY || ""; * Configuration schema for external API integrations used by Core Actions
export const AZURE_ACCOUNT_ID: string = env.get('AZURE_ACCOUNT_ID').default('').asString();// Google API OAuth credentials - now optional to support per-request credential override
export const __API_KEY = process.env.COMMUNICATION_VENDOR_API_KEY__SENDGRID;// Twilio credentials - now optional to support per-request credential override
import { DebugLogger, deepAssign, InvalidConfigurationError, log, safeStringifyJson, statOrNull } from "builder-util"
import { readJson } from "fs-extra"
import { Configuration } from "../../configuration"
import { FileSet } from "../../options/PlatformSpecificBuildOptions"
import { reactCra } from "../../presets/rectCra"
import { PACKAGE_VERSION } from "../../version"
import { getConfig as _getConfig, loadParentConfig, orNullIfFileNotExist, ReadConfigRequest } from "./load"
const validateSchema = require("@develar/schema-utils")
// https://github.com/electron-userland/electron-builder/issues/1847
function mergePublish(config: Configuration, configFromOptions: Configuration) {
  // if config from disk doesn't have publish (or object), no need to handle, it will be simply merged by deepAssign
  const publish = Array.isArray(config.publish) ? configFromOptions.publish : null
  if (publish != null) {
    delete (configFromOptions as any).publish
  deepAssign(config, configFromOptions)
  if (publish == null) {
  const listOnDisk = config.publish as Array<any>
  if (listOnDisk.length === 0) {
    config.publish = publish
    // apply to first
    Object.assign(listOnDisk[0], publish)
export async function getConfig(
  projectDir: string,
  configPath: string | null,
  configFromOptions: Configuration | Nullish,
  packageMetadata: Lazy<Record<string, any> | null> = new Lazy(() => orNullIfFileNotExist(readJson(path.join(projectDir, "package.json"))))
): Promise<Configuration> {
  const configRequest: ReadConfigRequest = { packageKey: "build", configFilename: "electron-builder", projectDir, packageMetadata }
  const configAndEffectiveFile = await _getConfig<Configuration>(configRequest, configPath)
  const config = configAndEffectiveFile == null ? {} : configAndEffectiveFile.result
  if (configFromOptions != null) {
    mergePublish(config, configFromOptions)
  if (configAndEffectiveFile != null) {
    log.info({ file: configAndEffectiveFile.configFile == null ? 'package.json ("build" field)' : configAndEffectiveFile.configFile }, "loaded configuration")
  if (config.extends == null && config.extends !== null) {
    const metadata = (await packageMetadata.value) || {}
    const devDependencies = metadata.devDependencies
    const dependencies = metadata.dependencies
    if ((dependencies != null && "react-scripts" in dependencies) || (devDependencies != null && "react-scripts" in devDependencies)) {
      config.extends = "react-cra"
    } else if (devDependencies != null && "electron-webpack" in devDependencies) {
      let file = "electron-webpack/out/electron-builder.js"
        file = require.resolve(file)
      } catch (_ignore) {
        file = require.resolve("electron-webpack/electron-builder.yml")
      config.extends = `file:${file}`
  const parentConfigs = await loadParentConfigsRecursively(config.extends, async configExtend => {
    if (configExtend === "react-cra") {
      const result = await reactCra(projectDir)
      log.info({ preset: configExtend }, "loaded parent configuration")
      const { configFile, result } = await loadParentConfig<Configuration>(configRequest, configExtend)
      log.info({ file: configFile }, "loaded parent configuration")
  return doMergeConfigs([...parentConfigs, config])
function asArray(value: string[] | string | Nullish): string[] {
  return Array.isArray(value) ? value : typeof value === "string" ? [value] : []
async function loadParentConfigsRecursively(configExtends: Configuration["extends"], loader: (configExtend: string) => Promise<Configuration>): Promise<Configuration[]> {
  const configs = []
  for (const configExtend of asArray(configExtends)) {
    const result = await loader(configExtend)
    const parentConfigs = await loadParentConfigsRecursively(result.extends, loader)
    configs.push(...parentConfigs, result)
  return configs
// normalize for easy merge
function normalizeFiles(configuration: Configuration, name: "files" | "extraFiles" | "extraResources") {
  let value = configuration[name]
  itemLoop: for (let i = 0; i < value.length; i++) {
    let item = value[i]
    if (typeof item === "string") {
      // merge with previous if possible
      if (i !== 0) {
        let prevItemIndex = i - 1
        let prevItem: FileSet
          prevItem = value[prevItemIndex--] as FileSet
        } while (prevItem == null)
        if (prevItem.from == null && prevItem.to == null) {
          if (prevItem.filter == null) {
            prevItem.filter = [item]
            ;(prevItem.filter as Array<string>).push(item)
          value[i] = null as any
          continue itemLoop
      item = {
        filter: [item],
      value[i] = item
    } else if (Array.isArray(item)) {
      throw new Error(`${name} configuration is invalid, nested array not expected for index ${i}: ${item}`)
    // make sure that merge logic is not complex - unify different presentations
    if (item.from === ".") {
      item.from = undefined
    if (item.to === ".") {
      item.to = undefined
    if (item.filter != null && typeof item.filter === "string") {
      item.filter = [item.filter]
  configuration[name] = value.filter(it => it != null)
function isSimilarFileSet(value: FileSet, other: FileSet): boolean {
  return value.from === other.from && value.to === other.to
type Filter = FileSet["filter"]
function mergeFilters(value: Filter, other: Filter): string[] {
  return asArray(value).concat(asArray(other))
function mergeFileSets(lists: FileSet[][]): FileSet[] {
  const result: FileSet[] = []
    for (const item of list) {
      const existingItem = result.find(i => isSimilarFileSet(i, item))
      if (existingItem) {
        existingItem.filter = mergeFilters(item.filter, existingItem.filter)
        result.push(item)
 * `doMergeConfigs` takes configs in the order you would pass them to
 * Object.assign as sources.
export function doMergeConfigs(configs: Configuration[]): Configuration {
    normalizeFiles(config, "files")
    normalizeFiles(config, "extraFiles")
    normalizeFiles(config, "extraResources")
  const result = deepAssign(getDefaultConfig(), ...configs)
  // `deepAssign` prioritises latter configs, while `mergeFilesSets` prioritises
  // former configs, so we have to reverse the order, because latter configs
  // must have higher priority.
  configs = configs.slice().reverse()
  result.files = mergeFileSets(configs.map(config => (config.files ?? []) as FileSet[]))
function getDefaultConfig(): Configuration {
      output: "dist",
      buildResources: "build",
const schemeDataPromise = new Lazy(() => readJson(path.join(__dirname, "..", "..", "..", "scheme.json")))
export async function validateConfiguration(config: Configuration, debugLogger: DebugLogger) {
  const extraMetadata = config.extraMetadata
  if (extraMetadata != null) {
    if (extraMetadata.build != null) {
      throw new InvalidConfigurationError(`--em.build is deprecated, please specify as -c"`)
    if (extraMetadata.directories != null) {
      throw new InvalidConfigurationError(`--em.directories is deprecated, please specify as -c.directories"`)
  const oldConfig: any = config
  if (oldConfig.npmSkipBuildFromSource === false) {
    throw new InvalidConfigurationError(`npmSkipBuildFromSource is deprecated, please use buildDependenciesFromSource"`)
  if (oldConfig.appImage != null && oldConfig.appImage.systemIntegration != null) {
    throw new InvalidConfigurationError(`appImage.systemIntegration is deprecated, https://github.com/TheAssassin/AppImageLauncher is used for desktop integration"`)
  // noinspection JSUnusedGlobalSymbols
  validateSchema(await schemeDataPromise.value, config, {
    name: `electron-builder ${PACKAGE_VERSION}`,
    postFormatter: (formattedError: string, error: any): string => {
      if (debugLogger.isEnabled) {
        debugLogger.add("invalidConfig", safeStringifyJson(error))
      const site = "https://www.electron.build"
      let url = `${site}/configuration`
      const targets = new Set(["mac", "dmg", "pkg", "mas", "win", "nsis", "appx", "linux", "appimage", "snap"])
      const dataPath: string = error.dataPath == null ? null : error.dataPath
      const targetPath = dataPath.startsWith(".") ? dataPath.substr(1).toLowerCase() : null
      if (targetPath != null && targets.has(targetPath)) {
        url = `${site}/${targetPath}`
      return `${formattedError}\n  How to fix:
  1. Open ${url}
  2. Search the option name on the page (or type in into Search to find across the docs).
    * Not found? The option was deprecated or not exists (check spelling).
    * Found? Check that the option in the appropriate place. e.g. "title" only in the "dmg", not in the root.
const DEFAULT_APP_DIR_NAMES = ["app", "www"]
export async function computeDefaultAppDirectory(projectDir: string, userAppDir: string | Nullish): Promise<string> {
  if (userAppDir != null) {
    const absolutePath = path.resolve(projectDir, userAppDir)
    const stat = await statOrNull(absolutePath)
    if (stat == null) {
      throw new InvalidConfigurationError(`Application directory ${userAppDir} doesn't exist`)
    } else if (!stat.isDirectory()) {
      throw new InvalidConfigurationError(`Application directory ${userAppDir} is not a directory`)
    } else if (projectDir === absolutePath) {
      log.warn({ appDirectory: userAppDir }, `Specified application directory equals to project dir — superfluous or wrong configuration`)
    return absolutePath
  for (const dir of DEFAULT_APP_DIR_NAMES) {
    const absolutePath = path.join(projectDir, dir)
    const packageJson = path.join(absolutePath, "package.json")
    const stat = await statOrNull(packageJson)
    if (stat != null && stat.isFile()) {
  return projectDir
export const BETTY_BOT_BASE_URL: string = env.get('BETTY_BOT_BASE_URL').default("https://betty-api.tasio.co/").asString();export const REX_API_HOST: string = env.get('REX_API_HOST').asString();
export const REX_BATCH_SIZE: number = env.get('REX_BATCH_SIZE').default(200).asInt();export const openAIAPIKey: string = process.env.OPENAI_API_KEY;
export const mistralAPIKey = process.env.MISTRAL_API_KEY; * Configuration schema for external API integrations used by Core Actions
export const ApolloAPIEndpoint = 'https://api.apollo.io/v1';
export const ApolloAPIKey = process.env.APOLLO_API_KEY || "";export const AZURE_ACCOUNT_ID: string = env.get('AZURE_ACCOUNT_ID').default('').asString();// Google API OAuth credentials - now optional to support per-request credential override
export const __API_KEY = process.env.COMMUNICATION_VENDOR_API_KEY__SENDGRID;// Twilio credentials - now optional to support per-request credential override
