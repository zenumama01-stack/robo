import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, TemplateRef, ChangeDetectorRef, inject, DoCheck } from '@angular/core';
import { DiscoverISADescendants, BuildDescendantTree, IsaRelatedItem } from '../isa-related-panel/isa-hierarchy-utils';
import { FormWidthMode, FormContext } from '../types/form-types';
  CustomToolbarButtonClickEventArgs,
  CustomToolbarButton
 * Configurable form toolbar component.
 * Renders action buttons (edit, save, delete, favorite, history, lists),
 * the IS-A entity hierarchy breadcrumb, and section controls (search, expand/collapse).
 * All navigation actions are emitted as events - the toolbar never calls any routing
 * service directly. The host application subscribes and maps to its own navigation.
 * <mj-form-toolbar
 *   [record]="record"
 *   [editMode]="editMode"
 *   [config]="toolbarConfig"
 *   (EditModeChange)="editMode = $event"
 *   (SaveRequested)="onSave()"
 *   (DeleteRequested)="onDelete()">
 * </mj-form-toolbar>
  templateUrl: './form-toolbar.component.html',
  styleUrls: ['./form-toolbar.component.css']
export class MjFormToolbarComponent implements DoCheck {
  // ---- Deprecated form reference (backward compat) ----
  private _formRef: unknown;
   * @deprecated Use `<mj-record-form-container>` or pass individual inputs instead.
   * Accepts a form component reference for backward compatibility.
   * When set, the toolbar reads Record, EditMode, etc. from this reference.
  @Input() set Form(value: unknown) { this._formRef = value; }
  get Form(): unknown { return this._formRef; }
  /** @deprecated Use [Form] or individual inputs instead */
  // ---- Inputs ----
  /** Whether the current user has edit permission */
  /** Whether the current user has delete permission */
  /** Whether the record is currently a favorite */
  /** Whether favorite init has completed (prevents flash of wrong icon) */
  /** Whether the record has unsaved changes */
  /** Names of dirty fields for display in save bar */
  /** Count of lists this record belongs to */
  /** Entity info for IS-A hierarchy and metadata */
  /** Toolbar configuration controlling visibility and behavior */
  @Input() Config: FormToolbarConfig = DEFAULT_TOOLBAR_CONFIG;
  /** Whether to show the toolbar in a saving/loading state */
  // Section controls inputs
  @Input() VisibleSectionCount = 0;
  @Input() TotalSectionCount = 0;
  @Input() ExpandedSectionCount = 0;
  @Input() SearchFilter = '';
  @Input() ShowEmptyFields = false;
  @Input() HasCustomSectionOrder = false;
  /** Optional template for additional toolbar actions */
  @Input() AdditionalActionsTemplate: TemplateRef<unknown> | null = null;
  /** Emitted for all navigation actions (record links, hierarchy clicks, etc.) */
  /** Request to enter or exit edit mode */
  /** Request to save the current record */
  /** Request to cancel editing and revert changes */
  /** Request to delete the current record */
  /** Request to toggle favorite status */
  /** Request to show record change history */
  /** Request to show list management */
  /** Request to show dirty field changes */
  // Section control outputs
  @Output() FilterChange = new EventEmitter<string>();
  @Output() ExpandAllRequested = new EventEmitter<void>();
  @Output() CollapseAllRequested = new EventEmitter<void>();
  @Output() ShowEmptyFieldsChange = new EventEmitter<boolean>();
  @Output() WidthModeChange = new EventEmitter<FormWidthMode>();
  @Output() ResetSectionOrderRequested = new EventEmitter<void>();
  @Output() ManageSectionsRequested = new EventEmitter<void>();
  // ---- Internal state ----
  ShowDeleteDialog = false;
  ShowDiscardDialog = false;
  /** Computed descendant tree for breadcrumb display */
  DescendantTree: IsaRelatedItem[] = [];
  private _lastRecordForChains: BaseEntity | null = null;
  private _chainsLoading = false;
  ngDoCheck(): void {
    if (this._formRef) {
      this.SyncFromFormRef();
    this.CheckDescendantChains();
   * Sync toolbar state from the legacy form reference.
   * Only active when [Form] is set (backward-compat mode).
  private SyncFromFormRef(): void {
    const ref = this._formRef as Record<string, unknown>;
    const rec = ref['record'] as BaseEntity | undefined;
    let changed = false;
    if (rec && rec !== this.Record) {
      this.Record = rec;
      this.EntityInfo = rec.EntityInfo;
    if (rec) {
      const dirty = rec.Dirty;
      if (dirty !== this.IsDirty) {
        this.IsDirty = dirty;
        const dirtyNames = rec.Fields.filter(f => f.Dirty).map(f => f.Name);
        if (dirtyNames.length !== this.DirtyFieldNames.length) {
          this.DirtyFieldNames = dirtyNames;
    const editMode = !!ref['EditMode'];
    if (editMode !== this.EditMode) { this.EditMode = editMode; changed = true; }
    const canEdit = !!ref['UserCanEdit'];
    if (canEdit !== this.UserCanEdit) { this.UserCanEdit = canEdit; changed = true; }
    const canDelete = !!ref['UserCanDelete'];
    if (canDelete !== this.UserCanDelete) { this.UserCanDelete = canDelete; changed = true; }
    const isFavorite = !!ref['IsFavorite'];
    if (isFavorite !== this.IsFavorite) { this.IsFavorite = isFavorite; changed = true; }
    const favDone = !!ref['FavoriteInitDone'];
    if (favDone !== this.FavoriteInitDone) { this.FavoriteInitDone = favDone; changed = true; }
  /** Whether entity tracks record changes (for history button) */
  get TracksChanges(): boolean {
    return this.EntityInfo?.TrackRecordChanges === true;
  /** IS-A parent chain for breadcrumb display */
  get ParentChain(): EntityInfo[] {
    if (!this.EntityInfo) return [];
    return this.EntityInfo.ParentChain.slice().reverse();
  /** Whether this entity has parent entities (IS-A child) */
  get HasParentEntities(): boolean {
    return this.ParentChain.length > 0;
  /** Child entity types from metadata (all possible subtypes) */
  get ChildEntities(): EntityInfo[] {
    return this.EntityInfo?.ChildEntities ?? [];
  /** Whether this entity has child entity types (IS-A parent) */
  get HasChildEntities(): boolean {
    return this.ChildEntities.length > 0;
   * The actual loaded IS-A child chain for the current record.
   * Walks Record.ISAChild → ISAChild.ISAChild → ... collecting each child entity.
   * This differs from ChildEntities (metadata) because it represents the SPECIFIC
   * child type that exists for THIS record, not all possible subtypes.
   * For overlapping subtype parents, this is empty (ISAChild returns null) —
   * use OverlappingChildren instead.
  get ChildChain(): BaseEntity[] {
    if (!this.Record) return [];
    const chain: BaseEntity[] = [];
    let current = this.Record.ISAChild;
    while (current) {
      chain.push(current);
      current = current.ISAChild;
  /** Whether this record has a loaded IS-A child entity */
  get HasLoadedChild(): boolean {
    return this.Record?.ISAChild != null;
   * For overlapping subtype parents (AllowMultipleSubtypes = true), returns the
   * list of child entity type names that have records for this PK.
   * Populated by InitializeChildEntity() during record load.
  get OverlappingChildren(): { entityName: string }[] {
    return this.Record?.ISAChildren ?? [];
  /** Whether the current record has overlapping child types discovered */
  get HasOverlappingChildren(): boolean {
    return this.OverlappingChildren.length > 0;
  /** Whether this entity is part of any IS-A hierarchy (parent or child side) */
  get IsInHierarchy(): boolean {
    return this.HasParentEntities || this.HasLoadedChild || this.HasOverlappingChildren || this.DescendantTree.length > 0;
  /** Display-friendly names of dirty fields for the edit banner */
  get DirtyFieldDisplayNames(): string[] {
    if (!this.Record?.EntityInfo || this.DirtyFieldNames.length === 0) return [];
    return this.DirtyFieldNames.map(name => {
      const field = this.Record.EntityInfo.Fields.find(f => f.Name === name);
      return field?.DisplayNameOrName ?? name;
  /** Display name for the edit banner */
  get RecordDisplayName(): string {
    const info = this.Record.EntityInfo;
    if (info?.NameField) {
      const name = this.Record.Get(info.NameField.Name);
      if (name) return String(name);
    return this.Record.PrimaryKey.ToConcatenatedString();
  // ---- Descendant Chain Computation ----
   * Check if descendant chains need recomputation (called from DoCheck).
   * Only triggers async computation when the record identity changes.
  private CheckDescendantChains(): void {
    if (!this.Record) {
      if (this.DescendantTree.length > 0) {
        this.DescendantTree = [];
        this._lastRecordForChains = null;
    if (this.Record !== this._lastRecordForChains && !this._chainsLoading) {
      this.ComputeDescendantChains();
   * Asynchronously discover all IS-A descendants and convert to chains
   * for breadcrumb display. Each chain is a root-to-leaf path of entity names.
  private ComputeDescendantChains(): void {
    this._lastRecordForChains = this.Record;
    if (!this.Record?.EntityInfo?.IsParentType) {
    this._chainsLoading = true;
    DiscoverISADescendants(this.Record).then(descendants => {
      this.DescendantTree = BuildDescendantTree(descendants);
      this._chainsLoading = false;
    }).catch(() => {
   * Navigate to a descendant entity record in the IS-A hierarchy.
  OnDescendantBadgeClick(entityName: string, event: MouseEvent): void {
      PrimaryKey: this.Record.PrimaryKey,
      Direction: 'child'
  OnEdit(): void {
    if (this.DispatchToFormRef('StartEditMode')) return;
    this.EditModeChange.emit(true);
      // Emit Before event - handler can cancel by setting event.Cancel = true
      const beforeEvent = new BeforeSaveEventArgs(true);
      this.BeforeSave.emit(beforeEvent);
      if (this.DispatchToFormRef('SaveRecord', true)) return;
    // If there are unsaved changes, show confirmation dialog
    if (this.IsDirty) {
      this.ShowDiscardDialog = true;
    // No changes - cancel immediately
    this.EmitCancel();
  OnDiscardConfirm(): void {
    this.ShowDiscardDialog = false;
  OnDiscardCancel(): void {
  private EmitCancel(): void {
    const beforeEvent = new BeforeCancelEventArgs();
    this.BeforeCancel.emit(beforeEvent);
    if (this.DispatchToFormRef('CancelEdit')) return;
  OnDeleteClick(): void {
    this.ShowDeleteDialog = true;
  OnDeleteConfirm(): void {
    this.ShowDeleteDialog = false;
    const beforeEvent = new BeforeDeleteEventArgs();
    this.BeforeDelete.emit(beforeEvent);
    if (beforeEvent.Cancel) {
    if (this.DispatchToFormRef('OnDeleteRequested')) {
  OnDeleteCancel(): void {
  OnFavoriteToggle(): void {
    if (this.DispatchToFormRef('OnFavoriteToggled')) return;
  OnHistory(): void {
    if (this.DispatchToFormRef('OnHistoryRequested')) return;
  OnListManagement(): void {
    if (this.DispatchToFormRef('OnListManagementRequested')) return;
  OnCustomButtonClick(button: CustomToolbarButton): void {
    if (button.Disabled) return;
    this.CustomButtonClick.emit({
      ButtonKey: button.Key,
      Button: button
  OnShowChanges(): void {
    if (this.DispatchToFormRef('ShowChanges')) return;
   * Try to call a method on the legacy form reference.
   * Returns true if the method was found and called, false otherwise.
  private DispatchToFormRef(methodName: string, ...args: unknown[]): boolean {
    if (!this._formRef) return false;
    const method = ref[methodName];
    if (typeof method === 'function') {
      (method as (...a: unknown[]) => unknown).call(this._formRef, ...args);
   * Navigate to a parent entity record in the IS-A hierarchy.
  OnParentBadgeClick(parentEntity: EntityInfo, event: MouseEvent): void {
      EntityName: parentEntity.Name,
   * Navigate to a child entity type list view.
  OnChildEntityClick(childEntity: EntityInfo): void {
      Kind: 'child-entity-type',
      ChildEntityName: childEntity.Name,
      ParentEntityName: this.EntityInfo!.Name,
      ParentRecordId: this.Record.PrimaryKey.ToConcatenatedString()
   * Navigate to a loaded child entity record in the IS-A hierarchy.
   * The child shares the same primary key as the parent (IS-A inheritance).
  OnChildBadgeClick(childEntity: BaseEntity, event: MouseEvent): void {
      EntityName: childEntity.EntityInfo.Name,
   * Navigate to an overlapping child entity record.
   * Used when the parent has AllowMultipleSubtypes = true and multiple child
   * types coexist for the same PK.
  OnOverlappingChildClick(childEntityName: string, event: MouseEvent): void {
      EntityName: childEntityName,
  // ---- Section Controls ----
  OnFilterInput(event: Event): void {
    this.FilterChange.emit(input.value);
  OnClearFilter(): void {
    this.FilterChange.emit('');
    this.ExpandAllRequested.emit();
    this.CollapseAllRequested.emit();
  OnToggleEmptyFields(): void {
    this.ShowEmptyFieldsChange.emit(!this.ShowEmptyFields);
  OnToggleWidthMode(): void {
    const newMode: FormWidthMode = this.WidthMode === 'centered' ? 'full-width' : 'centered';
    this.WidthModeChange.emit(newMode);
    this.ResetSectionOrderRequested.emit();
    this.ManageSectionsRequested.emit();
