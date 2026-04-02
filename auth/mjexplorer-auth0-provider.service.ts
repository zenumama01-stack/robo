import { MJAuthBase } from '../mjexplorer-auth-base.service';
import { AuthService, IdToken, User, AuthGuard, AuthConfigService, Auth0ClientService, Auth0ClientFactory, AuthClientConfig } from '@auth0/auth0-angular';
import { Observable, of, firstValueFrom } from 'rxjs';
import { take, filter } from 'rxjs/operators';
import { AngularAuthProviderConfig } from '../IAuthProvider';
  AuthErrorType,
} from '../auth-types';
 * Auth0 authentication provider implementation - v3.0.0
 * Implements the abstract methods from MJAuthBase to hide Auth0-specific details.
 * The key abstraction is that Auth0 stores the JWT in `idTokenClaims.__raw`,
 * but consumers never need to know this detail.
@RegisterClass(MJAuthBase, 'auth0')
export class MJAuth0Provider extends MJAuthBase {
  static readonly PROVIDER_TYPE = 'auth0';
  readonly type = MJAuth0Provider.PROVIDER_TYPE;
  private _initialized = false;
   * Factory function to provide Angular dependencies required by Auth0
   * Stored as a static property for the factory to access without instantiation
  static angularProviderFactory = (environment: Record<string, unknown>) => {
    // Check if we're on the MCP OAuth callback path
      AuthService,
      AuthGuard,
        provide: AuthConfigService,
        useValue: {
          domain: environment['AUTH0_DOMAIN'],
          clientId: environment['AUTH0_CLIENTID'],
          authorizationParams: {
            redirect_uri: window.location.origin,
            scope: 'openid profile email offline_access',
            // No audience parameter - uses ID tokens (matches pre-refactor behavior)
            // ID tokens have aud=clientId by default and contain user claims
          cacheLocation: 'localstorage',
          // Enable refresh tokens so we can refresh without relying on Auth0 session cookies
          useRefreshTokens: true,
          // Use rotating refresh tokens for better security
          useRefreshTokensFallback: true,
          // CRITICAL: Skip Auth0's redirect callback handling for MCP OAuth callbacks
          // The /oauth/callback path is used by MCP OAuth, not Auth0. If we don't skip,
          // Auth0 will consume the code parameter and redirect to /, preventing the
          // OAuthCallbackComponent from processing the MCP OAuth flow.
          skipRedirectCallback: isMCPOAuthCallback,
        provide: Auth0ClientService,
        useFactory: Auth0ClientFactory.createClient,
        deps: [AuthClientConfig],
  constructor(public auth: AuthService) {
    const config: AngularAuthProviderConfig = {
      name: MJAuth0Provider.PROVIDER_TYPE,
      type: MJAuth0Provider.PROVIDER_TYPE
    super(config);
  // LIFECYCLE METHODS
    if (this._initialized) {
    // CRITICAL: Wait for Auth0 SDK to process any redirect callback
    // The SDK handles this internally and updates isLoading$ when done
    await firstValueFrom(this.auth.isLoading$.pipe(
      filter(loading => !loading),
      take(1)
    // Get current authentication state
    const isAuthenticated = await firstValueFrom(this.auth.isAuthenticated$);
    // CRITICAL: Get current user BEFORE setting up subscriptions
    // This ensures userInfo$ has a value before app.component.ts subscribes
    if (isAuthenticated) {
      const user = await firstValueFrom(this.auth.user$);
        const userInfo = this.mapAuth0UserToStandard(user);
    // Subscribe to authentication state and user info for future changes
    this.auth.isAuthenticated$.subscribe((loggedIn) => {
      this.updateAuthState(loggedIn);
    this.auth.user$.subscribe((user) => {
        this.updateUserInfo(null);
    this._initialized = true;
  protected async loginInternal(options?: Record<string, unknown>): Promise<void> {
    this.auth.loginWithRedirect(options);
  async logout(): Promise<void> {
    this.auth.logout({ logoutParams: { returnTo: document.location.origin } });
  async handleCallback(): Promise<void> {
    // Auth0 Angular SDK handles callbacks internally via interceptors
    // This method exists for interface compatibility
  // ABSTRACT METHOD IMPLEMENTATIONS (v3.0.0)
   * Extract ID token from Auth0's storage
   * Auth0 stores the JWT string in idTokenClaims.__raw
   * This is the key abstraction - consumers never need to know about __raw!
  protected async extractIdTokenInternal(): Promise<string | null> {
      const claims = await firstValueFrom(this.auth.idTokenClaims$);
      // Auth0-specific detail: JWT is in __raw property
      return claims?.__raw || null;
      console.error('[Auth0] Error extracting ID token:', error);
   * Extract complete token info from Auth0
   * Maps Auth0's token structure to StandardAuthToken
  protected async extractTokenInfoInternal(): Promise<StandardAuthToken | null> {
      if (!claims) {
      const idToken = claims.__raw;
      if (!idToken) {
        idToken,
        expiresAt: claims.exp ? claims.exp * 1000 : 0, // Convert to milliseconds
        scopes: claims.scope ? claims.scope.split(' ') : undefined
      console.error('[Auth0] Error extracting token info:', error);
   * Extract user info from Auth0 claims
   * Maps Auth0's user object structure to StandardUserInfo
  protected async extractUserInfoInternal(): Promise<StandardUserInfo | null> {
      return this.mapAuth0UserToStandard(user);
      console.error('[Auth0] Error extracting user info:', error);
   * Refresh token using Auth0's silent authentication
   * Uses cacheMode: 'off' to bypass cache and force token refresh
   * With useRefreshTokens: true and offline_access scope, this will use refresh tokens
  protected async refreshTokenInternal(): Promise<TokenRefreshResult> {
      console.log('[Auth0] Attempting to refresh token...');
      // Force token refresh by bypassing cache
      // With useRefreshTokens: true and offline_access scope, this will use refresh tokens
      await firstValueFrom(this.auth.getAccessTokenSilently({
        cacheMode: 'off'
      // Small delay to ensure Auth0 SDK has updated observables
      // Extract fresh token info from updated observables
      const token = await this.extractTokenInfoInternal();
            type: AuthErrorType.NO_ACTIVE_SESSION,
            message: 'Token refresh succeeded but could not extract token info',
            userMessage: 'Session refresh failed. Please log in again.'
      console.log('[Auth0] Token refresh successful', {
        expiresAt: new Date(token.expiresAt).toISOString()
        token
      console.error('[Auth0] Token refresh failed:', error);
        error: this.classifyErrorInternal(error)
   * Classify Auth0-specific errors into semantic types
   * Maps Auth0 error patterns to AuthErrorType enum
  protected classifyErrorInternal(error: unknown): StandardAuthError {
    const errorObj = error as Record<string, unknown>;
    const message = errorObj?.['message'] as string || 'Unknown error';
    const errorType = errorObj?.['error'] as string || '';
    // Check for specific Auth0 error patterns
    if (message.includes('jwt expired') || message.includes('token expired')) {
        type: AuthErrorType.TOKEN_EXPIRED,
        originalError: error,
        userMessage: 'Your session has expired. Please log in again.'
    if (errorType === 'login_required' || message.includes('login_required')) {
        userMessage: 'You need to log in to continue.'
    if (errorType === 'consent_required' || message.includes('consent_required')) {
        type: AuthErrorType.INTERACTION_REQUIRED,
        userMessage: 'Additional permissions are required. Please log in again.'
    if (message.includes('user closed') || message.includes('cancelled')) {
        type: AuthErrorType.USER_CANCELLED,
        userMessage: 'Login was cancelled.'
    if (message.includes('network') || message.includes('fetch')) {
        type: AuthErrorType.NETWORK_ERROR,
        userMessage: 'Network error. Please check your connection and try again.'
    // Default to unknown error
      type: AuthErrorType.UNKNOWN_ERROR,
      userMessage: 'An unexpected error occurred. Please try again.'
  // HELPER METHODS
    if (!this._initialized) {
   * Map Auth0 User object to StandardUserInfo
  private mapAuth0UserToStandard(user: User): StandardUserInfo {
      id: user.sub || '',
      email: user.email || '',
      name: user.name || '',
      givenName: user.given_name,
      familyName: user.family_name,
      preferredUsername: user.nickname,
      pictureUrl: user.picture,
      locale: user.locale,
      emailVerified: user.email_verified
   * Get profile picture URL from Auth0
   * Auth0 includes the picture URL directly in user claims.
   * No additional API call needed.
  protected async getProfilePictureUrlInternal(): Promise<string | null> {
      return user?.picture || null;
      console.error('[Auth0] Error getting profile picture:', error);
   * Handle session expiry - no-op for Auth0
   * Auth0 uses refresh tokens (offline_access scope), so it doesn't need
   * interactive re-authentication when tokens expire. If refresh fails,
   * the base class will throw an error and the user must log out/in manually.
  protected async handleSessionExpiryInternal(): Promise<void> {
    // No-op - Auth0 doesn't need interactive re-auth
  // CONFIGURATION
  override getRequiredConfig(): string[] {
    return ['clientId', 'domain'];
  override validateConfig(_config: Record<string, unknown>): boolean {
    // Auth0 configuration is handled by Angular module providers
