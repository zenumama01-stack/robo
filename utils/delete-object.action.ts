 * Action for deleting a file object from a storage provider.
 *   ActionName: 'Delete Storage Object',
 *     Value: 'SharePoint Storage'
 *     Name: 'ObjectName',
 *     Value: 'old-files/deprecated.doc'
@RegisterClass(BaseAction, "File Storage: Delete Object")
export class DeleteObjectAction extends BaseFileStorageAction {
     * Delete a file object
     *   - ObjectName: Required - Name/path of the object to delete
     *   - Success: Boolean indicating if the delete succeeded
            const objectName = this.getStringParam(params, 'objectname');
            if (!objectName) {
                    "ObjectName parameter is required",
                    "MISSING_OBJECTNAME"
            // Execute the delete operation
            const success: boolean = await driver!.DeleteObject(objectName);
                operation: 'DeleteObject',
                objectName,
                `Delete object operation failed: ${error instanceof Error ? error.message : String(error)}`,
