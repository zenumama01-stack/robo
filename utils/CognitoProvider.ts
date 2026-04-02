 * AWS Cognito authentication provider implementation
@RegisterClass(BaseAuthProvider, 'cognito')
export class CognitoProvider extends BaseAuthProvider {
   * Extracts user information from Cognito JWT payload
    // Cognito uses custom claims with 'cognito:' prefix for some fields
    const email = payload.email as string | undefined || 
                  payload['cognito:username'] as string | undefined;
    const preferredUsername = payload['cognito:username'] as string | undefined || 
                             payload.preferred_username as string | undefined || 
                             email;
   * Validates Cognito-specific configuration
    const hasRegion = !!this.config.region;
    const hasUserPoolId = !!this.config.userPoolId;
    return baseValid && hasClientId && hasRegion && hasUserPoolId;
