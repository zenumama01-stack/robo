import { StorageListResult } from "@memberjunction/storage";
 * Action for listing objects (files and directories) in a storage account path.
 * Uses the enterprise credential model where storage accounts are configured
 * with credentials managed at the organization level.
 *   ActionName: 'File Storage: List Objects',
 *     Value: 'My Google Drive'
 *     Name: 'Path',
 *     Value: 'documents/'
 *     Value: '/'
@RegisterClass(BaseAction, "File Storage: List Objects")
export class ListObjectsAction extends BaseFileStorageAction {
     * List objects in a storage account path
     *   - StorageAccount: Required - Name of the storage account
     *   - Path: Optional - Directory path to list (default: "/")
     *   - Delimiter: Optional - Path delimiter (default: "/")
     *   - Objects: Array of file objects
     *   - Prefixes: Array of directory prefixes
     *   - ObjectCount: Number of objects
     *   - DirectoryCount: Number of directories
            // Get optional parameters with defaults
            const path = this.getStringParamWithDefault(params, 'path', '/');
            const delimiter = this.getStringParamWithDefault(params, 'delimiter', '/');
            // Execute the list operation
            const result: StorageListResult = await driver!.ListObjects(path, delimiter);
            this.addOutputParam(params, 'Objects', result.objects);
            this.addOutputParam(params, 'Prefixes', result.prefixes);
            this.addOutputParam(params, 'ObjectCount', result.objects.length);
            this.addOutputParam(params, 'DirectoryCount', result.prefixes.length);
                operation: 'ListObjects',
                objectCount: result.objects.length,
                directoryCount: result.prefixes.length,
                objects: result.objects,
                prefixes: result.prefixes
                `List objects operation failed: ${error instanceof Error ? error.message : String(error)}`,
