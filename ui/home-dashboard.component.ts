import { Component, AfterViewInit, OnDestroy, ChangeDetectorRef, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { BaseResourceComponent, NavigationService, RecentAccessService, RecentAccessItem } from '@memberjunction/ng-shared';
import { ResourceData, MJUserFavoriteEntity, MJUserNotificationEntity, UserInfoEngine } from '@memberjunction/core-entities';
import { ApplicationManager, BaseApplication } from '@memberjunction/ng-base-application';
import { UserAppConfigComponent } from '@memberjunction/ng-explorer-settings';
 * Cached app data with pre-computed values for optimal rendering performance
interface AppDisplayData {
  navItemsCount: number;
  navItemsPreview: { Label: string; Icon: string }[];
  showMoreItems: boolean;
  moreItemsCount: number;
 * Home Dashboard - Personalized home screen showing all available applications
 * with quick access navigation and configuration options.
 * Uses OnPush change detection and cached computed values for optimal performance.
 * Registered as a BaseResourceComponent so it can be used as a Custom resource type
 * in nav items, allowing users to return to the Home dashboard after viewing orphan resources.
  selector: 'mj-home-dashboard',
  templateUrl: './home-dashboard.component.html',
  styleUrls: ['./home-dashboard.component.css'],
@RegisterClass(BaseResourceComponent, 'HomeDashboard')
export class HomeDashboardComponent extends BaseResourceComponent implements AfterViewInit, OnDestroy {
  @ViewChild('appConfigDialog') appConfigDialog!: UserAppConfigComponent;
  public apps: BaseApplication[] = [];
  public appsDisplayData: AppDisplayData[] = []; // Pre-computed display data
  public currentUser: { Name: string; Email: string } | null = null;
  // Favorites
  public favorites: MJUserFavoriteEntity[] = [];
  public favoritesLoading = true;
  // Recents
  public recentItems: RecentAccessItem[] = [];
  public recentsLoading = true;
  // Notifications
  public unreadNotifications: MJUserNotificationEntity[] = [];
  public notificationsLoading = true;
  // Sidebar state - default closed on all screen sizes
  public sidebarOpen = false;
  // Cached icon lookups to avoid repeated method calls
  private favoriteIconCache = new Map<string, string>();
  private resourceIconCache = new Map<string, string>();
   * Check if sidebar has any content to show
  get hasSidebarContent(): boolean {
    return this.unreadNotifications.length > 0 ||
           this.favorites.length > 0 ||
           this.recentItems.length > 0 ||
           this.favoritesLoading ||
           this.recentsLoading;
   * Toggle sidebar visibility
  toggleSidebar(): void {
    this.sidebarOpen = !this.sidebarOpen;
   * Check if current device is mobile (width <= 768px)
  private isMobileDevice(): boolean {
    return typeof window !== 'undefined' && window.innerWidth <= 768;
    private appManager: ApplicationManager,
    return 'Home';
  async ngAfterViewInit(): Promise<void> {
    this.currentUser = {
      Name: this.metadata.CurrentUser?.Name || 'User',
      Email: this.metadata.CurrentUser?.Email || ''
    // Subscribe to loading state from ApplicationManager
    this.appManager.Loading
      .subscribe(loading => {
        // Only update isLoading if manager is actively loading
        // (we start with isLoading=true and only set to false when we have apps)
    // Subscribe to applications list, filtering out the Home app
    this.appManager.Applications
      .subscribe(async apps => {
        // Exclude the Home app from the list (users are already on Home)
        this.apps = apps.filter(app => app.Name !== 'Home');
        // Pre-compute display data for all apps
        await this.computeAppsDisplayData();
    // Subscribe to unread notifications
    MJNotificationService.Notifications$
      .subscribe(notifications => {
        this.unreadNotifications = notifications.filter(n => n.Unread).slice(0, 5);
        this.notificationsLoading = false;
    // Subscribe to recent items
    this.recentAccessService.RecentItems
      .subscribe(items => {
        this.recentItems = this.deduplicateRecents(items).slice(0, 5);
        this.recentsLoading = false;
    // Favorites and recents load asynchronously in the sidebar
    // Load favorites and recents asynchronously (don't block rendering)
    this.loadRecents();
   * Get a greeting based on time of day
  get greeting(): string {
    const hour = new Date().getHours();
    if (hour < 12) return 'Good morning';
    if (hour < 17) return 'Good afternoon';
    return 'Good evening';
   * Get formatted date string
  get formattedDate(): string {
    return new Date().toLocaleDateString('en-US', {
   * Navigate to an application
  async onAppClick(app: BaseApplication): Promise<void> {
    // Use NavigationService to switch to the app (handles tab creation if needed)
    await this.navigationService.SwitchToApp(app.ID);
   * Open app configuration dialog
  openConfigDialog(): void {
      if (this.appConfigDialog) {
        this.appConfigDialog.open();
   * Handle when config is saved
  onConfigSaved(): void {
   * Pre-compute display data for all apps to avoid repeated calculations during change detection
  private async computeAppsDisplayData(): Promise<void> {
    this.appsDisplayData = await Promise.all(this.apps.map(async app => {
      const navItems = await app.GetNavItems();
      const navItemsCount = navItems.length;
      const navItemsPreview = navItems.slice(0, 3).map(item => ({
        Label: item.Label,
        Icon: item.Icon || 'fa-solid fa-circle'
        color: app.GetColor() || '#1976d2',
        icon: app.Icon || 'fa-solid fa-cube',
        navItemsCount,
        navItemsPreview,
        showMoreItems: navItemsCount > 3,
        moreItemsCount: navItemsCount - 3
   * Track function for apps loop
  trackByApp(_index: number, item: AppDisplayData): string {
    return item.app.ID;
   * Track function for nav items preview
  trackByNavItem(_index: number, item: { Label: string; Icon: string }): string {
    return item.Label;
   * Load user favorites from UserInfoEngine (cached)
      this.favoritesLoading = true;
      // Get first 10 favorites (already ordered by __mj_CreatedAt DESC in engine)
      this.favorites = UserInfoEngine.Instance.UserFavorites.slice(0, 10);
      console.error('Error loading favorites:', error);
      this.favoritesLoading = false;
   * Load recent items via the RecentAccessService
  private async loadRecents(): Promise<void> {
      this.recentsLoading = true;
      await this.recentAccessService.loadRecentItems(10);
      console.error('Error loading recents:', error);
   * Navigate to a favorite item using NavigationService
  onFavoriteClick(favorite: MJUserFavoriteEntity): void {
    // Navigate based on entity type using NavigationService
    const entityName = favorite.Entity?.toLowerCase();
    const recordId = favorite.RecordID;
    if (entityName === 'dashboards') {
      this.navigationService.OpenDashboard(recordId, 'Dashboard');
    } else if (entityName === 'user views') {
      this.navigationService.OpenView(recordId, 'View');
    } else if (entityName === 'reports') {
      this.navigationService.OpenReport(recordId, 'Report');
    } else if (entityName?.includes('artifact')) {
      this.navigationService.OpenArtifact(recordId, 'Artifact');
      // Default: navigate to record
      compositeKey.LoadFromSingleKeyValuePair('ID', recordId);
      this.navigationService.OpenEntityRecord(favorite.Entity, compositeKey);
   * Navigate to a recent item using NavigationService
  onRecentClick(item: RecentAccessItem): void {
    // Use recordName if available, otherwise fall back to generic titles
    const name = item.recordName;
    switch (item.resourceType) {
        this.navigationService.OpenView(item.recordId, name || 'View');
        this.navigationService.OpenDashboard(item.recordId, name || 'Dashboard');
      case 'artifact':
        this.navigationService.OpenArtifact(item.recordId, name || 'Artifact');
      case 'report':
        this.navigationService.OpenReport(item.recordId, name || 'Report');
        // Regular record
        compositeKey.LoadFromSingleKeyValuePair('ID', item.recordId);
        this.navigationService.OpenEntityRecord(item.entityName, compositeKey);
   * Navigate to a notification using NavigationService
  onNotificationClick(notification: MJUserNotificationEntity): void {
    // Navigate to the notifications view using NavigationService
    this.navigationService.OpenDynamicView('MJ: User Notifications');
   * Get icon for a resource type (cached)
  getResourceIcon(resourceType: string): string {
    if (this.resourceIconCache.has(resourceType)) {
      return this.resourceIconCache.get(resourceType)!;
    switch (resourceType) {
        icon = 'fa-solid fa-table';
        icon = 'fa-solid fa-gauge-high';
        icon = 'fa-solid fa-cube';
        icon = 'fa-solid fa-chart-bar';
        icon = 'fa-solid fa-file';
    this.resourceIconCache.set(resourceType, icon);
   * Get icon for a favorite based on its entity type (cached)
  getFavoriteIcon(favorite: MJUserFavoriteEntity): string {
    const cacheKey = favorite.ID;
    if (this.favoriteIconCache.has(cacheKey)) {
      return this.favoriteIconCache.get(cacheKey)!;
      icon = 'fa-solid fa-star';
    this.favoriteIconCache.set(cacheKey, icon);
   * Format a date for display (pure function, safe to call in template)
  formatDate(date: Date): string {
   * Track function for favorites
  trackByFavorite(_index: number, item: MJUserFavoriteEntity): string {
    return item.ID;
   * Remove duplicate recent items (same entity + recordId). Keeps the first occurrence.
  private deduplicateRecents(items: RecentAccessItem[]): RecentAccessItem[] {
    const seen = new Set<string>();
    return items.filter(item => {
      const key = `${item.entityName}-${item.recordId}`;
      if (seen.has(key)) {
      seen.add(key);
   * Track function for recent items
  trackByRecent(_index: number, item: RecentAccessItem): string {
    return `${item.entityName}-${item.recordId}`;
   * Track function for notifications
  trackByNotification(_index: number, item: MJUserNotificationEntity): string {
