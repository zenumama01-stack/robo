import { BaseFileStorageAction } from "./base-file-storage.action";
 * Action for copying a file object within a storage provider.
 *   ActionName: 'Copy Storage Object',
 *     Name: 'StorageAccount',
 *     Value: 'Azure Blob Storage'
 *     Name: 'SourceObjectName',
 *     Value: 'drafts/document.pdf'
 *     Name: 'DestinationObjectName',
 *     Value: 'archive/document.pdf'
@RegisterClass(BaseAction, "File Storage: Copy Object")
export class CopyObjectAction extends BaseFileStorageAction {
     * Copy a file object
     * @param params - The action parameters:
     *   - StorageAccount: Required - Name of the storage provider
     *   - SourceObjectName: Required - Source file path
     *   - DestinationObjectName: Required - Destination file path
     * @returns Operation result with:
     *   - Success: Boolean indicating if the copy succeeded
            // Get and initialize storage driver
            const { driver, error } = await this.getDriverFromParams(params);
            if (error) return error;
            // Get required parameters
            const sourceObjectName = this.getStringParam(params, 'sourceobjectname');
            if (!sourceObjectName) {
                    "SourceObjectName parameter is required",
                    "MISSING_SOURCE"
            const destinationObjectName = this.getStringParam(params, 'destinationobjectname');
            if (!destinationObjectName) {
                    "DestinationObjectName parameter is required",
                    "MISSING_DESTINATION"
            // Execute the copy operation
            const success: boolean = await driver!.CopyObject(sourceObjectName, destinationObjectName);
            // Add output parameter
            this.addOutputParam(params, 'Success', success);
            return this.createSuccessResult({
                operation: 'CopyObject',
                sourceObjectName,
                destinationObjectName,
                `Copy object operation failed: ${error instanceof Error ? error.message : String(error)}`,
                "OPERATION_FAILED"
