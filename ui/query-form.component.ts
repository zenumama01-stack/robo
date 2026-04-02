import { Component, OnInit, OnDestroy, ViewChild, ChangeDetectorRef, AfterViewInit } from '@angular/core';
import { MJQueryEntity, MJQueryParameterEntity, MJQueryCategoryEntity, MJQueryFieldEntity, MJQueryEntityEntity, MJQueryPermissionEntity } from '@memberjunction/core-entities';
import { MJQueryFormComponent } from '../../generated/Entities/MJQuery/mjquery.form.component';
import { Metadata, RunView, RUN_QUERY_SQL_FILTERS } from '@memberjunction/core';
import { CodeEditorComponent } from '@memberjunction/ng-code-editor';
interface CategoryTreeNode {
    items?: CategoryTreeNode[];
@RegisterClass(BaseFormComponent, 'MJ: Queries')
    selector: 'mj-query-form',
    templateUrl: './query-form.component.html',
    styleUrls: ['../../../shared/form-styles.css', './query-form.component.css']
export class QueryFormExtendedComponent extends MJQueryFormComponent implements OnInit, OnDestroy, AfterViewInit {
    public record!: MJQueryEntity;
    public queryParameters: MJQueryParameterEntity[] = [];
    public queryFields: MJQueryFieldEntity[] = [];
    public queryEntities: MJQueryEntityEntity[] = [];
    public queryPermissions: MJQueryPermissionEntity[] = [];
    public isLoadingParameters = false;
    public isLoadingFields = false;
    public isLoadingEntities = false;
    public isLoadingPermissions = false;
    public showFiltersHelp = false;
    public showRunDialog = false;
    public showCategoryDialog = false;
    // Expansion panel states
    public sqlPanelExpanded = true;
    public parametersPanelExpanded = false;
    public fieldsPanelExpanded = false;
    public entitiesPanelExpanded = false;
    public detailsPanelExpanded = false;
    public permissionsPanelExpanded = false;
    // Category data
    public categoryOptions: Array<{text: string, value: string}> = [
        { text: 'Select Category...', value: '' }
    public categoryTreeData: CategoryTreeNode[] = [];
    // Status options
        { text: 'Approved', value: 'Approved' },
        { text: 'Rejected', value: 'Rejected' },
        { text: 'Expired', value: 'Expired' }
    @ViewChild('sqlEditor') sqlEditor: CodeEditorComponent | null = null;
    // SQL Filters for help display
    public sqlFilters = RUN_QUERY_SQL_FILTERS;
    private isUpdatingEditorValue = false;
        // Load categories first to ensure they're available for the dropdown
        // Then load other data in parallel
            this.loadQueryParameters(),
            this.loadQueryFields(),
            this.loadQueryEntities(),
            this.loadQueryPermissions()
        // Ensure form is properly initialized after all data is loaded
        super.ngAfterViewInit();
        this.sqlEditor?.setEditable(this.EditMode);
        // Set initial SQL value in the editor
        this.updateEditorValue();
        // Ensure all form controls are properly initialized with data
            // Force Angular to update all bindings
            // If in edit mode, trigger another update to ensure Kendo components are initialized
            if (this.EditMode) {
    override EndEditMode(): void {
        super.EndEditMode();
        this.sqlEditor?.setEditable(false);
    override StartEditMode(): void {
        this.sqlEditor?.setEditable(true);
        // Force change detection after a brief delay to ensure form controls are initialized
    override CancelEdit(): void {
        this.updateEditorValue(); // Reset editor value to record SQL
        this.updateUnsavedChangesFlag(); // Reset unsaved changes flag
    private updateEditorValue() {
        if (!this.sqlEditor || this.isUpdatingEditorValue) {
        // Use setTimeout to avoid ExpressionChangedAfterItHasBeenCheckedError
            if (!this.sqlEditor) {
            this.isUpdatingEditorValue = true;
            const sqlValue = this.record?.SQL || '';
            // Use the setValue method from mj-code-editor component
            this.sqlEditor.setValue(sqlValue);
            this.isUpdatingEditorValue = false;
    public isFormReadOnly(): boolean {
        return !this.EditMode;
    async loadQueryParameters() {
        if (this.record && this.record.ID) {
            this.isLoadingParameters = true;
                const results = await rv.RunView<MJQueryParameterEntity>({
                    EntityName: 'MJ: Query Parameters',
                    ExtraFilter: `QueryID='${this.record.ID}'`,
                if (results.Success) {
                    this.queryParameters = results.Results || [];
                console.error('Error loading query parameters:', error);
                this.isLoadingParameters = false;
    async loadQueryFields() {
            this.isLoadingFields = true;
                const results = await rv.RunView<MJQueryFieldEntity>({
                    EntityName: 'MJ: Query Fields',
                    this.queryFields = results.Results || [];
                console.error('Error loading query fields:', error);
                this.isLoadingFields = false;
    async loadQueryEntities() {
            this.isLoadingEntities = true;
                const results = await rv.RunView<MJQueryEntityEntity>({
                    EntityName: 'MJ: Query Entities',
                    OrderBy: 'Entity ASC',
                    this.queryEntities = results.Results || [];
                    console.log('Loaded query entities:', this.queryEntities);
                console.error('Error loading query entities:', error);
                this.isLoadingEntities = false;
    async loadQueryPermissions() {
            this.isLoadingPermissions = true;
                const results = await rv.RunView<MJQueryPermissionEntity>({
                    EntityName: 'MJ: Query Permissions',
                    OrderBy: 'Role ASC',
                    this.queryPermissions = results.Results || [];
                console.error('Error loading query permissions:', error);
                this.isLoadingPermissions = false;
            const results = await rv.RunView<MJQueryCategoryEntity>({
            if (results.Success && results.Results) {
                this.categories = results.Results;
                // Build flat options for legacy compatibility
                this.categoryOptions = [
                    { text: 'Select Category...', value: '' },
                    ...this.categories.map(cat => ({
                        text: cat.Name,
                        value: cat.ID
                // Build tree data after options are set
                this.categoryTreeData = this.buildCategoryTree(this.categories);
                // Trigger change detection to update the view
            this.categoryOptions = [{ text: 'Select Category...', value: '' }];
            this.categoryTreeData = [];
    private buildCategoryTree(categories: MJQueryCategoryEntity[]): CategoryTreeNode[] {
        const categoryMap = new Map<string, CategoryTreeNode>();
        const rootCategories: CategoryTreeNode[] = [];
                if (!parent.items) parent.items = [];
                parent.items.push(node);
        const sortNodes = (nodes: CategoryTreeNode[]) => {
                    sortNodes(node.items);
    getCategoryPath(): string {
        if (!this.record.CategoryID) return '';
        const findPath = (categoryId: string): string[] => {
            const category = this.categories.find(c => c.ID === categoryId);
            if (!category) return [];
                return [...findPath(category.ParentID), category.Name];
            return [category.Name];
        return findPath(this.record.CategoryID).join(' / ');
    async onCategoryChange(value: string) {
        // If it's a new category (string but not in existing options)
        if (value && !this.categoryOptions.find(opt => opt.value === value)) {
            // Check for duplicate category names (case-insensitive, trimmed)
            if (this.isDuplicateCategory(value)) {
                const existingCategory = this.categoryOptions.find(option => 
                    option.text && option.text.trim().toLowerCase() === value.trim().toLowerCase()
                if (existingCategory) {
                    // Use the existing category instead
                    this.record.CategoryID = existingCategory.value;
                        `Category "${existingCategory.text}" already exists. Using existing category.`, 
                // Create new category with trimmed name
                newCategory.Name = value.trim();
                    // Add to options and set the ID
                    this.categoryOptions.push({
                        text: newCategory.Name,
                        value: newCategory.ID
                    this.record.CategoryID = newCategory.ID;
                        `New category "${newCategory.Name}" created successfully.`, 
                        `Failed to create new category. ${newCategory.LatestResult?.Message || ''}`, 
                console.error('Error creating new category:', error);
                    'Error creating new category. Please try again.', 
    private isDuplicateCategory(categoryName: string): boolean {
        const normalizedName = categoryName?.trim().toLowerCase();
        return this.categoryOptions.some(option => 
            option.text && option.text.trim().toLowerCase() === normalizedName
     * Updates the hasUnsavedChanges flag based on entity dirty states
    private updateUnsavedChangesFlag() {
        this.hasUnsavedChanges = this.queryParameters.some(param => param.Dirty) || 
                                this.record?.Dirty || false;
    toggleFiltersHelp() {
        this.showFiltersHelp = !this.showFiltersHelp;
     * Run the query with parameter dialog
    async runQuery() {
        if (!this.record?.IsSaved) {
                'Please save the query before running it.', 
        // Save any unsaved changes first
        if (this.hasUnsavedChanges) {
            const saveResult = await this.SaveRecord(false); // Don't exit edit mode
                    'Failed to save query changes.', 
        // Reload parameters in case they were updated
        await this.loadQueryParameters();
        // Show the run dialog
        this.showRunDialog = true;
     * Handle run dialog close
    onRunDialogClose() {
        this.showRunDialog = false;
     * Add a new parameter
    async addParameter() {
            const newParam = await md.GetEntityObject<MJQueryParameterEntity>('MJ: Query Parameters');
            newParam.QueryID = this.record.ID;
            newParam.Name = `param${this.queryParameters.length + 1}`;
            newParam.Type = 'string';
            const saved = await newParam.Save();
                this.queryParameters.push(newParam);
                this.updateUnsavedChangesFlag();
                    'Parameter added successfully',
                    'Failed to add parameter',
            console.error('Error adding parameter:', error);
                'Error adding parameter',
     * Edit a parameter
    async editParameter(param: MJQueryParameterEntity) {
        // TODO: Show parameter edit dialog
        console.log('Edit parameter:', param);
     * Delete a parameter
    async deleteParameter(param: MJQueryParameterEntity) {
        if (!confirm(`Are you sure you want to delete parameter "${param.Name}"?`)) {
                const index = this.queryParameters.indexOf(param);
                    this.queryParameters.splice(index, 1);
                    'Parameter deleted successfully',
                    'Failed to delete parameter',
            console.error('Error deleting parameter:', error);
                'Error deleting parameter',
     * Handle category creation from dialog
    async onCategoryCreated(newCategory: MJQueryCategoryEntity) {
        // Reload categories to include the new one
        // Set the new category as selected
     * Format date for display
        if (!date) return '-';
        return d.toLocaleDateString() + ' ' + d.toLocaleTimeString();
    async SaveRecord(StopEditModeAfterSave: boolean = true): Promise<boolean> {
        // Handle category creation before saving query
        if (this.record.CategoryID && !this.categoryOptions.find(opt => opt.value === this.record.CategoryID)) {
            if (this.isDuplicateCategory(this.record.CategoryID)) {
                    option.text && option.text.trim().toLowerCase() === this.record.CategoryID?.trim().toLowerCase()
                    newCategory.Name = this.record.CategoryID.trim();
                        console.error('Failed to create new category');
                    console.error('Error creating new category during save:', error);
                        'Error creating new category during save. Please try again.', 
        // Save any unsaved query entities first
            for (const entity of this.queryEntities) {
                if (!entity.IsSaved && entity.EntityID) {
                        console.error('Error saving query entity:', error);
        // Call the parent save method
        const result = await super.SaveRecord(StopEditModeAfterSave);
            // Reload related data after successful save as server-side processes may have updated them
                    this.loadQueryEntities()
    getStatusBadgeColor(): string {
            case 'Approved':
            case 'Pending':
            case 'Rejected':
     * Handle SQL value changes from the code editor
    onSQLChange(value: any) {
        if (this.isUpdatingEditorValue || !this.record) {
        // Update the record SQL value
        this.record.SQL = value;
     * Add a new field
    async addField() {
            const newField = await md.GetEntityObject<MJQueryFieldEntity>('MJ: Query Fields');
            newField.QueryID = this.record.ID;
            newField.Name = `field${this.queryFields.length + 1}`;
            newField.Description = '';
            newField.Sequence = (this.queryFields.length + 1) * 10;
            newField.SQLBaseType = 'nvarchar';
            newField.SQLFullType = 'nvarchar(255)';
            const saved = await newField.Save();
                this.queryFields.push(newField);
                this.queryFields.sort((a, b) => (a.Sequence || 0) - (b.Sequence || 0));
                    'Field added successfully',
            console.error('Error adding field:', error);
                'Failed to add field',
     * Delete a field
    async deleteField(field: MJQueryFieldEntity) {
        if (!confirm(`Are you sure you want to delete field "${field.Name}"?`)) {
            const deleted = await field.Delete();
                this.queryFields = this.queryFields.filter(f => f.ID !== field.ID);
                    'Field deleted successfully',
            console.error('Error deleting field:', error);
                'Failed to delete field',
     * Add a new entity
    async addEntity() {
            const newEntity = await md.GetEntityObject<MJQueryEntityEntity>('MJ: Query Entities');
            newEntity.QueryID = this.record.ID;
            // Add to the list immediately for UI responsiveness
            this.queryEntities.push(newEntity);
            console.error('Error adding entity:', error);
                'Failed to add entity',
     * Delete an entity
    async deleteEntity(entity: MJQueryEntityEntity) {
        if (!confirm(`Are you sure you want to delete entity "${entity.Entity}"?`)) {
                this.queryEntities = this.queryEntities.filter(e => e.ID !== entity.ID);
                    'Entity deleted successfully',
            console.error('Error deleting entity:', error);
                'Failed to delete entity',
     * Get entity options for dropdown
    getEntityOptions(): Array<{text: string, id: string}> {
        return Metadata.Provider.Entities.map(e => ({
            text: e.Name,
            id: e.ID
        })).sort((a, b) => a.text.localeCompare(b.text));
     * Get the grid edit mode based on component edit mode
    override GridEditMode(): "None" | "Save" | "Queue" {
        return this.EditMode ? "Queue" : "None";
