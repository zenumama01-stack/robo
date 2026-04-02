  AfterViewInit, OnInit, OnDestroy, Directive,
  ViewChildren, QueryList, ElementRef, ChangeDetectorRef,
  Output, EventEmitter, inject
import { Subject, Subscription, debounceTime } from 'rxjs';
  EntityInfo, ValidationResult, BaseEntity, EntityPermissionType,
  EntityRelationshipInfo, Metadata, RunViewParams, LogError,
  RecordDependency, BaseEntityEvent, CompositeKey, RunView, RunViewResult
import { MJEventType, MJGlobal, ValidationErrorInfo } from '@memberjunction/global';
import { FormEditingCompleteEvent, PendingRecordItem, BaseFormComponentEventCodes } from '@memberjunction/ng-base-types';
import { BaseRecordComponent } from './base-record-component';
import { BaseFormSectionInfo } from './base-form-section-info';
import { MjCollapsiblePanelComponent } from './panel/collapsible-panel.component';
import { FormNavigationEvent } from './types/navigation-events';
  FormContext,
  FormNotificationEvent,
  RecordSavedEvent,
  RecordDeletedEvent,
  RecordSaveFailedEvent,
  RecordDeleteFailedEvent,
  ValidationFailedEvent
} from './types/form-types';
import { FormStateService } from './form-state.service';
 * Abstract base class for all entity record forms in MemberJunction.
 * Generated forms (via CodeGen) and custom forms extend this class.
 * It provides core form logic — section management, validation, pending records,
 * permissions, favorites, related entity helpers — with ZERO Explorer dependencies.
 * Instead of calling Explorer services directly, events are emitted via @Output:
 * - **Navigate**: Form navigation requests (record links, new records, external links)
 * - **Notification**: User-facing notifications (save success, validation errors, etc.)
 * - **RecordSaved**: Emitted after a successful save
 * - **RecordDeleted**: Emitted after a successful delete
 * The host application (e.g. SingleRecordComponent in Explorer) subscribes to
 * these events and maps them to its own services (NavigationService, SharedService, etc.).
export abstract class BaseFormComponent extends BaseRecordComponent implements AfterViewInit, OnInit, OnDestroy {
  public EditMode: boolean = false;
  public FavoriteInitDone: boolean = false;
  public isHistoryDialogOpen: boolean = false;
  public showDeleteDialog: boolean = false;
  public showCreateDialog: boolean = false;
   * Height of the top area when a splitter layout is used.
   * Referenced by CodeGen-generated templates for entities with "top area" sections.
  public TopAreaHeight: string = '300px';
   * Called when the splitter layout changes (for entities with "top area" sections).
   * No-op in the generic version; override in host application if splitter resizing is needed.
  public splitterLayoutChange(): void {
    // No-op — host application can override
  private _pendingRecords: PendingRecordItem[] = [];
  // #region Injected Dependencies (no constructor params)
  protected elementRef = inject(ElementRef);
  public cdr = inject(ChangeDetectorRef);
  protected formStateService = inject(FormStateService);
  // #endregion
  // #region @Output Events
  /** Emitted when navigation is requested (record link, external link, email, new record, etc.) */
  @Output() Navigate = new EventEmitter<FormNavigationEvent>();
  /** Emitted when a user-facing notification should be shown */
  @Output() Notification = new EventEmitter<FormNotificationEvent>();
  /** Emitted after a record is saved successfully */
  @Output() RecordSaved = new EventEmitter<RecordSavedEvent>();
  /** Emitted after a record is deleted successfully */
  @Output() RecordDeleted = new EventEmitter<RecordDeletedEvent>();
  /** Emitted when a record save fails */
  @Output() RecordSaveFailed = new EventEmitter<RecordSaveFailedEvent>();
  /** Emitted when a record delete fails */
  @Output() RecordDeleteFailed = new EventEmitter<RecordDeleteFailedEvent>();
  /** Emitted when validation fails before save */
  @Output() ValidationFailed = new EventEmitter<ValidationFailedEvent>();
  /** Subscription to form state changes */
  private formStateSubscription?: Subscription;
  @ViewChildren(MjCollapsiblePanelComponent) collapsiblePanels!: QueryList<MjCollapsiblePanelComponent>;
        this.StartEditMode();
      this._isFavorite = await md.GetRecordFavoriteStatus(md.CurrentUser.ID, this.record.EntityInfo.Name, this.record.PrimaryKey);
      this.FavoriteInitDone = true;
      // Initialize form state from User Settings
      const entityName = this.getEntityName();
        await this.formStateService.initializeState(entityName);
        this.formStateSubscription = this.formStateService.getState$(entityName).subscribe(state => {
          this.showEmptyFields = state.showEmptyFields;
    // Set up debounced filter subscription
    this.filterSubscription = this.filterSubject
      .pipe(debounceTime(100))
        this.searchFilter = searchTerm;
        // After change detection propagates FormContext to panels and they update
        // their IsVisible state, expand any visible (matching) sections that are collapsed
          Promise.resolve().then(() => this.expandMatchingSections());
    // Subclasses can override for post-view-init logic
    if (this.filterSubscription) {
      this.filterSubscription.unsubscribe();
    if (this.formStateSubscription) {
      this.formStateSubscription.unsubscribe();
  // #region Pending Records
  protected get PendingRecords(): PendingRecordItem[] {
    return this._pendingRecords;
  protected PendingRecordsDirty(): boolean {
    this.PopulatePendingRecords();
    const pendingRecords = this.PendingRecords;
    if (pendingRecords && pendingRecords.length > 0) {
      for (const p of pendingRecords) {
        if (p.action === 'delete')
        else if (p.entityObject.Dirty)
    this._pendingRecords = [];
    this.RaiseEvent(BaseFormComponentEventCodes.EDITING_COMPLETE);
    const result = this.RaiseEvent(BaseFormComponentEventCodes.POPULATE_PENDING_RECORDS);
    if (result && result.pendingChanges) {
      for (const p of result.pendingChanges)
        this.PendingRecords.push(p);
  protected ValidatePendingRecords(): ValidationResult[] {
    const results: ValidationResult[] = [];
    for (let i = 0; i < this._pendingRecords.length; i++) {
      const pendingRecord = this._pendingRecords[i];
      results.push(pendingRecord.entityObject.Validate());
  // #region Edit Mode
    this.EditMode = true;
  public EndEditMode(): void {
    this.EditMode = false;
    this.clearValidationState();
  public handleHistoryDialog(): void {
    this.isHistoryDialogOpen = !this.isHistoryDialogOpen;
  // #region Core Form Operations
  public Validate(): ValidationResult {
    const valResults = (<BaseEntity>this.record).Validate();
    const pendingValResults = this.ValidatePendingRecords();
    for (let i = 0; i < pendingValResults.length; i++) {
      const pendingValResult = pendingValResults[i];
      if (!pendingValResult.Success) {
        valResults.Success = false;
        valResults.Errors.push(...pendingValResult.Errors);
    return valResults;
  protected RaiseEvent(eventCode: BaseFormComponentEventCodes) {
    const event = new FormEditingCompleteEvent();
    event.elementRef = this.elementRef;
    event.subEventCode = eventCode;
      eventCode: BaseFormComponentEventCodes.BASE_CODE,
      args: event
    return event;
  public async SaveRecord(StopEditModeAfterSave: boolean): Promise<boolean> {
        (document.activeElement as HTMLElement).blur();
      } catch (_e) {
        // ignore blur errors
        const valResults = this.Validate();
        if (valResults.Success) {
          const result = await this.InternalSaveRecord();
            if (StopEditModeAfterSave)
              this.EndEditMode();
            this.Notification.emit({ Message: 'Record saved successfully', Type: 'success', Duration: 2500 });
            this.RecordSaved.emit({
              EntityName: this.record.EntityInfo.Name,
              RecordId: this.record.PrimaryKey.ToString(),
              Result: { Success: true }
            const serverMsg = this.record.LatestResult?.Message || '';
            const errorMsg = serverMsg ? `Save failed: ${serverMsg}` : 'Error saving record';
            this.Notification.emit({ Message: errorMsg, Type: 'error', Duration: 5000 });
            this.RecordSaveFailed.emit({ EntityName: this.record.EntityInfo.Name, ErrorMessage: errorMsg });
          // Broadcast validation errors to all fields via FormContext
          this._showValidation = true;
          this._validationErrors = valResults.Errors;
          const errorMessages = valResults.Errors.map(x => x.Message);
          this.Notification.emit({
            Message: 'Validation Errors\n' + errorMessages.join('\n'),
            Type: 'warning',
            Duration: 5000
          this.ValidationFailed.emit({ EntityName: this.record.EntityInfo.Name, Errors: errorMessages });
      LogError("Could not save record: Record not found");
      const errorMsg = 'Error saving record: ' + e;
      const r = <BaseEntity>this.record;
      if (r.Dirty || this.PendingRecordsDirty()) {
        // Revert is safe here — the toolbar's discard dialog already confirmed with the user
        r.Revert();
          pendingRecords.forEach(p => {
            if (p.action === 'save')
              p.entityObject.Revert();
        this.RaiseEvent(BaseFormComponentEventCodes.REVERT_PENDING_CHANGES);
      if (this._pendingRecords.length > 0) {
        this.record.TransactionGroup = tg;
        for (const x of this._pendingRecords) {
          x.entityObject.TransactionGroup = tg;
          if (x.action === 'save') {
            await x.entityObject.Save();
            await x.entityObject.Delete();
        return await tg.Submit();
        return await this.record.Save();
  public async Wait(duration: number): Promise<void> {
    return new Promise<void>(resolve => setTimeout(resolve, duration));
  // #region Permissions
  private _userPermissions: { permission: EntityPermissionType; canDo: boolean }[] = [];
  public CheckUserPermission(type: EntityPermissionType): boolean {
        const perm = this._userPermissions.find(x => x.permission === type);
        if (perm)
          return perm.canDo;
          const result = this.record.CheckPermissions(type, false);
          this._userPermissions.push({ permission: type, canDo: result });
  public get UserCanEdit(): boolean {
    return this.CheckUserPermission(EntityPermissionType.Update);
  public get UserCanRead(): boolean {
    return this.CheckUserPermission(EntityPermissionType.Read);
    return this.CheckUserPermission(EntityPermissionType.Create);
    return this.CheckUserPermission(EntityPermissionType.Delete);
  // #region Grid Edit Mode
  public GridEditMode(): "None" | "Save" | "Queue" {
  // #region Favorites
  private _isFavorite: boolean = false;
  public get IsFavorite(): boolean {
    return this._isFavorite;
  public async RemoveFavorite(): Promise<void> {
    return this.SetFavoriteStatus(false);
  public async MakeFavorite(): Promise<void> {
    return this.SetFavoriteStatus(true);
  public async SetFavoriteStatus(isFavorite: boolean) {
    await md.SetRecordFavoriteStatus(md.CurrentUser.ID, this.record.EntityInfo.Name, this.record.PrimaryKey, isFavorite);
    this._isFavorite = isFavorite;
  // #region Related Entity Helpers
  public BuildRelationshipViewParams(item: EntityRelationshipInfo): RunViewParams {
    return EntityInfo.BuildRelationshipViewParams(this.record, item);
  public BuildRelationshipViewParamsByEntityName(relatedEntityName: string, relatedEntityJoinField?: string): RunViewParams {
    const eri = this.GetEntityRelationshipByRelatedEntityName(relatedEntityName, relatedEntityJoinField);
    if (eri)
      return this.BuildRelationshipViewParams(eri);
  public GetEntityRelationshipByRelatedEntityName(relatedEntityName: string, relatedEntityJoinField?: string): EntityRelationshipInfo | undefined {
      const ret = r.EntityInfo.RelatedEntities.filter(x => x.RelatedEntity.trim().toLowerCase() === relatedEntityName.trim().toLowerCase());
      if (ret.length > 1 && relatedEntityJoinField) {
        const ret2 = ret.find(x => x.RelatedEntityJoinField.trim().toLowerCase() === relatedEntityJoinField.trim().toLowerCase());
        if (ret2)
          return ret2;
      } else if (ret.length === 1) {
        return ret[0];
  public NewRecordValues(relatedEntityName: string): Record<string, unknown> {
    const eri = this.GetEntityRelationshipByRelatedEntityName(relatedEntityName);
      return this.NewRecordValuesByEntityRelationship(eri);
  public NewRecordValuesByEntityRelationship(item: EntityRelationshipInfo): Record<string, unknown> {
    return EntityInfo.BuildRelationshipNewRecordValues(this.record, item);
  public GetRelatedEntityTabDisplayName(relatedEntityName: string): string {
      const eri = (<BaseEntity>this.record).EntityInfo.RelatedEntities.find(x => x.RelatedEntity === relatedEntityName);
        return eri.DisplayName;
    return relatedEntityName;
  public get HasRelatedEntities(): boolean {
      const relatedEntities = (<BaseEntity>this.record).EntityInfo.RelatedEntities;
      return relatedEntities && relatedEntities.length > 0;
  // #region Entity Info & Dependencies
  public get EntityInfo(): EntityInfo | undefined {
      if (r)
        return r.EntityInfo;
  public async ShowDependencies() {
    const dep = await md.GetRecordDependencies(this.record.EntityInfo.Name, this.record.PrimaryKey);
    console.log('Dependencies for: ' + this.record.EntityInfo.Name + ' ' + this.record.PrimaryKey.ToString());
    console.log(dep);
  public ShowChanges(): void {
      const changes = this.record.GetAll(false, true);
      const oldValues = this.record.GetAll(true, true);
      const message =
        'Changes for: ' + this.record.EntityInfo.Name + ' ' + this.record.PrimaryKey.ToString() +
        '\n\n' + JSON.stringify(changes, null, 2) +
        '\n\nOld Values\n\n' + JSON.stringify(oldValues, null, 2);
      console.log(message);
      this.Notification.emit({ Message: message, Type: 'info', Duration: 30000 });
  public async GetListsCanAddTo(): Promise<MJListEntity[]> {
      LogError('Unable to fetch List records: Record not found');
    const rvResult: RunViewResult<MJListEntity> = await rv.RunView({
      ExtraFilter: `UserID = '${md.CurrentUser.ID}' AND EntityID = '${this.record.EntityInfo.ID}'`,
      LogError('Error running view to fetch lists', undefined, rvResult.ErrorMessage);
  public async GetRecordDependencies(): Promise<RecordDependency[]> {
    const dependencies: RecordDependency[] = await md.GetRecordDependencies(this.record.EntityInfo.Name, this.record.PrimaryKey);
    return dependencies;
  // #region Collapsible Section Management
  protected sections: BaseFormSectionInfo[] = [];
  private sectionMap: Map<string, BaseFormSectionInfo> = new Map();
  public searchFilter: string = '';
  public showEmptyFields: boolean = false;
  /** Whether to show all validation errors on fields (set after save failure) */
  private _showValidation = false;
  /** Validation errors from the most recent failed save attempt */
  private _validationErrors: ValidationErrorInfo[] = [];
  public get formContext(): FormContext {
      sectionFilter: this.searchFilter,
      showEmptyFields: this.showEmptyFields,
      showValidation: this._showValidation,
      validationErrors: this._validationErrors
  /** Clears all validation display state (called on save success, cancel, end edit) */
  private clearValidationState(): void {
    this._showValidation = false;
    this._validationErrors = [];
  private filterSubject = new Subject<string>();
  private filterSubscription?: Subscription;
  protected initSections(sections: (BaseFormSectionInfo | { sectionKey: string; sectionName: string; isExpanded: boolean; rowCount?: number; metadata?: unknown })[]): void {
    this.sections = sections.map(s =>
      s instanceof BaseFormSectionInfo
        ? s
        : new BaseFormSectionInfo(s.sectionKey, s.sectionName, s.isExpanded, s.rowCount, s.metadata)
    this.sectionMap.clear();
    this.sections.forEach(section => {
      this.sectionMap.set(section.sectionKey, section);
  protected getSection(sectionKey: string): BaseFormSectionInfo | undefined {
    return this.sectionMap.get(sectionKey);
  public IsSectionExpanded(sectionKey: string, defaultExpanded?: boolean): boolean {
      return this.formStateService.isSectionExpanded(entityName, sectionKey, defaultExpanded);
    const section = this.sectionMap.get(sectionKey);
    return section ? section.isExpanded : (defaultExpanded !== undefined ? defaultExpanded : true);
  public SetSectionExpanded(sectionKey: string, isExpanded: boolean): void {
      this.formStateService.setSectionExpanded(entityName, sectionKey, isExpanded);
    if (section) {
      section.isExpanded = isExpanded;
  public GetSectionRowCount(sectionKey: string): number | undefined {
    return section?.rowCount;
  public SetSectionRowCount(sectionKey: string, rowCount: number): void {
      section.rowCount = rowCount;
  public GetSectionPanelHeight(sectionKey: string): number | undefined {
      return this.formStateService.getSectionPanelHeight(entityName, sectionKey);
  public SetSectionPanelHeight(sectionKey: string, height: number): void {
      this.formStateService.setSectionPanelHeight(entityName, sectionKey, height);
  public toggleSection(sectionKey: string): void {
      this.formStateService.toggleSection(entityName, sectionKey);
      section.isExpanded = !section.isExpanded;
   * Expand any sections that are visible (match the current filter) but currently collapsed.
   * Called after filter changes so the user can immediately see matching content.
  private expandMatchingSections(): void {
    if (!this.collapsiblePanels) return;
    this.collapsiblePanels.forEach(panel => {
      if (panel.IsVisible && !panel.Expanded) {
        this.SetSectionExpanded(panel.SectionKey, true);
  public expandAllSections(): void {
    const sectionKeys = this.sections.map(s => s.sectionKey);
    if (entityName && sectionKeys.length > 0) {
      this.formStateService.expandAllSections(entityName, sectionKeys);
      section.isExpanded = true;
  public collapseAllSections(): void {
      this.formStateService.collapseAllSections(entityName, sectionKeys);
      section.isExpanded = false;
  public getExpandedCount(): number {
      return this.formStateService.getExpandedCount(entityName, sectionKeys);
    return this.sections.filter(section => section.isExpanded).length;
  public getVisibleSectionCount(): number {
    if (!this.collapsiblePanels || this.collapsiblePanels.length === 0) {
      return this.sections.length;
    return this.collapsiblePanels.filter(panel => panel.IsVisible).length;
  public getTotalSectionCount(): number {
  public onFilterChange(searchTerm: string): void {
    this.filterSubject.next(searchTerm);
    return this.record?.EntityInfo?.Name || '';
  public getFormWidthMode(): 'centered' | 'full-width' {
      return this.formStateService.getWidthMode(entityName);
    return 'centered';
  public setFormWidthMode(widthMode: 'centered' | 'full-width'): void {
      this.formStateService.setWidthMode(entityName, widthMode);
  public toggleFormWidthMode(): void {
      this.formStateService.toggleWidthMode(entityName);
  // #region Section Ordering
  public getSectionOrder(): string[] {
      const customOrder = this.formStateService.getSectionOrder(entityName);
      if (customOrder && customOrder.length > 0) {
        return customOrder;
    return this.sections.map(s => s.sectionKey);
  public setSectionOrder(sectionOrder: string[]): void {
      this.formStateService.setSectionOrder(entityName, sectionOrder);
  public resetSectionOrder(): void {
      this.formStateService.resetSectionOrder(entityName);
  public hasCustomSectionOrder(): boolean {
      return this.formStateService.hasCustomSectionOrder(entityName);
  public getSectionDisplayOrder(sectionKey: string): number {
    const order = this.getSectionOrder();
    const index = order.indexOf(sectionKey);
    return index >= 0 ? index : this.sections.length;
  // #region Form Container Event Handlers
   * Handles navigation events from the form container and related entity grids.
   * Relays the event upward via the Navigate @Output for the host application to handle.
   * Generated form templates bind this as: (Navigate)="OnFormNavigate($event)"
  public OnFormNavigate(event: FormNavigationEvent): void {
    this.Navigate.emit(event);
   * Handles delete requests from the form container.
   * Emits RecordDeleted on success, RecordDeleteFailed on failure.
   * Generated form templates bind this as: (DeleteRequested)="OnDeleteRequested()"
  public async OnDeleteRequested(): Promise<void> {
    if (!this.record || !this.record.IsSaved) return;
      const result = await this.record.Delete();
        this.Notification.emit({ Message: 'Record deleted successfully', Type: 'success', Duration: 2500 });
        this.RecordDeleted.emit({
          RecordId: this.record.PrimaryKey.ToString()
        const errorMsg = 'Error deleting record';
        this.RecordDeleteFailed.emit({ EntityName: this.record.EntityInfo.Name, ErrorMessage: errorMsg });
      const errorMsg = 'Error deleting record: ' + e;
   * Handles favorite toggle from the form container.
   * Generated form templates bind this as: (FavoriteToggled)="OnFavoriteToggled()"
  public async OnFavoriteToggled(): Promise<void> {
    await this.SetFavoriteStatus(!this.IsFavorite);
   * Handles history view requests from the form container.
   * Generated form templates bind this as: (HistoryRequested)="OnHistoryRequested()"
  public OnHistoryRequested(): void {
    this.handleHistoryDialog();
   * Handles list management requests from the form container.
   * Generated form templates bind this as: (ListManagementRequested)="OnListManagementRequested()"
  public OnListManagementRequested(): void {
    // List management dialog is handled by the host application
    // Subclasses can override this to implement custom list management
