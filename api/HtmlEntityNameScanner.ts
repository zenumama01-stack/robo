 * Regex-based scanner/fixer for hardcoded entity names in Angular HTML templates.
 * Since HTML templates cannot be parsed with the TypeScript AST, this scanner
 * uses targeted regex patterns to find entity name references in:
 *   - Method calls in template expressions (event bindings, property bindings)
 *   - Static attribute values for entity-name-related attributes
 * Uses the same rename map as EntityNameScanner (built from entity_subclasses.ts).
 *   mj codegen fix-html-entity-names --path packages/Angular
 *   mj codegen fix-html-entity-names --path packages/Angular --fix
 *   import { scanHtmlEntityNames } from '@memberjunction/codegen-lib';
 *   const result = await scanHtmlEntityNames({ TargetPath: './packages/Angular' });
import { resolveEntityNameMap } from './EntityNameScanner';
/** The kind of HTML pattern where an old entity name was found. */
export type HtmlPatternKind =
    | 'methodCallSingleQuote'   // someMethod('EntityName', ...) in template expression
    | 'methodCallDoubleQuote'   // someMethod("EntityName", ...) in template expression
    | 'attributeValue';         // EntityName="EntityName" as static attribute
 * A single finding: one entity name reference in an HTML template that needs
 * the "MJ: " prefix.
export interface HtmlEntityNameFinding {
    /** The old entity name found */
    /** The corrected entity name */
    /** The kind of HTML pattern matched */
    PatternKind: HtmlPatternKind;
    /** Contextual description of the match */
    Context: string;
 * Options for the HTML entity name scanner.
export interface HtmlEntityNameScanOptions {
     * Path to entity_subclasses.ts for building the rename map.
     * If not provided, attempts to locate it relative to common workspace roots.
export interface HtmlEntityNameScanResult {
    Findings: HtmlEntityNameFinding[];
 * Method names that take an entity name as their first string argument
 * in Angular template expressions.
const HTML_ENTITY_NAME_METHODS = [
    'openEntityRecord',
 * HTML attribute names whose static string value is an entity name.
const HTML_ENTITY_NAME_ATTRIBUTES = [
    'RowsEntityName',
    'JoinEntityName',
// HTML File Scanner
 * Builds regex patterns for matching entity name references in HTML templates.
function buildMethodCallRegexes(): RegExp[] {
    const methodGroup = HTML_ENTITY_NAME_METHODS.join('|');
        // Single-quoted: someMethod('EntityName'  — in event/property bindings
        new RegExp(`(?:${methodGroup})\\s*\\(\\s*'([^']+)'`, 'g'),
        // Double-quoted inside single-quoted binding: someMethod("EntityName"
        // This handles cases like (click)="someMethod('EntityName')"
        // The outer quotes are double, inner are single — already caught above.
        // But also: [Prop]="someMethod('EntityName')" — same pattern.
function buildAttributeRegexes(): RegExp[] {
    const attrGroup = HTML_ENTITY_NAME_ATTRIBUTES.join('|');
        // Static attribute: RowsEntityName="Entity Name"
        new RegExp(`(?:${attrGroup})="([^"]+)"`, 'g'),
 * Gets the 1-based line number for a character offset in the source text.
function getLineNumber(sourceText: string, offset: number): number {
    for (let i = 0; i < offset && i < sourceText.length; i++) {
        if (sourceText[i] === '\n') line++;
 * Scans a single HTML file for entity name references that need the "MJ: " prefix.
export function scanHtmlFile(
): HtmlEntityNameFinding[] {
    const findings: HtmlEntityNameFinding[] = [];
    // Quick check: does this file contain any old entity name?
    // Scan method call patterns
    const methodRegexes = buildMethodCallRegexes();
    for (const regex of methodRegexes) {
        regex.lastIndex = 0;
        while ((match = regex.exec(sourceText)) !== null) {
            const entityName = match[1];
            if (renameMap.has(entityName)) {
                    Line: getLineNumber(sourceText, match.index),
                    OldName: entityName,
                    NewName: renameMap.get(entityName)!,
                    PatternKind: 'methodCallSingleQuote',
                    Context: match[0],
    // Scan attribute patterns
    const attrRegexes = buildAttributeRegexes();
    for (const regex of attrRegexes) {
                    PatternKind: 'attributeValue',
// HTML File Fixer
 * Applies entity name fixes to an HTML file using targeted string replacements.
export function fixHtmlFile(
    findings: HtmlEntityNameFinding[],
    for (const finding of findings) {
        // Replace old context with new context (old name swapped for new name)
        const oldContext = finding.Context;
        const newContext = oldContext.replace(finding.OldName, finding.NewName);
        result = replaceAll(result, oldContext, newContext);
/** Simple replaceAll using split/join for broad compatibility. */
function replaceAll(text: string, search: string, replacement: string): string {
    return text.split(search).join(replacement);
 * Scans HTML template files for hardcoded entity names that need the "MJ: " prefix,
 * and optionally fixes them in place.
export async function scanHtmlEntityNames(
    options: HtmlEntityNameScanOptions,
): Promise<HtmlEntityNameScanResult> {
    // Build rename map (tries .ts file first, falls back to embedded rename map)
    let renameMap: Map<string, string>;
        renameMap = resolveEntityNameMap(targetPath, options.EntitySubclassesPath, verbose);
    // Find HTML files
    let htmlFiles: string[];
        htmlFiles = [targetPath];
        htmlFiles = await glob('**/*.html', {
        console.log(`Scanning ${htmlFiles.length} HTML files...`);
    const allFindings: HtmlEntityNameFinding[] = [];
    for (const filePath of htmlFiles) {
            const findings = scanHtmlFile(filePath, sourceText, renameMap);
                    const fixedText = fixHtmlFile(sourceText, findings);
                        console.log(`  Fixed ${findings.length} entity name(s) in ${filePath}`);
                    console.log(`  Found ${findings.length} entity name(s) in ${filePath}`);
        FilesScanned: htmlFiles.length,
        RenameMapSize: renameMap.size,
