import { BaseApplication, DynamicNavItem, NavItem, WorkspaceStateManager, WorkspaceConfiguration } from '@memberjunction/ng-base-application';
 * Event emitted when a nav item is clicked
export interface NavItemClickEvent {
  item: NavItem;
  shiftKey: boolean;
 * Horizontal navigation items for the current app.
 * Uses OnPush change detection and reactive state management for optimal performance.
  selector: 'mj-app-nav',
  templateUrl: './app-nav.component.html',
  styleUrls: ['./app-nav.component.css'],
export class AppNavComponent implements OnInit, OnDestroy {
  private _app: BaseApplication | null = null;
  private _cachedNavItems: NavItem[] = [];
  private _cachedAppColor: string = '#1976d2';
  private _servicesInjected = false;
   * Monotonically increasing counter used to detect and discard stale async results.
   * Because GetNavItems() is async (HomeApplication does a DB lookup for record names),
   * and RxJS subscribe() does NOT serialize async callbacks, multiple calls to
   * updateCachedData() can overlap. Without this guard, a slow call (e.g., Home app
   * doing a DB lookup) that started BEFORE a fast call (e.g., switching to App B)
   * could resolve AFTER the fast call and overwrite the correct nav items with stale ones.
   * How it works:
   *   1. Each updateCachedData() call increments this counter and captures it as `gen`
   *   2. After the await, it checks: does `gen` still match `_updateGeneration`?
   *   3. If not, a newer call started while we were waiting — discard our stale results
  private _updateGeneration = 0;
  // Map of nav item key (Route or Label) to active state
  private activeStateMap = new Map<string, boolean>();
  @Output() navItemClick = new EventEmitter<NavItemClickEvent>();
  @Output() navItemDismiss = new EventEmitter<NavItem>();
   * Input setter for app - triggers cache update when app changes
  set app(value: BaseApplication | null) {
    if (this._app !== value) {
      this._app = value;
      this._cachedNavItems = []; // Clear stale items immediately so previous app's items don't flash
      this.activeStateMap.clear();
      this._servicesInjected = false; // Reset injection flag
      this.updateCachedData();
  get app(): BaseApplication | null {
    return this._app;
    // Subscribe to workspace configuration changes.
    // Must rebuild nav items (not just active states) because dynamic nav items
    // are generated based on the currently active tab - when a user navigates
    // from one record to another (e.g., via OpenEntityRecord), the active tab
    // changes and the dynamic nav item needs to reflect the new record.
    this.workspaceManager.Configuration
      .subscribe(async () => {
        await this.updateCachedData();
   * Update cached nav items and app color when app changes
  private async updateCachedData(): Promise<void> {
    // Capture the current generation before any async work.
    // See _updateGeneration JSDoc for full explanation of the race condition this prevents.
    const gen = ++this._updateGeneration;
    if (this._app) {
      // Inject services once for apps that need them (e.g., HomeApplication for dynamic nav items)
      if (!this._servicesInjected) {
        const appWithServices = this._app as BaseApplication & {
          SetWorkspaceManager?: (manager: WorkspaceStateManager) => void;
          SetSharedService?: (service: SharedService) => void;
        if (typeof appWithServices.SetWorkspaceManager === 'function') {
          appWithServices.SetWorkspaceManager(this.workspaceManager);
        if (typeof appWithServices.SetSharedService === 'function') {
          appWithServices.SetSharedService(this.sharedService);
        this._servicesInjected = true;
      const items = await this._app.GetNavItems() || [];
      // If a newer call started while we were awaiting, our results are stale — bail out
      // so we don't overwrite the newer call's (correct) results.
      if (gen !== this._updateGeneration) {
      // Only show items with Status 'Active' or undefined (default to Active)
      this._cachedNavItems = items.filter(item => !item.Status || item.Status === 'Active');
      this._cachedAppColor = this._app.GetColor() || '#1976d2';
      this._cachedNavItems = [];
      this._cachedAppColor = '#1976d2';
    // Update active states after nav items change
    this.updateActiveStates(config);
   * Update active state map based on current workspace configuration
  private updateActiveStates(config: WorkspaceConfiguration | null): void {
    if (!config || !this._app) {
    if (!activeTab || activeTab.applicationId !== this._app.ID) {
    // Compute active state for each nav item once
    for (const item of this._cachedNavItems) {
      const key = this.getItemKey(item);
      const isActive = this.computeIsActive(item, activeTab);
      this.activeStateMap.set(key, isActive);
   * Get unique key for nav item (used for tracking and active state).
   * Prefers RecordID for dynamic items to avoid label collisions.
  private getItemKey(item: NavItem): string {
    return item.RecordID || item.Route || item.Label || '';
   * Check if a nav item is dynamic (generated from recent orphan resources)
  isDynamic(item: NavItem): boolean {
    return (item as DynamicNavItem).isDynamic === true;
   * Compute if nav item is active based on active tab
  private computeIsActive(item: NavItem, activeTab: any): boolean {
    // Check if nav item has a custom matching function (for dynamic items)
    const dynamicItem = item as NavItem & { isActiveMatch?: (tab: unknown) => boolean };
    if (dynamicItem.isActiveMatch && typeof dynamicItem.isActiveMatch === 'function') {
      return dynamicItem.isActiveMatch(activeTab);
    // Standard matching: route or label
    return (item.Route && activeTab.configuration['route'] === item.Route) ||
           activeTab.title === item.Label;
   * Get cached navigation items (no computation in getter)
  get navItems(): NavItem[] {
    return this._cachedNavItems;
   * Get cached app color (no computation in getter)
  get appColor(): string {
    return this._cachedAppColor;
   * Check if nav item is active (uses cached state from Map)
  isActive(item: NavItem): boolean {
    return this.activeStateMap.get(key) || false;
   * Track function for @for to optimize rendering
  trackByNavItem(_index: number, item: NavItem): string {
    return this.getItemKey(item);
   * Handle nav item click
  onNavClick(item: NavItem, event?: MouseEvent): void {
    this.navItemClick.emit({
      shiftKey: event?.shiftKey || false
   * Handle dismiss click on a dynamic nav item.
   * Removes from the app's recent stack and refreshes nav items immediately.
   * Stops propagation so the nav click handler doesn't fire.
  onDismiss(item: NavItem, event: MouseEvent): void {
    // Remove from the app's recent stack directly so we can refresh immediately
      const appWithRemove = this._app as BaseApplication & {
        RemoveDynamicNavItem?: (navItem: NavItem) => void;
    this.navItemDismiss.emit(item);
