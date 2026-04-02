import { Component, OnInit, OnDestroy, ViewChild, ElementRef, HostListener, ChangeDetectorRef, Output, EventEmitter } from '@angular/core';
import { CommandPaletteService } from './command-palette.service';
 * Command Palette Component
 * Provides a Notion-style command palette for quickly searching and navigating to applications.
 * Triggered by Cmd+K (Mac) or Ctrl+/ (Windows/Linux).
 * - Fuzzy search with relevance scoring
 * - Keyboard navigation (arrow keys, enter, escape)
 * - Loading state during navigation
 * - Empty state with helpful message
  selector: 'mj-command-palette',
  templateUrl: './command-palette.component.html',
  styleUrls: ['./command-palette.component.css']
export class CommandPaletteComponent implements OnInit, OnDestroy {
  @ViewChild('searchInput') SearchInput!: ElementRef<HTMLInputElement>;
  @Output() AppSelected = new EventEmitter<string>();
  IsOpen = false;
  SearchQuery = '';
  AllApps: BaseApplication[] = [];
  FilteredApps: BaseApplication[] = [];
  SelectedIndex = 0;
  IsNavigating = false;
    private service: CommandPaletteService,
    // Subscribe to service's open/close state
    this.service.IsOpen.pipe(takeUntil(this.destroy$)).subscribe((isOpen) => {
        this.onOpen();
    // Subscribe to application changes
    this.appManager.AllApplications.pipe(takeUntil(this.destroy$)).subscribe((apps) => {
      this.AllApps = apps;
      this.filterAndSortApps();
   * Called when command palette opens
  private onOpen(): void {
    this.SearchQuery = '';
    this.IsNavigating = false;
    // Filter apps (will show all initially)
    // Focus search input after view updates
      if (this.SearchInput) {
        this.SearchInput.nativeElement.focus();
   * Called when command palette closes
  private onClose(): void {
   * Handle search query change
  OnSearchChange(): void {
    this.SelectedIndex = 0; // Reset selection
   * Filter and sort applications based on search query
  private filterAndSortApps(): void {
    const query = this.SearchQuery.toLowerCase().trim();
    // If no query, show all apps
      this.FilteredApps = [...this.AllApps];
    // Score all apps
    const scored = this.AllApps.map(app => ({
      score: this.calculateMatchScore(app, query)
    // Filter to only matches (score > 0)
    const matches = scored.filter(item => item.score > 0);
    // Sort by score (descending)
    matches.sort((a, b) => b.score - a.score);
    // Extract apps
    this.FilteredApps = matches.map(item => item.app);
   * Calculate match score for fuzzy search
   * Scoring:
   * - Exact match: 1000 points
   * - Starts with: 500 points
   * - Contains: 100 points
   * - Description match: 50 points
   * - Initials match: 25 points (e.g., "de" matches "Data Explorer")
  private calculateMatchScore(app: BaseApplication, query: string): number {
    const name = app.Name.toLowerCase();
    const desc = (app.Description || '').toLowerCase();
    // Exact match (highest priority)
    if (name === query) return 1000;
    // Starts with query (very high priority)
    if (name.startsWith(query)) return 500;
    // Contains query in name (high priority)
    if (name.includes(query)) return 100;
    // Description match (medium priority)
    if (desc.includes(query)) return 50;
    // Fuzzy match - initials (e.g., "de" matches "Data Explorer")
    const initials = name
      .map(word => word[0] || '')
      .join('')
      .toLowerCase();
    if (initials.includes(query)) return 25;
    return 0; // No match
   * Select an application and emit event for shell to handle navigation
  SelectApp(app: BaseApplication): void {
    if (this.IsNavigating) return;
    // Close the palette first
    this.service.Close();
    // Emit event for shell to handle navigation (same pattern as app-switcher)
    this.AppSelected.emit(app.ID);
   * Close the command palette
  Close(): void {
   * Handle keyboard navigation
  HandleKeyDown(event: KeyboardEvent): void {
    if (!this.IsOpen) return;
    // Skip if user is typing in input (except for navigation keys)
    const navigationKeys = ['Escape', 'Enter', 'ArrowUp', 'ArrowDown'];
    if (target.tagName === 'INPUT' && !navigationKeys.includes(event.key)) {
      case 'ArrowDown':
        this.SelectedIndex = Math.min(this.SelectedIndex + 1, this.FilteredApps.length - 1);
        this.scrollToSelected();
      case 'ArrowUp':
        this.SelectedIndex = Math.max(this.SelectedIndex - 1, 0);
      case 'Enter':
        if (this.FilteredApps[this.SelectedIndex]) {
          this.SelectApp(this.FilteredApps[this.SelectedIndex]);
      case 'Escape':
   * Ensure selected item is visible in results list
  private scrollToSelected(): void {
      const selected = document.querySelector('.result-item.selected');
      if (selected) {
        selected.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
