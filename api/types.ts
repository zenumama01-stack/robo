 * @fileoverview Type definitions for the MCP Client package
 * Defines interfaces for MCP server connections, tools, authentication,
 * rate limiting, logging, and execution results.
 * @module @memberjunction/ai-mcp-client/types
import type { UserInfo } from '@memberjunction/core';
 * Supported MCP transport types
export type MCPTransportType = 'StreamableHTTP' | 'SSE' | 'Stdio' | 'WebSocket';
 * Supported authentication types for MCP connections
export type MCPAuthType = 'None' | 'Bearer' | 'APIKey' | 'OAuth2' | 'Basic' | 'Custom';
 * Status values for MCP servers
export type MCPServerStatus = 'Active' | 'Inactive' | 'Deprecated';
 * Status values for MCP connections
export type MCPConnectionStatus = 'Active' | 'Inactive' | 'Error';
 * Status values for MCP tools
export type MCPToolStatus = 'Active' | 'Inactive' | 'Deprecated';
 * Configuration for connecting to an MCP server
export interface MCPServerConfig {
    /** Unique identifier for the server */
    /** Display name */
    /** Server description */
    /** Server URL for HTTP/SSE/WebSocket transports */
    ServerURL?: string;
    /** Command for Stdio transport */
    Command?: string;
    /** Command arguments as JSON array string */
    CommandArgs?: string;
    /** Transport type to use */
    TransportType: MCPTransportType;
    /** Default authentication type */
    DefaultAuthType: MCPAuthType;
    /** Expected credential type ID */
    CredentialTypeID?: string;
    /** Server status */
    Status: MCPServerStatus;
    /** Rate limit per minute (null = unlimited) */
    RateLimitPerMinute?: number;
    /** Rate limit per hour (null = unlimited) */
    RateLimitPerHour?: number;
    /** Connection timeout in milliseconds */
    ConnectionTimeoutMs?: number;
    /** Request timeout in milliseconds */
    RequestTimeoutMs?: number;
    // OAuth2 configuration fields
    /** OAuth2 issuer URL for authorization server discovery */
    OAuthIssuerURL?: string;
    /** OAuth2 scopes to request (space-delimited) */
    OAuthScopes?: string;
    /** Metadata cache TTL in minutes (default: 60) */
    OAuthMetadataCacheTTLMinutes?: number;
    /** Pre-configured OAuth client ID (if DCR not used) */
    OAuthClientID?: string;
    /** Pre-configured OAuth client secret (encrypted) */
    OAuthClientSecretEncrypted?: string;
    /** Require PKCE for OAuth2 (default: true) */
    OAuthRequirePKCE?: boolean;
 * Configuration for a specific MCP connection
export interface MCPConnectionConfig {
    /** Unique identifier for the connection */
    /** Reference to the MCP server */
    MCPServerID: string;
    /** Connection name */
    /** Connection description */
    /** Credential ID for authentication */
    CredentialID?: string;
    /** Custom header name for API key auth */
    CustomHeaderName?: string;
    /** Company ID for multi-tenancy */
    CompanyID: string;
    /** Connection status */
    Status: MCPConnectionStatus;
    /** Auto-sync tools on connect */
    AutoSyncTools: boolean;
    /** Auto-generate MJ Actions from tools */
    AutoGenerateActions: boolean;
    /** Log all tool calls */
    LogToolCalls: boolean;
    /** Include input parameters in logs */
    LogInputParameters: boolean;
    /** Include output content in logs */
    LogOutputContent: boolean;
    /** Maximum output size to log in bytes */
    MaxOutputLogSize?: number;
    /** Environment variables for Stdio transport (JSON object) */
    EnvironmentVars?: string;
 * MCP tool definition discovered from server
export interface MCPToolDefinition {
    /** Unique identifier */
    /** Tool name (unique per server) */
    ToolTitle?: string;
    /** Tool description */
    ToolDescription?: string;
    InputSchema: string;
    /** JSON Schema for output (if provided) */
    OutputSchema?: string;
    /** Tool annotations/hints as JSON */
    Annotations?: string;
    /** Tool status */
    Status: MCPToolStatus;
 * Tool annotations from MCP spec
export interface MCPToolAnnotations {
    /** Display name for the tool */
    /** Tool doesn't modify state */
    readOnlyHint?: boolean;
    /** Tool can delete/modify data */
    destructiveHint?: boolean;
    /** Safe to repeat the operation */
    idempotentHint?: boolean;
    /** Interacts with external entities */
    openWorldHint?: boolean;
 * Connection tool configuration
export interface MCPConnectionToolConfig {
    /** Reference to the connection */
    MCPServerConnectionID: string;
    /** Reference to the tool */
    MCPServerToolID: string;
    /** Whether this tool is enabled */
    IsEnabled: boolean;
    /** Default input values as JSON */
    DefaultInputValues?: string;
    /** Per-tool rate limit override */
    MaxCallsPerMinute?: number;
 * Permission entry for a connection
export interface MCPConnectionPermission {
    /** User ID (mutually exclusive with RoleID) */
    UserID?: string;
    /** Role ID (mutually exclusive with UserID) */
    RoleID?: string;
    /** Can invoke tools via this connection */
    CanExecute: boolean;
    /** Can modify connection settings */
    CanModify: boolean;
    /** Can view (not decrypt) credential info */
    CanViewCredentials: boolean;
 * Credential data for MCP authentication
export interface MCPCredentialData {
    /** API key for Bearer/APIKey/Custom auth */
    apiKey?: string;
    /** Username for Basic auth */
    username?: string;
    /** Password for Basic auth */
    password?: string;
    /** Client ID for OAuth2 */
    clientId?: string;
    /** Client secret for OAuth2 */
    clientSecret?: string;
    /** Token URL for OAuth2 */
    tokenUrl?: string;
    /** OAuth2 scopes (space-separated) */
    scope?: string;
 * Options for calling an MCP tool
export interface MCPCallToolOptions {
    /** Tool input parameters */
    arguments: Record<string, unknown>;
    /** Request timeout override in milliseconds */
    /** Abort signal for cancellation */
    signal?: AbortSignal;
    /** Progress callback */
    onProgress?: (progress: MCPProgressInfo) => void;
 * Progress information during tool execution
export interface MCPProgressInfo {
    /** Progress value (0-100) */
    /** Total expected value */
    /** Status message */
 * Result from calling an MCP tool
export interface MCPToolCallResult {
    /** Whether the call succeeded */
    /** Unstructured content from the tool */
    /** Structured result (if provided by tool) */
    structuredContent?: Record<string, unknown>;
    /** Error message if failed */
    /** Error code if failed */
    errorCode?: number;
    /** Execution duration in milliseconds */
    /** Whether the tool indicated an error */
    isToolError: boolean;
 * Content block returned by an MCP tool
export interface MCPContentBlock {
    /** Content type */
    type: 'text' | 'image' | 'audio' | 'resource';
    /** Text content */
    text?: string;
    /** MIME type for binary content */
    /** Base64-encoded data for binary content */
    /** Resource URI */
    uri?: string;
 * Result from listing tools
export interface MCPListToolsResult {
    /** Whether the list succeeded */
    /** Available tools */
    tools: MCPToolInfo[];
 * Tool information returned from list
export interface MCPToolInfo {
    /** Tool name */
    /** JSON Schema for input */
    /** JSON Schema for output */
    outputSchema?: Record<string, unknown>;
    /** Tool annotations */
    annotations?: MCPToolAnnotations;
 * Result from syncing tools
export interface MCPSyncToolsResult {
    /** Whether the sync succeeded */
    /** Number of tools added */
    /** Number of tools updated */
    updated: number;
    /** Number of tools marked deprecated */
    deprecated: number;
    /** Total tools after sync */
 * Result from syncing MCP tools to MJ Actions
export interface MCPSyncActionsResult {
    /** Number of actions created */
    actionsCreated: number;
    /** Number of actions updated */
    actionsUpdated: number;
    /** Number of action params created */
    paramsCreated: number;
    /** Number of action params updated */
    paramsUpdated: number;
    /** Number of action params deleted */
    paramsDeleted: number;
    /** Server category ID that was used/created */
    serverCategoryId?: string;
 * Result from testing a connection
export interface MCPTestConnectionResult {
    /** Whether the test passed */
    /** Server name reported by server */
    serverName?: string;
    /** Server version reported by server */
    serverVersion?: string;
    /** Server capabilities */
    capabilities?: MCPServerCapabilities;
    /** Connection latency in milliseconds */
    latencyMs?: number;
 * Server capabilities from MCP handshake
export interface MCPServerCapabilities {
    /** Supports logging */
    logging?: boolean;
    /** Supports completions */
    completions?: boolean;
    /** Supports prompts */
        listChanged?: boolean;
    /** Supports resources */
    resources?: {
        subscribe?: boolean;
    /** Supports tools */
    tools?: {
 * Active client connection state
export interface MCPActiveConnection {
    /** Connection ID */
    /** Server config */
    serverConfig: MCPServerConfig;
    /** Connection config */
    connectionConfig: MCPConnectionConfig;
    /** MCP Client instance */
    client: unknown; // Client from MCP SDK
    /** Transport instance */
    transport: unknown; // Transport from MCP SDK
    /** When connected */
    connectedAt: Date;
    /** Last activity time */
    lastActivityAt: Date;
    /** Cached server capabilities */
 * Rate limit configuration
export interface RateLimitConfig {
    /** Maximum requests per minute */
    perMinute?: number;
    /** Maximum requests per hour */
    perHour?: number;
 * Rate limit state for tracking
export interface RateLimitState {
    /** Timestamps of requests in current minute window */
    minuteRequests: number[];
    /** Timestamps of requests in current hour window */
    hourRequests: number[];
 * Queued request waiting for rate limit
export interface QueuedRequest {
    /** Unique request ID */
    /** Resolve function to call when request can proceed */
    resolve: () => void;
    /** Reject function to call if request times out */
    reject: (error: Error) => void;
    /** When the request was queued */
    queuedAt: Date;
    /** Maximum time to wait in queue */
    maxWaitMs: number;
 * Execution log entry for tool calls
export interface MCPExecutionLogEntry {
    /** Tool ID (if cached) */
    toolId?: string;
    /** User who initiated the call */
    /** When execution started */
    startedAt: Date;
    /** When execution ended */
    endedAt?: Date;
    /** Duration in milliseconds */
    durationMs?: number;
    /** Whether execution succeeded */
    /** Input parameters (if logging enabled) */
    inputParameters?: Record<string, unknown>;
    /** Output content (if logging enabled) */
    outputContent?: Record<string, unknown>;
    /** Whether output was truncated */
    outputTruncated: boolean;
 * Logging configuration for a connection
export interface MCPLoggingConfig {
    logToolCalls: boolean;
    logInputParameters: boolean;
    logOutputContent: boolean;
    maxOutputLogSize: number;
 * Options for MCPClientManager methods
export interface MCPClientOptions {
    /** User context for permissions and logging */
    /** Skip permission validation (internal use only) */
    skipPermissionCheck?: boolean;
 * Options for connecting to an MCP server
export interface MCPConnectOptions extends MCPClientOptions {
    /** Force reconnect even if already connected */
    forceReconnect?: boolean;
    /** Skip auto-sync of tools */
    skipAutoSync?: boolean;
 * Options for disconnecting from an MCP server
export interface MCPDisconnectOptions extends MCPClientOptions {
    /** Force disconnect even if operations pending */
    force?: boolean;
 * Event types emitted by MCPClientManager
export type MCPClientEventType =
    | 'connected'
    | 'disconnected'
    | 'toolCalled'
    | 'toolCallCompleted'
    | 'toolsSynced'
    | 'connectionError'
    | 'rateLimitExceeded'
    | 'authorizationRequired'
    | 'authorizationCompleted'
    | 'tokenRefreshed'
    | 'tokenRefreshFailed';
 * Event data for MCPClientManager events
export interface MCPClientEvent {
    /** Event type */
    type: MCPClientEventType;
    /** Additional event data */
 * Event listener function type
export type MCPClientEventListener = (event: MCPClientEvent) => void;
 * JSON Schema property definition for MCP tool input parameters
export interface JSONSchemaProperty {
    /** Property type */
    type?: string | string[];
    /** Property description */
    /** Default value */
    default?: unknown;
    /** Enum values */
    enum?: unknown[];
    /** Items schema for arrays */
    items?: JSONSchemaProperty;
    /** Nested properties for objects */
    properties?: Record<string, JSONSchemaProperty>;
    /** Required properties for objects */
    required?: string[];
 * JSON Schema root definition for MCP tool input
export interface JSONSchemaProperties {
    /** Schema type (usually 'object') */
    /** Property definitions */
    /** Required property names */
 * @fileoverview OAuth 2.1 type definitions for MCP server authentication
 * Defines interfaces for OAuth authorization flow with PKCE and
 * Dynamic Client Registration (DCR) per RFC 8414 and RFC 7591.
 * @module @memberjunction/ai-mcp-client/oauth/types
 * OAuth 2.0 Authorization Server Metadata (RFC 8414)
 * Cached from {issuer}/.well-known/oauth-authorization-server
export interface AuthServerMetadata {
    /** Authorization server's issuer identifier URL */
    issuer: string;
    /** URL of the authorization endpoint */
    authorization_endpoint: string;
    /** URL of the token endpoint */
    token_endpoint: string;
    /** URL of the registration endpoint for DCR (optional) */
    registration_endpoint?: string;
    /** URL of the token revocation endpoint (optional) */
    revocation_endpoint?: string;
    /** URL of the token introspection endpoint (optional) */
    introspection_endpoint?: string;
    /** URL of the JSON Web Key Set document */
    jwks_uri?: string;
    /** Array of supported scopes */
    scopes_supported?: string[];
    /** Array of supported response types */
    response_types_supported: string[];
    /** Array of supported grant types */
    grant_types_supported?: string[];
    /** Array of supported token endpoint authentication methods */
    token_endpoint_auth_methods_supported?: string[];
    /** Array of PKCE code challenge methods supported */
    code_challenge_methods_supported?: string[];
 * Cached authorization server metadata with expiration
export interface CachedAuthServerMetadata {
    /** The full metadata object */
    metadata: AuthServerMetadata;
    /** When the metadata was cached */
    cachedAt: Date;
    /** When the cache entry expires */
    expiresAt: Date;
 * Dynamic Client Registration request (RFC 7591)
export interface DCRRequest {
    /** Client name for display */
    client_name: string;
    /** Array of allowed redirect URIs */
    redirect_uris: string[];
    /** Array of grant types the client will use */
    grant_types: string[];
    /** Array of response types the client will use */
    response_types: string[];
    /** Token endpoint authentication method */
    token_endpoint_auth_method: string;
    /** Requested scope (space-delimited) */
 * Dynamic Client Registration response (RFC 7591)
export interface DCRResponse {
    /** Assigned client ID */
    client_id: string;
    /** Assigned client secret (for confidential clients) */
    client_secret?: string;
    /** When the client ID was issued (Unix timestamp) */
    client_id_issued_at?: number;
    /** When the client secret expires (Unix timestamp, 0 = never) */
    client_secret_expires_at?: number;
    /** Registration access token for managing the registration */
    registration_access_token?: string;
    /** Registration client URI for managing the registration */
    registration_client_uri?: string;
    /** Granted redirect URIs */
    redirect_uris?: string[];
    /** Granted grant types */
    grant_types?: string[];
    /** Granted response types */
    response_types?: string[];
    /** Granted scope */
 * PKCE (Proof Key for Code Exchange) challenge data
export interface PKCEChallenge {
    /** Code verifier (random string 43-128 chars) */
    codeVerifier: string;
    /** Code challenge (SHA256 hash of verifier, base64url encoded) */
    codeChallenge: string;
    /** Challenge method (always 'S256' for OAuth 2.1) */
    codeChallengeMethod: 'S256';
 * OAuth 2.0 token response
export interface OAuthTokenResponse {
    /** Access token */
    access_token: string;
    /** Token type (usually 'Bearer') */
    token_type: string;
    /** Seconds until access token expires */
    expires_in?: number;
    /** Refresh token for obtaining new access tokens */
    refresh_token?: string;
    /** Granted scopes (space-delimited) */
 * OAuth token set with expiration tracking
export interface OAuthTokenSet {
    tokenType: string;
    /** Unix timestamp when access token expires */
    expiresAt: number;
    /** Refresh token (optional) */
    /** Authorization server issuer URL */
    /** Unix timestamp of last refresh */
    lastRefreshAt?: number;
    /** Number of times token has been refreshed */
 * OAuth authorization state for tracking in-progress flows
export interface OAuthAuthorizationState {
    /** Database record ID */
    id?: string;
    /** MCP Server Connection ID */
    /** User initiating the flow */
    /** Cryptographic state parameter for CSRF protection */
    /** PKCE challenge data */
    pkce: PKCEChallenge;
    /** Redirect URI used for this flow */
    /** Requested scopes */
    /** Flow status */
    status: OAuthAuthorizationStatus;
    /** Full authorization URL for user redirect */
    authorizationUrl: string;
    errorCode?: string;
    /** Error description if failed */
    /** When flow was initiated */
    initiatedAt: Date;
    /** When flow expires */
    /** When flow completed */
    completedAt?: Date;
    /** URL to redirect to after OAuth completion (for frontend integration) */
    frontendReturnUrl?: string;
 * OAuth authorization flow status
export type OAuthAuthorizationStatus = 'Pending' | 'Completed' | 'Failed' | 'Expired';
 * OAuth client registration status
export type OAuthClientRegistrationStatus = 'Active' | 'Expired' | 'Revoked';
 * Stored OAuth client registration
export interface OAuthClientRegistration {
    /** MCP Server ID (denormalized) */
    /** Registered client ID */
    clientId: string;
    /** Client secret (encrypted at rest) */
    /** When client ID was issued */
    clientIdIssuedAt?: Date;
    /** When client secret expires */
    clientSecretExpiresAt?: Date;
    /** Registration access token for management */
    registrationAccessToken?: string;
    /** Registration client URI for management */
    registrationClientUri?: string;
    /** Registered redirect URIs */
    redirectUris: string[];
    grantTypes: string[];
    responseTypes: string[];
    /** Registration status */
    status: OAuthClientRegistrationStatus;
    /** Full registration response JSON */
    registrationResponse: string;
 * OAuth error response from authorization server
export interface OAuthErrorResponse {
    /** Error code */
    /** Human-readable error description */
    error_description?: string;
    /** URI to error documentation */
    error_uri?: string;
 * OAuth credential values stored in CredentialEngine
export interface OAuth2AuthCodeCredentialValues {
    /** OAuth access token */
    expires_at: number;
    /** OAuth refresh token (optional) */
    authorization_server_issuer: string;
    /** Unix timestamp of last token refresh */
    last_refresh_at?: number;
    refresh_count?: number;
 * Result from initiating OAuth authorization flow
export interface InitiateAuthorizationResult {
    /** Whether initialization succeeded */
    /** URL to redirect user for authorization */
    /** State parameter for tracking this flow */
    /** When this authorization flow expires */
    expiresAt?: Date;
    /** Whether DCR was used (true) or pre-configured credentials (false) */
    usedDynamicRegistration?: boolean;
 * Result from completing OAuth authorization
export interface CompleteAuthorizationResult {
    /** Whether completion succeeded */
    /** Error code from authorization server */
    /** Token set if successful */
    tokens?: OAuthTokenSet;
 * Result from token refresh operation
export interface TokenRefreshResult {
    /** Whether refresh succeeded */
    /** New token set if successful */
 * OAuth connection status for UI display
export interface OAuthConnectionStatus {
    /** Whether OAuth is configured for this connection */
    /** Whether valid OAuth tokens exist */
    /** Whether the access token is expired */
    isAccessTokenExpired?: boolean;
    /** When the access token expires */
    /** Whether a refresh token exists */
    hasRefreshToken?: boolean;
    /** Reason re-authorization is required */
    /** OAuth issuer URL */
    /** Granted scopes */
 * OAuth event types for MCPClientManager
export type OAuthEventType =
    | 'tokenRefreshFailed'
    | 'credentialsRevoked';
 * OAuth event data for MCPClientManager events
export interface OAuthEventData {
    type: OAuthEventType;
    /** Authorization URL (for authorizationRequired) */
    /** State parameter (for authorizationRequired) */
    /** Error message (for failure events) */
    /** Whether re-authorization is required (for tokenRefreshFailed) */
    requiresReauthorization?: boolean;
 * MCP Server OAuth configuration fields
export interface MCPServerOAuthConfig {
    /** Space-delimited OAuth scopes */
    /** Cache TTL for auth server metadata in minutes */
    /** Pre-configured client ID (if DCR not supported) */
    /** Pre-configured client secret (encrypted) */
    /** Whether to require PKCE (always true for OAuth 2.1) */
 * Exception thrown when OAuth authorization is required
export class OAuthAuthorizationRequiredError extends Error {
    /** Error code for identification */
    public readonly code = 'OAUTH_AUTHORIZATION_REQUIRED';
    /** Authorization URL to open in browser */
    public readonly authorizationUrl: string;
    /** State parameter for tracking */
    public readonly stateParameter: string;
    /** When the authorization expires */
    public readonly expiresAt: Date;
        message: string,
        authorizationUrl: string,
        expiresAt: Date
        super(message);
        this.name = 'OAuthAuthorizationRequiredError';
        this.authorizationUrl = authorizationUrl;
        this.stateParameter = stateParameter;
        this.expiresAt = expiresAt;
 * Exception thrown when token refresh fails and re-authorization is needed
export class OAuthReauthorizationRequiredError extends Error {
    public readonly code = 'OAUTH_REAUTHORIZATION_REQUIRED';
    /** Reason for requiring re-authorization */
    public readonly reason: string;
    /** Original error that caused the failure */
    public readonly originalError?: string;
    /** Authorization URL if a new flow was initiated */
    public readonly authorizationUrl?: string;
    /** State parameter for tracking the authorization flow */
    public readonly stateParameter?: string;
        originalError?: string,
        authorizationUrl?: string,
        stateParameter?: string
        this.name = 'OAuthReauthorizationRequiredError';
        this.reason = reason;
        this.originalError = originalError;
 * @fileoverview Type definitions for MCP Server OAuth authentication.
 * Defines interfaces for authentication configuration, token validation results,
 * and session context. These types support the four authentication modes:
 * - apiKey: Traditional API key authentication (default)
 * - oauth: OAuth 2.1 Bearer token authentication
 * - both: Accept either API key or OAuth token
 * - none: No authentication (development only)
 * @module @memberjunction/ai-mcp-server/auth/types
import type { JwtPayload } from 'jsonwebtoken';
 * Authentication mode for the MCP Server.
 * - `apiKey`: API key authentication only (default, backward compatible)
 * - `oauth`: OAuth Bearer token authentication only
 * - `both`: Accept either API key or OAuth token
 * - `none`: No authentication (local development only)
export type AuthMode = 'apiKey' | 'oauth' | 'both' | 'none';
 * OAuth authentication settings for MCP Server.
 * Configures the authentication mode and OAuth-specific options.
export interface MCPServerAuthSettings {
   * Authentication mode controlling which credential types are accepted.
   * @default 'apiKey'
  mode: AuthMode;
   * Resource identifier for OAuth audience validation.
   * This value MUST match the 'aud' claim in OAuth tokens.
   * Should be the MCP Server's public URL or a registered API identifier.
   * - "https://mcp.example.com"
   * - "api://mcp-server-prod"
   * If not specified and autoResourceIdentifier is true,
   * defaults to "http://localhost:{port}"
   * Automatically generate resourceIdentifier from server configuration.
   * When true and resourceIdentifier is not set:
   * - Uses "http://localhost:{port}" where port is mcpServerSettings.port
   * Set to false if you want to require explicit resourceIdentifier.
  autoResourceIdentifier?: boolean;
 * Error codes for OAuth token validation failures.
export type OAuthErrorCode =
  | 'invalid_token'
  | 'expired_token'
  | 'invalid_audience'
  | 'unknown_issuer'
  | 'user_not_found'
  | 'user_inactive'
  | 'provider_unavailable';
 * Result of OAuth token validation.
export interface OAuthValidationResult {
  /** Whether the token is valid */
  /** JWT payload if valid */
  payload?: JwtPayload;
  /** Extracted user information from token claims */
  userInfo?: {
  /** Error details if invalid */
  error?: {
    code: OAuthErrorCode;
 * Result of unified authentication (API key or OAuth).
 * Returned by the AuthGate middleware after credential validation.
export interface AuthResult {
  /** Whether authentication succeeded */
  authenticated: boolean;
  /** Authentication method used */
  method: 'apiKey' | 'oauth' | 'none';
  /** MemberJunction user (if authenticated) */
   * Granted scopes for this authentication.
   * Populated from either:
   * - OAuth JWT 'scopes' claim (when method='oauth')
   * - APIKeyScope entity (when method='apiKey')
  /** API key context (if method='apiKey') */
  apiKeyContext?: {
    apiKeyId: string;
  /** OAuth context (if method='oauth') */
  oauthContext?: {
  /** Error details (if not authenticated) */
    status: 401 | 403 | 503;
 * Session context for MCP requests.
 * Extended to support OAuth authentication alongside existing API key auth.
 * This interface is backward-compatible with the existing MCPSessionContext,
 * adding OAuth-specific fields while preserving API key fields.
export interface MCPSessionContext {
   * The raw API key used for authentication.
   * Present only when authMethod='apiKey'.
   * The database ID of the API key record.
   * The SHA-256 hash of the API key (used for authorization calls).
   * The MemberJunction user associated with the authenticated session.
   * Always present after successful authentication.
   * Authentication method used for this session.
   * Unified field - populated from either:
   * - OAuth JWT 'scopes' claim (when authMethod='oauth')
   * - APIKeyScope entity (when authMethod='apiKey')
   * - Empty array (when authMethod='none')
   * Tools should use this field to check permissions regardless of auth method.
   * OAuth-specific context.
   * Present only when authMethod='oauth'.
    /** The issuer (IdP) that issued the token */
    /** The subject claim (unique user ID at the IdP) */
    /** The user's email from token claims */
    /** When the token expires */
    /** Granted scopes from the JWT token (also available at session.scopes) */
 * OAuth 2.0 Protected Resource Metadata per RFC 9728.
 * Returned from the /.well-known/oauth-protected-resource endpoint.
export interface ProtectedResourceMetadata {
  /** Protected resource identifier - MUST be the resource's URL */
  /** Array of authorization server issuer URLs */
  authorization_servers: string[];
  /** OAuth 2.0 scopes supported by this resource */
  /** Token delivery methods supported */
  bearer_methods_supported?: ('header' | 'body' | 'query')[];
  resource_name?: string;
  resource_documentation?: string;
  /** JWK Set document URL (if resource validates tokens directly) */
 * Claims structure for proxy-signed JWTs.
 * These tokens are issued by the MCP Server OAuth proxy after successful
 * upstream authentication, providing a consistent format across all providers.
export interface ProxyJWTClaims {
  /** Issuer - always "urn:mj:mcp-server" for proxy tokens */
  iss: string;
  /** Subject - user's email address */
  sub: string;
  /** Audience - must match resourceIdentifier */
  aud: string;
  /** Issued at timestamp (seconds since epoch) */
  iat: number;
  /** Expiration timestamp (seconds since epoch) */
  exp: number;
  /** User's email (same as sub) */
  /** MemberJunction User ID (GUID) */
  /** Granted scopes (selected during consent, or all available if no consent screen) */
  /** Upstream provider name for audit trail (from config, not hardcoded enum) */
  /** Upstream subject claim for audit trail */
 * Options for signing a proxy JWT.
export interface SignProxyJWTOptions {
  /** User's email address (becomes sub claim) */
  /** Name of upstream provider that authenticated the user */
 * UI configuration for scope display.
export interface ScopeUIConfig {
  /** Font Awesome icon class */
  /** Hex color for category header */
  color?: string;
 * Scope information loaded from __mj.APIScope entity.
 * Used for consent screen display and scope validation.
export interface APIScopeInfo {
  /** Scope name (e.g., "read" - the last segment) */
  /** Full scope path including parent (e.g., "entity:read") */
  /** Parent scope ID (null for root scopes) */
  /** Category for grouping (e.g., "Entities", "Actions") */
  /** Human-readable description for consent screen */
  /** Whether this scope is active */
  /** UI configuration for display */
  UIConfig?: ScopeUIConfig;
 * Consent flow state (stored in-memory during OAuth flow).
 * Tracks the state of a consent request while the user selects scopes.
export interface ConsentRequest {
  /** Unique request identifier */
  requestId: string;
  /** Timestamp when consent was requested */
  requestedAt: Date;
  /** User information from upstream token */
  /** Upstream provider name that authenticated the user (from config) */
  /** Upstream subject claim */
  /** Available scopes user can select from */
  availableScopes: APIScopeInfo[];
  /** Client redirect URI to return to after consent */
  /** Original OAuth state parameter */
  state?: string;
  /** Code challenge for PKCE */
  /** Code challenge method */
  /** Client ID that initiated the request */
  /** Requested scope string from client */
  requestedScope?: string;
 * User's consent response.
export interface ConsentResponse {
  /** Request ID this response is for */
  /** Scopes the user granted */
  grantedScopes: string[];
  /** Timestamp of consent */
  consentedAt: Date;
import { MJRecommendationEntity, MJRecommendationItemEntity, MJRecommendationProviderEntity, MJRecommendationRunEntity } from "@memberjunction/core-entities";
 * Used to make requests to Recommendation providers
export class RecommendationRequest<T = Record<string, any>> {
     * The ID of the RecommendationRun record that will be created by the caller of a given provider. This must be created before a provider is called.
     * This is done automatically by the Recommendation Engine and can be populated manually if for some reason you want to call a provider directly outside
     * of the engine. This is a required field.
    RunID?: string;
     * Array of the requested recommendations. When preparing a batch of recommendations to request, do NOT set the RecommendationRunID or attempt
     * to save the Recommendation records. This will be done as the batch is processed. You cannot save a Recommendation record until a Run is created
     * which is done by the RecommendationEngineBase.Recommend() method.
    Recommendations?: MJRecommendationEntity[] = [];
     * This is an optional field that can be passed in instead of the Recommendations field. If passed in,
     * the Recommendations field will be populated with Recommendation Entities whose Primary key value matches any of the RecordIDs passed in.
    EntityAndRecordsInfo?: {
         * The name of the entity that the RecordIDs belong to
        EntityName: string,
         * The RecordIDs to fetch recommendations for. Note that if the record IDs
        RecordIDs: Array<string | number>
    ListID?: string;
     * The specific provider to use for the request. Leave this undefined if you want to use the default provider.
    Provider?: MJRecommendationProviderEntity;
     * UserInfo object to use when applicable.
    CurrentUser?: UserInfo;
     * Additional options to pass to the provider
    Options?: T;
     * If true, creates a list that will contain additional informaton regarding errors
     * that may occur during the recommendation run
    CreateErrorList?: boolean;
     * The ID of the error list, if one was created
    ErrorListID?: string;
 * This response is generated for each Recommend() request
export class RecommendationResult {
    Request: RecommendationRequest;
     * The Recommendation Run entity that was created by the recommendation engine
    RecommendationRun?: MJRecommendationRunEntity;
     * The Recommendation Item Entities that were created by the recommendation provider
    RecommendationItems?: MJRecommendationItemEntity[] = [];
    ErrorMessage: string;
    constructor(request: RecommendationRequest) {
        this.Request = request;
        this.Success = true;
        this.ErrorMessage = "";
     * Appends the provided warningMessage param to the ErrorMessage property,
     * with "Warning: " prepended to the message and a newline character appended to the end.
     * The value of the Success property is not changed.
    AppendWarning(warningMessage: string) {
        this.ErrorMessage += `Warning: ${warningMessage} \n`;
     * Appends the provided errorMessage param to the ErrorMessage property,
     * with "Error: " prepended to the message and a newline character appended to the end.
     * Also sets the Success property to false.
    AppendError(errorMessage: string) {
        this.Success = false;
        this.ErrorMessage += `Error: ${errorMessage} \n`;
     * Returns the ErrorMessage property as an array of strings.
     * Useful in the event multiple errors or warnings were produced
     * during the recommendation run.
    GetErrorMessages(): string[] {
        return this.ErrorMessage.split('\n');
 * Enum used for common events coordination throughout various types of application components and forms
export const BaseFormComponentEventCodes = {
    BASE_CODE: 'BaseFormComponent_Event',
    EDITING_COMPLETE: 'EDITING_COMPLETE',
    REVERT_PENDING_CHANGES: 'REVERT_PENDING_CHANGES',
    POPULATE_PENDING_RECORDS: 'POPULATE_PENDING_RECORDS'
} as const
export type BaseFormComponentEventCodes = typeof BaseFormComponentEventCodes[keyof typeof BaseFormComponentEventCodes];
 * Base type for events emitted by classes that interact with the Form Component architecture
export class BaseFormComponentEvent {
    subEventCode!: string
    elementRef: any
    returnValue: any
 * Specialized type of event that is emitted when a form component is telling everyone that editing is complete
export class FormEditingCompleteEvent extends BaseFormComponentEvent {
    subEventCode: string = BaseFormComponentEventCodes.EDITING_COMPLETE;
    pendingChanges: PendingRecordItem[] = [];
 * Type that is used for building an array of pending records that need to be saved or deleted by the Form architecture that sub-components have been editing during an edit cycle
export class PendingRecordItem {
    entityObject!: BaseEntity;
    action: 'save' | 'delete' = 'save';
import { EntityInfo, CompositeKey } from '@memberjunction/core';
  ViewColumnPinned as CoreViewColumnPinned,
  ViewGridSortSetting as CoreViewGridSortSetting,
  ViewGridColumnSetting as CoreViewGridColumnSetting,
  ViewGridState as CoreViewGridState,
// Re-export core types for direct usage
export type ViewColumnPinned = CoreViewColumnPinned;
export type ViewGridSortSetting = CoreViewGridSortSetting;
export type ViewGridColumnSetting = CoreViewGridColumnSetting;
export type ViewGridState = CoreViewGridState;
 * View modes supported by the EntityViewer component
export type EntityViewMode = 'grid' | 'cards' | 'timeline';
 * Behavior when a record is selected
export type RecordSelectionBehavior =
  | 'emit-only'      // Only emit the recordSelected event
  | 'show-detail'    // Show the detail panel (if available)
  | 'emit-and-detail'; // Both emit event and show detail panel
 * Color categories for status pills based on semantic meaning
export type PillColorType = 'success' | 'warning' | 'danger' | 'info' | 'neutral';
  /** Field name from the entity */
  /** Display type for rendering */
  /** Human-readable label */
export interface CardTemplate {
  /** Primary title field name */
  /** Secondary subtitle field name */
  /** Description/notes field name */
   * Array of thumbnail field names in priority order
   * Per-record fallback: if the first field is empty, try the next, etc.
  thumbnailFields: string[];
  /** Badge/priority field name */
 * Column definition for the grid view
export interface GridColumnDef {
  /** Column header text */
  headerName: string;
  /** Column width in pixels */
  /** Minimum column width */
  /** Maximum column width */
  /** Whether column is sortable */
  sortable?: boolean;
  /** Whether column is filterable */
  filter?: boolean;
  /** Whether column is resizable */
  resizable?: boolean;
  /** Whether to hide this column */
  hide?: boolean;
  /** Custom cell renderer type */
  cellRenderer?: string;
  /** Value formatter function name */
  valueFormatter?: string;
 * Event emitted when a record is selected (clicked)
export interface RecordSelectedEvent {
  /** The selected record (plain object from ResultType: 'simple') */
  /** The entity metadata */
  /** The composite key of the record */
 * Event emitted when a record should be opened (double-click or open button)
export interface RecordOpenedEvent {
  /** The record to open (may be undefined for FK navigation where record isn't loaded) */
  record?: Record<string, unknown>;
 * Event emitted when data is loaded
export interface DataLoadedEvent {
  /** Total number of records available */
  totalRowCount: number;
  /** Number of records currently loaded */
  loadedRowCount: number;
  /** Time taken to load in milliseconds */
  loadTime: number;
  /** The loaded records - allows parent to access records for state restoration */
  records: Record<string, unknown>[];
 * Event emitted when filtered count changes
export interface FilteredCountChangedEvent {
  /** Number of records after filtering */
  filteredCount: number;
  /** Total number of records before filtering */
 * Sort direction for server-side sorting
export type SortDirection = 'asc' | 'desc' | null;
 * Sort state for a column
export interface SortState {
  /** Field name to sort by */
  /** Sort direction */
 * Event emitted when sort changes in the grid
export interface SortChangedEvent {
  /** The new sort state (null if no sorting) */
  sort: SortState | null;
 * Pagination state for server-side paging
export interface PaginationState {
  /** Current page number (0-based) */
  /** Number of records per page */
  /** Total number of records available (from server) */
  totalRecords: number;
  /** Whether there are more records to load */
  hasMore: boolean;
  /** Whether data is currently being loaded */
 * Event emitted when pagination changes
export interface PaginationChangedEvent {
  /** The new pagination state */
  pagination: PaginationState;
 * Event emitted when requesting to load more data
export interface LoadMoreEvent {
  /** Current page being requested (0-based) */
  /** Page size */
 * Event emitted when grid state changes (column resize, reorder, etc.)
export interface GridStateChangedEvent {
  /** The updated grid state */
  gridState: ViewGridState;
  /** What changed: 'columns', 'sort', 'filter' */
  changeType: 'columns' | 'sort' | 'filter';
 * Configuration options for the EntityViewer component
export interface EntityViewerConfig {
   * Whether to show the filter input box
  showFilter?: boolean;
   * Whether to show the view mode toggle (grid/cards)
  showViewModeToggle?: boolean;
   * @default 'emit-only'
  selectionBehavior?: RecordSelectionBehavior;
   * Initial view mode
   * @default 'grid'
  defaultViewMode?: EntityViewMode;
   * Whether to enable multi-select
   * Maximum number of records to load per page
   * @default 100
   * Whether to show record count in header
  showRecordCount?: boolean;
   * Placeholder text for the filter input
   * @default 'Filter records...'
  filterPlaceholder?: string;
   * Debounce time for filter input in milliseconds
   * @default 250
  filterDebounceMs?: number;
   * Custom grid column definitions (optional - auto-generated if not provided)
  gridColumns?: GridColumnDef[];
   * Custom card template (optional - auto-generated if not provided)
  cardTemplate?: CardTemplate;
   * Height of the component (CSS value)
   * @default '100%'
  height?: string;
   * Whether to use server-side filtering via UserSearchString
   * When true, filter text is sent to the server for SQL-based filtering
   * When false, filtering is done client-side on loaded records
  serverSideFiltering?: boolean;
   * Whether to use server-side sorting via OrderBy
   * When true, sort changes trigger a new server request
   * When false, sorting is done client-side by AG Grid
  serverSideSorting?: boolean;
   * Whether to show pagination controls
  showPagination?: boolean;
   * Default sort field when loading data
  defaultSortField?: string;
   * Default sort direction when loading data
   * @default 'asc'
  defaultSortDirection?: SortDirection;
 * Timeline segment grouping options
export type TimelineSegmentGrouping = 'none' | 'day' | 'week' | 'month' | 'quarter' | 'year';
export type TimelineOrientation = 'vertical' | 'horizontal';
 * Timeline-specific configuration state
 * Persisted in UserView.DisplayState JSON
export interface TimelineState {
  /** The date field name to use for timeline ordering */
  dateFieldName: string;
  /** Time segment grouping */
  segmentGrouping?: TimelineSegmentGrouping;
  /** Sort order for timeline events */
  sortOrder?: 'asc' | 'desc';
  /** Whether segments are collapsible */
  segmentsCollapsible?: boolean;
  /** Whether segments start expanded */
  segmentsDefaultExpanded?: boolean;
  /** Timeline orientation (vertical or horizontal) */
  orientation?: TimelineOrientation;
 * Card-specific configuration state
export interface CardState {
  /** Custom card size (small, medium, large) */
  cardSize?: 'small' | 'medium' | 'large';
 * Grid-specific configuration state
 * Note: Most grid state is already in GridState column, this is for additional settings
export interface GridDisplayState {
  /** Row height preference */
  rowHeight?: 'compact' | 'normal' | 'comfortable';
   * Enable text wrapping in grid cells
   * When true, long text will wrap to multiple lines and rows will auto-size
 * View display state - persisted in UserView.DisplayState
 * Contains the default view mode and mode-specific configuration
export interface ViewDisplayState {
  /** The default view mode to show when loading this view */
  defaultMode: EntityViewMode;
  /** Which view modes are enabled/visible for this view */
  enabledModes?: {
    grid?: boolean;
    cards?: boolean;
    timeline?: boolean;
  /** Timeline-specific configuration */
  timeline?: TimelineState;
  /** Card-specific configuration */
  cards?: CardState;
  /** Grid-specific configuration */
  grid?: GridDisplayState;
 * Default configuration values
 * Summary of what a view configuration includes, for preview in Quick Save dialog
export interface ViewConfigSummary {
  /** Number of visible columns */
  ColumnCount: number;
  /** Number of active filters */
  FilterCount: number;
  /** Number of sort levels */
  SortCount: number;
  /** Whether smart filter is active */
  SmartFilterActive: boolean;
  /** Smart filter prompt text (if active) */
  SmartFilterPrompt: string;
  /** Number of enabled aggregates */
  AggregateCount: number;
 * Event emitted by QuickSaveDialog when user saves
export interface QuickSaveEvent {
  /** View name */
  /** View description (optional) */
  /** Whether the view is shared */
  IsShared: boolean;
  /** Whether to save as new (true) or update existing (false) */
  SaveAsNew: boolean;
 * Result of a view save operation (for notification display)
export interface ViewSaveResult {
  /** Whether the save succeeded */
  /** Error message (on failure) */
  /** Whether this was a new view or update */
export const DEFAULT_VIEWER_CONFIG: Required<EntityViewerConfig> = {
  showFilter: true,
  showViewModeToggle: true,
  selectionBehavior: 'emit-only',
  defaultViewMode: 'grid',
  pageSize: 100,
  showRecordCount: true,
  filterPlaceholder: 'Filter records...',
  filterDebounceMs: 250,
  gridColumns: [],
  cardTemplate: {
    titleField: '',
    subtitleField: null,
    descriptionField: null,
    displayFields: [],
    thumbnailFields: [],
    badgeField: null
  serverSideFiltering: true,
  serverSideSorting: true,
  showPagination: true,
  defaultSortField: '',
  defaultSortDirection: 'asc'
 * @fileoverview Core type definitions for the MJ Timeline component.
 * This module defines all interfaces, types, and classes used by the timeline.
 * The types are designed to work with both MemberJunction BaseEntity objects
 * and plain JavaScript objects, making the component usable in any Angular application.
 * @module @memberjunction/ng-timeline/types
// UTILITY TYPES
 * Orientation options for the timeline display.
 * - `vertical`: Events displayed top-to-bottom (default)
 * - `horizontal`: Events displayed left-to-right with horizontal scrolling
 * Layout options for vertical timeline.
 * - `single`: All events on one side of the axis
 * - `alternating`: Events alternate between left and right sides
export type TimelineLayout = 'single' | 'alternating';
 * Sort order for timeline events.
 * - `asc`: Oldest events first (ascending by date)
 * - `desc`: Newest events first (descending by date)
export type TimelineSortOrder = 'asc' | 'desc';
 * Grouping options for time segments.
 * - `none`: No grouping, flat list of events
 * - `day`: Group by day
 * - `week`: Group by week
 * - `month`: Group by month (default)
 * - `quarter`: Group by quarter
 * - `year`: Group by year
export type TimeSegmentGrouping = 'none' | 'day' | 'week' | 'month' | 'quarter' | 'year';
 * Button style variants for action buttons.
export type ActionVariant = 'primary' | 'secondary' | 'danger' | 'link';
 * Image position options within a card.
export type ImagePosition = 'left' | 'top' | 'none';
 * Image size presets.
export type ImageSize = 'small' | 'medium' | 'large';
// FIELD DISPLAY CONFIGURATION
 * Configuration for displaying a field within a timeline card.
 * Used for both summary fields (always visible) and expanded fields (visible when expanded).
 * const field: TimelineDisplayField = {
 *   fieldName: 'AssignedTo',
 *   label: 'Assignee',
 *   icon: 'fa-solid fa-user',
 *   formatter: (value) => value?.toString().toUpperCase() ?? 'Unassigned'
export interface TimelineDisplayField {
   * The field name to extract from the record.
   * For BaseEntity objects, this is passed to `.Get()`.
   * For plain objects, this is used as a property key.
   * Display label shown before the value.
   * If not provided, defaults to the fieldName.
   * Font Awesome icon class to display before the label.
   * @example 'fa-solid fa-user'
   * Format string for dates/numbers.
   * For dates, uses Angular DatePipe format strings.
   * For numbers, uses Angular DecimalPipe format strings.
   * Custom formatter function for complex value transformations.
   * Takes precedence over the `format` property.
   * @param value - The raw field value
   * @returns Formatted string to display
  formatter?: (value: unknown) => string;
   * If true, only shows the value without the label.
  hideLabel?: boolean;
   * Additional CSS class(es) to apply to this field's container.
// ACTION BUTTON CONFIGURATION
 * Configuration for an action button displayed on a timeline card.
 * const viewAction: TimelineAction = {
 *   id: 'view',
 *   label: 'View Details',
 *   icon: 'fa-solid fa-eye',
 *   variant: 'primary',
 *   tooltip: 'Open the full details view'
export interface TimelineAction {
   * Unique identifier for this action.
   * Used in event handlers to identify which action was clicked.
   * Display text for the button.
   * Font Awesome icon class to display in the button.
   * @example 'fa-solid fa-edit'
   * Visual style variant for the button.
   * - `primary`: Prominent action (filled/solid)
   * - `secondary`: Standard action (outlined)
   * - `danger`: Destructive action (red)
   * - `link`: Text-only button
   * @default 'secondary'
  variant?: ActionVariant;
   * Tooltip text shown on hover.
   * Whether the action is disabled.
   * Disabled actions are visually muted and not clickable.
   * Additional CSS class(es) to apply to the button.
 * Configuration for how timeline cards are displayed.
 * Can be set at the group level (applies to all events in group) or
 * overridden per-event via `EventConfigFunction`.
 * const cardConfig: TimelineCardConfig = {
 *   showIcon: true,
 *   showDate: true,
 *   dateFormat: 'MMM d, yyyy h:mm a',
 *   descriptionMaxLines: 3,
 *     { fieldName: 'Status', icon: 'fa-solid fa-circle' },
export interface TimelineCardConfig {
  // === Header Section ===
   * Whether to show the icon in the card header.
  showIcon?: boolean;
   * Whether to show the date in the card header.
  showDate?: boolean;
   * Whether to show the subtitle below the title.
  showSubtitle?: boolean;
   * Date format string (Angular DatePipe format).
   * @default 'MMM d, yyyy'
   * @example 'short', 'medium', 'MMM d, yyyy h:mm a'
  dateFormat?: string;
  // === Image Section ===
   * Field name containing the image URL.
   * If not set, no image is displayed.
   * Position of the image within the card.
   * - `left`: Image on the left side of the header
   * - `top`: Image above the content (full width)
   * - `none`: No image displayed
   * @default 'left'
  imagePosition?: ImagePosition;
   * Size preset for the image.
   * - `small`: 48x48px
   * - `medium`: 80x80px
   * - `large`: 120x120px
   * @default 'small'
  imageSize?: ImageSize;
  // === Body Section ===
   * If not set, uses the group's DescriptionFieldName.
   * Maximum number of lines to show before truncating.
   * Set to 0 for no limit.
   * @default 3
  descriptionMaxLines?: number;
   * Whether to render HTML in the description.
   * WARNING: Only enable if content is trusted to prevent XSS.
  allowHtmlDescription?: boolean;
  // === Expansion ===
   * Whether this card can be expanded/collapsed.
  collapsible?: boolean;
   * Whether the card starts in expanded state.
   * Fields to display only when the card is expanded.
   * These appear in the detail section below the description.
  expandedFields?: TimelineDisplayField[];
   * Fields to always display (even when collapsed).
   * These appear as compact field:value pairs.
  summaryFields?: TimelineDisplayField[];
  // === Actions ===
   * Action buttons to display at the bottom of the card.
  actions?: TimelineAction[];
   * If true, actions are only visible on hover/focus.
  actionsOnHover?: boolean;
  // === Styling ===
   * Additional CSS class(es) to apply to the card container.
   * Minimum width for the card.
   * @example '200px', '15rem'
  minWidth?: string;
   * Maximum width for the card.
   * @default '400px'
  maxWidth?: string;
// PER-EVENT CONFIGURATION
 * Per-event display configuration returned by `EventConfigFunction`.
 * Allows customizing individual events within a group.
 * group.EventConfigFunction = (record) => ({
 *   icon: record.Priority === 'High' ? 'fa-solid fa-exclamation' : undefined,
 *   color: record.Status === 'Overdue' ? '#f44336' : undefined,
 *   cssClass: record.IsImportant ? 'important-event' : ''
export interface TimelineEventConfig {
   * Override the icon for this specific event.
   * Font Awesome class string.
   * Override the marker/accent color for this event.
   * Additional CSS class(es) for this event's card.
   * Override the actions for this specific event.
   * Override whether this specific event is collapsible.
   * Override whether this specific event starts expanded.
// VIRTUAL SCROLLING
 * Configuration for virtual scrolling behavior.
 * const scrollConfig: VirtualScrollConfig = {
 *   enabled: true,
 *   batchSize: 25,
 *   loadThreshold: 300,
 *   showLoadingIndicator: true,
 *   loadingMessage: 'Loading more events...'
export interface VirtualScrollConfig {
   * Whether virtual scrolling is enabled.
   * When disabled, all events load at once.
   * Number of events to load per batch.
   * @default 20
  batchSize: number;
   * Distance from bottom (in pixels) at which to trigger loading more.
   * @default 200
  loadThreshold: number;
   * Whether to show a loading indicator while fetching.
  showLoadingIndicator: boolean;
   * Custom message to display while loading.
   * @default 'Loading more events...'
  loadingMessage?: string;
 * Runtime state for virtual scrolling.
 * Exposed as a public property on the component for monitoring.
   * Total number of events available (if known).
   * May be undefined if total is unknown.
  totalCount?: number;
   * Number of events currently loaded and displayed.
  loadedCount: number;
   * Whether a load operation is currently in progress.
   * Current scroll position offset.
  scrollOffset: number;
// TIMELINE EVENT (Internal Representation)
 * Internal representation of a timeline event.
 * Created by mapping source records to this structure.
export interface MJTimelineEvent<T = any> {
   * Unique identifier for this event.
   * Extracted from the source record using IdFieldName.
   * Reference to the original source record.
   * Can be a BaseEntity or plain object.
  entity: T;
   * Event title extracted from TitleFieldName.
   * Event date extracted from DateFieldName.
   * Optional subtitle extracted from SubtitleFieldName.
   * Event description/body text.
   * Image URL if configured.
  imageUrl?: string;
   * Display configuration for this event.
   * Merged from group defaults and per-event overrides.
  config: TimelineEventConfig;
   * Index of the parent group in the groups array.
  groupIndex: number;
   * Current expansion state.
// TIME SEGMENT
 * Represents a collapsible time segment (e.g., "December 2025").
 * Contains a collection of events within the time period.
export interface TimelineSegment {
   * Human-readable label for the segment.
   * @example "December 2025", "Week of Nov 25", "Q4 2025"
   * Start date of this segment (inclusive).
   * End date of this segment (exclusive).
   * Events within this time segment.
  events: MJTimelineEvent[];
   * Current expansion state of the segment.
   * Total count of events (for display when collapsed).
// DEFAULT CONFIGURATIONS
 * Default card configuration applied when not overridden.
export const DEFAULT_CARD_CONFIG: TimelineCardConfig = {
  showIcon: true,
  showSubtitle: true,
  dateFormat: 'MMM d, yyyy',
  imagePosition: 'left',
  imageSize: 'small',
  descriptionMaxLines: 3,
  allowHtmlDescription: false,
  actionsOnHover: false,
  maxWidth: '400px'
 * Default virtual scroll configuration.
export const DEFAULT_VIRTUAL_SCROLL_CONFIG: VirtualScrollConfig = {
  batchSize: 20,
  loadThreshold: 200,
  showLoadingIndicator: true,
  loadingMessage: 'Loading more events...'
 * Default virtual scroll state.
export const DEFAULT_VIRTUAL_SCROLL_STATE: VirtualScrollState = {
  loadedCount: 0,
  scrollOffset: 0
 * Shared types for the ng-versions package.
/** Data passed to the record micro-view for displaying a snapshot record. */
export interface MicroViewData {
    RecordChangeID: string;
    FullRecordJSON: Record<string, unknown> | null;
    FieldDiffs: FieldChangeView[] | null;
/** A single field-level change in a diff comparison. */
export interface FieldChangeView {
/** Display mode for the slide panel container. */
export type SlidePanelMode = 'slide' | 'dialog';
 * Individual channel settings for notification delivery.
 * Each channel can be independently enabled or disabled.
export interface DeliveryChannels {
   * Whether in-app notifications are enabled
  inApp: boolean;
   * Whether email notifications are enabled
  email: boolean;
   * Whether SMS notifications are enabled
  sms: boolean;
 * Parameters for sending a notification through the unified notification system
export interface SendNotificationParams {
   * User ID to send notification to
   * Notification type name (e.g., 'Agent Completion') or UUID
  typeNameOrId: string;
   * Short notification title (used for in-app and email subject)
   * Full notification message (used for in-app display)
   * Optional resource type ID for linking notification to a specific resource
  resourceTypeId?: string;
   * Optional resource record ID for linking notification to a specific record
  resourceRecordId?: string;
   * Optional navigation context stored as JSON (conversation ID, artifact details, etc.)
  resourceConfiguration?: any;
   * Data object for template rendering (email/SMS templates)
  templateData?: Record<string, any>;
   * Force specific delivery channels, overriding user preferences and type defaults.
   * When specified, these channels are used instead of resolving from preferences.
  forceDeliveryChannels?: DeliveryChannels;
 * Result of sending a notification
export interface NotificationResult {
   * Whether the notification operation succeeded overall
   * ID of the created in-app notification (if created)
  inAppNotificationId?: string;
   * Whether email was sent successfully
  emailSent?: boolean;
   * Whether SMS was sent successfully
  smsSent?: boolean;
   * Actual delivery channels used (after resolving preferences)
  deliveryChannels: DeliveryChannels;
   * Any errors that occurred during notification delivery
  errors?: string[];
 * DataSourceInfo holds information about a database connection pool
 * and its configuration. Used to track multiple connections (read-write, read-only).
export class DataSourceInfo {
  dataSource: sql.ConnectionPool;
  port: number;
  instance?: string;
  database: string;
  type: "Admin" | "Read-Write" | "Read-Only" | "Other";
  constructor(init: {
    dataSource: sql.ConnectionPool,
    type: "Admin" | "Read-Write" | "Read-Only" | "Other",
    host: string,
    port: number,
    database: string,
    userName: string
    this.dataSource = init.dataSource;
    this.host = init.host;
    this.port = init.port;
    this.database = init.database;
    this.userName = init.userName;
    this.type = init.type;
 * Configuration options for ComponentRegistryAPIServer
export interface ComponentRegistryServerOptions {
   * Mode of operation for the server
   * - 'standalone': Creates its own Express app and listens on a port (default)
   * - 'router': Returns an Express Router for mounting on existing app
  mode?: 'standalone' | 'router';
   * Base path for API routes
   * Default: '/api/v1'
  basePath?: string;
   * Skip database setup if already initialized by parent application
   * Useful in router mode when parent app manages the database connection
  skipDatabaseSetup?: boolean;
 * Parameters for component feedback submission
export interface ComponentFeedbackParams {
   * Name of the component
  componentName: string;
   * Namespace of the component
  componentNamespace: string;
   * Version of the component (optional)
  componentVersion?: string;
   * Name of the registry (optional)
  registryName?: string;
   * Rating (0-5 scale)
   * Type of feedback (optional)
  feedbackType?: string;
   * Comments/feedback text (optional)
  comments?: string;
   * Associated conversation ID (optional)
  conversationID?: string;
   * Associated conversation detail ID (optional)
  conversationDetailID?: string;
   * Associated report ID (optional)
  reportID?: string;
   * Associated dashboard ID (optional)
  dashboardID?: string;
   * User email for contact lookup (optional)
  userEmail?: string;
 * Response from feedback submission
export interface ComponentFeedbackResponse {
   * Whether the feedback was successfully submitted
   * ID of the created feedback record (if applicable)
  feedbackID?: string;
   * Error message (if unsuccessful)
 * Interface for custom feedback handlers
 * Implement this interface to provide custom feedback logic
export interface FeedbackHandler {
   * Submit component feedback
   * @param params - Feedback parameters
   * @param context - Request context (e.g., authentication info)
   * @returns Promise resolving to feedback response
  submitFeedback(params: ComponentFeedbackParams, context?: any): Promise<ComponentFeedbackResponse>;
 * Configuration for the Component Registry Client
export interface ComponentRegistryClientConfig {
   * Base URL of the Component Registry Server
   * Optional API key for authentication
   * Request timeout in milliseconds
   * Retry policy configuration
  retryPolicy?: RetryPolicy;
   * Custom headers to include with all requests
  headers?: Record<string, string>;
export interface RetryPolicy {
   * Maximum number of retry attempts
   * Initial delay in milliseconds
  initialDelay?: number;
   * Maximum delay in milliseconds
  maxDelay?: number;
   * Backoff multiplier for exponential backoff
  backoffMultiplier?: number;
 * Parameters for getting a component
export interface GetComponentParams {
   * Registry identifier
   * Component namespace
   * Component name
   * Component version (defaults to 'latest')
  version?: string;
   * Optional hash for caching - if provided and matches current spec, returns 304
  hash?: string;
   * User email for tracking and analytics (optional)
 * Component response with hash and caching support
export interface ComponentResponse {
   * Component ID
   * Component version
   * Component specification (undefined if not modified - 304 response)
  specification?: ComponentSpec;
   * SHA-256 hash of the specification
   * Indicates if the component was not modified (304 response)
  notModified?: boolean;
   * Message from server (e.g., "Not modified")
 * Parameters for searching components
export interface SearchComponentsParams {
   * Optional registry filter
  registry?: string;
   * Optional namespace filter
  namespace?: string;
   * Search query string
   * Component type filter
   * Tags to filter by
   * Maximum number of results
   * Offset for pagination
   * Sort field
  sortBy?: 'name' | 'version' | 'createdAt' | 'updatedAt';
   * Sort direction
 * Search result containing components and metadata
export interface ComponentSearchResult {
   * Array of matching components
  components: ComponentSpec[];
   * Total number of matches
   * Current offset
   * Current limit
 * Resolved version information
export interface ResolvedVersion {
   * The resolved version number
   * Whether this is the latest version
  isLatest: boolean;
   * Available versions that match the range
  availableVersions?: string[];
 * Registry information
export interface RegistryInfo {
   * Registry name
   * Registry description
   * Registry version
   * Available namespaces
  namespaces?: string[];
   * Total component count
  componentCount?: number;
   * Registry capabilities
  capabilities?: string[];
 * Namespace information
export interface Namespace {
   * Namespace path
   * Description
   * Number of components in namespace
   * Child namespaces
  children?: Namespace[];
 * Dependency tree structure
export interface DependencyTree {
   * Component identifier
  componentId: string;
   * Direct dependencies
  dependencies?: DependencyTree[];
   * Whether this is a circular dependency
  circular?: boolean;
   * Total count of all dependencies
 * Error codes for registry operations
export enum RegistryErrorCode {
  COMPONENT_NOT_FOUND = 'COMPONENT_NOT_FOUND',
  UNAUTHORIZED = 'UNAUTHORIZED',
  FORBIDDEN = 'FORBIDDEN',
  RATE_LIMITED = 'RATE_LIMITED',
  INVALID_VERSION = 'INVALID_VERSION',
  INVALID_NAMESPACE = 'INVALID_NAMESPACE',
  TIMEOUT = 'TIMEOUT',
  SERVER_ERROR = 'SERVER_ERROR',
  INVALID_RESPONSE = 'INVALID_RESPONSE',
  UNKNOWN = 'UNKNOWN'
 * Registry-specific error class
export class RegistryError extends Error {
    public code: RegistryErrorCode,
    public statusCode?: number,
    public details?: any
    this.name = 'RegistryError';
import { MJCredentialEntity } from "@memberjunction/core-entities";
 * Options for resolving credentials.
export interface CredentialResolutionOptions {
     * Optional: Specific credential ID to retrieve.
     * Takes precedence over credentialName lookup.
     * Optional: Override the credential name to look up.
     * If not provided, uses the name passed to getCredential().
    credentialName?: string;
     * Optional: Direct values to use instead of database lookup.
     * Takes precedence over all other options. Useful for testing
     * or when credentials are provided via other means.
    directValues?: Record<string, string>;
     * Required on server-side: The user context for the operation.
     * Used for audit logging and database access.
     * Optional: Subsystem name for audit logging.
     * Helps identify where credentials are being used.
 * The resolved credential with its values and source information.
 * @template T - The shape of the credential values (defaults to Record<string, string>)
export interface ResolvedCredential<T extends Record<string, string> = Record<string, string>> {
     * The credential entity if loaded from database.
     * Will be null if directValues was provided.
    credential: MJCredentialEntity | null;
     * The decrypted credential values, typed according to the credential type's FieldSchema.
     * For example, for an "API Key" type credential:
     * interface APIKeyValues {
     *     apiKey: string;
     * const cred = await engine.getCredential<APIKeyValues>('OpenAI', options);
     * console.log(cred.values.apiKey); // Strongly typed!
     * Where the credential came from.
     * - 'database': Loaded from the Credentials table
     * - 'request': Provided via directValues option
    source: 'database' | 'request';
     * Optional expiration date from the credential.
     * Null if no expiration is set.
    expiresAt?: Date | null;
 * Options for storing a new credential.
export interface StoreCredentialOptions {
     * If true, this credential will be set as the default for its type.
     * Optional: Category ID to organize the credential.
     * Optional: Icon class for UI display.
     * Optional: Description for the credential.
     * Optional: Expiration date.
 * Result of credential validation.
export interface CredentialValidationResult {
     * Whether the credential is valid.
     * Errors encountered during validation.
     * Non-fatal warnings.
     * When the validation occurred.
    validatedAt: Date;
 * Details logged for credential access.
export interface CredentialAccessDetails {
     * The type of operation performed.
    operation: 'Decrypt' | 'Create' | 'Update' | 'Delete' | 'Validate';
     * The subsystem that accessed the credential.
     * Whether the operation succeeded.
     * Time taken for the operation in milliseconds.
// Common Credential Value Interfaces
// These interfaces match the FieldSchema definitions in the credential types.
// Use them as generic type parameters for getCredential<T>() for type safety.
 * Values for "API Key" credential type.
 * Used by: OpenAI, Anthropic, Groq, Mistral, Google Gemini, SendGrid, etc.
export interface APIKeyCredentialValues {
 * Values for "API Key with Endpoint" credential type.
 * Used by: Azure OpenAI, custom API endpoints
export interface APIKeyWithEndpointCredentialValues {
 * Values for "OAuth2 Client Credentials" credential type.
export interface OAuth2ClientCredentialValues {
    tokenUrl: string;
 * Values for "Basic Auth" credential type.
export interface BasicAuthCredentialValues {
    password: string;
 * Values for "Azure Service Principal" credential type.
 * Used by: Microsoft Graph, Azure OpenAI, Azure Blob Storage
export interface AzureServicePrincipalCredentialValues {
 * Values for "AWS IAM" credential type.
 * Used by: S3, SES, Lambda, other AWS services
export interface AWSIAMCredentialValues {
    accessKeyId: string;
    secretAccessKey: string;
    region: string;
 * Values for "Database Connection" credential type.
export interface DatabaseConnectionCredentialValues {
    port?: number;
 * Values for "Twilio" credential type.
export interface TwilioCredentialValues {
import { BaseEntity, DataObjectRelatedEntityParam, EntityInfo, LogError, Metadata, KeyValuePair, QueryInfo, RunQuery, RunView, RunViewParams, UserInfo, CompositeKey, IMetadataProvider, IRunViewProvider } from "@memberjunction/core";
import { MJDataContextEntity, MJDataContextItemEntity, MJDataContextItemEntityType, UserViewEntityExtended } from "@memberjunction/core-entities";
 * Utility class for storing field info from within a DataContextItem object
export class DataContextFieldInfo {
    Type!: string;
 * Base class and the default implementation for the DataContextItem object, other implementations (sub-classes) can be registered as well with higher priorities to take over for this particular class.
export class DataContextItem {
     * The type of the item, either "view", "query", "full_entity", or "sql", or "single_record"
    Type!: 'view' | 'query' | 'full_entity' | 'sql' | 'single_record';
     * The primary key of the single record in the system, only used if type = 'single_record'. If the Entity has a composite key, this will be a command separated list of the primary key values in order of their definition in the entity.
    RecordID!: string;
     * EntityID - the ID of the entity in the system, only used if type = 'full_entity', 'view', or 'single_record' --- for type of 'query' or 'sql' this property is not used as results can come from any number of entities in combination
    EntityID?: string;
     * ViewID - the ID of the view in the system, only used if type = 'view' 
     * QueryID - the ID of the query in the system, only used if type = 'query'
     * The name of the view, query, or entity in the system. Not used with type='single_record' or type='sql'  
    RecordName!: string;
     * SQL - the SQL statement to execute, only used if type = 'sql'
     * CodeName - property that is generated by the system to be used as a unique name programmatically within a given data context.
     * This is not used in the API, but can be used in the UI or other places where a unique name is needed for the data context item.
     * This is generated by the system and is not set by the user.
     * The name of the entity in the system, only used if type = 'full_entity', 'view', or 'single_record' --- for type of 'query' or 'sql' this property is not used as results can come from any number of entities in combination
    * The fields in the view, query, or entity
    Fields: DataContextFieldInfo[] = [];    
     * This field can be used at run time to stash the record ID in the database of the Data Context Item, if it was already saved. For items that haven't/won't be saved, this property can be ignored.
    DataContextItemID?: string;
     * ViewEntity - the object instantiated that contains the metadata for the UserView being used - only populated if the type is 'view', also this is NOT to be sent to/from the API server, it is a placeholder that can be used 
     *              within a given tier like in the MJAPI server or in the UI.
    ViewEntity?: UserViewEntityExtended;
     * SingleRecord - the object instantiated that contains the data for the single record being used - only populated if the type is 'single_record' - also this is NOT to be sent to/from the API server, it is a placeholder that can be used in a given tier
    SingleRecord?: BaseEntity;
     * Entity - the object that contains metadata for the entity being used, only populated if the type is 'full_entity' or 'view' - also this is NOT to be sent to/from the API server, it is a placeholder that can be used
     *          within a given tier like in the MJAPI server or in the UI.
    Entity?: EntityInfo;
    /** Additional Description has any other information that might be useful for someone (or an LLM) intepreting the contents of this data item */
    AdditionalDescription?: string;
     * This property contains the loaded data for the DataContextItem, if it was loaded successfully. The data will be in the form of an array of objects, where each object is a row of data. 
    public get Data(): any[] {
        return this._Data;
    public set Data(value: any[]) {
        this._Data = value;
        this.DataLoaded = value !== null && value !== undefined;
        if (this.DataLoaded)
            this.DataLoadingError = null;
    private _Data?: any[];
     * This property is set to true if the data has been loaded for this DataContextItem, and false if it has not been loaded or if there was an error loading the data.  
    DataLoaded: boolean = false;
     * This property contains an error message if there was an error loading the data for this DataContextItem. If there was no error, this property will be null;
    DataLoadingError?: string;
     * Generated description of the item  which is dependent on the type of the item
        let ret: string = '';
        switch (this.Type) {
                ret = `View: ${this.RecordName}, From Entity: ${this.EntityName}`;
                ret = `Query: ${this.RecordName}`;
            case 'full_entity':
                ret = `Full Entity - All Records: ${this.EntityName}`;
                ret = `SQL Statement: ${this.RecordName}`;
                ret = `Unknown Type: ${this.Type}`;
        if (this.AdditionalDescription && this.AdditionalDescription.length > 0) 
            ret += ` (More Info: ${this.AdditionalDescription})`;
     * Create a new DataContextItem from a MJUserViewEntity class instance
     * @param viewEntity 
    public static FromViewEntity(viewEntity: UserViewEntityExtended) {
        const instance = DataContext.CreateDataContextItem();
        // update our data from the viewEntity definition
        instance.Type= 'view';
        instance.ViewEntity = viewEntity;
        instance.Entity = viewEntity.ViewEntityInfo;
        instance.EntityName = viewEntity.ViewEntityInfo.Name;
        instance.ViewID = viewEntity.ID;
        instance.RecordName = viewEntity.Name;
        instance.Fields = viewEntity.ViewEntityInfo.Fields.map(f => {
     * Create a new DataContextItem from a BaseEntity class instance
     * @param singleRecord 
    public static FromSingleRecord(singleRecord: BaseEntity) {
        instance.Type = 'single_record';
        instance.RecordID = singleRecord.PrimaryKey.ToString();
        instance.EntityID = singleRecord.EntityInfo.ID;
        instance.EntityName = singleRecord.EntityInfo.Name;
        instance.SingleRecord = singleRecord;
     * Create a new DataContextItem from a QueryInfo class instance
    public static FromQuery(query: QueryInfo) {
        instance.Type = 'query';
        instance.QueryID = query.ID;
        instance.RecordName = query.Name;
        instance.Fields = query.Fields.map(f => {
                Type: f.SQLBaseType,
     * Create a new DataContextItem from a EntityInfo class instance
    public static FromFullEntity(entity: EntityInfo) {
        instance.Type = 'full_entity';
        instance.EntityID = entity.ID;
        instance.EntityName = entity.Name;
        instance.Entity = entity;
        instance.RecordName = entity.Name;
        instance.Fields = entity.Fields.map(f => {
     * This method should only be called after this Item has been fully initialized. That can be done by calling LoadMetadata() on the DataContext object, 
     * or by calling the static methods FromViewEntity, FromSingleRecord, FromQuery, or FromFullEntity, or finally by manually setting the individual properties of the DataContextItem object.
     * A helper method, Load() at the DataContext level can be called to load the metadata and then all of the data for all items in the data context at once.
     * @param dataSource - the data source to use to execute the SQL statement - specified as an any type to allow for any type of data source to be used, but the actual implementation will be specific to the server side only. For client side use of this method, you can leave this as undefined and the Load will work so long as the Data Context Items you are loading are NOT of type 'sql'
     * @param forceRefresh - (defaults to false) if true, the data will be reloaded from the data source even if it is already loaded, if false, the data will only be loaded if it hasn't already been loaded
     * @param loadRelatedDataOnSingleRecords - (defaults to false) if true, related entity data will be loaded for single record items, if false, related entity data will not be loaded for single record items
     * @param maxRecordsPerRelationship - (defaults to 0) for the LoadData() portion of this routine --- if this param is set to a value greater than 0, the maximum number of records to load for each relationship will be limited to this value. Applies to single_record items only.
     * @param contextUser - the user that is requesting the data context (only required on server side operations, or if you want a different user's permissions to be used for the data context load)
    public async LoadData(dataSource: any, forceRefresh: boolean = false, loadRelatedDataOnSingleRecords: boolean = false, maxRecordsPerRelationship: number = 0, contextUser?: UserInfo): Promise<boolean> {
            if (this.Data && this.Data.length > 0 && !forceRefresh) // if we already have data and we aren't forcing a refresh, then we are done
                        return this.LoadFromFullEntity(contextUser);
                        return this.LoadFromView(contextUser);
                    case 'single_record':
                        return this.LoadFromSingleRecord(contextUser, loadRelatedDataOnSingleRecords, maxRecordsPerRelationship);
                        return this.LoadFromQuery(contextUser);
                        return this.LoadFromSQL(dataSource, contextUser);
            LogError(`Error in DataContextItem.Load: ${e && e.message ? e.message : ''}`);
    public async LoadMetadataFromEntityRecord(dataContextItem: MJDataContextItemEntity, provider: IMetadataProvider, contextUser: UserInfo) {
        this.DataContextItemID = dataContextItem.ID;
        this.Type = <"view" | "query" | "full_entity" | "sql" | "single_record">dataContextItem.Type;
                this.EntityID = dataContextItem.EntityID;  
                this.RecordID = dataContextItem.RecordID;  
                this.QueryID = dataContextItem.QueryID; // map the QueryID in our database to the RecordID field in the object model for runtime use
                const q = provider.Queries.find((q) => q.ID === this.QueryID);
                this.RecordName = q?.Name;
                this.SQL = q.SQL;
                this.CodeName = dataContextItem.CodeName;
                this.SQL = dataContextItem.SQL;  
                this.ViewID = dataContextItem.ViewID;
                this.EntityID = dataContextItem.EntityID; // attempt to get this from the database, often will be null though
                if (this.ViewID) {
                    const v = await provider.GetEntityObject<UserViewEntityExtended>('MJ: User Views', contextUser);
                    await v.Load(this.ViewID);
                    this.RecordName = v.Name;
                    this.EntityID = v.ViewEntityInfo.ID; // if we get here, we overwrite whateer we had above because we have the actual view metadata.
                    this.ViewEntity = v;
                    this.SQL =  `SELECT * FROM [${v.ViewEntityInfo.SchemaName}].[${v.ViewEntityInfo.BaseView}]${v.WhereClause && v.WhereClause.length > 0 ? ' WHERE ' + v.WhereClause : ''}`;
        if (this.EntityID) {
            this.Entity = provider.Entities.find((e) => e.ID === this.EntityID);
            this.EntityName = this.Entity.Name;
            this.Fields = DataContext.MapEntityFieldsToDataContextFields(this.Entity);
            if (this.Type === 'full_entity')
                this.RecordName = this.EntityName;
        if (dataContextItem.DataJSON && dataContextItem.DataJSON.length > 0) {
            this.Data = JSON.parse(dataContextItem.DataJSON);
     * Loads the data context item data from a view. This method is called by the LoadData method if the type of the data context item is 'view'
    protected async LoadFromView(contextUser: UserInfo): Promise<boolean> {
            const viewParams: RunViewParams = { IgnoreMaxRows: true }; // ignore max rows for both types
            viewParams.Fields = this.ViewEntity.ViewEntityInfo.Fields.map((f) => f.Name); // include all fields
            viewParams.ViewID = this.ViewID;
            const viewResult = await rv.RunView(viewParams, contextUser);
            if (viewResult && viewResult.Success) {
                this.Data = viewResult.Results;
                this.DataLoadingError = `Error running view. View Params: ${JSON.stringify(viewParams)}`;
                LogError(this.DataLoadingError);
            LogError(`Error in DataContextItem.LoadFromView: ${e && e.message ? e.message : ''}`);
     * Loads the data context item data from a full entity (meaning all rows in a given entity). This method is called by the LoadData method if the type of the data context item is 'full_entity'
    protected async LoadFromFullEntity(contextUser: UserInfo): Promise<boolean> {
            const e = md.Entities.find((e) => e.ID === this.EntityID);
            viewParams.EntityName = e.Name;
            LogError(`Error in DataContextItem.LoadFromFullEntity: ${e && e.message ? e.message : ''}`);
     * Loads the data context item data from a query. This method is called by the LoadData method if the type of the data context item is 'query' 
    protected async LoadFromSingleRecord(contextUser: UserInfo, includeRelatedEntityData: boolean, maxRecordsPerRelationship: number): Promise<boolean> {
            const record = await md.GetEntityObject(this.EntityName, contextUser);
            const pkeyVals: KeyValuePair[] = [];
            const ei = md.Entities.find((e) => e.ID === this.EntityID);
            const rawVals = this.RecordID.split(',');
            for (let i = 0; i < ei.PrimaryKeys.length; i++) {
                const pk = ei.PrimaryKeys[i];
                const v = rawVals[i];
                pkeyVals.push({FieldName: pk.Name, Value: v});
            compositeKey.KeyValuePairs = pkeyVals;
            if (await record.InnerLoad(compositeKey)) {
                const dataObject = await record.GetDataObject({
                    includeRelatedEntityData: includeRelatedEntityData,
                    relatedEntityList: includeRelatedEntityData ? this.buildRelatedEntityArray(maxRecordsPerRelationship) : [],
                    excludeFields: []
                this.Data = [dataObject]; // we always return an array of one object for single record loads
                this.DataLoadingError = `Error loading single record: ${this.RecordName}`;
            this.DataLoadingError = `Error in DataContextItem.LoadFromSingleRecord: ${e && e.message ? e.message : ''}`;
    protected buildRelatedEntityArray(maxRecords: number): DataObjectRelatedEntityParam[] {
        return this.Entity.RelatedEntities.map(re => {
                relatedEntityName: re.RelatedEntity,
                maxRecords: maxRecords
    protected async LoadFromQuery(contextUser: UserInfo): Promise<boolean> {
            const queryResult = await rq.RunQuery({QueryID: this.QueryID}, contextUser);
            if (queryResult && queryResult.Success) {
                this.Data = queryResult.Results;
                this.DataLoadingError = `Error running query ${this.RecordName}`;
            this.DataLoadingError = `Error in DataContextItem.LoadFromQuery: ${e && e.message ? e.message : ''}`;
     * If you already have the data loaded for an individual Data Context Item, you can load it into the object using this method. It is your responsibility to ensure
     * that the data object is in the correct format for the DataContextItem object. This method will not validate the data object, it will just load it into the Data property of the object.
     * @param dataObject 
    public LoadDataFromObject(data: any[]): boolean {
                this.Data = data;
                this.DataLoadingError = `Error loading - data is null or undefined`;
            this.DataLoadingError = `Error in DataContextItem.LoadFromDataObject: ${e && e.message ? e.message : ''}`;
     * Overrideable in sub-classes, the default implementation will throw an error because we don't have the ability to execute random SQL on the client side
     * @param dataSource - the data source to use to execute the SQL statement - specified as an any type to allow for any type of data source to be used, but the actual implementation will be specific to the server side only
    protected async LoadFromSQL(dataSource: any, contextUser: UserInfo): Promise<boolean> {
        throw new Error(`Not implemented in the base DataContextItem object. The server-side only sub-class of the DataContextItem object implements this method. 
                         Make sure you include @memberjunction/data-context-server in your project and use the DataContextItemServer class instead of DataContextItem. 
                         This happens automatically if you use the DataContext.Load() or DataContext.LoadMetadata() methods to load the data context.`);
     * Validates that the Data property is set. Valid states include a zero length array, or an array with one or more elements. If the Data property is not set, this method will return false
     * @param ignoreFailedLoad - if true, we will not validate the data if the DataLoaded property is false, if false, we will validate the data regardless of the DataLoaded property
    public ValidateDataExists(ignoreFailedLoad: boolean = false): boolean {
        if (ignoreFailedLoad && !this.DataLoaded)
            return this.Data ? this.Data.length >= 0 : false; // can have 0 to many rows, just need to make sure we have a Data object to work with
     * Creates a new DataContextItem object from a raw data object. This method will return a new DataContextItem object if the raw data was successfully converted, and will return null if the raw data was not successfully converted.
     * @param rawItem 
    public static FromRawItem(rawItem: any): DataContextItem {
        const item = DataContext.CreateDataContextItem();
        item.Type = rawItem.Type;
        item.RecordID = rawItem.RecordID;
        item.EntityID = rawItem.EntityID;
        item.ViewID = rawItem.ViewID;
        item.QueryID = rawItem.QueryID;
        item.SQL = rawItem.SQL;
        item.CodeName = rawItem.CodeName;
        item.EntityName = rawItem.EntityName;
        item.RecordName = rawItem.RecordName;
        item.AdditionalDescription = rawItem.AdditionalDescription;
        item.DataContextItemID = rawItem.DataContextItemID;
        item._Data = rawItem._Data;
        item.DataLoaded = rawItem.DataLoaded;
        item.DataLoadingError = rawItem.DataLoadingError;
        if (rawItem.Fields && rawItem.Fields.length > 0) {
            item.Fields = rawItem.Fields.map((f: any) => {
 *  Base class and the default implementation for the DataContext object, other implementations can be registered as well with higher priorities
export class DataContext {
     * The ID of the data context in the system
     * The object holding all the metadata for the data context - this only is in place automatically if you called the `LoadMetadata` method
    MJDataContextEntity: MJDataContextEntity;
     * The items in the data context
    Items: DataContextItem[] = [];
     * Simple validation method that determines if all of the items in the data context have data set. This doesn't mean the items have data in them as zero-length data is consider valid, it is checking to see if the Data property is set on each item or not
     * @param ignoreFailedLoadItems - if set to true, we will ignore individual items that have not been loaded due to loading errors and only validate the data exists in the items that have been loaded. If set to false, we will validate all items regardless of their load state
    public ValidateDataExists(ignoreFailedLoadItems: boolean = false): boolean {
        if (this.Items)
            return !this.Items.some(i => !i.ValidateDataExists(ignoreFailedLoadItems)); // if any data item is invalid, return false
     * Return a simple object that will have a property for each item in our Items array. We will name each item sequentially as data_item_1, data_item_2, etc, using the itemPrefix parameter
     * @param itemPrefix defaults to 'data_item_' and can be set to anything desired
     * @param includeFailedLoadItems - if true, we will include items that have not been loaded due to loading errors in the output object, if false, we will only include items that have been loaded successfully
    public ConvertToSimpleObject(itemPrefix: string = 'data_item_', includeFailedLoadItems: boolean = false): any {
        const ret: any = {};
        const items = includeFailedLoadItems ? this.Items : this.Items.filter(i => i.DataLoaded);
            const item = items[i];
            ret[`${itemPrefix}${i}`] = item.Data;
     * Return a string that contains a type definition for a simple object for this data context. The object will have a property for each item in our Items array. We will name each item sequentially as data_item_1, data_item_2, etc, using the itemPrefix parameter
    public CreateSimpleObjectTypeDefinition(itemPrefix: string = 'data_item_', includeFailedLoadItems: boolean = false): string {
        let sOutput: string = "";
            sOutput += `${itemPrefix}${i}: []; // ${item.Description}\n`;
        return `{${sOutput}}`;
     * This method will load ONLY the metadata for the data context and data context items associated with the data context. This method will not load any data for the data context items. This method will return a promise that will resolve to true if the metadata was loaded successfully, and false if it was not.
     * @param DataContextID - the ID of the data context to load
     * @param provider - optional, the metadata provider to use to load the metadata. If not provided, the default metadata provider will be used.
    public async LoadMetadata(DataContextID: string, contextUser?: UserInfo, provider?: IMetadataProvider): Promise<boolean> {
            if (!DataContextID || DataContextID.length === 0)
                throw new Error(`Data Context ID not set or invalid`);
            const p = provider ? provider : Metadata.Provider; 
            const rv = <IRunViewProvider><any>p;
            const dciEntityInfo = p.Entities.find((e) => e.Name === 'MJ: Data Context Items');
            if (!dciEntityInfo)
              throw new Error(`Data Context Items entity not found`);
            this.MJDataContextEntity = await p.GetEntityObject<MJDataContextEntity>('MJ: Data Contexts', contextUser);
            await this.MJDataContextEntity.Load(DataContextID);
            this.ID = this.MJDataContextEntity.ID; // do it this way to make sure it loaded properly
            if (!this.ID)
                throw new Error(`Data Context ID: ${DataContextID} not found`);
            const result = await rv.RunView<MJDataContextItemEntity>({EntityName: 'MJ: Data Context Items', IgnoreMaxRows: true, ExtraFilter: `DataContextID = '${DataContextID}'`}, contextUser);
              throw new Error(`Error running view to retrieve data context items for data context ID: ${DataContextID}`);
                const items = result.Results;
                    const r = items[i];
                    const item = this.AddDataContextItem();
                    await item.LoadMetadataFromEntityRecord(r, p, contextUser);
            LogError(`Error in DataContext.LoadMetadata: ${ex && ex.message ? ex.message : ''}`);
     * Utilty method to map an EntityInfo object's fields to the simpler DataContextFieldInfo object. This is used to simplify the data context item fields.
    public static MapEntityFieldsToDataContextFields(entity: EntityInfo): DataContextFieldInfo[] {
        return entity.Fields.map(f => {
     * Saves the data context items to the database. For each data context item, if it has an existing ID in the database, that database record will be updated. 
     * For data context items that don't have an ID (meaning they've not yet been saved), a new record will be created in the database.
     * This method will return a promise that will resolve to true if the data was saved successfully, and false if it was not.
     * IMPORTANT: This method will not save if the ID property of the object is not set to a valid value.
     * @param contextUser - optional, the user that is requesting the data context (only required on server side operations, or if you want a different user's permissions to be used for the data context load)
     * @param persistItemData - optional, if true, the data for each item will be saved to the database, if false, the data will not be saved to the database. The default is false.
    public async SaveItems(contextUser?: UserInfo, persistItemData: boolean = false): Promise<boolean> {
            if (!this.ID || this.ID.length === 0)
            const itemsArray = this.Items.map((item) => {return {
                item: item,
                dciEntity: null
            for (const itemEntry of itemsArray) {
                const item = itemEntry.item;
                const dciEntity = await md.GetEntityObject<MJDataContextItemEntity>('MJ: Data Context Items', contextUser);
                if (item.DataContextItemID && item.DataContextItemID.length > 0) 
                  await dciEntity.Load(item.DataContextItemID);
                  dciEntity.NewRecord();
                itemEntry.dciEntity = dciEntity; // saved for later 
                dciEntity.Description = item.AdditionalDescription;
                dciEntity.DataContextID = this.ID;
                dciEntity.Type = item.Type;
                dciEntity.CodeName = item.CodeName;
                switch (item.Type) {
                    const e = item.Entity || md.Entities.find((e) => e.Name === item.EntityName);
                    dciEntity.EntityID = e.ID;
                    if (item.Type === 'single_record')
                      dciEntity.RecordID = item.RecordID;
                    dciEntity.ViewID = item.ViewID;  
                    dciEntity.QueryID = item.QueryID;  
                    dciEntity.SQL = item.SQL;  
                if (persistItemData && item.Data && item.Data.length > 0 )
                    dciEntity.DataJSON = JSON.stringify(item.Data); 
                    dciEntity.DataJSON = null; //JSON.stringify(item.Data); 
                dciEntity.TransactionGroup = tg;
                await dciEntity.Save()  
            const result = await tg.Submit();
                // go through each item and update its DataContextItemID
                    itemEntry.item.DataContextItemID = itemEntry.dciEntity.ID;
            LogError(`Error in DataContext.SaveItems: ${e && e.message ? e.message : ''}`);
     * This method will create a new DataContextItem object and add it to the data context. This method will return the newly created DataContextItem object.
    public AddDataContextItem(): DataContextItem {
        // get a new data context item. Using class factory instead of directly instantiating the class so that we can use the class factory 
        // to override the default class with a custom class if another package registers a higher priority sub-class than our default impleemtnation - for example - server side implementations...
        this.Items.push(item);
     * This method will create a new DataContextItem object. This method is used internally by the AddDataContextItem method, but can also be called directly if you need to create a DataContextItem object for some other purpose. 
     * NOTE: this method does NOT add the newly created DataContextItem to the data context, you must do that yourself if you use this method directly.
    public static CreateDataContextItem(): DataContextItem {
        const item = <DataContextItem>MJGlobal.Instance.ClassFactory.CreateInstance(DataContextItem); 
     * This method will load the data for the data context items associated with the data context. This method must be called ONLY after LoadMetadata(). This method will return a promise that will resolve to true if the data was loaded successfully, and false if it was not.
            let promises = this.Items.map(async (item) => {
                return item.LoadData(dataSource, forceRefresh, loadRelatedDataOnSingleRecords, maxRecordsPerRelationship, contextUser);
            const results: boolean[] = await Promise.all(promises);
            return results.every(r => r); // return true only if all items loaded successfully, otherwise return false
            LogError(`Error in DataContext.LoadData: ${e && e.message ? e.message : ''}`);
     * If you already have the data loaded for an entire Data Context you can pass it in as a two dimensional array.
     * The first dimension is the Dataset Item and the second dimension is the array of rows for that given Dataset Item.
     * YOU are responsible for ensuring the ORDER of the first dimension, for the sequence of the Dataset Items, matches the items in the metadata, 
     * this method doesn't attempt to do any validation.
    public LoadDataFromObject(data: any[][]): boolean {
            if (data && data.length > 0 && data.length === this.Items.length) {
                for (let i = 0; i < this.Items.length; ++i) {
                    const item = this.Items[i];
                    const dataItem = data[i];
                    success = success && item.LoadDataFromObject(dataItem);
                // invalid state either data is not provided, has no length or the length isn't a match for the # of items in the data context
                throw new Error(`Error loading data from object - data is not valid. Data: ${JSON.stringify(data ? data : {})}`);
     * This method will load both the metadata and the data for the data context items associated with the data context. This method will return a promise that will resolve to true if the data was loaded successfully, and false if it was not.
     * @param forceRefresh - (defaults to false) for the LoadData() portion of this routine --- if this param is set to true, the data will be reloaded from the data source even if it is already loaded, if false, the data will only be loaded if it hasn't already been loaded
     * @param loadRelatedDataOnSingleRecords - (defaults to false) for the LoadData() portion of this routine --- if this param is set to true, related entity data will be loaded for single record items, if false, related entity data will not be loaded for single record items
    public async Load(DataContextID: string, dataSource: any, forceRefresh: boolean = false, loadRelatedDataOnSingleRecords: boolean = false, maxRecordsPerRelationship: number = 0, contextUser?: UserInfo): Promise<boolean> {
        // load the metadata and THEN the data afterwards
        return await this.LoadMetadata(DataContextID, contextUser) && 
               await this.LoadData(dataSource, forceRefresh, loadRelatedDataOnSingleRecords, maxRecordsPerRelationship, contextUser);
     * Utility method to create a new DataContext object from a raw data object. This method will return a promise that will resolve to a new DataContext object if the raw data was successfully converted, and will reject if the raw data was not successfully converted.
     * @param rawData 
    public static async FromRawData(rawData: any): Promise<DataContext> {
        const newContext = new DataContext();
        if (rawData) {
            newContext.ID = rawData.ID;
            if (rawData.Items && rawData.Items.length > 0) {
                for (const rawItem of rawData.Items) {
                    const item = DataContextItem.FromRawItem(rawItem); 
                        newContext.Items.push(item);
        return newContext;
     * This method will clone the data context and all of its items. This method will return a promise that will resolve to a new DataContext object if the cloning was successful, and will reject if the cloning was not successful.
     * @param context 
    public static async Clone(context: DataContext, includeData: boolean = false, contextUser: UserInfo = undefined): Promise<DataContext> {
            // first, clone the data context itself at the top level
            const currentContext = await md.GetEntityObject<MJDataContextEntity>('MJ: Data Contexts', contextUser);
            await currentContext.Load(context.ID);
            const newContext = await md.GetEntityObject<MJDataContextEntity>('MJ: Data Contexts', contextUser);
            newContext.NewRecord();
            newContext.CopyFrom(currentContext, false);
            if (await newContext.Save()) {
                // we've saved our new data context, now we need to save all of the items
                for (let item of context.Items) {
                    const currentItem = await md.GetEntityObject<MJDataContextItemEntity>('MJ: Data Context Items', contextUser);
                    await currentItem.Load(item.DataContextItemID);
                    const newItem = await md.GetEntityObject<MJDataContextItemEntity>('MJ: Data Context Items', contextUser); 
                    newItem.NewRecord();
                    newItem.CopyFrom(currentItem, false);
                    newItem.DataContextID = newContext.ID; // overwrite the data context ID with the new data context ID
                    if (!includeData)
                        newItem.DataJSON = null; // if we aren't including the data, we need to clear it out
                    newItem.TransactionGroup = tg;
                    await newItem.Save();  
                    throw new Error(`Error saving new data context items`);
                // if we get here we've succeeded, so return the new data context
                const newContextObject = new DataContext();
                await newContextObject.LoadMetadata(newContext.ID, contextUser);
                return newContextObject;
                throw new Error(`Error saving new data context`);
            LogError(`Error in DataContext.Clone: ${e && e.message ? e.message : ''}`);
 * Export format types
export type ExportFormat = 'excel' | 'csv' | 'json';
 * Data row type - either an object with string keys or an array
export type ExportDataRow = Record<string, unknown> | unknown[];
 * Data array type for export
export type ExportData = ExportDataRow[];
 * Row sampling mode for exports
export type SamplingMode = 'all' | 'top' | 'bottom' | 'every-nth' | 'random';
 * Column data type hint for formatting
export type ColumnDataType = 'string' | 'number' | 'date' | 'boolean' | 'currency' | 'percentage';
 * Cell border line style
export type BorderLineStyle = 'thin' | 'medium' | 'thick' | 'dotted' | 'dashed' | 'double';
 * Horizontal alignment options
export type HorizontalAlignment = 'left' | 'center' | 'right' | 'fill' | 'justify';
 * Vertical alignment options
export type VerticalAlignment = 'top' | 'middle' | 'bottom';
 * Fill pattern type
export type FillPattern = 'none' | 'solid' | 'darkGray' | 'mediumGray' | 'lightGray' |
  'darkHorizontal' | 'darkVertical' | 'darkDown' | 'darkUp' | 'darkGrid' | 'darkTrellis' |
  'lightHorizontal' | 'lightVertical' | 'lightDown' | 'lightUp' | 'lightGrid' | 'lightTrellis';
 * Font styling options
export interface FontStyle {
  /** Font family name */
  /** Font size in points */
  /** Underline text */
  underline?: boolean | 'single' | 'double';
  /** Strikethrough text */
  /** Font color (hex without #, e.g., 'FF0000' for red) */
 * Cell fill/background options
export interface FillStyle {
  /** Fill pattern type */
  pattern?: FillPattern;
  /** Foreground color (hex without #) */
  fgColor?: string;
  /** Background color (hex without #) */
  bgColor?: string;
 * Border definition for a single side
export interface BorderSide {
  /** Border style */
  style?: BorderLineStyle;
  /** Border color (hex without #) */
 * Cell border options
export interface CellBorder {
  top?: BorderSide;
  bottom?: BorderSide;
  left?: BorderSide;
  right?: BorderSide;
  diagonal?: BorderSide & { up?: boolean; down?: boolean };
 * Cell alignment options
export interface AlignmentStyle {
  /** Horizontal alignment */
  horizontal?: HorizontalAlignment;
  /** Vertical alignment */
  vertical?: VerticalAlignment;
  /** Wrap text */
  /** Shrink to fit */
  shrinkToFit?: boolean;
  /** Text rotation in degrees (-90 to 90) */
  textRotation?: number;
  /** Indent level */
 * Complete cell style definition
export interface CellStyle {
  /** Font styling */
  font?: FontStyle;
  /** Fill/background styling */
  fill?: FillStyle;
  /** Border styling */
  border?: CellBorder;
  /** Alignment styling */
  alignment?: AlignmentStyle;
  /** Number format string (e.g., '#,##0.00', 'yyyy-mm-dd') */
  numFmt?: string;
 * Column definition for export
export interface ExportColumn {
  /** The field/property name in the data */
  /** Display name for the column header */
  /** Column width in characters (Excel only) */
  /** Data type hint for formatting */
  dataType?: ColumnDataType;
  /** Number format string (Excel only) */
  numberFormat?: string;
  /** Column-level style (applied to all cells in this column) */
  style?: CellStyle;
  /** Whether the column is hidden */
 * Formula definition for a cell
export interface CellFormula {
  /** Cell address (e.g., 'A1', 'B5') */
  cell: string;
  /** Formula string (without leading =) */
  formula: string;
  /** Optional result value for display before calculation */
  result?: unknown;
 * Conditional formatting rule
export interface ConditionalFormatRule {
  /** Cell range (e.g., 'A1:A100', 'B2:D50') */
  range: string;
  /** Rule type */
  type: 'cellIs' | 'containsText' | 'colorScale' | 'dataBar' | 'iconSet' | 'top10' | 'aboveAverage' | 'duplicateValues' | 'expression';
  /** Operator for cellIs type */
  operator?: 'equal' | 'notEqual' | 'greaterThan' | 'lessThan' | 'greaterThanOrEqual' | 'lessThanOrEqual' | 'between' | 'notBetween';
  /** Value(s) for comparison */
  value?: unknown;
  value2?: unknown;
  /** Text to search for (containsText type) */
  /** Style to apply when rule matches */
  /** Priority of the rule (lower = higher priority) */
 * Data validation rule
export interface DataValidationRule {
  /** Cell range to apply validation */
  /** Validation type */
  type: 'list' | 'whole' | 'decimal' | 'date' | 'time' | 'textLength' | 'custom';
  /** Operator for numeric/date/text validations */
  /** Formula or value for validation */
  formula1?: string | number | Date;
  /** Second formula/value for between operators */
  formula2?: string | number | Date;
  /** List of allowed values (for list type) */
  allowedValues?: string[];
  /** Show dropdown for list validation */
  showDropdown?: boolean;
  /** Allow blank cells */
  allowBlank?: boolean;
  /** Show error alert */
  showError?: boolean;
  errorTitle?: string;
  /** Error style */
  errorStyle?: 'stop' | 'warning' | 'information';
  /** Show input message */
  showInputMessage?: boolean;
  /** Input title */
  inputTitle?: string;
  /** Input message */
  inputMessage?: string;
 * Merged cell range definition
export interface MergedCellRange {
  /** Start row (1-based) */
  startRow: number;
  /** Start column (1-based) */
  startColumn: number;
  /** End row (1-based) */
  endRow: number;
  /** End column (1-based) */
  endColumn: number;
 * Image to embed in the worksheet
export interface EmbeddedImage {
  /** Image data as base64 string or Buffer */
  data: string | Buffer;
  /** Image type */
  type: 'png' | 'jpeg' | 'gif';
  /** Top-left cell position */
  position: {
    /** Column (1-based or letter) */
    col: number | string;
    /** Row (1-based) */
    row: number;
    /** Offset from left of cell in pixels */
    colOffset?: number;
    /** Offset from top of cell in pixels */
    rowOffset?: number;
  /** Image dimensions */
  size?: {
 * Row-level styling
export interface RowStyle {
  /** Row number (1-based) */
  /** Row height in points */
  /** Style to apply to entire row */
  /** Whether row is hidden */
  /** Outline level for grouping (0-7) */
  outlineLevel?: number;
 * Specific cell styling (overrides column/row styles)
export interface CellStyleOverride {
  /** Cell address (e.g., 'A1') or range (e.g., 'A1:B5') */
  /** Style to apply */
  style: CellStyle;
  /** Value to set (optional, overrides data) */
 * Page setup options for printing
export interface PageSetup {
  /** Paper size */
  paperSize?: 'letter' | 'legal' | 'tabloid' | 'a3' | 'a4' | 'a5';
  /** Page orientation */
  orientation?: 'portrait' | 'landscape';
  /** Fit to page width (number of pages) */
  fitToWidth?: number;
  /** Fit to page height (number of pages) */
  fitToHeight?: number;
  /** Scale percentage (10-400) */
  /** Print gridlines */
  showGridLines?: boolean;
  /** Print row/column headings */
  showRowColHeaders?: boolean;
  /** Margins in inches */
  margins?: {
    left?: number;
    right?: number;
    top?: number;
    bottom?: number;
    header?: number;
    footer?: number;
  /** Header text (use &L, &C, &R for left/center/right) */
  header?: string;
  /** Footer text */
  /** Rows to repeat at top (e.g., '1:2' for rows 1-2) */
  printTitlesRow?: string;
  /** Columns to repeat at left (e.g., 'A:B') */
  printTitlesColumn?: string;
 * Sheet protection options
export interface SheetProtection {
  /** Password for protection (will be hashed) */
  /** Allow selecting locked cells */
  selectLockedCells?: boolean;
  /** Allow selecting unlocked cells */
  selectUnlockedCells?: boolean;
  /** Allow formatting cells */
  formatCells?: boolean;
  /** Allow formatting columns */
  formatColumns?: boolean;
  /** Allow formatting rows */
  formatRows?: boolean;
  /** Allow inserting columns */
  insertColumns?: boolean;
  /** Allow inserting rows */
  insertRows?: boolean;
  /** Allow inserting hyperlinks */
  insertHyperlinks?: boolean;
  /** Allow deleting columns */
  deleteColumns?: boolean;
  /** Allow deleting rows */
  deleteRows?: boolean;
  /** Allow sorting */
  sort?: boolean;
  /** Allow auto filter */
  autoFilter?: boolean;
  /** Allow pivot tables */
  pivotTables?: boolean;
 * Sheet definition for multi-sheet exports
export interface SheetDefinition {
  /** Sheet name (required) */
  /** Data rows for this sheet */
  /** Column definitions (optional, derived from data if not provided) */
  /** Custom column headers (alternative to columns) */
  /** Column widths in characters */
  /** Include headers row */
  /** Header row style */
  headerStyle?: CellStyle;
  /** Default data row style */
  dataStyle?: CellStyle;
  /** Alternating row style */
  alternateRowStyle?: CellStyle;
  /** Specific row styles */
  rowStyles?: RowStyle[];
  /** Specific cell style overrides */
  cellStyles?: CellStyleOverride[];
  /** Formulas to add */
  formulas?: CellFormula[];
  /** Conditional formatting rules */
  conditionalFormatting?: ConditionalFormatRule[];
  /** Data validation rules */
  dataValidation?: DataValidationRule[];
  /** Merged cell ranges */
  mergedCells?: MergedCellRange[];
  /** Embedded images */
  images?: EmbeddedImage[];
  /** Freeze panes configuration */
  freeze?: {
    /** Freeze at row (1-based, rows above this are frozen) */
    row?: number;
    /** Freeze at column (1-based, columns left of this are frozen) */
  /** Auto-filter range (e.g., 'A1:D100' or true for all data) */
  autoFilter?: string | boolean;
  /** Tab color (hex without #) */
  tabColor?: string;
  /** Right-to-left reading order */
  rightToLeft?: boolean;
  /** Show gridlines */
  /** Page setup for printing */
  pageSetup?: PageSetup;
  /** Sheet protection options */
  protection?: SheetProtection;
  /** Default row height */
  defaultRowHeight?: number;
  /** Default column width */
  defaultColWidth?: number;
  /** Outline/grouping settings */
  outlineProperties?: {
    /** Summary rows below detail */
    summaryBelow?: boolean;
    /** Summary columns to right of detail */
    summaryRight?: boolean;
 * Row sampling options
export interface SamplingOptions {
  /** Sampling mode */
  mode: SamplingMode;
  /** Number of rows for top/bottom/random modes */
  /** Interval for every-nth mode */
 * Export styling options (Excel only) - simplified version for single-sheet exports
export interface ExportStyleOptions {
  /** Apply bold headers */
  boldHeaders?: boolean;
  /** Header background color (hex without #) */
  headerBackgroundColor?: string;
  /** Header text color (hex without #) */
  /** Apply alternating row colors */
  alternatingRowColors?: boolean;
  /** Alternate row background color (hex without #) */
  alternateRowColor?: string;
  /** Freeze header row */
  freezeHeader?: boolean;
  /** Apply auto-filter to headers */
  /** Full header style (overrides individual header options) */
  /** Full data style (applied to all data cells) */
  /** Full alternate row style (overrides alternateRowColor) */
 * Workbook metadata
export interface WorkbookMetadata {
  /** Author name */
  /** Title */
  /** Subject */
  subject?: string;
  /** Description/comments */
  /** Keywords (comma-separated or array) */
  keywords?: string | string[];
  /** Category */
  /** Company */
  company?: string;
  /** Manager */
  manager?: string;
 * CSV-specific options
export interface CSVOptions {
  /** Field delimiter (default: ',') */
  delimiter?: string;
  /** Line terminator (default: '\r\n') */
  lineTerminator?: string;
  /** Quote character (default: '"') */
  quoteChar?: string;
  /** Always quote fields */
  alwaysQuote?: boolean;
  /** Escape character for quotes within fields */
  escapeChar?: string;
  /** Include BOM for UTF-8 */
  includeBOM?: boolean;
  /** Encoding (default: 'utf-8') */
  encoding?: 'utf-8' | 'utf-16le' | 'utf-16be' | 'ascii' | 'latin1';
 * JSON-specific options
export interface JSONOptions {
  /** Pretty print with indentation */
  pretty?: boolean;
  /** Indentation size (default: 2) */
  /** Include metadata wrapper */
  /** Custom replacer function for JSON.stringify */
  replacer?: (key: string, value: unknown) => unknown;
  /** Date format for serialization */
  dateFormat?: 'iso' | 'timestamp' | 'locale';
 * Main export options
  format: ExportFormat;
  /** Columns to export (if not provided, derives from data) */
  /** Include column headers in output */
  sampling?: SamplingOptions;
  /** File name (without extension) */
  /** Sheet name (Excel only, for single-sheet export) */
  sheetName?: string;
  /** Styling options (Excel only, for single-sheet export) */
  styling?: ExportStyleOptions;
  /** Workbook metadata */
  /** @deprecated Use metadata.author instead */
  /** @deprecated Use metadata.title instead */
  // Multi-sheet support
  /** Multiple sheet definitions (Excel only) */
  sheets?: SheetDefinition[];
  // Global workbook options (Excel only)
  /** Default date format for all sheets */
  /** Default number format for all sheets */
  /** Calculate formulas on save */
  // Format-specific options
  /** CSV-specific options */
  csv?: CSVOptions;
  /** JSON-specific options */
  json?: JSONOptions;
 * Result of an export operation
  /** Whether the export succeeded */
  /** The exported data as a buffer/blob */
  data?: Uint8Array;
  /** MIME type of the exported data */
  /** Suggested file name with extension */
  /** Number of rows exported (total across all sheets) */
  /** Number of columns exported */
  columnCount?: number;
  /** Number of sheets (Excel only) */
  sheetCount?: number;
  /** Per-sheet statistics */
  sheetStats?: Array<{
 * Default export options
export const DEFAULT_EXPORT_OPTIONS: Partial<ExportOptions> = {
  fileName: 'export',
  sheetName: 'Sheet1',
  styling: {
    boldHeaders: true,
    freezeHeader: true,
    autoFilter: true
 * Common cell styles for convenience
export const CommonStyles = {
  bold: { font: { bold: true } } as CellStyle,
  italic: { font: { italic: true } } as CellStyle,
  /** Red text */
  redText: { font: { color: 'FF0000' } } as CellStyle,
  /** Green text */
  greenText: { font: { color: '008000' } } as CellStyle,
  /** Blue text */
  blueText: { font: { color: '0000FF' } } as CellStyle,
  /** Yellow highlight */
  yellowHighlight: { fill: { pattern: 'solid', fgColor: 'FFFF00' } } as CellStyle,
  /** Light gray background */
  lightGrayBg: { fill: { pattern: 'solid', fgColor: 'F5F5F5' } } as CellStyle,
  /** Centered text */
  centered: { alignment: { horizontal: 'center', vertical: 'middle' } } as CellStyle,
  /** Right-aligned text */
  rightAligned: { alignment: { horizontal: 'right' } } as CellStyle,
  /** Wrapped text */
  wrapped: { alignment: { wrapText: true } } as CellStyle,
  /** Currency format */
  currency: { numFmt: '$#,##0.00' } as CellStyle,
  /** Percentage format */
  percentage: { numFmt: '0.00%' } as CellStyle,
  /** Date format */
  date: { numFmt: 'yyyy-mm-dd' } as CellStyle,
  /** DateTime format */
  dateTime: { numFmt: 'yyyy-mm-dd hh:mm:ss' } as CellStyle,
  /** Thin border all around */
  thinBorder: {
      top: { style: 'thin' as BorderLineStyle, color: '000000' },
      bottom: { style: 'thin' as BorderLineStyle, color: '000000' },
      left: { style: 'thin' as BorderLineStyle, color: '000000' },
      right: { style: 'thin' as BorderLineStyle, color: '000000' }
  } as CellStyle,
  /** Header style (bold, centered, with background) */
    font: { bold: true, color: 'FFFFFF' },
    fill: { pattern: 'solid' as FillPattern, fgColor: '4472C4' },
    alignment: { horizontal: 'center' as HorizontalAlignment, vertical: 'middle' as VerticalAlignment },
      bottom: { style: 'thin' as BorderLineStyle, color: '000000' }
  } as CellStyle
 * Helper function to merge cell styles
export function mergeCellStyles(...styles: (CellStyle | undefined)[]): CellStyle {
  for (const style of styles) {
    if (!style) continue;
      result.font = { ...(result.font || {}), ...style.font };
      result.fill = { ...(result.fill || {}), ...style.fill };
      result.border = { ...(result.border || {}), ...style.border };
      result.alignment = { ...(result.alignment || {}), ...style.alignment };
      result.numFmt = style.numFmt;
import { AggregateExpression, DatabaseProviderBase, UserInfo } from '@memberjunction/core';
import { GraphQLSchema } from 'graphql';
export type UserPayload = {
  userRecord: any;
  isSystemUser?: boolean;
  /** ID of the MJ API key used for authentication (when using mj_sk_* format keys) */
  /** SHA-256 hash of the MJ API key (used for scope authorization) */
 * AppContext is the context object that is passed to all resolvers.
export type AppContext = {
   * The default and backwards compatible connection pool.
  userPayload: UserPayload;
  queryRunner?: sql.Request;
   * Array of connection pools that have additional information about their intended use e.g. Admin, Read-Write, Read-Only.
  dataSources: DataSourceInfo[];
   * Per-request DatabaseProviderBase instances  
  providers: Array<ProviderInfo>;
export class ProviderInfo {
  provider: DatabaseProviderBase;
  type: 'Admin' | 'Read-Write' | 'Read-Only' | 'Other';
export class DataSourceInfo  {
  constructor(init: {dataSource: sql.ConnectionPool, type: "Admin" | "Read-Write" | "Read-Only" | "Other", host: string, port: number, database: string, userName: string} ) {
export type DirectiveBuilder = {
  typeDefs: string;
  transformer: (schema: GraphQLSchema) => GraphQLSchema;
export type RunViewGenericParams = {
  viewInfo: UserViewEntityExtended;
  extraFilter: string;
  orderBy: string;
  userSearchString: string;
  excludeUserViewRunID?: string;
  overrideExcludeFilter?: string;
  saveViewResults?: boolean;
  ignoreMaxRows?: boolean;
  startRow?: number;
  excludeDataFromAllPriorViewRuns?: boolean;
  forceAuditLog?: boolean;
  auditLogDescription?: string;
  resultType?: string;
  userPayload?: UserPayload;
  aggregates?: AggregateExpression[];
export class MJServerEvent {
  type: 'setupComplete' | 'requestReceived' | 'requestCompleted' | 'requestFailed';
  systemUser: UserInfo;
export const MJ_SERVER_EVENT_CODE = 'MJ_SERVER_EVENT';
export async function raiseEvent(type: MJServerEvent['type'], dataSources: DataSourceInfo[], userPayload: UserPayload, component?: any) {
  const event = new MJServerEvent();
  event.dataSources = dataSources;
  event.userPayload = userPayload;
  event.systemUser = await getSystemUser();
  const mje = new MJEvent();
  mje.args = event;
  mje.component = component;
  mje.event = MJEventType.ComponentEvent;
  mje.eventCode = MJ_SERVER_EVENT_CODE;
  MJGlobal.Instance.RaiseEvent(mje);
 * @fileoverview Type definitions for the unified ComponentManager
import { ComponentObject } from '../types';
 * Resolution mode for specs
export type ResolutionMode = 'embed' | 'preserve-metadata';
 * Options for loading a component
export interface LoadOptions {
   * User context for database operations and registry fetching
   * Force re-fetch from registry even if cached
   * Force recompilation even if compiled version exists
  forceRecompile?: boolean;
   * Whether this is a dependent component (for tracking)
  isDependent?: boolean;
   * What to return from the load operation
  returnType?: 'component' | 'spec' | 'both';
   * Enable registry usage tracking for licensing (default: true)
  trackUsage?: boolean;
   * Namespace to use if not specified in spec
  defaultNamespace?: string;
   * Version to use if not specified in spec
  defaultVersion?: string;
   * How to format resolved specs:
   * - 'preserve-metadata': Keep registry/namespace/name intact (for UI display)
   * - 'embed': Convert to embedded format (for test harness)
   * @default 'preserve-metadata'
  resolutionMode?: ResolutionMode;
   * All available component libraries (for browser context where ComponentMetadataEngine isn't available)
  allLibraries?: MJComponentLibraryEntity[];
 * Result of loading a single component
   * Whether the load operation succeeded
   * The compiled component object
  component?: ComponentObject;
   * The fully resolved component specification
  spec?: ComponentSpec;
   * Whether the component was loaded from cache
  fromCache: boolean;
   * Any errors that occurred during loading
  errors?: Array<{
    phase: 'fetch' | 'compile' | 'register' | 'dependency';
   * Components that were loaded as dependencies
  dependencies?: Record<string, ComponentObject>;
 * Result of loading a component hierarchy
export interface HierarchyResult {
   * Whether the entire hierarchy loaded successfully
   * The root component object
  rootComponent?: ComponentObject;
   * The fully resolved root specification
  resolvedSpec?: ComponentSpec;
   * List of all component names that were loaded
  loadedComponents: string[];
   * Map of all loaded components by name
  components?: Record<string, ComponentObject>;
   * Number of components loaded from cache vs fetched
  stats?: {
    fromCache: number;
    fetched: number;
    compiled: number;
 * Configuration for ComponentManager
export interface ComponentManagerConfig {
   * Enable debug logging
   * Maximum cache size for fetched specs
   * Cache TTL in milliseconds (default: 1 hour)
  cacheTTL?: number;
   * Whether to track registry usage for licensing
  enableUsageTracking?: boolean;
   * Batch size for parallel dependency loading
  dependencyBatchSize?: number;
   * Timeout for registry fetch operations (ms)
  fetchTimeout?: number;
 * Internal cache entry for fetched specs
export interface CacheEntry {
  fetchedAt: Date;
  usageNotified: boolean;
 * @fileoverview Type definitions for SQL Server Data Provider
 * This module contains all the types, interfaces, and configuration classes
 * used by the SQL Server Data Provider.
 * @module @memberjunction/sqlserver-dataprovider/types
import { ProviderConfigDataBase, UserInfo } from '@memberjunction/core';
  /** Optional description for this SQL operation */
  /** Whether this is a data mutation operation (INSERT/UPDATE/DELETE) */
  /** Simple SQL fallback for loggers with logRecordChangeMetadata=false (only for Save/Delete operations) */
   * Optional connection source to use instead of the default pool.
   * Used by IS-A chain orchestration to execute SPs on a shared transaction.
   * Should be a sql.Transaction or sql.ConnectionPool instance.
  connectionSource?: sql.ConnectionPool | sql.Transaction;
 * Context for SQL execution containing all necessary resources
export interface SQLExecutionContext {
  /** The connection pool to use for queries */
  pool: sql.ConnectionPool;
  /** Optional transaction if one is active */
  transaction?: sql.Transaction | null;
  /** Function to log SQL statements */
  logSqlStatement?: (
    ignoreLogging?: boolean,
    isMutation?: boolean,
  ) => Promise<void>;
  /** Function to clear transaction reference on EREQINPROG */
  clearTransaction?: () => void;
 * Options for internal SQL execution
export interface InternalSQLOptions {
  /** If true, this statement will not be logged */
  /** Whether this is a data mutation operation */
  /** Simple SQL fallback for loggers */
  /** User context for logging */
 * Configuration options for batch SQL execution
export interface ExecuteSQLBatchOptions {
  /** Optional description for this batch operation */
  /** If true, this batch will not be logged to any logging session */
  /** Whether this batch contains data mutation operations */
 * Configuration data specific to SQL Server provider
export class SQLServerProviderConfigData extends ProviderConfigDataBase<SQLServerProviderConfigOptions> {
   * Gets the SQL Server connection pool configuration
  get ConnectionPool(): sql.ConnectionPool {
    return this.Data.ConnectionPool;
   * Gets the interval in seconds for checking metadata refresh
  get CheckRefreshIntervalSeconds(): number {
    return this.Data.CheckRefreshIntervalSeconds;
    connectionPool: sql.ConnectionPool,
    checkRefreshIntervalSeconds: number = 0 /*default to disabling auto refresh */,
    ignoreExistingMetadata: boolean = true
        ConnectionPool: connectionPool,
        CheckRefreshIntervalSeconds: checkRefreshIntervalSeconds,
      excludeSchemas,
      ignoreExistingMetadata
export interface SQLServerProviderConfigOptions {
  ConnectionPool: sql.ConnectionPool;
  CheckRefreshIntervalSeconds: number;
 * Configuration options for SQL logging sessions
export interface SqlLoggingOptions {
  /** Whether to format output as a flyway migration file with schema placeholders */
   * Optional default schema name to use for Flyway migrations for replacing schema names with 
   * the placeholder ${flyway:defaultSchema}
  /** Optional description to include as a comment at the start of the log */
  /** Which types of statements to log: 'queries' (all), 'mutations' (only data changes), 'both' (default) */
  /** Optional batch separator to emit after each statement (e.g., "GO" for SQL Server) */
  /** Whether to pretty print SQL statements with proper formatting */
  /** Whether to log record change metadata wrapper SQL (default: false). When false, only core spCreate/spUpdate/spDelete calls are logged */
  /** Whether to retain log files that contain no SQL statements (default: false). When false, empty log files are automatically deleted on dispose */
  /** Optional user ID to filter SQL logging - only log SQL executed by this user */
  /** Optional friendly name for this logging session (for UI display) */
   * Supports both regex (RegExp objects) and simple wildcard patterns (strings).
   * How these patterns are applied depends on filterType.
   * String patterns support:
   * - Simple wildcards: "*AIPrompt*", "spCreate*", "*Run"
   * - Regex strings: "/spCreate.*Run/i", "/^SELECT.*FROM/i"
   * RegExp examples:
   * - /spCreateAIPromptRun/i - Match stored procedure calls
   * - /^SELECT.*FROM.*vw.*Metadata/i - Match metadata view queries
   * - /INSERT INTO EntityFieldValue/i - Match specific inserts
  filterPatterns?: (string | RegExp)[];
   * Note: If filterPatterns is empty/undefined, all SQL is logged regardless of filterType.
 * Interface for SQL logging session with disposable pattern
export interface SqlLoggingSession {
  /** Unique session ID */
  readonly id: string;
  /** File path where SQL is being logged */
  readonly filePath: string;
  /** Session start time */
  readonly startTime: Date;
  /** Number of statements logged so far */
  readonly statementCount: number;
  /** Configuration options for this session */
  readonly options: SqlLoggingOptions;
  /** Dispose method to stop logging and clean up resources */
  dispose(): Promise<void>;
 * @fileoverview Core type definitions for the Scheduled Jobs system
 * Result returned by a scheduled job execution
export interface ScheduledJobResult {
     * Whether the job completed successfully
     * Error message if the job failed
     * Job-type specific execution details
     * For Agents: { AgentRunID: string, TokensUsed: number, Cost: number }
     * For Actions: { ResultCode: string, IsSuccess: boolean, OutputParams: any }
    Details?: Record<string, any>;
 * Configuration for job-specific execution
 * Each job type defines its own configuration schema
export interface ScheduledJobConfiguration {
 * Configuration for Agent scheduled jobs
export interface AgentJobConfiguration extends ScheduledJobConfiguration {
    StartingPayload?: any;
    InitialMessage?: string;
 * Configuration for Action scheduled jobs
export interface ActionJobConfiguration extends ScheduledJobConfiguration {
    Params?: Array<{
        ValueType: 'Static' | 'SQL Statement';
 * Notification content structure
export interface NotificationContent {
    Subject: string;
    Priority: 'Low' | 'Normal' | 'High';
    Metadata?: Record<string, any>;
 * Job execution status values
export type ScheduledJobRunStatus = 'Running' | 'Completed' | 'Failed' | 'Cancelled' | 'Timeout';
 * Schedule status values
export type ScheduledJobStatus = 'Pending' | 'Active' | 'Paused' | 'Disabled' | 'Expired';
 * Notification delivery channels
export type NotificationChannel = 'Email' | 'InApp';
 * Contains the results of a call to render a template
export class TemplateRenderResult {
    Output: string;
     * Optional, typically used only for Success=false
 * @fileoverview CLI-specific types and interfaces
 * @module @memberjunction/testing-cli
 * Output format options for CLI commands
export type OutputFormat = 'console' | 'json' | 'markdown';
 * CLI configuration loaded from mj.config.cjs
export interface CLIConfig {
    defaultEnvironment?: string;
    defaultFormat?: OutputFormat;
    failFast?: boolean;
    maxParallelTests?: number;
    database?: {
        host?: string;
        schema?: string;
 * Common flags shared across commands
export interface CommonFlags {
    format?: OutputFormat;
 * Flags for run command
export interface RunFlags extends CommonFlags {
    suite?: string;
    difficulty?: string;
    all?: boolean;
     * Variable values to pass to the test (format: name=value)
     * Can be specified multiple times for multiple variables
    var?: string[];
 * Flags for suite command
export interface SuiteFlags extends CommonFlags {
    sequence?: string;
     * Variable values to pass to all tests in the suite (format: name=value)
 * Flags for list command
export interface ListFlags extends CommonFlags {
    suites?: boolean;
    types?: boolean;
     * Show available variables for the test/test type
    showVariables?: boolean;
 * Flags for validate command
export interface ValidateFlags extends CommonFlags {
    saveReport?: boolean;
 * Flags for report command
export interface ReportFlags extends CommonFlags {
    test?: string;
    includeCosts?: boolean;
    includeTrends?: boolean;
 * Flags for history command
export interface HistoryFlags extends CommonFlags {
    recent?: number;
 * Flags for compare command
export interface CompareFlags extends CommonFlags {
    version?: string[];
    commit?: string[];
    diffOnly?: boolean;
 * Core type definitions for the Testing Engine
 * Note: UI-safe types are defined in @memberjunction/testing-engine-base
 * This file contains execution-specific types that depend on engine internals
  MJAIAgentRunEntity
import { IOracle } from './oracles/IOracle';
// Re-export all types from EngineBase for convenience
  TestLogMessage,
  TestProgress,
  TestRunOptions,
  SuiteRunOptions,
  OracleResult,
  TestRunResult,
  TestSuiteRunResult,
  ScoringWeights,
  RunContextDetails,
  OracleConfig,
  // Variable system types
  TestVariableDataType,
  TestVariableValueSource,
  TestVariablePossibleValue,
  TestVariableDefinition,
  TestTypeVariablesSchema,
  TestVariableOverride,
  TestVariablesConfig,
  TestSuiteVariablesConfig,
  ResolvedTestVariables,
  TestVariableValue
} from '@memberjunction/testing-engine-base';
// Import types we need for local interfaces
  ResolvedTestVariables
 * Context for test driver execution
export interface DriverExecutionContext {
   * Test definition
  test: MJTestEntity;
   * Test run entity (for bidirectional linking)
  testRun: MJTestRunEntity;
   * User context for data access
   * Runtime options
  options: TestRunOptions;
   * Oracle registry for evaluations
  oracleRegistry: Map<string, IOracle>;
   * Resolved variable values for this execution.
   * Variables have been resolved through the hierarchy and validated.
   * May be undefined if no variables are defined for this test type.
  resolvedVariables?: ResolvedTestVariables;
 * Result from a single turn in multi-turn test
export interface TurnResult {
   * Turn number (1-indexed)
  turnNumber: number;
   * Agent run for this turn
  agentRun: MJAIAgentRunEntity;
   * Input payload for this turn
  inputPayload?: Record<string, unknown>;
   * Output payload from this turn
  outputPayload: Record<string, unknown>;
   * Oracle results for this turn (if per-turn evaluation)
  oracleResults?: OracleResult[];
   * Duration in milliseconds
   * Cost in USD
 * Result from driver execution
export interface DriverExecutionResult {
   * Optional sub-category or variant label for the test target.
   * Use for ad-hoc labeling or to distinguish test scenarios within the same entity type.
   * Examples: "Summarization", "Classification", "Code Review", "Multi-turn Chat"
   * Entity ID identifying the type of target being tested.
   * References Entity.ID (e.g., Entity ID for "MJ: AI Agent Runs").
   * This is the proper FK reference for entity linkage.
  targetLogEntityId?: string;
   * Target entity ID (final AgentRun ID for single/multi-turn)
   * Execution status
  status: 'Passed' | 'Failed' | 'Error' | 'Timeout';
   * Overall score
   * Oracle results
  oracleResults: OracleResult[];
   * Number of checks that passed
   * Number of checks that failed
  failedChecks: number;
   * Total number of checks
   * Input data used
  inputData?: unknown;
   * Expected output data
  expectedOutput?: unknown;
   * Actual output data
  actualOutput?: unknown;
   * Error message if status is Error
   * Multi-turn specific: Total number of turns
  totalTurns?: number;
   * Multi-turn specific: Results for each turn
  turnResults?: TurnResult[];
   * Multi-turn specific: All AgentRun IDs
  allAgentRunIds?: string[];
 * Oracle evaluation input
export interface OracleInput {
   * The test being evaluated
   * Expected output from test definition
   * Actual output from execution
   * Target entity (e.g., AgentRun)
  targetEntity?: unknown;
   * User context
 * Core type definitions for the Testing Framework
 * These types are UI-safe and do not depend on execution logic
 * Log message from test execution
export interface TestLogMessage {
   * Timestamp when the message was logged
   * Log level
  level: 'info' | 'warn' | 'error' | 'debug';
   * Log message content
   * Optional metadata for additional context
 * Progress callback for test execution
export interface TestProgress {
   * Current execution step
   * Progress percentage (0-100)
   * Human-readable message
   * Additional metadata
    testRun?: unknown;
    oracleType?: string;
 * Variables to pass to a test/suite run
export interface TestRunVariables {
    [variableName: string]: string | number | boolean | Date;
 * Options for running a single test
export interface TestRunOptions {
   * Verbose logging
   * Validate configuration without executing
   * Environment context (dev, staging, prod)
   * Git commit SHA for versioning
  gitCommit?: string;
   * Agent/system version being tested
  agentVersion?: string;
   * Override test configuration
  configOverride?: Record<string, unknown>;
   * Progress callback for real-time updates
  progressCallback?: (progress: TestProgress) => void;
   * Log callback for streaming execution details to the test run log
  logCallback?: (message: TestLogMessage) => void;
   * Tags to apply to the test run (JSON string array)
  tags?: string;
   * Variable values to use for this run.
   * These values take highest priority in the resolution order:
   * run > suite > test > type
  variables?: TestRunVariables;
 * Options for running a test suite
export interface SuiteRunOptions extends TestRunOptions {
   * Run tests in parallel
   * Stop on first failure
   * Maximum parallel tests (if parallel=true)
  maxParallel?: number;
   * Run only specific sequence numbers (e.g., [1, 3, 5] runs tests at those positions)
  sequence?: number[];
 * Oracle evaluation result
   * Oracle type that produced this result
   * Whether the oracle check passed
   * Numeric score (0.0 to 1.0)
   * Additional details (oracle-specific)
  details?: unknown;
 * Result from running a single test
export interface TestRunResult {
   * Test Run ID
   * Test ID
   * Test name (from lookup field)
   * Test execution status
  status: 'Passed' | 'Failed' | 'Skipped' | 'Error' | 'Timeout';
   * Overall score (0.0000 to 1.0000)
   * Oracle evaluation results
   * Target entity ID (e.g., AIAgentRun.ID)
   * When execution started
   * When execution completed
  completedAt: Date;
   * Iteration number for repeated tests (when RepeatCount > 1)
   * Resolved variables that were used for this test run
export interface TestSuiteRunResult {
   * Suite Run ID
   * Suite ID
   * Suite name (from lookup field)
   * Suite execution status
  status: 'Completed' | 'Failed' | 'Cancelled' | 'Pending' | 'Running';
   * Tests that passed
   * Tests that failed
   * Total tests
   * Average score across all tests
   * Individual test results
   * Total duration in milliseconds
   * Total cost in USD
   * Resolved variables that were provided at suite run level
 * Scoring weights for different evaluation dimensions
export interface ScoringWeights {
   * Weight for each oracle type
   * Keys are oracle types, values are weights (should sum to 1.0)
  [oracleType: string]: number;
 * Validation result for test configuration
   * Whether validation passed
   * Validation errors (blocking issues)
   * Validation warnings (non-blocking issues)
 * Validation error
   * Error category
  category: 'configuration' | 'input' | 'expected-outcome';
   * Field path (if applicable)
   * Suggested fix
 * Validation warning
   * Warning category
  category: 'best-practice' | 'performance' | 'cost';
   * Warning message
   * Recommendation
  recommendation?: string;
 * Execution context details for test runs.
 * Stored as JSON in the RunContextDetails field of TestRun and TestSuiteRun entities.
 * Enables cross-server aggregation and detailed environment tracking.
export interface RunContextDetails {
   * Operating system type (e.g., "darwin", "linux", "win32")
  osType?: string;
   * Operating system version/release
  osVersion?: string;
   * Node.js version used to run the tests
  nodeVersion?: string;
   * Timezone identifier (e.g., "America/New_York", "UTC")
   * System locale (e.g., "en-US", "fr-FR")
   * IP address of the machine running tests (useful for network debugging)
   * CI/CD provider name (e.g., "GitHub Actions", "Azure DevOps", "Jenkins")
   * CI/CD pipeline or workflow ID
   * Build number or run number from CI/CD
   * Git branch name
   * Pull request number (if applicable)
   * Additional custom properties for extensibility
 * Oracle configuration (can have any additional properties)
export interface OracleConfig {
   * Oracle-specific configuration properties
// TEST VARIABLES SYSTEM
 * Data types supported for test variables
export type TestVariableDataType = 'string' | 'number' | 'boolean' | 'date';
 * How the valid values for a variable are determined
export type TestVariableValueSource =
  | 'static'    // Hardcoded list in possibleValues
  | 'freeform'; // Any value of the given dataType
  // Future: | 'entity'   // Pull from MJ entity (e.g., AI Configurations)
 * A possible value for a static variable
export interface TestVariablePossibleValue {
   * The actual value
   * Display label (defaults to value.toString() if not provided)
   * Optional description of what this value means
 * Definition of a single test variable.
 * Stored in TestType.VariablesSchema.variables array.
export interface TestVariableDefinition {
   * Unique name for the variable (e.g., "AIConfiguration", "Temperature")
   * Human-readable display name
   * Description of what this variable controls
   * Data type of the variable value
  dataType: TestVariableDataType;
   * How valid values are determined
  valueSource: TestVariableValueSource;
   * For static valueSource: list of valid values
   * Each entry has a value and optional display label
  possibleValues?: TestVariablePossibleValue[];
   * Default value (must match dataType)
  defaultValue?: string | number | boolean | Date;
   * Whether this variable must have a value to run the test
 * Variables schema for a TestType.
 * Stored in TestType.VariablesSchema JSON column.
export interface TestTypeVariablesSchema {
   * Version of the schema format (for future migrations)
  schemaVersion: '1.0';
   * Variables available for tests of this type
  variables: TestVariableDefinition[];
 * Override settings for a variable at the test level
export interface TestVariableOverride {
   * Whether this variable is exposed for this test.
   * If false, the variable is not available for override.
  exposed: boolean;
   * Override the default value for this test
   * If true, this variable cannot be overridden at suite/run level
  locked?: boolean;
   * Restrict possible values to a subset of the type's values
  restrictedValues?: (string | number | boolean)[];
 * Variable configuration for a specific Test.
 * Stored in Test.Variables JSON column.
export interface TestVariablesConfig {
   * Variables exposed by this test (subset of type's variables).
   * Key is the variable name from TestType.
    [variableName: string]: TestVariableOverride;
 * Variable values for a TestSuite.
 * Stored in TestSuite.Variables JSON column.
export interface TestSuiteVariablesConfig {
   * Key is the variable name.
 * Resolved variables with metadata.
 * Used during test execution and stored in TestRun.ResolvedVariables.
export interface ResolvedTestVariables {
   * The resolved values
   * Source of each resolved value (for debugging/auditing)
  sources: {
    [variableName: string]: 'run' | 'suite' | 'test' | 'type';
 * Variable value type union
export type TestVariableValue = string | number | boolean | Date;
import { CompositeKey, EntityInfo, EntityRelationshipInfo } from '@memberjunction/core';
// Label types
 * The breadth of a version label.
 * - System: captures all tracked entities
 * - Entity: captures all records of one entity type
 * - Record: captures one record and its dependency graph
export type VersionLabelScope = 'System' | 'Entity' | 'Record';
 * Lifecycle state of a version label.
 * - Active: current / usable
 * - Archived: kept for reference, not usable for restore
 * - Restored: this label was the target of a restore operation
export type VersionLabelStatus = 'Active' | 'Archived' | 'Restored';
 * Parameters for creating a new version label.
export interface CreateLabelParams {
    /** Scope of the label (default: Record) */
    Scope?: VersionLabelScope;
    /** When scope is Entity or Record, the target entity name */
    /** When scope is Record, the target record key */
    RecordKey?: CompositeKey;
    /** Optional parent label ID for grouping related labels */
    /** Optional external system reference (git SHA, release tag, etc.) */
     * Whether to include dependent records when scope is Record.
     * If true, walks the dependency graph and captures child/grandchild records.
     * Maximum depth of dependency graph traversal.
     * Only relevant when IncludeDependencies is true.
     * Default: 10
     * Entity names to exclude from dependency graph traversal.
     * Optional callback invoked during label creation to report progress.
     * Useful for long-running operations to display status in the UI.
    OnProgress?: CreateLabelProgressCallback;
 * Filter options for querying version labels.
export interface LabelFilter {
    /** Filter by scope */
    /** Filter by status */
    Status?: VersionLabelStatus;
    /** Filter by entity name (for Entity/Record scope labels) */
    /** Filter by record ID */
    /** Filter by creator user ID */
    /** Filter by name (partial match) */
    NameContains?: string;
    /** Maximum number of results */
    /** Order by field */
// Snapshot types
 * Result of a snapshot capture operation.
export interface CaptureResult {
    /** Whether the capture succeeded */
    /** The label that was created/populated */
    LabelID: string;
    /** Number of records captured */
    ItemsCaptured: number;
    /** Number of synthetic snapshots created (for records with no prior RecordChange) */
    SyntheticSnapshotsCreated: number;
    /** Any errors encountered during capture */
    Errors: CaptureError[];
export interface CaptureError {
 * Represents a record's state at a specific point in time.
export interface RecordSnapshot {
    /** Entity ID */
    /** Record primary key as concatenated string */
    /** The RecordChange ID this snapshot came from */
    /** When the snapshot was taken */
    /** Full record data as parsed JSON */
    FullRecordJSON: Record<string, unknown>;
// Dependency graph types
 * A node in a record dependency graph.
    /** Entity metadata */
    /** The record's primary key */
    RecordKey: CompositeKey;
    /** The record's primary key as a concatenated string */
    /** Current field values of the record */
    RecordData: Record<string, unknown>;
    /** Relationship from parent to this node (null for root) */
    Relationship: EntityRelationshipInfo | null;
    /** Child/dependent record nodes */
    Children: DependencyNode[];
    /** Depth in the graph (0 = root) */
 * Options for walking the dependency graph.
export interface WalkOptions {
    /** Maximum recursion depth (default: 10) */
    /** Only include these entities */
    EntityFilter?: string[];
    /** Skip these entities */
    /** Include soft-deleted records (default: false) */
    IncludeDeleted?: boolean;
// Diff types
 * The result of comparing two labels or a label to the current state.
export interface DiffResult {
    /** The starting label */
    FromLabelID: string;
    FromLabelName: string;
    /** The ending label (null if comparing to current state) */
    ToLabelID: string | null;
    ToLabelName: string | null;
    /** Summary statistics */
    Summary: DiffSummary;
    /** Changes grouped by entity */
    EntityDiffs: EntityDiffGroup[];
export interface DiffSummary {
    TotalRecordsChanged: number;
    TotalRecordsAdded: number;
    TotalRecordsDeleted: number;
    TotalRecordsModified: number;
    EntitiesAffected: number;
 * Changes within a single entity type.
export interface EntityDiffGroup {
    Records: RecordDiff[];
    /** Counts for this entity */
    DeletedCount: number;
 * The difference for a single record between two labels.
export interface RecordDiff {
    DiffType: RecordDiffType;
    /** Field-level changes (only populated for Modified records) */
    FieldChanges: FieldChange[];
    /** Snapshot from the "from" label (null for Added records) */
    FromSnapshot: Record<string, unknown> | null;
    /** Snapshot from the "to" label or current state (null for Deleted records) */
    ToSnapshot: Record<string, unknown> | null;
export type RecordDiffType = 'Added' | 'Modified' | 'Deleted' | 'Unchanged';
 * A single field-level change between two snapshots.
export interface FieldChange {
    OldValue: unknown;
    NewValue: unknown;
// Progress callback types
 * Step in the label creation lifecycle, reported via progress callbacks.
export type CreateLabelStep = 'initializing' | 'walking_dependencies' | 'capturing_snapshots' | 'finalizing';
 * Progress update emitted during label creation.
 * Consumers can use this to display a progress bar and status messages.
export interface CreateLabelProgressUpdate {
    Step: CreateLabelStep;
    /** Number of records processed so far (only during capturing_snapshots) */
    /** Total records to process (only during capturing_snapshots) */
    /** Entity currently being processed (only during capturing_snapshots) */
 * Callback invoked during label creation to report progress.
export type CreateLabelProgressCallback = (progress: CreateLabelProgressUpdate) => void;
// Restore types
 * Status of a restore operation.
export type RestoreStatus = 'Pending' | 'In Progress' | 'Complete' | 'Error' | 'Partial';
 * Options for restoring to a version label.
export interface RestoreOptions {
    /** Preview only, don't actually apply changes (default: false) */
    DryRun?: boolean;
    /** Restore all items or only selected records */
    Scope?: 'Full' | 'Selected';
    /** When Scope is Selected, the records to restore */
    SelectedRecords?: Array<{ EntityName: string; RecordID: string }>;
    /** Entities to exclude from restore */
    SkipEntities?: string[];
    /** Auto-create a "before restore" label as safety net (default: true) */
    CreatePreRestoreLabel?: boolean;
 * The result of a restore operation.
export interface RestoreResult {
    /** The restore audit record ID */
    RestoreID: string;
    /** The safety-net label created before restore (if CreatePreRestoreLabel was true) */
    /** Overall status */
    Status: RestoreStatus;
    /** Number of records successfully restored */
    RestoredCount: number;
    /** Number of records that failed */
    /** Number of records skipped (e.g. already at target state) */
    SkippedCount: number;
    /** Per-record details */
    Details: RestoreItemResult[];
 * Result for restoring a single record.
export interface RestoreItemResult {
    Status: 'Restored' | 'Failed' | 'Skipped';
export const ELECTRON_ORG = 'electron';
export const ELECTRON_REPO = 'electron';
export const NIGHTLY_REPO = 'nightlies';
export type ElectronReleaseRepo = 'electron' | 'nightlies';
export type VersionBumpType = 'nightly' | 'alpha' | 'beta' | 'minor' | 'stable';
export const isVersionBumpType = (s: string): s is VersionBumpType => {
  return ['nightly', 'alpha', 'beta', 'minor', 'stable'].includes(s);
export type PackageJson = {
  dependencies?: Record<string, string>
  devDependencies?: Record<string, string>
  peerDependencies?: Record<string, string>
  optionalDependencies?: Record<string, string>
  workspaces?: string[] | { packages: string[] }
export interface NodeModuleInfo {
  dir: string
  dependencies?: Array<NodeModuleInfo>
export type ParsedDependencyTree = {
  readonly name: string
  readonly version: string
  readonly path: string
  readonly workspaces?: string[] | { packages: string[] } // we only use this at root level
// Note: `PnpmDependency` and `NpmDependency` include the output of `JSON.parse(...)` of `pnpm list` and `npm list` respectively
// This object has a TON of info - a majority, if not all, of each dependency's package.json
// We extract only what we need when constructing DependencyTree in `extractProductionDependencyTree`
export interface PnpmDependency extends Dependency<PnpmDependency, PnpmDependency> {
  readonly from: string
  readonly resolved: string
  // Present in pnpm 10.29.3+ `pnpm list --json` output for deduped references.
  // Context: https://github.com/pnpm/pnpm/issues/10601
  readonly dedupedDependenciesCount?: number
export interface NpmDependency extends Dependency<NpmDependency, string> {
  readonly resolved?: string
  // implicit dependencies, returned only through `npm list`
  readonly _dependencies?: {
    [packageName: string]: string
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface TraversedDependency extends Dependency<TraversedDependency, TraversedDependency> {}
export type Dependency<T, V> = Dependencies<T, V> & ParsedDependencyTree
export type Dependencies<T, V> = {
  readonly dependencies?: {
    [packageName: string]: T
  readonly optionalDependencies?: {
    [packageName: string]: V
  [packageNameAndVersion: string]: {
    readonly dependencies: string[]
import { CancellationToken, PackageFileInfo, ProgressInfo, UpdateFileInfo, UpdateInfo } from "builder-util-runtime"
import { EventEmitter } from "events"
import { URL } from "url"
import { LoginCallback } from "./electronHttpExecutor"
export { CancellationToken, PackageFileInfo, ProgressInfo, UpdateFileInfo, UpdateInfo }
export const DOWNLOAD_PROGRESS = "download-progress"
export const UPDATE_DOWNLOADED = "update-downloaded"
export interface Logger {
  info(message?: any): void
  warn(message?: any): void
  error(message?: any): void
  debug?(message: string): void
export class UpdaterSignal {
  constructor(private emitter: EventEmitter) {}
   * Emitted when an authenticating proxy is [asking for user credentials](https://github.com/electron/electron/blob/master/docs/api/client-request.md#event-login).
  login(handler: LoginHandler): void {
    addHandler(this.emitter, "login", handler)
  progress(handler: (info: ProgressInfo) => void): void {
    addHandler(this.emitter, DOWNLOAD_PROGRESS, handler)
  updateDownloaded(handler: (info: UpdateDownloadedEvent) => void): void {
    addHandler(this.emitter, UPDATE_DOWNLOADED, handler)
  updateCancelled(handler: (info: UpdateInfo) => void): void {
    addHandler(this.emitter, "update-cancelled", handler)
const isLogEvent = false
export function addHandler(emitter: EventEmitter, event: UpdaterEvents, handler: (...args: Array<any>) => void): void {
  if (isLogEvent) {
    emitter.on(event, (...args: Array<any>) => {
      console.log("%s %s", event, args)
      handler(...args)
    emitter.on(event, handler)
export interface UpdateCheckResult {
  readonly isUpdateAvailable: boolean
  readonly updateInfo: UpdateInfo
  readonly downloadPromise?: Promise<Array<string>> | null
  readonly cancellationToken?: CancellationToken
  /** @deprecated */
  readonly versionInfo: UpdateInfo
export interface UpdateDownloadedEvent extends UpdateInfo {
  downloadedFile: string
export interface ResolvedUpdateFileInfo {
  readonly url: URL
  readonly info: UpdateFileInfo
  packageInfo?: PackageFileInfo
export type UpdaterEvents = "login" | "checking-for-update" | "update-available" | "update-not-available" | "update-cancelled" | "download-progress" | "update-downloaded" | "error"
export type LoginHandler = (authInfo: any, callback: LoginCallback) => void
