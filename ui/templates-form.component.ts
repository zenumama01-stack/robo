import { Component, OnInit, OnDestroy, ViewChild, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { MJTemplateEntity, MJTemplateContentEntity, MJTemplateCategoryEntity } from '@memberjunction/core-entities';
import { MJTemplateFormComponent } from '../../generated/Entities/MJTemplate/mjtemplate.form.component';
import { TemplateEngineBase } from '@memberjunction/templates-base-types';
import { LanguageDescription } from '@codemirror/language';
import { languages } from '@codemirror/language-data';
@RegisterClass(BaseFormComponent, 'MJ: Templates') 
    selector: 'mj-templates-form',
    templateUrl: './templates-form.component.html',
    styleUrls: ['../../../shared/form-styles.css', './templates-form.component.css']
export class TemplatesFormExtendedComponent extends MJTemplateFormComponent implements OnInit, OnDestroy, AfterViewInit {
    public record!: MJTemplateEntity;
    public templateContents: MJTemplateContentEntity[] = [];
    public selectedContentIndex: number = 0;
    public isAddingNewContent: boolean = false;
    public newTemplateContent: MJTemplateContentEntity | null = null;
    public hasUnsavedChanges: boolean = false;
    public templateInfoExpanded: boolean = true;
    public templateContentsExpanded: boolean = true;
        // TODO: Load from database
    public contentTypeOptions: Array<{text: string, value: string}> = [];
    public supportedLanguages: LanguageDescription[] = languages;
    @ViewChild('codeEditor') codeEditor: CodeEditorComponent | null = null;
    public isRunningTemplate = false;
    public templateTestResult: string | null = null;
    public templateTestError: string | null = null;
    public showParamDialog = false;
    // Template editor configuration for shared component
    public templateEditorConfig: TemplateEditorConfig = {
        showRunButton: true,
        await this.loadTemplateContents();
        this.loadContentTypes();
        // Clean up any active timeouts
        this.activeTimeouts.forEach(timeoutId => clearTimeout(timeoutId));
        // Initial sync when view is ready
        this.syncEditorValue();
    async loadTemplateContents() {
                    ExtraFilter: `TemplateID='${this.record.ID}'`,
                    OrderBy: 'Priority ASC, __mj_CreatedAt ASC',
                this.templateContents = results.Results;
                // If we have contents but no selection, select the first one
                if (this.templateContents.length > 0 && this.selectedContentIndex === 0) {
                    this.selectedContentIndex = 0;
                // If no template contents exist, create a default one for single-content optimization
                if (this.templateContents.length === 0) {
                    await this.createDefaultTemplateContent();
                // Entity change tracking is handled automatically by BaseEntity
                // Sync editor value after loading content
                console.error('Error loading template contents:', error);
    async createDefaultTemplateContent() {
        const defaultContent = await md.GetEntityObject<MJTemplateContentEntity>('MJ: Template Contents');
        defaultContent.TemplateID = this.record.ID;
        defaultContent.Priority = 1;
        defaultContent.IsActive = true;
        // Set default to first real content type (skip "Select Type..." if it exists)
        const validContentTypes = this.contentTypeOptions.filter(option => option.value !== '');
        if (validContentTypes.length > 0) {
            defaultContent.TypeID = validContentTypes[0].value;
        this.templateContents = [defaultContent];
        // Sync editor value after creating default content
    selectTemplateContent(index: number, confirmSwitch: boolean = true) {
        // If we're adding new content and user clicks on existing content, ask for confirmation
        if (this.isAddingNewContent && confirmSwitch) {
            if (!confirm('You have unsaved changes to a new content version. Are you sure you want to switch? Your changes will be lost.')) {
        if (index >= 0 && index < this.templateContents.length) {
            this.selectedContentIndex = index;
            this.isAddingNewContent = false;
            // Don't clear newTemplateContent to preserve the work in progress
            // Sync editor value when switching content
            const results = await rv.RunView({
                EntityName: 'MJ: Template Categories' 
                ...results.Results.map((cat: any) => ({
    loadContentTypes() {
            // Get content types from TemplateEngine cache
            const contentTypes = TemplateEngineBase.Instance.TemplateContentTypes;
            this.contentTypeOptions = [
                { text: 'Select Type...', value: '' },
                ...contentTypes.map(ct => ({
                    text: ct.Name,
                    value: ct.ID
            console.error('Error loading content types:', error);
            // Fallback to basic types
                { text: 'HTML', value: 'HTML' },
                { text: 'Plain Text', value: 'Text' },
                { text: 'Markdown', value: 'Markdown' }
                const newCategory = await md.GetEntityObject<MJTemplateCategoryEntity>('MJ: Template Categories');
                newCategory.UserID = this.record.UserID || md.CurrentUser.ID;
                        `Failed to create new category. ${newCategory.LatestResult?.CompleteMessage || ''}`, 
    async addNewTemplateContent() {
        this.newTemplateContent = await md.GetEntityObject<MJTemplateContentEntity>('MJ: Template Contents');
        this.newTemplateContent.TemplateID = this.record.ID;
        this.newTemplateContent.Priority = this.templateContents.length + 1;
        this.newTemplateContent.IsActive = true;
            this.newTemplateContent.TypeID = validContentTypes[0].value;
        this.newTemplateContent.TemplateText = '';
        // Add immediately to the array
        this.templateContents.push(this.newTemplateContent);
        this.selectedContentIndex = this.templateContents.length - 1;
        this.isAddingNewContent = true;
        // Sync editor value when adding new content
    cancelNewTemplateContent() {
        this.newTemplateContent = null;
    async deleteTemplateContent(index: number) {
            const contentToDelete = this.templateContents[index];
            if (contentToDelete.ID) {
                    const result = await contentToDelete.Delete();
                        this.templateContents.splice(index, 1);
                        // Adjust selected index if necessary
                        if (this.selectedContentIndex >= this.templateContents.length) {
                            this.selectedContentIndex = Math.max(0, this.templateContents.length - 1);
                        // If no contents remain, create a default one
                        console.error('Delete returned false');
                        MJNotificationService.Instance.CreateSimpleNotification(`Failed to delete template content. ${contentToDelete.LatestResult.CompleteMessage}`, 'error');
                    console.error('Error deleting template content:', error);
                // Not saved yet, just remove from array
                // Reset adding new content state if we're deleting the new content
                if (this.isAddingNewContent && index === this.templateContents.length) {
            console.error('Invalid index for deletion:', index);
    get currentTemplateContent(): MJTemplateContentEntity | null {
        if (this.isAddingNewContent) {
            return this.newTemplateContent;
        return this.templateContents[this.selectedContentIndex] || null;
    get hasMultipleContents(): boolean {
        return this.templateContents.length > 1 || this.isAddingNewContent;
    onContentTypeChange() {
        // Content type changes just modify the current content, no new record creation
    onContentChange() {
        this.hasUnsavedChanges = this.templateContents.some(content => content.Dirty) || 
                                this.isAddingNewContent ||
    onTemplateTextChange(event: any) {
        if (this.isUpdatingEditorValue) {
            // Ignore change events when we're programmatically updating the editor
        if (this.currentTemplateContent) {
            // Extract value from event - might be event.target.value or just event depending on component
            const value = typeof event === 'string' ? event : (event.target?.value || event);
            this.currentTemplateContent.TemplateText = value;
            // hasUnsavedChanges is automatically handled by entity's IsDirty flag
     * Helper method to track setTimeout calls for cleanup
            // Remove from tracking when it executes
     * Manually sync the editor value without triggering change events
    private syncEditorValue() {
        // Use Promise.resolve() to wait for the next microtask after any pending changes
            // Then tracked setTimeout for the next macrotask to ensure DOM is updated
                if (!this.codeEditor) {
                const newValue = this.currentTemplateContent?.TemplateText || '';
                this.codeEditor.setValue(newValue);  
    async saveTemplateContents(): Promise<boolean> {
            // Save all template contents that have changes
            for (const content of this.templateContents) {
                content.TemplateID = this.record.ID; // Ensure FK is set
                if (content.Dirty || !content.ID) {
                    const contentResult = await content.Save();
                    if (!contentResult) {
                        console.error('Failed to save template content:', content);
            this.updateUnsavedChangesFlag(); // Update based on current entity states
            console.error('Error saving template contents:', error);
        // Check if we need to create a new category first
                    newCategory.Name = this.record.CategoryID.trim(); // CategoryID contains the new category name, trim it
        // Call the parent save method to save the template
        // Before saving, if a new record, make sure the UserID is set
        if (!this.record.IsSaved && !this.record.UserID) {
            this.record.UserID = md.CurrentUser.ID;
        const templateSaved = await super.SaveRecord(StopEditModeAfterSave);
        if (templateSaved) {
            // Then save all template contents
            return await this.saveTemplateContents();
    getContentTypeDisplayText(typeId: string): string {
        if (!typeId) return '-';
        const option = this.contentTypeOptions.find(opt => opt.value === typeId);
        return option ? option.text : typeId;
    getEditorLanguage(): string {
        if (!this.currentTemplateContent?.TypeID) {
            return 'jinja2'; // default to jinja2 for template syntax (compatible with Nunjucks)
        const contentType = this.currentTemplateContent.TypeID.toLowerCase();
        // Map content types to CodeMirror language modes
        switch (contentType) {
                return 'jinja2'; // Use jinja2 for HTML templates to get template syntax highlighting
            case 'markdown':
            case 'md':
            case 'javascript':
            case 'js':
            case 'css':
                return 'css';
                return 'xml';
            case 'sql':
            case 'plain':
                return 'jinja2'; // Default to jinja2 since templates often contain template syntax
        const normalizedName = categoryName.trim().toLowerCase();
     * Test run the current template using the parameter dialog
        if (!this.record?.IsSaved || !this.currentTemplateContent) {
                'Please save the template before running it.', 
                    'Failed to save template changes.', 
        this.showParamDialog = true;
     * Handle parameter dialog close
    onParamDialogClose() {
        this.showParamDialog = false;
    getContentTypeOptionsForContent(): Array<{text: string, value: string}> {
        // Always exclude "Select Type..." option for all content
        return this.contentTypeOptions.filter(option => option.value !== '');
     * Handles template content changes from the shared editor
    public onSharedTemplateContentChange(content: MJTemplateContentEntity[]) {
        this.templateContents = content;
     * Handles template run requests from the shared editor
    public onSharedTemplateRun(template: MJTemplateEntity) {
        this.runTemplate();
