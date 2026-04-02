 * Action for checking if a file object exists in a storage provider.
 *   ActionName: 'Check Storage Object Exists',
 *     Value: 'reports/monthly.xlsx'
@RegisterClass(BaseAction, "File Storage: Check Object Exists")
export class ObjectExistsAction extends BaseFileStorageAction {
     * Check if a file object exists
     *   - Exists: Boolean indicating if the object exists
            // Execute the object exists operation
            const exists: boolean = await driver!.ObjectExists(objectName);
                operation: 'ObjectExists',
                `Object exists check failed: ${error instanceof Error ? error.message : String(error)}`,
