import { MJTemplateEntity, MJTemplateParamEntity } from '@memberjunction/core-entities';
export interface ParameterPair {
    isFromTemplate: boolean;
    isRequired?: boolean;
export interface TemplateRunResult {
    output?: string;
    selector: 'mj-template-param-dialog',
    templateUrl: './template-param-dialog.component.html',
    styleUrls: ['./template-param-dialog.component.css']
export class TemplateParamDialogComponent implements OnInit {
    @Input() template: MJTemplateEntity | null = null;
    public _isVisible: boolean = false;
    @Input() 
    get isVisible(): boolean {
        return this._isVisible;
    set isVisible(value: boolean) {
        const wasVisible = this._isVisible;
        this._isVisible = value;
        // Reset dialog state when opening
        if (value && !wasVisible) {
            this.resetDialogState();
    public parameters: ParameterPair[] = [];
    public isLoading = false;
    public testResult: TemplateRunResult | null = null;
    public hasUnsavedParameters = false;
    public jsonPreviewExpanded = false;
    private originalTemplateParams: MJTemplateParamEntity[] = [];
        if (this.template) {
            this.loadTemplateParams();
    async loadTemplateParams() {
                ExtraFilter: `TemplateID='${this.template.ID}'`,
            this.originalTemplateParams = results.Results;
            // Convert template params to parameter pairs
            this.parameters = this.originalTemplateParams.map(param => ({
                key: param.Name,
                isFromTemplate: true,
                description: param.Description || undefined,
                isRequired: param.IsRequired,
                type: param.Type
            // If no template params, add one empty pair to start
            if (this.parameters.length === 0) {
                this.addParameter();
                'Error loading template parameters',
    addParameter() {
        this.parameters.push({
            key: '',
            value: '',
            isFromTemplate: false
    removeParameter(index: number) {
        if (this.parameters.length > 1) {
            this.parameters.splice(index, 1);
    onParameterChange() {
        // Check if we have new parameters not in template
        this.hasUnsavedParameters = this.parameters.some(param => 
            param.key && 
            !param.isFromTemplate && 
            !this.originalTemplateParams.find(tp => tp.Name === param.key)
    async runTemplate() {
        // Validate parameter names - check for empty parameter names
        const emptyNameParams = this.parameters.filter(p => p.value && !p.key);
        if (emptyNameParams.length > 0) {
                'All parameters must have a name. Please enter parameter names or remove empty parameters.',
        const invalidParams = this.parameters.filter(p => p.key && !p.value && p.isRequired);
                `Required parameters missing: ${invalidParams.map(p => p.key).join(', ')}`,
        this.testResult = null;
            // Build context data object from parameters
            const contextData: any = {};
            this.parameters.forEach(param => {
                if (param.key && param.value) {
                        contextData[param.key] = JSON.parse(param.value);
                        contextData[param.key] = param.value;
            // Execute the RunTemplate GraphQL mutation
                mutation RunTemplate($templateId: String!, $contextData: String) {
                    RunTemplate(templateId: $templateId, contextData: $contextData) {
                        output
                        executionTimeMs
            const variables = {
                templateId: this.template.ID,
                contextData: JSON.stringify(contextData)
            const result = await dataProvider.ExecuteGQL(query, variables);
            if (result?.RunTemplate) {
                this.testResult = result.RunTemplate;
                // Collapse parameters and expand results after execution
                if (this.testResult?.success) {
                        `Template executed successfully in ${this.testResult.executionTimeMs || 0}ms`,
                        `Template execution failed: ${this.testResult?.error || 'Unknown error'}`,
                throw new Error(result.errors?.[0]?.message || 'Unknown GraphQL error');
            console.error('Template test error:', error);
            this.testResult = {
                error: (error as Error).message || 'Unknown error occurred'
            // Still collapse parameters and expand results on error
                `Template test failed: ${this.testResult?.error || 'Unknown error'}`,
    async updateTemplateParams() {
        if (!this.template?.ID || !this.hasUnsavedParameters) return;
        const newParams = this.parameters.filter(param => 
        if (newParams.length === 0) return;
            for (const param of newParams) {
                const templateParam = await md.GetEntityObject<MJTemplateParamEntity>('MJ: Template Params');
                templateParam.TemplateID = this.template.ID;
                templateParam.Name = param.key;
                templateParam.Description = param.description || null;
                templateParam.Type = 'Scalar'; // Default type
                templateParam.DefaultValue = param.value || null;
                templateParam.IsRequired = param.isRequired || false;
                const saved = await templateParam.Save();
                    throw new Error(`Failed to save parameter: ${param.key}`);
                `Added ${newParams.length} new parameter(s) to template`,
            this.hasUnsavedParameters = false;
            // Reload template params to sync
            await this.loadTemplateParams();
            console.error('Error updating template params:', error);
                'Error saving template parameters',
        this._isVisible = false;
    private resetDialogState() {
        // Reset expansion states for a clean testing experience
        this.parametersExpanded = true;     // Show params by default
        this.jsonPreviewExpanded = false;   // Hide JSON (developer-focused)
        this.resultsExpanded = true;        // Show results when they exist
        // Clear previous test results
        // Reset unsaved parameters flag
    saveResults() {
        if (!this.testResult) return;
        const content = this.testResult.success 
            ? this.testResult.output || 'No output'
            : this.testResult.error || 'No error details';
        const timestamp = new Date().toISOString().replace(/[:.]/g, '-').slice(0, -5);
        const templateName = this.template?.Name?.replace(/[^a-zA-Z0-9]/g, '_') || 'template';
        const status = this.testResult.success ? 'success' : 'error';
        const filename = `${templateName}_${status}_${timestamp}.txt`;
        // Create blob and download
        const blob = new Blob([content], { type: 'text/plain' });
        link.download = filename;
            `Results saved as ${filename}`,
    get parametersAsJson(): string {
        return JSON.stringify(contextData, null, 2);
