 * Action for generating a pre-authenticated upload URL for a file.
 *   ActionName: 'Get Storage Upload URL',
 *     Value: 'uploads/new-file.pdf'
@RegisterClass(BaseAction, "File Storage: Get Upload URL")
export class GetUploadUrlAction extends BaseFileStorageAction {
     * Generate a pre-authenticated upload URL
     *   - ObjectName: Required - Name/path of the object to upload
     *   - UploadUrl: Pre-authenticated URL for uploading the file
     *   - ProviderKey: Provider-specific key (if returned by provider)
            // Execute the create upload URL operation
            const result = await driver!.CreatePreAuthUploadUrl(objectName);
            this.addOutputParam(params, 'UploadUrl', result.UploadUrl);
            if (result.ProviderKey) {
                this.addOutputParam(params, 'ProviderKey', result.ProviderKey);
                operation: 'GetUploadUrl',
                uploadUrl: result.UploadUrl,
                providerKey: result.ProviderKey
                `Get upload URL operation failed: ${error instanceof Error ? error.message : String(error)}`,
