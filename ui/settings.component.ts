import { Component, OnInit, OnDestroy, EventEmitter, Output, ChangeDetectorRef, NgZone } from '@angular/core';
import { Location } from '@angular/common';
import { BaseNavigationComponent } from '@memberjunction/ng-shared';
export interface SettingsTab {
  component?: unknown;
  badgeCount?: number;
  badgeColor?: 'primary' | 'danger' | 'warning' | 'success';
  disabled?: boolean;
export interface SettingsComponentState {
  expandedSections: string[];
export interface SearchableItem {
  sectionId?: string;
  keywords: string[];
  selector: 'mj-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.css']
@RegisterClass(BaseNavigationComponent, 'Settings')
export class SettingsComponent extends BaseNavigationComponent implements OnInit, OnDestroy {
  @Output() stateChange = new EventEmitter<SettingsComponentState>();
  public activeTab = 'general';
  public filteredTabs: SettingsTab[] = [];
  public searchResults: SearchableItem[] = [];
  public isSearching = false;
  public showSearchResults = false;
  // Tab configuration - User-focused tabs only
  public tabs: SettingsTab[] = [
      id: 'general',
      label: 'General',
      icon: 'fa-solid fa-user',
      description: 'Profile and account information'
      id: 'notifications',
      label: 'Notifications',
      icon: 'fa-solid fa-bell',
      description: 'Notification preferences and delivery channels'
      id: 'applications',
      label: 'Applications',
      icon: 'fa-solid fa-th-large',
      description: 'Manage visible applications and ordering'
      id: 'appearance',
      label: 'Appearance',
      icon: 'fa-solid fa-palette',
      description: 'Theme and display settings',
      disabled: true
  // Searchable content registry - User settings only
  private searchableItems: SearchableItem[] = [
    // General tab
      tabId: 'general',
      sectionId: 'profile',
      label: 'Profile Information',
      keywords: ['profile', 'avatar', 'picture', 'photo', 'name', 'user', 'image', 'account'],
      description: 'Manage your profile picture and account information'
      id: 'account',
      sectionId: 'account',
      label: 'Account Information',
      keywords: ['account', 'email', 'role', 'login', 'status', 'type'],
      description: 'View your account details and status'
    // Notifications tab
      tabId: 'notifications',
      sectionId: 'notifications',
      label: 'Notification Preferences',
      keywords: ['notifications', 'alerts', 'email', 'sms', 'in-app', 'messages'],
      description: 'Configure how and when you receive notifications'
    // Applications tab
      tabId: 'applications',
      sectionId: 'applications',
      label: 'MJ: Application Settings',
      keywords: ['applications', 'apps', 'switcher', 'order', 'visibility', 'menu'],
      description: 'Choose which applications appear in your app switcher'
    // Appearance tab
      tabId: 'appearance',
      sectionId: 'appearance',
      label: 'Appearance Settings',
      keywords: ['appearance', 'theme', 'dark', 'light', 'display', 'font', 'density'],
      description: 'Customize how the application looks'
  public expandedSections: string[] = ['profile', 'account'];
  // Mobile state
  public isMobile = window.innerWidth < 768;
  public isMobileNavOpen = false;
  constructor(private location: Location, private cdr: ChangeDetectorRef, private ngZone: NgZone) {
    window.addEventListener('resize', this.handleResize.bind(this));
    this.filteredTabs = [...this.tabs];
    this.setupSearchFilter();
    window.removeEventListener('resize', this.handleResize.bind(this));
  private setupSearchFilter(): void {
    this.searchTerm$
        debounceTime(200),
      .subscribe(term => {
        this.filterContent(term);
      await this.simulateDataLoad();
        this.error = 'Failed to load settings data';
  private async simulateDataLoad(): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, 300));
    const tab = this.tabs.find(t => t.id === tabId);
    if (tab?.disabled) {
      return; // Don't switch to disabled tabs
    const term = (event.target as HTMLInputElement).value;
  public toggleSection(sectionId: string): void {
    const index = this.expandedSections.indexOf(sectionId);
    if (index === -1) {
      this.expandedSections.push(sectionId);
      this.expandedSections.splice(index, 1);
  public isSectionExpanded(sectionId: string): boolean {
    return this.expandedSections.includes(sectionId);
   * Filters searchable content based on search term
  private filterContent(term: string): void {
    if (!term || term.trim() === '') {
      this.showSearchResults = false;
      this.searchResults = [];
    const searchLower = term.toLowerCase().trim();
    this.isSearching = true;
    // Filter searchable items
    this.searchResults = this.searchableItems.filter(item => {
      const matchesLabel = item.label.toLowerCase().includes(searchLower);
      const matchesKeywords = item.keywords.some(kw => kw.toLowerCase().includes(searchLower));
      const matchesDescription = item.description?.toLowerCase().includes(searchLower) || false;
      return matchesLabel || matchesKeywords || matchesDescription;
    // Get unique tabs that have matching results
    const matchingTabIds = new Set(this.searchResults.map(r => r.tabId));
    this.filteredTabs = this.tabs.filter(tab => matchingTabIds.has(tab.id));
    this.showSearchResults = true;
    this.isSearching = false;
   * Navigates to a search result
  public navigateToSearchResult(result: SearchableItem): void {
    const tab = this.tabs.find(t => t.id === result.tabId);
      return; // Don't navigate to disabled tabs
    this.activeTab = result.tabId;
    // Expand the section if applicable
    if (result.sectionId && !this.expandedSections.includes(result.sectionId)) {
      this.expandedSections.push(result.sectionId);
    this.clearSearch();
   * Clears the search and resets the view
    this.searchTerm$.next('');
   * Toggles the mobile navigation rail
  public toggleMobileNav(): void {
    this.isMobileNavOpen = !this.isMobileNavOpen;
   * Closes the mobile navigation rail
  public closeMobileNav(): void {
    this.isMobileNavOpen = false;
   * Closes the settings page and navigates back
  public closeSettings(): void {
    this.location.back();
   * Handles tab change on mobile
  public onMobileTabChange(tabId: string): void {
    this.onTabChange(tabId);
    this.closeMobileNav();
  private handleResize(): void {
    this.isMobile = window.innerWidth < 768;
    if (!this.isMobile) {
    const state: SettingsComponentState = {
      searchTerm: this.searchTerm$.value,
      expandedSections: [...this.expandedSections]
    this.stateChange.emit(state);
  public loadUserState(state: Partial<SettingsComponentState>): void {
    if (state.searchTerm !== undefined) {
      this.searchTerm$.next(state.searchTerm);
    if (state.expandedSections) {
      this.expandedSections = [...state.expandedSections];
  public getTabIcon(tab: SettingsTab): string {
    return tab.icon;
  public getTabClass(tab: SettingsTab): string {
    const classes = ['settings-tab'];
    if (this.activeTab === tab.id) {
      classes.push('active');
    if (tab.disabled) {
      classes.push('disabled');
    if (tab.badgeCount && tab.badgeCount > 0) {
      classes.push('has-badge');
  public isTabDisabled(tab: SettingsTab): boolean {
    return tab.disabled || false;
  selector: 'app-settings',
  imports: [CommonModule, RouterModule],
    <div class="settings-container">
      <div class="settings-sidebar">
        <h3>User Preferences</h3>
        <nav class="settings-nav">
          <a routerLink="/settings/profile" routerLinkActive="active" class="nav-link">
            Profile
          <a routerLink="/settings/notifications" routerLinkActive="active" class="nav-link">
          <a routerLink="/settings/appearance" routerLinkActive="active" class="nav-link">
            Appearance
    .settings-sidebar {
    .nav-link {
        background: #eeeeee;
export class SettingsComponent {}
