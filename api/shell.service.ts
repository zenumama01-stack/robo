import { IApp, TabRequest, TabState } from '../models/app.interface';
import { StorageService } from './storage.service';
 * Shell service - coordinates between apps and manages tab state
export class ShellService {
  private apps = new Map<string, IApp>();
  private activeApp$ = new BehaviorSubject<IApp | null>(null);
  private tabs$ = new BehaviorSubject<TabState[]>([]);
  private activeTabId$ = new BehaviorSubject<string | null>(null);
    private storage: StorageService
    this.loadTabsFromStorage();
  // App Registration
  RegisterApp(app: IApp): void {
    this.apps.set(app.Id, app);
  GetApp(appId: string): IApp | null {
    return this.apps.get(appId) || null;
  SetActiveApp(appId: string): void {
    const app = this.apps.get(appId);
      const previousApp = this.activeApp$.value;
      this.activeApp$.next(app);
      // Check if there are any tabs for this app
      const appTabs = this.tabs$.value.filter(t => t.AppId === appId);
      // If no tabs exist for this app, open the default tab
        const navItems = app.GetNavItems();
        const defaultRoute = navItems[0]?.Route || `/${appId}`;
        const defaultTitle = navItems[0]?.Label || app.Name;
        // Check if switching from another app with a single temporary tab - replace it
        if (previousApp && previousApp.Id !== appId) {
          const previousAppTabs = this.tabs$.value.filter(t => t.AppId === previousApp.Id);
          if (previousAppTabs.length === 1 && !previousAppTabs[0].IsPermanent) {
            // Replace the temporary tab from the previous app
            const tempTab = previousAppTabs[0];
            tempTab.AppId = appId;
            tempTab.Title = defaultTitle;
            tempTab.Route = defaultRoute;
            tempTab.Color = app.Color;
            this.tabs$.next([...this.tabs$.value]);
            this.SetActiveTab(tempTab.Id);
            this.saveTabsToStorage();
          AppId: appId,
          Title: defaultTitle,
          Route: defaultRoute
        // Router navigation will be handled by OpenTab's activeTabId change
        // Find the last active tab for this app, or use the first one
        const lastActiveTab = appTabs[appTabs.length - 1];
        this.SetActiveTab(lastActiveTab.Id);
        // Router navigation will be handled by tab activation in tab-container
  GetActiveApp(): Observable<IApp | null> {
  GetAllApps(): IApp[] {
    return Array.from(this.apps.values());
  // Tab Management
  OpenTab(request: TabRequest): string {
    const tabId = this.generateTabId();
    const app = this.apps.get(request.AppId);
    const tab: TabState = {
      Id: tabId,
      AppId: request.AppId,
      Title: request.Title,
      Route: request.Route,
      Data: request.Data,
      IsPermanent: false, // New tabs start as temporary
      Color: app?.Color // App signature color for visual identification
    const currentTabs = this.tabs$.value;
    currentTabs.push(tab);
    this.tabs$.next(currentTabs);
    this.activeTabId$.next(tabId);
    const filtered = currentTabs.filter(t => t.Id !== tabId);
    this.tabs$.next(filtered);
    // If closing active tab, activate the last tab
    if (this.activeTabId$.value === tabId && filtered.length > 0) {
      this.activeTabId$.next(filtered[filtered.length - 1].Id);
    } else if (filtered.length === 0) {
      this.activeTabId$.next(null);
  GetTabs(): Observable<TabState[]> {
    return this.tabs$.asObservable();
  GetActiveTabId(): Observable<string | null> {
    return this.activeTabId$.asObservable();
  HasTabs(): boolean {
    return this.tabs$.value.length > 0;
  // Navigation - VSCode-style behavior
  Navigate(route: string): void {
    const activeTabId = this.activeTabId$.value;
    const activeApp = this.activeApp$.value;
    if (!activeApp) {
      this.router.navigate([route]);
    // Determine which app this route belongs to
    const targetAppId = this.getAppIdForRoute(route);
    // If route belongs to a different app, switch apps
    if (targetAppId && targetAppId !== activeApp.Id) {
      this.SetActiveApp(targetAppId);
    // Check if route is already open in an existing tab
    const existingTab = currentTabs.find(t => t.Route === route && t.AppId === activeApp.Id);
      // Just activate the existing tab - router will be updated by tab-container subscription
      this.SetActiveTab(existingTab.Id);
    // Find the current active tab for this app
    const activeTab = currentTabs.find(t => t.Id === activeTabId && t.AppId === activeApp.Id);
    // If active tab is temporary (not permanent), replace its content
    // This ensures one temporary tab per app, not globally
    if (activeTab && !activeTab.IsPermanent) {
      activeTab.Route = route;
      activeTab.Title = this.getTitleFromRoute(route);
      this.tabs$.next([...currentTabs]); // Trigger update which will recreate content
      // Navigate directly since we're just updating the current tab's content
      // Active tab is permanent or no active tab, open new temporary tab
      // OpenTab will set activeTabId which triggers router navigation in tab-container
        AppId: activeApp.Id,
        Title: this.getTitleFromRoute(route),
        Route: route
  // Helper to determine which app a route belongs to
  private getAppIdForRoute(route: string): string | null {
    if (route.startsWith('/conversations')) return 'conversations';
    if (route.startsWith('/settings')) return 'settings';
    if (route.startsWith('/crm')) return 'crm';
  // Toggle tab between temporary and permanent
  ToggleTabPermanent(tabId: string): void {
    const tab = currentTabs.find(t => t.Id === tabId);
      tab.IsPermanent = !tab.IsPermanent;
      this.tabs$.next([...currentTabs]);
  // Helper to extract title from route
  private getTitleFromRoute(route: string): string {
    // Extract the last segment and capitalize
    const lastSegment = segments[segments.length - 1] || 'Home';
    return lastSegment.charAt(0).toUpperCase() + lastSegment.slice(1);
  // Search handling
  HandleSearch(query: string): void {
    if (activeApp?.CanHandleSearch()) {
      activeApp.OnSearchRequested(query);
      // Default global search
      console.log('Global search:', query);
  private generateTabId(): string {
    return `tab_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  private saveTabsToStorage(): void {
    this.storage.Save('tabs', this.tabs$.value);
    this.storage.Save('activeTabId', this.activeTabId$.value);
  private loadTabsFromStorage(): void {
    const tabs = this.storage.Load<TabState[]>('tabs') || [];
    const activeTabId = this.storage.Load<string>('activeTabId');
    this.tabs$.next(tabs);
    if (activeTabId && tabs.some(t => t.Id === activeTabId)) {
      this.activeTabId$.next(activeTabId);
  // Golden Layout state persistence
  SaveLayoutConfig(config: any): void {
    this.storage.Save('layoutConfig', config);
  LoadLayoutConfig(): any | null {
    return this.storage.Load<any>('layoutConfig');
