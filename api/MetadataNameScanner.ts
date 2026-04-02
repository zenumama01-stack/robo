 * JSON-based scanner/fixer for entity names in MemberJunction metadata files.
 * Scans metadata directories for entity name references that need the "MJ: "
 * prefix, including:
 *   - `@lookup:` references (both the entity name and the lookup value)
 *   - `.mj-sync.json` / `.mj-folder.json` entity/entityName config fields
 *   - `relatedEntities` object keys
 * Reuses the rename map from {@link buildEntityNameMap} (entity_subclasses.ts).
 *   mj codegen fix-metadata-names --path metadata/
 *   mj codegen fix-metadata-names --path metadata/ --fix
 *   import { scanMetadataNames } from '@memberjunction/codegen-lib';
 *   const result = await scanMetadataNames({ TargetPath: './metadata' });
/** The kind of pattern where an old entity name was found in metadata. */
export type MetadataPatternKind =
    | 'lookupEntity'      // Entity name in @lookup:ENTITY.field=value
    | 'lookupValue'       // Value in @lookup:MJ: Entities.Name=VALUE
    | 'folderConfig'      // entity/entityName in .mj-sync.json or .mj-folder.json
    | 'relatedEntityKey'  // Key in relatedEntities object
    | 'entityNameField';  // fields.Name in folders where .mj-sync.json entity is "Entities" or "MJ: Entities"
 * A single finding: one entity name reference in a metadata file that needs
export interface MetadataFinding {
    /** The kind of metadata pattern matched */
    PatternKind: MetadataPatternKind;
    /** Human-readable context (e.g., the full @lookup string) */
 * Options for the metadata name scanner.
export interface MetadataNameScanOptions {
 * Result of a metadata scan (and optional fix) operation.
export interface MetadataNameScanResult {
    Findings: MetadataFinding[];
// Lookup Reference Parser
interface ParsedLookup {
    /** The entity being looked up (e.g., "MJ: Entities" or "Entities") */
    /** The field being matched (e.g., "Name") */
    /** The value being searched for (e.g., "Dashboards") */
    /** Optional suffix like "allowDefer" */
    Options: string | null;
    /** The full raw lookup string after @lookup: */
    RawText: string;
 * Parses a `@lookup:` reference string into its components.
 * Format: `ENTITY.FIELD=VALUE` or `ENTITY.FIELD=VALUE?options`
 *   - `MJ: Entities.Name=Dashboards` → entity="MJ: Entities", field="Name", value="Dashboards"
 *   - `MJ: Dashboards.Name=ERD?allowDefer` → entity="MJ: Dashboards", field="Name", value="ERD"
function parseLookupReference(rawText: string): ParsedLookup | null {
    // Find the = sign separating ENTITY.FIELD from VALUE
    const eqIdx = rawText.indexOf('=');
    if (eqIdx === -1) return null;
    // Parse VALUE and optional ?suffix
    let value = rawText.substring(eqIdx + 1);
    let options: string | null = null;
    const qIdx = value.indexOf('?');
    if (qIdx !== -1) {
        options = value.substring(qIdx + 1);
        value = value.substring(0, qIdx);
    // Parse ENTITY.FIELD — find the last dot before = to split
    const leftSide = rawText.substring(0, eqIdx);
    const dotIdx = leftSide.lastIndexOf('.');
    if (dotIdx === -1) return null;
    const entityName = leftSide.substring(0, dotIdx);
    const fieldName = leftSide.substring(dotIdx + 1);
    if (!entityName || !fieldName || !value) return null;
    return { EntityName: entityName, FieldName: fieldName, Value: value, Options: options, RawText: rawText };
// Line Number Helper
/** Returns 1-based line number for a character position in source text. */
function getLineNumber(sourceText: string, position: number): number {
    for (let i = 0; i < position && i < sourceText.length; i++) {
// Metadata File Scanner
 * Scans a single metadata JSON file for entity name references that need
 * @param isEntitiesFolder - If true, the folder's .mj-sync.json entity is
 *   "Entities" or "MJ: Entities", so `fields.Name` values are entity names.
export function scanMetadataFile(
    isEntitiesFolder?: boolean,
): MetadataFinding[] {
    const findings: MetadataFinding[] = [];
    let parsed: unknown;
        parsed = JSON.parse(sourceText);
        return findings; // Invalid JSON, skip
    // 1. Scan @lookup patterns
    scanLookupPatterns(sourceText, filePath, renameMap, findings);
    // 2. Scan folder config files for entity/entityName
    const basename = path.basename(filePath);
    if (basename === '.mj-sync.json' || basename === '.mj-folder.json') {
        scanFolderConfig(parsed, sourceText, filePath, renameMap, findings);
    // 3. Scan relatedEntities keys
    scanRelatedEntityKeys(parsed, sourceText, filePath, renameMap, findings);
    // 4. Scan fields.Name when this folder manages the "Entities" entity
    if (isEntitiesFolder && basename !== '.mj-sync.json' && basename !== '.mj-folder.json') {
        scanEntityNameFields(parsed, sourceText, filePath, renameMap, findings);
 * Scans all @lookup: patterns in a file line-by-line.
 * Checks both the entity name and the lookup value.
function scanLookupPatterns(
    findings: MetadataFinding[],
    const lines = sourceText.split('\n');
    for (let i = 0; i < lines.length; i++) {
        const line = lines[i];
        const lineNum = i + 1;
            const lookupIdx = line.indexOf('@lookup:', searchStart);
            if (lookupIdx === -1) break;
            // Extract the raw lookup text up to the closing quote
            const afterPrefix = line.substring(lookupIdx + 8); // after "@lookup:"
            const quoteIdx = afterPrefix.indexOf('"');
            if (quoteIdx === -1) {
                searchStart = lookupIdx + 8;
            const rawText = afterPrefix.substring(0, quoteIdx);
            const fullLookup = `@lookup:${rawText}`;
            const parsed = parseLookupReference(rawText);
                checkLookupEntityName(parsed, fullLookup, lineNum, filePath, renameMap, findings);
                checkLookupValue(parsed, fullLookup, lineNum, filePath, renameMap, findings);
            searchStart = lookupIdx + 8 + quoteIdx;
 * Checks if the entity name in a @lookup reference needs the MJ: prefix.
 * e.g., `@lookup:Entities.Name=...` → should be `@lookup:MJ: Entities.Name=...`
function checkLookupEntityName(
    parsed: ParsedLookup,
    fullLookup: string,
    lineNum: number,
    if (renameMap.has(parsed.EntityName)) {
            Line: lineNum,
            OldName: parsed.EntityName,
            NewName: renameMap.get(parsed.EntityName)!,
            PatternKind: 'lookupEntity',
            Context: fullLookup,
 * Checks if the lookup value is an entity name that needs the MJ: prefix.
 * Only applies when looking up `MJ: Entities` (or `Entities`) by `Name`.
function checkLookupValue(
    // Only check values when the lookup is against the Entities entity by Name
    const entity = parsed.EntityName.trim();
    const isEntitiesLookup = entity === 'MJ: Entities' || entity === 'Entities';
    const isNameField = parsed.FieldName.trim() === 'Name';
    if (isEntitiesLookup && isNameField && renameMap.has(parsed.Value)) {
            OldName: parsed.Value,
            NewName: renameMap.get(parsed.Value)!,
            PatternKind: 'lookupValue',
 * Checks the entity/entityName field in .mj-sync.json or .mj-folder.json.
function scanFolderConfig(
    parsed: unknown,
    if (!parsed || typeof parsed !== 'object') return;
    const data = parsed as Record<string, unknown>;
    const entityName = (data.entity ?? data.entityName) as string | undefined;
    if (entityName && renameMap.has(entityName)) {
        const searchStr = `"${entityName}"`;
        const idx = sourceText.indexOf(searchStr);
        const lineNum = idx !== -1 ? getLineNumber(sourceText, idx) : 1;
            PatternKind: 'folderConfig',
            Context: `Folder config entity: "${entityName}"`,
 * Checks all keys in relatedEntities objects against the rename map.
function scanRelatedEntityKeys(
    const records = Array.isArray(parsed) ? parsed : [parsed];
        if (!record || typeof record !== 'object') continue;
        const rec = record as Record<string, unknown>;
        if (!rec.relatedEntities || typeof rec.relatedEntities !== 'object') continue;
        const relatedEntities = rec.relatedEntities as Record<string, unknown>;
        // Find where relatedEntities starts in the source for line-number search context
        const relIdx = sourceText.indexOf('"relatedEntities"');
        for (const key of Object.keys(relatedEntities)) {
            if (renameMap.has(key)) {
                const searchStr = `"${key}"`;
                const keyIdx = relIdx !== -1
                    ? sourceText.indexOf(searchStr, relIdx)
                    : sourceText.indexOf(searchStr);
                const lineNum = keyIdx !== -1 ? getLineNumber(sourceText, keyIdx) : 1;
                    OldName: key,
                    NewName: renameMap.get(key)!,
                    PatternKind: 'relatedEntityKey',
                    Context: `relatedEntities["${key}"]`,
 * Checks `fields.Name` values in data files within folders that manage the
 * "Entities" or "MJ: Entities" entity. In these folders, the Name field
 * is the entity name itself and may need the "MJ: " prefix.
function scanEntityNameFields(
        const fields = rec.fields as Record<string, unknown> | undefined;
        if (!fields || typeof fields !== 'object') continue;
        const nameValue = fields.Name;
        if (typeof nameValue !== 'string') continue;
        if (renameMap.has(nameValue)) {
            // Find the line number by searching for "Name": "OldValue"
            const searchStr = `"Name": "${nameValue}"`;
                OldName: nameValue,
                NewName: renameMap.get(nameValue)!,
                PatternKind: 'entityNameField',
                Context: `fields.Name: "${nameValue}"`,
// Metadata File Fixer
 * Applies entity name fixes to a metadata file using targeted string
 * replacements that preserve original formatting.
export function fixMetadataFile(sourceText: string, findings: MetadataFinding[]): string {
        switch (finding.PatternKind) {
            case 'lookupEntity':
                // @lookup:OldName.  →  @lookup:NewName.
                result = replaceAll(result,
                    `@lookup:${finding.OldName}.`,
                    `@lookup:${finding.NewName}.`);
            case 'lookupValue':
                // Entities.Name=OldValue  →  Entities.Name=NewValue
                // Safe because this only matches within @lookup: context strings
                    `Entities.Name=${finding.OldName}`,
                    `Entities.Name=${finding.NewName}`);
            case 'folderConfig': {
                // "entity": "OldName"  →  "entity": "NewName"
                // Also handles "entityName": "..."
                const entityPattern = new RegExp(
                    `("entity(?:Name)?"\\s*:\\s*)"${escapeRegex(finding.OldName)}"`,
                    'g'
                result = result.replace(entityPattern, `$1"${finding.NewName}"`);
            case 'relatedEntityKey':
                // "OldName":  →  "NewName":  (within relatedEntities context)
                    `"${finding.OldName}":`,
                    `"${finding.NewName}":`);
            case 'entityNameField':
                // "Name": "OldName"  →  "Name": "NewName"
                    `"Name": "${finding.OldName}"`,
                    `"Name": "${finding.NewName}"`);
function escapeRegex(str: string): string {
const DEFAULT_METADATA_EXCLUDE: string[] = [
 * Scans metadata JSON files for entity name references that need the "MJ: "
 * prefix, and optionally fixes them in place.
export async function scanMetadataNames(options: MetadataNameScanOptions): Promise<MetadataNameScanResult> {
            Success: false, Findings: [], FixedFiles: [],
            FilesScanned: 0, RenameMapSize: 0,
    // Find JSON files (including dotfiles like .mj-sync.json)
    let jsonFiles: string[];
        jsonFiles = [targetPath];
        jsonFiles = await glob('**/*.json', {
            dot: true,  // Include dotfiles like .mj-sync.json
            ignore: DEFAULT_METADATA_EXCLUDE,
    if (verbose) console.log(`Scanning ${jsonFiles.length} JSON files...`);
    // Build a set of directories whose .mj-sync.json declares entity as "Entities" or "MJ: Entities"
    const entityFolders = new Set<string>();
    for (const filePath of jsonFiles) {
        if (path.basename(filePath) === '.mj-sync.json') {
                const configText = fs.readFileSync(filePath, 'utf-8');
                const config = JSON.parse(configText) as Record<string, unknown>;
                const entityVal = (config.entity ?? config.entityName) as string | undefined;
                if (entityVal === 'Entities' || entityVal === 'MJ: Entities') {
                    entityFolders.add(path.dirname(filePath));
                // skip unparseable config
    const allFindings: MetadataFinding[] = [];
            const isEntitiesFolder = entityFolders.has(path.dirname(filePath));
            const findings = scanMetadataFile(filePath, sourceText, renameMap, isEntitiesFolder);
                    const fixedText = fixMetadataFile(sourceText, findings);
            if (verbose) console.error(`  ${message}`);
        FilesScanned: jsonFiles.length,
