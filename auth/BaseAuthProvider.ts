import { JwtHeader, JwtPayload, SigningKeyCallback } from 'jsonwebtoken';
import { AuthProviderConfig, AuthUserInfo } from '@memberjunction/core';
import https from 'https';
import http from 'http';
 * Base implementation of IAuthProvider with common functionality
 * Concrete providers should extend this class and use @RegisterClass decorator
 * with BaseAuthProvider as the base class
export abstract class BaseAuthProvider implements IAuthProvider {
  jwksUri: string;
  /** OAuth client ID for this provider (used by OAuth proxy for upstream auth) */
  protected config: AuthProviderConfig;
  protected jwksClient: jwksClient.JwksClient;
  constructor(config: AuthProviderConfig) {
    this.name = config.name;
    this.issuer = config.issuer;
    this.audience = config.audience;
    this.jwksUri = config.jwksUri;
    this.clientId = config.clientId;
    // Create HTTP agent with keep-alive to prevent socket hangups
    const agent = this.jwksUri.startsWith('https')
      ? new https.Agent({
          keepAlive: true,
          keepAliveMsecs: 30000,
          maxSockets: 50,
          maxFreeSockets: 10,
          timeout: 60000
      : new http.Agent({
    // Initialize JWKS client with connection pooling and extended timeout
    this.jwksClient = jwksClient({
      jwksUri: this.jwksUri,
      timeout: 60000, // 60 seconds (increased from default 30s)
      requestAgent: agent
   * Validates that required configuration is present
  validateConfig(): boolean {
    return !!(this.name && this.issuer && this.audience && this.jwksUri);
   * Gets the signing key for token verification with retry logic
  getSigningKey(header: JwtHeader, callback: SigningKeyCallback): void {
    this.getSigningKeyWithRetry(header, 3, 1000)
        callback(null, signingKey);
        console.error(`Error getting signing key for provider ${this.name} after retries:`, err);
        callback(err);
   * Retrieves signing key with exponential backoff retry logic
  private async getSigningKeyWithRetry(
    header: JwtHeader,
    maxRetries: number,
    initialDelayMs: number
  ): Promise<jwksClient.SigningKey> {
        return await this.jwksClient.getSigningKey(header.kid);
        lastError = err instanceof Error ? err : new Error(String(err));
        // Check if this is a connection error that's worth retrying
        const isRetryableError =
          lastError.message.includes('socket hang up') ||
          lastError.message.includes('ECONNRESET') ||
          lastError.message.includes('ETIMEDOUT') ||
          lastError.message.includes('ENOTFOUND') ||
          lastError.message.includes('EAI_AGAIN');
        if (!isRetryableError || attempt === maxRetries) {
        // Exponential backoff: wait longer between each retry
        const delayMs = initialDelayMs * Math.pow(2, attempt);
          `Attempt ${attempt + 1}/${maxRetries + 1} failed for provider ${this.name}. ` +
          `Retrying in ${delayMs}ms... Error: ${lastError.message}`
        await new Promise(resolve => setTimeout(resolve, delayMs));
    throw lastError || new Error('Failed to retrieve signing key');
   * Checks if a given issuer URL belongs to this provider
  matchesIssuer(issuer: string): boolean {
    // Handle trailing slashes and case sensitivity
    const normalizedIssuer = issuer.toLowerCase().replace(/\/$/, '');
    const normalizedProviderIssuer = this.issuer.toLowerCase().replace(/\/$/, '');
    return normalizedIssuer === normalizedProviderIssuer;
   * Abstract method for extracting user info - must be implemented by each provider
  abstract extractUserInfo(payload: JwtPayload): AuthUserInfo;
