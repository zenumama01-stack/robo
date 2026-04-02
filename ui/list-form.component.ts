import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, inject } from '@angular/core';
import { Subject, debounceTime } from 'rxjs';
import { MJListFormComponent } from '../../generated/Entities/MJList/mjlist.form.component';
import { MJListEntity, MJListDetailEntity, ListDetailEntityExtended, MJListCategoryEntity, UserViewEntityExtended } from '@memberjunction/core-entities';
import { Metadata, RunView, RunViewResult, EntityInfo, LogError, LogStatus } from '@memberjunction/core';
import { ListShareDialogConfig, ListShareDialogResult } from '@memberjunction/ng-list-management';
export type ListSection = 'overview' | 'items' | 'sharing' | 'activity' | 'settings';
export interface ListItemViewModel {
    detail: MJListDetailEntity;
    recordName: string;
export interface ListStats {
    itemCount: number;
    invitationCount: number;
    lastUpdated: Date | null;
 * Represents a record that can be added to a list
export interface AddableRecord {
    isInList: boolean;
    isSelected: boolean;
 * World-class List form component that provides a rich exploration experience
 * for managing lists in the MemberJunction system.
 * - Overview with visual stats and entity context
 * - Items grid with inline record navigation
 * - Sharing management (coming soon)
 * - Activity history
 * - Settings and configuration
@RegisterClass(BaseFormComponent, 'MJ: Lists')
    selector: 'mj-list-form-extended',
    templateUrl: './list-form.component.html',
    styleUrls: ['./list-form.component.css', '../../../shared/form-styles.css'],
export class ListFormComponentExtended extends MJListFormComponent implements OnInit, OnDestroy {
    public override record!: MJListEntity;
    public activeSection: ListSection = 'overview';
    public navItems = [
        { id: 'overview' as ListSection, icon: 'fa-solid fa-house', label: 'Overview' },
        { id: 'items' as ListSection, icon: 'fa-solid fa-list', label: 'Items', badge: 0 },
        { id: 'sharing' as ListSection, icon: 'fa-solid fa-share-nodes', label: 'Sharing', badge: 0, disabled: false },
        { id: 'activity' as ListSection, icon: 'fa-solid fa-clock-rotate-left', label: 'Activity' },
        { id: 'settings' as ListSection, icon: 'fa-solid fa-gear', label: 'Settings' }
    public listItems: ListItemViewModel[] = [];
    public categories: MJListCategoryEntity[] = [];
    public entityInfo: EntityInfo | null = null;
    public stats: ListStats = {
        itemCount: 0,
        shareCount: 0,
        invitationCount: 0,
        lastUpdated: null
    public isLoadingItems = false;
    public isLoadingStats = false;
    // Items section
    public itemSearchTerm = '';
    public selectedItems = new Set<string>();
    public isSelectAllChecked = false;
    // Edit state
    public isEditingName = false;
    public isEditingDescription = false;
    public editingName = '';
    public editingDescription = '';
    // Add Records dialog
    public showAddRecordsDialog = false;
    public addDialogLoading = false;
    public addDialogSaving = false;
    public addableRecords: AddableRecord[] = [];
    public addRecordsSearchFilter = '';
    public existingListDetailIds = new Set<string>();
    public addProgress = 0;
    public addTotal = 0;
    private searchSubject = new Subject<string>();
    // Add From View dialog
    public showAddFromViewDialog = false;
    public showAddFromViewLoader = false;
    public userViews: UserViewEntityExtended[] | null = null;
    public userViewsToAdd: UserViewEntityExtended[] = [];
    public addFromViewProgress = 0;
    public addFromViewTotal = 0;
    public fetchingRecordsToSave = false;
    // Share dialog
    public showShareDialog = false;
    public shareDialogConfig: ListShareDialogConfig | null = null;
    private metadata = new Metadata();
        // Set up search debounce
        this.searchSubject
            .pipe(debounceTime(300))
            .subscribe((searchText) => this.searchRecords(searchText));
        await this.loadExplorerData();
    // Helper to show notifications using SharedService's deprecated method
    private showNotification(message: string, style: 'success' | 'error' | 'info' = 'info', duration: number = 3000): void {
        this.sharedService.CreateSimpleNotification(message, style, duration);
    private async loadExplorerData(): Promise<void> {
            // Load entity info for context
            if (this.record?.EntityID) {
                this.entityInfo = this.metadata.Entities.find(e => e.ID === this.record.EntityID) || null;
            // Load categories for dropdown
            await this.loadCategories();
            // Load items and stats in parallel
                this.loadItems(),
                this.loadStats()
            console.error('Error loading list data:', error);
            this.explorerError = 'Failed to load list data';
    private async loadCategories(): Promise<void> {
        const result = await rv.RunView<MJListCategoryEntity>({
            EntityName: 'MJ: List Categories',
            this.categories = result.Results;
    private async loadItems(): Promise<void> {
        if (!this.record?.IsSaved) return;
        this.isLoadingItems = true;
            const result = await rv.RunView<MJListDetailEntity>({
                ExtraFilter: `ListID = '${this.record.ID}'`,
                this.listItems = result.Results.map(detail => ({
                    detail,
                    recordName: detail.RecordID || 'Loading...',
                    isLoading: true
                // Load record names asynchronously
                this.loadRecordNames();
            console.error('Error loading list items:', error);
            this.isLoadingItems = false;
    private async loadRecordNames(): Promise<void> {
        if (!this.entityInfo) return;
        // Get the name field - NameField is EntityFieldInfo or undefined
        const nameFieldInfo = this.entityInfo.NameField;
        const nameFieldName = nameFieldInfo ? nameFieldInfo.Name : 'ID';
        for (const item of this.listItems) {
                    EntityName: this.entityInfo.Name,
                    ExtraFilter: `ID = '${item.detail.RecordID}'`,
                    Fields: [nameFieldName],
                    MaxRows: 1
                if (result.Success && result.Results.length > 0) {
                    const record = result.Results[0] as Record<string, string>;
                    item.recordName = record[nameFieldName] || item.detail.RecordID || '';
                item.recordName = item.detail.RecordID || 'Unknown';
                item.isLoading = false;
    private async loadStats(): Promise<void> {
        this.isLoadingStats = true;
            const [itemsResult, sharesResult, invitationsResult] = await rv.RunViews([
                    EntityName: 'MJ: List Shares',
                    EntityName: 'MJ: List Invitations',
                itemCount: itemsResult.Success ? itemsResult.TotalRowCount : 0,
                shareCount: sharesResult.Success ? sharesResult.TotalRowCount : 0,
                invitationCount: invitationsResult.Success ? invitationsResult.TotalRowCount : 0,
                lastUpdated: this.record.__mj_UpdatedAt
            console.error('Error loading stats:', error);
            this.isLoadingStats = false;
                case 'items':
                    return { ...item, badge: this.stats.itemCount };
                case 'sharing':
                    return { ...item, badge: this.stats.shareCount + this.stats.invitationCount };
    // === Navigation ===
    public setActiveSection(section: ListSection): void {
        const navItem = this.navItems.find(n => n.id === section);
        if (navItem?.disabled) return;
    // === Items Management ===
    public get filteredItems(): ListItemViewModel[] {
        if (!this.itemSearchTerm) return this.listItems;
        const term = this.itemSearchTerm.toLowerCase();
        return this.listItems.filter(item =>
            item.recordName.toLowerCase().includes(term) ||
            item.detail.RecordID?.toLowerCase().includes(term)
    public toggleItemSelection(item: ListItemViewModel): void {
        const id = item.detail.ID;
        if (this.selectedItems.has(id)) {
            this.selectedItems.delete(id);
            this.selectedItems.add(id);
        this.updateSelectAllState();
    public toggleSelectAll(): void {
        if (this.isSelectAllChecked) {
            this.selectedItems.clear();
            for (const item of this.filteredItems) {
                this.selectedItems.add(item.detail.ID);
        this.isSelectAllChecked = !this.isSelectAllChecked;
    private updateSelectAllState(): void {
        this.isSelectAllChecked = this.filteredItems.length > 0 &&
            this.filteredItems.every(item => this.selectedItems.has(item.detail.ID));
    public async removeSelectedItems(): Promise<void> {
        if (this.selectedItems.size === 0) return;
        const count = this.selectedItems.size;
        const confirmMessage = `Remove ${count} item${count > 1 ? 's' : ''} from this list?`;
        if (!confirm(confirmMessage)) return;
            for (const id of this.selectedItems) {
                const item = this.listItems.find(i => i.detail.ID === id);
                if (item) {
                    await item.detail.Delete();
            this.showNotification(
                `Removed ${count} item${count > 1 ? 's' : ''} from list`,
            await this.loadItems();
            await this.loadStats();
            console.error('Error removing items:', error);
                'Error removing items from list',
    public openRecord(item: ListItemViewModel): void {
        if (!this.entityInfo || !item.detail.RecordID) return;
        // Use SharedService to open the record
    // === Inline Editing ===
    public startEditingName(): void {
        this.editingName = this.record.Name;
        this.isEditingName = true;
    public async saveNameEdit(): Promise<void> {
        if (!this.editingName.trim()) {
            this.cancelNameEdit();
        this.record.Name = this.editingName.trim();
        const saved = await this.record.Save();
            this.showNotification('Name updated', 'success', 2000);
            this.showNotification('Failed to update name', 'error', 3000);
        this.isEditingName = false;
    public cancelNameEdit(): void {
    public startEditingDescription(): void {
        this.editingDescription = this.record.Description || '';
        this.isEditingDescription = true;
    public async saveDescriptionEdit(): Promise<void> {
        this.record.Description = this.editingDescription.trim() || null;
            this.showNotification('Description updated', 'success', 2000);
            this.showNotification('Failed to update description', 'error', 3000);
        this.isEditingDescription = false;
    public cancelDescriptionEdit(): void {
    // === Helpers ===
        return this.entityInfo?.DisplayName || this.entityInfo?.Name || this.record?.Entity || 'Unknown';
        return this.entityInfo?.Icon || 'fa-solid fa-table';
    public get categoryName(): string {
        if (!this.record?.CategoryID) return 'Uncategorized';
        const category = this.categories.find(c => c.ID === this.record.CategoryID);
        return category?.Name || 'Unknown';
    public get formattedItemCount(): string {
        return this.stats.itemCount.toLocaleString();
    public get formattedLastUpdated(): string {
        if (!this.stats.lastUpdated) return 'Never';
        const date = new Date(this.stats.lastUpdated);
        const diffMs = now.getTime() - date.getTime();
        const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));
        if (diffDays === 0) return 'Today';
        if (diffDays === 1) return 'Yesterday';
        if (diffDays < 7) return `${diffDays} days ago`;
        return date.toLocaleDateString();
    public getOwnerName(): string {
        return this.record?.User || 'Unknown';
    public isCurrentUserOwner(): boolean {
        return this.record?.UserID === this.metadata.CurrentUser?.ID;
    public async onCategoryChange(categoryId: string | null): Promise<void> {
        this.record.CategoryID = categoryId;
            this.showNotification('Category updated', 'success', 2000);
            this.showNotification('Failed to update category', 'error', 3000);
    public async refreshItems(): Promise<void> {
    // Add Records Dialog
    public async openAddRecordsDialog(): Promise<void> {
        this.showAddRecordsDialog = true;
        this.addableRecords = [];
        this.addRecordsSearchFilter = '';
        this.addDialogLoading = true;
        this.addDialogSaving = false;
        // Load existing list detail IDs to mark which records are already in the list
        await this.loadExistingListDetailIds();
        this.addDialogLoading = false;
    public closeAddRecordsDialog(): void {
        this.showAddRecordsDialog = false;
        this.existingListDetailIds.clear();
        this.addProgress = 0;
        this.addTotal = 0;
    private async loadExistingListDetailIds(): Promise<void> {
        if (!this.record) return;
        const result = await rv.RunView<{ RecordID: string }>({
            Fields: ['RecordID'],
        }, this.metadata.CurrentUser);
            this.existingListDetailIds = new Set(result.Results.map(r => r.RecordID));
    public onAddRecordsSearchChange(value: string): void {
        this.addRecordsSearchFilter = value;
        this.searchSubject.next(value);
    private async searchRecords(searchText: string): Promise<void> {
        if (!this.record || !searchText || searchText.length < 2) {
        const sourceEntityInfo = this.metadata.EntityByID(this.record.EntityID);
        if (!sourceEntityInfo) {
        const nameField = sourceEntityInfo.Fields.find(field => field.IsNameField);
        const pkField = sourceEntityInfo.FirstPrimaryKey?.Name || 'ID';
        let filter: string | undefined;
        if (nameField) {
            filter = `${nameField.Name} LIKE '%${searchText}%'`;
        const result: RunViewResult = await rv.RunView({
            EntityName: this.record.Entity,
            MaxRows: 100,
            this.addableRecords = result.Results.map((record: Record<string, unknown>) => {
                const recordId = String(record[pkField]);
                    ID: recordId,
                    Name: nameField ? String(record[nameField.Name]) : recordId,
                    isInList: this.existingListDetailIds.has(recordId),
                    isSelected: false
    public toggleRecordSelection(record: AddableRecord): void {
        if (record.isInList) return; // Can't select records already in list
        record.isSelected = !record.isSelected;
    public get selectedAddableRecords(): AddableRecord[] {
        return this.addableRecords.filter(r => r.isSelected);
    public selectAllAddable(): void {
        this.addableRecords.forEach(r => {
            if (!r.isInList) r.isSelected = true;
    public deselectAllAddable(): void {
        this.addableRecords.forEach(r => r.isSelected = false);
    public async confirmAddRecords(): Promise<void> {
        const recordsToAdd = this.selectedAddableRecords;
        if (recordsToAdd.length === 0 || !this.record) return;
        this.addDialogSaving = true;
        this.addTotal = recordsToAdd.length;
        // Use transaction group for bulk insert
        const tg = await this.metadata.CreateTransactionGroup();
        for (const record of recordsToAdd) {
            const listDetail = await this.metadata.GetEntityObject<ListDetailEntityExtended>('MJ: List Details');
            listDetail.ListID = this.record.ID;
            listDetail.RecordID = record.ID;
            listDetail.TransactionGroup = tg;
            await listDetail.Save();
        const success = await tg.Submit();
            this.addProgress = this.addTotal;
                `Added ${recordsToAdd.length} record${recordsToAdd.length !== 1 ? 's' : ''} to list`,
                2500
            this.closeAddRecordsDialog();
            await this.refreshItems();
            LogError('Error adding records to list');
            this.showNotification('Failed to add some records', 'error', 2500);
    // Add From View Dialog
    public async openAddFromViewDialog(): Promise<void> {
        this.showAddFromViewDialog = true;
        this.userViewsToAdd = [];
        if (!this.userViews) {
            await this.loadEntityViews();
    public closeAddFromViewDialog(): void {
        this.showAddFromViewDialog = false;
        this.showAddFromViewLoader = false;
        this.addFromViewProgress = 0;
        this.addFromViewTotal = 0;
    private async loadEntityViews(): Promise<void> {
        if (!this.record || !this.record.Entity) return;
        this.showAddFromViewLoader = true;
        const runViewResult = await rv.RunView<UserViewEntityExtended>({
            EntityName: 'MJ: User Views',
            ExtraFilter: `UserID = '${this.metadata.CurrentUser.ID}' AND EntityID = '${this.record.EntityID}'`,
            LogError(`Error loading User Views for entity ${this.record.Entity}`);
            this.userViews = runViewResult.Results;
    public toggleViewSelection(view: UserViewEntityExtended): void {
        const index = this.userViewsToAdd.findIndex(v => v.ID === view.ID);
            this.userViewsToAdd.splice(index, 1);
            this.userViewsToAdd.push(view);
    public isViewSelected(view: UserViewEntityExtended): boolean {
        return this.userViewsToAdd.some(v => v.ID === view.ID);
    public async confirmAddFromView(): Promise<void> {
        if (!this.record || this.userViewsToAdd.length === 0) return;
        this.fetchingRecordsToSave = true;
        // Collect all unique record IDs from selected views
        const recordIdSet = new Set<string>();
        for (const userView of this.userViewsToAdd) {
                ViewEntity: userView,
            if (runViewResult.Success) {
                const records = runViewResult.Results as Array<{ ID: string }>;
                records.forEach(r => recordIdSet.add(r.ID));
        // Filter out records already in the list
        const recordsToAdd = [...recordIdSet].filter(id => !this.existingListDetailIds.has(id));
        this.addFromViewTotal = recordsToAdd.length;
        this.fetchingRecordsToSave = false;
        if (recordsToAdd.length === 0) {
            this.showNotification('All records already in list', 'info', 2500);
        LogStatus(`Adding ${recordsToAdd.length} records to list`);
        for (const recordID of recordsToAdd) {
            this.addFromViewProgress = this.addFromViewTotal;
            this.closeAddFromViewDialog();
            LogError('Error adding records from view to list');
    // Share Dialog
    public openShareDialog(): void {
        this.shareDialogConfig = {
            listId: this.record.ID,
            listName: this.record.Name,
            currentUserId: this.metadata.CurrentUser.ID,
            isOwner: this.isCurrentUserOwner()
        this.showShareDialog = true;
    public onShareDialogComplete(result: ListShareDialogResult): void {
        this.showShareDialog = false;
        this.shareDialogConfig = null;
        if (result.action === 'apply') {
            // Refresh stats to update share counts
            this.loadStats().then(() => {
    public onShareDialogCancel(): void {
