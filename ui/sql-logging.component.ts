import { Component, OnDestroy, HostListener, ChangeDetectorRef } from '@angular/core';
import { SharedService, BaseDashboard } from '@memberjunction/ng-shared';
/** Session options for SQL logging */
interface SqlLoggingSessionOptions {
  formatAsMigration: boolean;
  statementTypes: string;
  prettyPrint: boolean;
/** Represents an active SQL logging session */
interface SqlLoggingSession {
  filePath: string;
  statementCount: number;
  sessionName: string;
  filterByUserId?: string;
  options?: SqlLoggingSessionOptions;
/** SQL logging configuration from the server */
interface SqlLoggingConfig {
  allowedLogDirectory?: string;
  maxActiveSessions?: number;
  autoCleanupEmptyFiles?: boolean;
  sessionTimeout?: number;
  activeSessionCount?: number;
  defaultOptions?: SqlLoggingSessionOptions;
 * Angular component for managing SQL logging sessions in MemberJunction.
 * This component provides a user interface for:
 * - Viewing SQL logging configuration and status
 * - Starting and stopping SQL logging sessions
 * - Managing session options (filtering, formatting, etc.)
 * - Real-time monitoring of active sessions
 * **Security**: Only users with 'Owner' type can access SQL logging features.
 * <mj-sql-logging></mj-sql-logging>
 * @requires Owner-level user privileges
 * @requires SQL logging enabled in server configuration
  selector: 'mj-sql-logging',
  templateUrl: './sql-logging.component.html',
  styleUrls: ['./sql-logging.component.css'],
@RegisterClass(BaseDashboard, 'SqlLogging')
export class SqlLoggingComponent extends BaseDashboard implements OnDestroy {
  /** Whether the component is currently performing an async operation */
  /** Current error message to display to the user, if any */
  /** Whether the current user has Owner privileges to access SQL logging */
  isOwner = false;
  /** Whether SQL logging is enabled in the server configuration */
  configEnabled = false;
  /** Current SQL logging configuration from the server */
  sqlLoggingConfig: SqlLoggingConfig | null = null;
  /** List of currently active SQL logging sessions */
  activeSessions: SqlLoggingSession[] = [];
  /** Currently selected session for viewing logs */
  selectedSession: SqlLoggingSession | null = null;
  /** Content of the currently viewed log file */
  logContent = '';
  /** Whether to automatically refresh session data */
  /** Interval in milliseconds for auto-refresh functionality */
  refreshInterval = 5000; // 5 seconds
  /** Whether the start session dialog is currently visible */
  showStartSessionDialog = false;
  /** Whether to show the statistics cards section */
  showStats = false;
  /** Whether the log viewer is in expanded (fullscreen) mode */
  isLogViewerExpanded = false;
  /** Whether the start session dialog is in fullscreen mode */
  isStartDialogFullscreen = false;
  /** Whether the stop session confirmation dialog is visible */
  showStopConfirmDialog = false;
  /** Session pending stop confirmation (single session or null for all) */
  sessionToStop: SqlLoggingSession | null = null;
  /** Whether stopping all sessions (vs single session) */
  isStoppingAll = false;
  /** Options for creating a new SQL logging session */
  newSessionOptions = {
    /** Custom filename for the log file */
    fileName: '',
    /** Whether to filter SQL statements to current user only */
    filterToCurrentUser: true,
    /** Whether to format output as migration file */
    formatAsMigration: false,
    /** Types of SQL statements to capture */
    statementTypes: 'both' as 'queries' | 'mutations' | 'both',
    /** Whether to format SQL with proper indentation */
    prettyPrint: true,
    /** Human-readable name for the session */
    sessionName: '',
    /** Regex filter options */
    filterPatterns: '' as string, // Comma or newline separated patterns
    filterType: 'exclude' as 'include' | 'exclude',
    verboseOutput: false,
    defaultSchemaName: '__mj', // Default MJ schema
  /** Available options for SQL statement type filtering */
  statementTypeOptions = [
    { text: 'Both Queries and Mutations', value: 'both' },
    { text: 'Queries Only', value: 'queries' },
    { text: 'Mutations Only', value: 'mutations' },
  /** Options for Regex filter */
  filterTypeOptions = [
    { text: 'Exclude Matching (default)', value: 'exclude' },
    { text: 'Include Matching Only', value: 'include' },
    return 'SQL Logging';
    await this.checkUserPermissions();
    if (this.isOwner) {
      await this.loadSqlLoggingConfig();
      await this.loadActiveSessions();
   * Starts the auto-refresh timer for session data.
   * Only refreshes when autoRefresh is enabled and user is Owner.
    interval(this.refreshInterval)
        if (this.autoRefresh && this.isOwner) {
          this.loadActiveSessions();
          if (this.selectedSession) {
            this.loadSessionLog(this.selectedSession);
   * Checks if the current user has Owner privileges required for SQL logging.
   * Updates the isOwner flag and handles error states.
  private async checkUserPermissions() {
      // Try multiple ways to get the current user
      console.log('Method 1 - Metadata.CurrentUser:', {
        email: currentUser?.Email,
        type: currentUser?.Type,
        name: currentUser?.Name,
        id: currentUser?.ID,
      // Use the current user from Metadata
      const userToCheck = currentUser;
      if (userToCheck && userToCheck.Type?.trim().toLowerCase() === 'owner') {
        this.isOwner = true;
        console.log('User is an Owner - SQL logging features enabled');
        this.isOwner = false;
        this.error = 'SQL logging requires Owner privileges';
        console.log('User is NOT an Owner. Type:', userToCheck?.Type);
        // Also check if it's a string comparison issue
        if (userToCheck) {
          console.log('Type value (raw):', JSON.stringify(userToCheck.Type));
          console.log('Type trimmed:', JSON.stringify(userToCheck.Type?.trim()));
          console.log('Type comparison:', userToCheck.Type?.trim().toLowerCase(), '===', 'owner', ':', userToCheck.Type?.trim().toLowerCase() === 'owner');
      console.error('Error checking user permissions:', error);
      this.error = 'Error checking permissions';
   * Opens the dialog for creating a new SQL logging session.
   * Sets default values for session name and filename.
  openStartSessionDialog() {
    // Set default session name
    const currentUser = new Metadata().CurrentUser;
    this.newSessionOptions.sessionName = `SQL Logging - ${currentUser?.Name || currentUser?.Email || 'Unknown'} - ${new Date().toLocaleString()}`;
    this.newSessionOptions.fileName = `sql-log-${new Date().toISOString().replace(/[:.]/g, '-')}.sql`;
    this.showStartSessionDialog = true;
   * Creates and starts a new SQL logging session with the configured options.
   * Shows success/error notifications and refreshes the sessions list.
  async startNewSession() {
      const mutation = `
        mutation StartSqlLogging($input: StartSqlLoggingInput!) {
          startSqlLogging(input: $input) {
            filePath
            statementCount
            sessionName
            filterByUserId
            options {
              formatAsMigration
              statementTypes
              prettyPrint
          fileName: this.newSessionOptions.fileName,
          filterToCurrentUser: this.newSessionOptions.filterToCurrentUser,
            formatAsMigration: this.newSessionOptions.formatAsMigration,
            statementTypes: this.newSessionOptions.statementTypes,
            prettyPrint: this.newSessionOptions.prettyPrint,
            sessionName: this.newSessionOptions.sessionName,
            // Regex Filter Option
            filterPatterns: this.parseFilterPatterns(this.newSessionOptions.filterPatterns),
            filterType: this.newSessionOptions.filterType,
            verboseOutput: this.newSessionOptions.verboseOutput,
            defaultSchemaName: this.newSessionOptions.defaultSchemaName,
      const result = await dataProvider.ExecuteGQL(mutation, variables);
      if (result.errors) {
        throw new Error(result.errors[0].message);
      const newSession = result?.startSqlLogging;
      if (!newSession) {
        throw new Error('Failed to start SQL logging session - no session data returned');
      MJNotificationService.Instance.CreateSimpleNotification(`SQL logging session started: ${newSession.sessionName}`, 'success', 5000);
      this.showStartSessionDialog = false;
      this.selectSession(newSession);
      console.error('Error starting SQL logging session:', error);
      MJNotificationService.Instance.CreateSimpleNotification(`Error: ${error.message || 'Failed to start SQL logging session'}`, 'error', 5000);
   * Executes the stop operation for a specific SQL logging session.
   * Called after user confirms via the confirmation dialog.
   * @param session - The session object to stop
  private async executeStopSession(session: SqlLoggingSession) {
        mutation StopSqlLogging($sessionId: String!) {
          stopSqlLogging(sessionId: $sessionId)
      const result = await dataProvider.ExecuteGQL(mutation, { sessionId: session.id });
      MJNotificationService.Instance.CreateSimpleNotification('SQL logging session stopped', 'success', 3000);
      if (this.selectedSession?.id === session.id) {
        this.selectedSession = null;
        this.logContent = '';
      const errorMessage = error instanceof Error ? error.message : 'Failed to stop SQL logging session';
      console.error('Error stopping SQL logging session:', error);
      MJNotificationService.Instance.CreateSimpleNotification(`Error: ${errorMessage}`, 'error', 5000);
   * Executes the stop operation for all active SQL logging sessions.
  private async executeStopAllSessions() {
        mutation StopAllSqlLogging {
          stopAllSqlLogging
      const result = await dataProvider.ExecuteGQL(mutation, {});
      MJNotificationService.Instance.CreateSimpleNotification('All SQL logging sessions stopped', 'success', 3000);
      const errorMessage = error instanceof Error ? error.message : 'Failed to stop all SQL logging sessions';
      console.error('Error stopping all SQL logging sessions:', error);
   * Selects a session for viewing and loads its log content.
   * @param session - The session to select
  selectSession(session: any) {
    this.selectedSession = session;
    this.loadSessionLog(session);
   * Loads the log file content for a specific session using real-time GraphQL query.
   * Reads actual SQL statements from the log file on the server.
   * @param session - The session whose log to load
  async loadSessionLog(session: any) {
        query ReadSqlLogFile($sessionId: String!, $maxLines: Int) {
          readSqlLogFile(sessionId: $sessionId, maxLines: $maxLines)
        sessionId: session.id,
        maxLines: 1000, // Limit to last 1000 lines for performance
      const logContent = result?.readSqlLogFile || '';
      // Add session header information (show only filename for security)
      const fileName = session.filePath ? session.filePath.split(/[\\\/]/).pop() : 'unknown';
      const header =
        `-- =====================================================\n` +
        `-- SQL Log File: ${fileName}\n` +
        `-- Session: ${session.sessionName}\n` +
        `-- Started: ${new Date(session.startTime).toLocaleString()}\n` +
        `-- Statements Captured: ${session.statementCount}\n` +
        `-- User Filter: ${session.filterByUserId || 'All Users'}\n` +
        `-- Statement Types: ${session.options?.statementTypes || 'both'}\n` +
        `-- Pretty Print: ${session.options?.prettyPrint ? 'Yes' : 'No'}\n` +
        `-- Migration Format: ${session.options?.formatAsMigration ? 'Yes' : 'No'}\n` +
        `-- =====================================================\n\n`;
      this.logContent = header + (logContent || '-- No SQL statements captured yet --');
      console.error('Error loading session log:', error);
      this.logContent =
        `-- Error loading log file --\n-- ${error.message || 'Unknown error occurred'} --\n\n` +
        `-- Session Info --\n` +
        `-- File: ${session.filePath}\n` +
        `-- Started: ${new Date(session.startTime).toLocaleString()}\n`;
   * Loads the SQL logging configuration from the server.
   * Updates component state with current settings and capabilities.
  async loadSqlLoggingConfig() {
        query SqlLoggingConfig {
          sqlLoggingConfig {
            allowedLogDirectory
            maxActiveSessions
            autoCleanupEmptyFiles
            sessionTimeout
            activeSessionCount
            defaultOptions {
              batchSeparator
              logRecordChangeMetadata
              retainEmptyLogFiles
      const result = await dataProvider.ExecuteGQL(query, {});
      // Debug logging to understand the response structure
      console.log('SQL Logging Config Result:', result);
      console.log('Result keys:', Object.keys(result || {}));
      console.log('Direct result.sqlLoggingConfig:', result?.sqlLoggingConfig);
      // Access the data directly from the result, matching AI prompt pattern
      const configData = result?.sqlLoggingConfig;
      console.log('Extracted config data:', configData);
      this.sqlLoggingConfig = configData || null;
      this.configEnabled = this.sqlLoggingConfig?.enabled || false;
      console.log('Component state after update:');
      console.log('  this.sqlLoggingConfig:', this.sqlLoggingConfig);
      console.log('  this.configEnabled:', this.configEnabled);
      console.log('  this.isOwner:', this.isOwner);
      console.error('Error loading SQL logging config:', error);
      this.error = error.message || 'Failed to load SQL logging configuration';
   * Loads the list of currently active SQL logging sessions.
   * Updates the activeSessions array and handles session selection state.
  async loadActiveSessions() {
        query ActiveSqlLoggingSessions {
          activeSqlLoggingSessions {
      console.log('Active Sessions Result:', result);
      const sessionsData = result?.activeSqlLoggingSessions;
      console.log('Extracted sessions data:', sessionsData);
      this.activeSessions = sessionsData || [];
      // Update selected session if it still exists
        const selectedId = this.selectedSession.id;
        const stillExists = this.activeSessions.find((s) => s.id === selectedId);
        if (stillExists) {
          this.selectedSession = stillExists;
      console.error('Error loading active sessions:', error);
   * Calculates and formats the duration of a logging session.
   * @param startTime - ISO string of when the session started
   * @returns Formatted duration string (e.g., "2h 30m", "45m 23s", "12s")
  getSessionDuration(startTime: string): string {
    const start = new Date(startTime);
    const diff = now.getTime() - start.getTime();
    const hours = Math.floor(diff / (1000 * 60 * 60));
    const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
    const seconds = Math.floor((diff % (1000 * 60)) / 1000);
   * Refreshes user data and re-checks permissions.
   * Useful when user privileges have been updated.
  async refreshUserPermissions() {
    console.log('Refreshing user permissions...');
      // Try to refresh SharedService data
      await SharedService.RefreshData(false);
      // Wait a moment for data to propagate
      await new Promise((resolve) => setTimeout(resolve, 1000));
      // Re-check permissions
      console.log('Permissions refreshed');
      console.error('Error refreshing permissions:', error);
   * Extracts the filename from a full file path for security purposes.
   * Only shows the filename, not the full server path.
   * @param filePath - Full file path from server
   * @returns Just the filename portion
  getFileName(filePath: string): string {
    if (!filePath) return 'unknown';
    return filePath.split(/[\\\/]/).pop() || 'unknown';
   * Calculates the total number of SQL statements across all active sessions.
   * @returns Total statement count
  getTotalStatementCount(): number {
    return this.activeSessions.reduce((sum, session) => sum + (session.statementCount || 0), 0);
   * Parses a string of filter patterns (comma or newline separated) into an array
   * @param patternsString - Comma or newline separated patterns
   * @returns Array of pattern strings, or undefined if empty
  private parseFilterPatterns(patternsString: string): string[] | undefined {
    if (!patternsString || !patternsString.trim()) {
    /** Split by comma or newline, trim whitespace, filter empty */
    return patternsString
      .split(/[,\n]/)
      .map((p) => p.trim())
      .filter((p) => p.length > 0);
   * Toggles the log viewer between normal and expanded (fullscreen) mode.
  toggleLogViewerExpand() {
    this.isLogViewerExpanded = !this.isLogViewerExpanded;
   * Toggles the start session dialog between normal and fullscreen mode.
  toggleStartDialogFullscreen() {
    this.isStartDialogFullscreen = !this.isStartDialogFullscreen;
   * Handles keyboard events for the component.
   * Closes expanded log viewer or confirmation dialog when Escape is pressed.
    if (this.showStopConfirmDialog) {
      this.cancelStopConfirm();
    } else if (this.isLogViewerExpanded) {
      this.isLogViewerExpanded = false;
    } else if (this.isStartDialogFullscreen) {
      this.isStartDialogFullscreen = false;
    } else if (this.showStartSessionDialog) {
   * Opens the confirmation dialog for stopping a single session.
   * @param session - The session to stop
   * @param event - Optional event to stop propagation
  openStopSessionConfirm(session: SqlLoggingSession, event?: Event) {
    this.sessionToStop = session;
    this.isStoppingAll = false;
    this.showStopConfirmDialog = true;
   * Opens the confirmation dialog for stopping all sessions.
  openStopAllSessionsConfirm() {
    this.sessionToStop = null;
    this.isStoppingAll = true;
   * Closes the stop confirmation dialog without taking action.
  cancelStopConfirm() {
    this.showStopConfirmDialog = false;
   * Confirms and executes the stop action (single session or all sessions).
  async confirmStopSession() {
    if (this.isStoppingAll) {
      await this.executeStopAllSessions();
    } else if (this.sessionToStop) {
      await this.executeStopSession(this.sessionToStop);
   * Debug method to test contextUser flow for SQL filtering.
   * This shows how the new architecture handles user context without storing email in provider.
  async debugUserEmail() {
        query DebugCurrentUserEmail {
          debugCurrentUserEmail
      const debugInfo = result?.debugCurrentUserEmail || 'No debug info returned';
      alert(`Context User Info:\n\n${debugInfo}\n\nThe system now passes user context through method calls instead of storing it in the provider.`);
      console.error('Error getting context user info:', error);
      alert(`Error: ${error.message || 'Failed to get debug info'}`);
