 * Google Identity Platform authentication provider implementation
@RegisterClass(BaseAuthProvider, 'google')
export class GoogleProvider extends BaseAuthProvider {
   * Extracts user information from Google JWT payload
    // Google uses standard OIDC claims
    const preferredUsername = email; // Google typically uses email as username
   * Validates Google-specific configuration
    return baseValid && hasClientId;
