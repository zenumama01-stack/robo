import { StorageObjectMetadata } from "@memberjunction/storage";
 * Action for retrieving metadata about a file object in a storage provider.
 *   ActionName: 'Get Storage Object Metadata',
 *     Value: 'AWS S3 Storage'
 *     Value: 'reports/annual-report.pdf'
@RegisterClass(BaseAction, "File Storage: Get Object Metadata")
export class GetMetadataAction extends BaseFileStorageAction {
     * Get metadata for a storage object
     *   - ObjectID: Optional - Provider-specific object ID (bypasses path resolution for faster access)
     * @returns Operation result with metadata fields:
     *   - Name: File name
     *   - Path: Directory path
     *   - FullPath: Complete path
     *   - Size: File size in bytes
     *   - ContentType: MIME type
     *   - LastModified: Last modified timestamp
     *   - IsDirectory: Whether object is a directory
     *   - ETag: Entity tag (if available)
     *   - CustomMetadata: Custom metadata key-value pairs (if available)
            // Get identifier (prefer ObjectID if provided for performance)
            // Execute the get metadata operation with new params structure
            const metadata: StorageObjectMetadata = await driver!.GetObjectMetadata({
            this.addOutputParam(params, 'Name', metadata.name);
            this.addOutputParam(params, 'Path', metadata.path);
            this.addOutputParam(params, 'FullPath', metadata.fullPath);
            this.addOutputParam(params, 'Size', metadata.size);
            this.addOutputParam(params, 'ContentType', metadata.contentType);
            this.addOutputParam(params, 'LastModified', metadata.lastModified.toISOString());
            this.addOutputParam(params, 'IsDirectory', metadata.isDirectory);
            if (metadata.etag) {
                this.addOutputParam(params, 'ETag', metadata.etag);
            if (metadata.customMetadata) {
                this.addOutputParam(params, 'CustomMetadata', metadata.customMetadata);
                operation: 'GetMetadata',
                metadata
                `Get metadata operation failed: ${error instanceof Error ? error.message : String(error)}`,
