import { MJTemplateContentEntity, MJTemplateParamEntity, MJTemplateEntity } from "@memberjunction/core-entities";
    type: 'Scalar' | 'Array' | 'Object';
export class TemplateContentEntityExtended extends MJTemplateContentEntity {
            // Check if this is a new record or if TemplateText has changed
            const templateTextField = this.GetFieldByName('TemplateText');
            const shouldExtractParams = !this.IsSaved || templateTextField.Dirty;
            // Save the template content first
            // Extract and sync parameters if needed
            if (shouldExtractParams && this.TemplateText && this.TemplateText.trim().length > 0) {
                await this.extractAndSyncParameters();
            LogError('Failed to save template content and extract parameters:', e);
    private async extractAndSyncParameters(): Promise<void> {
            if (this.Template === "Template Parameter Extraction") {
                return; // Skip extraction for this special case - this is ourselves!
            // Find the Template Parameter Extraction prompt
                p.Name === 'Template Parameter Extraction' && 
                // prompt not configured, non-fatl, just warn and return
                console.warn('AI prompt for Template Parameter Extraction not found. Skipping parameter extraction.');
                templateText: this.TemplateText
                LogError('Failed to extract template parameters:', result.errorMessage);
            // Process the extracted parameters
            await this.syncTemplateParameters(result.result.parameters);
            LogError('Error extracting template parameters:', e);
            // Don't throw here - we don't want to fail the save if parameter extraction fails
            // The user can still manually manage parameters if needed
    private normalizeParameterType(type: string): 'Scalar' | 'Array' | 'Object' | 'Record' | 'Entity' {
        // Normalize LLM output to match database constraint values
        const normalized = type.toLowerCase();
            case 'scalar':
            case 'list':
                return 'Array';
            case 'dict':
            case 'dictionary':
                return 'Object';
                return 'Entity';
                console.warn(`Unknown parameter type '${type}', defaulting to 'Scalar'`);
    private async syncTemplateParameters(extractedParams: ExtractedParameter[]): Promise<void> {
            // Get existing template parameters
            const existingParamsResult = await rv.RunView<MJTemplateParamEntity>({
                throw new Error(`Failed to load existing template parameters: ${existingParamsResult.ErrorMessage}`);
            // Determine if we're in single or multiple template content scenario
            const templateContentsResult = await rv.RunView({
            const isMultipleContents = templateContentsResult.Success && 
                                     templateContentsResult.Results && 
                                     templateContentsResult.Results.length > 1;
            // For single template content, all params have TemplateContentID = NULL
            // For multiple contents, we need to be more careful about global vs content-specific params
            const templateContentID = isMultipleContents ? this.ID : null;
            // Filter existing params relevant to this content
            const relevantExistingParams = existingParams.filter(p => 
                isMultipleContents ? p.TemplateContentID === this.ID : p.TemplateContentID == null
                !relevantExistingParams.some(ep => ep.Name.toLowerCase() === p.name.toLowerCase())
            const paramsToUpdate = relevantExistingParams.filter(ep =>
            const paramsToRemove = relevantExistingParams.filter(ep =>
                const newParam = await md.GetEntityObject<MJTemplateParamEntity>('MJ: Template Params', this.ContextCurrentUser);
                newParam.TemplateID = this.TemplateID;
                newParam.TemplateContentID = templateContentID;
                newParam.Type = this.normalizeParameterType(param.type);
                newParam.IsRequired = false; // LLM has been unreliable here, make them ALL optional so we don't break template rendering in case it picks up stuff that is NOT really a param... param.isRequired;
                    const normalizedType = this.normalizeParameterType(extractedParam.type);
                    if (existingParam.Type !== normalizedType) {
                        existingParam.Type = normalizedType;
                        existingParam.IsRequired = false; // SEE ABOVE - make not required in all cases....  extractedParam.isRequired;
            // Remove parameters that are no longer in the template
                await Promise.all(promises); // saving these is non-critical
            // If this is multiple contents scenario, we might need to handle global params
            // This is a more complex scenario that would require analyzing all template contents
            // For now, we're handling content-specific params only
            LogError('Failed to sync template parameters:', e);
