import { BaseEntity, CompositeKey, EntitySaveOptions, IMetadataProvider, LogError, Metadata, QueryEntityInfo, QueryFieldInfo, QueryParameterInfo, QueryPermissionInfo, RunView, SimpleEmbeddingResult } from "@memberjunction/core";
import { MJQueryEntity, MJQueryParameterEntity, MJQueryFieldEntity, MJQueryEntityEntity } from "@memberjunction/core-entities";
import { BaseEmbeddings, EmbedTextParams, GetAIAPIKey } from "@memberjunction/ai";
import { SQLParser } from "./sql-parser";
interface ExtractedParameter {
    type: 'string' | 'number' | 'date' | 'boolean' | 'array' | 'object';
    usage: string[];
    defaultValue: string | null;
    sampleValue: string | null;
interface ExtractedField {
    dynamicName?: boolean;
    type: 'number' | 'string' | 'date' | 'boolean';
    optional: boolean;
    // Source entity tracking - identifies where the field data originates
    sourceEntity?: string | null;      // Entity name this field comes from (null if computed/aggregated)
    sourceFieldName?: string | null;   // Original field name on the source entity (null if computed/aggregated)
    isComputed?: boolean;              // True if field is an expression/calculation (not direct column)
    isSummary?: boolean;               // True if field uses aggregate function (SUM, COUNT, AVG, etc.)
    computationDescription?: string;   // Explanation of how the field is computed (if applicable)
interface ParameterExtractionResult {
    parameters: ExtractedParameter[];
    selectClause?: ExtractedField[];
export class QueryEntityExtended extends MJQueryEntity {
    private _queryEntities: QueryEntityInfo[] = [];
    private _queryFields: QueryFieldInfo[] = [];
    private _queryParameters: QueryParameterInfo[] = [];
    private _queryPermissions: QueryPermissionInfo[] = [];
        return this._queryEntities;
        return this._queryFields;
        return this._queryParameters;
        return this._queryPermissions;
            // Check if this is a new record or if SQL/Description has changed
            const sqlField = this.GetFieldByName('SQL');
            const descriptionField = this.GetFieldByName('Description');
            const shouldExtractData = !this.IsSaved || sqlField.Dirty;
            const shouldGenerateEmbedding = !this.IsSaved || descriptionField.Dirty;
            // Generate embedding for Description if needed, before saving
            if (shouldGenerateEmbedding) {
                await this.GenerateEmbeddingByFieldName("Description", "EmbeddingVector", "EmbeddingModelID");
            } else if (!this.Description || this.Description.trim().length === 0) {
                // Clear embedding if description is empty
            // Save the query first without AI processing (no transaction needed for basic save)
            // Extract and sync parameters AFTER saving, outside of any transaction
            // This prevents connection pool exhaustion from long-running AI operations
            if (shouldExtractData && this.SQL && this.SQL.trim().length > 0) {
                await this.extractAndSyncDataAsync();
                await this.RefreshRelatedMetadata(true); // sync the related metadata so this entity is correct
            } else if (!this.SQL || this.SQL.trim().length === 0) {
                // If SQL is empty, ensure UsesTemplate is false and remove all related data
                // This can also happen asynchronously since it's cleanup work
                this.UsesTemplate = false;
                await this.cleanupEmptyQueryAsync();
            LogError('Failed to save query:', e);
            this.LatestResult?.Errors.push(e);
    override async Delete(options?: EntitySaveOptions): Promise<boolean> {
            // Perform the actual delete operation
            const deleteResult = await super.Delete(options);
            // CRITICAL: Refresh metadata cache after deletion to prevent stale query references
            // This ensures that MJAPI and any other services using cached metadata
            // immediately see that this query no longer exists
            await this.RefreshRelatedMetadata(true);
            LogError('Failed to delete query:', e);
     * Asynchronous version of extractAndSyncData that runs outside the main save operation
     * to prevent connection pool exhaustion
    private async extractAndSyncDataAsync(): Promise<void> {
            await this.extractAndSyncData();
            // Save the query again to update the UsesTemplate flag and any changes from AI processing
            // This is a separate, fast operation that doesn't involve AI
            const updateResult = await super.Save();
            if (!updateResult) {
                LogError('Failed to save query after AI processing completed');
            LogError('Error in async AI processing:', e);
            // Set UsesTemplate to false on error and save
            // This ensures the query is still usable even if AI extraction fails
                await super.Save();
            } catch (saveError) {
                LogError('Failed to save query after AI processing error:', saveError);
     * Asynchronous cleanup for empty queries
    private async cleanupEmptyQueryAsync(): Promise<void> {
                this.removeAllQueryParameters(),
                this.removeAllQueryFields(),
                this.removeAllQueryEntities()
            // Save the updated UsesTemplate flag
                LogError('Failed to save query after cleanup');
            LogError('Error in async cleanup:', e);
    private async extractAndSyncData(): Promise<void> {
            // Check if SQL contains Nunjucks syntax (determines if query uses templates/parameters)
            const hasNunjucksSyntax = this.SQL && (
                this.SQL.includes('{{') ||
                this.SQL.includes('{%') ||
                this.SQL.includes('{#')
            await AIEngine.Instance.Config(false, this.ContextCurrentUser, this.ProviderToUse as any as IMetadataProvider);
            // Find the SQL Query Parameter Extraction prompt
                p.Name === 'SQL Query Parameter Extraction' &&
                p.Category === 'MJ: System'
                // Prompt not configured, non-fatal, just warn and return
                console.warn('AI prompt for SQL Query Parameter Extraction not found. Skipping query metadata extraction.');
            // First, do a quick parse to identify entities from the SQL
            // We'll use this to provide entity metadata to the LLM for better type inference
            const entityMetadata = await this.extractEntityMetadataFromSQL();
            // Prepare prompt data - we'll send the SQL as templateText since the prompt
            // is designed to extract both parameters (if any) and query fields/entities
                templateText: this.SQL,
                entities: entityMetadata
            const result = await promptRunner.ExecutePrompt<ParameterExtractionResult>(params);
                // AI extraction failed - log details for debugging
                console.warn(`Query "${this.Name}" - AI query metadata extraction failed:`, {
                    status: result.status,
                    hasResult: !!result.result
            // Process the extracted data in parallel
            const syncPromises: Promise<void>[] = [];
            // Sync parameters if we have Nunjucks syntax and parameters were extracted
            // For non-templated queries, ensure any stale parameters are removed
            if (hasNunjucksSyntax && result.result.parameters && Array.isArray(result.result.parameters)) {
                syncPromises.push(this.syncQueryParameters(result.result.parameters));
                // No Nunjucks syntax - remove any existing parameters
                syncPromises.push(this.removeAllQueryParameters());
            // Always sync query fields if we got a selectClause back
            if (result.result.selectClause && Array.isArray(result.result.selectClause) && result.result.selectClause.length > 0) {
                syncPromises.push(this.syncQueryFields(result.result.selectClause));
            // Use deterministic SQL parsing for entity extraction instead of LLM
            // entityMetadata is already extracted above and is more reliable than LLM extraction
            if (entityMetadata.length > 0) {
                syncPromises.push(this.syncQueryEntities(entityMetadata));
            await Promise.all(syncPromises);
            // Update UsesTemplate flag based on whether Nunjucks syntax exists and parameters were found
            this.UsesTemplate = hasNunjucksSyntax &&
                result.result.parameters &&
                Array.isArray(result.result.parameters) &&
                result.result.parameters.length > 0;
            // Unexpected error during extraction - log for debugging but don't fail the save
            LogError(`Query "${this.Name}" AI extraction error:`, e);
     * Extracts entity metadata from the SQL to provide context to the LLM for parameter type inference.
     * Uses node-sql-parser for robust SQL parsing after pre-processing Nunjucks templates.
    private async extractEntityMetadataFromSQL(): Promise<Array<{
        baseView: string;
        fields: Array<{ name: string; type: string; isPrimaryKey: boolean }>;
        const results: Array<{
        if (!this.SQL) return results;
            // Use the shared SQLParser with Nunjucks preprocessing
            const parseResult = SQLParser.ParseWithTemplatePreprocessing(this.SQL);
            // Cross-reference parsed tables against entity metadata
                    (e.BaseView.toLowerCase() === tableRef.TableName.toLowerCase() ||
                     e.BaseTable.toLowerCase() === tableRef.TableName.toLowerCase()) &&
                if (matchingEntity) {
                    // Filter to fields that are actually referenced in the SQL
                    const relevantFields = matchingEntity.Fields
                            const fieldLower = f.Name.toLowerCase();
                            for (const colRef of parseResult.Columns) {
                                const colLower = colRef.ColumnName.toLowerCase();
                                if (colLower === fieldLower) return true;
                                // Check if column qualifier matches table alias or name
                                if (colRef.TableQualifier) {
                                    const qualLower = colRef.TableQualifier.toLowerCase();
                                    if ((qualLower === tableRef.Alias.toLowerCase() ||
                                         qualLower === tableRef.TableName.toLowerCase()) &&
                                        colLower === fieldLower) {
                            // Also include primary keys as they're always useful context
                            return f.IsPrimaryKey;
                            isPrimaryKey: f.IsPrimaryKey
                    if (relevantFields.length > 0) {
                            name: matchingEntity.Name,
                            schemaName: matchingEntity.SchemaName,
                            baseView: matchingEntity.BaseView,
                            fields: relevantFields
            console.warn(`Error in extractEntityMetadataFromSQL: ${error}`);
    private async syncQueryParameters(extractedParams: ExtractedParameter[]): Promise<void> {
        // Use same casting pattern as RefreshRelatedMetadata method
            // Get existing query parameters
            const existingParams: MJQueryParameterEntity[] = [];
                const existingParamsResult = await rv.RunView<MJQueryParameterEntity>({
                    ExtraFilter: `QueryID='${this.ID}'`,
                if (!existingParamsResult.Success) {
                    throw new Error(`Failed to load existing query parameters: ${existingParamsResult.ErrorMessage}`);
                existingParams.push (...existingParamsResult.Results || []);
            // Convert extracted param names to lowercase for comparison
            const extractedParamNames = extractedParams.map(p => p.name.toLowerCase());
            const paramsToAdd = extractedParams.filter(p => 
                !existingParams.some(ep => ep.Name.toLowerCase() === p.name.toLowerCase())
            const paramsToUpdate = existingParams.filter(ep =>
                extractedParams.some(p => p.name.toLowerCase() === ep.Name.toLowerCase())
            const paramsToRemove = existingParams.filter(ep =>
                !extractedParamNames.includes(ep.Name.toLowerCase())
            // Prepare all save/delete operations
            const promises: Promise<boolean>[] = [];
                const newParam = await md.GetEntityObject<MJQueryParameterEntity>('MJ: Query Parameters', this.ContextCurrentUser);
                newParam.QueryID = this.ID;
                newParam.Name = param.name;
                // Normalize type to lowercase for case-insensitive matching
                const normalizedType = param.type?.toLowerCase();
                switch (normalizedType) {
                    case "array":
                    case "string":
                        newParam.Type = normalizedType;
                    case "object":
                        // Object type is supported in Nunjucks/RunQuery but not yet in database schema
                        // Store as string for now - the actual validation happens at runtime
                        console.log(`Query "${this.Name}" - Parameter "${param.name}" is type "object", storing as "string" (runtime will handle object validation)`);
                        console.warn(`Query "${this.Name}" - Unknown parameter type "${param.type}" for parameter "${param.name}", defaulting to "string"`);
                newParam.IsRequired = param.isRequired;
                newParam.DefaultValue = param.defaultValue;
                newParam.Description = param.description;
                newParam.SampleValue = param.sampleValue;
                newParam.DetectionMethod = 'AI'; // Indicate this was found via AI
                promises.push(newParam.Save());
                const extractedParam = extractedParams.find(p => p.name.toLowerCase() === existingParam.Name.toLowerCase());
                if (extractedParam) {
                    // Normalize type to lowercase for case-insensitive comparison
                    const normalizedType = extractedParam.type?.toLowerCase();
                    const validTypes: Array<'array' | 'boolean' | 'string' | 'date' | 'number'> = ['array', 'boolean', 'string', 'date', 'number'];
                    const isValidType = validTypes.includes(normalizedType as any);
                    let targetType: 'array' | 'boolean' | 'string' | 'date' | 'number';
                    if (isValidType) {
                        targetType = normalizedType as any;
                    } else if (normalizedType === 'object') {
                        console.log(`Query "${this.Name}" - Parameter "${extractedParam.name}" is type "object", storing as "string" (runtime will handle object validation)`);
                        targetType = 'string';
                        console.warn(`Query "${this.Name}" - Unknown parameter type "${extractedParam.type}" for parameter "${extractedParam.name}", defaulting to "string"`);
                    if (existingParam.Type !== targetType) {
                        existingParam.Type = targetType;
                    if (existingParam.IsRequired !== extractedParam.isRequired) {
                        existingParam.IsRequired = extractedParam.isRequired;
                    if (existingParam.DefaultValue !== extractedParam.defaultValue) {
                        existingParam.DefaultValue = extractedParam.defaultValue;
                    if (existingParam.Description !== extractedParam.description) {
                        existingParam.Description = extractedParam.description;
                    if (existingParam.SampleValue !== extractedParam.sampleValue) {
                        existingParam.SampleValue = extractedParam.sampleValue;
                    if (existingParam.DetectionMethod !== 'AI') {
                        existingParam.DetectionMethod = 'AI';
                        promises.push(existingParam.Save());
            // Remove parameters that are no longer in the SQL
                promises.push(paramToRemove.Delete());
            if (promises.length > 0) {
            LogError(`Query "${this.Name}" - Failed to sync parameters:`, e);
    private async removeAllQueryParameters(): Promise<void> {
                // Get all existing query parameters
                const existingParams = existingParamsResult.Results || [];
                // Delete all existing parameters
                const deletePromises = existingParams.map(param => param.Delete());
                if (deletePromises.length > 0) {
                    await Promise.all(deletePromises);
            LogError('Failed to remove query parameters:', e);
     * Expands wildcard (*) entries in the extracted fields list.
     * When AI returns a field with sourceFieldName="*", it indicates a SELECT table.* pattern.
     * We expand this into individual field entries by looking up the entity's fields from metadata.
    private expandWildcardFields(extractedFields: ExtractedField[], md: IMetadataProvider): ExtractedField[] {
        const expandedFields: ExtractedField[] = [];
        for (const field of extractedFields) {
            // Check if this is a wildcard entry: sourceFieldName is "*" and sourceEntity is set
            if (field.sourceFieldName === '*' && field.sourceEntity) {
                // Look up the entity in metadata
                const sourceEntityInfo = md.Entities.find(e =>
                    e.Name.toLowerCase() === field.sourceEntity!.toLowerCase()
                if (sourceEntityInfo) {
                    // Expand the wildcard into individual fields from the entity
                    for (const entityField of sourceEntityInfo.Fields) {
                        // Map SQL type to our simplified type system
                        let fieldType: 'number' | 'string' | 'date' | 'boolean' = 'string';
                        const sqlTypeLower = entityField.Type.toLowerCase();
                        if (sqlTypeLower.includes('int') || sqlTypeLower.includes('decimal') ||
                            sqlTypeLower.includes('numeric') || sqlTypeLower.includes('float') ||
                            sqlTypeLower.includes('real') || sqlTypeLower.includes('money')) {
                            fieldType = 'number';
                        } else if (sqlTypeLower.includes('date') || sqlTypeLower.includes('time')) {
                            fieldType = 'date';
                        } else if (sqlTypeLower.includes('bit')) {
                            fieldType = 'boolean';
                        expandedFields.push({
                            name: entityField.Name, // SQL Server returns original column names for *
                            description: entityField.Description || `${entityField.Name} field from ${field.sourceEntity}`,
                            optional: field.optional, // Inherit from the wildcard entry
                            sourceEntity: field.sourceEntity,
                            sourceFieldName: entityField.Name,
                            isComputed: false,
                            isSummary: false
                    // Entity not found in metadata - keep the original entry as-is
                    // but log a warning
                    console.warn(`Query "${this.Name}" - Could not expand wildcard for entity "${field.sourceEntity}" - entity not found in metadata`);
                    expandedFields.push(field);
                // Not a wildcard entry - keep as-is
        return expandedFields;
    private async syncQueryFields(extractedFields: ExtractedField[]): Promise<void> {
        // Expand any wildcard (*) entries before processing
        const fieldsToSync = this.expandWildcardFields(extractedFields, md);
            const existingFields: MJQueryFieldEntity[] = [];
                // Get existing query fields
                const existingFieldsResult = await rv.RunView<MJQueryFieldEntity>({
                if (!existingFieldsResult.Success) {
                    throw new Error(`Failed to load existing query fields: ${existingFieldsResult.ErrorMessage}`);
                existingFields.push(...existingFieldsResult.Results || []);
            // Convert field names to lowercase for comparison (using expanded fieldsToSync)
            const fieldNamesToSync = fieldsToSync.map(f => f.name.toLowerCase());
            // Find fields to add, update, or remove
            const fieldsToAdd = fieldsToSync.filter(f =>
                !existingFields.some(ef => ef.Name.toLowerCase() === f.name.toLowerCase())
            const fieldsToUpdate = existingFields.filter(ef =>
                fieldsToSync.some(f => f.name.toLowerCase() === ef.Name.toLowerCase())
            const fieldsToRemove = existingFields.filter(ef =>
                !fieldNamesToSync.includes(ef.Name.toLowerCase())
            // Add new fields
            for (let i = 0; i < fieldsToAdd.length; i++) {
                const field = fieldsToAdd[i];
                const newField = await md.GetEntityObject<MJQueryFieldEntity>('MJ: Query Fields', this.ContextCurrentUser);
                newField.QueryID = this.ID;
                newField.Name = field.name;
                newField.Description = field.description;
                newField.Sequence = i + 1;
                // Map type to SQL types
                        newField.SQLBaseType = 'decimal';
                        newField.SQLFullType = 'decimal(18,2)';
                        newField.SQLBaseType = 'datetime';
                        newField.SQLFullType = 'datetime';
                        newField.SQLBaseType = 'bit';
                        newField.SQLFullType = 'bit';
                        newField.SQLFullType = 'nvarchar(MAX)';
                // Set computed/summary flags
                newField.IsComputed = field.isComputed || field.dynamicName || false;
                newField.IsSummary = field.isSummary || false;
                if (field.computationDescription) {
                    newField.ComputationDescription = field.computationDescription;
                // Set source entity tracking
                if (field.sourceEntity) {
                    // Look up entity ID from entity name
                        newField.SourceEntityID = sourceEntityInfo.ID;
                newField.SourceFieldName = field.sourceFieldName || (field.dynamicName ? field.name : null);
                promises.push(newField.Save());
            // Update existing fields if properties changed
            for (const existingField of fieldsToUpdate) {
                const extractedField = fieldsToSync.find(f => f.name.toLowerCase() === existingField.Name.toLowerCase());
                if (extractedField) {
                    if (existingField.Description !== extractedField.description) {
                        existingField.Description = extractedField.description;
                    const newIsComputed = extractedField.isComputed || extractedField.dynamicName || false;
                    if (existingField.IsComputed !== newIsComputed) {
                        existingField.IsComputed = newIsComputed;
                    const newIsSummary = extractedField.isSummary || false;
                    if (existingField.IsSummary !== newIsSummary) {
                        existingField.IsSummary = newIsSummary;
                    if (extractedField.computationDescription && existingField.ComputationDescription !== extractedField.computationDescription) {
                        existingField.ComputationDescription = extractedField.computationDescription;
                    // Update source entity tracking
                    if (extractedField.sourceEntity) {
                            e.Name.toLowerCase() === extractedField.sourceEntity!.toLowerCase()
                        if (sourceEntityInfo && existingField.SourceEntityID !== sourceEntityInfo.ID) {
                            existingField.SourceEntityID = sourceEntityInfo.ID;
                    } else if (existingField.SourceEntityID != null) {
                        existingField.SourceEntityID = null;
                    const newSourceFieldName = extractedField.sourceFieldName || (extractedField.dynamicName ? extractedField.name : null);
                    if (existingField.SourceFieldName !== newSourceFieldName) {
                        existingField.SourceFieldName = newSourceFieldName;
                    if (existingField.Sequence !== fieldsToSync.indexOf(extractedField) + 1) {
                        existingField.Sequence = fieldsToSync.indexOf(extractedField) + 1;
                        promises.push(existingField.Save());
            // Remove fields that are no longer in the SQL
            for (const fieldToRemove of fieldsToRemove) {
                promises.push(fieldToRemove.Delete());
            LogError(`Query "${this.Name}" - Failed to sync fields:`, e);
    private async syncQueryEntities(extractedEntities: Array<{
    }>): Promise<void> {
            // Get existing query entities
            const existingEntities: MJQueryEntityEntity[] = [];
                const existingEntitiesResult = await rv.RunView<MJQueryEntityEntity>({
                if (!existingEntitiesResult.Success) {
                    throw new Error(`Failed to load existing query entities: ${existingEntitiesResult.ErrorMessage}`);
                existingEntities.push(...existingEntitiesResult.Results || []);
            // Look up MJ entity IDs for the extracted entities using pre-loaded metadata
            // Since extractEntityMetadataFromSQL already matched entities, we just need to find them by name
            const entityMappings = extractedEntities.map(extracted => {
                // Find matching entity in metadata by name (already matched during extraction)
                    e.Name === extracted.name &&
                    e.SchemaName.toLowerCase() === extracted.schemaName.toLowerCase()
                        extracted,
                        entityID: matchingEntity.ID,
                        entityName: matchingEntity.Name
            }).filter(m => m !== null);
            // Find entities to add or remove
            const entitiesToAdd = entityMappings.filter(mapping => 
                !existingEntities.some(ee => ee.EntityID === mapping!.entityID)
            const entitiesToRemove = existingEntities.filter(ee =>
                !entityMappings.some(mapping => mapping!.entityID === ee.EntityID)
            // Add new query entity relationships
            for (const mapping of entitiesToAdd) {
                if (mapping) {
                    const newEntity = await md.GetEntityObject<MJQueryEntityEntity>('MJ: Query Entities', this.ContextCurrentUser);
                    newEntity.QueryID = this.ID;
                    newEntity.EntityID = mapping.entityID;
                    newEntity.DetectionMethod = 'AI'; // Using 'AI' as it's the closest match to automated detection
                    newEntity.AutoDetectConfidenceScore = 1.0; // 100% confidence since we're using deterministic SQL parsing
                    promises.push(newEntity.Save());
            // Remove entities that are no longer in the SQL
            for (const entityToRemove of entitiesToRemove) {
                promises.push(entityToRemove.Delete());
            LogError(`Query "${this.Name}" - Failed to sync entities:`, e);
    private async removeAllQueryFields(): Promise<void> {
            if (!this.IsSaved) return; // Nothing to remove if not saved
            const existingFields = existingFieldsResult.Results || [];
            const deletePromises = existingFields.map(field => field.Delete());
            LogError('Failed to remove query fields:', e);
    private async removeAllQueryEntities(): Promise<void> {
            const existingEntities = existingEntitiesResult.Results || [];
            const deletePromises = existingEntities.map(entity => entity.Delete());
            LogError('Failed to remove query entities:', e);
        const result = await super.Load(ID, EntityRelationshipsToLoad);        
        await this.RefreshRelatedMetadata(false);
     * Refreshes this record's related metadata from the provider, refreshing
     * all the way up from the database if refreshFromDB is true, otherwise from
     * cache.
     * @param refreshFromDB 
    public async RefreshRelatedMetadata(refreshFromDB: boolean) {
        if (refreshFromDB) {
            const globalMetadataProvider = Metadata.Provider;
            await globalMetadataProvider.Refresh(md); // we pass in our metadata provider because that is the connection we want to use if we are in the midst of a transaction
            if (globalMetadataProvider !== md) {
                // If the global metadata provider is different, we need to refresh it
                await md.Refresh(); // will refresh FROM the global provider, meaning we do NOT hit the DB again, we just copy the data into our MD instance that is part of our trans scope
        this._queryPermissions = md.QueryPermissions.filter(p => p.QueryID === this.ID);
        this._queryEntities = md.QueryEntities.filter(e => e.QueryID === this.ID);
        this._queryFields = md.QueryFields.filter(f => f.QueryID === this.ID);
        this._queryParameters = md.QueryParameters.filter(p => p.QueryID === this.ID);
