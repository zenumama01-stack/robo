import axios, { AxiosResponse } from 'axios';
import { FileContentProcessor } from '../../../shared/file-content-processor';
 * Action to download file content from Typeform using file URL
 * This action retrieves the actual file content from Typeform's file storage
 * using the file_url that comes in response data. It handles authentication
 * and returns the file content in various formats for processing.
 *   ActionName: 'Get Typeform File Content',
 *     Name: 'FileURL',
 *     Value: 'https://api.typeform.com/responses/files/abc123/download.pdf'
 *     Name: 'Format',
 *     Value: 'text'
 * Format Options:
 * - 'auto' (default): Intelligent content extraction based on file type
 * - 'text': Force text extraction when possible
 * - 'base64': Return raw base64 encoded content
 * - 'raw': Return raw binary content as base64
@RegisterClass(BaseAction, 'TypeformGetFileContentAction')
export class GetTypeformFileContentAction extends TypeformBaseAction {
        return 'Downloads file content from Typeform using the file URL from response data. Features intelligent content extraction for PDFs, Office documents, images, and text files. Supports auto, text, base64, and raw output formats.';
            const fileUrl = this.getParamValue(params.Params, 'FileURL');
            const format = this.getParamValue(params.Params, 'Format') || 'auto';
            if (!fileUrl) {
                    ResultCode: 'MISSING_FILE_URL',
                    Message: 'FileURL parameter is required'
            // Validate format parameter
            const validFormats = ['auto', 'text', 'base64', 'raw'];
                    ResultCode: 'INVALID_FORMAT',
                    Message: `Format must be one of: ${validFormats.join(', ')}`
            if (!apiToken) {
                    ResultCode: 'MISSING_API_TOKEN',
                    Message: 'Could not retrieve Typeform API token. Please check your company integration settings.'
            LogStatus(`Downloading file from Typeform: ${fileUrl}`);
            // Download file content with authentication
            const response: AxiosResponse<ArrayBuffer> = await axios.get(fileUrl, {
                    'Authorization': `Bearer ${apiToken}`,
                    'Accept': this.getAcceptHeader(format)
                responseType: 'arraybuffer',
                timeout: 30000 // 30 second timeout for file downloads
            // Extract file metadata from response headers
            const contentType = response.headers['content-type'] || 'application/octet-stream';
            const contentLength = response.headers['content-length'] || response.data.byteLength;
            const filename = FileContentProcessor.extractFilename(fileUrl, response.headers['content-disposition']);
            // Process file content using the helper
            const processResult = await FileContentProcessor.processContent(
                Buffer.from(response.data),
                contentType,
                    format: format as any,
                    includeWarnings: true,
                    maxFileSize: 50 * 1024 * 1024 // 50MB limit
            if (!processResult.success) {
                    ResultCode: 'PROCESSING_ERROR',
                    Message: `Failed to process file content: ${processResult.error}`
                    Name: 'Content',
                    Value: processResult.content
                    Name: 'ContentType',
                    Value: processResult.contentType
                    Name: 'ContentFormat',
                    Value: processResult.format
                    Name: 'Size',
                    Value: processResult.size
                    Name: 'Filename',
                    Value: filename
                    Name: 'ExtractionMethod',
                    Value: processResult.extractionMethod
            // Add warning if present
            if (processResult.warning) {
                    Name: 'Warning',
                    Value: processResult.warning
                Message: `Successfully downloaded file ${filename} (${contentLength} bytes) from Typeform`
            LogError('Failed to download Typeform file:', error);
            // Handle specific error cases
                const axiosError = error as any;
                        ResultCode: 'UNAUTHORIZED',
                        Message: 'Invalid Typeform API token or insufficient permissions to access file'
                        ResultCode: 'FILE_NOT_FOUND',
                        Message: 'File not found or has been removed from Typeform'
                } else if (status === 413) {
                        ResultCode: 'FILE_TOO_LARGE',
                        Message: 'File is too large to download (Typeform limit: 256MB)'
                        ResultCode: 'RATE_LIMITED',
                        Message: 'Typeform API rate limit exceeded. Please try again later.'
                Message: this.buildFormErrorMessage('Get Typeform File Content', errorMessage, error)
     * Get appropriate Accept header based on requested format
    private getAcceptHeader(format: string): string {
                return 'text/*, application/*, */*';
            case 'raw':
            case 'base64':
                return 'application/octet-stream, */*';
            case 'auto':
                return '*/*';
                Name: 'FileURL',
                Name: 'Format',
                Value: 'auto', // Options: auto, text, base64, raw
 * Smart file content retrieval action that automatically handles content extraction
 * based on file type. Returns LLM-ready content (text for documents, base64 for images).
 * This action combines file download with intelligent content extraction:
 * - Text formats (txt, md, html, json, csv) → Returns plain text
 * - Images (png, jpg, gif, webp) → Returns base64 for LLM vision
 * - PDFs → Extracts text using pdf-parse
 * - Excel files → Converts to structured JSON/CSV text
 * - Word documents → Extracts text using mammoth
 * - Binary files → Returns base64 with warning
 * // Get PDF content (auto-extracts text)
 *   ActionName: 'File Storage: Get File Content',
 *     Value: 'Box'
 *     Name: 'ObjectID',
 *     Value: '347751234567'
 * // Returns: { Format: "text", Content: "extracted text...", ExtractionMethod: "pdf-parse" }
 * // Get image content (returns base64)
 *     Value: 'screenshots/diagram.png'
 * // Returns: { Format: "image", Content: "base64...", ExtractionMethod: "none" }
@RegisterClass(BaseAction, "File Storage: Get File Content")
export class GetFileContentAction extends BaseFileStorageAction {
     * Retrieve file content with automatic extraction based on file type
     *   - ObjectName: Required if ObjectID not provided - Name/path of the object
     *   - ObjectID: Optional - Provider-specific object ID (bypasses path resolution)
     *   - ContentType: Original MIME type of the file
     *   - Format: "text" | "image" | "structured" | "binary"
     *   - Content: Extracted/converted content ready for LLM
     *   - Size: Original file size in bytes
     *   - ExtractionMethod: Method used ("none" | "pdf-parse" | "excel-parse" | "word-extract")
     *   - Warning: Optional warning for unsupported formats
            // Get identifier (prefer ObjectID for performance)
            const objectId = this.getStringParam(params, 'objectid');
            if (!objectId && !objectName) {
                    "Either ObjectName or ObjectID parameter is required",
                    "MISSING_IDENTIFIER"
            // Get file metadata to determine content type
            const metadata = await driver!.GetObjectMetadata({
                objectId: objectId || undefined,
                fullPath: objectName || undefined
            // Download file content
            const buffer = await driver!.GetObject({
            // Route based on content type
            const contentType = metadata.contentType.toLowerCase();
            // Check file type and extract accordingly
            if (this.isImage(contentType)) {
                format = "image";
                extractionMethod = "none";
            } else if (this.isTextFormat(contentType)) {
                format = "text";
            } else if (this.isPDF(contentType)) {
                extractionMethod = "pdf-parse";
            } else if (this.isExcel(contentType)) {
                workbook.eachSheet((worksheet, sheetId) => {
                    worksheet.eachRow((row, rowNumber) => {
                        row.eachCell((cell, colNumber) => {
                format = "structured";
                extractionMethod = "excel-parse";
            } else if (this.isWord(contentType)) {
                extractionMethod = "word-extract";
                format = "binary";
                { Name: 'ContentType', Value: metadata.contentType, Type: 'Output' },
                { Name: 'Format', Value: format, Type: 'Output' },
                { Name: 'Content', Value: content, Type: 'Output' },
                { Name: 'Size', Value: metadata.size, Type: 'Output' },
                { Name: 'ExtractionMethod', Value: extractionMethod, Type: 'Output' }
            if (warning) {
                outputParams.push({ Name: 'Warning', Value: warning, Type: 'Output' });
                Message: `Retrieved ${metadata.name} (${format} format, ${extractionMethod !== 'none' ? 'extracted using ' + extractionMethod : 'no extraction needed'})`,
                `Get file content failed: ${error instanceof Error ? error.message : String(error)}`,
    private isImage(contentType: string): boolean {
    private isTextFormat(contentType: string): boolean {
            'application/typescript'
    private isPDF(contentType: string): boolean {
    private isExcel(contentType: string): boolean {
               contentType === 'application/vnd.ms-excel'; // xls
    private isWord(contentType: string): boolean {
               contentType === 'application/msword'; // doc
