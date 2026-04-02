 * Action that extracts content from PDF files
 * Can extract text, metadata, or specific pages
 * // Extract all text from PDF
 *   ActionName: 'PDF Extractor',
 *     Value: 'https://example.com/document.pdf'
 *     Name: 'ExtractType',
 * // Extract metadata from PDF
 *     Name: 'PDFData',
 *     Value: base64PdfData
 *     Value: 'metadata'
 * // Extract specific pages
 *     Value: 'uuid-of-pdf-file'
 *     Value: 'pages'
 *     Name: 'PageNumbers',
 *     Value: [1, 3, 5]
@RegisterClass(BaseAction, "PDF Extractor")
export class PDFExtractorAction extends BaseFileHandlerAction {
     * Extracts content from PDF files
     *   - FileURL: URL of PDF file (optional)
     *   - PDFData: Base64 encoded PDF data (optional)
     *   - ExtractType: "text" | "metadata" | "pages" (default: "text")
     *   - PageNumbers: Array of page numbers to extract (for pages extraction)
     *   - MergePages: Boolean - merge text from all pages (default: true)
     *   - IncludePageBreaks: Boolean - add page break markers (default: false)
     * @returns Extracted content based on extraction type
            const fileContent = await this.getFileContent(params, 'pdfdata', 'fileid', 'fileurl');
            // Get extraction parameters
            const extractType = (this.getParamValue(params, 'extracttype') || 'text').toLowerCase();
            const pageNumbersParam = this.getParamValue(params, 'pagenumbers');
            const mergePages = this.getBooleanParam(params, 'mergepages', true);
            const includePageBreaks = this.getBooleanParam(params, 'includepagebreaks', false);
            // Convert to Buffer if base64
            let pdfBuffer: Buffer;
                if (fileContent.source === 'direct' && this.isBase64(fileContent.content)) {
                    pdfBuffer = Buffer.from(fileContent.content, 'base64');
                    // Assume it's raw PDF data as string
                    pdfBuffer = Buffer.from(fileContent.content, 'binary');
                pdfBuffer = fileContent.content;
            // Parse PDF
            const pdfData = await pdfParse(pdfBuffer);
            switch (extractType) {
                case 'metadata':
                    result = await this.extractMetadata(pdfData);
                case 'pages':
                    result = await this.extractPages(pdfData, pageNumbersParam, mergePages, includePageBreaks);
                    result = await this.extractText(pdfData, mergePages, includePageBreaks);
            if (extractType === 'text' || extractType === 'pages') {
                    Name: 'ExtractedText',
                    Value: result.text
                    extractType: extractType,
                    ...result
                Message: `Failed to extract PDF content: ${error instanceof Error ? error.message : String(error)}`,
                ResultCode: "EXTRACTION_FAILED"
     * Extract text from PDF
    private async extractText(pdfData: any, mergePages: boolean, includePageBreaks: boolean): Promise<any> {
        let text = pdfData.text;
        if (includePageBreaks && pdfData.text) {
            // Add page break markers if requested
            const pages = pdfData.text.split('\n\n');
            text = pages.join('\n\n--- PAGE BREAK ---\n\n');
            totalPages: pdfData.numpages,
            textLength: text.length,
                title: pdfData.info?.Title,
                author: pdfData.info?.Author,
                subject: pdfData.info?.Subject,
                creator: pdfData.info?.Creator
     * Extract metadata from PDF
    private async extractMetadata(pdfData: any): Promise<any> {
                ...pdfData.info,
                pageCount: pdfData.numpages,
                version: pdfData.version,
                isEncrypted: pdfData.isEncrypted || false,
                permissions: pdfData.permissions || null
            documentInfo: {
                title: pdfData.info?.Title || null,
                author: pdfData.info?.Author || null,
                subject: pdfData.info?.Subject || null,
                keywords: pdfData.info?.Keywords || null,
                creator: pdfData.info?.Creator || null,
                producer: pdfData.info?.Producer || null,
                creationDate: pdfData.info?.CreationDate || null,
                modificationDate: pdfData.info?.ModDate || null
                totalTextLength: pdfData.text?.length || 0
     * Extract specific pages from PDF
    private async extractPages(pdfData: any, pageNumbersParam: any, mergePages: boolean, includePageBreaks: boolean): Promise<any> {
        // Parse page numbers
        let pageNumbers: number[] = [];
        if (pageNumbersParam) {
            if (Array.isArray(pageNumbersParam)) {
                pageNumbers = pageNumbersParam.map(n => parseInt(n.toString()));
            } else if (typeof pageNumbersParam === 'string') {
                // Support comma-separated list
                pageNumbers = pageNumbersParam.split(',').map(n => parseInt(n.trim()));
                pageNumbers = [parseInt(pageNumbersParam.toString())];
        // Validate page numbers
        pageNumbers = pageNumbers.filter(n => !isNaN(n) && n > 0 && n <= pdfData.numpages);
        if (pageNumbers.length === 0) {
            throw new Error('No valid page numbers specified');
        // Note: pdf-parse doesn't support page-by-page extraction natively
        // This would require a more advanced PDF library like pdf-lib
        // For now, we'll return all text with a note about the limitation
            text: pdfData.text,
            requestedPages: pageNumbers,
            note: 'Page-specific extraction requires advanced PDF processing. Currently returning all text.',
            textLength: pdfData.text.length
