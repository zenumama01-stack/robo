import { BaseEntity, DatabaseProviderBase, EntityInfo, EntitySaveOptions, LogError, Metadata, RunView, IMetadataProvider } from "@memberjunction/core";
import { MJActionLibraryEntity, MJActionParamEntity, MJActionResultCodeEntity } from "@memberjunction/core-entities";
import { MJEventType, MJGlobal, RegisterClass } from "@memberjunction/global";
import { DocumentationEngine, LibraryEntityExtended, LibraryItemEntityExtended } from "@memberjunction/doc-utils";
import { ActionEngineBase, ActionEntityExtended, ActionLibrary, GeneratedCode } from "@memberjunction/actions-base";
 * Extended GeneratedCode interface to include parameters and result codes
interface GeneratedCodeExtended extends GeneratedCode {
    Parameters?: Array<{
        ValueType: 'Scalar' | 'Simple Object' | 'BaseEntity Sub-Class' | 'Other';
        IsArray: boolean;
        DefaultValue: string | null;
    ResultCodes?: Array<{
        ResultCode: string;
        IsSuccess: boolean;
 * Server-Only custom sub-class for Actions entity. This sub-class handles the process of auto-generation code for the Actions entity.
export class ActionEntityServerEntity extends ActionEntityExtended {
    constructor(Entity: EntityInfo) {
        super(Entity); // call super
        // In constructor we must use new Metadata() since entity isn't fully initialized yet
        // This is an acceptable exception as it only checks provider type at construction time
        if (md.ProviderType !== 'Database')
            throw new Error('This class is only supported for server-side/database providers. Remove this package from your application.');
     * Override of the base Save method to handle the pre-processing to auto generate code whenever an action's UserPrompt is modified.
     * This happens when a new record is created and also whenever the UserPrompt field is changed.
        // make sure the ActionEngine is configured
        await ActionEngineBase.Instance.Config(false, this.ContextCurrentUser);
        await DocumentationEngine.Instance.Config(false, this.ContextCurrentUser);
        // Start a database transaction
            let newCodeGenerated: boolean = false;
            let codeLibraries: ActionLibrary[] = [];
            let generatedParameters: Array<any> = [];
            let generatedResultCodes: Array<any> = [];
            if ( this.Type === 'Generated' && // only generate when the type is Generated
                 !this.CodeLocked && // only generate when the code is not locked
                 (this.GetFieldByName('UserPrompt').Dirty || !this.IsSaved || this.ForceCodeGeneration)  // only generate when the UserPrompt field is dirty or this is a new record or we are being asked to FORCE code generation
                // UserPrompt field is dirty, or this is a new record, either way, this is the condition where we want to generate the Code.
                const result = await this.GenerateCode();
                    this.Code = result.Code;
                    this.CodeComments = result.Comments;
                    this.CodeApprovalStatus = 'Pending'; // set to pending even if previously approved since we changed the code
                    this.CodeApprovedAt = null; // reset the approved at date
                    this.CodeApprovedByUserID = null; // reset the approved by user id
                    newCodeGenerated = true; // flag for post-save processing of libraries
                    codeLibraries = result.LibrariesUsed;
                    // Store the generated parameters and result codes for later processing
                    generatedParameters = result.Parameters || [];
                    generatedResultCodes = result.ResultCodes || [];
                    throw new Error(`Failed to generate code for Action ${this.Name}.`);
            this.ForceCodeGeneration = false; // make sure to reset this flag every time we save, it should never live past one run of the Save method
            const wasNewRecord = !this.IsSaved;
                // Now handle all the child entities within the same transaction
                if (newCodeGenerated) {
                    // Process libraries
                    await this.manageLibraries(codeLibraries, wasNewRecord);
                    // Process parameters 
                    if (generatedParameters.length > 0) {
                        await this.ProcessGeneratedParameters(generatedParameters);
                    // Process result codes
                    if (generatedResultCodes.length > 0) {
                        await this.ProcessGeneratedResultCodes(generatedResultCodes);
                // Rollback on save failure
            // Rollback on any error
    protected async manageLibraries(codeLibraries: ActionLibrary[], wasNewRecord: boolean): Promise<void> {
        // new code was generated, we need to sync up the ActionLibraries table with the libraries used in the code for this Action
        // get a list of existing ActionLibrary records that match this Action
        const existingLibraries: MJActionLibraryEntity[] = [];
        if (!wasNewRecord) {
            const libResult = await rv.RunView(
                    ExtraFilter: `ActionID = '${this.ID}'`,
            if (libResult.Success && libResult.Results.length > 0) {
                existingLibraries.push(...libResult.Results);
        // now we need to go through the libraries we ARE currently using in the current code and make sure they are in the ActionLibraries table
        // and make sure nothing is in the table that we are not using
        const librariesToAdd = codeLibraries.filter(l => !existingLibraries.some(el => el.Library.trim().toLowerCase() === l.LibraryName.trim().toLowerCase()));
        const librariesToRemove = existingLibraries.filter(el => !codeLibraries.some(l => l.LibraryName.trim().toLowerCase() === el.Library.trim().toLowerCase()));
        const librariesToUpdate = existingLibraries.filter(el => codeLibraries.some(l => l.LibraryName.trim().toLowerCase() === el.Library.trim().toLowerCase()));
        // Use the entity's provider instead of creating new Metadata instance
        for (const lib of librariesToAdd) {
            const libMetadata = md.Libraries.find(l => l.Name.trim().toLowerCase() === lib.LibraryName.trim().toLowerCase());
            if (libMetadata) {
                const newLib = await md.GetEntityObject<MJActionLibraryEntity>('MJ: Action Libraries', this.ContextCurrentUser);
                newLib.ActionID = this.ID;
                newLib.LibraryID = libMetadata.ID;
                newLib.ItemsUsed = lib.ItemsUsed.join(',');
                await newLib.Save(); 
        // now update the libraries that were already in place to ensure the ItemsUsed are up to date
        for (const lib of librariesToUpdate) {
            const newCode = codeLibraries.find(l => l.LibraryName.trim().toLowerCase() === lib.Library.trim().toLowerCase());
            lib.ItemsUsed = newCode.ItemsUsed.join(',');
            await lib.Save();  
        // now remove the libraries that are no longer used
        for (const lib of librariesToRemove) {
            // each lib in this array iteration is already a BaseEntity derived object
            await lib.Delete();  
    protected SendMessage(message: string) {
            args: { message },
            eventCode: 'Actions',
     * This method will generate code using a combination of the UserPrompt field as well as the overall context of how an Action gets executed. Code from the Action is later used by CodeGen to inject into
     * the mj_actions library for each user environment. The mj_actions library will have a class for each action and that class will have certain libraries imported at the top of the file and available for use.
     * That information along with a detailed amount of system prompt steering goes into the AI model in order to generate contextually appropriate and reliable code that maps to the business logic of the user
    public async GenerateCode(maxAttempts: number = 3): Promise<GeneratedCodeExtended> {
            this.SendMessage('Generating code... ');
            await AIEngine.Instance.Config(false, this.ContextCurrentUser);
            // Load the consolidated prompt template from preloaded prompts
            const aiPrompt = AIEngine.Instance.Prompts.find(p => 
                p.Name === 'Generate Action Code' && 
                p.Category === 'Actions'
            if (!aiPrompt) {
                throw new Error('Failed to find AI prompt template: Generate Action Code in Actions category');
            // Prepare prompt data
            const promptData = await this.PreparePromptData();
            const promptRunner = new AIPromptRunner();
            params.prompt = aiPrompt;
            params.contextUser = this.ContextCurrentUser;
            const result = await promptRunner.ExecutePrompt<{
                explanation: string;
                libraries: Array<{LibraryName: string, ItemsUsed: string[]}>;
                resultCodes?: Array<{
                const generatedCode: GeneratedCodeExtended = {
                    Code: result.result.code.trim(),
                    Comments: result.result.explanation,
                    LibrariesUsed: result.result.libraries,
                    Parameters: result.result.parameters,
                    ResultCodes: result.result.resultCodes
                // Validate the generated code
                return await this.ValidateGeneratedCode(generatedCode, maxAttempts);
                    Code: '',
                    Comments: result.errorMessage || 'Failed to generate code',
                    LibrariesUsed: []
     * Prepares the data context for the prompt template
    protected async PreparePromptData(): Promise<Record<string, any>> {
        const data: Record<string, any> = {
            userPrompt: this.UserPrompt,
            entityInfo: this.PrepareEntityInfo(),
            availableLibraries: this.GenerateLibrariesContext(),
            IsChildAction: !!this.ParentID  // Template variable for conditional sections
        // If this is a child action, load parent context and create ChildActionInfo
        if (this.ParentID) {
            const parentAction = await this.LoadParentAction();
            if (parentAction) {
                // Create the ChildActionInfo template variable with all parent details
                data.ChildActionInfo = `
**Parent Action ID:** ${parentAction.ID.trim().toLowerCase() /*just to make sure casing doesn't mess up the string we pass in*/}
**Parent Action Name:** ${parentAction.Name}
**Parent Description:** ${parentAction.Description || 'No description provided'}
**Parent Parameters:**
${JSON.stringify(parentAction.Params.map(p => {
        Type: p.Type,
        ValueType: p.ValueType,
        IsArray: p.IsArray,
        IsRequired: p.IsRequired,
        DefaultValue: p.DefaultValue,
        Description: p.Description
}), null, 2)}
                // Also include the parent action object for template access
                data.parentAction = {
                    Name: parentAction.Name,
                    Description: parentAction.Description,
                    Category: parentAction.Category
                data.actionParams = parentAction.Params.map(p => {
     * Returns a simplified object with basic entity info: Name, Description and field list.
    protected PrepareEntityInfo(): any {
        const entities = md.Entities.map(entity => ({
            Fields: entity.Fields.map(field => ({
                NeedsQuotes: field.NeedsQuotes,
                ReadOnly: field.ReadOnly,
                AllowsNull: field.AllowsNull
     * Loads the parent action entity if this is a child action
    protected async LoadParentAction(): Promise<ActionEntityExtended | null> {
        const parent = await md.GetEntityObject<ActionEntityExtended>('MJ: Actions', this.ContextCurrentUser);
     * Processes generated parameters and saves them to the database
    protected async ProcessGeneratedParameters(parameters: Array<any>): Promise<void> {
            // First, get existing parameters
            const existingParams = await rv.RunView<MJActionParamEntity>({
                ExtraFilter: `ActionID='${this.ID}'`,
            if (!existingParams.Success) {
                throw new Error(`Failed to load existing parameters: ${existingParams.ErrorMessage}`);
            const existingParamsList = existingParams.Results || [];
            const generatedParamNames = parameters.map(p => p.Name.toLowerCase());
            // Find parameters to add, update, or remove
            const paramsToAdd = parameters.filter(p => 
                !existingParamsList.some(ep => ep.Name.toLowerCase() === p.Name.toLowerCase())
            const paramsToUpdate = existingParamsList.filter(ep =>
                parameters.some(p => p.Name.toLowerCase() === ep.Name.toLowerCase())
            const paramsToRemove = existingParamsList.filter(ep =>
                !generatedParamNames.includes(ep.Name.toLowerCase())
            // Add new parameters
            for (const param of paramsToAdd) {
                const newParam = await md.GetEntityObject<MJActionParamEntity>('MJ: Action Params', this.ContextCurrentUser);
                newParam.ActionID = this.ID;
                newParam.Name = param.Name;
                const t = param.Type;
                if (t === 'Input' || t === 'Output' || t === 'Both') {
                    newParam.Type = t;
                    newParam.Type = 'Input'; // Default to Input if type is not recognized and emit a warning
                    console.warn(`Action Generator: Unrecognized parameter type "${t}" for parameter "${param.Name}". Defaulting to "Input".`);
                newParam.ValueType = param.ValueType;
                newParam.IsArray = param.IsArray;
                newParam.IsRequired = param.IsRequired;
                newParam.DefaultValue = param.DefaultValue;
                newParam.Description = param.Description;
                await newParam.Save();
            // Update existing parameters if properties changed
            for (const existingParam of paramsToUpdate) {
                const generatedParam = parameters.find(p => p.Name.toLowerCase() === existingParam.Name.toLowerCase());
                if (generatedParam) {
                    // Check each property for changes
                    if (existingParam.Type !== generatedParam.Type) {
                        existingParam.Type = generatedParam.Type;
                    if (existingParam.ValueType !== generatedParam.ValueType) {
                        existingParam.ValueType = generatedParam.ValueType;
                    if (existingParam.IsArray !== generatedParam.IsArray) {
                        existingParam.IsArray = generatedParam.IsArray;
                    if (existingParam.IsRequired !== generatedParam.IsRequired) {
                        existingParam.IsRequired = generatedParam.IsRequired;
                    if (existingParam.DefaultValue !== generatedParam.DefaultValue) {
                        existingParam.DefaultValue = generatedParam.DefaultValue;
                    if (existingParam.Description !== generatedParam.Description) {
                        existingParam.Description = generatedParam.Description;
                        await existingParam.Save();
            // Remove parameters that are no longer in the generated list
            for (const paramToRemove of paramsToRemove) {
                await paramToRemove.Delete();
            LogError('Failed to save generated parameters:', e);
            throw e; // Re-throw since we're in a transaction
     * Processes generated result codes and saves them to the database
    protected async ProcessGeneratedResultCodes(resultCodes: Array<{ResultCode: string; Description: string; IsSuccess: boolean}>): Promise<void> {
            // First, get existing result codes
            const existingCodes = await rv.RunView<MJActionResultCodeEntity>({
            if (!existingCodes.Success) {
                throw new Error(`Failed to load existing result codes: ${existingCodes.ErrorMessage}`);
            const existingCodesList = existingCodes.Results || [];
            const generatedCodeNames = resultCodes.map(rc => rc.ResultCode.toLowerCase());
            // Find result codes to add, update, or remove
            const codesToAdd = resultCodes.filter(rc => 
                !existingCodesList.some(ec => ec.ResultCode.toLowerCase() === rc.ResultCode.toLowerCase())
            const codesToUpdate = existingCodesList.filter(ec =>
                resultCodes.some(rc => rc.ResultCode.toLowerCase() === ec.ResultCode.toLowerCase())
            const codesToRemove = existingCodesList.filter(ec =>
                !generatedCodeNames.includes(ec.ResultCode.toLowerCase())
            // Add new result codes
            for (const resultCode of codesToAdd) {
                const newCode = await md.GetEntityObject<MJActionResultCodeEntity>('MJ: Action Result Codes', this.ContextCurrentUser);
                newCode.ActionID = this.ID;
                newCode.ResultCode = resultCode.ResultCode;
                newCode.Description = resultCode.Description;
                newCode.IsSuccess = resultCode.IsSuccess;
                await newCode.Save();
            // Update existing result codes if properties changed
            for (const existingCode of codesToUpdate) {
                const generatedCode = resultCodes.find(rc => rc.ResultCode.toLowerCase() === existingCode.ResultCode.toLowerCase());
                if (generatedCode) {
                    if (existingCode.Description !== generatedCode.Description) {
                        existingCode.Description = generatedCode.Description;
                    if (existingCode.IsSuccess !== generatedCode.IsSuccess) {
                        existingCode.IsSuccess = generatedCode.IsSuccess;
                        await existingCode.Save();
            // Remove result codes that are no longer in the generated list
            for (const codeToRemove of codesToRemove) {
                await codeToRemove.Delete();
            LogError('Failed to save generated result codes:', e);
     * Validates the generated code
    protected async ValidateGeneratedCode(generatedCode: GeneratedCodeExtended, attemptsRemaining: number): Promise<GeneratedCodeExtended> {
        // For now, return the code as-is
        // TODO: Implement validation logic using a separate prompt
        return generatedCode;
     * Generates the libraries context for the prompt
    protected GenerateLibrariesContext(): 
            Items: {
        }[] 
        return DocumentationEngine.Instance.Libraries.map((library: LibraryEntityExtended) => {
                Name: library.Name,
                Description: library.Description,
                Items: library.Items.map((item: LibraryItemEntityExtended) => {
                        Name: item.Name,
                        Content: item.HTMLContent
