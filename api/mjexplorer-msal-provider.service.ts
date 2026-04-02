import { BehaviorSubject, Observable, Subject, catchError, filter, from, map, of, throwError, takeUntil, take } from 'rxjs';
import { MsalBroadcastService, MsalService, MSAL_INSTANCE, MSAL_GUARD_CONFIG, MSAL_INTERCEPTOR_CONFIG, MsalGuard } from '@azure/msal-angular';
import { AccountInfo, AuthenticationResult } from '@azure/msal-common';
import { CacheLookupPolicy, InteractionRequiredAuthError, InteractionStatus, PublicClientApplication, InteractionType, BrowserAuthError } from '@azure/msal-browser';
 * MSAL (Microsoft Authentication Library) provider implementation - v3.0.0
 * Implements the abstract methods from MJAuthBase to hide MSAL-specific details.
 * The key abstraction is that MSAL stores the JWT in AuthenticationResult.idToken,
@RegisterClass(MJAuthBase, 'msal')
export class MJMSALProvider extends MJAuthBase implements OnDestroy {
  static readonly PROVIDER_TYPE = 'msal';
  readonly type = MJMSALProvider.PROVIDER_TYPE;
  private readonly _destroying$ = new Subject<void>();
  private readonly _initializationCompleted$ = new BehaviorSubject<boolean>(false);
  private _initPromise: Promise<void> | null = null;
   * Factory function to provide Angular dependencies required by MSAL
  static angularProviderFactory = (environment: Record<string, unknown>) => [
      provide: MSAL_INSTANCE,
      useValue: new PublicClientApplication({
          clientId: environment['CLIENT_ID'] as string,
          authority: environment['CLIENT_AUTHORITY'] as string,
          redirectUri: window.location.origin,
        cache: {
          cacheLocation: 'localStorage',
      provide: MSAL_GUARD_CONFIG,
        interactionType: InteractionType.Redirect,
        authRequest: {
          scopes: ['User.Read'],
      provide: MSAL_INTERCEPTOR_CONFIG,
        protectedResourceMap: new Map([['https://graph.microsoft.com/v1.0/me', ['user.read']]]),
    MsalService,
    MsalGuard,
    MsalBroadcastService
  constructor(public auth: MsalService, private msalBroadcastService: MsalBroadcastService) {
      name: MJMSALProvider.PROVIDER_TYPE,
      type: MJMSALProvider.PROVIDER_TYPE
    if (this._initPromise) {
      return this._initPromise;
    this._initPromise = this._performInitialization();
  private async _performInitialization(): Promise<void> {
    console.log('[MSAL] Starting initialization...');
    await this.auth.instance.initialize();
    console.log('[MSAL] MSAL instance initialized');
    // Handle redirect immediately after initialization
    const redirectResponse = await this.auth.instance.handleRedirectPromise();
    console.log('[MSAL] Redirect response:', redirectResponse ? 'Found' : 'None');
    if (redirectResponse && redirectResponse.account) {
      // User just logged in via redirect
      console.log('[MSAL] Processing redirect login');
      this.auth.instance.setActiveAccount(redirectResponse.account);
      this.updateAuthState(true);
      // Update user info from account
      const userInfo = this.mapMSALAccountToStandard(redirectResponse.account);
      this._initializationCompleted$.next(true);
      console.log('[MSAL] Initialization completed (redirect login)');
      // Set active account if we have one from cache
      const accounts = this.auth.instance.getAllAccounts();
      console.log('[MSAL] Cached accounts found:', accounts.length);
        console.log('[MSAL] Restoring session from cached account:', accounts[0].username);
        this.auth.instance.setActiveAccount(accounts[0]);
        // Update user info from cached account
        const userInfo = this.mapMSALAccountToStandard(accounts[0]);
        // Proactively refresh tokens to extend the interaction-free period
        // This uses refreshTokenExpirationOffsetSeconds to ensure tokens remain valid
        // for at least 2 hours without requiring user interaction
        this.performProactiveRefresh(accounts[0]).catch((err: unknown) => {
          // Log but don't fail initialization - cached tokens may still be valid
          console.warn('[MSAL] Proactive token refresh failed during init:', err);
        console.log('[MSAL] Initialization completed (cached session restored)');
        console.log('[MSAL] No cached accounts, user needs to log in');
    // Subscribe to broadcast service for ongoing auth state changes
    this.msalBroadcastService.inProgress$
      .pipe(
        filter((status: InteractionStatus) => status === InteractionStatus.None),
        takeUntil(this._destroying$)
        const isAuth = accounts.length > 0;
        this.updateAuthState(isAuth);
        if (isAuth) {
    const silentRequest = {
      scopes: ['User.Read', 'email', 'profile'],
      this.auth.loginRedirect(silentRequest).subscribe({
    this.auth.logoutRedirect().subscribe(() => {
      // Logout will trigger a redirect
    // MSAL Angular handles callbacks internally through its broadcast service
    // The handleRedirectPromise is called in initialize()
   * Extract ID token from MSAL's storage
   * MSAL stores the JWT in AuthenticationResult.idToken
   * This is the key abstraction - consumers never need to know about MSAL's structure!
      const account = this.auth.instance.getActiveAccount();
      // First try to get cached token from account
      // This avoids unnecessary iframe calls that can timeout
      if (account.idToken) {
        return account.idToken;
      // If not in account, try silent token acquisition from cache only
      // Use CacheLookupPolicy.AccessToken to avoid iframe calls
      const response = await this.auth.instance.acquireTokenSilent({
        account: account,
        cacheLookupPolicy: CacheLookupPolicy.AccessToken
      // MSAL-specific detail: JWT is in idToken property
      return response.idToken || null;
      console.error('[MSAL] Error extracting ID token:', error);
   * Extract complete token info from MSAL
   * Maps MSAL's AuthenticationResult to StandardAuthToken
      // Use cache-only lookup to avoid iframe timeouts during normal token extraction
      if (!response.idToken) {
        idToken: response.idToken,
        accessToken: response.accessToken,
        expiresAt: response.expiresOn ? response.expiresOn.getTime() : 0,
        scopes: response.scopes
      // If acquireTokenSilent fails (e.g., iframe timeout), try to use cached account data
      console.error('[MSAL] Error extracting token info:', error);
      if (account?.idToken) {
        // Return basic token info from account if available
          idToken: account.idToken,
          expiresAt: 0, // Unknown from account alone
          scopes: []
   * Extract user info from MSAL account
   * Maps MSAL's AccountInfo structure to StandardUserInfo
      return this.mapMSALAccountToStandard(account);
      console.error('[MSAL] Error extracting user info:', error);
   * Refresh token using MSAL's silent token acquisition
   * MSAL 5.x Best Practices:
   * - Pass account parameter for reliable silent acquisition
   * - Use CacheLookupPolicy.Default for efficient cache → refresh token → iframe chain
   * - Handle MSAL 5.x specific error codes (timed_out, no_tokens_found, etc.)
            message: 'No active account found',
      // MSAL 5.x: Pass account for reliable silent acquisition
      // Use Default policy for efficient cache → refresh token → iframe chain
        cacheLookupPolicy: CacheLookupPolicy.Default
            message: 'Token refresh succeeded but no ID token returned',
      const token: StandardAuthToken = {
      console.error('[MSAL] Token refresh failed:', error);
      // Handle MSAL 5.x error codes that require user interaction
      // - timed_out: MSAL 5.x iframe timeout (replaces monitor_window_timeout from 3.x)
      // - monitor_window_timeout: Legacy MSAL 3.x iframe timeout
      // - no_tokens_found: No cached tokens available
      // - no_account_error: Account not found in cache
      // - InteractionRequiredAuthError: Server requires user interaction
      const errorCode = (error as Record<string, unknown>)?.errorCode as string | undefined;
      const interactionRequiredCodes = [
        'monitor_window_timeout',
        'timed_out',
        'no_tokens_found',
        'no_account_error',
        'login_required',
        'consent_required'
      if (interactionRequiredCodes.includes(errorCode || '') || error instanceof InteractionRequiredAuthError) {
        // Return INTERACTION_REQUIRED error - base class will call handleSessionExpiryInternal
            message: `Silent token refresh failed - interaction required (${errorCode || 'unknown'})`,
            userMessage: 'Your session has expired. Redirecting to login...',
            originalError: error
   * Classify MSAL-specific errors into semantic types
   * Maps MSAL error classes to AuthErrorType enum.
   * Updated for MSAL 5.x error codes.
    const errorCode = errorObj?.['errorCode'] as string || '';
    // Check for specific MSAL error types
    if (error instanceof InteractionRequiredAuthError) {
        userMessage: 'Additional authentication is required. Please log in again.'
    if (error instanceof BrowserAuthError) {
      // Check specific error codes - MSAL 5.x codes
      if (errorCode === 'user_cancelled' || message.includes('user cancelled')) {
      // MSAL 5.x timeout and session errors that require interaction
      if (interactionRequiredCodes.includes(errorCode)) {
        userMessage: 'Authentication error. Please log in again.'
    // Check message patterns
    if (message.includes('token') && message.includes('expired')) {
    if (message.includes('you need to be authorized')) {
    if (!this._initPromise) {
    } else if (!this._initializationCompleted$.value) {
      await this._initPromise;
   * Proactively refresh tokens to extend the interaction-free period
   * MSAL 5.x Best Practice: Use refreshTokenExpirationOffsetSeconds to get tokens
   * that will remain valid for a specified duration. This is called:
   * - On initialization (to ensure fresh tokens at app startup)
   * - Can be called manually before important operations
   * @param account - The account to refresh tokens for
   * @param offsetSeconds - How long tokens should remain valid (default: 2 hours)
  private async performProactiveRefresh(account: AccountInfo, offsetSeconds: number = 7200): Promise<void> {
      console.log(`[MSAL] Performing proactive token refresh (offset: ${offsetSeconds}s)...`);
      // Use forceRefresh with refreshTokenExpirationOffsetSeconds to get fresh tokens
      // that will be valid for at least the specified offset period
      await this.auth.instance.acquireTokenSilent({
        forceRefresh: true,
        refreshTokenExpirationOffsetSeconds: offsetSeconds
      console.log('[MSAL] Proactive token refresh successful');
      // Don't treat proactive refresh failures as critical - the app can still
      // function with existing cached tokens until they expire
      console.warn('[MSAL] Proactive token refresh failed (will use cached tokens):', error);
      throw error; // Re-throw so caller can decide how to handle
   * Map MSAL AccountInfo to StandardUserInfo
  private mapMSALAccountToStandard(account: AccountInfo): StandardUserInfo {
      id: account.localAccountId || account.homeAccountId,
      email: account.username || '',
      name: account.name || account.username || '',
      givenName: account.idTokenClaims?.['given_name'] as string,
      familyName: account.idTokenClaims?.['family_name'] as string,
      preferredUsername: account.username,
      emailVerified: true // MSAL doesn't provide this, assume true
   * Get profile picture URL from Microsoft Graph API
   * MSAL requires fetching the photo from Microsoft Graph.
   * This is the key advantage of encapsulation - consumers don't need
   * to know about Graph API, they just call getProfilePictureUrl()!
      // Get access token for Microsoft Graph
        forceRefresh: false
      if (!response.accessToken) {
      // Fetch photo from Microsoft Graph
      const graphResponse = await fetch('https://graph.microsoft.com/v1.0/me/photo/$value', {
        headers: { 'Authorization': `Bearer ${response.accessToken}` }
      if (graphResponse.ok) {
        const blob = await graphResponse.blob();
        return URL.createObjectURL(blob);
      console.error('[MSAL] Error getting profile picture:', error);
   * Handle session expiry by redirecting to Microsoft login
   * This method is called by the base class when silent token refresh fails
   * with INTERACTION_REQUIRED error. It redirects to Microsoft login and never returns.
   * After authentication, the app will reload and re-initialize with a fresh token.
    console.log('[MSAL] Redirecting to Microsoft login for re-authentication...');
    // Initiate redirect authentication - page will navigate away
    this.auth.loginRedirect({
      prompt: 'select_account'
    }).subscribe({
      error: (redirectError) => {
        console.error('[MSAL] Redirect initiation failed:', redirectError);
    // Return a promise that never resolves - page will navigate before this matters
    return new Promise<void>(() => {});
    return ['clientId', 'tenantId'];
    // MSAL configuration is handled by Angular module providers
  // CLEANUP
    this._destroying$.next();
    this._destroying$.complete();
    this._initPromise = null;
    this._initializationCompleted$.complete();
