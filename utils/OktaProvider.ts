 * Okta authentication provider implementation
@RegisterClass(BaseAuthProvider, 'okta')
export class OktaProvider extends BaseAuthProvider {
   * Extracts user information from Okta JWT payload
    // Okta uses standard OIDC claims plus some custom ones
   * Validates Okta-specific configuration
