 * Workspace Initializer Service
 * across all MemberJunction applications. Handles GraphQL setup, user validation,
 * error classification, and auth retry logic.
import { StartupValidationService } from '@memberjunction/ng-explorer-core';
import { WorkspaceEnvironment, WorkspaceInitResult, WorkspaceInitError } from '../models/workspace-types';
import { lastValueFrom } from 'rxjs';
export class WorkspaceInitializerService {
    private startupValidationService: StartupValidationService
   * Initialize workspace with authenticated user
   * Replaces all the logic from AppComponent.handleLogin()
  async initializeWorkspace(
    userInfo: StandardUserInfo,
    environment: WorkspaceEnvironment
  ): Promise<WorkspaceInitResult> {
          type: 'unknown',
          message: 'No token provided',
          userMessage: 'Authentication token is missing. Please log in again.',
          shouldRetry: false
      // 1. Setup GraphQL client with token refresh callback
        environment.GRAPHQL_URI,
        environment.GRAPHQL_WS_URI,
          // v3.0 API - clean abstraction, no provider-specific logic!
      console.log(`[Workspace] GraphQL client setup complete: ${end - start}ms`);
      // 2. Load metadata and validate user
            message: "User doesn't have access to the application",
            userMessage: "You don't have access to the application, contact your system administrator.",
      // 3. Run startup validation checks (with small delay for initialization)
        this.startupValidationService.validateSystemSetup();
      // Clear any JWT retry timestamps on successful login
      console.error('[Workspace] initializeWorkspace caught error:', err);
      console.error('[Workspace] Error message:', err?.message);
      console.error('[Workspace] Error stack:', err?.stack);
      if (err?.response?.errors) {
        console.error('[Workspace] GraphQL errors:', JSON.stringify(err.response.errors, null, 2));
      const error = this.classifyError(err);
      console.error('[Workspace] Classified as:', error.type, '-', error.message);
   * Classify errors into actionable types
   * Replaces AppComponent error handling logic
  classifyError(err: any): WorkspaceInitError {
    // Check for no-roles error first (highest priority)
    if (this.isNoUserRolesError(err)) {
      // Add the validation issue through the service
      this.startupValidationService.addNoRolesValidationIssue();
        message: err.message || 'No user roles assigned',
        userMessage: 'Your account does not have any roles assigned. Please contact your administrator.',
    // Check for access denied
    if (err.message?.includes("don't have access")) {
        userMessage: err.message,
    // Check for token expiration
    if (authError.type === AuthErrorType.TOKEN_EXPIRED) {
        type: 'token_expired',
        message: authError.message,
        userMessage: authError.userMessage || 'Your session has expired. Please log in again.',
        shouldRetry: true
    // Network errors
    if (err.message?.includes('network') || err.message?.includes('fetch')) {
        type: 'network',
        userMessage: 'Network error. Please check your connection and try again.',
    // Unknown error
      message: err.message || 'Unknown error',
      userMessage: 'An unexpected error occurred. Please try again.',
   * Helper function to safely check if an error is related to missing user roles
   * Extracted from AppComponent.isNoUserRolesError()
  private isNoUserRolesError(err: any): boolean {
      // Check if error has response with errors array
      // Check for specific "ResourceTypes" error which happens when user has no roles
        // This is the specific error we're seeing that indicates no roles
   * Handle authentication retry with backoff
   * Replaces AppComponent.handleAuthRetry() logic
  async handleAuthRetry(error: WorkspaceInitError, currentPath: string): Promise<boolean> {
    if (!error.shouldRetry) {
    if (!retriedRecently && error.type === 'token_expired') {
      LogStatus('JWT Expired, retrying once: ' + error.message);
      const login$ = this.authBase.login({ appState: { target: currentPath } });
      await lastValueFrom(login$);
