import archiver from "archiver";
import unzipper from "unzipper";
import { Readable } from "stream";
 * Action that compresses or decompresses files
 * Supports ZIP format with various compression levels
 * // Compress multiple files into ZIP
 *   ActionName: 'File Compress',
 *     Name: 'Operation',
 *     Value: 'compress'
 *     Name: 'Files',
 *       { name: 'report.pdf', data: pdfBase64Data },
 *       { name: 'data.csv', data: csvData }
 * // Decompress ZIP file
 *     Value: 'decompress'
 *     Value: 'https://example.com/archive.zip'
 * // Compress with options
 *     Value: filesArray
 *     Name: 'CompressionLevel',
 *     Value: 9
 *     Name: 'OutputFileID',
@RegisterClass(BaseAction, "File Compress")
export class FileCompressAction extends BaseFileHandlerAction {
     * Compresses or decompresses files
     *   - Operation: "compress" | "decompress" (required)
     *   - For compress:
     *     - Files: Array of file objects with { name: string, data: string/Buffer }
     *     - CompressionLevel: 0-9 (default: 6, 0=store only, 9=maximum compression)
     *     - Format: "zip" (currently only ZIP supported)
     *   - For decompress:
     *     - FileID/FileURL/Data: Input compressed file
     *   - OutputFileID: Save result to MJ Storage (optional)
     *   - FileName: Name for output file (default: 'archive.zip' or 'extracted_files.json')
     * @returns Compressed/decompressed data
            const operation = (this.getParamValue(params, 'operation') || '').toLowerCase();
            if (!['compress', 'decompress'].includes(operation)) {
                    Message: "Operation must be 'compress' or 'decompress'",
            if (operation === 'compress') {
                return await this.compress(params);
                return await this.decompress(params);
                Message: `Failed to ${this.getParamValue(params, 'operation')} files: ${error instanceof Error ? error.message : String(error)}`,
                ResultCode: "OPERATION_FAILED"
     * Compress files
    private async compress(params: RunActionParams): Promise<ActionResultSimple> {
        let files: any[];
            files = JSONParamHelper.getRequiredJSONParam(params, 'files');
                ResultCode: "MISSING_FILES"
        const compressionLevel = this.getNumericParam(params, 'compressionlevel', 6);
        const format = (this.getParamValue(params, 'format') || 'zip').toLowerCase();
        const fileName = this.getParamValue(params, 'filename') || 'archive.zip';
        if (!Array.isArray(files) || files.length === 0) {
                Message: "Files must be a non-empty array",
                ResultCode: "INVALID_FILES"
        // Validate compression level
        if (compressionLevel < 0 || compressionLevel > 9) {
                Message: "CompressionLevel must be between 0 and 9",
                ResultCode: "INVALID_COMPRESSION_LEVEL"
        // Currently only support ZIP
        if (format !== 'zip') {
                Message: "Currently only 'zip' format is supported",
                ResultCode: "UNSUPPORTED_FORMAT"
        // Create archive
        const archive = archiver('zip', {
            zlib: { level: compressionLevel }
        archive.on('data', (chunk) => chunks.push(chunk));
        // Add files to archive
            if (!file.name) {
                    Message: "Each file must have a 'name' property",
                    ResultCode: "INVALID_FILE_DEFINITION"
            let fileData: Buffer;
            if (typeof file.data === 'string') {
                // Check if base64
                if (this.isBase64(file.data)) {
                    fileData = Buffer.from(file.data, 'base64');
                    fileData = Buffer.from(file.data);
            } else if (Buffer.isBuffer(file.data)) {
                fileData = file.data;
                    Message: `Invalid data type for file '${file.name}'`,
                    ResultCode: "INVALID_FILE_DATA"
            archive.append(fileData, { name: file.name });
        // Finalize archive
        const buffer = Buffer.concat(chunks as unknown as Uint8Array[]);
                    'application/zip',
                    Name: 'CompressedFileID',
                        message: "Files compressed and saved successfully",
                        filesCompressed: files.length,
                        compressionLevel: compressionLevel,
                        compressedSize: buffer.length
            Name: 'CompressedData',
                message: "Files compressed successfully",
                compressedSize: buffer.length,
     * Decompress files
    private async decompress(params: RunActionParams): Promise<ActionResultSimple> {
        // Get compressed file content
        const fileContent = await this.getFileContent(params, 'data', 'fileid', 'fileurl');
        const fileName = this.getParamValue(params, 'filename') || 'extracted_files.json';
        // Convert to Buffer
        let zipBuffer: Buffer;
                zipBuffer = Buffer.from(fileContent.content, 'base64');
                zipBuffer = Buffer.from(fileContent.content, 'binary');
            zipBuffer = fileContent.content;
        // Extract files
        const extractedFiles: any[] = [];
            const directory = await unzipper.Open.buffer(zipBuffer);
            for (const file of directory.files) {
                if (!file.type || file.type === 'File') {
                    const content = await file.buffer();
                    extractedFiles.push({
                        name: file.path,
                        size: file.uncompressedSize,
                        data: content.toString('base64'),
                        compressed: file.compressedSize,
                        compressionMethod: file.compressionMethod
                Message: `Failed to decompress file: ${error instanceof Error ? error.message : String(error)}`,
                ResultCode: "DECOMPRESSION_FAILED"
            files: extractedFiles,
            totalFiles: extractedFiles.length,
            sourceFileName: fileContent.fileName
            Name: 'ExtractedFiles',
            Value: extractedFiles
     * Check if string is base64
