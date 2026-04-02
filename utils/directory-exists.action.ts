 * Action for checking if a directory exists in a storage provider.
 *   ActionName: 'Check Storage Directory Exists',
@RegisterClass(BaseAction, "File Storage: Check Directory Exists")
export class DirectoryExistsAction extends BaseFileStorageAction {
     * Check if a directory exists
     *   - DirectoryPath: Required - Path to the directory
     *   - Exists: Boolean indicating if the directory exists
            // Execute the directory exists operation
            const exists: boolean = await driver!.DirectoryExists(directoryPath);
            this.addOutputParam(params, 'Exists', exists);
                operation: 'DirectoryExists',
                exists
                `Directory exists check failed: ${error instanceof Error ? error.message : String(error)}`,
