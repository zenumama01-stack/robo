 * Microsoft Authentication Library (MSAL) provider implementation
@RegisterClass(BaseAuthProvider, 'msal')
export class MSALProvider extends BaseAuthProvider {
   * Extracts user information from MSAL/Azure AD JWT payload
    // MSAL/Azure AD uses some custom claims
    const email = payload.email as string | undefined || payload.preferred_username as string | undefined;
   * Validates MSAL-specific configuration
    const hasTenantId = !!this.config.tenantId;
    return baseValid && hasClientId && hasTenantId;
