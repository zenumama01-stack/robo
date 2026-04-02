import { Component, OnDestroy, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { ResourceData, UserInfoEngine } from '@memberjunction/core-entities';
import { CreateAgentService, CreateAgentDialogResult, CreateAgentResult } from '@memberjunction/ng-agents';
interface AgentFilter {
  searchTerm: string;
  agentType: string;
  parentAgent: string;
  executionMode: string;
  exposeAsAction: string;
 * User preferences for the Agent Configuration dashboard
interface AgentConfigurationUserPreferences {
  filterPanelVisible: boolean;
  viewMode: 'grid' | 'list';
  sortColumn: string;
  sortDirection: 'asc' | 'desc';
  filters: AgentFilter;
 * AI Agents Resource - displays AI agent configuration and management
@RegisterClass(BaseResourceComponent, 'AIAgentsResource')
  selector: 'app-agent-configuration',
  templateUrl: './agent-configuration.component.html',
  styleUrls: ['./agent-configuration.component.css']
export class AgentConfigurationComponent extends BaseResourceComponent implements AfterViewInit, OnDestroy {
  // Settings persistence
  private readonly USER_SETTINGS_KEY = 'AI.Agents.UserPreferences';
  private settingsPersistSubject = new Subject<void>();
  private settingsLoaded = false;
  public filterPanelVisible = true;
  public viewMode: 'grid' | 'list' = 'grid';
  public expandedAgentId: string | null = null;
  public agents: AIAgentEntityExtended[] = [];
  public filteredAgents: AIAgentEntityExtended[] = [];
  // Detail panel
  public selectedAgent: AIAgentEntityExtended | null = null;
  public detailPanelVisible = false;
  // Sorting state
  public sortColumn: string = 'Name';
  public sortDirection: 'asc' | 'desc' = 'asc';
  public currentFilters: AgentFilter = {
    searchTerm: '',
    agentType: 'all',
    parentAgent: 'all',
    status: 'all',
    executionMode: 'all',
    exposeAsAction: 'all'
  public selectedAgentForTest: AIAgentEntityExtended | null = null;
  // === Permission Checks ===
  /** Check if user can create AI Agents */
  public get UserCanCreateAgents(): boolean {
  /** Check if user can read AI Agents */
  public get UserCanReadAgents(): boolean {
    return this.checkEntityPermission('MJ: AI Agents', 'Read');
  /** Check if user can update AI Agents */
  public get UserCanUpdateAgents(): boolean {
  /** Check if user can delete AI Agents */
  public get UserCanDeleteAgents(): boolean {
    private testHarnessService: AITestHarnessDialogService,
    private createAgentService: CreateAgentService,
    // Set up debounced settings persistence
    this.settingsPersistSubject.pipe(
      debounceTime(500),
      this.persistUserPreferences();
  async ngAfterViewInit() {
    // Load saved user preferences first
    // Apply initial state from resource configuration if provided (overrides saved prefs)
      this.applyInitialState(this.Data.Configuration);
    await this.loadAgents();
    // Apply filters after data is loaded (uses saved preferences)
    this.applyFilters();
  // User Settings Persistence
   * Load saved user preferences from the UserInfoEngine
      const savedPrefs = UserInfoEngine.Instance.GetSetting(this.USER_SETTINGS_KEY);
      if (savedPrefs) {
        const prefs = JSON.parse(savedPrefs) as AgentConfigurationUserPreferences;
        this.applyUserPreferences(prefs);
      console.warn('[AgentConfiguration] Failed to load user preferences:', error);
      this.settingsLoaded = true;
   * Apply loaded preferences to component state
  private applyUserPreferences(prefs: AgentConfigurationUserPreferences): void {
    if (prefs.filterPanelVisible !== undefined) {
      this.filterPanelVisible = prefs.filterPanelVisible;
    if (prefs.viewMode) {
      this.viewMode = prefs.viewMode;
    if (prefs.sortColumn) {
      this.sortColumn = prefs.sortColumn;
    if (prefs.sortDirection) {
      this.sortDirection = prefs.sortDirection;
    if (prefs.filters) {
      this.currentFilters = {
        searchTerm: prefs.filters.searchTerm || '',
        agentType: prefs.filters.agentType || 'all',
        parentAgent: prefs.filters.parentAgent || 'all',
        status: prefs.filters.status || 'all',
        executionMode: prefs.filters.executionMode || 'all',
        exposeAsAction: prefs.filters.exposeAsAction || 'all'
   * Get current preferences as an object for saving
  private getCurrentPreferences(): AgentConfigurationUserPreferences {
      filterPanelVisible: this.filterPanelVisible,
      viewMode: this.viewMode,
      sortColumn: this.sortColumn,
      sortDirection: this.sortDirection,
        ...this.currentFilters
   * Persist user preferences to storage (debounced)
  private saveUserPreferencesDebounced(): void {
    if (!this.settingsLoaded) return; // Don't save during initial load
    this.settingsPersistSubject.next();
   * Actually persist user preferences to the UserInfoEngine
  private async persistUserPreferences(): Promise<void> {
      const prefs = this.getCurrentPreferences();
      await UserInfoEngine.Instance.SetSetting(this.USER_SETTINGS_KEY, JSON.stringify(prefs));
      console.warn('[AgentConfiguration] Failed to persist user preferences:', error);
  private applyInitialState(state: any): void {
    if (state.filterPanelVisible !== undefined) {
      this.filterPanelVisible = state.filterPanelVisible;
    if (state.viewMode) {
      this.viewMode = state.viewMode;
    if (state.expandedAgentId) {
      this.expandedAgentId = state.expandedAgentId;
    if (state.currentFilters) {
      this.currentFilters = { ...this.currentFilters, ...state.currentFilters };
  private async loadAgents(): Promise<void> {
      // Ensure AIEngineBase is configured (no-op if already loaded)
      await AIEngineBase.Instance.Config(false);
      // Get cached agents from AIEngineBase
      this.agents = AIEngineBase.Instance.Agents;
      this.filteredAgents = [...this.agents];
      console.error('Error loading AI agents:', error);
      // force change detection to update the view
  public toggleFilterPanel(): void {
    this.filterPanelVisible = !this.filterPanelVisible;
    this.saveUserPreferencesDebounced();
  public onMainSplitterChange(_event: any): void {
  public onFiltersChange(filters: AgentFilter): void {
    this.currentFilters = { ...filters };
  public onFilterChange(): void {
  public onResetFilters(): void {
  private applyFilters(): void {
    let filtered = [...this.agents];
    // Apply search filter (name contains)
    if (this.currentFilters.searchTerm) {
      const searchTerm = this.currentFilters.searchTerm.toLowerCase();
        (agent.Name || '').toLowerCase().includes(searchTerm) ||
        (agent.Description || '').toLowerCase().includes(searchTerm)
    // Apply agent type filter
    if (this.currentFilters.agentType !== 'all') {
      filtered = filtered.filter(agent => agent.TypeID === this.currentFilters.agentType);
    // Apply parent agent filter
    if (this.currentFilters.parentAgent !== 'all') {
      if (this.currentFilters.parentAgent === 'none') {
        filtered = filtered.filter(agent => !agent.ParentID);
        filtered = filtered.filter(agent => agent.ParentID === this.currentFilters.parentAgent);
    // Apply status filter
    if (this.currentFilters.status !== 'all') {
      const wantActive = this.currentFilters.status === 'active';
      if (wantActive) {
        filtered = filtered.filter(agent => agent.Status === 'Active');
        filtered = filtered.filter(agent => agent.Status !== 'Active');
    // Apply execution mode filter
    if (this.currentFilters.executionMode !== 'all') {
      filtered = filtered.filter(agent => agent.ExecutionMode === this.currentFilters.executionMode);
    // Apply expose as action filter
    if (this.currentFilters.exposeAsAction !== 'all') {
      const isExposed = this.currentFilters.exposeAsAction === 'true';
      filtered = filtered.filter(agent => agent.ExposeAsAction === isExposed);
    filtered = this.applySorting(filtered);
    this.filteredAgents = filtered;
   * Sort the agents by the specified column
  public sortBy(column: string): void {
    if (this.sortColumn === column) {
      // Toggle direction if same column
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
      // New column, default to ascending
      this.sortColumn = column;
      this.sortDirection = 'asc';
   * Apply sorting to the filtered list
  private applySorting(agents: AIAgentEntityExtended[]): AIAgentEntityExtended[] {
    return agents.sort((a, b) => {
      let valueA: string | boolean | null | undefined;
      let valueB: string | boolean | null | undefined;
      switch (this.sortColumn) {
        case 'Name':
          valueA = a.Name;
          valueB = b.Name;
        case 'Status':
          valueA = a.Status;
          valueB = b.Status;
        case 'ExecutionMode':
          valueA = a.ExecutionMode;
          valueB = b.ExecutionMode;
      // Handle null/undefined values
      const strA = (valueA ?? '').toString().toLowerCase();
      const strB = (valueB ?? '').toString().toLowerCase();
      let comparison = strA.localeCompare(strB);
      return this.sortDirection === 'desc' ? -comparison : comparison;
    // State change handling - could be used for persisting user preferences in the future
    // For now, just a placeholder for tracking state changes
  public setViewMode(mode: 'grid' | 'list'): void {
    this.viewMode = mode;
  public toggleAgentExpansion(agentId: string): void {
    this.expandedAgentId = this.expandedAgentId === agentId ? null : agentId;
   * Show the detail panel for an agent
  public showAgentDetails(agent: AIAgentEntityExtended, event?: Event): void {
    if (event) {
    this.selectedAgent = agent;
    this.detailPanelVisible = true;
   * Close the detail panel
    this.detailPanelVisible = false;
    // Delay clearing selectedAgent for smoother animation
      if (!this.detailPanelVisible) {
        this.selectedAgent = null;
   * Open the full entity record from the detail panel
  public openAgentFromPanel(): void {
    if (this.selectedAgent) {
      this.openAgentRecord(this.selectedAgent.ID);
   * Get the parent agent name if it exists
  public getParentAgentName(agent: AIAgentEntityExtended): string | null {
    if (!agent.ParentID) return null;
    const parent = this.agents.find(a => a.ID === agent.ParentID);
    return parent?.Name || 'Unknown Parent';
   * Get agent type name
  public getAgentTypeName(agent: AIAgentEntityExtended): string {
    return agent.Type || 'Standard Agent';
  public openAgentRecord(agentId: string): void {
    const compositeKey = new CompositeKey([{ FieldName: 'ID', Value: agentId }]);
    this.navigationService.OpenEntityRecord('MJ: AI Agents', compositeKey);
   * Opens the create agent slide-in panel. Upon successful creation,
   * saves the agent and navigates to the new record.
  public createNewAgent(): void {
    this.createAgentService.OpenSlideIn({
      Title: 'Create New Agent'
      next: async (dialogResult: CreateAgentDialogResult) => {
          await this.handleAgentCreated(dialogResult.Result);
        console.error('Error in create agent slide-in:', error);
          'Error opening agent creation panel. Please try again.',
   * Handles the result from the create agent slide-in.
   * Saves the agent and navigates to the new record.
  private async handleAgentCreated(result: CreateAgentResult): Promise<void> {
      const agent = result.Agent;
      // Save linked prompts if any
      if (result.AgentPrompts && result.AgentPrompts.length > 0) {
          // Update the AgentID to the saved agent's ID
          await agentPrompt.Save();
      // Save linked actions if any
      if (result.AgentActions && result.AgentActions.length > 0) {
          agentAction.AgentID = agent.ID;
          await agentAction.Save();
      // Refresh the agent list
      await AIEngineBase.Instance.Config(true); // Force refresh
      // Navigate to the new agent record
      const compositeKey = new CompositeKey([{ FieldName: 'ID', Value: agent.ID }]);
        `Agent "${agent.Name}" created successfully`,
      console.error('Error saving created agent:', error);
        'Error saving agent. Please try again.',
  public runAgent(agent: AIAgentEntityExtended): void {
    // Use the test harness service for window management features
    this.testHarnessService.openForAgent(agent.ID);
  public closeTestHarness(): void {
    // No longer needed - window manages its own closure
    this.selectedAgentForTest = null;
  public getAgentIconColor(agent: AIAgentEntityExtended): string {
    // Generate a consistent color based on agent properties
    const colors = ['#17a2b8', '#28a745', '#ffc107', '#dc3545', '#6c757d', '#007bff'];
    const index = (agent.Name?.charCodeAt(0) || 0) % colors.length;
  public onOpenRecord(entityName: string, recordId: string): void {
    const compositeKey = new CompositeKey([{ FieldName: 'ID', Value: recordId }]);
  public getExecutionModeColor(mode: string): string {
      case 'Sequential': return 'info';
      case 'Parallel': return 'success';
      default: return 'info';
      case 'Sequential': return 'fa-solid fa-list-ol';
      case 'Parallel': return 'fa-solid fa-layer-group';
      default: return 'fa-solid fa-robot';
  public getAgentIcon(agent: AIAgentEntityExtended): string {
    if (agent?.LogoURL) {
    return agent?.IconClass || 'fa-solid fa-robot';
  public hasLogoURL(agent: AIAgentEntityExtended): boolean {
    return !!agent?.LogoURL;
    return 'Agents';
