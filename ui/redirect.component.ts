import { Component, OnInit } from '@angular/core';
 * Generic redirect component that handles OAuth callbacks for all providers
 * This replaces the MSAL-specific redirect component to work with any auth provider
  selector: 'app-redirect',
    @if (isProcessing) {
      <div style="display: flex; justify-content: center; align-items: center; height: 100vh;">
        <div style="text-align: center;">
          <h2>Processing authentication...</h2>
          <p>Please wait while we complete your login.</p>
export class RedirectComponent implements OnInit {
  isProcessing = false;
  constructor(private authService: MJAuthBase) {}
    // Only show the processing message if we're actually handling an auth redirect
    // Check for auth codes in the URL that indicate we're in a redirect flow
    const hasAuthCode = window.location.hash.includes('code=') ||
                       window.location.hash.includes('id_token=') ||
                       window.location.search.includes('code=') ||
                       window.location.search.includes('state=');
    // Don't handle MCP OAuth callbacks - those go to /oauth/callback and are handled
    // by the OAuthCallbackComponent in explorer-core. This component only handles
    // the main application auth (MSAL/Azure AD).
    const isMCPOAuthCallback = window.location.pathname.startsWith('/oauth/callback');
    if (hasAuthCode && !isMCPOAuthCallback) {
      this.isProcessing = true;
        // Handle the callback for the current auth provider
        await this.authService.handleCallback();
        console.error('Error handling auth callback:', error);
        // Always hide the component after processing
        this.isProcessing = false;
