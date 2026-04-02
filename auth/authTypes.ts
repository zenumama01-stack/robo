 * Common authentication types shared between frontend and backend authentication systems
 * This file provides type definitions used by both JWT validation (backend) and OAuth flows (frontend)
 * Standard authentication provider types
export const AUTH_PROVIDER_TYPES = {
  MSAL: 'msal',
  AUTH0: 'auth0',
  OKTA: 'okta',
  COGNITO: 'cognito',
  GOOGLE: 'google',
  CUSTOM: 'custom'
 * Type for authentication provider identifiers
export type AuthProviderType = typeof AUTH_PROVIDER_TYPES[keyof typeof AUTH_PROVIDER_TYPES];
 * Base configuration for authentication providers
 * Used by both backend (JWT validation) and frontend (OAuth flows)
export interface AuthProviderConfig {
   * Unique name identifier for this provider instance
   * Type of authentication provider (e.g., 'msal', 'auth0', 'okta')
  type: AuthProviderType | string;
   * OAuth client ID
   * OAuth client secret (backend only, never expose to frontend)
   * Provider domain (e.g., 'your-domain.auth0.com')
   * Tenant ID for multi-tenant providers (e.g., Azure AD)
   * Token issuer URL (must match 'iss' claim in JWT)
   * Expected audience for tokens
   * JWKS endpoint URL for retrieving signing keys
  jwksUri?: string;
   * OAuth redirect URI for callback after authentication
   * OAuth scopes to request
   * Authority URL for providers that use it (e.g., MSAL)
  authority?: string;
   * Allow provider-specific configuration fields
 * User information extracted from authentication tokens or user profiles
export interface AuthUserInfo {
   * User's first name
   * User's last name
   * User's roles or groups
  roles?: string[];
   * Additional provider-specific claims
 * Token information structure
export interface AuthTokenInfo {
   * OAuth access token for API calls
   * ID token containing user claims
  idToken?: string;
   * Refresh token for obtaining new access tokens
   * Token expiration time
   * Token type (usually 'Bearer')
 * JWT payload structure based on standard OIDC claims
export interface AuthJwtPayload {
   * Subject - unique identifier for the user
   * Email address
   * Given/first name
  given_name?: string;
   * Family/last name
  family_name?: string;
   * Full name
   * Preferred username
   * Token issuer
  iss?: string;
   * Token audience
  aud?: string | string[];
   * Expiration time (seconds since epoch)
  exp?: number;
   * Issued at time (seconds since epoch)
  iat?: number;
   * Not before time (seconds since epoch)
  nbf?: number;
   * Additional claims
