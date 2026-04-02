import OktaAuth, { OktaAuthOptions, IDToken, AccessToken } from '@okta/okta-auth-js';
 * Okta authentication provider implementation - v3.0.0
 * Implements the abstract methods from MJAuthBase to hide Okta-specific details.
 * The key abstraction is that Okta stores the JWT in IDToken.idToken,
@RegisterClass(MJAuthBase, 'okta')
export class MJOktaProvider extends MJAuthBase {
  static readonly PROVIDER_TYPE = 'okta';
  readonly type = MJOktaProvider.PROVIDER_TYPE;
  private oktaAuth: OktaAuth;
  private isRefreshing = false;
   * Factory function to provide Angular dependencies required by Okta
      provide: 'oktaConfig',
        clientId: environment['OKTA_CLIENTID'],
        domain: environment['OKTA_DOMAIN'],
        issuer: environment['OKTA_ISSUER'] || `https://${environment['OKTA_DOMAIN']}/oauth2/default`,
        redirectUri: environment['OKTA_REDIRECT_URI'] || window.location.origin,
        scopes: environment['OKTA_SCOPES'] || ['openid', 'profile', 'email']
  constructor(@Inject('oktaConfig') private oktaConfig: OktaAuthOptions & { domain?: string }) {
      name: MJOktaProvider.PROVIDER_TYPE,
      type: MJOktaProvider.PROVIDER_TYPE
    // Build configuration with defaults
    const oktaAuthConfig: OktaAuthOptions = {
      clientId: this.oktaConfig.clientId,
      redirectUri: this.oktaConfig.redirectUri || window.location.origin + '/callback',
      scopes: this.oktaConfig.scopes || ['openid', 'profile', 'email'],
      pkce: true, // Use PKCE for security
      ...this.oktaConfig,
      // Set issuer after spread to ensure it's not overwritten if not present in config
      issuer: this.oktaConfig.issuer || (this.oktaConfig.domain ? `https://${this.oktaConfig.domain}/oauth2/default` : '')
    this.oktaAuth = new OktaAuth(oktaAuthConfig);
    // Listen for token events
    this.oktaAuth.authStateManager.subscribe((authState: unknown) => {
      const state = authState as { isAuthenticated?: boolean; idToken?: IDToken };
      this.updateAuthState(state.isAuthenticated || false);
      // Don't update user info if we're in the middle of a refresh
      if (!this.isRefreshing) {
        if (state.isAuthenticated && state.idToken) {
          const userInfo = this.mapOktaTokenToStandard(state.idToken);
    // Check URL for logout indicator
    const urlParams = new URLSearchParams(window.location.search);
    const isPostLogout = urlParams.has('logout');
    if (isPostLogout) {
      // We're returning from a logout, clear the URL and stay logged out
      window.history.replaceState({}, document.title, window.location.pathname);
    // Check if we're returning from a login redirect
    // Note: handleRedirect() is the modern v7.x replacement for deprecated handleLoginRedirect()
    if (this.oktaAuth.isLoginRedirect()) {
        await this.oktaAuth.handleRedirect();
        // After handling redirect, check if we're authenticated
        const authState = await this.oktaAuth.authStateManager.getAuthState();
        if (authState?.isAuthenticated) {
          // Get and update user info
          const idToken = await this.oktaAuth.tokenManager.get('idToken') as IDToken;
          if (idToken) {
            const userInfo = this.mapOktaTokenToStandard(idToken);
        console.error('[Okta] Initialization redirect handling error:', error);
      return; // Don't check for existing session after handling redirect
    // Only check for existing session if not a redirect
      const isAuthenticated = await this.oktaAuth.isAuthenticated();
        // Double-check we actually have valid tokens
        if (idToken?.idToken) {
          // Update user info from cached token
          // No valid tokens, stay logged out
          this.updateAuthState(false);
      console.warn('[Okta] Error checking authentication status:', error);
      // Check if we're in a redirect callback
      // Start the login flow
      await this.oktaAuth.signInWithRedirect({
        originalUri: (options?.['targetUrl'] as string) || '/',
      console.error('[Okta] Login error:', error);
      // Clear the local authentication state immediately
      // Clear all tokens from local storage
      await this.oktaAuth.tokenManager.clear();
      // Sign out from Okta completely
      await this.oktaAuth.signOut({
        postLogoutRedirectUri: window.location.origin,
        clearTokensBeforeRedirect: true
      // Note: The signOut call will redirect the browser, so code after this won't execute
      console.error('[Okta] Logout error:', error);
      // If logout fails, at least clear local state and reload
      window.location.href = window.location.origin;
          // Do a controlled reload after successful login
          }, 100);
      console.error('[Okta] Callback handling error:', error);
   * Extract ID token from Okta's storage
   * Okta stores the JWT in IDToken.idToken
   * This is the key abstraction - consumers never need to know about Okta's structure!
      // Okta-specific detail: JWT is in idToken property
      return idToken?.idToken || null;
      console.error('[Okta] Error extracting ID token:', error);
   * Extract complete token info from Okta
   * Maps Okta's token structure to StandardAuthToken
      const accessToken = await this.oktaAuth.tokenManager.get('accessToken') as AccessToken;
      if (!idToken?.idToken) {
        idToken: idToken.idToken,
        accessToken: accessToken?.accessToken,
        expiresAt: idToken.expiresAt ? idToken.expiresAt * 1000 : 0, // Convert to milliseconds
        scopes: idToken.scopes
      console.error('[Okta] Error extracting token info:', error);
   * Extract user info from Okta claims
   * Maps Okta's IDToken structure to StandardUserInfo
      return this.mapOktaTokenToStandard(idToken);
      console.error('[Okta] Error extracting user info:', error);
   * Refresh token using Okta's token renewal
   * Uses renewTokens() to get new tokens silently
      console.log('[Okta] Attempting to refresh token...');
      // Set flag to prevent authStateManager from triggering updates
      this.isRefreshing = true;
      // First check if we're authenticated
      if (!isAuthenticated) {
        this.isRefreshing = false;
            message: 'User is not authenticated',
      // Check if tokens exist
      if (!idToken || !accessToken) {
            message: 'No tokens available to refresh',
            userMessage: 'Session not found. Please log in again.'
      // Attempt to renew tokens
      const renewedTokens = await this.oktaAuth.token.renewTokens();
      if (renewedTokens.idToken) {
        // Store the renewed tokens
        this.oktaAuth.tokenManager.setTokens(renewedTokens);
        // Wait a moment before resetting the flag
        const newIdToken = renewedTokens.idToken as IDToken;
        console.log('[Okta] Token refresh successful', {
          expiresAt: newIdToken.expiresAt ? new Date(newIdToken.expiresAt * 1000).toISOString() : 'N/A'
          idToken: newIdToken.idToken || '',
          accessToken: renewedTokens.accessToken?.accessToken,
          expiresAt: newIdToken.expiresAt ? newIdToken.expiresAt * 1000 : 0,
          scopes: newIdToken.scopes
          message: 'Token renewal succeeded but no ID token returned',
      console.error('[Okta] Token refresh failed:', error);
   * Classify Okta-specific errors into semantic types
   * Maps Okta error patterns to AuthErrorType enum.
   * Updated for okta-auth-js v7.x error codes.
   * Error sources:
   * - errorCode: From Okta SDK errors (AuthSdkError, AuthApiError)
   * - error: From OAuth /token endpoint responses (invalid_grant, access_denied, etc.)
    // OAuth errors from /token endpoint use 'error' field instead of 'errorCode'
    const oauthError = errorObj?.['error'] as string || '';
    // Handle invalid_grant - refresh token is invalid or expired
    // This is a common error when refresh tokens expire or are revoked
    if (errorCode === 'invalid_grant' || oauthError === 'invalid_grant' ||
        message.includes('refresh token is invalid or expired')) {
    // Handle access_denied - user or server denied the request
    if (errorCode === 'access_denied' || oauthError === 'access_denied' ||
        message.includes('access_denied')) {
        userMessage: 'Access was denied. Please try again.'
    // Check for specific Okta error patterns
    if (errorCode === 'login_required' || oauthError === 'login_required' ||
        message.includes('login_required')) {
    if (message.includes('not to prompt') || message.includes('consent_required') ||
        oauthError === 'consent_required') {
    // Handle both user_cancelled (SDK) and user_canceled_request (OAuth redirect)
    if (errorCode === 'user_cancelled' || errorCode === 'user_canceled_request' ||
        oauthError === 'user_canceled_request' ||
        message.includes('user cancelled') || message.includes('user canceled')) {
    if (message.includes('token expired') || message.includes('invalid_token') || message.includes('unauthorized')) {
    if (message.includes('network') || message.includes('fetch') || message.includes('Load failed')) {
   * Map Okta IDToken to StandardUserInfo
  private mapOktaTokenToStandard(idToken: IDToken): StandardUserInfo {
    const claims = idToken.claims;
      id: claims.sub || '',
      email: claims.email as string || '',
      name: claims.name as string || '',
      givenName: claims.given_name as string,
      familyName: claims.family_name as string,
      preferredUsername: claims.preferred_username as string,
      emailVerified: claims.email_verified as boolean
   * Get profile picture URL from Okta
   * Okta may include picture URL in user claims, similar to Auth0.
   * If available, we can also fetch from Okta's /userinfo endpoint.
      // Check if picture is in claims
      const pictureUrl = idToken.claims.picture as string;
      if (pictureUrl) {
        return pictureUrl;
      // Alternatively, fetch from userinfo endpoint
      const user = await this.oktaAuth.getUser();
      return (user?.picture as string) || null;
      console.error('[Okta] Error getting profile picture:', error);
   * Handle session expiry - no-op for Okta
   * Okta uses refresh tokens, so it doesn't need interactive re-authentication
   * when tokens expire. If refresh fails, the base class will throw an error
   * and the user must log out/in manually.
    // No-op - Okta doesn't need interactive re-auth
  override validateConfig(config: Record<string, unknown>): boolean {
    return !!(config['clientId'] && (config['domain'] || config['issuer']));
