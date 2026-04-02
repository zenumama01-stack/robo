import { Component, ViewContainerRef, ViewChild, AfterViewInit, OnDestroy, inject } from '@angular/core';
import { MJActionEntity, MJAIAgentActionEntity, MJAIAgentLearningCycleEntity, MJAIAgentNoteEntity, MJAIAgentPromptEntity, MJAIAgentTypeEntity, MJAIAgentRelationshipEntity } from '@memberjunction/core-entities';
import { AIAgentRunEntityExtended, AIPromptEntityExtended, AIAgentEntityExtended, } from "@memberjunction/ai-core-plus";
import { RegisterClass, MJGlobal } from '@memberjunction/global';
import { BaseFormComponent, BaseFormSectionComponent } from '@memberjunction/ng-base-forms';
import { CompositeKey, Metadata, RunView } from '@memberjunction/core';
import { UserInfoEngine } from '@memberjunction/core-entities';
import { MJAIAgentFormComponent } from '../../generated/Entities/MJAIAgent/mjaiagent.form.component';
import { DialogService } from '@progress/kendo-angular-dialog';
import { AIAgentManagementService } from './ai-agent-management.service';
import { AITestHarnessDialogService } from '@memberjunction/ng-ai-test-harness';
import { firstValueFrom, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { PromptSelectorResult } from './prompt-selector-dialog.component';
import { ActionEngineBase } from '@memberjunction/actions-base';
import { PromptSelectorDialogComponent } from './prompt-selector-dialog.component';
import { CreateAgentService, CreateAgentResult } from '@memberjunction/ng-agents';
// AgentPermissionsDialogComponent is now from @memberjunction/ng-agents (shown via ShowPermissionsDialog flag)
 * Type for sub-agent filter options
export type SubAgentFilterType = 'all' | 'child' | 'related';
 * Interface for unified sub-agent display
export interface UnifiedSubAgent {
    type: 'child' | 'related';
    relationship?: MJAIAgentRelationshipEntity;  // Only for related sub-agents
 * Enhanced AI Agent form component that extends the auto-generated base form
 * with comprehensive agent management capabilities including test harness integration,
 * related entity management, and execution history tracking.
 * - **Integrated Test Harness**: Built-in access to agent testing capabilities
 * - **Related Entity Management**: Display and manage sub-agents, prompts, and actions
 * - **Execution History**: View recent agent runs with status and timing information
 * - **Rich UI Components**: Enhanced cards, badges, and status indicators
 * - **Navigation Support**: Links to related entities and management functions
 * ## Form Sections:
 * - **Agent Details**: Basic agent configuration and settings
 * - **Sub-Agents**: Hierarchical agent relationships
 * - **Prompts**: Associated prompts with priority ordering
 * - **Actions**: Available actions and configurations
 * - **Execution History**: Recent runs with detailed status information
 * ## Usage:
 * This component is automatically loaded when editing AI Agent entities through
 * the MemberJunction form system. It extends the base generated form with
 * additional functionality while maintaining full compatibility.
 * ```html
 * <!-- Automatically used by form system -->
 * <mj-ai-agent-form [recordId]="agentId"></mj-ai-agent-form>
@RegisterClass(BaseFormComponent, 'MJ: AI Agents')
    selector: 'mj-ai-agent-form',
    templateUrl: './ai-agent-form.component.html',
    styleUrls: ['./ai-agent-form.component.css']
export class AIAgentFormComponentExtended extends MJAIAgentFormComponent implements OnDestroy {
    /** The AI Agent entity being edited */
    public record!: AIAgentEntityExtended;
    /** Subject for managing component lifecycle and cleaning up subscriptions */
    /** Track active timeouts for cleanup */
    private activeTimeouts: number[] = [];
    /** Helper method to create tracked setTimeout calls */
    private setTrackedTimeout(callback: () => void, delay: number): number {
        const timeoutId = setTimeout(() => {
            // Remove from tracking array when timeout executes
            this.removeTimeoutFromTracking(timeoutId);
        }, delay) as any as number;
        this.activeTimeouts.push(timeoutId);
        return timeoutId;
    /** Remove timeout from tracking array */
    private removeTimeoutFromTracking(timeoutId: number): void {
        const index = this.activeTimeouts.indexOf(timeoutId);
            this.activeTimeouts.splice(index, 1);
    /** ViewChild for dynamic custom section container */
    private _customSectionContainer!: ViewContainerRef;
    @ViewChild('customSectionContainer', { read: ViewContainerRef }) 
    set customSectionContainer(container: ViewContainerRef) {
        this._customSectionContainer = container;
        // When the container becomes available, load the custom section if needed
        if (container && this.agentType?.UIFormSectionKey && !this.customSectionLoaded) {
            this.setTrackedTimeout(() => this.loadCustomFormSection(), 0);
    get customSectionContainer(): ViewContainerRef {
        return this._customSectionContainer;
    /** The agent type entity for this agent */
    public agentType: MJAIAgentTypeEntity | null = null;
    /** Reference to the dynamically loaded custom section component */
    private customSectionComponent: BaseFormSectionComponent | null = null;
    /** Track if custom section has been loaded to avoid reloading */
    private customSectionLoaded = false;
    /** Track the component reference to check if it still exists */
    private customSectionComponentRef: any = null;
    /** Update custom section when EditMode changes */
    ngDoCheck() {
        if (this.customSectionComponent && this.customSectionComponent.EditMode !== this.EditMode) {
            this.customSectionComponent.EditMode = this.EditMode;
    // === Related Entity Counts ===
    /** Number of sub-agents under this agent (for backward compatibility) */
    public get subAgentCount(): number {
        return this.allSubAgents.length;
    /** Number of child sub-agents (ParentID-based) */
    public get childSubAgentCount(): number {
        return this.allSubAgents.filter(s => s.type === 'child').length;
    /** Number of related sub-agents (Relationship-based) */
    public get relatedSubAgentCount(): number {
        return this.allSubAgents.filter(s => s.type === 'related').length;
    /** Total number of sub-agents across both types */
    public get totalSubAgentCount(): number {
    /** Number of prompts associated with this agent */
    public get promptCount(): number {
        return this.agentPrompts.length;
    /** Number of actions configured for this agent */
    public get actionCount(): number {
        return this.agentActions.length;
    /** Number of learning cycles for this agent */
    public get learningCycleCount(): number {
        return this.learningCycles.length;
    /** Number of notes associated with this agent */
    public get noteCount(): number {
        return this.agentNotes.length;
    /** Number of execution history records */
    public get executionHistoryCount(): number {
        return this.recentExecutions.length;
    // === Related Entity Data for Display ===
    /** Array of sub-agent entities for card display (DEPRECATED - use allSubAgents) */
    public subAgents: AIAgentEntityExtended[] = [];
    /** Unified sub-agent data (both child and related) */
    private allSubAgents: UnifiedSubAgent[] = [];
    /** Current filter for sub-agents display */
    public subAgentFilter: SubAgentFilterType = 'all';
    /** Filtered sub-agents based on current filter */
    public get filteredSubAgents(): UnifiedSubAgent[] {
        switch (this.subAgentFilter) {
            case 'child':
                return this.allSubAgents.filter(s => s.type === 'child');
            case 'related':
                return this.allSubAgents.filter(s => s.type === 'related');
                return this.allSubAgents;
    /** Array of agent prompt entities for card display */
    public agentPrompts: AIPromptEntityExtended[] = [];
    /** Array of agent action entities for card display */
    public agentActions: MJActionEntity[] = [];
    /** Array of learning cycle entities for display */
    public learningCycles: MJAIAgentLearningCycleEntity[] = [];
    /** Array of agent note entities for display */
    public agentNotes: MJAIAgentNoteEntity[] = [];
    /** Array of recent execution records for history display */
    public recentExecutions: AIAgentRunEntityExtended[] = [];
    public totalExecutionHistoryCount: number = 0;
    /** Track which execution cards are expanded */
    public expandedExecutions: { [key: string]: boolean } = {};
    /** Search functionality for execution history */
    public executionSearchText: string = '';
    public filteredExecutions: AIAgentRunEntityExtended[] = [];
    /** Pagination state for execution history */
    public executionHistoryPageSize: number = 20;
    public executionHistoryCurrentPage: number = 1;
    public isLoadingPage: boolean = false;
    /** Cache all loaded execution records for pagination */
    private allLoadedExecutions: AIAgentRunEntityExtended[] = [];
    // === Loading States ===
    /** Main loading state for initial data load */
    public isLoadingData = true;
    /** Individual loading states for each section, start off true until loading complete */
    public loadingStates = {
        executionHistory: true,
        subAgents: true,
        prompts: true,
        actions: true,
        learningCycles: true,
        notes: true,
        customSection: true
    // === User Preferences ===
    private static readonly PREFS_KEY = 'ai-agent-form/preferences';
    private preferencesLoaded = false;
    /** Whether the form header is collapsed to a single compact line */
    public HeaderCollapsed = false;
    /** Tracked expanded/collapsed state for each panelbar section */
    public SectionStates: Record<string, boolean> = {};
    // === Dropdown Data ===
    /** Model selection mode options for the dropdown */
    public modelSelectionModes = [
        { text: 'Agent Type', value: 'Agent Type' },
        { text: 'Agent', value: 'Agent' }
    /** Agent status options for the dropdown */
    public statusOptions = [
        { text: 'Pending', value: 'Pending' },
        { text: 'Disabled', value: 'Disabled' }
    /** Agent types loaded from the database */
    public agentTypes: any[] = [];
    /** Currently selected context compression prompt */
    public selectedContextCompressionPrompt: any = null;
     * Loads agent types from the database for the dropdown
    private async loadAgentTypes(): Promise<void> {
        this.agentTypes = AIEngineBase.Instance.AgentTypes
     * Loads the context compression prompt details for display
    private async loadContextCompressionPrompt(): Promise<void> {
        if (!this.record?.ContextCompressionPromptID) {
            this.selectedContextCompressionPrompt = null;
        this.selectedContextCompressionPrompt = AIEngineBase.Instance.Prompts.find(p => p.ID === this.record.ContextCompressionPromptID);
        if (!this.selectedContextCompressionPrompt) {
            console.warn('Context compression prompt not found:', this.record.ContextCompressionPromptID);
     * Opens the prompt selector dialog for context compression prompt
    public async openContextCompressionPromptSelector(): Promise<void> {
            const dialogRef = this.dialogService.open({
                title: 'Select Context Compression Prompt',
                content: PromptSelectorDialogComponent,
                width: 800,
                height: 600
            const promptSelector = dialogRef.content.instance;
            // Configure the prompt selector for single selection
            promptSelector.config = {
                multiSelect: false,
                selectedPromptIds: this.record.ContextCompressionPromptID ? [this.record.ContextCompressionPromptID] : [],
                showCreateNew: false
            // Subscribe to the result
            promptSelector.result.subscribe({
                next: (result: PromptSelectorResult | null) => {
                    if (result && result.selectedPrompts.length > 0) {
                        const selectedPrompt = result.selectedPrompts[0];
                        this.record.ContextCompressionPromptID = selectedPrompt.ID;
                        this.selectedContextCompressionPrompt = selectedPrompt;
                error: (error: any) => {
                    console.error('Error in prompt selector dialog:', error);
            console.error('Error opening context compression prompt selector:', error);
                'Error opening prompt selector. Please try again.',
     * Clears the selected context compression prompt
    public clearContextCompressionPrompt(): void {
        this.record.ContextCompressionPromptID = null;
    // === Permission Checks for Related Entities ===
    /** Cache for permission checks to avoid repeated calculations */
    private _permissionCache = new Map<string, boolean>();
    // Main AI Agent permissions inherited from BaseFormComponent:
    // - UserCanEdit (Update permission)
    // - UserCanRead (Read permission) 
    // - UserCanCreate (Create permission)
    // - UserCanDelete (Delete permission)
    /** Check if user can create AI Agent Actions */
    public get UserCanCreateActions(): boolean {
        return this.checkEntityPermission('MJ: AI Agent Actions', 'Create');
    /** Check if user can update AI Agent Actions */
    public get UserCanUpdateActions(): boolean {
        return this.checkEntityPermission('MJ: AI Agent Actions', 'Update');
    /** Check if user can delete AI Agent Actions */
    public get UserCanDeleteActions(): boolean {
        return this.checkEntityPermission('MJ: AI Agent Actions', 'Delete');
    /** Check if user can create AI Agent Prompts */
    public get UserCanCreatePrompts(): boolean {
        return this.checkEntityPermission('MJ: AI Agent Prompts', 'Create');
    /** Check if user can update AI Agent Prompts */
    public get UserCanUpdatePrompts(): boolean {
        return this.checkEntityPermission('MJ: AI Agent Prompts', 'Update');
    /** Check if user can delete AI Agent Prompts */
    public get UserCanDeletePrompts(): boolean {
        return this.checkEntityPermission('MJ: AI Agent Prompts', 'Delete');
    /** Check if user can create AI Agents (for sub-agents) */
    public get UserCanCreateSubAgents(): boolean {
        return this.checkEntityPermission('MJ: AI Agents', 'Create');
    /** Check if user can update AI Agents (for sub-agents) */
    public get UserCanUpdateSubAgents(): boolean {
        return this.checkEntityPermission('MJ: AI Agents', 'Update');
    /** Check if user can delete AI Agents (for sub-agents) */
    public get UserCanDeleteSubAgents(): boolean {
        return this.checkEntityPermission('MJ: AI Agents', 'Delete');
    /** Check if user can create AI Agent Notes */
    public get UserCanCreateNotes(): boolean {
        return this.checkEntityPermission('MJ: AI Agent Notes', 'Create');
    /** Check if user can update AI Agent Notes */
    public get UserCanUpdateNotes(): boolean {
        return this.checkEntityPermission('MJ: AI Agent Notes', 'Update');
    /** Check if user can delete AI Agent Notes */
    public get UserCanDeleteNotes(): boolean {
        return this.checkEntityPermission('MJ: AI Agent Notes', 'Delete');
    /** Check if user can view AI Agent Learning Cycles */
    public get UserCanViewLearningCycles(): boolean {
        return this.checkEntityPermission('MJ: AI Agent Learning Cycles', 'Read');
    /** Check if user can view AI Agent Runs (execution history) */
    public get UserCanViewExecutionHistory(): boolean {
        return this.checkEntityPermission('MJ: AI Agent Runs', 'Read');
    /** Check if user can create AI Prompts (needed for creating new prompts) */
    public get UserCanCreateAIPrompts(): boolean {
        return this.checkEntityPermission('MJ: AI Prompts', 'Create');
    /** Check if user can create both AI Prompts and AI Agent Prompts (for createNewPrompt functionality) */
    public get UserCanCreateNewPrompts(): boolean {
        return this.UserCanCreateAIPrompts && this.UserCanCreatePrompts;
     * Helper method to check entity permissions with caching
     * @param entityName - The name of the entity to check permissions for
     * @param permissionType - The type of permission to check (Create, Read, Update, Delete)
     * @returns boolean indicating if user has the permission
    private checkEntityPermission(entityName: string, permissionType: 'Create' | 'Read' | 'Update' | 'Delete'): boolean {
        const cacheKey = `${entityName}_${permissionType}`;
        if (this._permissionCache.has(cacheKey)) {
            return this._permissionCache.get(cacheKey)!;
                console.warn(`Entity '${entityName}' not found for permission check`);
                this._permissionCache.set(cacheKey, false);
            const userPermissions = entityInfo.GetUserPermisions(md.CurrentUser);
                case 'Create':
                    hasPermission = userPermissions.CanCreate;
                case 'Read':
                    hasPermission = userPermissions.CanRead;
                case 'Update':
                    hasPermission = userPermissions.CanUpdate;
                case 'Delete':
                    hasPermission = userPermissions.CanDelete;
            this._permissionCache.set(cacheKey, hasPermission);
            return hasPermission;
            console.error(`Error checking ${permissionType} permission for ${entityName}:`, error);
     * Clears the permission cache. Call this when user context changes or permissions are updated.
    public clearPermissionCache(): void {
        this._permissionCache.clear();
    // === Transaction-based editing support ===
    /** Now using BaseFormComponent's PendingRecords system exclusively */
    /** Flag to indicate if there are unsaved changes */
    public hasUnsavedChanges = false;
    // Emergency circuit breaker to prevent infinite loops
    private _changeDetectionCount = 0;
    private static readonly MAX_CHANGE_DETECTIONS = 50;
    // === Original State for Cancel/Revert ===
    /** Snapshots of original data for reverting UI changes */
    private originalSnapshots: {
        agentPrompts: AIPromptEntityExtended[];
        agentActions: MJActionEntity[];
        subAgents: AIAgentEntityExtended[];
        promptCount: number;
        learningCycleCount: number;
        noteCount: number;
        executionHistoryCount: number;
    // Dependency injection using inject() function
    private sharedService = inject(SharedService);
    private dialogService = inject(DialogService);
    private viewContainerRef = inject(ViewContainerRef);
    private agentManagementService = inject(AIAgentManagementService);
    private testHarnessService = inject(AITestHarnessDialogService);
    private createAgentService = inject(CreateAgentService);
     * After view initialization, load any custom form section if defined
        // Restore user preferences (header state, section expand/collapse)
        this.loadUserPreferences();
        // Load agent types for dropdown (needed for both new and existing records)
        await AIEngineBase.Instance.Config(false); // in UI context user and provider default to global
        await ActionEngineBase.Instance.Config(false);
        await this.loadAgentTypes();
        // Load context compression prompt if one is set
        if (this.record?.ContextCompressionPromptID) {
            await this.loadContextCompressionPrompt();
            await this.loadRelatedCounts(false); // no need to force refresh on initial load
            await this.loadCurrentAgentType();
            // Schedule change detection - safer than manual detectChanges()
            this.cdr.markForCheck();
            // Defer custom section loading to next tick after DOM updates
            this.setTrackedTimeout(() => {
                this.loadCustomFormSection();
            // Start background timer for running time updates
            this.startRunningTimeUpdater();
     * Loads counts and preview data for all related entities including sub-agents,
     * prompts, actions, learning cycles, notes, and execution history. 
     * This data populates the various expander panels in the enhanced form interface.
    private async loadRelatedCounts(forceRefresh: boolean): Promise<void> {
        // Reset pagination state on refresh
        if (forceRefresh) {
            this.executionHistoryCurrentPage = 1;
            this.isLoadingPage = false;
            this.allLoadedExecutions = [];
            // Don't clear recentExecutions - keep existing data visible while loading
        // Set loading state
        this.isLoadingData = true;
        this.loadingStates = {
        this.cdr.detectChanges(); // update UI
            await AIEngineBase.Instance.Config(true); // force refresh
            // Clear unified sub-agents array
            this.allSubAgents = [];
            // Load child sub-agents (ParentID-based)
            const childAgents = AIEngineBase.Instance.Agents.filter(a => a.ParentID === this.record.ID);
            for (const agent of childAgents) {
                this.allSubAgents.push({
                    type: 'child'
            // Also populate the deprecated subAgents array for backward compatibility
            this.subAgents = [...childAgents];
            // Load related sub-agents (Relationship-based)
            const relationshipsResult = await rv.RunView<MJAIAgentRelationshipEntity>({
                ExtraFilter: `AgentID='${this.record.ID}' AND Status='Active'`,
            if (relationshipsResult.Success && relationshipsResult.Results) {
                for (const relationship of relationshipsResult.Results) {
                    const agent = AIEngineBase.Instance.Agents.find(
                        a => a.ID === relationship.SubAgentID
                    if (agent) {
                            type: 'related',
                            relationship
            // Sort: child agents first, then by name
            this.allSubAgents.sort((a, b) => {
                if (a.type !== b.type) {
                    return a.type === 'child' ? -1 : 1;
                return (a.agent.Name || '').localeCompare(b.agent.Name || '');
            this.agentPrompts = AIEngineBase.Instance.Prompts.filter(p => {
                const filteredAgentPrompts = AIEngineBase.Instance.AgentPrompts.filter(ap => ap.AgentID === this.record.ID);
                return filteredAgentPrompts.some(ap => ap.PromptID === p.ID);
            this.agentActions = ActionEngineBase.Instance.Actions.filter(a => {
                const filteredAgentActions = AIEngineBase.Instance.AgentActions.filter(aa => aa.AgentID === this.record.ID);
                return filteredAgentActions.some(aa => aa.ActionID === a.ID);
            // Execute all queries in a single batch for better performance
            const results = await rv.RunViews([
                // Learning cycles
                    EntityName: 'MJ: AI Agent Learning Cycles',
                    // limit fields
                    Fields: ["ID","Name","StartedAt", "EndedAt","Status","AgentID"],
                    ExtraFilter: `AgentID='${this.record.ID}'`,
                    OrderBy: 'StartedAt DESC'
                // Notes
                    Fields: ["ID","Name", "AgentID", "AgentNoteType","AgentNoteTypeID","UserID"],
                    ExtraFilter: `AgentID='${this.record.ID}'`
                // Execution history (initial page)
                    Fields: [ // limit what we take from runs as this is where we can have a LOT come down if we include JSON fields
                        "ID","AgentID","ParentRunID","Status","StartedAt","CompletedAt",
                        "Success","TotalTokensUsed","TotalCost","TotalCostRollUp","TotalTokensUsedRollUp",
                        "Configuration","ConversationID","Result","ErrorMessage","__mj_CreatedAt"
                    MaxRows: this.executionHistoryPageSize
                // Agent permissions (to determine open-to-everyone state)
            // Process results in the same order as queries
            if (results && results.length > 0) {
                this.learningCycles = results[0].Results as MJAIAgentLearningCycleEntity[] || [];
                this.agentNotes = results[1].Results as MJAIAgentNoteEntity[] || [];
                this.recentExecutions = results[2].Results as AIAgentRunEntityExtended[] || [];
                this.totalExecutionHistoryCount = results[2].TotalRowCount;
                // Initialize cache with first page of results
                this.allLoadedExecutions = [...this.recentExecutions];
                // Initialize filtered executions
                this.filteredExecutions = [...this.recentExecutions];
                // Determine open-to-everyone state from permissions query
                const permissionRows = results[3]?.Results || [];
                this.IsOpenToEveryone = permissionRows.length === 0;
            // Create snapshot for cancel/revert functionality
            this.createOriginalSnapshot();
            console.error('Error loading related data:', error);
            // Set all counts to 0 on error to ensure UI shows proper empty states
            // Clear loading states
            this.isLoadingData = false;
                executionHistory: false,
                subAgents: false,
                prompts: false,
                actions: false,
                learningCycles: false,
                notes: false,
                customSection: false
     * Creates a snapshot of the current UI state for cancel/revert functionality
    private createOriginalSnapshot() {
        this.originalSnapshots = {
            agentPrompts: [...this.agentPrompts], // Deep copy of arrays
            agentActions: [...this.agentActions],
            subAgents: [...this.subAgents],
            promptCount: this.promptCount,
            actionCount: this.actionCount,
            subAgentCount: this.subAgentCount,
            learningCycleCount: this.learningCycleCount,
            noteCount: this.noteCount,
            executionHistoryCount: this.executionHistoryCount
     * Navigates to the next page of execution history
    public async goToNextPage(): Promise<void> {
        if (!this.hasNextPage || this.isLoadingPage || !this.record?.ID) {
        const nextPage = this.executionHistoryCurrentPage + 1;
        await this.loadPage(nextPage);
     * Navigates to the previous page of execution history
    public async goToPreviousPage(): Promise<void> {
        if (!this.hasPreviousPage || this.isLoadingPage) {
        const previousPage = this.executionHistoryCurrentPage - 1;
        await this.loadPage(previousPage);
     * Loads a specific page of execution history, using cache when available
    private async loadPage(pageNumber: number): Promise<void> {
        if (!this.record?.ID) {
        this.isLoadingPage = true;
            const startIndex = (pageNumber - 1) * this.executionHistoryPageSize;
            const endIndex = startIndex + this.executionHistoryPageSize;
            // Check if we have this page in cache
            const cachedPageData = this.allLoadedExecutions.slice(startIndex, endIndex);
            const hasFullPageInCache = cachedPageData.length === this.executionHistoryPageSize;
            const isLastPage = endIndex >= this.totalExecutionHistoryCount;
            const hasPartialPageInCache = isLastPage && cachedPageData.length > 0 &&
                                         cachedPageData.length === (this.totalExecutionHistoryCount - startIndex);
            if (hasFullPageInCache || hasPartialPageInCache) {
                // We have the page in cache (either full page or complete last page)
                this.executionHistoryCurrentPage = pageNumber;
                this.recentExecutions = cachedPageData;
                await this.applySearchFilter();
                // Need to load from database
                const result = await rv.RunView<AIAgentRunEntityExtended>({
                    Fields: [
                    MaxRows: this.executionHistoryPageSize,
                    StartRow: startIndex > 0 ? startIndex : undefined,
                    // Update cache - ensure we have enough space
                    while (this.allLoadedExecutions.length < startIndex) {
                        this.allLoadedExecutions.push(null as any);
                    // Insert the new results into cache
                    this.allLoadedExecutions.splice(startIndex, result.Results.length, ...result.Results);
                    this.recentExecutions = result.Results;
            console.error('Error loading page:', error);
     * Checks if there is a next page available
    public get hasNextPage(): boolean {
        const maxPage = Math.ceil(this.totalExecutionHistoryCount / this.executionHistoryPageSize);
        return this.executionHistoryCurrentPage < maxPage;
     * Checks if there is a previous page available
    public get hasPreviousPage(): boolean {
        return this.executionHistoryCurrentPage > 1;
     * Gets the total number of pages
    public get totalPages(): number {
        return Math.ceil(this.totalExecutionHistoryCount / this.executionHistoryPageSize);
     * Loads the agent type entity for this agent
    private async loadCurrentAgentType(): Promise<void> {
        if (!this.record?.TypeID) {
            this.agentType = await md.GetEntityObject<MJAIAgentTypeEntity>('MJ: AI Agent Types');
            if (this.agentType) {
                await this.agentType.Load(this.record.TypeID);
            console.error('Error loading agent type:', error);
            this.agentType = null;
     * Dynamically loads a custom form section if the agent type defines one
    private loadCustomFormSection(): void {
        if (!this.agentType?.UIFormSectionKey || !this.customSectionContainer) {
        // Check if component still exists in container
        if (this.customSectionLoaded && this.customSectionContainer.length > 0) {
        this.loadingStates.customSection = true;
            // Build the full registration key (Entity.Section pattern)
            const sectionKey = `AI Agents.${this.agentType.UIFormSectionKey}`;
            // Get the component registration from the class factory
            const registration = MJGlobal.Instance.ClassFactory.GetRegistration(BaseFormSectionComponent, sectionKey);
            if (registration && registration.SubClass) {
                // Clear any existing custom section
                this.customSectionContainer.clear();
                // Create the component
                const componentRef = this.customSectionContainer.createComponent(registration.SubClass);
                this.customSectionComponent = componentRef.instance as BaseFormSectionComponent;
                this.customSectionComponentRef = componentRef;
                // Pass the record and edit mode to the custom section
                this.customSectionComponent.record = this.record;
                // Mark as loaded
                this.customSectionLoaded = true;
            console.error('Error loading custom form section:', error);
            this.loadingStates.customSection = false;
     * Handles state change events for the custom section panel
     * @param event The panel bar state change event
    public onCustomSectionStateChange(event: { expanded: boolean }): void {
        // When panel is expanded, check if we need to load or reload the custom section
        if (event.expanded && this.agentType?.UIFormSectionKey) {
            // Always try to load on expand to handle cases where container might have been recreated
    // === User Preferences (Header & Section State) ===
    /** Load saved preferences for header state and section expand/collapse */
    private loadUserPreferences(): void {
            const raw = UserInfoEngine.Instance.GetSetting(AIAgentFormComponentExtended.PREFS_KEY);
            if (raw) {
                const prefs = JSON.parse(raw);
                this.HeaderCollapsed = prefs.headerCollapsed ?? false;
                this.SectionStates = prefs.sectionStates ?? {};
            console.error('Error loading AI Agent form preferences:', error);
        this.preferencesLoaded = true;
    /** Persist preferences with debounce */
    private persistPreferences(): void {
        if (!this.preferencesLoaded) return;
        const prefs = {
            headerCollapsed: this.HeaderCollapsed,
            sectionStates: this.SectionStates
        UserInfoEngine.Instance.SetSettingDebounced(
            AIAgentFormComponentExtended.PREFS_KEY,
            JSON.stringify(prefs)
    /** Toggle the header between expanded and collapsed modes */
    public ToggleHeaderCollapsed(): void {
        this.HeaderCollapsed = !this.HeaderCollapsed;
        this.persistPreferences();
    /** Get the expanded state for a panelbar section, falling back to a default */
    public GetSectionExpanded(sectionId: string, defaultValue: boolean): boolean {
        if (this.preferencesLoaded && sectionId in this.SectionStates) {
            return this.SectionStates[sectionId];
    /** Handle panelbar stateChange — fires when any section expands or collapses */
    public OnPanelBarStateChange(event: { items: Array<{ id: string; expanded: boolean }> }): void {
        if (!event?.items) return;
        for (const item of event.items) {
            if (item.id) {
                this.SectionStates[item.id] = item.expanded;
                // Keep existing custom section load logic
                if (item.id === 'custom' && item.expanded) {
                    this.onCustomSectionStateChange({ expanded: true });
     * Restores the UI to its original state using saved snapshots
    private restoreFromSnapshots() {
        if (this.originalSnapshots) {
            // Restore arrays (create new copies to ensure reactivity)
            this.agentPrompts = [...this.originalSnapshots.agentPrompts];
            this.agentActions = [...this.originalSnapshots.agentActions];
            this.subAgents = [...this.originalSnapshots.subAgents];
            // Reset other UI state
            this.hasUnsavedChanges = false;
            // Mark for check instead of forcing immediate detection
     * Override CancelEdit to restore UI state when user cancels changes
    public override CancelEdit(): void {
        // Set flag to indicate we're performing a cancel operation
        this.isPerformingCancel = true;
            // CRITICAL: Clear our pending records BEFORE calling parent
            // This ensures that any prompt/action/sub-agent changes we added don't persist
            this.PendingRecords.length = 0;
            // Call the parent CancelEdit first (this handles main record revert and pending records)
            super.CancelEdit();
            // Restore our UI state after parent cancel
            this.restoreFromSnapshots();
            // Reset the unsaved changes flag
            // Always reset the flag
            this.isPerformingCancel = false;
     * Opens the integrated test harness for the current agent.
     * Validates that the agent has been saved before allowing testing.
     * Shows a notification if the agent needs to be saved first.
    public openTestHarness() {
                'Please save the AI agent before testing',
                'warning',
                4000
        // Use the new test harness dialog service
        // Don't pass viewContainerRef so window is top-level
        this.testHarnessService.openForAgent(this.record.ID);
     * Opens the permissions management dialog for this agent.
     * Allows viewing and editing user/role-based permissions for the agent.
    /** Controls visibility of the new permissions dialog from @memberjunction/ng-agents */
    public ShowPermissionsDialog = false;
    /** True when no explicit permission records exist (agent is open to everyone) */
    public IsOpenToEveryone = true;
    public openPermissionsDialog() {
                'Please save the AI agent before managing permissions',
        this.ShowPermissionsDialog = true;
    public async onPermissionsDialogClosed() {
        this.ShowPermissionsDialog = false;
        // Refresh open-to-everyone state in case permissions were added/removed
        await this.refreshPermissionState();
    private async refreshPermissionState(): Promise<void> {
        const result = await rv.RunView<{ID: string}>({
            this.IsOpenToEveryone = (result.Results || []).length === 0;
     * Returns the appropriate color for the agent status badge.
     * Uses standard color coding: green for active, yellow for pending, gray for disabled.
     * @returns CSS color value for the status badge
    public getStatusBadgeColor(): string {
        switch (this.record?.Status) {
            case 'Active': return '#28a745';
            case 'Pending': return '#ffc107';
            case 'Disabled': return '#6c757d';
            default: return '#6c757d';
     * Event handler for test harness visibility changes.
     * Updates the component state when the test harness is opened or closed.
     * @param isVisible - Whether the test harness is currently visible
     * Gets the count of sub-agents
    public getSubAgentCount(): number {
        return this.subAgentCount;
     * Gets the count of prompts
    public getPromptCount(): number {
        return this.promptCount;
     * Gets the count of actions
    public getActionCount(): number {
        return this.actionCount;
     * Gets the icon for the execution mode
    public getExecutionModeIcon(mode: string): string {
            case 'Sequential':
                return 'fa-solid fa-list-ol';
            case 'Parallel':
                return 'fa-solid fa-layer-group';
                return 'fa-solid fa-robot';
     * Gets the agent's display icon
     * Prioritizes LogoURL, falls back to IconClass, then default robot icon
    public getAgentIcon(): string {
        if (this.record?.LogoURL) {
            // LogoURL is used in img tag, not here
        return this.record?.IconClass || 'fa-solid fa-robot';
     * Checks if the agent has a logo URL (for image display)
    public hasLogoURL(): boolean {
        return !!this.record?.LogoURL;
     * Gets the icon for a sub-agent
    public getSubAgentIcon(subAgent: AIAgentEntityExtended): string {
        if (subAgent?.LogoURL) {
        return subAgent?.IconClass || 'fa-solid fa-robot';
     * Gets the icon class for an action
     * Falls back to default bolt icon if no IconClass is set
    public getActionIcon(action: MJActionEntity): string {
        return action?.IconClass || 'fa-solid fa-bolt';
     * Checks if a sub-agent has a logo URL
    public hasSubAgentLogoURL(subAgent: AIAgentEntityExtended): boolean {
        return !!subAgent?.LogoURL;
     * Creates a new sub-agent using the CreateAgentService slide-in panel.
     * Uses the new unified agent creation UI from @memberjunction/ng-agents.
    public async createSubAgent() {
            this.createAgentService.OpenSubAgentSlideIn(
                this.record.ID,
                this.record.Name || 'Agent'
            ).pipe(takeUntil(this.destroy$)).subscribe({
                next: async (dialogResult) => {
                    if (!dialogResult.Cancelled && dialogResult.Result) {
                        await this.handleSubAgentCreated(dialogResult.Result);
                    console.error('Error in create sub-agent slide-in:', error);
                        'Error opening sub-agent creation panel. Please try again.',
            console.error('Error in createSubAgent:', error);
                'Error creating sub-agent. Please try again.',
     * Handles the result from the create sub-agent slide-in.
     * Adds entities to PendingRecords for atomic save with parent.
    private async handleSubAgentCreated(result: CreateAgentResult): Promise<void> {
            const subAgent = result.Agent;
            // Handle deferred sub-agent creation if parent is not saved
            if (!this.record.IsSaved) {
                // Store a temporary reference to the parent - will be resolved during save
                subAgent.Set('_tempParentId', this.record.ID);
            // Add the sub-agent to pending records
            this.PendingRecords.push({
                entityObject: subAgent,
                action: 'save'
            // Add agent prompt links to pending records
            if (result.AgentPrompts) {
                for (const agentPrompt of result.AgentPrompts) {
                        entityObject: agentPrompt,
            // Add agent action links to pending records
            if (result.AgentActions) {
                for (const agentAction of result.AgentActions) {
                        entityObject: agentAction,
            // Update UI to show the new sub-agent
            this.subAgents.push(subAgent);
            this.hasUnsavedChanges = true;
                `Sub-agent "${subAgent.Name}" created and will be saved when you save the parent agent`,
                'success',
            console.error('Error processing created sub-agent:', error);
                'Error processing created sub-agent. Please try again.',
     * Adds a new prompt to the agent (deferred until save)
    public async addPrompt() {
        // Get currently linked and pending prompt IDs for pre-selection
        const currentPromptIds = this.agentPrompts.map(ap => ap.ID);
        const pendingAddIds = this.PendingRecords
            .filter(p => p.entityObject.EntityInfo.Name === 'MJ: AI Agent Prompts' && p.action === 'save')
            .map(p => p.entityObject.Get('PromptID'));
        const allLinkedIds = [...currentPromptIds, ...pendingAddIds];
            this.agentManagementService.openPromptSelectorDialog({
                title: 'Add Prompts to Agent',
                multiSelect: true,
                selectedPromptIds: [],
                showCreateNew: true,
                linkedPromptIds: allLinkedIds,
                viewContainerRef: this.viewContainerRef
            }).pipe(takeUntil(this.destroy$)).subscribe({
                next: async (result) => {
                        // Filter out already linked or pending prompts
                        const newPrompts = result.selectedPrompts.filter(prompt => 
                            !allLinkedIds.includes(prompt.ID)
                        if (newPrompts.length === 0) {
                                'All selected prompts are already linked to this agent',
                                'info',
                        // Add to pending changes (defer until save)
                        for (const prompt of newPrompts) {
                            const agentPrompt = await md.GetEntityObject<MJAIAgentPromptEntity>('MJ: AI Agent Prompts');
                            agentPrompt.AgentID = this.record.ID;
                        // Update UI to show the new prompts (cast to extended type for display)
                        this.agentPrompts.push(...(newPrompts as AIPromptEntityExtended[]));
                        // Show success notification
                            `${newPrompts.length} prompt${newPrompts.length === 1 ? '' : 's'} will be added when you save the agent`,
                    } else if (result && result.createNew) {
                        // User wants to create a new prompt
                        await this.createNewPrompt();
                    console.error('Error opening prompt selector:', error);
            console.error('Error in addPrompt:', error);
                'Error adding prompts. Please try again.',
     * Handle context compression toggle and reset related fields when disabled
    public onContextCompressionToggle(value: any) {
        const enabled = value === true || value === 'true';
        if (!enabled) {
            // Reset context compression related fields to null when disabled
            this.record.ContextCompressionMessageThreshold = null;
            this.record.ContextCompressionMessageRetentionCount = null;
     * Opens the modern Add Action dialog for selecting actions (deferred until save)
    public async configureActions() {
        // Get currently linked and pending action IDs for pre-selection
        const currentActionIds = this.agentActions.map(aa => aa.ID);
            .filter(p => p.entityObject.EntityInfo.Name === 'MJ: AI Agent Actions' && p.action === 'save')
            .map(p => p.entityObject.Get('ActionID'));
        const allLinkedIds = [...currentActionIds, ...pendingAddIds];
        this.agentManagementService.openAddActionDialog({
            agentId: this.record.ID,
            agentName: this.record.Name || 'Agent',
            existingActionIds: allLinkedIds,
            next: async (selectedActions) => {
                if (selectedActions && selectedActions.length > 0) {
                    // Filter out already linked or pending actions
                    const newActions = selectedActions.filter(action => 
                        !allLinkedIds.includes(action.ID)
                    if (newActions.length === 0) {
                            'All selected actions are already linked to this agent',
                    for (const action of newActions) {
                        const agentAction = await md.GetEntityObject<MJAIAgentActionEntity>('MJ: AI Agent Actions');
                        agentAction.NewRecord();
                        agentAction.AgentID = this.record.ID;
                        agentAction.ActionID = action.ID;
                        agentAction.Status = 'Active';
                    // Update UI to show the new actions
                    this.agentActions.push(...newActions);
                        `${newActions.length} action${newActions.length === 1 ? '' : 's'} will be added when you save the agent`,
                console.error('Error in add action dialog:', error);
                    'Error opening action selection dialog. Please try again.',
     * Gets the status icon for execution runs
    public getExecutionStatusIcon(status: string): string {
        switch (status?.toLowerCase()) {
                return 'fa-solid fa-check-circle';
                return 'fa-solid fa-exclamation-triangle';
            case 'in_progress':
                return 'fa-solid fa-spinner fa-spin';
            case 'pending':
                return 'fa-solid fa-clock';
                return 'fa-solid fa-question-circle';
     * Gets the status color for execution runs
    public getExecutionStatusColor(status: string): string {
                return '#28a745';
                return '#dc3545';
                return '#17a2b8';
                return '#ffc107';
                return '#6c757d';
    public formatExecutionTimeFromDates(startDate: Date, endDate: Date): string {
        if (!startDate || !endDate) return 'N/A';
        // check to see if we have dates or timestamps
        let startTime;
        let endTime;
        if (typeof startDate === 'string') {
            startTime = new Date(startDate).getTime();
        else if (typeof startDate === 'number') {   
            startTime = startDate;
            startTime = startDate.getTime();
        if (typeof endDate === 'string') {
            endTime = new Date(endDate).getTime();
        else if (typeof endDate === 'number') {
            endTime = endDate;
            endTime = endDate.getTime();
        if (isNaN(startTime) || isNaN(endTime)) 
            return 'N/A';        
        const milliseconds = endTime - startTime;
        return this.formatExecutionTime(milliseconds);
     * Formats execution time
    public formatExecutionTime(milliseconds: number): string {
        if (!milliseconds) return 'N/A';
        if (milliseconds >= 60000) {
            const seconds = ((milliseconds % 60000) / 1000).toFixed(1);
        } else if (milliseconds >= 1000) {
            return `${(milliseconds / 1000).toFixed(1)}s`;
     * Formats token count with appropriate units (K for thousands, M for millions)
    public formatTokenCount(tokens: number | null): string {
        if (tokens == null || tokens === 0) return '0';
        if (tokens >= 1000000) {
            return `${(tokens / 1000000).toFixed(1)}M`;
        } else if (tokens >= 1000) {
            return `${(tokens / 1000).toFixed(1)}K`;
     * Formats cost with appropriate precision
    public formatCost(cost: number | null): string {
        if (cost == null || cost === 0) return '0.00';
        if (cost >= 1) {
            return cost.toFixed(2);
        } else if (cost >= 0.01) {
            return cost.toFixed(3);
            return cost.toFixed(4);
     * Gets the running time for an execution that hasn't completed yet
     * Uses a cached timestamp to avoid ExpressionChangedAfterItHasBeenCheckedError
    private _runningTimeCache = new Map<string, { time: string, timestamp: number }>();
    private _runningTimeUpdater: any = null;
    public getRunningTime(startDate: Date): string {
        if (!startDate) return 'N/A';
        const startTime = new Date(startDate).getTime();
        if (isNaN(startTime)) return 'N/A';
        const cacheKey = startTime.toString();
        const cached = this._runningTimeCache.get(cacheKey);
        // Update cache every second to avoid constant changes
        if (!cached || now - cached.timestamp > 1000) {
            const milliseconds = now - startTime;
            const timeString = this.formatExecutionTime(milliseconds);
            this._runningTimeCache.set(cacheKey, { time: timeString, timestamp: now });
            // Don't trigger change detection here - let the background timer handle it
            return timeString;
        return cached.time;
     * Starts the background timer for updating running times
    private startRunningTimeUpdater() {
        if (!this._runningTimeUpdater) {
            this._runningTimeUpdater = setInterval(() => {
                if (!this.destroy$.closed) {
                    // Force cache refresh by clearing old entries
                    for (const [key, cached] of this._runningTimeCache.entries()) {
                        if (now - cached.timestamp > 500) { // Refresh every 500ms
                            this._runningTimeCache.delete(key);
            }, 1000); // Update every second
     * Gets the priority badge color
    public getPriorityBadgeColor(priority: number): string {
        if (priority <= 1) return '#dc3545'; // High priority - red
        if (priority <= 5) return '#ffc107'; // Medium priority - yellow
        return '#28a745'; // Low priority - green
     * Gets the priority label
    public getPriorityLabel(priority: number): string {
        if (priority <= 1) return 'High';
        if (priority <= 5) return 'Medium';
        return 'Low';
     * Navigates to a related entity
    public navigateToEntity(entityName: string, recordId: string) {
        this.sharedService.OpenEntityRecord(entityName, CompositeKey.FromID(recordId));
     * Toggles the expanded state of an execution card
    public toggleExecutionExpanded(executionId: string) {
        this.expandedExecutions[executionId] = !this.expandedExecutions[executionId];
     * Handles search text changes - debounced to avoid excessive processing
    public onExecutionSearchChange(): void {
        // Debounce search to avoid excessive processing during typing
        if (this._searchDebounceTimer) {
            clearTimeout(this._searchDebounceTimer);
        this._searchDebounceTimer = setTimeout(() => {
            this.applySearchFilter();
    private _searchDebounceTimer: any = null;
     * Applies search filter across all cached records and loads from database if needed
    private async applySearchFilter(): Promise<void> {
        if (!this.executionSearchText || this.executionSearchText.trim() === '') {
            // No search text - show current page's executions
        const searchLower = this.executionSearchText.toLowerCase().trim();
        // First, search across all cached records
        const cachedMatches = this.allLoadedExecutions.filter(execution =>
            execution && execution.ID.toLowerCase().includes(searchLower)
        if (cachedMatches.length > 0) {
            // Found matches in cache
            this.filteredExecutions = cachedMatches;
            // No matches in cache - search database
            await this.searchExecutionsFromDatabase(searchLower);
     * Searches execution history from database when not found in cache
    private async searchExecutionsFromDatabase(searchText: string): Promise<void> {
                ExtraFilter: `AgentID='${this.record.ID}' AND ID LIKE '%${searchText}%'`,
                MaxRows: 100, // Limit search results
                this.filteredExecutions = result.Results;
                this.filteredExecutions = [];
            console.error('Error searching executions:', error);
     * Opens the full execution record in a new view
    public openExecutionRecord(executionId: string) {
        this.sharedService.OpenEntityRecord('MJ: AI Agent Runs', CompositeKey.FromID(executionId));
     * Gets a preview of the execution result for collapsed view
    public getExecutionResultPreview(execution: AIAgentRunEntityExtended, trimLongMessages: boolean): string {
            if (!execution.Result) return 'No result';
            // Try to parse the result as JSON
            const parsed = JSON.parse(execution.Result);
            // Extract the user message if it exists
            if (parsed.returnValue?.nextStep?.userMessage) {
                const message = parsed.returnValue.nextStep.userMessage;
                if (trimLongMessages)
                    return message.length > 120 ? message.substring(0, 120) + '...' : message;
            // Otherwise return the stringified result
            const stringified = JSON.stringify(parsed, null, 2);
                return stringified.length > 120 ? stringified.substring(0, 120) + '...' : stringified;
                return stringified;
            // If not JSON, just return the string
            const result = execution.Result || '';
                return result.length > 120 ? result.substring(0, 120) + '...' : result;
     * Gets the full execution result message for expanded view
    public getExecutionResultMessage(execution: AIAgentRunEntityExtended): string {
                return parsed.returnValue.nextStep.userMessage;
            // Otherwise return the pretty-printed JSON
            return JSON.stringify(parsed, null, 2);
            return execution.Result || '';
     * Refreshes the related data and updates snapshots
    public async refreshRelatedData() {
            await this.loadRelatedCounts(true); // force refresh
                'Related data refreshed',
                2000
     * Manually refreshes the snapshot for cancel/revert functionality
     * Useful when you want to reset the "original state" to the current state
    public refreshSnapshot() {
            'Current state saved as new baseline',
     * Debug method to check current pending records state
     * Useful for troubleshooting cancel/revert issues
    public debugPendingRecords() {
        // Debug method for troubleshooting - console output removed for production
     * Adds a new note to the agent
    public addNote() {
            'Opening new note form...',
        // In a full implementation, this would open a new AI Agent Note form
        // with AgentID pre-populated to this.record.ID
     * Creates a new prompt and links it to the agent
    private async createNewPrompt() {
            this.agentManagementService.openCreatePromptDialog({
                title: `Create New Prompt for ${this.record.Name || 'Agent'}`,
                initialName: '',
                    if (result && result.prompt) {
                            // Get current user using proper MJ pattern
                            const currentUserId = md.CurrentUser.ID;
                            // Add the prompt to PendingRecords (will be saved with agent)
                                entityObject: result.prompt,
                            // Add template to PendingRecords if created
                            if (result.template) {
                                // Set UserID on template (required field)
                                result.template.UserID = currentUserId;
                                    entityObject: result.template,
                            // Add template contents to PendingRecords if created
                            if (result.templateContents && result.templateContents.length > 0) {
                                for (const content of result.templateContents) {
                                    // Template content does not have UserID field, no manual user assignment needed
                                        entityObject: content,
                            // Create the AI Agent Prompt link
                            agentPrompt.PromptID = result.prompt.ID;
                            // AI Agent Prompt does not have UserID field, no manual user assignment needed
                            // Update UI to show the new prompt
                            this.agentPrompts.push(result.prompt);
                            // Trigger change detection to update UI
                                `New prompt "${result.prompt.Name}" will be created and linked when you save the agent`,
                            console.error('Error processing created prompt:', error);
                                'Error processing created prompt. Please try again.',
                    console.error('Error in create prompt dialog:', error);
                        'Error opening prompt creation dialog. Please try again.',
            console.error('Error in createNewPrompt:', error);
                'Error creating new prompt. Please try again.',
     * Removes a prompt from the agent (deferred until save)
    public async removePrompt(prompt: AIPromptEntityExtended, event: Event) {
        event.stopPropagation(); // Prevent navigation
        const confirmDialog = this.dialogService.open({
            title: 'Remove Prompt',
            content: `Are you sure you want to remove the prompt "${prompt.Name}" from this agent?`,
            actions: [
                { text: 'Cancel' },
                { text: 'Remove', themeColor: 'error' }
            width: 450,
            const result = await firstValueFrom(confirmDialog.result);
            if (result && (result as any).text === 'Remove') {
                    // Check if this is a pending add (not yet in database)
                    const pendingAddIndex = this.PendingRecords.findIndex(
                        p => p.entityObject.EntityInfo.Name === 'MJ: AI Agent Prompts' && 
                             p.action === 'save' && 
                             p.entityObject.Get('PromptID') === prompt.ID
                    if (pendingAddIndex >= 0) {
                        // Remove from pending adds
                        this.PendingRecords.splice(pendingAddIndex, 1);
                        // Find the existing AI Agent Prompt link record for deferred deletion
                        const linkResult = await rv.RunView<MJAIAgentPromptEntity>({
                            ExtraFilter: `AgentID='${this.record.ID}' AND PromptID='${prompt.ID}'`,
                        if (linkResult.Success && linkResult.Results && linkResult.Results.length > 0) {
                            const agentPromptToDelete = linkResult.Results[0];
                            // Add to pending deletions
                                entityObject: agentPromptToDelete,
                                action: 'delete'
                            throw new Error('AI Agent Prompt link not found');
                    // Remove from UI immediately
                    const promptIndex = this.agentPrompts.findIndex(p => p.ID === prompt.ID);
                    if (promptIndex >= 0) {
                        this.agentPrompts.splice(promptIndex, 1);
                        `Prompt "${prompt.Name}" will be removed when you save the agent`,
                    console.error('Error removing prompt from agent:', error);
                        'Failed to remove prompt',
        } catch (dialogError) {
            console.error('Error with dialog:', dialogError);
     * Updates payload field values from code editor
    public updatePayloadField(fieldName: string, value: any) {
        if (this.record) {
            // Handle the value - it might be a string or an event
            const newValue = typeof value === 'string' ? value : value?.target?.value || value;
            (this.record as any)[fieldName] = newValue;
     * Opens the sub-agent selector dialog for adding sub-agents (deferred until save)
    public async addSubAgents() {
            // Get list of already pending sub-agent IDs to filter duplicates
            const pendingSubAgentIds = this.PendingRecords
                .filter(p => p.entityObject.EntityInfo.Name === 'MJ: AI Agents' && 
                            p.entityObject.Get('ParentID') === this.record.ID)
                .map(p => p.entityObject.Get('ID'));
            const existingSubAgentIds = this.subAgents.map(agent => agent.ID);
            const allLinkedIds = [...pendingSubAgentIds, ...existingSubAgentIds];
            this.agentManagementService.openSubAgentSelectorDialog({
                title: 'Add Sub-Agents',
                parentAgentId: this.record.ID,
                    if (result && result.selectedAgents && result.selectedAgents.length > 0) {
                        // Filter out already linked or pending agents
                        const newAgents = result.selectedAgents.filter(agent => 
                            !allLinkedIds.includes(agent.ID)
                        if (newAgents.length === 0) {
                                'All selected agents are already linked to this agent',
                        for (const agent of newAgents) {
                            const subAgentToUpdate = await md.GetEntityObject<AIAgentEntityExtended>('MJ: AI Agents');
                            await subAgentToUpdate.Load(agent.ID);
                            subAgentToUpdate.ParentID = this.record.ID;
                            // Database constraint requires ExposeAsAction = false for sub-agents
                            subAgentToUpdate.ExposeAsAction = false;
                                entityObject: subAgentToUpdate,
                        // Update UI to show the new sub-agents
                        this.subAgents.push(...newAgents);
                            `${newAgents.length} agent${newAgents.length === 1 ? '' : 's'} will be converted to sub-agent${newAgents.length === 1 ? '' : 's'} when you save`,
                        // User wants to create a new sub-agent
                        await this.createSubAgent();
                    console.error('Error opening sub-agent selector:', error);
                        'Error opening sub-agent selector. Please try again.',
            console.error('Error in addSubAgents:', error);
                'Error adding sub-agents. Please try again.',
     * Removes a sub-agent from this agent (deferred until save)
    public async removeSubAgent(subAgent: AIAgentEntityExtended, event: Event) {
            title: 'Remove Sub-Agent',
            content: `Are you sure you want to remove "${subAgent.Name}" as a sub-agent? This will make it an independent root agent.`,
                        p => p.entityObject.EntityInfo.Name === 'MJ: AI Agents' && 
                             p.entityObject.Get('ID') === subAgent.ID &&
                             p.entityObject.Get('ParentID') === this.record.ID
                        // Add to pending removals (will restore to root agent)
                        await subAgentToUpdate.Load(subAgent.ID);
                        subAgentToUpdate.ParentID = null; // Will become a root agent
                    const subAgentIndex = this.subAgents.findIndex(sa => sa.ID === subAgent.ID);
                    if (subAgentIndex >= 0) {
                        this.subAgents.splice(subAgentIndex, 1);
                        `"${subAgent.Name}" will be removed as a sub-agent when you save`,
                    console.error('Error removing sub-agent:', error);
                        'Failed to remove sub-agent',
     * Sets the sub-agent filter to show all, child, or related sub-agents
    public setSubAgentFilter(filter: SubAgentFilterType): void {
        this.subAgentFilter = filter;
     * Gets the badge color for a sub-agent based on its type
    public getSubAgentBadgeColor(item: UnifiedSubAgent): string {
        return item.type === 'child' ? '#2196F3' : '#9C27B0';
     * Gets the badge icon for a sub-agent based on its type
    public getSubAgentBadgeIcon(item: UnifiedSubAgent): string {
        return item.type === 'child' ? 'fa-solid fa-link' : 'fa-solid fa-share-nodes';
     * Gets the badge text for a sub-agent based on its type
    public getSubAgentBadgeText(item: UnifiedSubAgent): string {
        return item.type === 'child' ? 'CHILD' : 'RELATED';
     * Gets the payload information string for display
    public getSubAgentPayloadInfo(item: UnifiedSubAgent): string {
        if (item.type === 'child') {
            return 'Shared Payload';
        } else if (item.relationship?.SubAgentOutputMapping) {
                const mapping = JSON.parse(item.relationship.SubAgentOutputMapping);
                const entries = Object.entries(mapping);
                if (entries.length === 1 && entries[0][0] === '*') {
                    return `Mapped: * → ${entries[0][1]}`;
                return `Mapped: ${entries.length} path${entries.length === 1 ? '' : 's'}`;
                return 'Mapped Payload';
        return 'No Mapping';
     * Opens a dialog to configure the output mapping for a related sub-agent
    public async configureOutputMapping(item: UnifiedSubAgent, event: Event): Promise<void> {
        if (item.type !== 'related' || !item.relationship) return;
        // TODO: Implement JSON editor dialog for SubAgentOutputMapping
            'Output mapping configuration coming soon',
     * Unlinks a related sub-agent (removes the relationship)
    public async unlinkRelatedSubAgent(item: UnifiedSubAgent, event: Event): Promise<void> {
            title: 'Unlink Related Sub-Agent',
            content: `Are you sure you want to unlink "${item.agent.Name}"? This will remove the relationship but keep the agent itself.`,
                { text: 'Unlink', themeColor: 'error' }
            if (result && (result as any).text === 'Unlink') {
                    const success = await item.relationship.Delete();
                        // Remove from unified list
                        const index = this.allSubAgents.findIndex(s =>
                            s.type === 'related' && s.relationship?.ID === item.relationship!.ID
                            this.allSubAgents.splice(index, 1);
                            `Unlinked "${item.agent.Name}" successfully`,
                        throw new Error('Delete operation failed');
                    console.error('Error unlinking related sub-agent:', error);
                        'Failed to unlink sub-agent',
     * Removes a child sub-agent (updated to work with UnifiedSubAgent)
    public async removeChildSubAgent(item: UnifiedSubAgent, event: Event): Promise<void> {
        // Delegate to existing removeSubAgent method
        await this.removeSubAgent(item.agent, event);
     * Creates a new child sub-agent (renamed from createSubAgent for clarity)
    public async createChildSubAgent(): Promise<void> {
     * Opens dialog to link an existing agent as a related sub-agent
    public async linkRelatedSubAgent(): Promise<void> {
        // TODO: Implement dialog to select existing agents and create relationship
            'Link related sub-agent dialog coming soon',
     * Removes an action from the agent (deferred until save)
    public async removeAction(action: MJActionEntity, event: Event) {
            title: 'Remove Action',
            content: `Are you sure you want to remove the action "${action.Name}" from this agent?`,
                    p => p.entityObject.EntityInfo.Name === 'MJ: AI Agent Actions' && 
                         p.entityObject.Get('ActionID') === action.ID
                    // Find the existing AI Agent Action link record for deferred deletion
                    const linkResult = await rv.RunView<MJAIAgentActionEntity>({
                        ExtraFilter: `AgentID='${this.record.ID}' AND ActionID='${action.ID}'`,
                        const agentActionToDelete = linkResult.Results[0];
                            entityObject: agentActionToDelete,
                        throw new Error('AI Agent Action link not found');
                const actionIndex = this.agentActions.findIndex(a => a.ID === action.ID);
                if (actionIndex >= 0) {
                    this.agentActions.splice(actionIndex, 1);
                    `Action "${action.Name}" will be removed when you save the agent`,
                    console.error('Error removing action from agent:', error);
                        'Failed to remove action',
            console.error('Error with action dialog:', dialogError);
     * Opens the advanced settings dialog for a prompt
    public async openPromptAdvancedSettings(prompt: AIPromptEntityExtended, event: Event) {
            // Find the corresponding MJAIAgentPromptEntity for this prompt
            // Get all agent prompts for validation
            const allAgentPrompts = AIEngineBase.Instance.AgentPrompts.filter(ap => ap.AgentID === this.record.ID);
            const agentPrompt = allAgentPrompts.find(ap => ap.PromptID === prompt.ID);
                    'Unable to find prompt configuration for advanced settings',
            this.agentManagementService.openAgentPromptAdvancedSettingsDialog({
                agentPrompt: agentPrompt,
                allAgentPrompts: allAgentPrompts,
                next: async (formData) => {
                    if (formData) {
                            // Update the agent prompt entity with new values
                            agentPrompt.ExecutionOrder = formData.executionOrder;
                            agentPrompt.Purpose = formData.purpose;
                            agentPrompt.ConfigurationID = formData.configurationID;
                            agentPrompt.ContextBehavior = formData.contextBehavior;
                            agentPrompt.ContextMessageCount = formData.contextMessageCount;
                            agentPrompt.Status = formData.status;
                            // Save immediately to database
                            const saveResult = await agentPrompt.Save();
                                    'Prompt settings updated successfully',
                                // Refresh the related data to reflect changes
                                await this.loadRelatedCounts(true);
                                    'Failed to save prompt settings. Please try again.',
                            console.error('Error saving prompt advanced settings:', error);
                                'Error saving prompt settings. Please try again.',
                    console.error('Error opening prompt advanced settings dialog:', error);
                        'Error opening advanced settings. Please try again.',
            console.error('Error in openPromptAdvancedSettings:', error);
                'Error opening prompt advanced settings. Please try again.',
     * Opens the advanced settings dialog for a sub-agent
    public async openSubAgentAdvancedSettings(subAgentEntity: AIAgentEntityExtended, event: Event) {
            // Get all sub-agents under the same parent for validation
            const allSubAgents = AIEngineBase.Instance.Agents.filter(sa => sa.ParentID === subAgentEntity.ParentID);
            this.agentManagementService.openSubAgentAdvancedSettingsDialog({
                subAgent: subAgentEntity,
                allSubAgents: allSubAgents,
                            // Update the sub-agent entity with new values
                            subAgentEntity.ExecutionOrder = formData.executionOrder;
                            subAgentEntity.ExecutionMode = formData.executionMode;
                            subAgentEntity.Status = formData.status;
                            subAgentEntity.TypeID = formData.typeID;
                            subAgentEntity.ExposeAsAction = formData.exposeAsAction;
                            const saveResult = await subAgentEntity.Save();
                                    'Sub-agent settings updated successfully',
                                // Update the local sub-agent data to reflect changes
                                const localSubAgent = this.subAgents.find(sa => sa.ID === subAgentEntity.ID);
                                if (localSubAgent) {
                                    localSubAgent.ExecutionOrder = formData.executionOrder;
                                    localSubAgent.ExecutionMode = formData.executionMode;
                                    localSubAgent.Status = formData.status;
                                    localSubAgent.TypeID = formData.typeID;
                                    localSubAgent.ExposeAsAction = formData.exposeAsAction;
                                    'Failed to save sub-agent settings. Please try again.',
                            console.error('Error saving sub-agent advanced settings:', error);
                                'Error saving sub-agent settings. Please try again.',
                    console.error('Error opening sub-agent advanced settings dialog:', error);
            console.error('Error in openSubAgentAdvancedSettings:', error);
                'Error opening sub-agent advanced settings. Please try again.',
     * Override PopulatePendingRecords to preserve our pending records before parent clears them
     * However, during cancel operations, we want to clear all pending records completely
    protected PopulatePendingRecords() {
        // If we're in the middle of a cancel operation, don't preserve pending records
        // The base class CancelEdit will handle reverting pending records appropriately
        if (this.isPerformingCancel) {
            super.PopulatePendingRecords();
        // IMPORTANT: The parent method clears the pending records array, so we need to preserve
        // any records we've added before calling the parent method during normal operations
        const currentPendingRecords = [...this.PendingRecords]; // Make a copy
        // Call parent first to get child component pending records (this clears the array)
        // Re-add our preserved records (only during normal save operations)
        for (const record of currentPendingRecords) {
            this.PendingRecords.push(record);
    /** Flag to track if we're currently performing a cancel operation */
    private isPerformingCancel = false;
     * Override InternalSaveRecord to handle agent-specific transaction logic
     * AI Agent must be saved first since related entities depend on it
     * The base SaveRecord() method will call this InternalSaveRecord() method
     * after handling validation and pending record population
    protected async InternalSaveRecord(): Promise<boolean> {
        if (!this.record) {
            // Reset context compression fields if EnableContextCompression is false
            if (!this.record.EnableContextCompression) {
            const transactionGroup = await md.CreateTransactionGroup();
            // Set transaction group on main record first
            this.record.TransactionGroup = transactionGroup;
            // Save entities in dependency order to avoid foreign key constraint errors
            // We need to save Templates and Template Contents BEFORE the main AI Agent
            // since AI Prompts depend on Templates, and AI Agent Prompts depend on AI Agents
            // 1. First save Templates (they have no dependencies)
            const templateRecords = this.PendingRecords.filter(p => 
                p.entityObject.EntityInfo.Name === 'MJ: Templates'
            for (const templateRecord of templateRecords) {
                templateRecord.entityObject.TransactionGroup = transactionGroup;
                if (templateRecord.action === 'save') {
                    const saveResult = await templateRecord.entityObject.Save();
                            `Failed to save Template "${templateRecord.entityObject.Get('Name')}". Please check the data and try again.`,
                    await templateRecord.entityObject.Delete();
            // 2. Save Template Contents (depend on Templates)
            const templateContentRecords = this.PendingRecords.filter(p => 
                p.entityObject.EntityInfo.Name === 'MJ: Template Contents'
            for (const contentRecord of templateContentRecords) {
                contentRecord.entityObject.TransactionGroup = transactionGroup;
                if (contentRecord.action === 'save') {
                    const saveResult = await contentRecord.entityObject.Save();
                            `Failed to save Template Content. Please check the data and try again.`,
                    await contentRecord.entityObject.Delete();
            // 3. Save AI Prompts (depend on Templates)
            const promptRecords = this.PendingRecords.filter(p => 
                p.entityObject.EntityInfo.Name === 'MJ: AI Prompts'
            for (const promptRecord of promptRecords) {
                promptRecord.entityObject.TransactionGroup = transactionGroup;
                if (promptRecord.action === 'save') {
                    const saveResult = await promptRecord.entityObject.Save();
                            `Failed to save AI Prompt "${promptRecord.entityObject.Get('Name')}". Please check the data and try again.`,
                    await promptRecord.entityObject.Delete();
            // 4. Save the main AI Agent record (other entity links depend on it)
            // The record transaction group was already set above
            const agentSaveResult = await this.record.Save();
            if (!agentSaveResult) {
                    `Failed to save AI agent "${this.record.Name}". Please check the data and try again.`,
            // 4.1. Handle deferred sub-agent creation - set ParentID on any sub-agents created before parent was saved
            const subAgentRecords = this.PendingRecords.filter(p => 
                p.entityObject.EntityInfo.Name === 'MJ: AI Agents' && 
                p.entityObject.Get('_tempParentId') === this.record.ID
            for (const subAgentRecord of subAgentRecords) {
                // Cast to AIAgentEntityExtended to access ParentID property
                const subAgent = subAgentRecord.entityObject as AIAgentEntityExtended;
                // Set the proper ParentID now that parent is saved
                subAgent.ParentID = this.record.ID;
                // Clear the temporary reference
                subAgent.Set('_tempParentId', null);
            // 5. Save all other pending records (AI Agent Actions, AI Agent Prompts, etc.)
            const otherRecords = this.PendingRecords.filter(p => 
                p.entityObject.EntityInfo.Name !== 'MJ: Templates' &&
                p.entityObject.EntityInfo.Name !== 'MJ: Template Contents' &&
                p.entityObject.EntityInfo.Name !== 'MJ: AI Prompts'
            for (const record of otherRecords) {
                record.entityObject.TransactionGroup = transactionGroup;
                if (record.action === 'save') {
                    const saveResult = await record.entityObject.Save();
                            `Failed to save ${record.entityObject.EntityInfo.Name}. Transaction will be rolled back.`,
                    await record.entityObject.Delete();
            // Execute all operations atomically
            const success = await transactionGroup.Submit();
                // Clear our local state since save was successful
                // Clear pending records since they've been saved
                // Reload related data to reflect database state
                    'AI Agent and all related changes saved successfully',
                    'Save failed. Please try again.',
                `Save failed: ${error instanceof Error ? error.message : 'Unknown error'}. Please try again.`,
                5000
     * Navigates to the parent agent when the "Child of..." badge is clicked
    public navigateToParentAgent(): void {
        if (this.record.ParentID) {
            this.navigateToEntity('MJ: AI Agents', this.record.ParentID);
     * Component cleanup - critical for preventing memory leaks
        // Signal all subscriptions to complete
        // Clear all active timeouts
        this.activeTimeouts.forEach(timeoutId => {
        this.activeTimeouts.length = 0;
        // Clear running time updater
        if (this._runningTimeUpdater) {
            clearInterval(this._runningTimeUpdater);
            this._runningTimeUpdater = null;
        // Clear all data arrays to release memory
        this.subAgents.length = 0;
        this.agentPrompts.length = 0;
        this.agentActions.length = 0;
        this.recentExecutions.length = 0;
        this.learningCycles.length = 0;
        this.agentNotes.length = 0;
        // Reset pagination state
        this.totalExecutionHistoryCount = 0;
        // Clear maps and objects
        this._runningTimeCache.clear();
        this.expandedExecutions = {};
        this.originalSnapshots = null as any;
        // Clean up component references
        if (this.customSectionComponentRef) {
            this.customSectionComponentRef.destroy();
            this.customSectionComponentRef = null;
        this.customSectionComponent = null;
