import { METADATA_KEYWORDS, extractKeywordValue, createKeywordReference } from '../constants/metadata-keywords';
 * Handles externalization of field values to separate files with @file: references
export class FieldExternalizer {
   * Externalize a field value to a separate file and return @file: reference
  async externalizeField(
    fieldValue: any,
    pattern: string,
    recordData: BaseEntity,
    targetDir: string,
    existingFileReference?: string,
    mergeStrategy: string = 'merge',
    const { finalFilePath, fileReference } = this.determineFilePath(
      recordData, 
      existingFileReference, 
      mergeStrategy, 
      verbose
    const shouldWrite = await this.shouldWriteFile(finalFilePath, fieldValue, fieldName);
    if (shouldWrite) {
      await this.writeExternalFile(finalFilePath, fieldValue, fieldName, verbose);
      console.log(`External file ${finalFilePath} unchanged, skipping write`);
    return fileReference;
   * Determines the file path and reference for externalization
  private determineFilePath(
    fieldName: string = '',
  ): { finalFilePath: string; fileReference: string } {
    if (this.shouldUseExistingReference(existingFileReference, mergeStrategy)) {
      return this.useExistingFileReference(existingFileReference!, targetDir, verbose);
    return this.createNewFileReference(pattern, recordData, targetDir, fieldName, verbose);
   * Checks if we should use an existing file reference
  private shouldUseExistingReference(existingFileReference?: string, mergeStrategy: string = 'merge'): boolean {
    return mergeStrategy === 'merge' &&
           !!existingFileReference &&
           typeof existingFileReference === 'string' &&
           existingFileReference.startsWith(METADATA_KEYWORDS.FILE);
   * Uses an existing file reference
  private useExistingFileReference(
    existingFileReference: string,
    const existingPath = extractKeywordValue(existingFileReference) as string;
    const finalFilePath = path.resolve(targetDir, existingPath);
      console.log(`Using existing external file: ${finalFilePath}`);
    return { finalFilePath, fileReference: existingFileReference };
   * Creates a new file reference using the pattern
  private createNewFileReference(
    const processedPattern = this.processPattern(pattern, recordData, fieldName);
    const cleanPattern = this.removeFilePrefix(processedPattern);
    const finalFilePath = path.resolve(targetDir, cleanPattern);
    const fileReference = createKeywordReference('file', cleanPattern);
      console.log(`Creating new external file: ${finalFilePath}`);
    return { finalFilePath, fileReference };
   * Processes pattern placeholders with actual values
  private processPattern(pattern: string, recordData: BaseEntity, fieldName: string): string {
    let processedPattern = pattern;
    // Replace common placeholders
    processedPattern = this.replacePlaceholder(processedPattern, 'Name', (recordData as any).Name);
    processedPattern = this.replacePlaceholder(processedPattern, 'ID', (recordData as any).ID);
    processedPattern = this.replacePlaceholder(processedPattern, 'FieldName', fieldName);
    // Replace any other field placeholders
    processedPattern = this.replaceFieldPlaceholders(processedPattern, recordData);
    return processedPattern;
   * Replaces a single placeholder in the pattern
  private replacePlaceholder(pattern: string, placeholder: string, value: any): string {
    if (value != null) {
      const sanitizedValue = this.sanitizeForFilename(String(value));
      return pattern.replace(new RegExp(`\\{${placeholder}\\}`, 'g'), sanitizedValue);
   * Replaces field placeholders with values from the record
  private replaceFieldPlaceholders(pattern: string, recordData: BaseEntity): string {
    for (const [key, value] of Object.entries(recordData as any)) {
        processedPattern = processedPattern.replace(new RegExp(`\\{${key}\\}`, 'g'), sanitizedValue);
   * Removes @file: prefix if present
  private removeFilePrefix(pattern: string): string {
    return pattern.startsWith(METADATA_KEYWORDS.FILE) ? (extractKeywordValue(pattern) as string) : pattern;
   * Determines if the file should be written based on content comparison
  private async shouldWriteFile(finalFilePath: string, fieldValue: any, fieldName: string): Promise<boolean> {
    if (!(await fs.pathExists(finalFilePath))) {
      return true; // File doesn't exist, should write
      const existingContent = await fs.readFile(finalFilePath, 'utf8');
      const contentToWrite = this.prepareContentForWriting(fieldValue, fieldName);
      return existingContent !== contentToWrite;
      return true; // Error reading existing file, should write
   * Writes the external file with the field content
  private async writeExternalFile(
    finalFilePath: string, 
    // Ensure the directory exists
    await fs.ensureDir(path.dirname(finalFilePath));
    // Write the field value to the file
    await fs.writeFile(finalFilePath, contentToWrite, 'utf8');
      console.log(`Wrote externalized field ${fieldName} to ${finalFilePath}`);
   * Prepares content for writing, with JSON pretty-printing if applicable
  private prepareContentForWriting(fieldValue: any, fieldName: string): string {
    let contentToWrite = String(fieldValue);
    // If the value looks like JSON, try to pretty-print it
    if (this.shouldPrettyPrintAsJson(fieldName)) {
        const parsed = JSON.parse(contentToWrite);
        contentToWrite = JSON.stringify(parsed, null, 2);
        // Not valid JSON, use as-is
    return contentToWrite;
   * Determines if content should be pretty-printed as JSON
  private shouldPrettyPrintAsJson(fieldName: string): boolean {
    const lowerFieldName = fieldName.toLowerCase();
    return lowerFieldName.includes('json') || lowerFieldName.includes('example');
   * Sanitize a string for use in filenames
  private sanitizeForFilename(input: string): string {
    return input
      .replace(/\s+/g, '-') // Replace spaces with hyphens
      .replace(/[^a-z0-9.-]/g, '') // Remove special characters except dots and hyphens
      .replace(/--+/g, '-') // Replace multiple hyphens with single hyphen
      .replace(/^-+|-+$/g, ''); // Remove leading/trailing hyphens
