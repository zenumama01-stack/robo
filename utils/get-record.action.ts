 * Generic action for retrieving entity records from the database.
 * This action provides a flexible way to fetch records for any entity type
 * // Get a single record by ID
 *   ActionName: 'Get Record',
 * // Get a record with composite primary key
@RegisterClass(BaseAction, "GetRecordAction")
export class GetRecordAction extends BaseRecordMutationAction {
     * Retrieves a record for the specified entity type using its primary key.
     *   - EntityName (required): The name of the entity to retrieve
     *   - Success: true if record was retrieved successfully
     *   - ResultCode: SUCCESS, FAILED, ENTITY_NOT_FOUND, RECORD_NOT_FOUND, VALIDATION_ERROR, PERMISSION_DENIED
     *   - Params: Output parameter 'Record' contains the retrieved entity data
            // Load the record
            if (!loadResult.success) {
                return loadResult.error!;
            // Get all field values
            const recordData = entity!.GetAll();
            // Add output parameter with the record data
                Name: 'Record',
                Value: recordData,
                Message: `Successfully retrieved ${entityName} record`,
            return this.handleError(e, 'retrieving');
