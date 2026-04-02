import { RunActionParams } from "@memberjunction/actions-base";
import { Metadata, RunView, BaseEntity } from "@memberjunction/core";
 * Base class for actions that handle file inputs from multiple sources
 * Provides common functionality for loading files from:
 * - MJ Storage (Document Libraries)
 * - URLs
 * - Direct data
export abstract class BaseFileHandlerAction extends BaseAction {
     * Get file content from various sources based on parameters
     * Priority: FileID > FileURL > Data parameter
     * @param params - Action parameters
     * @param dataParamName - Name of the parameter containing direct data
     * @param fileParamName - Name of the parameter containing file ID (default: 'FileID')
     * @param urlParamName - Name of the parameter containing file URL (default: 'FileURL')
     * @returns Object with content and metadata
    protected async getFileContent(
        dataParamName: string,
        fileParamName: string = 'FileID',
        urlParamName: string = 'FileURL'
        content: string | Buffer;
        source: 'storage' | 'url' | 'direct';
        // Check for FileID first (MJ Storage)
        const fileIdParam = params.Params.find(p => p.Name.trim().toLowerCase() === fileParamName.toLowerCase());
        if (fileIdParam?.Value) {
            return await this.loadFromMJStorage(fileIdParam.Value.toString(), params);
        // Check for FileURL
        const fileUrlParam = params.Params.find(p => p.Name.trim().toLowerCase() === urlParamName.toLowerCase());
        if (fileUrlParam?.Value) {
            return await this.loadFromURL(fileUrlParam.Value.toString());
        // Check for direct data
        const dataParam = params.Params.find(p => p.Name.trim().toLowerCase() === dataParamName.toLowerCase());
        if (dataParam?.Value) {
                content: dataParam.Value.toString(),
                source: 'direct'
        throw new Error(`No input provided. Please provide ${fileParamName}, ${urlParamName}, or ${dataParamName}`);
     * Load file from MJ Storage (Document Libraries)
    private async loadFromMJStorage(fileId: string, params: RunActionParams): Promise<{
        source: 'storage';
            const result = await rv.RunView<BaseEntity>({
                EntityName: 'Document Libraries',
                ExtraFilter: `ID = '${fileId}'`,
                throw new Error(`File not found in Document Libraries: ${fileId}`);
            const doc = result.Results[0];
            // Get the actual file content (this would need implementation based on your storage provider)
            // For now, returning the file info - actual implementation would fetch from storage
                content: '', // TODO: Implement actual file retrieval based on storage provider
                fileName: doc.Get('FileName'),
                mimeType: doc.Get('MimeType'),
                source: 'storage'
            throw new Error(`Failed to load file from storage: ${error instanceof Error ? error.message : String(error)}`);
     * Load file from URL
    private async loadFromURL(url: string): Promise<{
        source: 'url';
            const urlObj = new URL(url);
            if (!['http:', 'https:'].includes(urlObj.protocol)) {
                throw new Error('Only HTTP and HTTPS URLs are supported');
            const contentType = response.headers.get('content-type');
            const contentDisposition = response.headers.get('content-disposition');
            let fileName: string | undefined;
            // Extract filename from content-disposition if available
                const match = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
                if (match && match[1]) {
                    fileName = match[1].replace(/['"]/g, '');
            // If no filename from header, extract from URL
            if (!fileName) {
                fileName = urlObj.pathname.split('/').pop();
            const content = await response.text();
                mimeType: contentType || undefined,
                source: 'url'
            throw new Error(`Failed to load file from URL: ${error instanceof Error ? error.message : String(error)}`);
     * Save file to MJ Storage
    protected async saveToMJStorage(
        content: string | Buffer,
            const doc = await md.GetEntityObject<BaseEntity>('Document Libraries', params.ContextUser);
            if (!doc) {
                throw new Error('Failed to create Document Library entity');
            doc.Set('FileName', fileName);
            doc.Set('MimeType', mimeType);
            // TODO: Set appropriate fields based on your Document Libraries schema
            // doc.Set('FileSize', Buffer.byteLength(content));
            // doc.Set('StorageProvider', 'default');
            const saveResult = await doc.Save();
                throw new Error('Failed to save document to library');
            // TODO: Actually store the file content to the storage provider
            return doc.Get('ID');
            throw new Error(`Failed to save file to storage: ${error instanceof Error ? error.message : String(error)}`);
