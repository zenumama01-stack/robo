  ApplicationRef,
  EnvironmentInjector,
  createComponent,
  ComponentRef,
  ViewEncapsulation,
  HostListener,
  EventEmitter
  TabComponentState,
  TabShownEvent,
  LayoutNode
import { ResourceData, MJResourceTypeEntity } from '@memberjunction/core-entities';
import { DatasetResultType, LogError, Metadata } from '@memberjunction/core';
import { ComponentCacheManager } from './component-cache-manager';
 * Container for Golden Layout tabs with app-colored styling.
 * - Golden Layout initialization
 * - Tab creation and styling
 * - Context menu for pin/close
 * - Layout persistence
  selector: 'mj-tab-container',
  templateUrl: './tab-container.component.html',
  styleUrls: ['./tab-container.component.css'],
export class TabContainerComponent implements OnInit, OnDestroy, AfterViewInit {
  @ViewChild('glContainer', { static: false }) glContainer!: ElementRef<HTMLDivElement>;
  @ViewChild('directContentContainer', { static: false }) directContentContainer!: ElementRef<HTMLDivElement>;
   * Emitted when the first resource component finishes loading.
   * This allows the shell to keep showing its loading indicator until the first
   * resource is ready, eliminating the visual gap between shell loading and resource loading.
  @Output() firstResourceLoadComplete = new EventEmitter<void>();
   * Emitted when Golden Layout fails to initialize after multiple retries.
   * The shell can use this to show an error dialog and redirect.
  @Output() layoutInitError = new EventEmitter<void>();
  private layoutInitRetryCount = 0;
  private readonly MAX_LAYOUT_INIT_RETRIES = 5;
  private layoutInitialized = false;
  private layoutRestorationComplete = false; // True only AFTER layout is fully restored/created
  // Track component references for cleanup (legacy - keep for backward compat during transition)
  private componentRefs = new Map<string, ComponentRef<BaseResourceComponent>>();
  // NEW: Smart component cache for preserving state across tab switches
  private cacheManager: ComponentCacheManager;
  // Single-resource mode: render component directly without Golden Layout
  // This avoids the 20px height issue when GL header is hidden
  useSingleResourceMode = false;
  private singleResourceComponentRef: ComponentRef<BaseResourceComponent> | null = null;
  private previousTabBarVisible: boolean | null = null;
  private currentSingleResourceSignature: string | null = null; // Track loaded content signature to avoid unnecessary reloads
  private isCreatingInitialTabs = false; // Flag to prevent syncTabsWithConfiguration during initial tab creation
  contextMenuVisible = false;
  contextMenuTabId: string | null = null;
    private appRef: ApplicationRef,
    private environmentInjector: EnvironmentInjector,
    // Initialize component cache manager
    this.cacheManager = new ComponentCacheManager(this.appRef);
    // Subscribe to tab events
      this.layoutManager.TabShown.subscribe(event => {
        this.onTabShown(event);
      this.layoutManager.TabClosed.subscribe(tabId => {
        this.cleanupTabComponent(tabId);
        this.workspaceManager.CloseTab(tabId);
      this.layoutManager.LayoutChanged.subscribe(event => {
        const layout = this.layoutManager.SaveLayout();
        this.workspaceManager.UpdateLayout(layout);
      this.layoutManager.ActiveTab.subscribe(tabId => {
        if (tabId) {
          this.workspaceManager.SetActiveTab(tabId);
      this.layoutManager.TabDoubleClicked.subscribe(tabId => {
        this.workspaceManager.TogglePin(tabId);
      this.layoutManager.TabRightClicked.subscribe(event => {
        this.showContextMenu(event.x, event.y, event.tabId);
    // Subscribe to configuration changes to sync tabs
      this.workspaceManager.Configuration.subscribe(config => {
          if (this.useSingleResourceMode) {
            // In single-resource mode, reload content if the tab content changed
            // The same tab ID can have different content (tab gets reused)
            const activeTab = config.tabs.find(t => t.id === config.activeTabId) || config.tabs[0];
              const signature = this.getTabContentSignature(activeTab);
              if (signature !== this.currentSingleResourceSignature) {
                this.loadSingleResourceContent();
          } else if (this.layoutRestorationComplete && !this.isCreatingInitialTabs) {
            // In multi-tab mode, sync with Golden Layout
            // IMPORTANT: Only sync AFTER layout restoration is complete to avoid creating duplicate tabs
            // layoutRestorationComplete is set to true only after initializeGoldenLayout finishes
            this.syncTabsWithConfiguration(config.tabs);
    // Subscribe to tab bar visibility changes for single-resource mode
      this.workspaceManager.TabBarVisible.subscribe(tabBarVisible => {
        this.handleTabBarVisibilityChange(tabBarVisible);
    // Initialize Golden Layout only if we're not in single-resource mode
    if (!this.useSingleResourceMode) {
      this.initializeGoldenLayout();
      // In single-resource mode, load content directly
   * Initialize Golden Layout and load tabs
   * @param forceCreateTabs - If true, always creates tabs fresh from config.tabs instead of restoring saved layout
  private initializeGoldenLayout(forceCreateTabs = false): void {
    // If we are in single resource mode we do NOT need to do this work as golden layout should not exist in that state
    if (this.useSingleResourceMode)
    if (!this.glContainer?.nativeElement) {
      this.layoutInitRetryCount++;
      if (this.layoutInitRetryCount > this.MAX_LAYOUT_INIT_RETRIES) {
        console.error(`Golden Layout container not available after ${this.MAX_LAYOUT_INIT_RETRIES} retries, emitting error`);
        this.layoutInitError.emit();
      console.warn(`Golden Layout container not available, retry ${this.layoutInitRetryCount}/${this.MAX_LAYOUT_INIT_RETRIES}...`);
      setTimeout(() => this.initializeGoldenLayout(forceCreateTabs), 50);
    // Reset retry counter on success
    this.layoutInitRetryCount = 0;
    if (this.layoutInitialized) {
      return; // Already initialized
    // Check if configuration is available
    // If not, wait for it to be loaded before proceeding
      // Configuration not loaded yet - wait for it
      const configSub = this.workspaceManager.Configuration.subscribe(loadedConfig => {
        if (loadedConfig) {
          configSub.unsubscribe();
          // Re-call initializeGoldenLayout now that config is available
          this.initializeGoldenLayout(forceCreateTabs);
    // Initialize Golden Layout (we have config now)
    this.layoutManager.Initialize(this.glContainer.nativeElement);
    // Mark layout as initialized
    this.layoutInitialized = true;
    // Check if config has no tabs
    if (config.tabs.length === 0) {
      // No tabs to load, but mark restoration as complete
      this.layoutRestorationComplete = true;
    // Check if we have a saved layout structure with actual content
    const hasSavedLayout = config.layout?.root?.content && config.layout.root.content.length > 0;
    if (hasSavedLayout && !forceCreateTabs && config.layout) {
      // VALIDATE: Check that layout component count matches tabs array count
      const layoutComponentCount = this.countLayoutComponents(config.layout.root);
      if (layoutComponentCount !== config.tabs.length) {
        console.warn(`[TabContainer.initializeGoldenLayout] Layout/tabs mismatch: layout has ${layoutComponentCount} components but tabs array has ${config.tabs.length} tabs. Clearing layout.`);
        this.workspaceManager.ClearLayout();
        // Fall through to create fresh tabs
        // RESTORE SAVED LAYOUT - preserves drag/drop arrangements (stacks, columns, rows)
        // This is the single source of truth for visual arrangement
        const layoutLoaded = this.layoutManager.LoadLayout(config.layout);
        if (layoutLoaded) {
          // Mark layout restoration as complete AFTER layout is loaded
          // Focus active tab and ensure proper sizing
          // Also trigger updateSize() to force Golden Layout to fire 'show' events
          // for the active tab in ALL stacks (not just the globally active tab)
            if (config.activeTabId) {
              this.layoutManager.FocusTab(config.activeTabId);
            // Trigger resize to ensure all visible tabs in all stacks render their content
            this.layoutManager.updateSize();
          return; // Layout restored successfully
        // Layout load FAILED - clear the corrupted layout and fall through to create tabs fresh
        console.warn('[TabContainer] Saved layout was corrupted, clearing and recreating tabs');
    // CREATE FRESH - no saved layout, forceCreateTabs=true, or layout load failed
    // Use config.tabs sorted by sequence to build a simple single-stack layout
    const sortedTabs = [...config.tabs].sort((a, b) => a.sequence - b.sequence);
    this.isCreatingInitialTabs = true;
      sortedTabs.forEach(tab => {
        this.createTab(tab);
      this.isCreatingInitialTabs = false;
    // Mark layout restoration as complete AFTER tabs are created
    // Cleanup single-resource mode component if exists
    this.cleanupSingleResourceComponent();
    // Clear the component cache (destroys all components)
    this.cacheManager.clearCache();
    // Cleanup any legacy componentRefs
    this.componentRefs.forEach((ref, _tabId) => {
      this.appRef.detachView(ref.hostView);
      ref.destroy();
    this.componentRefs.clear();
   * Handle window resize events as a fallback safety mechanism.
   * Golden Layout's ResizeObserver should handle most cases, but this
   * ensures the layout is properly sized after browser window changes.
    if (this.layoutInitialized && !this.useSingleResourceMode) {
   * Handle changes to tab bar visibility - switches between single-resource and multi-tab modes
  private handleTabBarVisibilityChange(tabBarVisible: boolean): void {
    // Skip if no change
    if (this.previousTabBarVisible === tabBarVisible) {
    this.previousTabBarVisible = tabBarVisible;
    // Determine if we should use single-resource mode
    const shouldUseSingleResourceMode = !tabBarVisible;
    if (shouldUseSingleResourceMode !== this.useSingleResourceMode) {
      this.useSingleResourceMode = shouldUseSingleResourceMode;
        // Transitioning to single-resource mode
        // **CRITICAL FIX**: Wait for the template to render directContentContainer
        // before trying to load content. detectChanges() only marks dirty, doesn't render immediately.
          // First, destroy Golden Layout if it was initialized (prevents stale state)
            this.layoutInitialized = false;
          // Load the active tab's content directly (now container will exist)
        // Transitioning to multi-tab mode
        // Pin the previously displayed tab (it was the "current" content in single-resource mode)
        // This ensures we only have ONE temporary tab at a time
        if (config && config.tabs.length > 0) {
          // The new tab (just added via OpenTabForced) is now the activeTabId
          // All OTHER unpinned tabs should be pinned since they represent content
          // the user explicitly kept open
            // Pin all tabs except the newly active one (which is the temporary tab)
            if (tab.id !== config.activeTabId && !tab.isPinned) {
              return { ...tab, isPinned: true };
          // Only update if we actually changed something
          const hasChanges = updatedTabs.some((tab, i) => tab.isPinned !== config.tabs[i].isPinned);
          if (hasChanges) {
            this.workspaceManager.UpdateConfiguration({
        // Clean up direct component, Golden Layout will handle tabs
        this.currentSingleResourceSignature = null; // Reset tracking
        // Reset layout initialized flag since we're switching from single-resource mode
        // The gl-container is a new DOM element (due to @if), so we need fresh initialization
        // Initialize Golden Layout - use setTimeout to allow the template to update first
        // and ensure the gl-container div exists in the DOM
        // IMPORTANT: Use forceCreateTabs=true to create tabs fresh from config.tabs
        // instead of restoring potentially stale saved layout structure
          this.initializeGoldenLayout(true /* forceCreateTabs */);
   * Load content directly for single-resource mode (bypasses Golden Layout)
  private async loadSingleResourceContent(): Promise<void> {
    // Wait for next tick to ensure the container is rendered
    if (!config || config.tabs.length === 0) {
    // Get the active tab (or first tab)
    // Track which content we're loading (signature includes resource type and record ID)
    const newSignature = this.getTabContentSignature(activeTab);
    if (this.currentSingleResourceSignature === newSignature) {
      // Content already loaded, no action needed
    this.currentSingleResourceSignature = newSignature;
    // Get the container element
    const container = this.directContentContainer?.nativeElement;
      console.warn('Direct content container not available yet, retrying...');
      // Retry after view is updated
      setTimeout(() => this.loadSingleResourceContent(), 50);
    // Create ResourceData from tab
    const resourceData = await this.getResourceDataFromTab(activeTab);
    if (!resourceData) {
      LogError(`Unable to create ResourceData for tab: ${activeTab.title}`);
    // Get driver class for component lookup
    const driverClass = resourceData.Configuration?.resourceTypeDriverClass || resourceData.ResourceType;
    // **OPTIMIZATION: Check cache first to reuse existing loaded component**
    const cached = this.cacheManager.getCachedComponent(
      activeTab.applicationId
      // Clean up previous single-resource component (if different)
      // Detach from tab tracking (it was attached to a tab in Golden Layout)
      this.cacheManager.markAsDetached(activeTab.id);
      // Reattach the cached wrapper element to single-resource container
      cached.wrapperElement.style.height = "100%"; // Ensure full height
      container.appendChild(cached.wrapperElement);
      // Store reference for cleanup
      this.singleResourceComponentRef = cached.componentRef;
    // Get the component registration
    const resourceReg = MJGlobal.Instance.ClassFactory.GetRegistration(
      BaseResourceComponent,
      LogError(`Unable to find resource registration for driver class: ${driverClass}`);
    // Clean up previous component if any
    // Create the component dynamically
    const componentRef = createComponent(resourceReg.SubClass, {
      environmentInjector: this.environmentInjector
    // Attach to Angular's change detection
    this.appRef.attachView(componentRef.hostView);
    // Set the resource data on the component
    const instance = componentRef.instance as BaseResourceComponent;
    instance.Data = resourceData;
    // Wire up events
      this.emitFirstLoadCompleteOnce();
    // Get the native element and append to container
    const nativeElement = (componentRef.hostView as unknown as { rootNodes: HTMLElement[] }).rootNodes[0];
    container.appendChild(nativeElement);
    // now make sure that the container's direct child is 100% height
    if (container.children?.length > 0) {
      (container.children[0] as any).style.height = "100%";
    this.singleResourceComponentRef = componentRef as ComponentRef<BaseResourceComponent>;
   * Clean up single-resource mode component
  private cleanupSingleResourceComponent(): void {
    if (this.singleResourceComponentRef) {
      this.appRef.detachView(this.singleResourceComponentRef.hostView);
      this.singleResourceComponentRef.destroy();
      this.singleResourceComponentRef = null;
    // Clear the container
   * Generate a signature for tab content to detect when content changes
   * This is needed because in single-resource mode, the same tab ID can have different content
  private getTabContentSignature(tab: WorkspaceTab): string {
    // Include key identifying fields that determine what component/content is shown
    // IMPORTANT: Check both resourceRecordId AND configuration.recordId
    // because for nav items, the recordId is stored in configuration, not resourceRecordId
    const effectiveRecordId = tab.resourceRecordId || (tab.configuration?.recordId as string) || '';
      tab.applicationId,
      tab.configuration?.resourceType || '',
      tab.configuration?.driverClass || '',
      tab.configuration?.Entity || '',  // Include Entity name for Records resource type
      effectiveRecordId,
      tab.configuration?.route || ''
    return parts.join('|');
   * Create a tab in Golden Layout from workspace tab data
  private createTab(tab: WorkspaceTab): void {
    const app = this.appManager.GetAppById(tab.applicationId);
    const state: TabComponentState = {
      appId: tab.applicationId,
      appColor,
      route: tab.configuration['route'] as string || '',
      isPinned: tab.isPinned,
    this.layoutManager.AddTab(state);
    // Load display name in background without loading full component
    this.updateTabDisplayName(tab);
   * Handle tab shown event for lazy loading
  private async onTabShown(event: TabShownEvent): Promise<void> {
    if (event.isFirstShow) {
      // Load content for this tab
      await this.loadTabContent(event.tabId, event.container);
      this.layoutManager.MarkTabLoaded(event.tabId);
   * Load content into a tab container
   * Uses component cache to reuse components for same resources
  private async loadTabContent(tabId: string, container: unknown): Promise<void> {
      const tab = this.workspaceManager.GetTab(tabId);
      if (!tab) {
        LogError(`Tab not found: ${tabId}`);
      // Get the container element from Golden Layout
      const glContainer = container as { element: HTMLElement };
      if (!glContainer?.element) {
        LogError('Golden Layout container element not found');
      // Extract resource data from tab configuration
      const resourceData = await this.getResourceDataFromTab(tab);
        LogError(`Unable to create ResourceData for tab: ${tab.title}`);
      // Clear any existing content from the container (important for tab reuse)
      glContainer.element.innerHTML = '';
      // Get driver class for cache lookup (resolves to actual component class name)
      // Check if we have a cached component for this resource
        tab.applicationId
        // Reattach the cached wrapper element
        glContainer.element.appendChild(cached.wrapperElement);
        // Mark as attached to this tab
        this.cacheManager.markAsAttached(
          tabId
        // Keep legacy componentRefs map updated
        this.componentRefs.set(tabId, cached.componentRef);
        // If resource is already loaded, update tab title immediately
        const instance = cached.componentRef.instance as BaseResourceComponent;
        if (instance.LoadComplete) {
          this.updateTabTitleFromResource(tabId, instance, resourceData);
      // Get the component registration using the driver class
        // Tab content loaded - update tab title with resource display name
      instance.ResourceRecordSavedEvent = (entity: { Get?: (key: string) => unknown }) => {
        // Update tab title if needed
        if (entity && entity.Get && entity.Get('Name')) {
          // TODO: Implement UpdateTabTitle in WorkspaceStateManager
      // Create a container div for the component
      const componentElement = document.createElement('div');
      componentElement.className = 'tab-content-wrapper';
      componentElement.style.cssText = 'width: 100%; height: 100%;';
      // Append the component's native element
      componentElement.appendChild(nativeElement);
      // Add to Golden Layout container
      glContainer.element.appendChild(componentElement);
      // Cache the component for future reuse
      this.cacheManager.cacheComponent(
        componentRef as ComponentRef<BaseResourceComponent>,
        componentElement,
        resourceData,
      // Store reference for cleanup (legacy)
      this.componentRefs.set(tabId, componentRef as ComponentRef<BaseResourceComponent>);
   * Update tab display name in background without loading full component
   * This ensures all tabs show proper names immediately, not just when clicked
  private async updateTabDisplayName(tab: WorkspaceTab): Promise<void> {
      // Only update display names for resource-based tabs
      const resourceType = tab.configuration['resourceType'] as string;
      // Get ResourceData from tab
      // Get the resource registration to access GetResourceDisplayName without loading full component
      // Create a lightweight instance just to call GetResourceDisplayName
      const tempInstance = new resourceReg.SubClass() as BaseResourceComponent;
      const displayName = await tempInstance.GetResourceDisplayName(resourceData);
      if (displayName && displayName !== tab.title) {
        // Update the tab title in Golden Layout
        this.layoutManager.UpdateTabStyle(tab.id, { title: displayName });
        // Update the tab title in workspace configuration for persistence
        this.workspaceManager.UpdateTabTitle(tab.id, displayName);
      console.error('[TabContainer.updateTabDisplayName] Error updating tab display name:', error);
   * Update tab title with resource display name after resource loads
  private async updateTabTitleFromResource(
    tabId: string,
    resourceComponent: BaseResourceComponent,
    resourceData: ResourceData
      // Get the display name from the resource component
      const displayName = await resourceComponent.GetResourceDisplayName(resourceData);
      if (!displayName) {
      this.layoutManager.UpdateTabStyle(tabId, { title: displayName });
      this.workspaceManager.UpdateTabTitle(tabId, displayName);
      console.error('[TabContainer.updateTabTitleFromResource] Error updating tab title:', error);
   * Convert tab configuration to ResourceData
  private async getResourceDataFromTab(tab: WorkspaceTab): Promise<ResourceData | null> {
    const config = tab.configuration;
    // Extract resource type from configuration or route
    let resourceType = config['resourceType'] as string;
    if (!resourceType && config['route']) {
      // Parse route to determine resource type
      resourceType = this.getResourceTypeFromRoute(config['route'] as string);
      console.error('[TabContainer.getResourceDataFromTab] No resourceType found in config or route');
    // Determine the driver class to use for component instantiation
    let driverClass = resourceType; // Default: use resourceType as driver class
    // For Custom resource type, get DriverClass from configuration or ResourceType metadata
    if (resourceType.toLowerCase() === 'custom') {
      // Custom resource type uses NavItem's DriverClass
      driverClass = config['driverClass'] as string;
        LogError('Custom resource type requires driverClass in configuration');
        console.error('[TabContainer.getResourceDataFromTab] Missing driverClass for Custom resource type');
      // For standard resource types, look up DriverClass from metadata
      const resourceTypeEntity = await this.getResourceTypeEntity(resourceType);
      if (resourceTypeEntity?.DriverClass) {
        driverClass = resourceTypeEntity.DriverClass;
      // If no DriverClass in metadata, fall back to resourceType (backward compatibility)
    // Include applicationId and driverClass in configuration
    const resourceConfig = {
      applicationId: tab.applicationId,
      resourceTypeDriverClass: driverClass  // Store resolved driver class for component lookup
    // Get ResourceRecordID from config or fall back to tab.resourceRecordId
    // Important: Some tabs store the record ID in config['recordId'], others in tab.resourceRecordId
    const resourceRecordId = (config['recordId'] as string) || tab.resourceRecordId || '';
    const resourceData = new ResourceData({
      ResourceTypeID: await this.getResourceTypeId(resourceType),
      ResourceRecordID: resourceRecordId,
      Configuration: resourceConfig
    return resourceData;
  private static _resourceTypesDataset: DatasetResultType | null = null;
   * Get ResourceType entity by name (includes DriverClass field)
  private async getResourceTypeEntity(resourceType: string): Promise<MJResourceTypeEntity | null> {
    const ds = TabContainerComponent._resourceTypesDataset || await md.GetDatasetByName("ResourceTypes");
    if (!ds || !ds.Success || ds.Results.length === 0) {
    if (!TabContainerComponent._resourceTypesDataset) {
      TabContainerComponent._resourceTypesDataset = ds; // cache for next time
    const result = ds.Results.find(r => r.Code.trim().toLowerCase() === 'resourcetypes');
    if (result && result.Results?.length > 0) {
      const rt = result.Results.find(rt => rt.Name.trim().toLowerCase() === resourceType.trim().toLowerCase()) as MJResourceTypeEntity;
      return rt || null;
  private async getResourceTypeId(resourceType: string): Promise<string> {
    const rt = await this.getResourceTypeEntity(resourceType);
    if (rt) {
      return rt.ID;
    throw new Error(`ResourceType ID not found for type: ${resourceType}`);
   * Determine resource type from route
  private getResourceTypeFromRoute(route: string): string {
    // Parse route segments to determine resource type
    const segments = route.split('/').filter(s => s);
    if (segments.length === 0) {
      return 'home';
    // Common route patterns
    if (route.includes('/record/')) {
      return 'record';
    if (route.includes('/view/')) {
      return 'view';
    if (route.includes('/dashboard/')) {
      return 'dashboard';
    if (route.includes('/report/')) {
      return 'report';
    if (route.includes('/search')) {
      return 'search';
    if (route.includes('/query/')) {
      return 'query';
    // Default based on first segment
    return segments[0] || 'home';
   * Count the number of component nodes in a layout tree.
   * Used to validate that saved layout matches the tabs array before restoring.
  private countLayoutComponents(node: LayoutNode): number {
    if (!node) {
    // If this is a component node, count it
    if (node.type === 'component') {
    // If this node has children (row, column, stack), recursively count them
    if (node.content && Array.isArray(node.content)) {
      return node.content.reduce((count, child) => count + this.countLayoutComponents(child), 0);
   * Cleanup a tab's component
   * Detaches from DOM but keeps in cache for potential reuse
  private cleanupTabComponent(tabId: string): void {
    // First, try to detach from cache (preserves component for reuse)
    const cachedInfo = this.cacheManager.markAsDetached(tabId);
    if (cachedInfo) {
      // Remove from legacy componentRefs but keep in cache
      this.componentRefs.delete(tabId);
      // Fallback: destroy if not in cache (shouldn't happen in normal flow)
      const componentRef = this.componentRefs.get(tabId);
      if (componentRef) {
        this.appRef.detachView(componentRef.hostView);
        componentRef.destroy();
   * Sync tabs with configuration changes
  private syncTabsWithConfiguration(tabs: WorkspaceTab[]): void {
    // Get existing tab IDs from Golden Layout
    const existingTabIds = this.layoutManager.GetAllTabIds();
    // Get tab IDs from configuration
    const configTabIds = tabs.map(tab => tab.id);
    // Remove tabs that are no longer in configuration
    existingTabIds.forEach(tabId => {
      if (!configTabIds.includes(tabId)) {
        this.layoutManager.RemoveTab(tabId);
    // Create tabs that don't exist yet
    tabs.forEach(tab => {
      if (!existingTabIds.includes(tab.id)) {
        // Check if tab content needs to be reloaded (app or resource type changed)
        const existingComponentRef = this.componentRefs.get(tab.id);
        if (existingComponentRef) {
          const existingResourceData = existingComponentRef.instance.Data;
          // For Custom resource types, also check driverClass to distinguish between different custom resources
          const existingDriverClass = existingResourceData?.Configuration?.driverClass || existingResourceData?.Configuration?.resourceTypeDriverClass;
          const newDriverClass = tab.configuration['driverClass'] || tab.configuration['resourceTypeDriverClass'];
          // Normalize record IDs for comparison (treat null/undefined as empty string)
          // IMPORTANT: Check both tab.resourceRecordId AND tab.configuration['recordId']
          const existingRecordId = existingResourceData?.ResourceRecordID || '';
          const newRecordId = tab.resourceRecordId || tab.configuration['recordId'] as string || '';
          const needsReload = existingResourceData?.ResourceType !== tab.configuration['resourceType'] ||
                             existingResourceData?.Configuration?.applicationId !== tab.applicationId ||
                             existingRecordId !== newRecordId ||
                             (tab.configuration['resourceType'] === 'Custom' && existingDriverClass !== newDriverClass);
          if (needsReload) {
            // Clean up old component
            this.cleanupTabComponent(tab.id);
            // Mark tab as not loaded so it will reload when shown
            this.layoutManager.MarkTabNotLoaded(tab.id);
            // Update display name in background
            // If this tab is currently active, reload it immediately
            if (config?.activeTabId === tab.id) {
              const glContainer = this.layoutManager.GetContainer(tab.id);
              if (glContainer) {
                this.loadTabContent(tab.id, glContainer);
        // Update styling for existing tabs
        this.layoutManager.UpdateTabStyle(tab.id, {
          appColor: app?.GetColor() || '#757575'
    // Focus the active tab
    if (config?.activeTabId) {
   * Show context menu
  showContextMenu(x: number, y: number, tabId: string): void {
    this.contextMenuX = x;
    this.contextMenuY = y;
    this.contextMenuTabId = tabId;
    this.contextMenuVisible = true;
    // Close menu when clicking outside - use setTimeout to avoid immediate trigger
      const clickHandler = (event: MouseEvent) => {
        if (!target.closest('.context-menu')) {
          this.hideContextMenu();
          document.removeEventListener('click', clickHandler);
          document.removeEventListener('keydown', keyHandler);
      const keyHandler = (event: KeyboardEvent) => {
      document.addEventListener('click', clickHandler);
      document.addEventListener('keydown', keyHandler);
   * Hide context menu
  hideContextMenu(): void {
    this.contextMenuVisible = false;
    this.contextMenuTabId = null;
   * Check if context menu tab is pinned
  get isContextTabPinned(): boolean {
    if (!this.contextMenuTabId) return false;
    const tab = this.workspaceManager.GetTab(this.contextMenuTabId);
    return tab?.isPinned || false;
   * Toggle pin from context menu
  onContextPin(): void {
    if (this.contextMenuTabId) {
      this.workspaceManager.TogglePin(this.contextMenuTabId);
   * Close tab from context menu
  onContextClose(): void {
      this.layoutManager.RemoveTab(this.contextMenuTabId);
   * Close all other tabs from context menu
  onContextCloseOthers(): void {
      this.workspaceManager.CloseOtherTabs(this.contextMenuTabId);
   * Close tabs to the right from context menu
  onContextCloseToRight(): void {
      this.workspaceManager.CloseTabsToRight(this.contextMenuTabId);
   * While the naming implies this is only invoked once, components we DO NOT CONTROL might have race
   * conditions that result in unpredictable behavior. To avoid those causing loading screen overaly to show
   * forever we emit all events upstream
  private emitFirstLoadCompleteOnce(): void {
    this.firstResourceLoadComplete.emit(); // do this each time to be sure we don't suppress messages
import { Component, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewInit, createComponent, EnvironmentInjector, ApplicationRef } from '@angular/core';
import { RouterModule, Router } from '@angular/router';
import { TabState } from '../../core/models/app.interface';
import { LayoutConfig, ComponentItemConfig, ResolvedComponentItemConfig, ComponentContainer, ResolvedLayoutConfig, LayoutConfig as GLLayoutConfig } from 'golden-layout';
import { VirtualLayout, LayoutConfig as GoldenLayoutConfigClass } from 'golden-layout';
// Import all the components we might need to render
import { ChatComponent } from '../../apps/conversations/chat/chat.component';
import { CollectionsComponent } from '../../apps/conversations/collections/collections.component';
import { TasksComponent } from '../../apps/conversations/tasks/tasks.component';
import { SettingsComponent } from '../../apps/settings/settings.component';
import { CrmDashboardComponent } from '../../apps/crm/dashboard/dashboard.component';
import { ContactsComponent } from '../../apps/crm/contacts/contacts.component';
import { ContactDetailComponent } from '../../apps/crm/contact-detail/contact-detail.component';
import { CompaniesComponent } from '../../apps/crm/companies/companies.component';
import { OpportunitiesComponent } from '../../apps/crm/opportunities/opportunities.component';
interface TabComponentState {
// Map routes to components
const ROUTE_COMPONENT_MAP: { [key: string]: any } = {
  '/conversations/chat': ChatComponent,
  '/conversations/collections': CollectionsComponent,
  '/conversations/tasks': TasksComponent,
  '/settings': SettingsComponent,
  '/crm/dashboard': CrmDashboardComponent,
  '/crm/contacts': ContactsComponent,
  '/crm/contact': ContactDetailComponent,
  '/crm/companies': CompaniesComponent,
  '/crm/opportunities': OpportunitiesComponent
  selector: 'app-tab-container',
  styleUrls: ['./tab-container.component.scss']
  @ViewChild('layoutContainer', { static: false }) layoutContainerRef!: ElementRef;
  private layout: VirtualLayout | null = null;
  private componentRefs = new Map<string, any>();
    private appRef: ApplicationRef
    // Subscribe to tabs and update Golden Layout
      this.shellService.GetTabs().subscribe(tabs => {
        if (this.isInitialized && this.layout) {
          this.updateLayout(tabs);
    // Subscribe to active tab
      this.shellService.GetActiveTabId().subscribe(tabId => {
          const tabs = this.shellService['tabs$'].value;
          const tab = tabs.find((t: TabState) => t.Id === tabId);
          if (tab && this.layout) {
            this.focusTab(tabId);
            // Update URL for deep linking only if it's different
            if (currentUrl !== tab.Route) {
              this.router.navigate([tab.Route], { skipLocationChange: false });
    setTimeout(() => this.initializeGoldenLayout(), 0);
    // Clean up all component refs
    this.componentRefs.forEach(compRef => {
      this.appRef.detachView(compRef.hostView);
      compRef.destroy();
  private initializeGoldenLayout(): void {
    const container = this.layoutContainerRef.nativeElement;
      this.bindComponentEvent.bind(this),
      this.unbindComponentEvent.bind(this)
    // Try to load saved layout configuration
    const savedConfig = this.shellService.LoadLayoutConfig();
    // Check if saved config has actual content (not just empty root)
    const savedConfigHasContent = savedConfig?.root?.content && savedConfig.root.content.length > 0;
    if (savedConfigHasContent && tabs.length > 0) {
      // Restore saved layout with existing tabs
        this.layout.loadLayout(savedConfig);
        console.warn('Failed to restore saved layout, using default:', error);
        this.loadDefaultLayout(tabs);
      // Start with empty layout and add tabs
    // Set initial size explicitly
    // Force layout update after loading
    // Handle window resize
    window.addEventListener('resize', () => {
    // Save layout configuration whenever it changes
      this.saveLayoutConfig();
      // Reapply styles to all tabs after layout changes (handles drag/drop)
    // Wait for DOM to settle after Golden Layout operations
      if (!this.layout || !this.layout.rootItem) return;
      // Find all component items
      const getAllComponents = (item: any): any[] => {
        let components: any[] = [];
        if (item.type === 'component') {
          components.push(item);
        } else if (item.contentItems) {
            components = components.concat(getAllComponents(child));
      const currentItems = getAllComponents(this.layout.rootItem);
      // Reapply styles to each tab
      currentItems.forEach((item: any) => {
        const state = item.container?.state as TabComponentState | undefined;
          const tabElement = this.findTabElementByContainer(item.container);
            this.updateTabStyle(tabElement, state.tabId, state.appColor);
  private loadDefaultLayout(tabs: TabState[]): void {
    const config: LayoutConfig = {
    this.layout!.loadLayout(config);
    // Add existing tabs
    tabs.forEach((tab: TabState) => this.addTabToLayout(tab));
  private saveLayoutConfig(): void {
        const resolvedConfig = this.layout.saveLayout();
        // Convert ResolvedLayoutConfig to LayoutConfig for storage
        const config = GoldenLayoutConfigClass.fromResolved(resolvedConfig);
        this.shellService.SaveLayoutConfig(config);
        console.warn('Failed to save layout config:', error);
  private bindComponentEvent(
    itemConfig: ResolvedComponentItemConfig
  ): ComponentContainer.BindableComponent {
    const state = itemConfig.componentState as TabComponentState;
    // Create container div for this tab's content
    element.style.overflow = 'auto';
    // Get the component type for this route
    const componentType = this.getComponentForRoute(state.route);
    if (componentType) {
        // Create the Angular component dynamically
        const componentRef = createComponent(componentType, {
        // Append component's DOM to our container
        const componentElement = componentRef.location.nativeElement;
        element.appendChild(componentElement);
        this.componentRefs.set(state.tabId, componentRef);
        console.error('Error creating component:', error);
        element.innerHTML = `<div style="padding: 20px; color: red;">
          <h2>Error creating component</h2>
          <p>${error}</p>
      // Fallback content if component not found
      element.innerHTML = `<div style="padding: 20px; color: #666;">
        <h2>${state.title}</h2>
        <p>Route: ${state.route}</p>
        <p>Component not found in ROUTE_COMPONENT_MAP</p>
    // Set tab title
    // IMPORTANT: Append element to container's element
    container.element.appendChild(element);
    // Handle tab close
    container.on('destroy', () => {
      const compRef = this.componentRefs.get(state.tabId);
      if (compRef) {
        this.componentRefs.delete(state.tabId);
      this.shellService.CloseTab(state.tabId);
    // Handle tab activation
      this.shellService.SetActiveTab(state.tabId);
      // Router navigation handled by activeTabId subscription
    // Handle double-click on tab header to toggle permanent status
    // We need to wait for the tab element to be created, then attach the listener
      const tabElement = this.findTabElementByContainer(container);
        // Double-click to pin/unpin
          this.shellService.ToggleTabPermanent(state.tabId);
        // Right-click context menu
          this.showTabContextMenu(e, state.tabId);
        // Set initial style based on permanent status and app color
      virtual: false  // Actual content renders in tabs!
  private findTabElement(container: ComponentContainer): HTMLElement | null {
    // Golden Layout tab elements have the class 'lm_tab'
    // Find the tab that corresponds to this container
    const allTabs = document.querySelectorAll('.lm_tab');
    for (let i = 0; i < allTabs.length; i++) {
      const tab = allTabs[i] as HTMLElement;
      // The tab title should match our container title
      const titleElement = tab.querySelector('.lm_title');
      if (titleElement && titleElement.textContent === container.title) {
  // More reliable method to find tab element using Golden Layout's internal structure
  private findTabElementByContainer(container: ComponentContainer): HTMLElement | null {
    // Access Golden Layout's tab element through the component item
    const componentItem = (container as any)._componentItem;
    if (componentItem && componentItem.tab && componentItem.tab.element) {
      return componentItem.tab.element as HTMLElement;
    // Fallback to title matching
    return this.findTabElement(container);
  private showTabContextMenu(event: MouseEvent, tabId: string): void {
    // Remove any existing context menu
    this.removeContextMenu();
    const tab = tabs.find(t => t.Id === tabId);
    if (!tab) return;
    // Create context menu
    menu.className = 'tab-context-menu';
    menu.style.cssText = `
      top: ${event.clientY}px;
      left: ${event.clientX}px;
    // Pin/Unpin option
    const pinOption = document.createElement('div');
    pinOption.className = 'context-menu-item';
    pinOption.innerHTML = tab.IsPermanent
      ? '<i class="fa-solid fa-thumbtack" style="margin-right: 8px; width: 14px;"></i>Unpin Tab'
      : '<i class="fa-solid fa-thumbtack" style="margin-right: 8px; width: 14px;"></i>Pin Tab';
    pinOption.style.cssText = `
    pinOption.addEventListener('mouseenter', () => pinOption.style.background = '#f5f5f5');
    pinOption.addEventListener('mouseleave', () => pinOption.style.background = 'transparent');
    pinOption.addEventListener('click', () => {
      this.shellService.ToggleTabPermanent(tabId);
      // Update the tab style
      const tabElement = this.findTabElementById(tabId);
        const app = this.shellService.GetApp(tab.AppId);
        this.updateTabStyle(tabElement, tabId, app?.Color);
    // Close option
    const closeOption = document.createElement('div');
    closeOption.className = 'context-menu-item';
    closeOption.innerHTML = '<i class="fa-solid fa-xmark" style="margin-right: 8px; width: 14px;"></i>Close Tab';
    closeOption.style.cssText = `
    closeOption.addEventListener('mouseenter', () => closeOption.style.background = '#f5f5f5');
    closeOption.addEventListener('mouseleave', () => closeOption.style.background = 'transparent');
    closeOption.addEventListener('click', () => {
      this.shellService.CloseTab(tabId);
    menu.appendChild(pinOption);
    menu.appendChild(closeOption);
    const closeMenu = (e: MouseEvent) => {
      if (!menu.contains(e.target as Node)) {
    setTimeout(() => document.addEventListener('click', closeMenu), 0);
  private removeContextMenu(): void {
    const existingMenu = document.querySelector('.tab-context-menu');
  private findTabElementById(tabId: string): HTMLElement | null {
    // Find tab element by searching through all tabs and matching the tabId in their state
    if (!tab) return null;
      const tabElement = allTabs[i] as HTMLElement;
      const titleElement = tabElement.querySelector('.lm_title');
      if (titleElement && titleElement.textContent === tab.Title) {
        return tabElement;
  private updateTabStyle(tabElement: HTMLElement, tabId: string, appColor?: string): void {
    // VSCode behavior: only italic changes, font-weight stays constant
    if (tab?.IsPermanent) {
      tabElement.style.fontStyle = 'normal';
      // Add pin icon in the close button area if not already present
          this.updateTabStyle(tabElement, tabId, appColor);
      tabElement.style.fontStyle = 'italic';
    // Apply app color as CSS variable for the accent indicator
    if (appColor) {
      tabElement.style.setProperty('--app-color', appColor);
    } else if (tab) {
      // Get color from app if not provided
      if (app?.Color) {
        tabElement.style.setProperty('--app-color', app.Color);
  private unbindComponentEvent(container: ComponentContainer): void {
    const state = container.state as TabComponentState;
    if (state && state.tabId) {
  private getComponentForRoute(route: string): any {
    if (ROUTE_COMPONENT_MAP[route]) {
      return ROUTE_COMPONENT_MAP[route];
    // Try to match pattern (e.g., /crm/contact/123 -> /crm/contact)
    for (const pattern in ROUTE_COMPONENT_MAP) {
      if (route.startsWith(pattern)) {
        return ROUTE_COMPONENT_MAP[pattern];
  private updateLayout(tabs: TabState[]): void {
    if (!this.layout) return;
    // If layout has no root item (all tabs were closed), add all tabs as new
    if (!this.layout.rootItem) {
      tabs.forEach(tab => this.addTabToLayout(tab));
    // Get current component items
    const currentTabIds = currentItems
      .map((item: any) => {
        return state?.tabId;
      .filter((id: string | undefined): id is string => !!id);
    // Find new tabs
    const newTabs = tabs.filter(tab => !currentTabIds.includes(tab.Id));
    // Add new tabs
    newTabs.forEach(tab => this.addTabToLayout(tab));
    // Check for tabs with updated routes (temporary tab content replacement)
        const updatedTab = tabs.find(t => t.Id === state.tabId);
        if (updatedTab && (updatedTab.Route !== state.route || updatedTab.Title !== state.title)) {
          // Tab content has changed - need to recreate the component
          this.recreateTabContent(item, updatedTab);
    // Remove closed tabs
      if (state?.tabId && !tabs.find(t => t.Id === state.tabId)) {
          item.remove();
          console.warn('Failed to remove item:', state.tabId, error);
  private recreateTabContent(item: any, updatedTab: TabState): void {
    const container = item.container;
    // Get the app for the updated tab
    const app = this.shellService.GetApp(updatedTab.AppId);
    const appColor = app?.Color || '#757575';
    // Update the state
    state.route = updatedTab.Route;
    state.title = updatedTab.Title;
    state.appId = updatedTab.AppId;
    state.appColor = appColor;
    // Update the tab title
    container.setTitle(updatedTab.Title);
    // Update tab styling with new app color
      const tabElement = this.findTabElement(container);
        this.updateTabStyle(tabElement, state.tabId, appColor);
    const oldCompRef = this.componentRefs.get(state.tabId);
    if (oldCompRef) {
      this.appRef.detachView(oldCompRef.hostView);
      oldCompRef.destroy();
    const containerElement = container.element;
    while (containerElement.firstChild) {
      containerElement.removeChild(containerElement.firstChild);
    const componentType = this.getComponentForRoute(updatedTab.Route);
        element.appendChild(componentRef.location.nativeElement);
        console.error('Error recreating component:', error);
          <h2>Error recreating component</h2>
    containerElement.appendChild(element);
  private addTabToLayout(tab: TabState): void {
    // Get the app color for this tab
      componentType: 'TabContent',
      componentState: {
        tabId: tab.Id,
        title: tab.Title,
        route: tab.Route,
        appId: tab.AppId,
        appColor: appColor
      } as TabComponentState,
      title: tab.Title
      // Check if layout has any content - if not, we need to reset it first
      // This happens when all tabs were closed
      if (!this.layout.rootItem ||
          (this.layout.rootItem.contentItems && this.layout.rootItem.contentItems.length === 0)) {
        // Reset to a fresh layout before adding the component
      this.layout.addComponent(componentConfig.componentType, componentConfig.componentState, componentConfig.title);
      console.error('Failed to add tab to layout:', error);
      // Try resetting layout and adding again
      } catch (retryError) {
        console.error('Failed to add tab even after layout reset:', retryError);
  private focusTab(tabId: string): void {
    // Find the component item with matching tabId
    const findComponent = (item: any): any => {
        if (state?.tabId === tabId) {
          const found = findComponent(child);
    const tabItem = findComponent(this.layout.rootItem);
    if (tabItem && tabItem.parent?.type === 'stack') {
      tabItem.parent.setActiveContentItem(tabItem);
