  Component, Input, Output, EventEmitter,
  ChangeDetectionStrategy, ChangeDetectorRef, inject, NgZone,
  ContentChildren, QueryList, AfterContentInit, OnDestroy,
  ViewEncapsulation
import { BaseEntity, EntityInfo } from '@memberjunction/core';
import { FormToolbarConfig, DEFAULT_TOOLBAR_CONFIG } from '../types/toolbar-config';
import { FormNavigationEvent } from '../types/navigation-events';
import { FormWidthMode } from '../types/form-types';
import { MjCollapsiblePanelComponent } from '../panel/collapsible-panel.component';
import { SectionManagerItem } from '../section-manager/section-manager.component';
  BeforeSaveEventArgs,
  BeforeDeleteEventArgs,
  BeforeCancelEventArgs,
  BeforeHistoryViewEventArgs,
  BeforeListManagementEventArgs,
  CustomToolbarButtonClickEventArgs
} from '../types/form-events';
import { BaseFormComponent } from '../base-form-component';
 * Top-level container that composes the toolbar, content slots, and sticky behavior.
 * **Two usage modes:**
 * 1. **With FormComponent** (generated forms): Pass `[FormComponent]="this"` and the
 *    container derives all state from the BaseFormComponent instance. Save/Cancel/Edit
 *    are handled internally by calling FormComponent methods.
 * 2. **Standalone**: Pass individual @Input properties and handle all @Output events.
 * @example Generated form usage:
 * <mj-record-form-container [Record]="record" [FormComponent]="this"
 *   (Navigate)="OnFormNavigate($event)"
 *   (DeleteRequested)="OnDeleteRequested()"
 *   (FavoriteToggled)="OnFavoriteToggled()"
 *   (HistoryRequested)="OnHistoryRequested()"
 *   (ListManagementRequested)="OnListManagementRequested()">
 *   <mj-collapsible-panel SectionKey="details" ...>
 *     <mj-form-field ...></mj-form-field>
 *   </mj-collapsible-panel>
 *   <mj-collapsible-panel SectionKey="relatedOrders" Variant="related-entity" ...>
 *     <!-- related entity grid -->
 * </mj-record-form-container>
  selector: 'mj-record-form-container',
  templateUrl: './record-form-container.component.html',
  styleUrls: ['./record-form-container.component.css']
export class MjRecordFormContainerComponent implements AfterContentInit, OnDestroy {
  private panelNavReset$ = new Subject<void>();
  // ---- Internal State ----
  /** Controls visibility of record changes drawer */
  ShowRecordChanges = false;
  /** Controls visibility of list management dialog */
  ShowListManagement = false;
  /** Controls visibility of section manager drawer */
  ShowSectionManager = false;
  // ---- Primary Inputs ----
  /** The entity record being displayed/edited */
  @Input() Record!: BaseEntity;
   * Reference to the parent form component (e.g. BaseFormComponent subclass).
   * When provided, the container derives toolbar state from this reference and
   * handles Save/Cancel/Edit internally by calling its methods.
  @Input() FormComponent: BaseFormComponent | null = null;
  // ---- Fallback Inputs (used when FormComponent is NOT provided) ----
  @Input() EntityInfo: EntityInfo | null = null;
  @Input() EditMode = false;
  @Input() UserCanEdit = false;
  @Input() UserCanDelete = false;
  @Input() IsFavorite = false;
  @Input() FavoriteInitDone = false;
  @Input() IsDirty = false;
  @Input() DirtyFieldNames: string[] = [];
  @Input() ListCount = 0;
  @Input() IsSaving = false;
  @Input() ToolbarConfig: FormToolbarConfig = DEFAULT_TOOLBAR_CONFIG;
  @Input() WidthMode: FormWidthMode = 'centered';
  // ---- Outputs ----
  /** Emitted for all navigation actions (the host app maps these to its routing) */
  /** Emitted when edit mode changes (only in standalone mode; FormComponent mode handled internally) */
  @Output() EditModeChange = new EventEmitter<boolean>();
  /** Emitted BEFORE save - can be cancelled by setting event.Cancel = true */
  @Output() BeforeSave = new EventEmitter<BeforeSaveEventArgs>();
  /** Emitted when save is requested (only in standalone mode) */
  @Output() SaveRequested = new EventEmitter<void>();
  /** Emitted BEFORE cancel - can be cancelled by setting event.Cancel = true */
  @Output() BeforeCancel = new EventEmitter<BeforeCancelEventArgs>();
  /** Emitted when cancel is requested (only in standalone mode) */
  @Output() CancelRequested = new EventEmitter<void>();
  /** Emitted BEFORE delete - can be cancelled by setting event.Cancel = true */
  @Output() BeforeDelete = new EventEmitter<BeforeDeleteEventArgs>();
  /** Emitted when delete is confirmed (host app handles actual deletion) */
  @Output() DeleteRequested = new EventEmitter<void>();
  /** Emitted when favorite toggle is requested */
  @Output() FavoriteToggled = new EventEmitter<void>();
  /** Emitted BEFORE history view - can be cancelled by setting event.Cancel = true */
  @Output() BeforeHistoryView = new EventEmitter<BeforeHistoryViewEventArgs>();
  /** Emitted when history view is requested */
  @Output() HistoryRequested = new EventEmitter<void>();
  /** Emitted BEFORE list management - can be cancelled by setting event.Cancel = true */
  @Output() BeforeListManagement = new EventEmitter<BeforeListManagementEventArgs>();
  /** Emitted when list management is requested */
  @Output() ListManagementRequested = new EventEmitter<void>();
  /** Emitted when show-changes is requested */
  @Output() ShowChangesRequested = new EventEmitter<void>();
  /** Emitted when a custom toolbar button is clicked */
  @Output() CustomButtonClick = new EventEmitter<CustomToolbarButtonClickEventArgs>();
  // ---- Content Children ----
  @ContentChildren(MjCollapsiblePanelComponent, { descendants: true })
  Panels!: QueryList<MjCollapsiblePanelComponent>;
  // ---- FormComponent accessor ----
  /** Typed accessor for the form component reference */
  private get fc(): BaseFormComponent | null {
    return this.FormComponent;
  // ---- Effective state (bridges FormComponent → toolbar inputs) ----
  get EffectiveRecord(): BaseEntity {
    return this.fc?.record ?? this.Record;
  get EffectiveEditMode(): boolean {
    return this.fc?.EditMode ?? this.EditMode;
  get EffectiveUserCanEdit(): boolean {
    return this.fc?.UserCanEdit ?? this.UserCanEdit;
  get EffectiveUserCanDelete(): boolean {
    return this.fc?.UserCanDelete ?? this.UserCanDelete;
  get EffectiveIsFavorite(): boolean {
    return this.fc?.IsFavorite ?? this.IsFavorite;
  get EffectiveFavoriteInitDone(): boolean {
    return this.fc?.FavoriteInitDone ?? this.FavoriteInitDone;
  get EffectiveEntityInfo(): EntityInfo | null {
    return (this.fc?.EntityInfo as EntityInfo) ?? this.EntityInfo;
  get EffectiveIsDirty(): boolean {
    if (this.fc) {
      return this.fc.record?.Dirty ?? false;
    return this.IsDirty;
  get EffectiveDirtyFieldNames(): string[] {
    if (this.fc?.record?.Fields) {
      return this.fc.record.Fields.filter(f => f.Dirty).map(f => f.Name);
    return this.DirtyFieldNames;
  get EffectiveIsSaving(): boolean {
    return this.IsSaving;
  get EffectiveWidthMode(): FormWidthMode {
    if (this.fc?.getFormWidthMode) {
      return this.fc.getFormWidthMode();
    return this.WidthMode;
  get EffectiveSearchFilter(): string {
    return this.fc?.searchFilter ?? '';
  get EffectiveShowEmptyFields(): boolean {
    return this.fc?.showEmptyFields ?? false;
  get EffectiveHasCustomSectionOrder(): boolean {
    if (this.fc?.hasCustomSectionOrder) {
      return this.fc.hasCustomSectionOrder();
  // ---- Section counts ----
  get TotalSectionCount(): number {
    if (this.fc?.getTotalSectionCount) {
      return this.fc.getTotalSectionCount();
    return this.Panels?.length ?? 0;
  get VisibleSectionCount(): number {
    if (this.fc?.getVisibleSectionCount) {
      return this.fc.getVisibleSectionCount();
    if (!this.Panels) return 0;
    return this.Panels.filter(p => p.IsVisible).length;
  get ExpandedSectionCount(): number {
    if (this.fc?.getExpandedCount) {
      return this.fc.getExpandedCount();
    return this.Panels.filter(p => p.Expanded && p.IsVisible).length;
  // ---- IS-A Related Panel ----
  /** Whether the current record has IS-A related items to display in the side panel */
  get HasIsaRelatedItems(): boolean {
    const record = this.EffectiveRecord;
    if (!record?.EntityInfo) return false;
    const entityInfo = record.EntityInfo;
    // Child entity with overlapping parent — may have siblings
    if (entityInfo.IsChildType && entityInfo.ParentEntityInfo?.AllowMultipleSubtypes) {
      const parent = record.ISAParent;
      if (parent?.ISAChildren && parent.ISAChildren.length > 1) return true;
    // Parent entity with children
    if (entityInfo.IsParentType) {
      if (entityInfo.AllowMultipleSubtypes && record.ISAChildren && record.ISAChildren.length > 0) return true;
      if (!entityInfo.AllowMultipleSubtypes && record.ISAChild) return true;
  // ---- Section Manager ----
  /** Builds section info array from projected panels for the section manager drawer */
  get SectionManagerItems(): SectionManagerItem[] {
    if (!this.Panels) return [];
    return this.Panels.map(p => ({
      SectionKey: p.SectionKey,
      SectionName: p.SectionName,
      Variant: p.Variant,
      Icon: p.Icon
  /** Current section order from the form component */
  get SectionManagerOrder(): string[] {
    if (this.fc?.getSectionOrder) {
      return this.fc.getSectionOrder();
  // ---- Lifecycle ----
  ngAfterContentInit(): void {
    // Subscribe to panel Navigate events and relay them
    this.SubscribeToPanelNavigateEvents();
    // Watch for panel changes to update counts and re-subscribe
    this.Panels.changes.pipe(takeUntil(this.destroy$)).subscribe(() => {
    // Watch for changes to record dirty state
    this.watchRecordChanges();
    this.panelNavReset$.next();
    this.panelNavReset$.complete();
   * Subscribes to Navigate events from all child collapsible panels
   * and relays them through this container's Navigate output.
  private SubscribeToPanelNavigateEvents(): void {
    this.panelNavReset$.next(); // tear down previous subscriptions
    this.Panels.forEach(panel => {
      panel.Navigate.pipe(takeUntil(this.panelNavReset$)).subscribe((event: FormNavigationEvent) => {
   * Monitor record dirty state changes and trigger change detection.
   * This ensures the edit banner updates when fields are modified.
  private watchRecordChanges(): void {
    // Poll for dirty state changes (BaseEntity doesn't expose observables)
      if (this.EffectiveRecord?.Dirty !== undefined) {
    }, 200);
    // Cleanup on destroy
    this.destroy$.subscribe(() => clearInterval(checkInterval));
  // ---- Toolbar Event Handlers ----
   * Navigation events are always re-emitted for the host app to handle.
  OnNavigate(event: FormNavigationEvent): void {
   * Edit mode change: delegate to FormComponent if available, otherwise re-emit.
  OnEditModeChange(editMode: boolean): void {
      if (editMode) {
        this.fc.StartEditMode();
        this.fc.EndEditMode();
      this.EditModeChange.emit(editMode);
   * Save: delegate to FormComponent if available, otherwise re-emit.
  async OnSaveRequested(): Promise<void> {
    if (this.fc?.SaveRecord) {
      // Mark as saving to prevent double-click
        await this.fc.SaveRecord(true);
        // Use microtask timing to avoid ExpressionChangedAfterItHasBeenCheckedError
      this.SaveRequested.emit();
   * Cancel: delegate to FormComponent if available, otherwise re-emit.
  OnCancelRequested(): void {
    if (this.fc?.CancelEdit) {
      this.fc.CancelEdit();
      this.CancelRequested.emit();
   * Delete, Favorite, History, Lists, ShowChanges: always re-emit for host app.
  OnDeleteRequested(): void {
    this.DeleteRequested.emit();
  OnFavoriteToggled(): void {
    this.FavoriteToggled.emit();
  OnHistoryRequested(): void {
    // Check if event should be cancelled
    const beforeEvent = new BeforeHistoryViewEventArgs();
    this.BeforeHistoryView.emit(beforeEvent);
    if (beforeEvent.Cancel) return;
    // If not cancelled, show built-in record changes drawer
    this.ShowRecordChanges = true;
    // Also emit for backward compatibility
    this.HistoryRequested.emit();
  OnListManagementRequested(): void {
    const beforeEvent = new BeforeListManagementEventArgs();
    this.BeforeListManagement.emit(beforeEvent);
    // If not cancelled, show built-in list management dialog
    this.ShowListManagement = true;
    this.ListManagementRequested.emit();
  OnRecordChangesClosed(): void {
    this.ShowRecordChanges = false;
  OnListManagementClosed(): void {
    this.ShowListManagement = false;
  OnShowChangesRequested(): void {
    if (this.fc?.ShowChanges) {
      this.fc.ShowChanges();
      this.ShowChangesRequested.emit();
  // ---- Section Control Handlers ----
  OnFilterChange(filter: string): void {
    if (this.fc?.onFilterChange) {
      this.fc.onFilterChange(filter);
  OnExpandAll(): void {
    if (this.fc?.expandAllSections) {
      this.fc.expandAllSections();
  OnCollapseAll(): void {
    if (this.fc?.collapseAllSections) {
      this.fc.collapseAllSections();
  OnShowEmptyFieldsChange(show: boolean): void {
      this.fc.showEmptyFields = show;
  OnWidthModeChange(mode: FormWidthMode): void {
    if (this.fc?.setFormWidthMode) {
      this.fc.setFormWidthMode(mode);
      this.WidthMode = mode;
  OnResetSectionOrder(): void {
    if (this.fc?.resetSectionOrder) {
      this.fc.resetSectionOrder();
  // ---- Section Manager Handlers ----
  OnManageSections(): void {
    this.ShowSectionManager = true;
  OnSectionOrderChange(newOrder: string[]): void {
    if (this.fc?.setSectionOrder) {
      this.fc.setSectionOrder(newOrder);
  OnSectionManagerReset(): void {
    this.OnResetSectionOrder();
  OnSectionManagerClosed(): void {
    this.ShowSectionManager = false;
