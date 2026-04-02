 * Action for deleting a directory from a storage provider.
 *   ActionName: 'Delete Storage Directory',
 *     Value: 'Dropbox'
 *     Value: 'old-projects/'
 *     Name: 'Recursive',
 *     Value: 'true'
@RegisterClass(BaseAction, "File Storage: Delete Directory")
export class DeleteDirectoryAction extends BaseFileStorageAction {
     * Delete a directory
     *   - DirectoryPath: Required - Path to the directory to delete
     *   - Recursive: Optional - Delete recursively (default: false)
     *   - Success: Boolean indicating if the directory was deleted
            // Get optional recursive parameter
            const recursive = this.getBooleanParam(params, 'recursive', false);
            // Execute the delete directory operation
            const success: boolean = await driver!.DeleteDirectory(directoryPath, recursive);
                operation: 'DeleteDirectory',
                recursive,
                `Delete directory operation failed: ${error instanceof Error ? error.message : String(error)}`,
