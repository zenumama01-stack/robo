import { Component, Input, OnInit, ViewChild, ChangeDetectorRef } from '@angular/core';
import { BaseEntity, CompositeKey, LogError, LogErrorEx, LogStatus, Metadata, RunView, RunViewResult } from '@memberjunction/core';
import { MJListDetailEntity, ListDetailEntityExtended, MJListEntity, UserViewEntityExtended } from '@memberjunction/core-entities';
import { ListDetailGridComponent, ListGridRowClickedEvent } from '@memberjunction/ng-list-detail-grid';
import { GridToolbarConfig } from '@memberjunction/ng-entity-viewer';
import { NewItemOption } from '../../generic/Item.types';
interface AddableRecord {
  selector: 'mj-list-detail',
  templateUrl: './single-list-detail.component.html',
  styleUrls: ['./single-list-detail.component.css', '../../shared/first-tab-styles.css']
export class SingleListDetailComponent implements OnInit {
  @Input() public ListID: string = "";
  @ViewChild('listDetailGrid') listDetailGrid: ListDetailGridComponent | undefined;
  // List record
  public listRecord: MJListEntity | null = null;
  public showLoader: boolean = false;
  // Grid state
  public selectedKeys: string[] = [];
  public rowCount: number = 0;
  // Toolbar config - hide EDG toolbar, we'll use our own
  public gridToolbarConfig: GridToolbarConfig = {
    showSearch: false,
    showRefresh: false,
    showAdd: false,
    showDelete: false,
    showExport: false,
    showRowCount: false,
    showSelectionCount: false
  // Remove from list dialog
  public showRemoveDialog: boolean = false;
  public isRemoving: boolean = false;
  public removeProgress: number = 0;
  public removeTotal: number = 0;
  // Add records dialog
  public showAddRecordsDialog: boolean = false;
  public addDialogLoading: boolean = false;
  public addDialogSaving: boolean = false;
  public addRecordsSearchFilter: string = "";
  public existingListDetailIds: Set<string> = new Set();
  public addProgress: number = 0;
  public addTotal: number = 0;
  private searchSubject: Subject<string> = new Subject();
  // Add from view dialog (existing)
  public showAddFromViewDialog: boolean = false;
  public showAddFromViewLoader: boolean = false;
  public addFromViewProgress: number = 0;
  public addFromViewTotal: number = 0;
  public fetchingRecordsToSave: boolean = false;
  // Dropdown menu options
  public addOptions: NewItemOption[] = [
      Text: 'Add Records',
      Description: 'Search and add specific records to this list',
      Icon: 'search',
      Action: () => this.openAddRecordsDialog()
      Text: 'Add From View',
      Description: 'Add all records from a saved view',
      Icon: 'folder',
      Action: () => this.openAddFromViewDialog()
  public async ngOnInit(): Promise<void> {
    if (this.ListID) {
      await this.loadListRecord();
   * Load the list entity record
  private async loadListRecord(): Promise<void> {
    if (!this.ListID) return;
    this.showLoader = true;
      this.listRecord = await md.GetEntityObject<MJListEntity>("MJ: Lists");
      const loadResult = await this.listRecord.Load(this.ListID);
        LogError("Error loading list with ID " + this.ListID, undefined, this.listRecord.LatestResult);
        this.listRecord = null;
      LogError("Error loading list", undefined, error);
      this.showLoader = false;
  // Grid Event Handlers
  onRowClicked(_event: ListGridRowClickedEvent): void {
    // Selection is handled by the grid
  onRowDoubleClicked(_event: ListGridRowClickedEvent): void {
    // Navigation is handled by mj-list-detail-grid
  onSelectionChange(keys: string[]): void {
    this.selectedKeys = keys;
  onDataLoaded(event: { totalCount: number }): void {
    this.rowCount = event.totalCount;
  refreshGrid(): void {
    if (this.listDetailGrid) {
      this.listDetailGrid.refresh();
  // Toolbar Actions
  onRefreshClick(): void {
    this.refreshGrid();
  onExportClick(): void {
    // Trigger export on the underlying grid
      this.listDetailGrid.export();
  onDropdownItemClick(item: NewItemOption): void {
    if (item.Action) {
      item.Action();
  // Remove from List Dialog
  openRemoveDialog(): void {
    if (this.selectedKeys.length === 0) {
      this.sharedService.CreateSimpleNotification("Please select records to remove", 'warning', 2500);
    this.showRemoveDialog = true;
  closeRemoveDialog(): void {
    this.showRemoveDialog = false;
    this.isRemoving = false;
    this.removeProgress = 0;
    this.removeTotal = 0;
  async confirmRemoveFromList(): Promise<void> {
    if (!this.listRecord || this.selectedKeys.length === 0) return;
    this.isRemoving = true;
    this.removeTotal = this.selectedKeys.length;
    const entityInfo = md.EntityByID(this.listRecord.EntityID);
    // selectedKeys from grid are in concatenated format (ID|value)
    // For single PK entities, RecordID in DB is just the raw value
    // For composite PK entities, RecordID in DB is the concatenated format
    // Extract the appropriate format for the query
    const selectedRecordIds = this.selectedKeys.map(key => {
      if (entityInfo && entityInfo.PrimaryKeys.length === 1) {
        // Single PK: extract just the value from concatenated format
        compositeKey.LoadFromConcatenatedString(key);
        return compositeKey.KeyValuePairs[0]?.Value || key;
        // Composite PK: use full concatenated format as-is
    const listDetailsFilter = `ListID = '${this.listRecord.ID}' AND RecordID IN (${selectedRecordIds.map(id => `'${id}'`).join(',')})`;
    const listDetailsResult = await rv.RunView<MJListDetailEntity>({
      ExtraFilter: listDetailsFilter,
    if (!listDetailsResult.Success) {
      LogError("Error loading list details for removal", undefined, listDetailsResult.ErrorMessage);
      this.sharedService.CreateSimpleNotification("Failed to remove records", 'error', 2500);
    // Use transaction group for bulk delete
    const listDetails = listDetailsResult.Results;
    for (const listDetail of listDetails) {
      await listDetail.Delete();
      this.removeProgress = this.removeTotal;
        `Removed ${listDetails.length} record${listDetails.length !== 1 ? 's' : ''} from list`,
      this.closeRemoveDialog();
      this.listDetailGrid?.clearSelection();
      LogError("Error removing records from list");
      this.sharedService.CreateSimpleNotification("Failed to remove some records", 'error', 2500);
  async openAddRecordsDialog(): Promise<void> {
    this.addRecordsSearchFilter = "";
  closeAddRecordsDialog(): void {
    if (!this.listRecord) return;
      ExtraFilter: `ListID = '${this.listRecord.ID}'`,
  onAddRecordsSearchChange(value: string): void {
    if (!this.listRecord || !searchText || searchText.length < 2) {
    const sourceEntityInfo = md.EntityByID(this.listRecord.EntityID);
      EntityName: this.listRecord.Entity,
  toggleRecordSelection(record: AddableRecord): void {
  get selectedAddableRecords(): AddableRecord[] {
  selectAllAddable(): void {
  deselectAllAddable(): void {
  async confirmAddRecords(): Promise<void> {
    if (recordsToAdd.length === 0 || !this.listRecord) return;
    // Reserve 20% of progress for tg.Submit()
    const progressPerRecord = 0.8 / recordsToAdd.length; // 80% for individual saves
    for (let i = 0; i < recordsToAdd.length; i++) {
      const record = recordsToAdd[i];
      const listDetail = await md.GetEntityObject<ListDetailEntityExtended>("MJ: List Details", md.CurrentUser);
      listDetail.ListID = this.listRecord.ID;
      const result = await listDetail.Save();
          message: listDetail.LatestResult?.CompleteMessage
      // Update progress (0-80%)
      this.addProgress = Math.round((i + 1) * progressPerRecord * this.addTotal);
    // Show 80% complete before submit
    this.addProgress = Math.round(this.addTotal * 0.8);
      LogError("Error adding records to list");
      this.sharedService.CreateSimpleNotification("Failed to add some records", 'error', 2500);
  // Add From View Dialog (existing functionality, cleaned up)
  async openAddFromViewDialog(): Promise<void> {
  closeAddFromViewDialog(): void {
    if (!this.listRecord || !this.listRecord.Entity) return;
      EntityName: "MJ: User Views",
      ExtraFilter: `UserID = '${md.CurrentUser.ID}' AND EntityID = '${this.listRecord.EntityID}'`,
      LogError(`Error loading User Views for entity ${this.listRecord.Entity}`);
  toggleViewSelection(view: UserViewEntityExtended): void {
  isViewSelected(view: UserViewEntityExtended): boolean {
  async confirmAddFromView(): Promise<void> {
    if (!this.listRecord || this.userViewsToAdd.length === 0) return;
        Fields: ["ID"]
    const progressPerRecord = 0.8 / Math.max(recordsToAdd.length, 1); // 80% for individual saves
      this.sharedService.CreateSimpleNotification("All records already in list", 'info', 2500);
      const recordID = recordsToAdd[i];
      this.addFromViewProgress = Math.round((i + 1) * progressPerRecord * this.addFromViewTotal);
    this.addFromViewProgress = Math.round(this.addFromViewTotal * 0.8);
      LogError("Error adding records from view to list");
