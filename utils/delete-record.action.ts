 * Generic action for deleting entity records from the database.
 * This action provides a flexible way to delete records for any entity type
 * by accepting the entity name and primary key values as parameters.
 * // Delete a single record by ID
 *   ActionName: 'Delete Record',
 *       Name: 'PrimaryKey',
 *       Value: { ID: '123e4567-e89b-12d3-a456-426614174000' }
 * // Delete a record with composite primary key
 *       Value: 'UserRoles'
 *         UserID: '123e4567-e89b-12d3-a456-426614174000',
 *         RoleID: '987f6543-e21b-12d3-a456-426614174000'
@RegisterClass(BaseAction, "DeleteRecordAction")
export class DeleteRecordAction extends BaseRecordMutationAction {
     * Deletes a record for the specified entity type using its primary key.
     *   - EntityName (required): The name of the entity to delete
     *   - PrimaryKey (required): Object containing primary key field(s) and value(s)
     *   - Success: true if record was deleted successfully
     *   - ResultCode: SUCCESS, FAILED, ENTITY_NOT_FOUND, RECORD_NOT_FOUND, VALIDATION_ERROR, 
     *                 PERMISSION_DENIED, CASCADE_CONSTRAINT, REFERENCE_CONSTRAINT
     *   - Params: Output parameter 'DeletedRecord' contains the data of the deleted record
            const primaryKeyResult = this.getPrimaryKeyParam(params);
            if (primaryKeyResult.error) return primaryKeyResult.error;
            const primaryKey = primaryKeyResult.value!;
            // Load the existing record
            const loadResult = await this.loadRecord(entity!, entityInfo!, primaryKey, entityName);
            if (!loadResult.success) return loadResult.error!;
            // Store the record data before deletion
            const deletedRecordData = entity.GetAll();
            // Delete the record
            const deleteResult = await entity.Delete();
                // Add output parameter with the deleted record data
                    Name: 'DeletedRecord',
                    Value: deletedRecordData,
                    Message: `Successfully deleted ${entityName} record`,
                return this.analyzeEntityError(entity!, 'delete', entityName);
            return this.handleError(e, 'deleting');
