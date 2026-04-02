    BaseEntity, CompositeKey, EntityDeleteOptions, EntityPermissionType, 
    EntitySaveOptions, LogError, Metadata, UserInfo 
 * Entity CRUD Implementation Functions for REST endpoints
 * These functions contain the detailed implementation for entity operations
export class EntityCRUDHandler {
     * Create a new entity
    static async createEntity(entityName: string, data: any, user: UserInfo): Promise<{ success: boolean, entity?: any, error?: string, details?: any, validationErrors?: any[] }> {
            // Get entity object
            const entity = await md.GetEntityObject(entityName, user);
            if (!entity.CheckPermissions(EntityPermissionType.Create, false)) {
                    error: `User ${user.Name} does not have permission to create ${entityName} records` 
            // Extract save options if provided
            const options = new EntitySaveOptions();
            if (data.options) {
                const { IgnoreDirtyState, SkipEntityAIActions, SkipEntityActions, ReplayOnly, SkipOldValuesCheck } = data.options;
                if (IgnoreDirtyState !== undefined) options.IgnoreDirtyState = !!IgnoreDirtyState;
                if (SkipEntityAIActions !== undefined) options.SkipEntityAIActions = !!SkipEntityAIActions;
                if (SkipEntityActions !== undefined) options.SkipEntityActions = !!SkipEntityActions;
                if (ReplayOnly !== undefined) options.ReplayOnly = !!ReplayOnly;
                if (SkipOldValuesCheck !== undefined) options.SkipOldValuesCheck = !!SkipOldValuesCheck;
                // Remove options from data
                delete data.options;
            // Set values on the entity
            for (const key in data) {
                entity.Set(key, data[key]);
            // Validate entity
            const validationResult = entity.Validate();
                    error: 'Validation failed',
                    validationErrors: validationResult.Errors
            // Save entity
            const saveSuccess = await entity.Save(options);
            if (!saveSuccess) {
                    error: latestResult?.Message || 'Failed to create entity',
                    details: latestResult
            // Get entity data for response
            const entityData = await entity.GetDataObject();
            return { success: true, entity: entityData };
            return { success: false, error: error?.message || 'Unknown error' };
     * Read an entity by ID
    static async getEntity(entityName: string, id: string | number, relatedEntities: string[] = null, user: UserInfo): Promise<{ success: boolean, entity?: any, error?: string }> {
            if (!entity.CheckPermissions(EntityPermissionType.Read, false)) {
                    error: `User ${user.Name} does not have permission to read ${entityName} records` 
            // Create composite key
            const compositeKey = this.createCompositeKeyFromId(entity, id);
            const loadSuccess = await entity.InnerLoad(compositeKey, relatedEntities);
            if (!loadSuccess) {
                    error: `${entityName} with ID ${id} not found` 
     * Update an existing entity
    static async updateEntity(entityName: string, id: string | number, data: any, user: UserInfo): Promise<{ success: boolean, entity?: any, error?: string, details?: any, validationErrors?: any[] }> {
            if (!entity.CheckPermissions(EntityPermissionType.Update, false)) {
                    error: `User ${user.Name} does not have permission to update ${entityName} records` 
            const loadSuccess = await entity.InnerLoad(compositeKey);
            // Update entity with new values
            // Check if entity is dirty
            if (!entity.Dirty && !options.IgnoreDirtyState) {
                // Nothing changed, return success
                    error: latestResult?.Message || 'Failed to update entity',
    static async deleteEntity(entityName: string, id: string | number, options: EntityDeleteOptions, user: UserInfo): Promise<{ success: boolean, error?: string, details?: any }> {
            if (!entity.CheckPermissions(EntityPermissionType.Delete, false)) {
                    error: `User ${user.Name} does not have permission to delete ${entityName} records` 
            // Delete the entity
            const deleteSuccess = await entity.Delete(options);
            if (!deleteSuccess) {
                    error: latestResult?.Message || 'Failed to delete entity',
     * Helper method to create a composite key from an ID
    private static createCompositeKeyFromId(entity: BaseEntity, id: string | number): CompositeKey {
        if (entity.EntityInfo.PrimaryKeys.length === 1) {
            // Single primary key
            const primaryKeyField = entity.EntityInfo.PrimaryKeys[0].Name;
            const strId = id.toString();
            // Use key-value pairs instead of SetValue
            compositeKey.KeyValuePairs = [
                { FieldName: primaryKeyField, Value: strId }
            // Composite primary key - this is a simplification
            // In a real implementation, you would need to parse a composite ID string
            // or accept an object with all primary key values
            throw new Error('Composite primary keys are not supported in this simplified implementation');
