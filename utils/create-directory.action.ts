 * Action for creating a directory in a storage provider.
 *   ActionName: 'Create Storage Directory',
 *     Value: 'Google Drive'
 *     Name: 'DirectoryPath',
 *     Value: 'projects/2025/'
@RegisterClass(BaseAction, "File Storage: Create Directory")
export class CreateDirectoryAction extends BaseFileStorageAction {
     * Create a directory
     *   - DirectoryPath: Required - Path for the new directory
     *   - Success: Boolean indicating if the directory was created
            // Get required parameter
            const directoryPath = this.getStringParam(params, 'directorypath');
            if (!directoryPath) {
                    "DirectoryPath parameter is required",
                    "MISSING_DIRECTORYPATH"
            // Execute the create directory operation
            const success: boolean = await driver!.CreateDirectory(directoryPath);
                operation: 'CreateDirectory',
                directoryPath,
                `Create directory operation failed: ${error instanceof Error ? error.message : String(error)}`,
