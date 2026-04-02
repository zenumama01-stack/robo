import { Directive, HostListener, Output, EventEmitter } from '@angular/core';
 * Directive to handle global search keyboard shortcut (Ctrl+K or Cmd+K)
 * Usage: Add to your main app component or workspace component
 * <div mjSearchShortcut (searchTriggered)="openSearch()">
  selector: '[mjSearchShortcut]'
export class SearchShortcutDirective {
   * Listen for Ctrl+K or Cmd+K
  handleKeyboardEvent(event: KeyboardEvent): void {
    // Check for Ctrl+K or Cmd+K
    if (isCtrlOrCmd && event.key === 'k') {
      this.searchTriggered.emit();
