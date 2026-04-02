 * MemberJunction Initialization Service
 * Handles all core initialization tasks:
 * - Generated entities loading
 * - GraphQL client setup
 * - Token refresh configuration
 * - System validation
import { Injectable, Inject } from '@angular/core';
import { setupGraphQLClient, GraphQLProviderConfigData } from '@memberjunction/graphql-dataprovider';
import { MJEnvironmentConfig, MJ_ENVIRONMENT, MJStartupValidationService, MJ_STARTUP_VALIDATION } from '../bootstrap.types';
export interface InitializationResult {
    type: 'no_access' | 'no_roles' | 'auth_error' | 'unknown';
    showValidationOnly?: boolean;
export class MJInitializationService {
    private authBase: MJAuthBase,
    @Inject(MJ_STARTUP_VALIDATION) private startupValidationService: MJStartupValidationService | null
   * Initialize GraphQL client with authentication token and refresh callback
  async initializeGraphQL(token: string, environment: MJEnvironmentConfig): Promise<void> {
    const url: string = environment.GRAPHQL_URI;
    const wsurl: string = environment.GRAPHQL_WS_URI;
    const config = new GraphQLProviderConfigData(
      wsurl,
      async () => {
        // Token refresh callback - refreshToken() returns StandardAuthToken directly or throws
        const token = await this.authBase.refreshToken();
        return token.idToken;
      environment.MJ_CORE_SCHEMA_NAME
    await setupGraphQLClient(config);
    const end = Date.now();
    console.log(`✓ GraphQL Client Setup Complete: ${end - start}ms`);
   * Refresh shared data and validate user access
  async validateUserAccess(): Promise<InitializationResult> {
    await SharedService.RefreshData(true);
    if (!md.CurrentUser) {
          message: "You don't have access to the application, contact your system administrator.",
          type: 'no_access'
   * Run startup validation checks (if validation service is provided)
  runValidationChecks(): void {
    if (!this.startupValidationService) {
      return; // No validation service provided
    // Small delay to ensure everything is initialized
      this.startupValidationService?.validateSystemSetup();
   * Navigate to initial route after successful login
  navigateToInitialRoute(initialPath: string, document: Document): void {
    localStorage.removeItem('jwt-retry-ts');
    if (initialPath === '/') {
      // Use first nav item instead
        // Find the KendoDrawer element and simulate a click for the first item
        const drawerElement = document.querySelector('li.k-drawer-item.k-level-0') as any;
        if (drawerElement) drawerElement.click();
      }, 10); // Wait for the drawer to finish render
      this.router.navigateByUrl(initialPath, { replaceUrl: true });
   * Check if error is related to missing user roles
  isNoUserRolesError(err: any): boolean {
      if (!err || typeof err !== 'object') return false;
      // Check for GraphQL-style errors
      if (err.response && Array.isArray(err.response.errors)) {
        return err.response.errors.some((e: any) =>
          e && e.message && typeof e.message === 'string' &&
          e.message.includes('does not have read permissions on User Roles')
      // Check for specific "ResourceTypes" error
      if (err.toString && typeof err.toString === 'function') {
        const errorString = err.toString();
        if (errorString.includes("Cannot read properties of undefined (reading 'ResourceTypes')")) {
      // Check for error message directly on the error object
      if (err.message && typeof err.message === 'string') {
        const message = err.message;
        return message.includes('does not have read permissions on User Roles') ||
               message.includes("Cannot read properties of undefined (reading 'ResourceTypes')");
      // Check for nested error object
      if (err.error && typeof err.error === 'object') {
        return this.isNoUserRolesError(err.error);
      console.error('Error while checking for user roles error:', e);
   * Handle no roles error by showing validation banner (if validation service is provided)
  handleNoRolesError(): InitializationResult {
    this.startupValidationService?.addNoRolesValidationIssue();
        message: 'User has no roles assigned',
        type: 'no_roles',
        showValidationOnly: true
   * Handle authentication retry logic
  async handleAuthRetry(err: any): Promise<boolean> {
    const retryKey = 'auth-retry-dt';
    const lastRetryDateTime = localStorage.getItem(retryKey);
    const yesterday = +new Date(Date.now() - 1 * 24 * 60 * 60 * 1000);
    const retriedRecently = lastRetryDateTime && +new Date(lastRetryDateTime) > yesterday;
    const authError = this.authBase.classifyError(err);
    const isTokenExpired = authError.type === AuthErrorType.TOKEN_EXPIRED;
    if (!retriedRecently && isTokenExpired) {
      console.log('JWT Expired, retrying once: ' + err);
      localStorage.setItem(retryKey, new Date().toISOString());
      await this.authBase.login({ appState: { target: window.location.pathname } }).toPromise();
   * Get auth error message for display
  getAuthErrorMessage(err: unknown): string {
    switch (authError.type) {
      case AuthErrorType.NO_ACTIVE_SESSION:
        return "Welcome back! Please log in to your account.";
      case AuthErrorType.INTERACTION_REQUIRED:
      case AuthErrorType.TOKEN_EXPIRED:
        return "Your session has expired. Please log in to your account.";
        return authError.userMessage || "Welcome back! Please log in to your account.";
