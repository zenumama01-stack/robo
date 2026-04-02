 * Action for downloading file content from a storage provider.
 *   ActionName: 'Get Storage Object',
 *     Value: 'Google Cloud Storage'
 *     Value: 'data/export.json'
@RegisterClass(BaseAction, "File Storage: Get Object")
export class GetObjectAction extends BaseFileStorageAction {
     * Download file content from storage
     *   - Content: Base64-encoded file content
            // Execute the get object operation with new params structure
            const content: Buffer = await driver!.GetObject({
            // Convert buffer to base64 for transport
            const base64Content = content.toString('base64');
            this.addOutputParam(params, 'Content', base64Content);
            this.addOutputParam(params, 'Size', content.length);
                operation: 'GetObject',
                size: content.length,
                contentBase64: base64Content
                `Get object operation failed: ${error instanceof Error ? error.message : String(error)}`,
