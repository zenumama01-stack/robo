 * @fileoverview OAuth Module
 * Provides the OAuth callback handling functionality for MJExplorer.
 * This module handles OAuth provider redirects and completes the
 * authorization flow by exchanging codes for tokens.
 * NOTE: The OAuthCallbackComponent is declared in ExplorerCoreModule and
 * routed directly (not lazy-loaded) because lazy loading doesn't work
 * correctly when routes are defined in a library rather than the main app.
 * @module OAuth Module
 * OAuth Module - provides dependencies for OAuth callback handling.
 * The OAuthCallbackComponent itself is declared in ExplorerCoreModule.
export class OAuthModule { }
