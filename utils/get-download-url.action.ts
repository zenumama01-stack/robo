 * Action for generating a pre-authenticated download URL for a file.
 *   ActionName: 'Get Storage Download URL',
 *     Value: 'documents/contract.pdf'
@RegisterClass(BaseAction, "File Storage: Get Download URL")
export class GetDownloadUrlAction extends BaseFileStorageAction {
     * Generate a pre-authenticated download URL
     *   - ObjectName: Required - Name/path of the object
     *   - DownloadUrl: Pre-authenticated URL for downloading the file
            // Execute the create download URL operation
            const url: string = await driver!.CreatePreAuthDownloadUrl(objectName);
            this.addOutputParam(params, 'DownloadUrl', url);
                operation: 'GetDownloadUrl',
                downloadUrl: url
                `Get download URL operation failed: ${error instanceof Error ? error.message : String(error)}`,
