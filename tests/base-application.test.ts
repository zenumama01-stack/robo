 * Tests for base-application package:
 * - WorkspaceConfiguration defaults
 * - WorkspaceStateManager (tab management)
  Component: () => (target: Function) => target,
  Directive: () => (target: Function) => target,
  Input: () => () => {},
  Output: () => () => {},
  EventEmitter: class { emit() {} },
    CurrentUser = { ID: 'user-123', Name: 'Test User' };
    Applications = [];
  ApplicationInfo: class {},
  StartupManager: { Instance: { Startup: vi.fn() } },
  UserInfoEngine: {
      Workspaces: [],
      UserApplications: [],
      CreateDefaultApplications: vi.fn().mockResolvedValue([]),
      DataChange$: { subscribe: vi.fn() },
      GetSetting: vi.fn(),
      SetSetting: vi.fn(),
      FindApplicationByPathOrName: vi.fn(),
      IsApplicationInactive: vi.fn(),
      CheckUserApplicationAccess: vi.fn(),
      InstallApplication: vi.fn(),
      EnableApplication: vi.fn(),
      DisableApplication: vi.fn(),
      UninstallApplication: vi.fn(),
  MJWorkspaceEntity: class {},
  MJUserApplicationEntity: class {},
      GetEventListener: vi.fn(() => ({ subscribe: vi.fn() })),
  MJEventType: { LoggedIn: 'LoggedIn' },
// ======================= createDefaultWorkspaceConfiguration =======================
describe('createDefaultWorkspaceConfiguration', () => {
  it('should return a valid default configuration', async () => {
    const { createDefaultWorkspaceConfiguration } = await import('../interfaces/workspace-configuration.interface');
    const config = createDefaultWorkspaceConfiguration();
    expect(config.version).toBe(1);
    expect(config.activeTabId).toBeNull();
    expect(config.theme).toBe('light');
    expect(config.tabs).toEqual([]);
    expect(config.preferences.tabPosition).toBe('top');
    expect(config.preferences.showTabIcons).toBe(true);
    expect(config.preferences.autoSaveInterval).toBe(5000);
    expect(config.layout).toBeDefined();
    expect(config.layout!.root.type).toBe('row');
    expect(config.layout!.root.content).toEqual([]);
// ======================= WorkspaceStateManager =======================
describe('WorkspaceStateManager', () => {
  let manager: InstanceType<typeof import('../workspace-state-manager').WorkspaceStateManager>;
    const mod = await import('../workspace-state-manager');
    manager = new mod.WorkspaceStateManager();
  describe('initial state', () => {
    it('should have null initial configuration', () => {
      expect(manager.GetConfiguration()).toBeNull();
    it('should return null for active tab ID when not initialized', () => {
      expect(manager.GetActiveTabId()).toBeNull();
  describe('UpdateConfiguration', () => {
    it('should update configuration', async () => {
      manager.UpdateConfiguration(config);
      expect(manager.GetConfiguration()).toEqual(config);
  describe('OpenTab', () => {
    it('should create a new tab when config is initialized', async () => {
      manager.UpdateConfiguration(createDefaultWorkspaceConfiguration());
      const tabId = manager.OpenTab(
        { ApplicationId: 'app-1', Title: 'Test Tab', Configuration: { resourceType: 'Records', Entity: 'MJ: Users' } },
        '#ff0000'
      expect(tabId).toBeDefined();
      const config = manager.GetConfiguration();
      expect(config!.tabs.length).toBe(1);
      expect(config!.tabs[0].title).toBe('Test Tab');
      expect(config!.activeTabId).toBe(tabId);
    it('should return existing tab ID if matching tab exists', async () => {
      const tabId1 = manager.OpenTab(
        { ApplicationId: 'app-1', Title: 'Test', ResourceRecordId: 'rec-1', Configuration: { resourceType: 'Records', Entity: 'MJ: Users' } },
      const tabId2 = manager.OpenTab(
      expect(tabId1).toBe(tabId2);
      expect(manager.GetConfiguration()!.tabs.length).toBe(1);
    it('should replace temporary (unpinned) tab', async () => {
      manager.OpenTab(
        { ApplicationId: 'app-1', Title: 'Tab 1', Configuration: { resourceType: 'Records', Entity: 'MJ: Users' }, ResourceRecordId: '1' },
      // Open a different resource - should replace the unpinned tab
        { ApplicationId: 'app-1', Title: 'Tab 2', Configuration: { resourceType: 'Records', Entity: 'Contacts' }, ResourceRecordId: '2' },
      expect(manager.GetConfiguration()!.tabs[0].title).toBe('Tab 2');
    it('should throw if configuration is not initialized', () => {
      expect(() => manager.OpenTab(
        { ApplicationId: 'app-1', Title: 'Test', Configuration: {} },
      )).toThrow('Configuration not initialized');
  describe('CloseTab', () => {
    it('should remove a tab when multiple tabs exist', async () => {
      const tab1Id = manager.OpenTab(
        { ApplicationId: 'app-1', Title: 'Tab 1', IsPinned: true, Configuration: { resourceType: 'Records', Entity: 'MJ: Users' }, ResourceRecordId: '1' },
      const tab2Id = manager.OpenTab(
        { ApplicationId: 'app-1', Title: 'Tab 2', IsPinned: true, Configuration: { resourceType: 'Records', Entity: 'Contacts' }, ResourceRecordId: '2' },
      manager.CloseTab(tab1Id);
      expect(manager.GetConfiguration()!.tabs[0].id).toBe(tab2Id);
    it('should keep last tab but mark as unpinned', async () => {
        { ApplicationId: 'app-1', Title: 'Only Tab', IsPinned: true, Configuration: { resourceType: 'Records' }, ResourceRecordId: '1' },
      manager.CloseTab(tabId);
      // Tab should still exist but be unpinned
      expect(manager.GetConfiguration()!.tabs[0].isPinned).toBe(false);
  describe('TogglePin', () => {
    it('should toggle pin state of a tab', async () => {
      // OpenTab creates unpinned (temporary) tabs by default
        { ApplicationId: 'app-1', Title: 'Tab 1', Configuration: { resourceType: 'Records' }, ResourceRecordId: '1' },
      expect(manager.GetTab(tabId)!.isPinned).toBe(false);
      manager.TogglePin(tabId);
      expect(manager.GetTab(tabId)!.isPinned).toBe(true);
  describe('SetActiveTab', () => {
    it('should set the active tab', async () => {
        { ApplicationId: 'app-1', Title: 'Tab 1', IsPinned: true, Configuration: { resourceType: 'Records', Entity: 'A' }, ResourceRecordId: '1' },
        { ApplicationId: 'app-1', Title: 'Tab 2', IsPinned: true, Configuration: { resourceType: 'Records', Entity: 'B' }, ResourceRecordId: '2' },
      manager.SetActiveTab(tab1Id);
      expect(manager.GetActiveTabId()).toBe(tab1Id);
  describe('GetAppTabs', () => {
    it('should return tabs for a specific application', async () => {
      // Use OpenTabForced to create separate pinned tabs (OpenTab replaces temp tabs)
      manager.OpenTabForced(
        { ApplicationId: 'app-1', Title: 'App1 Tab', IsPinned: true, Configuration: { resourceType: 'Records', Entity: 'A' }, ResourceRecordId: '1' },
        { ApplicationId: 'app-2', Title: 'App2 Tab', IsPinned: true, Configuration: { resourceType: 'Records', Entity: 'B' }, ResourceRecordId: '2' },
        '#0000ff'
      expect(manager.GetAppTabs('app-1').length).toBe(1);
      expect(manager.GetAppTabs('app-2').length).toBe(1);
      expect(manager.GetAppTabs('app-3').length).toBe(0);
  describe('UpdateTabTitle', () => {
    it('should update the title of a tab', async () => {
        { ApplicationId: 'app-1', Title: 'Original', IsPinned: true, Configuration: { resourceType: 'Records' }, ResourceRecordId: '1' },
      manager.UpdateTabTitle(tabId, 'Updated Title');
      expect(manager.GetTab(tabId)!.title).toBe('Updated Title');
  describe('CloseOtherTabs', () => {
    it('should close all tabs except specified one', async () => {
        { ApplicationId: 'app-1', Title: 'Tab 3', IsPinned: true, Configuration: { resourceType: 'Records', Entity: 'C' }, ResourceRecordId: '3' },
      manager.CloseOtherTabs(tab2Id);
  describe('ClearLayout', () => {
    it('should clear the layout', async () => {
      manager.ClearLayout();
      expect(manager.GetConfiguration()!.layout).toBeUndefined();
