 * MemberJunction Explorer Application Component
 * Complete branded entry point for Explorer-style applications.
 * Provides login screen with MJ branding and wraps mj-shell for authenticated users.
 *   <mj-explorer-app></mj-explorer-app>
import { Component, OnInit, Inject, ViewEncapsulation } from '@angular/core';
import { LogError, SetProductionStatus } from '@memberjunction/core';
import { WorkspaceInitializerService } from '@memberjunction/ng-workspace-initializer';
import { MJEnvironmentConfig, MJ_ENVIRONMENT } from '@memberjunction/ng-bootstrap';
  selector: 'mj-explorer-app',
  templateUrl: './explorer-app.component.html',
  styleUrls: ['./explorer-app.component.css'],
export class MJExplorerAppComponent implements OnInit {
  public title = 'MJ Explorer';
  public initialPath = '/';
  /** True when the current URL is the OAuth callback route - used for conditional rendering */
  public isOAuthCallback = false;
    @Inject(MJ_ENVIRONMENT) private environment: MJEnvironmentConfig,
    private workspaceInit: WorkspaceInitializerService
  async handleLogin(token: string, userInfo: StandardUserInfo) {
      // Delegate all initialization logic to the service
      const result = await this.workspaceInit.initializeWorkspace(token, userInfo, {
        GRAPHQL_URI: this.environment.GRAPHQL_URI,
        GRAPHQL_WS_URI: this.environment.GRAPHQL_WS_URI,
        MJ_CORE_SCHEMA_NAME: this.environment.MJ_CORE_SCHEMA_NAME
        if (this.initialPath === '/') {
          // use first nav item url instead
            // Find the KendoDrawer element, and simulate a click for the first item
            const drawerElement = this.document.querySelector('li.k-drawer-item.k-level-0') as HTMLElement;
          }, 10); // wait for the drawer to finish rerender and then do this
          this.router.navigateByUrl(this.initialPath, { replaceUrl: true });
      } else if (result.error) {
        // Handle errors based on type
        if (result.error.type === 'no_roles') {
          // Show validation banner instead of generic error
          return; // Don't throw, just return to show validation banner
        // Try auth retry for retryable errors
        const retried = await this.workspaceInit.handleAuthRetry(result.error, window.location.pathname);
        if (!retried) {
          // Show error to user
          this.ErrorMessage = result.error.userMessage;
          LogError('Error Logging In: ' + result.error.message);
          throw new Error(result.error.message);
  async setupAuth() {
    // v3.0 API - Clean abstraction using observables
            // v3.0 API - No more provider-specific logic!
              this.handleLogin(token, userInfo);
              // Auth state is managed by the provider itself via observables
          // v3.0 API - Use semantic error classification
              this.subHeaderText = "Welcome back! Please log in to your account.";
              this.subHeaderText = "Your session has expired. Please log in to your account.";
              this.subHeaderText = authError.userMessage || "Welcome back! Please log in to your account.";
    // Check auth state - the provider manages this internally now
          // Instead of kicking off the login process,
          // just display the login screen to the user
          // Auth state is already false if we're here
    // Check if this is the OAuth callback route - used for conditional rendering in template
    // Note: We still run setupAuth() to restore the user's session
    this.isOAuthCallback = window.location.pathname.startsWith('/oauth/callback');
    // Always run auth setup - this restores the user's session
    // For OAuth callback, once authenticated, the OAuthCallbackComponent handles the code exchange
    this.setupAuth();
