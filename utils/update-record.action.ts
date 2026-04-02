import { LogError } from "@memberjunction/core";
 * Generic action for updating existing entity records in the database.
 * This action provides a flexible way to update records for any entity type
 * by accepting the entity name, primary key, and field values as parameters.
 * // Update a customer record
 *   ActionName: 'Update Record',
 *         Email: 'newemail@example.com',
 *         Status: 'Inactive',
 *         UpdatedAt: new Date()
@RegisterClass(BaseAction, "UpdateRecordAction")
export class UpdateRecordAction extends BaseRecordMutationAction {
     * Updates an existing record for the specified entity type.
     *   - EntityName (required): The name of the entity to update
     *   - Fields (required): Object containing field names and values to update
     *   - Success: true if record was updated successfully
     *                 PERMISSION_DENIED, NO_CHANGES, CONCURRENT_UPDATE
     *   - Params: Output parameter 'UpdatedFields' contains the fields that were actually changed
            // Track which fields are actually being changed
            const updatedFields: Record<string, { oldValue: any, newValue: any }> = {};
            let hasChanges = false;
            // Set each field value
                if (fieldName in entity!) {
                    const currentValue = entity!.Get(fieldName);
                    if (currentValue !== fieldValue) {
                        entity!.Set(fieldName, fieldValue);
                        updatedFields[fieldName] = {
                            oldValue: currentValue,
                            newValue: fieldValue
                        hasChanges = true;
            // Check if there are any changes to save
            if (!hasChanges) {
                    Message: 'No fields were modified'
                // Add output parameter with the updated fields
                    Message: `Successfully updated ${entityName} record`,
                return this.analyzeEntityError(entity!, 'update', entityName);
            return this.handleError(e, 'updating');
