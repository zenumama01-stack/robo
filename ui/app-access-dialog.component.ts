import { Component, Input, Output, EventEmitter, OnDestroy, ChangeDetectorRef, HostListener } from '@angular/core';
 * Type of app access issue
export type AppAccessDialogType =
  | 'not_installed'    // User can install the app
  | 'disabled'         // User has disabled the app, can re-enable
  | 'no_access'        // User doesn't have permission
  | 'not_found'        // App doesn't exist
  | 'inactive'         // App is inactive/disabled by admin
  | 'no_apps'          // User has no apps configured at all
  | 'layout_error';    // Golden Layout failed to initialize
 * Configuration for the app access dialog
export interface AppAccessDialogConfig {
  type: AppAccessDialogType;
  appName?: string;
 * Result from the dialog
export interface AppAccessDialogResult {
  action: 'install' | 'enable' | 'redirect' | 'dismissed';
 * Dialog component for handling app access errors.
 * Shows appropriate messages and actions based on the type of access issue.
 * Features auto-dismiss with countdown timer for certain dialog types.
  selector: 'mj-app-access-dialog',
  templateUrl: './app-access-dialog.component.html',
  styleUrls: ['./app-access-dialog.component.css']
export class AppAccessDialogComponent implements OnDestroy {
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() result = new EventEmitter<AppAccessDialogResult>();
  config: AppAccessDialogConfig | null = null;
  // Auto-dismiss countdown
  private countdownInterval: ReturnType<typeof setInterval> | null = null;
  countdownSeconds = 0;
  private readonly AUTO_DISMISS_SECONDS = 5;
   * Show the dialog with the specified configuration
  show(config: AppAccessDialogConfig): void {
    this.visible = true;
    this.visibleChange.emit(true);
    // Start countdown for types that auto-dismiss
    if (this.shouldAutoDismiss()) {
      this.startCountdown();
   * Hide the dialog
  hide(): void {
    this.stopCountdown();
    this.visible = false;
    this.visibleChange.emit(false);
   * Get the dialog title based on type
  get title(): string {
    if (!this.config) return '';
    switch (this.config.type) {
        return 'Add Application?';
      case 'no_access':
        return 'Access Denied';
        return 'Application Not Found';
        return 'Application Unavailable';
      case 'no_apps':
        return 'No Applications Available';
      case 'layout_error':
        return 'Display Error';
        return 'Application Error';
   * Get the dialog icon based on type
  get icon(): string {
    if (!this.config) return 'fa-circle-info';
        return 'fa-circle-question';
        return 'fa-lock';
        return 'fa-circle-xmark';
        return 'fa-circle-pause';
        return 'fa-folder-open';
        return 'fa-triangle-exclamation';
        return 'fa-circle-info';
   * Get the dialog icon color based on type
  get iconColor(): string {
    if (!this.config) return '#666';
        return '#2196F3'; // Blue for actionable
        return '#FF9800'; // Orange for warning
        return '#F44336'; // Red for error
        return '#9E9E9E'; // Gray for info
   * Get the main message based on type
  get message(): string {
    const appName = this.config.appName || 'this application';
        return `Would you like to add "${appName}" to your applications?`;
        return `You don't have permission to access "${appName}".`;
        return `The application "${appName}" doesn't exist in this system.`;
        return `The application "${appName}" is currently inactive and unavailable.`;
        return `You don't have any applications configured. Your system administrator needs to set up your application access.`;
        return `There was an error displaying the application interface. The system will redirect you to an available application.`;
        return 'An error occurred while loading the application.';
   * Get the secondary/help message based on type
  get helpMessage(): string {
        return 'If you believe this is an error, please contact your system administrator.';
        return 'Please contact your system administrator to configure your application access.';
        return 'If this error persists, try clearing your browser cache or contact your system administrator.';
   * Check if the primary action button should be shown
  get showPrimaryAction(): boolean {
    if (!this.config) return false;
    return this.config.type === 'not_installed' || this.config.type === 'disabled';
   * Get the primary action button text
  get primaryActionText(): string {
        return 'Add';
        return 'OK';
   * Get the secondary/dismiss button text with countdown if applicable
   * For actionable dialogs (install/enable), show "Cancel"
   * For non-actionable dialogs (errors), show "OK" with countdown
  get dismissButtonText(): string {
    // For actionable dialogs, use "Cancel"
    if (this.showPrimaryAction) {
      return 'Cancel';
    // For non-actionable dialogs, show countdown if active
    if (this.countdownSeconds > 0) {
      return `OK (${this.countdownSeconds})`;
   * Check if this dialog type should auto-dismiss
  private shouldAutoDismiss(): boolean {
    return ['no_access', 'not_found', 'inactive', 'layout_error'].includes(this.config.type);
   * Start the countdown timer for auto-dismiss
  private startCountdown(): void {
    this.countdownSeconds = this.AUTO_DISMISS_SECONDS;
    this.countdownInterval = setInterval(() => {
      this.countdownSeconds--;
      if (this.countdownSeconds <= 0) {
        this.onDismiss();
   * Stop the countdown timer
  private stopCountdown(): void {
    if (this.countdownInterval) {
      clearInterval(this.countdownInterval);
      this.countdownInterval = null;
    this.countdownSeconds = 0;
   * Handle primary action (install/enable)
  async onPrimaryAction(): Promise<void> {
    if (!this.config) return;
    const action = this.config.type === 'not_installed' ? 'install' : 'enable';
      appId: this.config.appId
    // Don't hide yet - let the parent component handle the result and close when ready
   * Handle dismiss/redirect action
  onDismiss(): void {
    this.result.emit({ action: 'redirect' });
    this.hide();
   * Mark processing as complete (called by parent after install/enable)
  completeProcessing(): void {
   * Handle keyboard events for the dialog
   * Enter key triggers primary action (Install/Enable)
   * Escape key dismisses the dialog
  handleKeyDown(event: KeyboardEvent): void {
    if (!this.visible || this.isProcessing) return;
    if (event.key === 'Enter') {
      // Enter triggers primary action if available
        this.onPrimaryAction();
    } else if (event.key === 'Escape') {
      // Escape dismisses the dialog
