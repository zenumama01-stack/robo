 * @fileoverview AuthGate - Unified authentication middleware for MCP Server.
 * AuthGate handles both API key and OAuth Bearer token authentication,
 * supporting all four auth modes: apiKey, oauth, both, none.
 * The middleware:
 * - Extracts credentials from request headers
 * - Routes to appropriate validation (API key or OAuth)
 * - Creates MCPSessionContext with authenticated user
 * - Returns proper HTTP error responses with WWW-Authenticate headers
 * @module @memberjunction/ai-mcp-server/auth/AuthGate
import type { Request, Response } from 'express';
import type * as http from 'http';
import { RunView, type UserInfo } from '@memberjunction/core';
import type { MCPSessionContext, AuthMode, AuthResult } from './types.js';
import { getAuthMode, isOAuthEnabled } from './OAuthConfig.js';
import { validateBearerToken, resolveOAuthUser } from './TokenValidator.js';
import { send401Response, send403Response, send503Response } from './WWWAuthenticate.js';
 * Credential extraction result from HTTP request.
interface ExtractedCredentials {
  /** API key from x-api-key or x-mj-api-key header */
  /** Bearer token from Authorization header */
  bearerToken?: string;
  /** Source of API key if found */
  apiKeySource?: 'x-api-key' | 'x-mj-api-key' | 'authorization' | 'query';
 * Configuration for the AuthGate middleware.
export interface AuthGateConfig {
  /** Function to validate API keys (provided by Server.ts) */
  validateApiKey: (
  ) => Promise<{
  /** Function to get system user for 'none' mode */
  getSystemUser: () => UserInfo | undefined;
 * Extracts credentials from an HTTP request.
 * Checks the following sources in order:
 * 3. Authorization: Bearer header (could be API key or OAuth token)
 * 4. Query parameters (apiKey, api_key)
 * @returns Extracted credentials
export function extractCredentials(request: Request | http.IncomingMessage): ExtractedCredentials {
  const result: ExtractedCredentials = {};
  // Check dedicated API key headers
  const xApiKey = request.headers['x-api-key'] as string | undefined;
  const xMjApiKey = request.headers['x-mj-api-key'] as string | undefined;
  if (xApiKey) {
    result.apiKey = xApiKey;
    result.apiKeySource = 'x-api-key';
  } else if (xMjApiKey) {
    result.apiKey = xMjApiKey;
    result.apiKeySource = 'x-mj-api-key';
  // Check Authorization header
  const authHeader = request.headers['authorization'] as string | undefined;
  if (authHeader?.startsWith('Bearer ')) {
    const token = authHeader.substring(7);
    // Determine if this is an API key or OAuth token
    // API keys start with "mj_sk_" prefix
    if (token.startsWith('mj_sk_') || token.startsWith('mj_pk_')) {
      // This is an MJ API key
      if (!result.apiKey) {
        result.apiKey = token;
        result.apiKeySource = 'authorization';
      // This is likely an OAuth Bearer token (JWT)
      result.bearerToken = token;
  // Check query parameters as fallback for API key
  if (!result.apiKey && request.url) {
        result.apiKey = queryKey;
        result.apiKeySource = 'query';
      // URL parsing failed, skip query params
 * Authenticates a request using the configured auth mode.
 * @param config - AuthGate configuration with validation functions
 * @returns Authentication result
export async function authenticateRequest(
  request: Request | http.IncomingMessage,
  config: AuthGateConfig
  const mode = getAuthMode();
  const credentials = extractCredentials(request);
  // Mode: none - skip authentication, use system user
  if (mode === 'none') {
    return handleNoneMode(config);
  // Mode: both - try API key first (for backward compatibility), then OAuth
  if (mode === 'both') {
    return handleBothMode(credentials, request, config);
  // Mode: apiKey - only accept API keys
  if (mode === 'apiKey') {
    return handleApiKeyMode(credentials, request, config);
  // Mode: oauth - only accept OAuth tokens
  if (mode === 'oauth') {
    return handleOAuthMode(credentials);
  // Should never reach here
      code: 'invalid_mode',
      message: `Unknown auth mode: ${mode}`,
 * Handles mode='none' - no authentication required.
function handleNoneMode(config: AuthGateConfig): AuthResult {
  const systemUser = config.getSystemUser();
    console.error('[AuthGate] mode=none but system user not available');
        status: 503,
        code: 'system_user_unavailable',
        message: 'System user not available',
 * Handles mode='apiKey' - only API key authentication.
async function handleApiKeyMode(
  credentials: ExtractedCredentials,
  if (!credentials.apiKey) {
        code: 'missing_credentials',
        message: 'API key required. Provide via x-api-key header.',
  return validateApiKeyCredentials(credentials.apiKey, request, config);
 * Handles mode='oauth' - only OAuth Bearer token authentication.
async function handleOAuthMode(credentials: ExtractedCredentials): Promise<AuthResult> {
  if (!credentials.bearerToken) {
        message: 'OAuth Bearer token required.',
  return validateOAuthCredentials(credentials.bearerToken);
 * Handles mode='both' - accept either API key or OAuth token.
 * API key takes precedence for backward compatibility.
async function handleBothMode(
  // API key takes precedence (backward compatibility)
    const result = await validateApiKeyCredentials(credentials.apiKey, request, config);
    if (result.authenticated) {
    // API key validation failed - try OAuth if available
    if (credentials.bearerToken) {
    return result; // Return API key error
  // No API key - try OAuth
  // No credentials provided
      message: 'Authentication required. Provide API key or OAuth token.',
 * Loads scopes for an API key from the APIKeyScope entity.
 * @param apiKeyId - The ID of the API key
 * @returns Array of scope names
async function loadApiKeyScopes(apiKeyId: string): Promise<string[]> {
  // System API key has all scopes
  if (apiKeyId === 'system') {
    return ['*']; // Wildcard means all scopes
    const result = await rv.RunView<{ Scope: string }>({
      EntityName: 'MJ: API Key Scopes',
      ExtraFilter: `APIKeyID = '${apiKeyId}'`,
      console.warn(`[AuthGate] Failed to load scopes for API key ${apiKeyId}: ${result.ErrorMessage}`);
    return result.Results.map((r) => r.Scope).filter(Boolean);
    console.warn(`[AuthGate] Error loading scopes for API key ${apiKeyId}:`, error);
 * Validates API key credentials.
async function validateApiKeyCredentials(
    const validation = await config.validateApiKey(apiKey, request);
    if (!validation.valid || !validation.user) {
          code: 'invalid_api_key',
          message: validation.error || 'Invalid API key',
    // Load scopes for this API key
    const scopes = await loadApiKeyScopes(validation.apiKeyId!);
      user: validation.user,
      scopes,
        apiKey,
        apiKeyId: validation.apiKeyId!,
        apiKeyHash: validation.apiKeyHash!,
    console.error('[AuthGate] API key validation error:', error);
        code: 'validation_error',
        message: error instanceof Error ? error.message : 'API key validation failed',
 * Validates OAuth Bearer token credentials.
 * Audience validation uses the auth provider's configured audience
 * (auto-populated from environment variables like WEB_CLIENT_ID for Azure AD),
 * matching the same approach as MJExplorer.
async function validateOAuthCredentials(token: string): Promise<AuthResult> {
  // Validate the token - audience is derived from the auth provider
  const validation = await validateBearerToken(token);
  if (!validation.valid || !validation.userInfo) {
    const errorCode = validation.error?.code || 'invalid_token';
    const errorMessage = validation.error?.message || 'Token validation failed';
    // Check for provider unavailable
    if (errorCode === 'provider_unavailable') {
      console.error('[AuthGate] OAuth provider unavailable');
          code: errorCode,
  // Resolve user in MemberJunction
  const userResult = await resolveOAuthUser(validation.userInfo);
  if (userResult.error) {
    const is403 = userResult.error.code === 'user_not_found' || userResult.error.code === 'user_inactive';
        status: is403 ? 403 : 401,
        code: userResult.error.code,
        message: userResult.error.message,
  const payload = validation.payload!;
  // Extract scopes from payload if available (proxy-issued tokens have 'scopes' claim)
  const scopes = (payload as { scopes?: string[] }).scopes;
  console.log(`[AuthGate] Authenticated via OAuth: ${validation.userInfo.email} (issuer: ${payload.iss}${scopes ? `, scopes: ${scopes.length}` : ''})`);
    user: userResult.user!,
    scopes, // Unified scopes field
      issuer: payload.iss!,
      subject: payload.sub!,
      email: validation.userInfo.email!,
      expiresAt: new Date((payload.exp || 0) * 1000),
 * Converts an AuthResult to MCPSessionContext.
 * This maintains backward compatibility with existing Server.ts code.
 * @param result - The authentication result
 * @returns MCPSessionContext for use with MCP handlers
export function toSessionContext(result: AuthResult): MCPSessionContext {
  if (!result.authenticated || !result.user) {
    throw new Error('Cannot create session context from unauthenticated result');
  // Map oauthContext.expiresAt to oauth.tokenExpiresAt for MCPSessionContext
  const oauth = result.oauthContext
    ? {
        issuer: result.oauthContext.issuer,
        subject: result.oauthContext.subject,
        email: result.oauthContext.email,
        tokenExpiresAt: result.oauthContext.expiresAt,
        scopes: result.oauthContext.scopes,
    user: result.user,
    authMethod: result.method,
    scopes: result.scopes, // Unified scopes from either auth method
    apiKey: result.apiKeyContext?.apiKey,
    apiKeyId: result.apiKeyContext?.apiKeyId,
    apiKeyHash: result.apiKeyContext?.apiKeyHash,
    oauth,
 * Sends an appropriate error response based on the AuthResult.
 * @param res - Express response object
 * @param result - The authentication result containing error details
export function sendAuthErrorResponse(res: Response, result: AuthResult): void {
    throw new Error('Cannot send error response for authenticated result');
  const error = result.error!;
  if (error.status === 503) {
    send503Response(res, error.message);
  } else if (error.status === 403) {
    send403Response(res, error.message, error.message);
    // 401 Unauthorized
    const options = isOAuthEnabled() ? {} : { resourceMetadataUrl: undefined };
    send401Response(res, error.message, options);
