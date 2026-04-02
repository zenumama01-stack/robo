import { JwtPayload } from 'jsonwebtoken';
import { BaseAuthProvider } from '../BaseAuthProvider.js';
 * Auth0 authentication provider implementation
@RegisterClass(BaseAuthProvider, 'auth0')
export class Auth0Provider extends BaseAuthProvider {
   * Extracts user information from Auth0 JWT payload
  extractUserInfo(payload: JwtPayload): AuthUserInfo {
    // Auth0 uses standard OIDC claims
    const email = payload.email as string | undefined;
    const firstName = payload.given_name as string | undefined;
    const lastName = payload.family_name as string | undefined;
    const preferredUsername = payload.preferred_username as string | undefined || email;
      firstName: firstName || fullName?.split(' ')[0],
      lastName: lastName || fullName?.split(' ')[1] || fullName?.split(' ')[0],
   * Validates Auth0-specific configuration
    const baseValid = super.validateConfig();
    const hasClientId = !!this.config.clientId;
    const hasDomain = !!this.config.domain;
    return baseValid && hasClientId && hasDomain;
