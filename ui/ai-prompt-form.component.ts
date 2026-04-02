import { Component, OnInit, ViewChild, ChangeDetectorRef, ViewContainerRef, inject } from '@angular/core';
import { MJTemplateEntity, MJTemplateContentEntity, MJTemplateParamEntity, MJAIPromptModelEntity, MJAIVendorEntity, MJAIModelVendorEntity, MJAIPromptTypeEntity, MJAIConfigurationEntity } from '@memberjunction/core-entities';
import { TemplateEditorConfig, TemplateEditorComponent } from '../../shared/components/template-editor.component';
import { MJAIPromptFormComponent } from '../../generated/Entities/MJAIPrompt/mjaiprompt.form.component';
import { AIPromptManagementService } from './ai-prompt-management.service';
import { AIModelEntityExtended, AIPromptCategoryEntityExtended, AIPromptEntityExtended, AIPromptRunEntityExtended } from '@memberjunction/ai-core-plus';
@RegisterClass(BaseFormComponent, 'MJ: AI Prompts')
    selector: 'mj-ai-prompt-form',
    templateUrl: './ai-prompt-form.component.html',
    styleUrls: ['./ai-prompt-form.component.css']
export class AIPromptFormComponentExtended extends MJAIPromptFormComponent implements OnInit {
    private promptManagementService = inject(AIPromptManagementService);
    public record!: AIPromptEntityExtended;
    public template: MJTemplateEntity | null = null;
    public templateContent: MJTemplateContentEntity | null = null;
    public templateParams: MJTemplateParamEntity[] = [];
    public isLoadingTemplate = true; // Default to loading state
    public isLoadingTemplateParams = false;
    public templateNotFoundInDatabase = false;
    public showTestHarness = false;
    // Model management
    public promptModels: MJAIPromptModelEntity[] = [];
    public availableModels: AIModelEntityExtended[] = [];
    public availableVendors: MJAIVendorEntity[] = [];
    public isLoadingModels = false;
    // Vendor management per model
    public modelVendorsMap = new Map<string, { vendors: MJAIVendorEntity[], modelVendors: MJAIModelVendorEntity[] }>();
    // AI Prompt Types
    public availablePromptTypes: MJAIPromptTypeEntity[] = [];
    public isLoadingPromptTypes = false;
    // AI Configurations
    public availableConfigurations: MJAIConfigurationEntity[] = [];
    public isLoadingConfigurations = false;
    // Result Selector Tree Data
    public resultSelectorTreeData: any[] = [];
    public isLoadingResultSelectorData = false;
    // Drag and drop state
    public draggedIndex: number = -1;
    // Execution History
    public executionHistory: AIPromptRunEntityExtended[] = [];
    public isLoadingHistory = false;
    public historySortField: 'runAt' | 'executionTime' | 'cost' | 'tokens' = 'runAt';
    public historySortDirection: 'asc' | 'desc' = 'desc';
    // Removed custom transaction tracking - we'll use base form's _pendingRecords instead
    // Store original state for cancel/revert functionality
    private originalTemplateID: string | null = null;
    // Main AI Prompt permissions inherited from BaseFormComponent:
    /** Check if user can create Templates */
    public get UserCanCreateTemplates(): boolean {
        return this.checkEntityPermission('Templates', 'Create');
    /** Check if user can update Templates */
    public get UserCanUpdateTemplates(): boolean {
        return this.checkEntityPermission('Templates', 'Update');
    /** Check if user can delete Templates */
    public get UserCanDeleteTemplates(): boolean {
        return this.checkEntityPermission('Templates', 'Delete');
    /** Check if user can read Templates */
    public get UserCanReadTemplates(): boolean {
        return this.checkEntityPermission('Templates', 'Read');
    /** Check if user can create Template Contents */
    public get UserCanCreateTemplateContents(): boolean {
        return this.checkEntityPermission('MJ: Template Contents', 'Create');
    /** Check if user can update Template Contents */
    public get UserCanUpdateTemplateContents(): boolean {
        return this.checkEntityPermission('MJ: Template Contents', 'Update');
    /** Check if user can create AI Prompt Models */
    public get UserCanCreatePromptModels(): boolean {
        return this.checkEntityPermission('MJ: AI Prompt Models', 'Create');
    /** Check if user can update AI Prompt Models */
    public get UserCanUpdatePromptModels(): boolean {
        return this.checkEntityPermission('MJ: AI Prompt Models', 'Update');
    /** Check if user can delete AI Prompt Models */
    public get UserCanDeletePromptModels(): boolean {
        return this.checkEntityPermission('MJ: AI Prompt Models', 'Delete');
            const entityInfo = this._metadata.Entities.find(e => e.Name === entityName);
            const userPermissions = entityInfo.GetUserPermisions(this._metadata.CurrentUser);
    // Template editor configuration
    public get templateEditorConfig(): TemplateEditorConfig {
            allowEdit: this.EditMode && this.UserCanUpdateTemplateContents,
            compactMode: false
    private _metadata = new Metadata();
    private __InferenceProvider_VendorTypeDefinitionID: string = '';
    @ViewChild('templateEditor') templateEditor: TemplateEditorComponent | undefined;
        // make sure AI Engine Base is configured, this will load stuff only if not already
        // loaded in the current process space
        await AIEngineBase.Instance.Config(false, this._metadata.CurrentUser);
        this.__InferenceProvider_VendorTypeDefinitionID = AIEngineBase.Instance.VendorTypeDefinitions.find(
            vtd => vtd.Name.trim().toLowerCase() === 'inference provider')?.ID || '';
        if (!this.__InferenceProvider_VendorTypeDefinitionID) {
            console.error('Inference Provider Vendor Type Definition ID not found');
                'Inference Provider Vendor Type Definition not found',
        // Load template when record changes
        if (this.record?.TemplateID) {
            // isLoadingTemplate is already true by default
            this.loadTemplate(); // Don't await so other loads can happen in parallel
            // No template ID, so we're not loading
            this.isLoadingTemplate = false;
        // Load available models, vendors, prompt types, configurations, prompt models, and result selector data
            this.loadAvailableModels(),
            this.loadAvailableVendors(),
            this.loadAvailablePromptTypes(),
            this.loadAvailableConfigurations(),
            this.loadPromptModels(),
            this.loadResultSelectorTreeData()
        // Load execution history if record is saved
        if (this.record?.IsSaved) {
            await this.loadExecutionHistory();
        // Set defaults for new records
            // Default to first prompt type if not set
            if (!this.record.TypeID && this.availablePromptTypes.length > 0) {
                this.record.TypeID = this.availablePromptTypes[0].ID;
            // Default status to Pending if not set
            if (!this.record.Status) {
                this.record.Status = 'Pending';
     * Loads the template associated with this AI prompt
    public async loadTemplate() {
        if (!this.record?.TemplateID) {
            this.template = null;
            this.templateNotFoundInDatabase = false;
        // First check if we already have this template in pending records (newly created)
        const pendingTemplate = this.PendingRecords.find(p => 
            p.entityObject.EntityInfo.Name === 'MJ: Templates' && 
            p.entityObject.Get('ID') === this.record.TemplateID
        if (pendingTemplate) {
            // Use the pending template
            this.template = pendingTemplate.entityObject as MJTemplateEntity;
            // Clear template content and params since this is a new template
            this.templateContent = null;
            this.templateParams = [];
        this.isLoadingTemplate = true;
        this.templateNotFoundInDatabase = false; // Reset the flag
            this.template = await this._metadata.GetEntityObject<MJTemplateEntity>('MJ: Templates');
            await this.template.Load(this.record.TemplateID);
            if (!this.template.IsSaved) {
                this.templateNotFoundInDatabase = true; // Set flag when template not found
                console.warn(`Template with ID ${this.record.TemplateID} not found`);
                // Load template content and parameters
                    this.loadTemplateContent(),
                    this.loadTemplateParams()
            console.error('Error loading template:', error);
            this.templateNotFoundInDatabase = true; // Set flag on error
                'Error loading associated template',
     * Handles template ID changes in the form
    public async onTemplateIdChange() {
            await this.loadTemplate();
     * Opens a dialog to link an existing template
    public async linkExistingTemplate() {
            this.promptManagementService.openTemplateSelectorDialog({
                title: 'Link Existing Template',
                showActiveOnly: true,
                selectedTemplateIds: this.record.TemplateID ? [this.record.TemplateID] : [],
                    if (result && result.selectedTemplates.length > 0) {
                        const selectedTemplate = result.selectedTemplates[0];
                        // First, clean up any pending changes related to the old template
                        this.cleanupOldTemplateRecords();
                        // Update the AI prompt to reference the selected template
                        this.record.TemplateID = selectedTemplate.ID;
                        // Load the selected template
                            `Template "${selectedTemplate.Name}" linked successfully`,
                        // User wants to create a new template
                        'Error opening template selector. Please try again.',
            console.error('Error in linkExistingTemplate:', error);
                'Error linking template. Please try again.',
     * Opens the current template in a new window
    public openTemplateInNewWindow() {
        if (!this.template?.ID) return;
        // TODO: Get the proper URL for template editing
        const templateUrl = `/templates/${this.template.ID}`;
        window.open(templateUrl, '_blank');
     * Creates a new template for this AI prompt (deferred until save)
    public async createNewTemplate() {
            const newTemplate = await this._metadata.GetEntityObject<MJTemplateEntity>('MJ: Templates');
            console.log("Record Name:", this.record.Name);
            newTemplate.NewRecord();
            newTemplate.Name = `${this.record.Name || 'AI Prompt'} Template`;
            newTemplate.Description = `Template for AI Prompt: ${this.record.Name}`;
            newTemplate.UserID = this._metadata.CurrentUser.ID;
            // Add to pending records instead of saving immediately
                entityObject: newTemplate,
            // Update the AI prompt to reference the new template
            this.record.TemplateID = newTemplate.ID;
            // Set the template for UI purposes
            this.template = newTemplate;
            // Clear existing template content and params since we have a new template
            // Force UI update in next microtask to ensure template editor refreshes
                'New template created and will be saved when you save the AI prompt',
                `Error creating new template: ${errorMessage}`,
                6000
     * Cleans up any pending records related to the old template when changing templates
    private cleanupOldTemplateRecords() {
        // Get current pending records and filter out template content/params from old template
        const currentPendingRecords = this.PendingRecords;
        // Remove template content and template param records
        for (let i = currentPendingRecords.length - 1; i >= 0; i--) {
            const record = currentPendingRecords[i];
            const entityName = record.entityObject.EntityInfo.Name;
            if (entityName === 'MJ: Template Contents' || entityName === 'MJ: Template Params') {
                currentPendingRecords.splice(i, 1);
     * Loads template content for the current template
    private async loadTemplateContent() {
        if (!this.template?.ID) {
            const results = await rv.RunView<MJTemplateContentEntity>({
                EntityName: 'MJ: Template Contents',
                ExtraFilter: `TemplateID = '${this.template.ID}'`,
                OrderBy: 'Priority ASC',
            // Get the first content (highest priority)
            this.templateContent = results.Results?.[0] || null;
            console.error('Error loading template content:', error);
     * Loads template parameters for the current template
    private async loadTemplateParams() {
        this.isLoadingTemplateParams = true;
            const results = await rv.RunView<MJTemplateParamEntity>({
                EntityName: 'MJ: Template Params',
            this.templateParams = results.Results || [];
            console.error('Error loading template params:', error);
            this.isLoadingTemplateParams = false;
     * Opens the AI prompt execution dialog
    public executeAIPrompt() {
                'Please save the AI prompt before executing',
        if (this.record.Status !== 'Active') {
                'AI prompt must be Active to execute',
        // Use test harness instead
        this.openTestHarness();
     * Opens the test harness
                'Please save the AI prompt before testing',
        // Use the dialog service instead of inline
        this.testHarnessService.openForPrompt(this.record.ID).subscribe({
                    // Reload execution history
                    this.loadExecutionHistory();
                console.error('Test harness error:', error);
                    'Test failed: ' + error.message,
     * Handles when test harness is closed
    public onTestHarnessVisibilityChanged(isVisible: boolean) {
        this.showTestHarness = isVisible;
     * Handles template content changes from the editor
    public onTemplateContentChange(content: MJTemplateContentEntity[]) {
        // Handle template content changes if needed
        console.log('Template content changed:', content);
        // Mark as having unsaved changes
        // If we have content changes, we need to ensure they're added to pending records
        // This is typically handled by the template editor component itself
     * Handles template content record deletion
     * This method should be called by the template editor to properly manage deletions
    public handleTemplateContentDelete(templateContent: MJTemplateContentEntity) {
        if (templateContent.IsSaved) {
            // If it's saved, add to pending deletions
                entityObject: templateContent,
            // If it's not saved, remove it from pending records if it exists there
                if (record.entityObject === templateContent || 
                    (record.entityObject.EntityInfo.Name === 'MJ: Template Contents' && 
                     record.entityObject.Get('ID') === templateContent.Get('ID'))) {
     * Handles template content record creation/modification
     * This method should be called by the template editor to properly manage saves
    public handleTemplateContentSave(templateContent: MJTemplateContentEntity) {
        if (templateContent.Dirty || !templateContent.IsSaved) {
            // Add to pending saves
     * Adds template content changes to pending records
    private addTemplateContentsToPendingRecords() {
        // This method would typically get pending changes from the template editor
        // The template editor should expose its pending changes through events or direct calls
        // For now, we'll rely on the template editor to manage its own pending records
        // and communicate them through the MJ event system
        // If the template editor has a method to get pending changes, we would call it here
        if (this.templateEditor && typeof (this.templateEditor as any).getPendingChanges === 'function') {
                const pendingChanges = (this.templateEditor as any).getPendingChanges();
                if (pendingChanges && pendingChanges.length > 0) {
                    this.PendingRecords.push(...pendingChanges);
                console.warn('Template editor does not support getPendingChanges method:', error);
     * Handles template run requests from the editor
    public onTemplateRun(template: MJTemplateEntity) {
        console.log('Template run requested:', template);
        // Could open the template parameter dialog here if needed
            'Template run functionality coming soon',
     * Gets the display text for parallelization mode
    public getParallelizationModeDisplay(): string {
        switch (this.record?.ParallelizationMode) {
            case 'None': return 'None';
            case 'StaticCount': return `Static count (${this.record.ParallelCount || 1})`;
            case 'ConfigParam': return `Config parameter (${this.record.ParallelConfigParam || 'not set'})`;
            case 'ModelSpecific': return 'Model-specific configuration';
            default: return 'Unknown';
     * Gets the display text for output type
    public getOutputTypeDisplay(): string {
        const type = this.record?.OutputType || 'string';
        const validationBehavior = this.record?.ValidationBehavior || 'Warn';
        if (validationBehavior === 'None') {
            return `${type} (${validationBehavior})`;
     * Gets the color for validation behavior display
    public getValidationColor(): string {
        switch (this.record?.ValidationBehavior) {
            case 'Strict': return '#dc3545'; // red
            case 'Warn': return '#ffc107'; // yellow
            default: return '#6c757d'; // default gray
     * Checks if ParallelCount field should be visible
    public get showParallelCount(): boolean {
        return this.record?.ParallelizationMode === 'StaticCount';
     * Checks if ParallelConfigParam field should be visible
    public get showParallelConfigParam(): boolean {
        return this.record?.ParallelizationMode === 'ConfigParam';
     * Checks if OutputExample field should be visible
    public get showOutputExample(): boolean {
        return this.record?.OutputType === 'object';
     * Checks if the AI prompt can be executed
    public get canExecute(): boolean {
        return !!(this.record?.ID && 
                  this.record.Status === 'Active' && 
                  this.record.TemplateID && 
                  this.template);
     * Gets status badge color
     * Loads available AI models for selection
    public async loadAvailableModels() {
            models.sort((a, b) => a.Name.localeCompare(b.Name));
            this.availableModels = models;
            console.error('Error loading available models:', error);
     * Loads available AI vendors for selection
    public async loadAvailableVendors() {
            const vendors = engine.Vendors;
            vendors.sort((a, b) => a.Name.localeCompare(b.Name));
            this.availableVendors = vendors;
            console.error('Error loading available vendors:', error);
     * Loads available AI prompt types for selection
    public async loadAvailablePromptTypes() {
        this.isLoadingPromptTypes = true;
            const promptTypes = engine.PromptTypes;
            promptTypes.sort((a, b) => a.Name.localeCompare(b.Name));
            this.availablePromptTypes = promptTypes;
            console.error('Error loading available prompt types:', error);
            this.isLoadingPromptTypes = false;
     * Loads available AI configurations for selection
    public async loadAvailableConfigurations() {
        this.isLoadingConfigurations = true;
            const configurations = engine.Configurations;
            configurations.sort((a, b) => a.Name.localeCompare(b.Name));
            this.availableConfigurations = configurations;
            console.error('Error loading available configurations:', error);
            this.isLoadingConfigurations = false;
     * Loads vendors available for a specific model
    public async loadVendorsForModel(modelId: string): Promise<{ vendors: MJAIVendorEntity[], modelVendors: MJAIModelVendorEntity[] }> {
        if (!modelId) {
            return { vendors: [], modelVendors: [] };
        if (this.modelVendorsMap.has(modelId)) {
            return this.modelVendorsMap.get(modelId)!;
            // Load model vendors for this model, filtering by TypeID for inference providers only
            const modelVendors = engine.ModelVendors.filter(mv => mv.ModelID === modelId && mv.TypeID === this.__InferenceProvider_VendorTypeDefinitionID);
            // filter vendors to just the vendors in the modelVendors array in VendorID
            const vendors = engine.Vendors.filter(v => modelVendors.some(mv => mv.VendorID === v.ID));
            const result = { vendors, modelVendors };
            this.modelVendorsMap.set(modelId, result);
            console.error('Error loading vendors for model:', error);
     * Gets vendors for a specific model from cache or loads them
    public async getVendorsForModel(modelId: string): Promise<MJAIVendorEntity[]> {
        const result = await this.loadVendorsForModel(modelId);
        return result.vendors;
     * Gets the status for a model-vendor combination
    public getModelVendorStatus(modelId: string, vendorId: string): string {
        const modelVendorData = this.modelVendorsMap.get(modelId);
        if (!modelVendorData) return 'Unknown';
        const modelVendor = modelVendorData.modelVendors.find(mv => mv.VendorID === vendorId);
        return modelVendor?.Status || 'Unknown';
     * Gets the color for vendor status display
    public getVendorStatusColor(modelId: string, vendorId: string): string {
        const status = this.getModelVendorStatus(modelId, vendorId);
            case 'Active': return '#28a745'; // green
            case 'Inactive': return '#dc3545'; // red  
            case 'Pending': return '#ffc107'; // yellow
            default: return '#6c757d'; // gray
     * Handles model selection change and loads vendors for that model
    public async onModelChange(modelId: string, promptModelIndex: number) {
        const promptModel = this.promptModels[promptModelIndex];
        if (!promptModel) return;
        // Clear the vendor selection when model changes
        promptModel.VendorID = null;
        // Load vendors for the new model
            const vendorData = await this.loadVendorsForModel(modelId);
            // Auto-select first vendor if available
            if (vendorData.vendors.length > 0) {
                promptModel.VendorID = vendorData.vendors[0].ID;
     * Handles configuration change for a prompt model
    public onConfigurationChange(configurationId: string | null, promptModelIndex: number) {
        promptModel.ConfigurationID = configurationId;
     * Gets vendors for a specific model
    public getVendorsForModelSync(modelId: string): MJAIVendorEntity[] {
        return modelVendorData?.vendors || [];
     * Checks if vendor dropdown should be shown (more than one vendor)
    public shouldShowVendorDropdown(modelId: string): boolean {
        if (!modelId) return false;
        const vendors = this.getVendorsForModelSync(modelId);
        return vendors.length > 1;
     * Loads prompt models for this AI prompt
    public async loadPromptModels() {
            this.promptModels = [];
        this.isLoadingModels = true;
            this.promptModels = engine.PromptModels.filter(pm => pm.PromptID === this.record.ID);
            this.promptModels.sort((a, b) => {
                // first sort on priority (descending), then by created date (ascending)
                return b.Priority - a.Priority || new Date(a.__mj_CreatedAt).getTime() - new Date(b.__mj_CreatedAt).getTime();
            // Load vendors for existing models
            const modelIds = this.promptModels
                .map(pm => pm.ModelID)
                .filter(id => id); // Filter out null/undefined
            await Promise.all(modelIds.map(modelId => this.loadVendorsForModel(modelId)));
            console.error('Error loading prompt models:', error);
            this.isLoadingModels = false;
     * Adds a new model to the prompt (deferred until save)
    public async addNewModel() {
            const newModel = await this._metadata.GetEntityObject<MJAIPromptModelEntity>('MJ: AI Prompt Models');
            newModel.PromptID = this.record.ID;
            // Set priority to 1 (lowest) for new models added at the end
            newModel.Priority = 1;
            // Generate a temporary ID for tracking if the model doesn't have one
            if (!newModel.ID) {
                (newModel as any)._tempId = `temp_${Date.now()}_${Math.random().toString(36).substring(2, 11)}`;
            // ModelID will be set by user
            this.promptModels.push(newModel);
            // Update priorities after adding
            this.updateModelPriorities();
                'New model added. Select a model and save to persist changes.',
            console.error('Error creating new model:', error);
                'Error creating new model',
     * Removes a model from the prompt (deferred until save)
    public async removePromptModel(index: number) {
        if (index < 0 || index >= this.promptModels.length) return;
        const model = this.promptModels[index];
            // If it's a saved model, add it to pending deletions
            if (model.IsSaved) {
                    entityObject: model,
            // Remove from local array
            this.promptModels.splice(index, 1);
            // Update priorities after removal
                'Model will be removed when you save the prompt',
            console.error('Error removing model:', error);
                'Error removing model',
     * Gets the display name for a model ID
    public getModelDisplayName(modelId: string): string {
        if (!modelId) return '';
        const model = this.availableModels.find(m => m.ID === modelId);
        return model ? model.Name : modelId;
     * Gets the display name for a vendor ID
    public getVendorDisplayName(vendorId: string): string {
        if (!vendorId) return '';
        const vendor = this.availableVendors.find(v => v.ID === vendorId);
        return vendor ? vendor.Name : vendorId;
     * Gets the display name for a prompt type ID
    public getPromptTypeDisplayName(typeId: string): string {
        if (!typeId) return '';
        const type = this.availablePromptTypes.find(t => t.ID === typeId);
        return type ? type.Name : typeId;
     * Gets the display name for a configuration ID
    public getConfigurationDisplayName(configurationId: string | null): string {
        if (!configurationId) return 'Default';
        const config = this.availableConfigurations.find(c => c.ID === configurationId);
        return config ? config.Name : configurationId;
     * Override PopulatePendingRecords to add AI prompt model changes
        // any records we've added (like templates) before calling the parent method
        // Re-add our preserved records
        // Add prompt model changes to pending records
        this.addPromptModelsToPendingRecords();
        // Handle template content changes through the template editor
        this.addTemplateContentsToPendingRecords();
     * Override StartEditMode to capture original state for cancel functionality
    public StartEditMode(): void {
        // Store original template ID for cancel functionality
        this.originalTemplateID = this.record.TemplateID;
        // Call parent implementation
        super.StartEditMode();
     * Override CancelEdit to restore original state
    public CancelEdit() {
        // Call parent implementation first
        // Restore original template state
        if (this.originalTemplateID !== this.record.TemplateID) {
            this.record.TemplateID = this.originalTemplateID || '';
            // Reload the template to reflect the reverted state
            this.loadTemplate().then(() => {
        } else if (this.templateEditor) {
            // Even if template didn't change, refresh the template editor to discard any unsaved content changes
            this.templateEditor.refreshAndDiscardChanges();
        // Clear the stored original state
        this.originalTemplateID = null;
     * Adds prompt model changes to the pending records
    private addPromptModelsToPendingRecords() {
        // Add all prompt models that have been modified or are new
        for (const model of this.promptModels) {
            if (model.ModelID && (model.Dirty || !model.IsSaved)) {
                // Set the PromptID if it's not already set
                if (!model.PromptID) {
                    model.PromptID = this.record.ID;
     * Override InternalSaveRecord to handle template dependencies and related entity changes
     * Templates must be saved before AI Prompts to avoid foreign key constraint errors
            // First, save any templates that need to be saved (they must be saved before AI Prompts)
                            'Failed to save template. Please check the template data.',
            // Now save the main AI Prompt record
                    'Failed to save AI prompt details. Please check the form data.',
            // Then save all other pending records (excluding templates which we already saved)
                p.entityObject.EntityInfo.Name !== 'MJ: Templates'
                    await record.entityObject.Save();
                // Reload prompt models to reflect database state
                await this.loadPromptModels();
                // Reload template to reflect any changes
                if (this.record.TemplateID) {
                    'AI Prompt saved successfully',
            console.error('Error in AI prompt save:', error);
     * Loads the result selector tree data (categories and prompts)
    public async loadResultSelectorTreeData() {
        this.isLoadingResultSelectorData = true;
            // Load categories and prompts
            const categories = engine.PromptCategories;
            const prompts = engine.Prompts.filter(p => p.Status === 'Active');
            categories.sort((a, b) => a.Name.localeCompare(b.Name));
            prompts.sort((a, b) => a.Name.localeCompare(b.Name));
            // Build tree structure
            this.resultSelectorTreeData = this.buildResultSelectorTree(categories, prompts);
            console.error('Error loading result selector tree data:', error);
            this.resultSelectorTreeData = [];
            this.isLoadingResultSelectorData = false;
     * Builds the tree structure for result selector
    private buildResultSelectorTree(categories: AIPromptCategoryEntityExtended[], prompts: AIPromptEntityExtended[]): any[] {
        const tree: any[] = [];
        // Add "Clear Selection" option at the top
        tree.push({
            text: '(Clear Selection)',
            value: null,
            hasChildren: false,
            isCategory: false,
            isClearOption: true
        // Create category nodes
        const categoryMap = new Map<string, any>();
            const node = {
                text: category.Name,
                value: null, // Categories don't have values
                hasChildren: true,
                items: [],
                isCategory: true,
                categoryId: category.ID
            categoryMap.set(category.ID, node);
            // Handle parent-child relationships
            if (category.ParentID) {
                const parentNode = categoryMap.get(category.ParentID);
                if (parentNode) {
                    parentNode.items.push(node);
                    tree.push(node); // Parent not found, add to root
                tree.push(node); // Root category
        // Collect uncategorized prompts
        const uncategorizedPrompts: any[] = [];
        // Add prompts to their categories
            const promptNode = {
                text: prompt.Name,
                value: prompt.ID,
                promptId: prompt.ID
            if (prompt.CategoryID) {
                const categoryNode = categoryMap.get(prompt.CategoryID);
                if (categoryNode) {
                    categoryNode.items.push(promptNode);
                    // Category not found, add to uncategorized
                    uncategorizedPrompts.push(promptNode);
                // No category, add to uncategorized
        // If there are uncategorized prompts, add them to a special section
        if (uncategorizedPrompts.length > 0) {
                text: 'Uncategorized',
                items: uncategorizedPrompts,
                categoryId: 'uncategorized'
        return tree;
     * Handles result selector selection
    public onResultSelectorChange(value: string) {
        this.record.ResultSelectorPromptID = value || null;
     * Gets the display name for a prompt ID from the tree data
    public getPromptDisplayName(promptId: string): string {
        if (!promptId) return '';
        const findPromptInTree = (nodes: any[]): string => {
                if (!node.isCategory && node.value === promptId) {
                    return node.text;
                if (node.items && node.items.length > 0) {
                    const found = findPromptInTree(node.items);
        return findPromptInTree(this.resultSelectorTreeData);
     * Moves a model up in the list by swapping with the previous item
    public moveModelUp(index: number) {
        if (index > 0 && index < this.promptModels.length) {
            // Create new array to ensure Angular detects the change
            const newModels = [...this.promptModels];
            // Swap with previous item
            [newModels[index - 1], newModels[index]] = 
            [newModels[index], newModels[index - 1]];
            // Replace the array and force full re-render
            this.promptModels = [...newModels];
            // Force Angular to re-evaluate all bindings
            // Additional force update for Kendo dropdowns
     * Moves a model down in the list by swapping with the next item
    public moveModelDown(index: number) {
        if (index >= 0 && index < this.promptModels.length - 1) {
            // Swap with next item
            [newModels[index], newModels[index + 1]] = 
            [newModels[index + 1], newModels[index]];
     * Updates priority values based on array order
    private updateModelPriorities() {
        // Update priorities based on position (higher priority for items at top)
        const maxPriority = this.promptModels.length;
        this.promptModels.forEach((model, index) => {
            model.Priority = maxPriority - index;
     * Gets a stable identifier for a model (for form tracking)
    public getModelTrackId(model: MJAIPromptModelEntity): string {
        return model.ID || (model as any)._tempId || '';
     * Handles drag start event
    public onDragStart(event: DragEvent, index: number) {
        this.draggedIndex = index;
        event.dataTransfer!.effectAllowed = 'move';
        event.dataTransfer!.setData('text/html', ''); // Required for Firefox
     * Handles drag over event
    public onDragOver(event: DragEvent) {
        event.preventDefault();
        event.dataTransfer!.dropEffect = 'move';
     * Handles drop event
    public onDrop(event: DragEvent, dropIndex: number) {
        if (this.draggedIndex !== -1 && this.draggedIndex !== dropIndex) {
            // Remove dragged item
            const draggedItem = newModels.splice(this.draggedIndex, 1)[0];
            // Insert at new position
            newModels.splice(dropIndex, 0, draggedItem);
            // Update priorities
        this.draggedIndex = -1;
     * Handles drag end event
    public onDragEnd(_event: DragEvent) {
     * Load execution history for this prompt
    public async loadExecutionHistory() {
        this.isLoadingHistory = true;
                ExtraFilter: `PromptID='${this.record.ID}'`,
                OrderBy: 'RunAt DESC' 
            this.executionHistory = result.Results;
            this.sortExecutionHistory();
            console.error('Error loading execution history:', error);
            this.executionHistory = [];
            this.isLoadingHistory = false;
     * Sort execution history based on current sort field and direction
    public sortExecutionHistory() {
        if (!this.executionHistory || this.executionHistory.length === 0) return;
        this.executionHistory.sort((a, b) => {
            let aVal: any, bVal: any;
            switch (this.historySortField) {
                case 'runAt':
                    aVal = a.RunAt ? new Date(a.RunAt).getTime() : 0;
                    bVal = b.RunAt ? new Date(b.RunAt).getTime() : 0;
                case 'executionTime':
                    aVal = a.ExecutionTimeMS || 0;
                    bVal = b.ExecutionTimeMS || 0;
                case 'cost':
                    aVal = a.TotalCost || a.Cost || 0;
                    bVal = b.TotalCost || b.Cost || 0;
                case 'tokens':
                    aVal = a.TokensUsed || 0;
                    bVal = b.TokensUsed || 0;
            if (this.historySortDirection === 'asc') {
                return aVal > bVal ? 1 : aVal < bVal ? -1 : 0;
                return aVal < bVal ? 1 : aVal > bVal ? -1 : 0;
     * Change sort field and direction for execution history
    public changeHistorySort(field: 'runAt' | 'executionTime' | 'cost' | 'tokens') {
        if (this.historySortField === field) {
            // Toggle direction if same field
            this.historySortDirection = this.historySortDirection === 'asc' ? 'desc' : 'asc';
            // New field, default to desc
            this.historySortField = field;
            this.historySortDirection = 'desc';
     * Navigate to a prompt run record
    public navigateToPromptRun(runId: string) {
        SharedService.Instance.OpenEntityRecord('MJ: AI Prompt Runs', CompositeKey.FromID(runId));
     * Format duration for display
    public formatDuration(ms: number | null): string {
     * Format cost for display
     * Format tokens for display
    public formatTokens(tokens: number | null): string {
     * Get status color for execution
    public getExecutionStatusColor(success: boolean | null): string {
        if (success === true) return '#28a745';
        if (success === false) return '#dc3545';
     * Get status icon for execution
    public getExecutionStatusIcon(success: boolean | null): string {
        if (success === true) return 'fa-check-circle';
        if (success === false) return 'fa-times-circle';
     * Gets the icon for a template parameter type
    public getParamTypeIcon(type: string): string {
            case 'Scalar': return 'fa-font';
            case 'Array': return 'fa-list';
            case 'Object': return 'fa-cube';
            case 'Record': return 'fa-file-alt';
            case 'Entity': return 'fa-table';
            default: return 'fa-question';
     * Gets the color for a template parameter type
    public getParamTypeColor(type: string): string {
            case 'Scalar': return '#17a2b8';
            case 'Array': return '#28a745';
            case 'Object': return '#6f42c1';
            case 'Record': return '#fd7e14';
            case 'Entity': return '#dc3545';
     * Gets a friendly description of the parameter type
    public getParamTypeDescription(param: MJTemplateParamEntity): string {
        switch (param.Type) {
                return 'Single value (text, number, date, etc.)';
            case 'Array': 
                return 'List of values';
            case 'Object': 
                return 'JSON object with multiple properties';
                if (param.EntityID) {
                    return `Single record from entity`;
                return 'Single database record';
                    return `Multiple records from entity`;
                return 'Entity data collection';
                return 'Unknown type';
