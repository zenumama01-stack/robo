import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { EntityInfo, EntityFieldInfo, Metadata } from '@memberjunction/core';
import { MJEntityERDComponent } from '../mj-entity-erd.component';
import { EntitySelectedEvent, OpenEntityRecordEvent } from '../mj-entity-erd.component';
import { ERDConfig, ERDState } from '../../interfaces/erd-types';
import { EntityFilter } from '../entity-filter-panel/entity-filter-panel.component';
 * State object for the ERD composite component.
 * Used for state persistence and restoration.
export interface ERDCompositeState {
  filterPanelWidth: number;
  filters: EntityFilter;
  selectedEntityId: string | null;
  zoomLevel: number;
  panPosition: { x: number; y: number };
  fieldsSectionExpanded: boolean;
  relationshipsSectionExpanded: boolean;
 * ERD Composite component that combines the ERD diagram with filter and details panels.
 * This is a complete, ready-to-use ERD exploration interface that includes:
 * - Left panel: Entity filter controls (schema, name, status, etc.)
 * The component handles all internal state but emits events for:
 * - State changes (for parent to persist user preferences)
 * - Open record requests (for parent to handle navigation)
 * ## Usage
 *   (stateChange)="onStateChange($event)"
 * ## State Persistence
 * The component emits `userStateChange` events (debounced 1s) when user changes
 * any settings. The parent should save this state and restore it using `loadUserState()`:
 * @ViewChild(ERDCompositeComponent) erdComposite!: ERDCompositeComponent;
 * ngAfterViewInit() {
 *   const savedState = await this.loadSavedState();
 *   if (savedState) {
 *     this.erdComposite.loadUserState(savedState);
 * onUserStateChange(state: ERDCompositeState) {
 *   await this.saveState(state);
  selector: 'mj-erd-composite',
  templateUrl: './erd-composite.component.html',
  styleUrls: ['./erd-composite.component.css']
export class ERDCompositeComponent implements OnInit, OnDestroy {
  @ViewChild(MJEntityERDComponent) mjEntityErd!: MJEntityERDComponent;
  /** Whether the ERD is in a refreshing state */
  @Input() isRefreshingERD = false;
   * Optional: Focus entities to display in the ERD.
   * When provided, only these entities will be shown (useful for single-entity views).
   * When not provided, all entities from metadata are loaded.
  @Input() focusEntities: EntityInfo[] | null = null;
   * Whether to show the filter panel on the left.
   * Set to false for focused/single-entity views.
  @Input() showFilterPanel = true;
   * Depth of relationships to display.
   * 1 = only direct relationships, 2 = relationships of relationships, etc.
   * Whether to show the ERD header bar.
  /** All entities loaded from metadata */
  /** All entity fields (flattened from all entities) */
  /** Emitted on any state change (debounced 50ms) */
  @Output() stateChange = new EventEmitter<ERDCompositeState>();
  /** Emitted on user-initiated state changes (debounced 1s) for persistence */
  @Output() userStateChange = new EventEmitter<ERDCompositeState>();
  /** Emitted when an entity is opened (e.g., double-clicked in details panel) */
  @Output() entityOpened = new EventEmitter<EntityInfo>();
  /** Emitted when requesting to open an entity record (for navigation) */
  @Output() openRecord = new EventEmitter<{EntityName: string, RecordID: string}>();
  // Panel visibility and configuration
  public fieldsSectionExpanded = true;
  public relationshipsSectionExpanded = true;
  /** ERD configuration - skip animation for faster rendering */
  public erdConfig: ERDConfig = { skipAnimation: true };
  // Entity state
  public isDataLoaded = false;
  public filters: EntityFilter = {
  private stateChangeSubject = new Subject<ERDCompositeState>();
  private filterChangeSubject = new Subject<void>();
    // Initialize filter panel visibility based on input
    this.filterPanelVisible = this.showFilterPanel;
    // Use focusEntities if provided, otherwise use all entities
    if (this.focusEntities && this.focusEntities.length > 0) {
      this.filteredEntities = [...this.focusEntities];
    this.isDataLoaded = true;
    // Notify parent that data is loaded and ready for state loading
    this.filterChangeSubject.complete();
    // Load entities from metadata (always needed for allEntityFields and relationship lookups)
    this.entities = md.Entities;
    // Load all entity fields from entities
    this.allEntityFields = this.entities
      .map((entity) => {
        return entity.Fields;
      .flat();
    // State change emissions with debouncing
      this.userStateChange.emit(state);
    // Filter changes with debouncing
    this.filterChangeSubject.pipe(
      debounceTime(300)
      this.emitUserStateChange();
    this.filterChangeSubject.next();
  public onFiltersChange(newFilters: EntityFilter): void {
    this.filters = { ...newFilters };
  public onToggleFilterPanel(): void {
    // Trigger ERD resize when filter panel is toggled
    if (this.mjEntityErd) {
      this.mjEntityErd.triggerResize();
  public onEntityDeselected(): void {
   * Handle entity selection from the mj-entity-erd wrapper.
   * Updates internal state.
  public onERDEntitySelected(event: EntitySelectedEvent): void {
    this.selectedEntity = event.entity;
   * Handle open record from the mj-entity-erd wrapper (double-click).
   * Opens the entity record using the entity form.
  public onERDOpenRecord(event: OpenEntityRecordEvent): void {
    this.openRecord.emit({ EntityName: event.EntityName, RecordID: event.RecordID });
   * Handle state changes from the ERD component.
  public onERDStateChange(_state: ERDState): void {
    // State changes are tracked for user preference persistence
    this.entityOpened.emit(entity);
  public onFieldsSectionToggle(): void {
    this.fieldsSectionExpanded = !this.fieldsSectionExpanded;
  public onRelationshipsSectionToggle(): void {
    this.relationshipsSectionExpanded = !this.relationshipsSectionExpanded;
    this.openRecord.emit(event);
  public onSplitterLayoutChange(_event: unknown): void {
    // Trigger ERD diagram resize when splitter layout changes
      // Schema filter
      if (this.filters.schemaName && entity.SchemaName !== this.filters.schemaName) {
      // Entity name filter
      if (this.filters.entityName) {
        const searchTerm = this.filters.entityName.toLowerCase();
        const entityName = (entity.Name || entity.SchemaName || '').toLowerCase();
        if (!entityName.includes(searchTerm)) {
      // Base table filter
      if (this.filters.baseTable) {
        const searchTerm = this.filters.baseTable.toLowerCase();
        const baseTable = (entity.BaseTable || '').toLowerCase();
        if (!baseTable.includes(searchTerm)) {
      if (this.filters.entityStatus && entity.Status !== this.filters.entityStatus) {
    const state = this.buildState();
  private emitUserStateChange(): void {
  private buildState(): ERDCompositeState {
      filterPanelWidth: 320,
      filters: { ...this.filters },
      selectedEntityId: this.selectedEntity?.ID || null,
      zoomLevel: 1,
      panPosition: { x: 0, y: 0 },
      fieldsSectionExpanded: this.fieldsSectionExpanded,
      relationshipsSectionExpanded: this.relationshipsSectionExpanded,
   * Load user state from a previously saved state object.
   * Call this after the component is initialized to restore user preferences.
  public loadUserState(state: Partial<ERDCompositeState>): void {
    if (state.filters) {
      this.filters = { ...state.filters };
    if (state.fieldsSectionExpanded !== undefined) {
      this.fieldsSectionExpanded = state.fieldsSectionExpanded;
    if (state.relationshipsSectionExpanded !== undefined) {
      this.relationshipsSectionExpanded = state.relationshipsSectionExpanded;
    if (state.selectedEntityId && this.entities.length > 0) {
      const entity = this.entities.find(e => e.ID === state.selectedEntityId);
   * Refresh the ERD diagram.
  public refreshERD(): void {
      this.mjEntityErd.refresh();
   * Get the current state for external use.
  public getState(): ERDCompositeState {
    return this.buildState();
