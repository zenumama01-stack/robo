import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, OnChanges, SimpleChanges, ViewChild, AfterViewInit } from '@angular/core';
import { MJTemplateEntity, MJTemplateContentEntity } from '@memberjunction/core-entities';
import { DEFAULT_SYSTEM_PLACEHOLDERS, SystemPlaceholder, SYSTEM_PLACEHOLDER_CATEGORIES, SystemPlaceholderCategory } from '@memberjunction/ai-core-plus';
export interface TemplateEditorConfig {
    allowEdit?: boolean;
    showRunButton?: boolean;
    compactMode?: boolean;
    selector: 'mj-template-editor',
    templateUrl: './template-editor.component.html',
    styleUrls: ['./template-editor.component.css']
export class TemplateEditorComponent implements OnInit, OnChanges, OnDestroy, AfterViewInit {
    @Input() config: TemplateEditorConfig = {
    @Output() contentChange = new EventEmitter<MJTemplateContentEntity[]>();
    @Output() runTemplate = new EventEmitter<MJTemplateEntity>();
    public activeHelpTab: 'syntax' | 'placeholders' = 'syntax';
    public activePlaceholderCategory: string = '';
    // System placeholders organized by category
    public placeholderCategories: Array<{
        category: SystemPlaceholderCategory;
        placeholders: SystemPlaceholder[];
    constructor(private notificationService: MJNotificationService) {}
        this.organizePlaceholdersByCategory();
    async ngOnChanges(changes: SimpleChanges) {
        if (changes['template']) {
            // Template input has changed, reload contents
                // Template cleared, reset state
                this.templateContents = [];
    private organizePlaceholdersByCategory() {
        // Group placeholders by their category
        this.placeholderCategories = SYSTEM_PLACEHOLDER_CATEGORIES.map(category => {
            const categoryPlaceholders = DEFAULT_SYSTEM_PLACEHOLDERS.filter(
                placeholder => placeholder.category === category.name
                category: category,
                placeholders: categoryPlaceholders
        }).filter(cat => cat.placeholders.length > 0); // Only include categories that have placeholders
        // Add any uncategorized placeholders to a misc category if needed
        const categorized = DEFAULT_SYSTEM_PLACEHOLDERS.filter(p => p.category);
        const uncategorized = DEFAULT_SYSTEM_PLACEHOLDERS.filter(p => !p.category);
        if (uncategorized.length > 0) {
            this.placeholderCategories.push({
                category: {
                    name: 'Other',
                    icon: 'fa-ellipsis-h',
                placeholders: uncategorized
        // Set the first category as active
        if (this.placeholderCategories.length > 0) {
            this.activePlaceholderCategory = this.placeholderCategories[0].category.name;
     * Copies a placeholder to the clipboard
    public async copyPlaceholder(placeholderName: string): Promise<void> {
            await navigator.clipboard.writeText(`{{ ${placeholderName} }}`);
            this.notificationService.CreateSimpleNotification('Placeholder copied to clipboard!', 'success', 2000);
            console.error('Failed to copy placeholder:', error);
            this.notificationService.CreateSimpleNotification('Failed to copy placeholder', 'error', 3000);
            // Reset state first
            if (this.template.IsSaved && this.template.ID) {
                // Load existing template contents for saved templates
            if (this.templateContents.length > 0) {
            // If no template contents exist (either new template or saved template with no content), 
            // create a default one for single-content optimization
            this.contentChange.emit(this.templateContents);
     * Public method to refresh the template editor and discard unsaved changes
     * This should be called when canceling edits to restore the original state
    public async refreshAndDiscardChanges() {
        const defaultContent = await this._metadata.GetEntityObject<MJTemplateContentEntity>('MJ: Template Contents');
        defaultContent.TemplateID = this.template!.ID;
        if (!this.config.allowEdit) return;
        this.newTemplateContent = await this._metadata.GetEntityObject<MJTemplateContentEntity>('MJ: Template Contents');
        this.newTemplateContent.TemplateID = this.template!.ID;
        if (!this.config.allowEdit || index < 0 || index >= this.templateContents.length) return;
        // Check if the entity is actually saved, not just if it has an ID
        if (contentToDelete.IsSaved) {
                    MJNotificationService.Instance.CreateSimpleNotification(`Failed to delete template content. ${contentToDelete.LatestResult?.Message}`, 'error');
                MJNotificationService.Instance.CreateSimpleNotification(`Error deleting template content: ${error}`, 'error');
                                this.isAddingNewContent;
        if (!this.config.allowEdit) return false;
                content.TemplateID = this.template!.ID; // Ensure FK is set
    async onRunTemplate() {
        if (!this.template?.IsSaved || !this.currentTemplateContent) {
            const saveResult = await this.saveTemplateContents();
        // Emit the run template event
        this.runTemplate.emit(this.template);
