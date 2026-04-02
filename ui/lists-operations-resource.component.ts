import { BaseResourceComponent, SharedService } from '@memberjunction/ng-shared';
import { ResourceData, MJListEntity, MJListDetailEntity, MJUserSettingEntity, UserInfoEngine } from '@memberjunction/core-entities';
import { Metadata, RunView, EntityInfo, CompositeKey } from '@memberjunction/core';
import { ListSetOperationsService, VennData, VennIntersection, SetOperation, SetOperationResult } from '../services/list-set-operations.service';
import { VennRegionClickEvent } from './venn-diagram/venn-diagram.component';
interface ListSelection {
 * Preview record with meaningful display fields
interface PreviewRecord {
  secondaryInfo?: string;
 * Entity option for filtering
interface EntityOption {
@RegisterClass(BaseResourceComponent, 'ListsOperationsResource')
  selector: 'mj-lists-operations-resource',
    <div class="operations-container">
      <div class="operations-header">
        <div class="header-top">
            <h2>List Operations</h2>
          @if (selectedLists.length > 0 || selectedEntityId) {
              class="clear-all-btn"
              (click)="clearAllSelections()"
              title="Clear all selections">
        <div class="header-subtitle">
          Visualize overlaps and perform set operations on your lists
      <div class="operations-content">
        <!-- Left Panel: List Selection -->
        <div class="selection-panel">
            <h3>Selected Lists</h3>
            @if (selectedLists.length > 0) {
              <span class="list-count">
                {{selectedLists.length}}/{{maxLists}}
          <!-- Entity Filter Selector -->
          <div class="entity-filter-section">
            <label class="filter-label">Filter by Entity</label>
            <div class="entity-selector">
                [(ngModel)]="selectedEntityId"
                (ngModelChange)="onEntityFilterChange()"
                class="entity-select">
                <option value="">All Entities</option>
                @for (entity of entityOptions; track entity) {
                  <option [value]="entity.id">
                    {{entity.name}} ({{entity.listCount}})
          <!-- Selected lists -->
          <div class="selected-lists">
            @for (item of selectedLists; track item; let i = $index) {
              <div class="selected-item">
                <div class="item-color" [style.background-color]="item.color"></div>
                  <span class="item-name">{{item.list.Name}}</span>
                  <span class="item-entity">{{item.entityName}}</span>
                <button class="remove-btn" (click)="removeList(i)">
            @if (selectedLists.length < maxLists) {
              <div class="add-list-area">
                <div class="add-list-search">
                    [(ngModel)]="listSearchTerm"
                    (ngModelChange)="filterAvailableLists()"
                    placeholder="Search lists to add..."
                    (focus)="showListDropdown = true" />
                <!-- Available lists dropdown -->
                @if (showListDropdown && filteredAvailableLists.length > 0) {
                  <div class="list-dropdown">
                    <div class="dropdown-backdrop" (click)="showListDropdown = false"></div>
                    <div class="dropdown-content">
                      @for (list of filteredAvailableLists; track list) {
                          (click)="addList(list)">
                          <span class="dropdown-name">{{list.Name}}</span>
                          <span class="dropdown-entity">{{list.Entity}}</span>
            @if (selectedLists.length >= maxLists) {
              <div class="lists-full">
                Maximum {{maxLists}} lists can be compared
          <!-- Entity consistency note -->
            <div class="entity-note">
              <span>Comparing lists of type: <strong>{{selectedLists[0].entityName}}</strong></span>
          <!-- Quick Operations -->
          @if (selectedLists.length >= 2) {
            <div class="quick-operations">
              <h4>Quick Operations</h4>
              <div class="operation-buttons">
                <button class="op-btn" (click)="performOperation('union')" [disabled]="isCalculating">
                  <span>Union All</span>
                <button class="op-btn" (click)="performOperation('intersection')" [disabled]="isCalculating">
                  <i class="fa-solid fa-circle-notch"></i>
                  <span>Intersection</span>
                <button class="op-btn" (click)="performOperation('symmetric_difference')" [disabled]="isCalculating">
                  <i class="fa-solid fa-arrows-split-up-and-left"></i>
                  <span>Unique Each</span>
        <!-- Center: Venn Diagram -->
        <div class="venn-panel">
            <mj-venn-diagram
              [data]="vennData"
              [selectedRegion]="selectedRegion"
              (regionClick)="onRegionClick($event)">
            </mj-venn-diagram>
          @if (selectedLists.length === 0) {
            <div class="venn-empty">
                <i class="fa-solid fa-circle-nodes"></i>
              <h3>Add Lists to Compare</h3>
              <p>Select 2-4 lists from the same entity to visualize their overlaps and perform set operations.</p>
          @if (isCalculating) {
              <mj-loading text="Calculating..."></mj-loading>
        <!-- Right Panel: Selected Region / Results -->
        <div class="results-panel">
              <i class="fa-solid fa-crosshairs"></i>
              {{selectedRegion ? 'Selected Region' : 'Results'}}
          <!-- Selected region details -->
          @if (selectedRegion) {
            <div class="region-details">
              <div class="region-header">
                <span class="region-label">{{selectedRegion.label}}</span>
                <span class="region-count">{{selectedRegion.size}} records</span>
              <div class="region-actions">
                <button class="action-btn primary" (click)="createListFromSelection()">
                  Create New List
                <button class="action-btn" (click)="addToExistingList()">
                  Add to List
                <button class="action-btn" (click)="exportToExcel()">
              @if (previewRecordsDisplay.length > 0) {
                <div class="record-preview">
                  <h5>Preview (first 10)</h5>
                  <div class="preview-list">
                    @for (record of previewRecordsDisplay; track record) {
                      <div class="preview-card">
                        <div class="preview-card-content">
                          <span class="preview-name">{{record.displayName}}</span>
                          @if (record.secondaryInfo) {
                            <span class="preview-secondary">{{record.secondaryInfo}}</span>
                        <button class="preview-open-btn" (click)="openRecord(record)" title="Open record">
                  @if (loadingPreview) {
                    <div class="preview-loading">
                      <mj-loading text="Loading preview..." size="small"></mj-loading>
          <!-- Operation result -->
          @if (lastOperationResult && !selectedRegion) {
            <div class="operation-result">
              <div class="result-header">
                <span class="result-operation">{{getOperationLabel(lastOperationResult.operation)}}</span>
                <span class="result-count">{{lastOperationResult.resultCount}} records</span>
                <button class="action-btn primary" (click)="createListFromResult()">
                <button class="action-btn" (click)="addResultToExistingList()">
          @if (!selectedRegion && !lastOperationResult) {
            <div class="results-empty">
              <p>Click a region in the diagram or run an operation to see results</p>
      <!-- Create List Dialog -->
        <div class="modal-overlay" (click)="cancelCreateDialog()"></div>
            <h3>Create New List from Selection</h3>
            <button class="modal-close" (click)="cancelCreateDialog()">
              <label>List Name *</label>
            <div class="form-info">
              {{recordsToAdd.length}} record{{recordsToAdd.length !== 1 ? 's' : ''}} will be added to this list
            <button class="btn-primary" (click)="confirmCreateList()" [disabled]="!newListName || isSaving">
              {{isSaving ? 'Creating...' : 'Create List'}}
            <button class="btn-secondary" (click)="cancelCreateDialog()">Cancel</button>
      <!-- Add to Existing List Dialog -->
      @if (showAddToListDialog) {
        <div class="modal-overlay" (click)="cancelAddToListDialog()"></div>
        <div class="modal-dialog add-to-list-dialog">
            <h3>Add to Existing List</h3>
            <button class="modal-close" (click)="cancelAddToListDialog()">
            <div class="form-info" style="margin-bottom: 16px;">
              {{recordsToAdd.length}} record{{recordsToAdd.length !== 1 ? 's' : ''}} will be added
            <!-- Search input -->
            <div class="list-search">
                [(ngModel)]="addToListSearchTerm"
                (ngModelChange)="filterAddToListOptions()"
            <!-- List options -->
            <div class="list-options">
              @for (list of filteredAddToListOptions; track list) {
                  class="list-option"
                  [class.selected]="selectedTargetListId === list.ID"
                  (click)="selectTargetList(list.ID)">
                  <div class="list-option-radio">
                      [checked]="selectedTargetListId === list.ID"
                      name="targetList" />
                  <div class="list-option-info">
                    <span class="list-option-name">{{list.Name}}</span>
                    <span class="list-option-entity">{{list.Entity}}</span>
              @if (filteredAddToListOptions.length === 0) {
                <div class="list-options-empty">
                  <p>{{addToListSearchTerm ? 'No lists match your search' : 'No other lists available'}}</p>
            <button class="btn-primary" (click)="confirmAddToList()" [disabled]="!selectedTargetListId || isSaving">
              {{isSaving ? 'Adding...' : 'Add to List'}}
            <button class="btn-secondary" (click)="cancelAddToListDialog()">Cancel</button>
    .operations-container {
    .operations-header {
    .header-top {
      border-color: #f44336;
    .clear-all-btn i {
      color: #9C27B0;
    .operations-content {
      grid-template-columns: 280px 1fr 300px;
    .selection-panel,
    .results-panel {
    .venn-panel {
    /* Entity Filter */
    .entity-filter-section {
    .entity-select {
    .entity-select:focus {
      border-color: #9C27B0;
    .entity-note {
      margin: 8px 12px;
    .entity-note i {
    /* Selected Lists */
    .selected-lists {
    .selected-item {
    .item-color {
    .item-entity {
    /* Add list area */
    .add-list-area {
    .add-list-search {
      border: 1px dashed #ddd;
    .add-list-search:focus-within {
    .add-list-search i {
    .add-list-search input {
    .list-dropdown {
    .dropdown-backdrop {
    .dropdown-content {
      box-shadow: 0 4px 12px rgba(0,0,0,0.15);
    .dropdown-name {
    .dropdown-entity {
    .lists-full {
    .entity-warning {
      margin: 12px;
    .entity-warning i {
    /* Quick Operations */
    .quick-operations {
    .quick-operations h4 {
      margin: 0 0 10px;
    .operation-buttons {
    .op-btn {
    .op-btn:hover:not(:disabled) {
    .op-btn:disabled {
    /* Venn Panel */
    .venn-empty {
      background: linear-gradient(135deg, rgba(156, 39, 176, 0.1) 0%, rgba(156, 39, 176, 0.05) 100%);
    .venn-empty h3 {
    .venn-empty p {
      background: rgba(255,255,255,0.9);
    /* Results Panel */
    .region-details,
    .operation-result {
    .region-header,
    .result-header {
    .region-label,
    .result-operation {
    .region-count,
    .region-actions {
    .action-btn.primary {
      background: #9C27B0;
    .action-btn.primary:hover {
      background: #7B1FA2;
    .record-preview h5 {
    .preview-list {
    .preview-card {
    .preview-card:hover {
      background: #eef1f5;
    .preview-card-content {
    .preview-name {
    .preview-secondary {
    .preview-open-btn {
    .preview-open-btn:hover {
      background: rgba(156, 39, 176, 0.1);
    .preview-loading {
    .results-empty {
    .results-empty i {
    .results-empty p {
    /* Modal */
      width: 420px;
      box-shadow: 0 0 0 3px rgba(156, 39, 176, 0.1);
    .form-info {
    .form-info i {
    /* Add to List Dialog */
    .add-to-list-dialog {
    .add-to-list-dialog .modal-body {
    .list-search {
    .list-search i {
    .list-search .form-input {
    .list-search .form-input:focus {
    .list-options {
      max-height: 250px;
    .list-option {
    .list-option:last-child {
    .list-option:hover {
    .list-option.selected {
    .list-option-radio {
    .list-option-radio input[type="radio"] {
      accent-color: #9C27B0;
    .list-option-info {
    .list-option-name {
    .list-option-entity {
    .list-options-empty {
    .list-options-empty i {
    .list-options-empty p {
        grid-template-rows: auto 1fr auto;
      .selection-panel {
        order: 1;
        order: 2;
export class ListsOperationsResource extends BaseResourceComponent implements OnDestroy {
  maxLists = 4;
  selectedLists: ListSelection[] = [];
  availableLists: MJListEntity[] = [];
  filteredAvailableLists: MJListEntity[] = [];
  listSearchTerm = '';
  showListDropdown = false;
  entityOptions: EntityOption[] = [];
  vennData: VennData | null = null;
  selectedRegion: VennIntersection | null = null;
  lastOperationResult: SetOperationResult | null = null;
  previewRecords: string[] = [];
  previewRecordsDisplay: PreviewRecord[] = [];
  loadingPreview = false;
  isCalculating = false;
  // Create dialog
  recordsToAdd: string[] = [];
  // Add to existing list dialog
  showAddToListDialog = false;
  addToListSearchTerm = '';
  filteredAddToListOptions: MJListEntity[] = [];
  selectedTargetListId: string | null = null;
  private entityIdFromSelectedLists: string | null = null;
  private currentEntityInfo: EntityInfo | null = null;
  // User Settings persistence
  private readonly USER_SETTING_KEY = 'ListsOperations.State';
  private saveSettingsTimeout: ReturnType<typeof setTimeout> | null = null;
  private isLoadingSettings = false;
    private setOperationsService: ListSetOperationsService,
  get hasMultipleEntities(): boolean {
    if (this.selectedLists.length < 2) return false;
    const entities = new Set(this.selectedLists.map(s => s.list.EntityID));
    return entities.size > 1;
    await this.loadAvailableLists();
    await this.loadSavedState();
    // Clear any pending save timeout
    if (this.saveSettingsTimeout) {
      clearTimeout(this.saveSettingsTimeout);
  async loadAvailableLists() {
    const result = await rv.RunView<MJListEntity>({
      ExtraFilter: `UserID = '${md.CurrentUser?.ID}'`,
      this.availableLists = result.Results || [];
      this.buildEntityOptions();
      this.filterAvailableLists();
   * Build entity options for the filter dropdown
  private buildEntityOptions(): void {
    const entityCounts = new Map<string, { name: string; count: number }>();
    for (const list of this.availableLists) {
      const existing = entityCounts.get(list.EntityID);
        entityCounts.set(list.EntityID, { name: list.Entity || 'Unknown', count: 1 });
    this.entityOptions = Array.from(entityCounts.entries())
      .map(([id, data]) => ({
        name: data.name,
        listCount: data.count
      .sort((a, b) => a.name.localeCompare(b.name));
   * Handle entity filter change
  onEntityFilterChange(): void {
    // Clear selected lists if changing entity filter
    if (this.selectedLists.length > 0) {
      const firstEntityId = this.selectedLists[0].list.EntityID;
      if (this.selectedEntityId && this.selectedEntityId !== firstEntityId) {
        // Entity changed - clear selections
        this.selectedLists = [];
        this.vennData = null;
        this.selectedRegion = null;
        this.lastOperationResult = null;
        this.previewRecordsDisplay = [];
  filterAvailableLists() {
    const selectedIds = new Set(this.selectedLists.map(s => s.list.ID));
    let filtered = this.availableLists.filter(l => !selectedIds.has(l.ID));
    // Apply entity filter from dropdown
    if (this.selectedEntityId) {
      filtered = filtered.filter(l => l.EntityID === this.selectedEntityId);
    // If we have selected lists, restrict to same entity
      this.entityIdFromSelectedLists = this.selectedLists[0].list.EntityID;
      filtered = filtered.filter(l => l.EntityID === this.entityIdFromSelectedLists);
    if (this.listSearchTerm) {
      const term = this.listSearchTerm.toLowerCase();
        l.Name.toLowerCase().includes(term) ||
        (l.Entity && l.Entity.toLowerCase().includes(term))
    this.filteredAvailableLists = filtered.slice(0, 10);
  addList(list: MJListEntity) {
    const color = this.setOperationsService.getColorForIndex(this.selectedLists.length);
    this.selectedLists.push({
      entityName: list.Entity || 'Unknown',
      color
    this.listSearchTerm = '';
    this.showListDropdown = false;
    this.recalculateVenn();
  removeList(index: number) {
    this.selectedLists.splice(index, 1);
    // Reassign colors
    this.selectedLists.forEach((item, i) => {
      item.color = this.setOperationsService.getColorForIndex(i);
  async recalculateVenn() {
    if (this.selectedLists.length === 0) {
    this.isCalculating = true;
      const lists = this.selectedLists.map(s => s.list);
      this.vennData = await this.setOperationsService.calculateVennData(lists);
      console.error('Error calculating Venn data:', error);
      this.isCalculating = false;
  onRegionClick(event: VennRegionClickEvent) {
    this.selectedRegion = event.intersection;
    this.previewRecords = event.recordIds.slice(0, 10);
    this.loadPreviewRecords(event.recordIds.slice(0, 10));
   * Load preview records with meaningful display fields
  private async loadPreviewRecords(recordIds: string[]): Promise<void> {
    if (recordIds.length === 0 || this.selectedLists.length === 0) {
    this.loadingPreview = true;
      const entityId = this.selectedLists[0].list.EntityID;
      const entityInfo = md.Entities.find(e => e.ID === entityId);
        // Fallback to showing just IDs
        this.previewRecordsDisplay = recordIds.map(id => ({
          displayName: id,
          entityName: this.selectedLists[0].entityName
      this.currentEntityInfo = entityInfo;
      // Find the best display fields
      const displayFields = this.getDisplayFields(entityInfo);
      // Build filter for the records
      const primaryKeyFields = entityInfo.PrimaryKeys;
      const primaryKeyField = primaryKeyFields.length > 0 ? primaryKeyFields[0].Name : 'ID';
      const recordIdFilter = recordIds.map(id => `'${id}'`).join(',');
        ExtraFilter: `${primaryKeyField} IN (${recordIdFilter})`,
        Fields: [primaryKeyField, ...displayFields],
        this.previewRecordsDisplay = result.Results.map(record => {
          const id = String(record[primaryKeyField] || '');
          const displayName = this.getDisplayValue(record, displayFields[0]) || id;
          const secondaryInfo = displayFields.length > 1
            ? this.getDisplayValue(record, displayFields[1])
            secondaryInfo,
            entityName: entityInfo.Name
        // Fallback
      console.error('Error loading preview records:', error);
        entityName: this.selectedLists[0]?.entityName || 'Unknown'
      this.loadingPreview = false;
   * Get the best display fields for an entity based on metadata
  private getDisplayFields(entityInfo: EntityInfo): string[] {
    const fields: string[] = [];
    // First priority: NameField (IsNameField = true)
    const nameField = entityInfo.Fields.find(f => f.IsNameField);
      fields.push(nameField.Name);
    // Second priority: DefaultInView fields (excluding primary key unless it's also the name field)
    const defaultViewFields = entityInfo.Fields
      .filter(f => f.DefaultInView && !f.IsPrimaryKey && f.Name !== nameField?.Name)
      .slice(0, 2); // Take up to 2 more fields
    for (const field of defaultViewFields) {
      if (!fields.includes(field.Name)) {
        fields.push(field.Name);
    // If we still don't have any fields, use common field names
    if (fields.length === 0) {
      const commonNames = ['Name', 'Title', 'Subject', 'Description', 'Email', 'FirstName'];
      for (const name of commonNames) {
        const field = entityInfo.Fields.find(f => f.Name === name);
        if (field) {
    // If still nothing, include primary key
    if (fields.length === 0 && entityInfo.PrimaryKeys.length > 0) {
      fields.push(entityInfo.PrimaryKeys[0].Name);
    return fields.slice(0, 3); // Max 3 fields
   * Get a display value from a record
  private getDisplayValue(record: Record<string, unknown>, fieldName: string): string | undefined {
    if (value === null || value === undefined) return undefined;
    if (value instanceof Date) {
      return value.toLocaleDateString();
   * Open a record in the entity viewer
  openRecord(record: PreviewRecord): void {
    if (!this.currentEntityInfo) {
      // Try to get entity info
      const entityId = this.selectedLists[0]?.list.EntityID;
        this.currentEntityInfo = md.Entities.find(e => e.ID === entityId) || null;
    if (this.currentEntityInfo) {
      // Create composite key for navigation
      const primaryKeyField = this.currentEntityInfo.PrimaryKeys.length > 0
        ? this.currentEntityInfo.PrimaryKeys[0].Name
        : 'ID';
      const compositeKey = new CompositeKey([{ FieldName: primaryKeyField, Value: record.id }]);
      SharedService.Instance.OpenEntityRecord(this.currentEntityInfo.Name, compositeKey);
      this.notificationService.CreateSimpleNotification('Unable to open record', 'error', 3000);
  async performOperation(operation: SetOperation) {
    if (this.selectedLists.length < 2) return;
      const listIds = this.selectedLists.map(s => s.list.ID);
      this.lastOperationResult = await this.setOperationsService.performOperation(operation, listIds);
      this.previewRecords = this.lastOperationResult.resultRecordIds.slice(0, 10);
      console.error('Error performing operation:', error);
  getOperationLabel(operation: SetOperation): string {
      case 'union': return 'Union (All Records)';
      case 'intersection': return 'Intersection (Common Records)';
      case 'difference': return 'Difference';
      case 'symmetric_difference': return 'Unique to Each List';
      case 'complement': return 'Complement';
      default: return operation;
  createListFromSelection() {
    if (!this.selectedRegion || this.selectedRegion.size === 0) return;
    this.recordsToAdd = [...this.selectedRegion.recordIds];
    this.newListDescription = `Created from: ${this.selectedRegion.label}`;
  createListFromResult() {
    if (!this.lastOperationResult || this.lastOperationResult.resultCount === 0) return;
    this.recordsToAdd = [...this.lastOperationResult.resultRecordIds];
    this.newListDescription = `Created from: ${this.getOperationLabel(this.lastOperationResult.operation)}`;
  cancelCreateDialog() {
    this.recordsToAdd = [];
  async confirmCreateList() {
    if (!this.newListName || this.recordsToAdd.length === 0) return;
    // Get entity ID from selected lists
    if (this.selectedLists.length === 0) return;
      const list = await md.GetEntityObject<MJListEntity>('MJ: Lists', md.CurrentUser);
      list.EntityID = entityId;
        this.notificationService.CreateSimpleNotification('Failed to create list', 'error', 4000);
      // Add records to the list using transaction group
      const tg = await md.CreateTransactionGroup();
      for (const recordId of this.recordsToAdd) {
        const detail = await md.GetEntityObject<MJListDetailEntity>('MJ: List Details', md.CurrentUser);
        detail.ListID = list.ID;
        detail.RecordID = recordId;
        detail.Sequence = 0;
        detail.TransactionGroup = tg;
        await detail.Save();
          `Created "${this.newListName}" with ${this.recordsToAdd.length} items`,
          `Created list but failed to add some records`,
      this.cancelCreateDialog();
      // Refresh available lists
      console.error('Error creating list:', error);
      this.notificationService.CreateSimpleNotification('Error creating list', 'error', 4000);
  addToExistingList() {
    this.openAddToListDialog();
  addResultToExistingList() {
   * Open the Add to Existing List dialog
  private openAddToListDialog(): void {
    this.showAddToListDialog = true;
    this.addToListSearchTerm = '';
    this.selectedTargetListId = null;
    this.filterAddToListOptions();
   * Filter available lists for add-to-list dialog
  filterAddToListOptions(): void {
    // Get entity ID from selected lists to filter to same entity type
      this.filteredAddToListOptions = [];
    // Filter to same entity, exclude already selected lists
    let filtered = this.availableLists.filter(l =>
      l.EntityID === entityId && !selectedIds.has(l.ID)
    if (this.addToListSearchTerm) {
      const term = this.addToListSearchTerm.toLowerCase();
        l.Name.toLowerCase().includes(term)
    this.filteredAddToListOptions = filtered;
   * Select a target list for adding records
  selectTargetList(listId: string): void {
    this.selectedTargetListId = listId;
   * Cancel the add to list dialog
  cancelAddToListDialog(): void {
    this.showAddToListDialog = false;
   * Confirm adding records to selected list
  async confirmAddToList(): Promise<void> {
    if (!this.selectedTargetListId || this.recordsToAdd.length === 0) return;
        detail.ListID = this.selectedTargetListId;
        const targetList = this.availableLists.find(l => l.ID === this.selectedTargetListId);
          `Added ${this.recordsToAdd.length} records to "${targetList?.Name || 'list'}"`,
          'Failed to add some records',
      this.cancelAddToListDialog();
      console.error('Error adding to list:', error);
      this.notificationService.CreateSimpleNotification('Error adding records to list', 'error', 4000);
  exportToExcel() {
    // TODO: Implement Excel export
    this.notificationService.CreateSimpleNotification('Export to Excel - coming soon', 'info', 2000);
    return 'Operations';
    return 'fa-solid fa-diagram-project';
   * Clear all selections and reset state
  clearAllSelections(): void {
   * Save state to User Settings (debounced to avoid excessive writes)
  private saveState(): void {
    // Don't save during initial load
    if (this.isLoadingSettings) return;
    // Debounce the server save
    this.saveSettingsTimeout = setTimeout(() => {
      this.saveStateToServer();
    }, 1000); // 1 second debounce
   * Save state to User Settings entity on server
  private async saveStateToServer(): Promise<void> {
      const stateToSave = {
        entityId: this.selectedEntityId,
        listIds: this.selectedLists.map(s => s.list.ID)
      let setting = engine.UserSettings.find(s => s.Setting === this.USER_SETTING_KEY);
        // Create new setting
        setting.Setting = this.USER_SETTING_KEY;
      setting.Value = JSON.stringify(stateToSave);
      console.warn('Failed to save operations state to User Settings:', error);
   * Load saved state from User Settings
  private async loadSavedState(): Promise<void> {
    this.isLoadingSettings = true;
        this.isLoadingSettings = false;
      // Load from cached User Settings
      const setting = engine.UserSettings.find(s => s.Setting === this.USER_SETTING_KEY);
        const state = JSON.parse(setting.Value) as { entityId?: string; listIds?: string[] };
        // Restore entity filter
        if (state.entityId) {
          this.selectedEntityId = state.entityId;
        // Restore selected lists
        if (state.listIds && state.listIds.length > 0) {
          for (const listId of state.listIds) {
            const list = this.availableLists.find(l => l.ID === listId);
          // If we restored lists, recalculate venn
            await this.recalculateVenn();
      console.warn('Failed to load operations state from User Settings:', error);
