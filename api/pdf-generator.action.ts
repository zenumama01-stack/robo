import PDFDocument from "pdfkit";
import { marked } from "marked";
 * Action that generates PDF files from HTML or Markdown content
 * Supports various formatting options and can save to MJ Storage
 * // Generate PDF from Markdown
 *   ActionName: 'PDF Generator',
 *     Name: 'Content',
 *     Value: '# Report Title\n\nThis is the report content...'
 *     Name: 'ContentType',
 *     Value: 'markdown'
 * // Generate PDF from HTML with custom options
 *     Value: '<h1>Report</h1><p>Content here...</p>'
 *     Name: 'Options',
 *       margin: { top: 50, bottom: 50, left: 72, right: 72 },
 *       fontSize: 12,
 *       font: 'Helvetica'
@RegisterClass(BaseAction, "PDF Generator")
export class PDFGeneratorAction extends BaseFileHandlerAction {
     * Generates a PDF from HTML or Markdown content
     *   - Content: HTML or Markdown string to convert to PDF
     *   - ContentType: "html" | "markdown" (default: "html")
     *   - Options: PDF generation options object containing:
     *     - margin: { top, bottom, left, right } in points (default: 72)
     *     - fontSize: Base font size (default: 12)
     *     - font: Font family (default: 'Helvetica')
     *     - orientation: 'portrait' | 'landscape' (default: 'portrait')
     *     - size: Paper size like 'A4', 'Letter', etc. (default: 'Letter')
     *   - FileName: Name for the generated PDF (default: 'document.pdf')
     * @returns Base64 encoded PDF data or FileID if saved to storage
            const content = this.getParamValue(params, 'content');
            const contentType = (this.getParamValue(params, 'contenttype') || 'html').toLowerCase();
            const fileName = this.getParamValue(params, 'filename') || 'document.pdf';
                    Message: "Content parameter is required",
            // Parse options
            let options: any = {
                margin: { top: 72, bottom: 72, left: 72, right: 72 },
                fontSize: 12,
                font: 'Helvetica',
                size: 'Letter'
            const customOptions = JSONParamHelper.getJSONParam(params, 'options');
            if (customOptions) {
                options = { ...options, ...customOptions };
            // Convert content if needed
            let htmlContent = content.toString();
            if (contentType === 'markdown') {
                // Convert markdown to HTML
                htmlContent = await marked(htmlContent);
            // Generate PDF
            const pdfBuffer = await this.generatePDF(htmlContent, options);
                        pdfBuffer,
                        'application/pdf',
                            message: "PDF generated and saved successfully",
                            sizeBytes: pdfBuffer.length
                    // If save fails, still return the PDF data
            const base64Data = pdfBuffer.toString('base64');
                Name: 'PDFData',
                    message: "PDF generated successfully",
                    sizeBytes: pdfBuffer.length,
                Message: `Failed to generate PDF: ${error instanceof Error ? error.message : String(error)}`,
     * Generate PDF from HTML content
    private async generatePDF(htmlContent: string, options: any): Promise<Buffer> {
                // Create PDF document
                const doc = new PDFDocument({
                    layout: options.orientation,
                    margins: options.margin,
                        Title: 'Generated Document',
                        Author: 'MemberJunction',
                        Creator: 'PDF Generator Action'
                // Collect PDF data
                doc.on('data', (chunk) => chunks.push(chunk));
                doc.on('end', () => resolve(Buffer.concat(chunks as unknown as Uint8Array[])));
                doc.on('error', reject);
                // Set default font
                doc.font(options.font || 'Helvetica');
                doc.fontSize(options.fontSize || 12);
                // Parse and render HTML (simplified - real implementation would need proper HTML parsing)
                const lines = this.parseHTMLToLines(htmlContent);
                    if (line.type === 'heading') {
                        doc.fontSize(line.size || 16).text(line.text, { 
                            align: line.align || 'left',
                            continued: false 
                    } else if (line.type === 'paragraph') {
                        doc.text(line.text, { 
                    } else if (line.type === 'list') {
                        doc.list([line.text], { 
                            bulletRadius: 2,
                            textIndent: 20,
                            bulletIndent: 10
                    // Add spacing between elements
                    doc.moveDown(0.5);
                // Finalize PDF
                doc.end();
     * Simple HTML parser (for demonstration - real implementation would use proper HTML parser)
    private parseHTMLToLines(html: string): any[] {
        const lines: any[] = [];
        // Remove HTML tags but preserve structure
        const text = html
            .replace(/<h1[^>]*>(.*?)<\/h1>/gi, (match, content) => {
                lines.push({ type: 'heading', text: content.trim(), size: 24 });
            .replace(/<h2[^>]*>(.*?)<\/h2>/gi, (match, content) => {
                lines.push({ type: 'heading', text: content.trim(), size: 20 });
            .replace(/<h3[^>]*>(.*?)<\/h3>/gi, (match, content) => {
                lines.push({ type: 'heading', text: content.trim(), size: 16 });
            .replace(/<p[^>]*>(.*?)<\/p>/gi, (match, content) => {
                lines.push({ type: 'paragraph', text: content.trim() });
            .replace(/<li[^>]*>(.*?)<\/li>/gi, (match, content) => {
                lines.push({ type: 'list', text: content.trim() });
            .replace(/<br\s*\/?>/gi, '\n')
            .replace(/<[^>]+>/g, '');
        // Add any remaining text
        const remainingText = text.trim();
        if (remainingText) {
            lines.push({ type: 'paragraph', text: remainingText });
