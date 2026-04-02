import { SQLServerProviderConfigData, setupSQLServerClient } from "@memberjunction/sqlserver-dataprovider";
import pool from "./db";
import { autoRefreshInterval, currentUserEmail, mjCoreSchema } from "./config";
export async function timeout(ms: number) {
            reject(new Error("Batch operation timed out"));
        }, ms);
let _serverInitalized = false;
export async function handleServerInit(autoRefresh: boolean = false) {
    if (!_serverInitalized) {
        await pool.connect(); // Connect the pool first
        const config = new SQLServerProviderConfigData(pool, mjCoreSchema, autoRefresh ? autoRefreshInterval : 0/*no auto refreshes*/);
        _serverInitalized = true;
import { logError } from "./status_logging";
import fs, { unlinkSync } from "fs";
import fsExtra from 'fs-extra';
import { globSync } from 'glob';
export function makeDirs(dirPaths: string[]) {
    for (let i = 0; i < dirPaths.length; i++) {
        makeDir(dirPaths[i]);
export function makeDir(dirPath: string): void {
    if (!fs.existsSync(dirPath)) {
        fs.mkdirSync(dirPath, { recursive: true });
export function copyDir(sourceDir: string, destDir: string) {
    // To copy a folder or file, select overwrite accordingly
      fsExtra.copySync(sourceDir, destDir, { overwrite: true })
export async function attemptDeleteFile(filePath: string, maxRetries: number, repeatDelay: number): Promise<void> {
    for (let i = 0; i < maxRetries; i++) {
        unlinkSync(filePath);
        return; // if we get here without an exception, we're good, move on
        if ((err as any).code === 'ENOENT') {
          // file doesn't exist, so just ignore this and move on
        else if ((err as any).code === 'EBUSY') {
          await new Promise(resolve => setTimeout(resolve, repeatDelay));
          console.warn(`    Failed to delete file ${filePath}: ${(err as any).message}`);
export function combineFiles(directory: string, combinedFileName: string, pattern: string, overwriteExistingFile: boolean): void {
    const combinedFilePath = path.join(directory, combinedFileName);
    // Check if the combined file exists and if overwriteExistingFile is false, skip the process
    if (fs.existsSync(combinedFilePath) && !overwriteExistingFile) {
        console.log(`File ${combinedFileName} already exists. Skipping the process as overwriteExistingFile is set to false.`);
    // Use globSync to find files that match the pattern synchronously, excluding the combinedFileName
    const files = globSync(pattern, { cwd: directory }).filter((file: string) => file !== combinedFileName);
    // Sort the files so that files ending with '.generated.sql' come before '.permissions.generated.sql'
    files.sort((a: string, b: string) => {
        const isAPermissions = a.includes('.permissions.generated.sql');
        const isBPermissions = b.includes('.permissions.generated.sql');
        if (isAPermissions && !isBPermissions) {
        } else if (!isAPermissions && isBPermissions) {
            return a.localeCompare(b);
    let combinedContent = '';
    files.forEach((file: string) => {
        const filePath = path.join(directory, file);
        combinedContent += fs.readFileSync(filePath, 'utf8') + '\n\n\n';
    // Write the combined content to the specified file
    fs.writeFileSync(combinedFilePath, combinedContent);
 * Logs the provided params to the console if the shouldLog parameter is true
 * @param shouldLog 
export function logIf(shouldLog: boolean, ...args: any[]) {
        console.log(...args);
 * Sorts an array of items by Sequence property first, then by __mj_CreatedAt for consistent ordering.
 * This ensures that generated code maintains the same order across multiple runs.
 * @param items - Array of items that have Sequence and optional __mj_CreatedAt properties
 * @returns A new sorted array
export function sortBySequenceAndCreatedAt<T extends { Sequence: number; __mj_CreatedAt?: Date }>(items: T[]): T[] {
    return [...items].sort((a, b) => {
        // Primary sort by Sequence
        // Secondary sort by __mj_CreatedAt for consistent ordering
        if (a.__mj_CreatedAt && b.__mj_CreatedAt) {
            return a.__mj_CreatedAt.getTime() - b.__mj_CreatedAt.getTime();
        // If one has a date and the other doesn't, prioritize the one with a date
        if (a.__mj_CreatedAt && !b.__mj_CreatedAt) return -1;
        if (!a.__mj_CreatedAt && b.__mj_CreatedAt) return 1;
        // If neither has a date, maintain original order
 * Builds the complete code for a component based on the provided spec.
 * This function generates the full code representation of a component, including
 * the root component code and recursively pulling in dependency components and also replacing
 * the placeholders for those dependency components in the parent component's code with the 
 * actual code for those dependency components (which were generated after the parent component was generated).
 * @param spec - The ComponentRootSpec defining the component structure and behavior
 * @returns A string containing the complete executable JavaScript code for the component
export function BuildComponentCompleteCode(spec: ComponentSpec): string {
    // Start with the base code for the root component
    // Return empty string if no code provided (allows UI to hide Code tab)
    if (!spec.code || !spec.code.trim()) {
    let code = spec.code;
    // Recursively replace placeholders for dependency components with their generated code
    if (!spec.dependencies || spec.dependencies.length === 0) {
        // If there are no dependency components, return the base code for this component
    for (const dep of spec.dependencies) {
        const depCode = BuildComponentCode(dep, "");
        if (depCode && depCode.length > 0) {
            // Append the generated code for this dependency component to the root component code
            code += '\n\n' + depCode;
    // Return the complete code for this component
 * Builds the code for a component dependency based on the provided spec including recursive dependency components.
 * @param spec - The ComponentSpec defining the dependency component structure and behavior
 * @returns A string containing the executable JavaScript code for the component dependency
export function BuildComponentCode(dep: ComponentSpec, path: string): string {
    // Start with the base code for the component
    let commentHeader = `/*******************************************************\n   ${path ? `${path} > ` : ''}${dep.name}\n   ${dep.description}\n*******************************************************/\n`
    let code = commentHeader + dep.code;
    if (!dep.dependencies || dep.dependencies.length === 0) {
    for (const sub of dep.dependencies) {
        const subCode = BuildComponentCode(sub, path + (path ? ' > ' : '') + sub.name);
        if (subCode && subCode.length > 0) {
            code += '\n\n' + subCode;
    // Return the complete code for this dependency component
import { initializeConfig, RunCodeGenBase } from "@memberjunction/codegen-lib";
export let ___initialized = false;
export let ___runObject: RunCodeGenBase | null = null;
export async function handleServerInit() {
    if (!___initialized) {
        ___runObject = MJGlobal.Instance.ClassFactory.CreateInstance<RunCodeGenBase>(RunCodeGenBase);
        if (!___runObject) {
            throw new Error("Failed to create RunCodeGenBase instance");
        await ___runObject.setupDataSource();
        ___initialized = true;
 * Returns the TypeScript type that corresponds to the SQL type passed in
export function TypeScriptTypeFromSQLType(sqlType: string): 'string' | 'number' | 'boolean' | 'Date' {
    switch (sqlType.trim().toLowerCase()) {
            return 'Date';
export function TypeScriptTypeFromSQLTypeWithNullableOption(sqlType: string, addNullableOption: boolean): 'string' | 'string | null' | 'number' | 'number | null' | 'boolean' | 'boolean | null' | 'Date' | 'Date | null' {
    const retVal = TypeScriptTypeFromSQLType(sqlType);
    if (addNullableOption) {
        return retVal + ' | null' as 'string | null' | 'number | null' | 'boolean | null' | 'Date | null';
 * Formats a value based on the parameters passed in
 * @param sqlType - Required - the base type in SQL Server, for example int, nvarchar, etc. For types that have a length like numeric(28,4) or nvarchar(50) do NOT provide the length, just numeric and nvarchar in those examples
export function FormatValue(sqlType: string, 
                            value: any, 
        const retVal = FormatValueInternal(sqlType, value, decimals, currency, maxLength, trailingChars);
        LogError(`Error formatting value ${value} of type ${sqlType} with decimals ${decimals} and currency ${currency}`, e);
        return value; // just return the value as is if we cant format it
// internal only function used by FormatValue() to do the actual formatting
function FormatValueInternal(sqlType: string, 
                             trailingChars: string = "...") {
            if (isNaN(value))
                return new Intl.NumberFormat(undefined, { style: 'currency', 
                                                    currency: currency, 
                                                    minimumFractionDigits: decimals, 
                                                    maximumFractionDigits: decimals}).format(value);
          let date = new Date(value);
          return new Intl.DateTimeFormat().format(date);
          return new Intl.NumberFormat(undefined, { minimumFractionDigits: decimals, maximumFractionDigits: decimals }).format(value);
          return new Intl.NumberFormat(undefined, { minimumFractionDigits: 0, maximumFractionDigits: 0 }).format(value);
          return new Intl.NumberFormat(undefined, { style: 'percent', 
 * Returns a string that contains the full SQL type including length, precision and scale if applicable
 * @param baseType 
 * @param length 
 * @param precision 
 * @param scale 
export function SQLFullType(baseType: string, length: number, precision: number, scale: number): string {
    const type = baseType.trim().toLowerCase();
    let sOutput: string = type;
    if (type === 'varchar' )
        sOutput += `(${length > 0 ? length : 'MAX'})`;
    else if (type === 'nvarchar')
        sOutput += `(${length > 0 ? length / 2 : 'MAX'})`; // nvarchar divide the system length by 2 to get the actual length for the output
    else if (type === 'char')
        sOutput += `(${length})`;
    else if (type === 'nchar')
        sOutput += `(${length / 2})`; // nchar divide the system length by 2 to get the actual length for the output
    else if (type === 'decimal' || type === 'numeric')
        sOutput += `(${precision}, ${scale})`;
    else if (type === 'float')
        sOutput += `(${precision})`;
 * This function determines the actual maximum length for a given SQL field based on the base type and the length specified in the schema. 
 * For example, for a varchar(50) field, the length is 50, for an nvarchar(50) field, the length is 50/2 = 25
 * @param sqlBaseType 
 * @param sqlLength 
export function SQLMaxLength(sqlBaseType: string, sqlLength: number): number {
    switch (sqlBaseType.trim().toLowerCase()) {
            return sqlLength;
            return sqlLength / 2; // length in the schema is the # of bytes and on unicode fields we divide by 2 to get the # of characters a user is allowed to put in.
const _stopwords = [
    "a", "about", "above", "after", "again", "against", "ain", "all", "am", "an", "and", "any", "are", "aren", "aren't", "as", "at",
    "be", "because", "been", "before", "being", "below", "between", "both", "but", "by",
    "can", "couldn", "couldn't", "could",
    "d", "did", "didn", "didn't", "do", "does", "doesn", "doesn't", "doing", "don", "don't", "down", "during",
    "each",
    "few", "for", "from", "further",
    "had", "hadn", "hadn't", "has", "hasn", "hasn't", "have", "haven", "haven't", "having", "he", "her", "here", "hers", "herself", "him", "himself", "his", "how",
    "i", "if", "in", "into", "is", "isn", "isn't", "it", "it's", "its", "itself",
    "just",
    "ll",
    "m", "ma", "me", "mightn", "mightn't", "more", "most", "mustn", "mustn't", "my", "myself",
    "needn", "needn't", "no", "nor", "not", "now",
    "o", "of", "off", "on", "once", "only", "or", "other", "our", "ours", "ourselves", "out", "over", "own",
    "re",
    "s", "same", "shan", "shan't", "she", "she's", "should", "should've", "shouldn", "shouldn't", "so", "some", "such",
    "t", "than", "that", "that'll", "the", "their", "theirs", "them", "themselves", "then", "there", "these", "they", "this", "those", "through", "to", "too",
    "under", "until", "up",
    "ve", "very",
    "was", "wasn", "wasn't", "we", "were", "weren", "weren't", "what", "when", "where", "which", "while", "who", "whom", "why", "will", "with", "won", "won't", "wouldn", "wouldn't",
    "y", "you", "you'd", "you'll", "you're", "you've", "your", "yours", "yourself", "yourselves"
 * This function returns an array of common stop words that are used in text processing
 * @returns An array of common stop words that are used in text processing
export function CommonStopWords(): string[] {
    return _stopwords;
 * This function takes a string and removes common stop words from it, using the CommonStopWords() function to get the list of stop words
 * @param inputString 
export function StripStopWords(inputString: string): string {
    const stopwordPattern = new RegExp(`\\b(${_stopwords.join('|')})\\b`, 'gi');
    let outputString = inputString.replace(stopwordPattern, '');
    outputString = outputString.replace(/ +/g, ' ');  // Replace multiple spaces with a single space
    return outputString;
 * Returns a system-wide standard CodeName which is a programmatically acceptable identifier for a class, variable, etc using a standard replacement strategy for characters that are not acceptable in that context from a regular name
export function CodeNameFromString(input: string): string {
    // the code below replaces characters invalid for SQL or TypeScript identifiers with _ and stashes the result in a private variable so we only do this once
    // Replace all invalid characters with _
    let codeName = input.replace(/[^a-zA-Z0-9_]/g, "_");
    // Prepend an underscore if the first character is a number
    if (/^[0-9]/.test(codeName)) {
        codeName = "_" + codeName;
    return codeName;
 * Run concurrent promises with a maximum concurrency level
 * @param concurrency - The number of concurrently running promises
 * @param funcs - An array of functions that return promises
 * @returns A promise that resolves to an array of the resolved values from the promises returned by funcs
export function Concurrent<V>(concurrency: number, funcs: (() => Promise<V>)[]): Promise<V[]> {
    const p: Promise<V>[] = [];
    for (let i = 0; i < Math.max(1, Math.min(concurrency, funcs.length)); i++) runPromise();
    function runPromise() {
      if (++index < funcs.length) (p[p.length] = funcs[index]()).then(runPromise).catch(reject);
      else if (index === funcs.length) Promise.all(p).then(resolve).catch(reject);
 * The DBMS may store the default value for a column with extra parens, for example ((1)) or (getdate()) or (('Pending')) or (('Active')) and in addition for unicode characters
 * it may prefix the value with an N, for example N'Active'. This function will strip out the extra parens and the N prefix if it exists and return the actual default value
 * @param storedDefaultValue - The default value as stored in the DBMS 
export function ExtractActualDefaultValue(storedDefaultValue: string): string {
    if (!storedDefaultValue || storedDefaultValue.trim().length === 0)
        return storedDefaultValue;
    const noParens = StripContainingParens(storedDefaultValue);
    const unicodeStripped = StripUnicodePrefix(noParens);
    // now, we need to see if the unicodeStripped value is exactly equal to NULL which should be treated 
    // as the same as no default value. Without checking this, string data types that have a DEFAULT set to NULL
    // which is the same as no default, will end up with a STRING 'null' in the default value, but that isn't the
    // intent. 
    // BY CHECKING this BEFORE we strip the single quotes, we allow for a string of 'NULL', 'null' etc to exist
    // in a string data type's default value. As odd as that might be for a string default value it should be allowed
    if (unicodeStripped.trim().toLowerCase() === 'null') {
        return null;  // return the actual TypeScript/JavaScript null here as opposed to a string
        // our unicodeStripped value is NOT equal to exact match of null, so whatever we have is what we use
        // but we strip single quotes now
        const finalValue = StripSingleQuotes(unicodeStripped);
        return finalValue;    
 * Strips out the N prefix and single quotes from a string if they exist so that a value like
 * N'Active' becomes Active
export function StripUnicodePrefix(value: string): string {
    if (!value){
    value = value.trim(); // trim it first
    // check to see if the first character is an N and if the character after
    // that as well as the last character are single quotes, if so, strip all of those out
    if (value && value.toUpperCase().startsWith('N') &&
        value.length > 1 && value.charAt(1) === '\'' && 
        value.charAt(value.length - 1) === '\'') {
        return value.substring(2, value.length - 1); // strip out the N and the single quotes for example N'Active' becomes Active
 * Strips out single quotes from a string if they exist so that a value like
 * 'Active' becomes Active
export function StripSingleQuotes(value: string): string {
    const val = value.trim(); // trim it first
    // now check for symmetrical single quotes and remove them
    // this is for cases like 'Pending' or 'Active' which are stored in the DB as ('Pending') or ('Active')
    return val.startsWith("'") && val.endsWith("'") ? val.substring(1, val.length - 1) : val;
 * Strips out any number of symmetric containing parens from a string, for example ((0)) becomes 0
 * and ('Active') becomes 'Active'
export function StripContainingParens(value: string): string {
    if (value.startsWith('(') && value.endsWith(')')) {
        // recursive call so if we have ((0)) we keep stripping parens until we get to inner value
        // could be something like (getdate()) in which case we'll only strip the SYMMENTRIC parens
        return StripContainingParens(value.substring(1, value.length - 1));
 * Helper Method for BaseEntity sub-classes to call for embeddings
export async function EmbedTextLocalHelper(entity: BaseEntity, textToEmbed: string): Promise<SimpleEmbeddingResult> {
    await AIEngine.Instance.Config(false, entity.ContextCurrentUser);
    const e = await AIEngine.Instance.EmbedTextLocal(textToEmbed);
    if (!e?.result?.vector || !e?.model?.ID) {
        throw new Error('Failed to generate embedding - no vector or model ID returned');
        vector: e.result.vector,
        modelID: e.model.ID
import { MJGlobal } from ".";
import { IMJComponent, MJEventType } from "./interface";
 * Type definition for the global object store that allows arbitrary string indexing.
 * Uses 'any' intentionally as this is a dynamic storage mechanism for cross-module state.
export interface GlobalObjectStore {
export function GetGlobalObjectStore(): GlobalObjectStore | null {
    try    {
        // we might be running in a browser, in that case, we use the window object for our global stuff
        if (window)
            return window as unknown as GlobalObjectStore;
            // if we get here, we don't have a window object, so try the global object (node environment)
            // won't get here typically because attempting to access the global object will throw an exception if it doesn't exist
            if (global)
                return global as unknown as GlobalObjectStore;
                return null; // won't get here typically because attempting to access the global object will throw an exception if it doesn't exist
            // if we get here, we don't have a global object either, so we're not running in a browser or node, so we're probably running in a unit test
            // in that case, we don't have a provider saved, return null, we need to be either in node or a browser
 * This utility function will copy all scalar and array properties from an object to a new object and return the new object.
 * This function will NOT copy functions or non-plain objects (unless resolveCircularReferences is true).
 * @param input - The object to copy
 * @param resolveCircularReferences - If true, handles circular references and complex objects for safe JSON serialization.
 *                                     When enabled, circular references are replaced with '[Circular Reference]',
 *                                     complex objects (Sockets, Streams, etc.) are replaced with their type names,
 *                                     Error objects are specially handled to extract name/message/stack,
 *                                     and Dates are converted to ISO strings. Default: false
 * @param maxDepth - Maximum recursion depth when resolveCircularReferences is true (default: 10)
 * @returns A new object with scalars and arrays copied
export function CopyScalarsAndArrays<T extends object>(
    input: T,
    resolveCircularReferences: boolean = false,
    maxDepth: number = 10
): Partial<T> {
    if (resolveCircularReferences) {
        // Use the enhanced version with circular reference detection
        const seen = new WeakSet();
        const copy = (value: any, depth: number): any => {
            // Stop at max depth
            if (depth > maxDepth) {
                return '[Max Depth Reached]';
            // Handle primitives (string, number, boolean)
            if (typeof value !== 'object' && typeof value !== 'function') {
            // Skip functions
            if (typeof value === 'function') {
                return '[Function]';
            // Detect circular references
            if (seen.has(value)) {
                return '[Circular Reference]';
            seen.add(value);
            if (_.isArray(value)) {
                return value.map(item => copy(item, depth + 1));
            if (_.isDate(value)) {
            // Handle Error objects specially to get their properties
            if (value instanceof Error) {
                    name: value.name,
                    message: value.message,
                    stack: value.stack,
                    // Spread any custom properties added to the error
                    ...copy(_.omit(value, ['name', 'message', 'stack']), depth + 1)
            // Handle plain objects (POJOs)
            if (_.isPlainObject(value)) {
                for (const key in value) {
                    if (value.hasOwnProperty(key)) {
                        result[key] = copy(value[key], depth + 1);
            // For complex objects (Socket, Stream, Buffer, etc.), just use the type name
            const typeName = value.constructor?.name || 'Object';
            if (typeName !== 'Object') {
                return `[${typeName}]`;
            return '[Complex Object]';
        return copy(input, 0);
        // Original implementation for backward compatibility
        const result: Partial<T> = {};
        Object.keys(input).forEach((key) => {
            const value = input[key as keyof T];
            // Check for null or scalar types directly
            if (value === null || typeof value !== 'object') {
                result[key as keyof T] = value;
                // Handle arrays by creating a new array with the same elements
                result[key as keyof T] = [...value] as any;
            } else if (typeof value === 'object' && value.constructor === Object) {
                // Recursively copy plain objects
                result[key as keyof T] = CopyScalarsAndArrays(value) as any;
            // Functions and non-plain objects are intentionally ignored
 * Combines CleanJSON and SafeJSONParse to clean, extract, and parse JSON in one operation.
 * This is a convenience function that first cleans the input string using CleanJSON to handle
 * various formats (double-escaped, markdown blocks, etc.), then safely parses the result.
 * @param inputString - The string to clean and parse, which may contain JSON in various formats
 * @param logErrors - If true, parsing errors will be logged to console (default: false)
 * @returns The parsed object of type T, or null if cleaning/parsing fails
 * // Parse double-escaped JSON
 * const result = CleanAndParseJSON<{name: string}>('{\\"name\\": \\"test\\"}', true);
 * // Returns: {name: "test"}
 * // Parse JSON from markdown
 * const data = CleanAndParseJSON<{id: number}>('```json\n{"id": 123}\n```', false);
 * // Returns: {id: 123}
 * // Parse complex AI response with type safety
 * interface AIResponse {
 *   status: string;
 *   data: any;
 * const response = CleanAndParseJSON<AIResponse>(aiOutput, true);
 * // Returns typed object or null
export function CleanAndParseJSON<T = any>(inputString: string | null, logErrors: boolean = false): T | null {
    if (!inputString) {
    const cleaned = CleanJSON(inputString);
    if (!cleaned) {
    return SafeJSONParse<T>(cleaned, logErrors);
 * Cleans and extracts valid JSON from various input formats including double-escaped strings, 
 * strings with embedded JSON, and markdown code blocks.
 * This function handles multiple scenarios in the following priority order:
 * 1. **Valid JSON**: If the input is already valid JSON, it returns it formatted
 * 2. **Double-escaped JSON**: Handles strings with escaped quotes (\\") and newlines (\\n)
 * 3. **Markdown blocks**: Extracts JSON from ```json code blocks (only as last resort)
 * 4. **Mixed content**: Extracts JSON objects/arrays from strings with surrounding text
 * @param inputString - The string to process, which may contain JSON in various formats
 * @returns A formatted JSON string if valid JSON is found, otherwise null
 * // Simple JSON
 * CleanJSON('{"name": "test"}') 
 * // Returns: '{\n  "name": "test"\n}'
 * // Double-escaped JSON
 * CleanJSON('{\\"name\\": \\"test\\", \\"value\\": 123}')
 * // Returns: '{\n  "name": "test",\n  "value": 123\n}'
 * // JSON with embedded markdown (preserves the markdown in string values)
 * CleanJSON('{"text": "```json\\n{\\"inner\\": true}\\n```"}')
 * // Returns: '{\n  "text": "```json\\n{\\"inner\\": true}\\n```"\n}'
 * // Markdown block extraction (only when not valid JSON)
 * CleanJSON('Some text ```json\n{"extracted": true}\n``` more text')
 * // Returns: '{\n  "extracted": true\n}'
export function CleanJSON(inputString: string | null): string | null {
    if (!inputString)
    let processedString = inputString.trim();
    let originalException: Error | null = null;
    // First, try to parse the string as-is
    // This preserves any embedded JSON or markdown blocks within string values
        const parsed = JSON.parse(processedString);
        // If successful, return formatted JSON
        // save the original exception to throw later if we can't find a path to valid JSON
        originalException = e instanceof Error ? e : new Error(String(e));
        // common JSON patterns from LLM often have extra } or missing final
        // } so let's test those two patterns quickly here and if they fail
        // then we'll continue with the rest of the logic
        if (processedString.endsWith('}')) {
            // first try to remove the last }
            const newString = processedString.slice(0, -1);
            // now try to parse again
            const result = SafeJSONParse(newString);
            // if we get here the above didn't work so try to add
            // an extra } at the end and see if that works
            const nextString = processedString + '}';
            const nextResult = SafeJSONParse(nextString);
            if (nextResult) {
                return JSON.stringify(nextResult, null, 2);
    // Now proceed with the extraction logic only as a last resort
    // Remove formatting newlines/tabs but preserve \n and \t inside strings
    processedString = processedString.replace(/(?<!\\)\n/g, '').replace(/(?<!\\)\t/g, '');
    // Handle double-escaped characters
    // This converts \\n to actual \n, \\" to actual ", etc.
    if (processedString.includes('\\\\') || processedString.includes('\\"')) {
            // Try to parse it as a JSON string to unescape it
            // This handles cases where the entire string is a JSON-encoded string
            processedString = JSON.parse('"' + processedString + '"');
            // If that doesn't work, manually replace common double-escaped sequences
            processedString = processedString
                .replace(/\\\\n/g, '\n')     // \\n -> \n
                .replace(/\\\\t/g, '\t')     // \\t -> \t
                .replace(/\\\\r/g, '\r')     // \\r -> \r
                .replace(/\\\\"/g, '"')      // \\" -> "
                .replace(/\\\\\\/g, '\\');   // \\\\ -> \\
    // Try to parse the processed string after unescaping
        // If direct parsing still fails, continue with extraction logic
    // Regular expression to match JavaScript code blocks within Markdown fences
    // This regex looks for ``` (including when the ` is escaped like \`)
    // optionally followed by js or javascript (case-insensitive), then captures until the closing ```
    const markdownRegex = /(?:```|\\`\\`\\`)(?:json|JSON)?\s*([\s\S]*?)(?:```|\\`\\`\\`)/gi;
    // Check if the input contains Markdown code fences for JavaScript
    const matches = Array.from(processedString.matchAll(markdownRegex));
    if (matches.length > 0) {
        // If there are matches, concatenate all captured groups (in case there are multiple code blocks)
        const extracted = matches.map(match => match[1].trim()).join('\n');
        return CleanJSON(extracted); // Recursively clean the extracted JSON
        // If there are no Markdown code fences, we could have a string that contains JSON, or is JUST JSON
        // Attempt to extract JSON from a mixed string
        const firstBracketIndex = processedString.indexOf('[');
        const firstBraceIndex = processedString.indexOf('{');
        let startIndex = -1;
        // Determine the starting index based on the position of the first '[' and '{'
        if ((firstBracketIndex !== -1 && firstBracketIndex < firstBraceIndex) || firstBraceIndex === -1) {
            startIndex = firstBracketIndex;
            endIndex = processedString.lastIndexOf(']');
        } else if (firstBraceIndex !== -1) {
            startIndex = firstBraceIndex;
            endIndex = processedString.lastIndexOf('}');
        if (startIndex === -1 || endIndex === -1 || endIndex < startIndex) {
            console.warn("No JSON found in the input.");
            return processedString; // Return the processed string instead of jsonString
        const potentialJSON = processedString.substring(startIndex, endIndex + 1);
            // Parse and stringify to format the JSON nicely
            // and to validate it's indeed a valid JSON.
            const jsonObject = JSON.parse(potentialJSON);
            return JSON.stringify(jsonObject, null, 2);
            // that was our last attempt and it failed so we need
            // to throw an exception here with the orignal exception
            throw new Error(`Failed to find a path to CleanJSON\n\n${originalException?.message}`);
 * This function takes in a string that may contain JavaScript code in a markdown code block and returns the JavaScript code without the code block.
 * @param javaScriptCode 
export function CleanJavaScript(javaScriptCode: string): string {
    // This regex looks for ``` optionally followed by js or javascript (case-insensitive), then captures until the closing ```
    const markdownRegex = /```(?:js|javascript)?\s*([\s\S]*?)```/gi;
    const matches = Array.from(javaScriptCode.matchAll(markdownRegex));
        return matches.map(match => match[1].trim()).join('\n');
        // If there are no Markdown code fences, assume the input is plain JavaScript code
        return javaScriptCode.trim();
 * Simple wrapper method to JSON.parse that catches any errors and optionally logs them to the console. 
 * This method is useful when you want to parse JSON but don't want to crash the application if the JSON is invalid.
 * @returns The parsed object of type T (default: any), or null if parsing fails or input is empty
 * // Basic usage without type
 * const data = SafeJSONParse('{"name": "test"}');
 * // With type parameter
 * interface User { name: string; age: number; }
 * const user = SafeJSONParse<User>('{"name": "John", "age": 30}', true);
 * // Invalid JSON returns null
 * const result = SafeJSONParse('invalid json', true); // logs error, returns null
export function SafeJSONParse<T = any>(jsonString: string, logErrors: boolean = false): T | null {
    if (!jsonString) {
        if (logErrors)
            console.error("Error parsing JSON string:", e);
 * This function takes in a string of text(assuming markdown, or just newline formatted), and converts it to an HTML list. The list type can be either ordered or unordered.
 * @param htmlListType 
 * @param text 
export function ConvertMarkdownStringToHtmlList(htmlListType: 'Ordered' | 'Unordered', text: string): string | null {
        const listTag = htmlListType === 'Unordered' ? 'ul' : 'ol';
        if (!text.includes('\n')) {
        const listItems = text.split('\n').map(line => `<li>${line.trim().replace(/^-\s*/, '')}</li>`).join('');
        return `<${listTag}>${listItems}</${listTag}>`;
        console.error("Error converting markdown string to HTML list:", e);
* Converts a string that uses camel casing or contains consecutive uppercase letters to have spaces between words.
* "DatabaseVersion" -> "Database Version"
* "AIAgentLearningCycle" -> "AI Agent Learning Cycle"
export function convertCamelCaseToHaveSpaces(s: string): string {
    for (let i = 0; i < s.length; ++i) {
          i > 0 && // Not the first character
          ((s[i] === s[i].toUpperCase() && s[i - 1] !== s[i - 1].toUpperCase()) || // Transition from lowercase to uppercase
             (s[i] === s[i].toUpperCase() && s[i - 1] === s[i - 1].toUpperCase() && // Transition within consecutive uppercase letters
             i + 1 < s.length && s[i + 1] !== s[i + 1].toUpperCase())) // Followed by a lowercase
          result += ' ';
       result += s[i];
 * Removes all whitespace characters (spaces, tabs, newlines) from a given string.
 * @param s - The input string from which to remove whitespace.
 * @returns A new string with all whitespace characters removed.
 * stripWhitespace("  Hello   World  "); // "HelloWorld"
 * stripWhitespace("\tExample\nString "); // "ExampleString"
 * stripWhitespace(""); // ""
export function stripWhitespace(s: string): string {
    if (!s) {
        // Return the original string if it is null, undefined, or empty
    return s.replace(/\s+/g, ''); // Use \s+ for efficiency in case of consecutive whitespace
const __irregularPlurals: Record<string, string> = {
    child: 'children',
    person: 'people',
    mouse: 'mice',
    foot: 'feet',
    tooth: 'teeth',
    goose: 'geese',
    man: 'men',
    woman: 'women',
    ox: 'oxen',
    cactus: 'cacti',
    focus: 'foci',
    fungus: 'fungi',
    nucleus: 'nuclei',
    syllabus: 'syllabi',
    analysis: 'analyses',
    diagnosis: 'diagnoses',
    thesis: 'theses',
    crisis: 'crises',
    phenomenon: 'phenomena',
    criterion: 'criteria',
    datum: 'data',
    appendix: 'appendices',
    index: 'indices',
    matrix: 'matrices',
    vertex: 'vertices',
    vortex: 'vortices',
    radius: 'radii',
    corpus: 'corpora',
    genus: 'genera',
    locus: 'loci',
    alga: 'algae',
    antenna: 'antennae',
    formula: 'formulae',
    nebula: 'nebulae',
    vertebra: 'vertebrae',
    memorandum: 'memoranda',
    medium: 'media',
    bacterium: 'bacteria',
    curriculum: 'curricula',
    referendum: 'referenda',
    stimulus: 'stimuli',
    automaton: 'automata',
    beau: 'beaux',
    bureau: 'bureaux',
    tableau: 'tableaux',
    cherub: 'cherubim',
    seraph: 'seraphim',
    elf: 'elves',
    calf: 'calves',
    half: 'halves',
    knife: 'knives',
    leaf: 'leaves',
    life: 'lives',
    loaf: 'loaves',
    scarf: 'scarves',
    self: 'selves',
    sheaf: 'sheaves',
    shelf: 'shelves',
    thief: 'thieves',
    wife: 'wives',
    wolf: 'wolves',
 * Retrieves the plural form of a word if it is an irregular plural.
 * @param singularName - The singular form of the word to check.
 * @returns The irregular plural form if found, or `null` if not found.
 * getIrregularPlural('child'); // returns 'children'
 * getIrregularPlural('dog'); // returns null
export function getIrregularPlural(singularName: string): string | null {
    return __irregularPlurals[singularName.toLowerCase()] || null;
 * Attempts to return a singular form of a given word, assuming it is already plural by reversing common pluralization rules.
 * @param word - The word to check.
 * @returns The singular form if detected, otherwise the original word since it would be assumed the original word is in fact singular
function getSingularForm(word: string): string | null {
    const lowerWord = word.toLowerCase();
    // Reverse lookup from __irregularPlurals values to keys
    const singularFromIrregular = Object.entries(__irregularPlurals).find(
        ([singular, plural]) => plural === lowerWord
    if (singularFromIrregular) {
        return singularFromIrregular[0]; // Return the singular form
    // Detect regular plural forms
    if (lowerWord.endsWith('ies')) {
        return lowerWord.slice(0, -3) + 'y'; // "parties" -> "party"
    if (/(s|ch|sh|x|z)es$/.test(lowerWord)) { // checks to see if the word ends with 'es'
        return lowerWord.slice(0, -2); // "boxes" -> "box"
    if (lowerWord.endsWith('s') && !lowerWord.endsWith('ss')) {
        return lowerWord.slice(0, -1); // "dogs" -> "dog"
    // if we get here, we return the word itself, because none of the reversals we
    // did above yielded anything, so it means that to best of this simplistic
    // algo's ability, we believe word is ALREADY plural
    return word;
 * Converts a singular word to its plural form, handling common pluralization rules 
 * and irregular plurals.
 * @param singularName - The singular form of the word to pluralize.
 * @returns The plural form of the word.
 * generatePluralName('child'); // returns 'children'
 * generatePluralName('box'); // returns 'boxes'
 * generatePluralName('party'); // returns 'parties'
 * generatePluralName('dog'); // returns 'dogs'
export function generatePluralName(singularName: string, options? : { capitalizeFirstLetterOnly?: boolean, capitalizeEntireWord?: boolean }): string {
    // Check if it's already plural
    const detectedSingular = getSingularForm(singularName);
    if (!detectedSingular) {
        // if we did NOT find a singular, assume it is already plural
        return adjustCasing(singularName, options); 
    else if (detectedSingular.trim().toLowerCase() !== singularName.trim().toLowerCase()) {
        // here we did detect a singular form. Check to see if it is DIFFERENT from
        // the provided value. Because we're supposed to be provided a singular to this
        // function if we are given a plural - like Customers - we want to just throw it back
        // but if we were passed a true singular, then we keep on going
    // Check for irregular plurals
    const irregularPlural = getIrregularPlural(singularName);
    if (irregularPlural) {
        return adjustCasing(irregularPlural, options);
    // Handle common pluralization rules
    if (singularName.endsWith('y') && singularName.length > 1) {
        const secondLastChar = singularName[singularName.length - 2].toLowerCase();
        if ('aeiou'.includes(secondLastChar)) {
            // Ends with a vowel + y, just add 's'
            return adjustCasing(singularName + 's', options);
            // Ends with a consonant + y, replace 'y' with 'ies'
            return adjustCasing(singularName.slice(0, -1) + 'ies', options);
    if (/(s|ch|sh|x|z)$/.test(singularName)) {
        // Ends with 's', 'ch', 'sh', 'x', or 'z', add 'es'
        return adjustCasing(singularName + 'es', options);
    // Default case: Add 's' to the singular name
 * Utility method that will adjust the casing of a word based on the options provided. The options object can have two properties:
 * * capitalizeFirstLetterOnly: If true, only the first letter of the word will be capitalized, and the rest will be lower case.
 * * capitalizeEntireWord: If true, the entire word will be capitalized.
 * @param word 
export function adjustCasing(word: string, options?: { 
    capitalizeFirstLetterOnly?: boolean, 
    capitalizeEntireWord?: boolean
    forceRestOfWordLowerCase?: boolean }): string {
    if (word && word.length > 0 && options) {
        if (options.capitalizeEntireWord) {
            return word.toUpperCase();
        else if (options.capitalizeFirstLetterOnly) {   
            if (options.forceRestOfWordLowerCase) {
                // make the first character upper case and rest lower case
                return word.charAt(0).toUpperCase() + word.slice(1).toLowerCase();
                // make the first character upper case and leave the rest as is
                return word.charAt(0).toUpperCase() + word.slice(1);
            // no changes requested, do nothing
        return word; //return whatever it is, blank string, null, or undefined, but no changes
 * Removes trailing characters from a string if they match the specified substring.
 * @param s - The input string from which trailing characters should be stripped.
 * @param charsToStrip - The substring to remove if it appears at the end of the input string.
 * @param skipIfExactMatch - If `true`, does not strip the trailing characters when the string is exactly equal to `charsToStrip`.
 * @returns The modified string with trailing characters stripped, or the original string if no match is found.
 * stripTrailingChars("example.txt", ".txt", false); // "example"
 * stripTrailingChars("example.txt", ".txt", true);  // "example"
 * stripTrailingChars("file.txt", "txt", false);     // "file.txt" (no match)
 * stripTrailingChars(".txt", ".txt", true);         // ".txt" (exact match, not stripped)
export function stripTrailingChars(s: string, charsToStrip: string, skipIfExactMatch: boolean): string {
    if (!s || !charsToStrip) {
        // Return the original string if either input is empty
    const shouldStrip =
        s.endsWith(charsToStrip) &&
        (!skipIfExactMatch || s !== charsToStrip);
    if (shouldStrip) {
        return s.substring(0, s.length - charsToStrip.length);
 * Recursively removes all spaces from a given string.
 * @param s - The input string from which to remove all spaces.
 * @returns A new string with all spaces removed.
 * replaceAllSpaces("Hello World");       // "HelloWorld"
 * replaceAllSpaces("  Leading spaces"); // "Leadingspaces"
 * replaceAllSpaces("Trailing spaces  "); // "Trailingspaces"
 * replaceAllSpaces("NoSpacesHere");     // "NoSpacesHere"
 * replaceAllSpaces("");                 // ""
export function replaceAllSpaces(s: string): string {
        // Handle null, undefined, or empty string cases
    if (s.includes(' ')) {
        // Recursive case: Replace a single space and call the function again
        return replaceAllSpaces(s.replace(' ', ''));
    // Base case: No spaces left to replace
 * This utility function sends a message to all components that are listening requesting a window resize. This is a cross-platform method of requesting a resize and is loosely coupled from
 * the actual implementation on a specific device/browser/etc.
 * @param delay 
 * @param component 
export function InvokeManualResize(delay: number = 50, component: IMJComponent | null = null) {
        event: MJEventType.ManualResizeRequest,
        args: null,
        component: component!
    }, delay ); // give the tabstrip time to render
 * Generates a version 4 UUID (Universally Unique Identifier) using the uuid library.
 * @returns the generated UUID as a string.
export function uuidv4(): string {
    return v4();
 * Compares two strings line by line and logs the differences to the console.
 * This function is useful for debugging purposes to identify discrepancies between two text inputs.
 * It will print the total number of lines in each string, and for each line that differs,
 * it will log the line number, the content of each line, and the first character difference
 * along with its position and character codes.
 * @param str1 
 * @param str2 
 * @returns An array of strings representing the differences found between the two input strings. If array is empty, it means no differences were found.
export function compareStringsByLine(str1: string, str2: string, logToConsole: boolean = true): string[] {
    const lines1 = str1.split('\n');
    const lines2 = str2.split('\n');
    const maxLines = Math.max(lines1.length, lines2.length);
    const returnArray: string[] = [];
    function emit (message: string) {
        if (logToConsole) {
        returnArray.push(message);
    if (lines1.length !== lines2.length) {
        emit(`Total lines: ${lines1.length} vs ${lines2.length}`);
    for (let i = 0; i < maxLines; i++) {
        const line1 = lines1[i] || '';
        const line2 = lines2[i] || '';
        if (line1 !== line2) {
            emit(`\n🔴 Difference at line ${i + 1}:`);
            emit(`Line 1: "${line1}"`);
            emit(`Line 2: "${line2}"`);
            // Find exact character difference within the line
            for (let j = 0; j < Math.max(line1.length, line2.length); j++) {
                if (line1[j] !== line2[j]) {
                    emit(`  First diff at column ${j + 1}:`);
                    emit(`  Char 1: "${line1[j]}" (code: ${line1.charCodeAt(j) || 'undefined'})`);
                    emit(`  Char 2: "${line2[j]}" (code: ${line2.charCodeAt(j) || 'undefined'})`);
                    emit(`  Context: "${line1.substring(Math.max(0, j - 10), j + 10)}"`);
            // Show first few differences only to avoid spam
            if (i > 5) {
                emit(`\n... and ${maxLines - i - 1} more lines with differences`);
    return returnArray;
 * Options for the ParseJSONRecursive function
export interface ParseJSONOptions {
  /** Maximum recursion depth to prevent infinite loops (default: 100) */
  maxDepth?: number;
  /** If true, extracts embedded JSON from strings and places it in a separate key with '_' suffix (default: false) */
  extractInlineJson?: boolean;
  /** If true, enables debug logging to console (default: false) */
  debug?: boolean;
 * Internal options for the ParseJSONRecursive function
 * This interface extends the public options with internal tracking fields
interface InternalParseJSONOptions {
  /** Public options */
  options: ParseJSONOptions;
  /** Current depth level (used for recursion tracking) */
  currentDepth: number;
  /** Set to track objects we've already processed to prevent circular references */
  processedObjects: WeakSet<object>;
  /** Set to track JSON strings we've already parsed to prevent infinite loops */
  processedStrings: Set<string>;
  /** Current path for debugging */
  currentPath: string;
 * Recursively parse JSON strings within an object/array structure.
 * This function will traverse through objects and arrays, attempting to parse
 * any string values as JSON. If parsing succeeds, it continues recursively.
 * This is particularly useful for handling deeply nested JSON structures
 * where JSON is stored as strings within other JSON objects.
 * The function makes no assumptions about property names - it will attempt
 * to parse any string value it encounters, regardless of the key name.
 * @param obj The object to process
 * @param options Configuration options for parsing
 * @returns The object with all JSON strings parsed
 * const input = {
 *   data: '{"nested": "{\\"deeply\\": \\"nested\\"}"}',
 *   payload: '{"foo": "bar"}',
 *   someOtherProp: '["a", "b", "c"]'
 * const output = ParseJSONRecursive(input);
 * //   data: { nested: { deeply: "nested" } },
 * //   payload: { foo: "bar" },
 * //   someOtherProp: ["a", "b", "c"]
 * @example with options
 *   content: 'Action results:\n[{"action": "test"}]'
 * const output = ParseJSONRecursive(input, { extractInlineJson: true, maxDepth: 50 });
 * //   content: "Action results:",
 * //   content_: [{ action: "test" }]
export function ParseJSONRecursive(obj: any, options: ParseJSONOptions = {}): any {
  // Set default options with more conservative depth limit for performance
  const opts: Required<ParseJSONOptions> = {
    maxDepth: options.maxDepth ?? 100,
    extractInlineJson: options.extractInlineJson ?? false,
    debug: options.debug ?? false
  // Start recursive parsing with depth 0
  return parseJSONRecursiveWithDepth(obj, opts, 0, 'root');
function parseJSONRecursiveWithDepth(obj: any, options: Required<ParseJSONOptions>, depth: number, path: string): any {
  if (depth >= options.maxDepth) {
    if (options.debug) {
      console.warn(`[ParseJSONRecursive] Maximum depth of ${options.maxDepth} reached at path: ${path}`);
  if (obj === null || obj === undefined) {
  if (typeof obj !== 'object') {
  // Use recursiveReplaceKey which handles all types
  return recursiveReplaceKey(obj, options, depth, path);
function recursiveReplaceKey(value: any, options: Required<ParseJSONOptions>, depth: number, path: string): any {
    console.log(`[ParseJSONRecursive] Depth: ${depth}, Path: ${path}, Type: ${typeof value}${Array.isArray(value) ? ' (array)' : ''}`);
    return recursiveReplaceString(value, options, depth, path);
  else if (Array.isArray(value)) {
    // Create a new array instead of modifying the original
    const newArray = new Array(value.length);
    for (let i = 0; i < value.length; i++) {
      newArray[i] = recursiveReplaceKey(value[i], options, depth + 1, `${path}[${i}]`);
    return newArray;
  else if (typeof value === 'object' && value !== null) {
    // Create a new object instead of modifying the original
        console.log(`[ParseJSONRecursive] Processing key: ${key} at path: ${path}.${key}`);
      result[key] = recursiveReplaceKey(value[key], options, depth + 1, `${path}.${key}`);
    return value; // return as-is for non-string, non-array, and non-object types
 * Checks if two dates differ only by a timezone-like shift.
 * Returns true if the difference is EXACTLY a whole number of hours
 * (no variance in minutes/seconds/milliseconds) and within 23 hours.
 * This helps detect timezone interpretation issues with datetime/datetime2 fields
 * that don't store timezone information.
 * @param date1 - The first date to compare
 * @param date2 - The second date to compare
 * @returns true if the dates differ only by a whole-hour timezone shift (1-23 hours)
 * // 6-hour timezone shift - returns true
 * IsOnlyTimezoneShift(new Date('2025-12-25T10:30:45.123Z'), new Date('2025-12-25T16:30:45.123Z'));
 * // Different by 1ms - returns false (real change)
 * IsOnlyTimezoneShift(new Date('2025-12-25T10:30:45.123Z'), new Date('2025-12-25T16:30:45.124Z'));
export function IsOnlyTimezoneShift(date1: Date, date2: Date): boolean {
  const diffMs = Math.abs(date1.getTime() - date2.getTime());
  // Check if difference is exactly a whole number of hours
  const msPerHour = 1000 * 60 * 60;
  const isExactHours = diffMs % msPerHour === 0;
  if (!isExactHours) return false;
  // Check if within reasonable timezone range (1-23 hours)
  const hoursDiff = diffMs / msPerHour;
  return hoursDiff >= 1 && hoursDiff <= 23;
function recursiveReplaceString(str: string, options: Required<ParseJSONOptions>, depth: number, path: string): any {
    console.log(`[ParseJSONRecursive] String preview: ${str.substring(0, 100)}${str.length > 100 ? '...' : ''}`);
  // PERFORMANCE OPTIMIZATION: Early exit for non-JSON strings
  // Check if the first non-whitespace character is { or [ - if not, it's definitely not JSON
  const trimmed = str.trim();
  if (trimmed.length === 0 || (trimmed[0] !== '{' && trimmed[0] !== '[' && trimmed[0] !== '"')) {
    // Not JSON-like, skip expensive JSON.parse() attempt unless extractInlineJson is enabled
    if (!options.extractInlineJson) {
    // With extractInlineJson, we still need to check for embedded JSON, but skip the initial parse
    // Looks JSON-like, attempt to parse
      const parsed = JSON.parse(str);
      // Check if parsing returned the same value (e.g., JSON.parse('"user"') === "user")
      if (parsed === str) {
          console.log(`[ParseJSONRecursive] JSON.parse returned same value at path: ${path}, stopping`);
      if (parsed && typeof parsed === 'object') {
          console.log(`[ParseJSONRecursive] Successfully parsed JSON at path: ${path}`);
        return parseJSONRecursiveWithDepth(parsed, options, depth + 1, `${path}[parsed-json]`);
        return parsed; // Keep simple values as-is
      // JSON.parse failed, continue to inline extraction if enabled
  // Handle extractInlineJson or return original string
  if (options?.extractInlineJson) {
      // Look for JSON patterns within the string
      // First try ```json blocks
      const codeBlockMatch = str.match(/```json\s*\n([\s\S]*?)\n```/);
      if (codeBlockMatch) {
          const parsedJson = JSON.parse(codeBlockMatch[1]);
            text: str.replace(codeBlockMatch[0], '').trim(),
            json: parseJSONRecursiveWithDepth(parsedJson, options, depth + 1, `${path}[embedded-json]`)
          // If parsing fails, continue
      // Simple approach: find first { or [ and try to parse from there
      const jsonStartIndex = str.search(/[{\[]/);
      if (jsonStartIndex !== -1) {
        // Try to parse from the JSON start to the end of string
        const possibleJson = str.substring(jsonStartIndex);
          const parsedJson = JSON.parse(possibleJson);
          const textBefore = str.substring(0, jsonStartIndex).trim();
            text: textBefore || undefined,
          // JSON.parse failed, the string doesn't contain valid JSON
  // If we get here, return the original string
import { request as httpRequest } from 'http';
import { request as httpsRequest } from 'https';
import { gzip as gzipCallback, createGunzip } from 'zlib';
import { DataSourceInfo, ProviderInfo } from './types';
const gzip = promisify(gzipCallback);
type StreamCallback = (jsonObject: any) => void;
 * Utility function to handle HTTP/HTTPS requests with optional compression, custom headers, and streaming response callback for JSON objects.
 * This function accumulates data chunks and parses complete JSON objects, assuming newline-delimited JSON in the stream.
 * @param {string} url - The URL to which the request is sent.
 * @param {any} payload - The payload to be sent with the request.
 * @param {boolean} useCompression - Flag to determine if payload compression should be used.
 * @param {Record<string, string> | null} headers - Custom headers for the request. Can be null.
 * @param {StreamCallback} [streamCallback] - Optional callback for handling streaming JSON objects.
 * @returns {Promise<any[]>} - A promise that resolves to an array of all JSON objects received during the streaming process.
export async function sendPostRequest(url: string, payload: any, useCompression: boolean, headers: Record<string, string> | null, streamCallback?: StreamCallback): Promise<any[]> {
      const { protocol, hostname, port, pathname } = new URL(url);
      if (useCompression) {
          data = await gzip(Buffer.from(JSON.stringify(payload)) as any);
          headers = headers || {}; // Ensure headers is an object
          headers['Content-Encoding'] = 'gzip';
          const err = z.object({ message: z.string() }).safeParse(error);
          console.error(`Error in sendPostRequest while compressing data: ${err.success ? err.data.message : error}`);
          return reject(error);
        data = Buffer.from(JSON.stringify(payload));
        port: port || (protocol === 'https:' ? 443 : 80),
        path: pathname,
      const request = protocol === 'https:' ? httpsRequest : httpRequest;
      const jsonObjects: any[] = [];
      let buffer = '';
      const req = request(options, (res) => {
        const gunzip = createGunzip();
        const stream = res.headers['content-encoding'] === 'gzip' ? res.pipe(gunzip) : res;
        let streamEnded = false;
        const handleStreamEnd = () => {
          if (streamEnded) return; // Prevent double-resolution
          streamEnded = true;
          // Attempt to parse any remaining data in buffer in case it's a complete JSON object
          if (buffer.trim()) {
              const jsonObject = JSON.parse(buffer.trim());
              jsonObjects.push(jsonObject);
              streamCallback?.(jsonObject);
              const err = z.object({ message: z.string() }).safeParse(e);
              // Handle JSON parse error for the last chunk
              console.warn(`Error in postRequest().stream(end) while parsing JSON object: ${err.success ? err.data.message : e}`);
          resolve(jsonObjects);
        stream.on('data', (chunk) => {
          buffer += chunk;
          let boundary;
          while ((boundary = buffer.indexOf('\n')) !== -1) {
            const jsonString = buffer.substring(0, boundary);
            buffer = buffer.substring(boundary + 1);
              const jsonObject = JSON.parse(jsonString);
              // Handle JSON parse error for cases of malformed JSON objects
              console.warn(`Error in postRequest().stream(data) while parsing JSON object: ${err.success ? err.data.message : e}`);
        stream.on('end', handleStreamEnd);
        // Handle premature connection close (e.g., server crashes mid-response)
        stream.on('close', () => {
          if (!streamEnded) {
            console.warn(`Stream closed prematurely for ${url}`);
            handleStreamEnd();
        // Handle stream errors (decompression failures, etc.)
        stream.on('error', (e) => {
            console.error(`Stream error for ${url}:`, e);
            reject(new Error(`Stream error: ${e.message}`));
      req.on('error', (e) => {
        const errorMessage = err.success ? err.data.message : String(e);
        console.error(`Error in sendPostRequest().req.on(error) for ${hostname}:${port}${pathname}: ${errorMessage}`);
        // Create a more informative error for the rejection
        const contextualError = new Error(`HTTP request failed to ${url}: ${errorMessage}`);
        // Preserve the original error as the cause
          (contextualError as any).cause = e;
        reject(contextualError);
      req.write(data);
      req.end();
      console.error(`Error in sendPostRequest: ${err.success ? err.data.message : e}`)
      reject(e);
   * Returns the read-only data source if it exists, otherwise returns the read-write data source if options is not provided or if options.allowFallbackToReadWrite is true.
   * @param dataSources 
  export function GetReadOnlyDataSource(dataSources: DataSourceInfo[], options?: {allowFallbackToReadWrite: boolean}): sql.ConnectionPool & { query: (sql: string, params?: any) => Promise<any[]> } {
    const readOnlyDataSource = dataSources.find((ds) => ds.type === 'Read-Only');
      return extendConnectionPoolWithQuery(readOnlyDataSource.dataSource);
    else if (!options || options.allowFallbackToReadWrite) {
      // default behavior for backward compatibility prior to MJ 2.22.3 where we introduced this functionality was to have a single
      // connection, so for back-compatability, if we don't have a read-only data source, we'll fall back to the read-write data source
      const readWriteDataSource = dataSources.find((ds) => ds.type === 'Read-Write');
      if (readWriteDataSource) {
        return extendConnectionPoolWithQuery(readWriteDataSource.dataSource);
    throw new Error('No suitable data source found');
   * Returns the read-only provider if it exists, otherwise returns the original provider if options is not provided or if options.allowFallbackToReadWrite is true.
  export function GetReadOnlyProvider(providers: Array<ProviderInfo>, options?: {allowFallbackToReadWrite: boolean}): DatabaseProviderBase {
    if (!providers || providers.length === 0) 
      return null; // no providers available
    const readOnlyProvider = providers.find((p) => p.type === 'Read-Only');
    if (readOnlyProvider) {
      return readOnlyProvider.provider;
    else if (options?.allowFallbackToReadWrite) {
      return providers[0].provider; // if no read-only provider is provided, use the original provider since we are allowed to fallback to read-write
      return null; // no read only provider available and we are not allowed to fallback to read-write
   * Returns the read-write provider if it exists, otherwise returns the original provider if options is not provided or if options.allowFallbackToReadOnly is true.
  export function GetReadWriteProvider(providers: Array<ProviderInfo>, options?: {allowFallbackToReadOnly: boolean}): DatabaseProviderBase {
    const readWriteProvider = providers.find((p) => p.type === 'Read-Write');
    if (readWriteProvider) {
      return readWriteProvider.provider;
    else if (options?.allowFallbackToReadOnly) {
      return GetReadOnlyProvider(providers, { allowFallbackToReadWrite: false }); // if no read-write provider is provided, use the read-only provider since we are allowed to fallback to read-only
      return null; // no read-write provider available and we are not allowed to fallback to read-only
   * Returns the read-write data source if it exists, otherwise throws an error.
  export function GetReadWriteDataSource(dataSources: DataSourceInfo[]): sql.ConnectionPool & { query: (sql: string, params?: any) => Promise<any[]> } {
    throw new Error('No suitable read-write data source found');
   * Extends a ConnectionPool with a query method that returns results in the format expected by generated code
   * This provides backwards compatibility with code that expects TypeORM-style query results
  export function extendConnectionPoolWithQuery(pool: sql.ConnectionPool): sql.ConnectionPool & { query: (sql: string, params?: any) => Promise<any[]> } {
    const extendedPool = pool as any;
    extendedPool.query = async (sqlQuery: string, parameters?: any): Promise<any[]> => {
      // Add parameters if provided
        if (Array.isArray(parameters)) {
          parameters.forEach((value, index) => {
            request.input(`p${index}`, value);
          // Replace ? with @p0, @p1, etc. in the query
          let paramIndex = 0;
          sqlQuery = sqlQuery.replace(/\?/g, () => `@p${paramIndex++}`);
      const result = await request.query(sqlQuery);
      return result.recordset || [];
    return extendedPool;
import { MJFileStorageProviderEntity, MJFileStorageAccountEntity } from '@memberjunction/core-entities';
import { LogStatus, UserInfo } from '@memberjunction/core';
import mime from 'mime-types';
import { FileStorageBase, FileSearchOptions, FileSearchResult, StorageProviderConfig } from './generic/FileStorageBase';
 * Callback function called when a new refresh token is issued.
 * This is important for providers like Box that issue new refresh tokens with each refresh.
export type TokenRefreshCallback = (newRefreshToken: string, newAccessToken?: string) => Promise<void>;
 * Configuration for OAuth-enabled storage providers.
 * This is the format expected by the Google Drive and similar OAuth-based drivers.
export interface OAuthStorageConfig {
  /** OAuth2 Client ID (from app registration) */
  clientID: string;
  /** OAuth2 Client Secret (from app registration) */
  /** User's OAuth refresh token (long-lived) */
  /** Optional root folder ID to restrict operations */
  rootFolderID?: string;
   * Callback called when a new refresh token is issued.
   * Required for providers like Box that issue new refresh tokens with each token refresh.
  onTokenRefresh?: TokenRefreshCallback;
 * Options for initializing a storage driver with user-specific credentials.
export interface UserStorageDriverOptions {
  /** The file storage provider entity */
  providerEntity: MJFileStorageProviderEntity;
  /** The user's ID for loading their OAuth tokens */
  userID: string;
  /** Context user for database operations */
 * @deprecated This function is being replaced by the enterprise file storage model.
 * Use FileStorageAccount with Credential Engine instead.
 * Initializes a storage driver with user-specific OAuth credentials.
 * NOTE: This function currently only supports non-OAuth providers.
 * OAuth provider support will be added via the Credential Engine integration.
 * @param options - Configuration options including provider, user ID, and context
 * @returns A promise resolving to an initialized FileStorageBase driver
 * @throws Error if the provider requires OAuth (not yet supported in enterprise model)
export async function initializeDriverWithUserCredentials(options: UserStorageDriverOptions): Promise<FileStorageBase> {
  const { providerEntity } = options;
  // Create the driver instance
  const driver = MJGlobal.Instance.ClassFactory.CreateInstance<FileStorageBase>(FileStorageBase, providerEntity.ServerDriverKey);
  // Check if this provider requires OAuth authentication
  if (providerEntity.RequiresOAuth) {
    // TODO: Implement Credential Engine integration for OAuth providers
    // This will load credentials from FileStorageAccount -> Credential
      `Provider "${providerEntity.Name}" requires OAuth authentication. ` + `Enterprise OAuth support via Credential Engine is not yet implemented.`,
    // Provider doesn't require OAuth - use admin/global configuration
    const configJson = providerEntity.Configuration;
    if (configJson) {
      const config = JSON.parse(configJson);
      await driver.initialize(config);
      console.log(`[initializeDriverWithUserCredentials] Initialized ${providerEntity.Name} with admin configuration`);
// TODO: This will be replaced with Credential Engine integration
// The loadUserOAuthCredentials function has been removed as part of the
// enterprise file storage refactoring. Credentials will be loaded from
// the FileStorageAccount -> Credential relationship using the Credential Engine.
 * Options for user context in storage operations.
 * Required for OAuth providers to access user-specific credentials.
export interface UserContextOptions {
 * Options for initializing a storage driver with account-based credentials.
 * This is the new enterprise model where credentials are stored in FileStorageAccount.
export interface AccountStorageDriverOptions {
  /** The file storage account entity (contains CredentialID) */
  accountEntity: MJFileStorageAccountEntity;
  /** The file storage provider entity (contains driver configuration) */
  /** Context user for database operations and credential access */
 * Initializes a storage driver using account-based credentials from the Credential Engine.
 * This is the enterprise model where credentials are stored in the Credential entity
 * and decrypted at runtime.
 * For providers that issue new refresh tokens on each token refresh (like Box.com),
 * this function automatically configures a callback to persist the new tokens back
 * to the Credential entity in the database.
 * @param options - Configuration options including account, provider, and context user
 * const driver = await initializeDriverWithAccountCredentials({
 *   accountEntity,
 *   providerEntity,
 * // Driver is now ready to use
 * const objects = await driver.ListObjects('/');
export async function initializeDriverWithAccountCredentials(options: AccountStorageDriverOptions): Promise<FileStorageBase> {
  const { accountEntity, providerEntity, contextUser } = options;
  // Create the driver instance using the provider's driver key
    throw new Error(`Failed to create storage driver for provider "${providerEntity.Name}" ` + `with driver key "${providerEntity.ServerDriverKey}"`);
  // Build the base config with account information (required in enterprise model)
  const baseConfig: StorageProviderConfig = {
    accountId: accountEntity.ID,
    accountName: accountEntity.Name,
  // Check if the account has a credential configured
  if (accountEntity.CredentialID) {
    // Initialize the Credential Engine if not already done
    // Get the credential by ID and decrypt it
    const credentialEntity = CredentialEngine.Instance.getCredentialById(accountEntity.CredentialID);
    if (!credentialEntity) {
      throw new Error(`Credential with ID ${accountEntity.CredentialID} not found for account "${accountEntity.Name}"`);
    // Resolve the credential to get decrypted values
    const resolved = await CredentialEngine.Instance.getCredential(credentialEntity.Name, {
      credentialId: accountEntity.CredentialID,
      subsystem: 'FileStorage',
    LogStatus(`[initializeDriverWithAccountCredentials] Decrypted credential "${credentialEntity.Name}" for account "${accountEntity.Name}"`);
    // Create a token refresh callback to persist new tokens back to the database
    // This is critical for providers like Box.com that issue new refresh tokens on each refresh
    const onTokenRefresh: TokenRefreshCallback = async (newRefreshToken: string, newAccessToken?: string) => {
        LogStatus(`[initializeDriverWithAccountCredentials] Token refresh callback invoked for account "${accountEntity.Name}"`);
        // Get the current credential values and update with new tokens
        const updatedValues: Record<string, string> = { ...resolved.values };
        updatedValues.refreshToken = newRefreshToken;
        if (newAccessToken) {
          updatedValues.accessToken = newAccessToken;
        // Update the credential in the database using the Credential Engine
        await CredentialEngine.Instance.updateCredential(accountEntity.CredentialID, updatedValues, contextUser);
        LogStatus(`[initializeDriverWithAccountCredentials] Successfully persisted new tokens for account "${accountEntity.Name}"`);
        // Log the error but don't throw - the driver still has the tokens in memory
        // and can continue operating. However, the next server restart will fail.
        console.error(`[initializeDriverWithAccountCredentials] Failed to persist new tokens for account "${accountEntity.Name}": ${errorMessage}`);
        console.error(`[initializeDriverWithAccountCredentials] WARNING: Authentication may fail after server restart!`);
    // Add the token refresh callback to the config
    baseConfig.onTokenRefresh = onTokenRefresh;
    // Initialize the driver with account info + decrypted credential values + token refresh callback
    await driver.initialize({
      ...resolved.values,
    // No credential configured - fall back to provider's static configuration
      // Merge account info with provider configuration
      LogStatus(`[initializeDriverWithAccountCredentials] Initialized "${accountEntity.Name}" with provider configuration (no credential)`);
      // Initialize with just account info (driver may use env vars or other config)
      await driver.initialize(baseConfig);
      LogStatus(`[initializeDriverWithAccountCredentials] Warning: No credential or configuration for account "${accountEntity.Name}"`);
 * Extended user context options that can also include account information
 * for the enterprise credential model.
export interface ExtendedUserContextOptions extends UserContextOptions {
  /** Optional account entity for enterprise credential model */
  accountEntity?: MJFileStorageAccountEntity;
 * Internal helper to initialize a storage driver with appropriate credentials.
 * 1. Enterprise model (preferred): When accountEntity is provided in userContext,
 *    uses the Credential Engine to decrypt credentials from the account's CredentialID.
 * 2. Legacy model: Uses provider configuration or throws for OAuth providers.
 * @param providerEntity - The file storage provider entity
 * @param userContext - Optional user context, may include accountEntity for enterprise model
 * @returns An initialized FileStorageBase driver
async function initializeDriver(providerEntity: MJFileStorageProviderEntity, userContext?: ExtendedUserContextOptions): Promise<FileStorageBase> {
  // Enterprise model: Use account-based credentials if accountEntity is provided
  if (userContext?.accountEntity) {
      accountEntity: userContext.accountEntity,
      contextUser: userContext.contextUser,
  // Legacy model: Use the deprecated user credentials approach
  if (userContext) {
    return initializeDriverWithUserCredentials({
      userID: userContext.userID,
  // No user context - use admin/legacy initialization
  // Check if this provider requires OAuth but no user context was provided
    throw new Error(`Provider "${providerEntity.Name}" requires OAuth authentication. ` + `Please provide userContext parameter with userID and contextUser.`);
  // Initialize with admin configuration
  const configJson = providerEntity.Get('Configuration') as string | null;
 * Creates a pre-authenticated upload URL for a file in the specified file storage provider.
 * This utility function simplifies the process of creating upload URLs by handling common
 * tasks such as:
 * - Setting the content type based on the file extension if not provided
 * - Setting the file status to 'Uploading'
 * - Managing the provider key if returned by the storage provider
 * The function returns both the updated input object (with additional metadata) and the
 * pre-authenticated upload URL that can be used by clients to upload the file directly
 * to the storage provider.
 * @param providerEntity - The file storage provider entity containing connection details
 * @param input - The input object containing the file details:
 *               - ID: A unique identifier for the file
 *               - Name: The filename to use for storage
 *               - ProviderID: The ID of the storage provider to use
 *               - ContentType: (Optional) The MIME type of the file
 *               - ProviderKey: (Optional) Provider-specific key for the file
 * @param userContext - Optional user context for OAuth providers (required if provider.RequiresOAuth is true)
 * @returns A promise that resolves to an object containing:
 *         - updatedInput: The input object with additional metadata (Status, ContentType, and possibly ProviderKey)
 *         - UploadUrl: The pre-authenticated URL for uploading the file
 * // Create a pre-authenticated upload URL for a PDF document
 * const fileStorageProvider = await entityMgr.FindById('FileStorageProvider', 'azure-main');
 * const result = await createUploadUrl(
 *   fileStorageProvider,
 *     ID: '123',
 *     Name: 'report.pdf',
 *     ProviderID: fileStorageProvider.ID
 *   { userID: currentUser.ID, contextUser } // Required for OAuth providers
 * // The content type is automatically determined from the file extension
 * console.log(result.updatedInput.ContentType); // 'application/pdf'
 * // The status is set to 'Uploading'
 * console.log(result.updatedInput.Status); // 'Uploading'
 * // The upload URL can be sent to the client for direct upload
 * console.log(result.UploadUrl);
export const createUploadUrl = async <TInput extends { ID: string; Name: string; ProviderID: string; ContentType?: string; ProviderKey?: string }>(
  input: TInput,
  userContext?: UserContextOptions,
  updatedInput: TInput & { Status: string; ContentType: string };
}> => {
  const { Name, ProviderID } = input;
  const ContentType = input.ContentType ?? mime.lookup(input.Name) ?? 'application/octet-stream';
  const Status = 'Uploading';
  await providerEntity.Load(ProviderID);
  // Initialize driver with user credentials if available, otherwise use admin config
  const driver = await initializeDriver(providerEntity, userContext);
  const { UploadUrl, ...maybeProviderKey } = await driver.CreatePreAuthUploadUrl(Name);
  const updatedInput = { ...input, ...maybeProviderKey, ContentType, Status };
  return { updatedInput, UploadUrl };
 * Creates a pre-authenticated download URL for a file from the specified file storage provider.
 * This utility function simplifies the process of generating download URLs by instantiating
 * the appropriate storage provider driver and delegating to its CreatePreAuthDownloadUrl method.
 * The returned URL can be provided directly to clients for downloading the file without
 * requiring additional authentication.
 * @param providerKeyOrName - The provider key or name of the file to download
 *                           (use the ProviderKey if it was returned during upload, otherwise use the file Name)
 * @returns A promise that resolves to the pre-authenticated download URL as a string
 * // Get a pre-authenticated download URL for a file
 * // Using the file name
 * const downloadUrl = await createDownloadUrl(fileStorageProvider, 'reports/annual-report.pdf', userContext);
 * // Or using the provider key if returned during upload
 * const downloadUrl = await createDownloadUrl(fileStorageProvider, file.ProviderKey, userContext);
 * // The download URL can be provided to clients for direct download
 * console.log(downloadUrl);
export const createDownloadUrl = async (
  providerKeyOrName: string,
): Promise<string> => {
  return driver.CreatePreAuthDownloadUrl(providerKeyOrName);
 * Moves an object from one location to another within the specified file storage provider.
 * This utility function handles moving files by instantiating the appropriate storage
 * provider driver and delegating to its MoveObject method. It can be used to rename files
 * or move them to different directories within the same storage provider.
 * @param oldProviderKeyOrName - The key or name of the object's current location
 *                              (use the ProviderKey if it was returned during upload, otherwise use the file Name)
 * @param newProviderKeyOrName - The key or name for the object's new location
 * @returns A promise that resolves to a boolean indicating whether the move operation was successful
 * // Move a file from one location to another
 * // Move a file to a different directory
 * const success = await moveObject(
 *   'drafts/report.docx',
 *   'published/final-report.docx',
 *   userContext
 * if (success) {
 *   console.log('File successfully moved');
 *   console.log('Failed to move file');
export const moveObject = async (
  oldProviderKeyOrName: string,
  newProviderKeyOrName: string,
): Promise<boolean> => {
  return driver.MoveObject(oldProviderKeyOrName, newProviderKeyOrName);
 * Copies an object from one location to another within the specified file storage provider.
 * This utility function handles copying files by instantiating the appropriate storage
 * provider driver and delegating to its CopyObject method. It can be used to duplicate files
 * within the same storage provider, either in the same folder with a different name or to
 * a different folder.
 * @param sourceProviderKeyOrName - The key or name of the source file to copy
 * @param destinationProviderKeyOrName - The key or name for the destination copy
 * @returns A promise that resolves to a boolean indicating whether the copy was successful
 * const success = await copyObject(
 *   'documents/archive/report-2024.pdf',
 *   console.log('File successfully copied');
 *   console.log('Failed to copy file');
export const copyObject = async (
  sourceProviderKeyOrName: string,
  destinationProviderKeyOrName: string,
  return driver.CopyObject(sourceProviderKeyOrName, destinationProviderKeyOrName);
 * Deletes a file from the specified file storage provider.
 * This utility function handles file deletion by instantiating the appropriate storage
 * provider driver and delegating to its DeleteObject method. It provides a simple way
 * to remove files that are no longer needed.
 * @param providerKeyOrName - The key or name of the file to delete
 * @returns A promise that resolves to a boolean indicating whether the deletion was successful
 * // Delete a file from storage
 * // Delete using the file name
 * const deleted = await deleteObject(fileStorageProvider, 'temp/obsolete-document.pdf', userContext);
 * const deleted = await deleteObject(fileStorageProvider, file.ProviderKey, userContext);
 * if (deleted) {
 *   console.log('File successfully deleted');
 *   console.log('Failed to delete file - it may not exist or there was an error');
export const deleteObject = async (
  console.log('[deleteObject] Called with:', {
    providerKeyOrName,
  console.log('[deleteObject] Driver initialized:', {
    driverType: driver.constructor.name,
    hasDeleteMethod: typeof driver.DeleteObject === 'function',
  console.log('[deleteObject] Calling driver.DeleteObject...');
  const result = await driver.DeleteObject(providerKeyOrName);
  console.log('[deleteObject] Result:', result);
 * Lists objects (files) and prefixes (directories) in a storage provider at the specified path.
 * This utility function provides access to the storage provider's file and folder listing
 * functionality. It returns both files and directories found at the specified path prefix,
 * allowing for hierarchical navigation through the storage provider's contents.
 * @param prefix - The path prefix to list objects from (e.g., "/" for root, "documents/" for a specific folder)
 * @param delimiter - The character used to group keys into a hierarchy (defaults to "/")
 * @returns A promise that resolves to a StorageListResult containing:
 *          - objects: Array of file metadata (name, size, contentType, lastModified, etc.)
 *          - prefixes: Array of directory/folder path strings
 * // List contents of the root directory
 * const fileStorageProvider = await entityMgr.FindById('FileStorageProvider', 'aws-s3-main');
 * const result = await listObjects(fileStorageProvider, '/', '/', userContext);
 * // Display files
 * for (const file of result.objects) {
 *   console.log(`File: ${file.name} (${file.size} bytes)`);
 * // Display folders
 * for (const folder of result.prefixes) {
 *   console.log(`Folder: ${folder}`);
 * // List contents of a specific folder
 * const docsResult = await listObjects(fileStorageProvider, 'documents/', '/', userContext);
export const listObjects = async (
  prefix: string,
  delimiter: string = '/',
): Promise<import('./generic/FileStorageBase').StorageListResult> => {
  console.log('[listObjects] Starting with:', {
    delimiter,
    hasUserContext: !!userContext,
  console.log('[listObjects] Driver initialized:', {
    isConfigured: driver.IsConfigured,
  const result = await driver.ListObjects(prefix, delimiter);
  console.log('[listObjects] Result:', {
 * Result of a cross-provider copy operation
export interface CopyBetweenProvidersResult {
  sourceProvider: string;
  destinationProvider: string;
 * Options for cross-provider copy operations.
export interface CopyBetweenProvidersOptions {
  /** User context for the source provider (required if source provider.RequiresOAuth is true). Can include accountEntity for enterprise credential model. */
  sourceUserContext?: ExtendedUserContextOptions;
  /** User context for the destination provider (required if destination provider.RequiresOAuth is true). Can include accountEntity for enterprise credential model. */
  destinationUserContext?: ExtendedUserContextOptions;
 * Copies a file from one storage provider to another.
 * This utility function enables transferring files between different storage providers
 * (e.g., from Dropbox to Google Drive, or from S3 to Azure). The transfer happens
 * server-side, so the file data flows: Source Provider → Server → Destination Provider.
 * @param sourceProviderEntity - The source file storage provider entity
 * @param destinationProviderEntity - The destination file storage provider entity
 * @param sourcePath - The path to the file in the source provider
 * @param destinationPath - The path where the file should be saved in the destination provider
 * @param options - Optional user context for OAuth providers
 * @returns A promise that resolves to a CopyBetweenProvidersResult
 * // Copy a file from Dropbox to Google Drive
 * const sourceProvider = await entityMgr.FindById('FileStorageProvider', 'dropbox-id');
 * const destProvider = await entityMgr.FindById('FileStorageProvider', 'gdrive-id');
 * const result = await copyObjectBetweenProviders(
 *   sourceProvider,
 *   destProvider,
 *   'imported/report.pdf',
 *     sourceUserContext: { userID: currentUser.ID, contextUser },
 *     destinationUserContext: { userID: currentUser.ID, contextUser }
 *   console.log(`Transferred ${result.bytesTransferred} bytes`);
export const copyObjectBetweenProviders = async (
  sourceProviderEntity: MJFileStorageProviderEntity,
  destinationProviderEntity: MJFileStorageProviderEntity,
  destinationPath: string,
  options?: CopyBetweenProvidersOptions,
): Promise<CopyBetweenProvidersResult> => {
  console.log('[copyObjectBetweenProviders] Starting transfer:', {
    sourceProvider: sourceProviderEntity.Name,
    destinationProvider: destinationProviderEntity.Name,
  const result: CopyBetweenProvidersResult = {
    message: '',
    // Initialize source driver with user credentials if available
    const sourceDriver = await initializeDriver(sourceProviderEntity, options?.sourceUserContext);
    // Initialize destination driver with user credentials if available
    const destDriver = await initializeDriver(destinationProviderEntity, options?.destinationUserContext);
    console.log('[copyObjectBetweenProviders] Drivers initialized, fetching file from source...');
    // Normalize source path: remove leading/trailing slashes and collapse multiple slashes
    // This ensures consistent path handling across different storage providers
    const normalizedSourcePath = sourcePath.replace(/^\/+|\/+$/g, '').replace(/\/+/g, '/');
    console.log('[copyObjectBetweenProviders] Path normalization:', {
      original: sourcePath,
      normalized: normalizedSourcePath,
    // Get the file from source provider
    const fileData = await sourceDriver.GetObject({ fullPath: normalizedSourcePath });
    if (!fileData || fileData.length === 0) {
      result.message = `Failed to retrieve file from source: ${normalizedSourcePath}`;
      console.error('[copyObjectBetweenProviders]', result.message);
    console.log('[copyObjectBetweenProviders] File retrieved, size:', fileData.length, 'bytes');
    // Get metadata for content type
    let contentType = 'application/octet-stream';
      const metadata = await sourceDriver.GetObjectMetadata({ fullPath: normalizedSourcePath });
      if (metadata.contentType) {
        contentType = metadata.contentType;
      // Use mime-types to guess content type from filename
      const mimeType = mime.lookup(sourcePath);
      if (mimeType) {
        contentType = mimeType;
    console.log('[copyObjectBetweenProviders] Uploading to destination with contentType:', contentType);
    // Upload to destination provider
    const uploadSuccess = await destDriver.PutObject(destinationPath, fileData, contentType);
    if (uploadSuccess) {
      result.bytesTransferred = fileData.length;
      result.message = `Successfully copied ${fileData.length} bytes from ${sourceProviderEntity.Name} to ${destinationProviderEntity.Name}`;
      console.log('[copyObjectBetweenProviders]', result.message);
      result.message = `Failed to upload file to destination: ${destinationPath}`;
    result.message = `Transfer failed: ${errorMessage}`;
    console.error('[copyObjectBetweenProviders] Error:', error);
 * Result from a single provider's search attempt
export interface ProviderSearchResult {
  /** Provider ID */
  providerID: string;
  /** Provider name for display */
  providerName: string;
  /** Whether this provider's search succeeded */
  /** Error message if search failed or provider doesn't support search */
  /** Search results (empty array if failed) */
  /** Total matches from this provider */
  /** Whether there are more results available */
  /** Pagination token for this provider */
 * Aggregated results from searching across multiple providers
  /** Results grouped by provider */
  providerResults: ProviderSearchResult[];
  /** Total results across all providers */
  /** Number of providers that succeeded */
  successfulProviders: number;
  /** Number of providers that failed */
  failedProviders: number;
 * Options for multi-provider search
export interface SearchAcrossProvidersOptions {
  /** Maximum results per provider (default: 50) */
  maxResultsPerProvider?: number;
  /** File types to filter by (e.g., ['pdf', 'docx']) */
  /** Whether to search file contents (default: false) */
  /** User context for OAuth providers - maps provider ID to user context */
  providerUserContexts?: Map<string, UserContextOptions>;
 * Searches for files across multiple storage providers in parallel.
 * This utility function enables searching for files across multiple storage providers
 * simultaneously. Each provider is queried in parallel using Promise.allSettled,
 * ensuring that failures in one provider don't affect results from others.
 * Providers that don't support search (SupportsSearch = false) will return a result
 * with success=false and an appropriate error message rather than being silently skipped.
 * @param providerEntities - Array of provider entities to search
 * @param query - Search query string
 * @param options - Optional search configuration including user contexts for OAuth providers
 * @returns A promise that resolves to aggregated results grouped by provider
 * // Search across Google Drive and Dropbox
 * const providers = [googleDriveProvider, dropboxProvider];
 * const userContexts = new Map([
 *   [googleDriveProvider.ID, { userID: currentUser.ID, contextUser }],
 *   [dropboxProvider.ID, { userID: currentUser.ID, contextUser }]
 * const result = await searchAcrossProviders(providers, 'quarterly report', {
 *   maxResultsPerProvider: 25,
 *   fileTypes: ['pdf', 'docx'],
 *   providerUserContexts: userContexts
 * // Process results by provider
 * for (const providerResult of result.providerResults) {
 *   if (providerResult.success) {
 *     console.log(`${providerResult.providerName}: ${providerResult.results.length} results`);
 *     console.log(`${providerResult.providerName}: ${providerResult.errorMessage}`);
export const searchAcrossProviders = async (
  providerEntities: MJFileStorageProviderEntity[],
  options?: SearchAcrossProvidersOptions,
): Promise<MultiProviderSearchResult> => {
  console.log('[searchAcrossProviders] Starting search:', {
    providerCount: providerEntities.length,
    providers: providerEntities.map((p) => p.Name),
    options: { ...options, providerUserContexts: options?.providerUserContexts ? '[Map]' : undefined },
  const maxResults = options?.maxResultsPerProvider ?? 50;
    fileTypes: options?.fileTypes,
    searchContent: options?.searchContent ?? false,
  // Create search promises for each provider
  const searchPromises = providerEntities.map(async (providerEntity): Promise<ProviderSearchResult> => {
    const providerResult: ProviderSearchResult = {
      // Check if provider supports search
      if (!providerEntity.SupportsSearch) {
        providerResult.errorMessage = 'This provider does not support search';
        console.log(`[searchAcrossProviders] ${providerEntity.Name}: Does not support search`);
        return providerResult;
      // Get user context for this provider if available
      const userContext = options?.providerUserContexts?.get(providerEntity.ID);
      // Initialize driver with user credentials if available
      console.log(`[searchAcrossProviders] ${providerEntity.Name}: Executing search...`);
      const searchResult = await driver.SearchFiles(query, searchOptions);
      providerResult.success = true;
      providerResult.results = searchResult.results;
      providerResult.totalMatches = searchResult.totalMatches;
      providerResult.hasMore = searchResult.hasMore;
      providerResult.nextPageToken = searchResult.nextPageToken;
      console.log(`[searchAcrossProviders] ${providerEntity.Name}: Found ${searchResult.results.length} results`);
      providerResult.errorMessage = errorMessage;
      console.error(`[searchAcrossProviders] ${providerEntity.Name}: Error -`, errorMessage);
  // Execute all searches in parallel
  const settledResults = await Promise.allSettled(searchPromises);
  // Aggregate results
  const providerResults: ProviderSearchResult[] = [];
  let totalResultsReturned = 0;
  let successfulProviders = 0;
  let failedProviders = 0;
  for (const settled of settledResults) {
    if (settled.status === 'fulfilled') {
      const result = settled.value;
      providerResults.push(result);
        successfulProviders++;
        totalResultsReturned += result.results.length;
        failedProviders++;
      // Promise rejected (shouldn't happen with our try/catch, but handle it)
      console.error('[searchAcrossProviders] Promise rejected:', settled.reason);
  const aggregatedResult: MultiProviderSearchResult = {
    providerResults,
    totalResultsReturned,
    successfulProviders,
    failedProviders,
  console.log('[searchAcrossProviders] Search complete:', {
  return aggregatedResult;
 * Information needed to search a single account
export interface AccountSearchInput {
  /** The file storage account entity */
  /** The file storage provider entity for this account */
 * Result from a single account's search attempt
  /** Account ID */
  /** Account name for display */
  /** Whether this account's search succeeded */
  /** Total matches from this account */
  /** Pagination token for this account */
 * Aggregated results from searching across multiple accounts
export interface MultiAccountSearchResult {
  /** Total results across all accounts */
  /** Number of accounts that succeeded */
  /** Number of accounts that failed */
 * Options for multi-account search
export interface SearchAcrossAccountsOptions {
  /** Maximum results per account (default: 50) */
  /** Context user for credential decryption */
 * Searches for files across multiple storage accounts in parallel.
 * This is the enterprise version of search that uses the account-based credential model.
 * Each account is searched independently, allowing multiple accounts from the same
 * provider type to be searched (e.g., two different Dropbox accounts).
 * @param accounts - Array of account/provider pairs to search
 * @param options - Search configuration including context user for credentials
 * @returns A promise that resolves to aggregated results grouped by account
 * const accounts = [
 *   { accountEntity: researchDropbox, providerEntity: dropboxProvider },
 *   { accountEntity: marketingDropbox, providerEntity: dropboxProvider },
 *   { accountEntity: engineeringGDrive, providerEntity: gdriveProvider }
 * const result = await searchAcrossAccounts(accounts, 'quarterly report', {
 *   maxResultsPerAccount: 25,
 * for (const accountResult of result.accountResults) {
 *   if (accountResult.success) {
 *     console.log(`${accountResult.accountName}: ${accountResult.results.length} results`);
export const searchAcrossAccounts = async (
  accounts: AccountSearchInput[],
  options: SearchAcrossAccountsOptions,
): Promise<MultiAccountSearchResult> => {
  console.log('[searchAcrossAccounts] Starting search:', {
    accountCount: accounts.length,
    accounts: accounts.map((a) => ({ account: a.accountEntity.Name, provider: a.providerEntity.Name })),
    maxResultsPerAccount: options.maxResultsPerAccount,
    fileTypes: options.fileTypes,
    searchContent: options.searchContent,
  const maxResults = options.maxResultsPerAccount ?? 50;
    searchContent: options.searchContent ?? false,
  // Create search promises for each account
  const searchPromises = accounts.map(async ({ accountEntity, providerEntity }): Promise<AccountSearchResult> => {
    const accountResult: AccountSearchResult = {
      accountID: accountEntity.ID,
        accountResult.errorMessage = 'This provider does not support search';
        console.log(`[searchAcrossAccounts] ${accountEntity.Name}: Provider does not support search`);
        return accountResult;
      // Initialize driver with account-based credentials
        contextUser: options.contextUser,
      console.log(`[searchAcrossAccounts] ${accountEntity.Name}: Executing search...`);
      accountResult.success = true;
      accountResult.results = searchResult.results;
      accountResult.totalMatches = searchResult.totalMatches;
      accountResult.hasMore = searchResult.hasMore;
      accountResult.nextPageToken = searchResult.nextPageToken;
      console.log(`[searchAcrossAccounts] ${accountEntity.Name}: Found ${searchResult.results.length} results`);
      accountResult.errorMessage = errorMessage;
      console.error(`[searchAcrossAccounts] ${accountEntity.Name}: Error -`, errorMessage);
  const accountResults: AccountSearchResult[] = [];
  let successfulAccounts = 0;
  let failedAccounts = 0;
      accountResults.push(result);
        successfulAccounts++;
        failedAccounts++;
      console.error('[searchAcrossAccounts] Promise rejected:', settled.reason);
  const aggregatedResult: MultiAccountSearchResult = {
    successfulAccounts,
    failedAccounts,
  console.log('[searchAcrossAccounts] Search complete:', {
import { EntityFieldInfo, EntityFieldValueInfo, EntityInfo, EntityRelationshipInfo } from "@memberjunction/core";
import { SimpleEntityInfo, SimpleEntityFieldInfo } from "@memberjunction/interactive-component-types";
import { SkipEntityFieldInfo, SkipEntityFieldValueInfo, SkipEntityInfo, SkipEntityRelationshipInfo } from "./entity-metadata-types";
// EntityInfo <-> SkipEntityInfo conversions
 * Maps a MemberJunction EntityInfo object to a SkipEntityInfo object for use in the Skip query system.
 * This function transforms the comprehensive MemberJunction entity metadata into a simplified
 * format optimized for the Skip query engine, including all fields and related entities.
 * @param e - The source EntityInfo object from MemberJunction core
 * @returns A SkipEntityInfo object with mapped fields and relationships
export function MapEntityInfoToSkipEntityInfo(e: EntityInfo): SkipEntityInfo {
        fields: e.Fields.map(f => MapEntityFieldInfoToSkipEntityFieldInfo(f)),
        relatedEntities: e.RelatedEntities?.map(r => MapEntityRelationshipInfoToSkipEntityRelationshipInfo(r)) || [],
        rowsPacked: e.RowsToPackWithSchema as 'None' | 'Sample' | 'All',
        rowsSampleMethod: e.RowsToPackSampleMethod as 'random' | 'top n' | 'bottom n'
 * Maps an array of MemberJunction EntityInfo objects to SkipEntityInfo objects.
 * @param entities - Array of EntityInfo from @memberjunction/core
 * @returns Array of SkipEntityInfo objects
export function MapEntityInfoArrayToSkipEntityInfoArray(entities: EntityInfo[]): SkipEntityInfo[] {
    return entities.map(e => MapEntityInfoToSkipEntityInfo(e));
 * Converts a SkipEntityInfo to a partial EntityInfo object.
 * @param skipEntity - The SkipEntityInfo to convert
export function MapSkipEntityInfoToEntityInfo(skipEntity: SkipEntityInfo): Partial<EntityInfo> {
        ID: skipEntity.id,
        Name: skipEntity.name,
        Description: skipEntity.description,
        SchemaName: skipEntity.schemaName,
        BaseView: skipEntity.baseView,
        RowsToPackWithSchema: skipEntity.rowsPacked,
        RowsToPackSampleMethod: skipEntity.rowsSampleMethod
// EntityFieldInfo <-> SkipEntityFieldInfo conversions
 * Maps a MemberJunction EntityFieldInfo object to a SkipEntityFieldInfo object.
 * This function converts detailed field metadata from MemberJunction's format to the Skip
 * query system format, preserving all field properties including type information, constraints,
 * relationships, and validation rules.
 * @param f - The EntityFieldInfo object to be mapped
 * @returns A SkipEntityFieldInfo object with all field properties mapped
export function MapEntityFieldInfoToSkipEntityFieldInfo(f: EntityFieldInfo): SkipEntityFieldInfo {
        possibleValues: f.EntityFieldValues?.map(v => ({
            displayValue: v.Code !== v.Value ? v.Code : undefined
 * Converts a SkipEntityFieldInfo to a partial EntityFieldInfo object.
 * @param skipField - The SkipEntityFieldInfo to convert
export function MapSkipEntityFieldInfoToEntityFieldInfo(skipField: SkipEntityFieldInfo): Partial<EntityFieldInfo> {
        EntityID: skipField.entityID,
        Sequence: skipField.sequence,
        Name: skipField.name,
        DisplayName: skipField.displayName,
        Description: skipField.description,
        IsPrimaryKey: skipField.isPrimaryKey,
        IsUnique: skipField.isUnique,
        Category: skipField.category,
        Type: skipField.type,
        Length: skipField.length,
        Precision: skipField.precision,
        Scale: skipField.scale,
        AllowsNull: skipField.allowsNull,
        DefaultValue: skipField.defaultValue,
        AutoIncrement: skipField.autoIncrement,
        ValueListType: skipField.valueListType,
        ExtendedType: skipField.extendedType,
        DefaultInView: skipField.defaultInView,
        DefaultColumnWidth: skipField.defaultColumnWidth,
        IsVirtual: skipField.isVirtual,
        IsNameField: skipField.isNameField,
        RelatedEntityID: skipField.relatedEntityID,
        RelatedEntityFieldName: skipField.relatedEntityFieldName
// EntityFieldValueInfo <-> SkipEntityFieldValueInfo conversions
 * Maps a MemberJunction EntityFieldValueInfo object to a SkipEntityFieldValueInfo object.
 * This function converts possible field values (used for dropdown lists, enums, and constraints)
 * from MemberJunction's format to the Skip query system format. These values represent the
 * allowed values for fields with restricted value lists.
 * @param pv - The EntityFieldValueInfo object representing a possible value
 * @returns A SkipEntityFieldValueInfo object with mapped value information
export function MapEntityFieldValueInfoToSkipEntityFieldValueInfo(pv: EntityFieldValueInfo): SkipEntityFieldValueInfo {
        value: pv.Value,
        displayValue: pv.Value
// EntityRelationshipInfo <-> SkipEntityRelationshipInfo conversions
 * Maps a MemberJunction EntityRelationshipInfo object to a SkipEntityRelationshipInfo object.
 * This function converts entity relationship metadata from MemberJunction's format to the Skip
 * query system format. Relationships define how entities are connected through foreign keys,
 * joins, and other associations, enabling complex queries across related data.
 * @param re - The EntityRelationshipInfo object to be mapped
 * @returns A SkipEntityRelationshipInfo object with all relationship properties mapped
export function MapEntityRelationshipInfoToSkipEntityRelationshipInfo(re: EntityRelationshipInfo): SkipEntityRelationshipInfo {
        entityID: re.EntityID,
        entity: re.Entity,
        entityBaseView: re.EntityBaseView,
        entityKeyField: re.EntityKeyField,
        relatedEntityID: re.RelatedEntityID,
        relatedEntityJoinField: re.RelatedEntityJoinField,
        relatedEntityBaseView: re.RelatedEntityBaseView,
        relatedEntity: re.RelatedEntity,
        type: re.Type,
        joinEntityInverseJoinField: re.JoinEntityInverseJoinField,
        joinView: re.JoinView,
        joinEntityJoinField: re.JoinEntityJoinField,
// SimpleEntityInfo <-> SkipEntityInfo conversions
 * Maps a SimpleEntityInfo object to a SkipEntityInfo object.
 * @param simpleEntity - The SimpleEntityInfo from @memberjunction/interactive-component-types
 * @returns A SkipEntityInfo object
export function MapSimpleEntityInfoToSkipEntityInfo(simpleEntity: SimpleEntityInfo): SkipEntityInfo {
        name: simpleEntity.name,
        description: simpleEntity.description,
        schemaName: '',
        baseView: '',
        fields: simpleEntity.fields.map(f => MapSimpleEntityFieldInfoToSkipEntityFieldInfo(f)),
        relatedEntities: []
 * Maps an array of SimpleEntityInfo objects to SkipEntityInfo objects.
 * @param entities - Array of SimpleEntityInfo from @memberjunction/interactive-component-types
export function MapSimpleEntityInfoArrayToSkipEntityInfoArray(entities: SimpleEntityInfo[]): SkipEntityInfo[] {
    return entities.map(e => MapSimpleEntityInfoToSkipEntityInfo(e));
 * Converts a SkipEntityInfo to a SimpleEntityInfo object.
 * @returns A new SimpleEntityInfo instance
export function MapSkipEntityInfoToSimpleEntityInfo(skipEntity: SkipEntityInfo): SimpleEntityInfo {
        name: skipEntity.name,
        description: skipEntity.description,
        fields: skipEntity.fields.map(f => MapSkipEntityFieldInfoToSimpleEntityFieldInfo(f))
// SimpleEntityFieldInfo <-> SkipEntityFieldInfo conversions
 * Maps a SimpleEntityFieldInfo object to a SkipEntityFieldInfo object.
 * @param simpleField - The SimpleEntityFieldInfo from @memberjunction/interactive-component-types
 * @returns A SkipEntityFieldInfo object
export function MapSimpleEntityFieldInfoToSkipEntityFieldInfo(simpleField: SimpleEntityFieldInfo): SkipEntityFieldInfo {
        entityID: '',
        sequence: simpleField.sequence,
        name: simpleField.name,
        displayName: undefined,
        description: simpleField.description,
        isPrimaryKey: simpleField.isPrimaryKey,
        isUnique: false,
        category: undefined,
        type: simpleField.type,
        length: 0,
        precision: 0,
        scale: 0,
        sqlFullType: '',
        allowsNull: simpleField.allowsNull,
        defaultValue: '',
        autoIncrement: false,
        valueListType: undefined,
        extendedType: undefined,
        defaultInView: simpleField.defaultInView,
        defaultColumnWidth: 0,
        isVirtual: false,
        isNameField: false,
        relatedEntityID: undefined,
        relatedEntityFieldName: undefined,
        relatedEntity: undefined,
        relatedEntitySchemaName: undefined,
        relatedEntityBaseView: undefined,
        possibleValues: simpleField.possibleValues?.map(v => ({ value: v }))
 * Converts a SkipEntityFieldInfo to a SimpleEntityFieldInfo object.
export function MapSkipEntityFieldInfoToSimpleEntityFieldInfo(skipField: SkipEntityFieldInfo): SimpleEntityFieldInfo {
        name: skipField.name,
        sequence: skipField.sequence,
        defaultInView: skipField.defaultInView,
        type: skipField.type,
        allowsNull: skipField.allowsNull,
        isPrimaryKey: skipField.isPrimaryKey,
        description: skipField.description,
        possibleValues: skipField.possibleValues?.map(v => v.value)
// Helper functions for working with SkipEntityInfo
 * Helper function to check if a field exists in a SkipEntityInfo
 * @param entity - The SkipEntityInfo to check
 * @param fieldName - The field name to check
export function skipEntityHasField(entity: SkipEntityInfo, fieldName: string): boolean {
    return entity.fields.some(f => f.name === fieldName);
 * Helper function to get a field by name from a SkipEntityInfo
 * @param entity - The SkipEntityInfo to search
 * @param fieldName - The field name to find
 * @returns The SkipEntityFieldInfo if found, undefined otherwise
export function skipEntityGetField(entity: SkipEntityInfo, fieldName: string): SkipEntityFieldInfo | undefined {
    return entity.fields.find(f => f.name === fieldName);
 * Helper function to get all field names as a Set for efficient lookup
 * @param entity - The SkipEntityInfo to get field names from
 * @returns Set of all field names in the entity
export function skipEntityGetFieldNameSet(entity: SkipEntityInfo): Set<string> {
    return new Set(entity.fields.map(f => f.name));
import { appBuilderPath } from "app-builder-bin"
import { retry, Nullish, safeStringifyJson } from "builder-util-runtime"
import * as chalk from "chalk"
import { ChildProcess, execFile, ExecFileOptions, SpawnOptions } from "child_process"
import { spawn as _spawn } from "cross-spawn"
import { createHash } from "crypto"
import _debug from "debug"
import { dump } from "js-yaml"
import { install as installSourceMap } from "source-map-support"
import { getPath7za } from "./7za"
import { debug, log } from "./log"
if (process.env.JEST_WORKER_ID == null) {
  installSourceMap()
export { safeStringifyJson, retry } from "builder-util-runtime"
export { TmpDir } from "temp-file"
export * from "./arch"
export { Arch, archFromString, ArchType, defaultArchFromString, getArchCliNames, getArchSuffix, toLinuxArchString } from "./arch"
export { AsyncTaskManager } from "./asyncTaskManager"
export { DebugLogger } from "./DebugLogger"
export * from "./log"
export { httpExecutor, NodeHttpExecutor } from "./nodeHttpExecutor"
export * from "./promise"
export { asArray } from "builder-util-runtime"
export * from "./fs"
export { deepAssign } from "./deepAssign"
export { getPath7x, getPath7za } from "./7za"
export const debug7z = _debug("electron-builder:7z")
export function serializeToYaml(object: any, skipInvalid = false, noRefs = false) {
  return dump(object, {
    lineWidth: 8000,
    skipInvalid,
    noRefs,
export function removePassword(input: string): string {
  const blockList = ["--accessKey", "--secretKey", "-P", "-p", "-pass", "-String", "/p", "pass:"]
  // Create a regex pattern that supports:
  //   - space-separated unquoted values: --key value
  //   - quoted values: --key "value with spaces" or 'value with spaces'
  const blockPattern = new RegExp(`(${blockList.map(s => s.replace(/[.*+?^${}()|[\]\\]/g, "\\$&")).join("|")})\\s*(?:(["'])(.*?)\\2|([^\\s]+))`, "g")
  input = input.replace(blockPattern, (_match, prefix, quote, quotedVal, unquotedVal) => {
    const value = quotedVal ?? unquotedVal
    if (prefix.trim() === "/p" && value.startsWith("\\\\Mac\\Host\\\\")) {
      return `${prefix} ${quote ?? ""}${value}${quote ?? ""}`
    const hashed = createHash("sha256").update(value).digest("hex")
    return `${prefix} ${quote ?? ""}${hashed} (sha256 hash)${quote ?? ""}`
  // Also handle `/b ... /c` block format
  return input.replace(/(\/b\s+)(.*?)(\s+\/c)/g, (_match, p1, p2, p3) => {
    const hashed = createHash("sha256").update(p2).digest("hex")
    return `${p1}${hashed} (sha256 hash)${p3}`
function getProcessEnv(env: Record<string, string | undefined> | Nullish): NodeJS.ProcessEnv | undefined {
  if (process.platform === "win32") {
    return env == null ? undefined : env
  const finalEnv = {
    ...(env || process.env),
  // without LC_CTYPE dpkg can returns encoded unicode symbols
  // set LC_CTYPE to avoid crash https://github.com/electron-userland/electron-builder/issues/503 Even "en_DE.UTF-8" leads to error.
  const locale = process.platform === "linux" ? process.env.LANG || "C.UTF-8" : "en_US.UTF-8"
  finalEnv.LANG = locale
  finalEnv.LC_CTYPE = locale
  finalEnv.LC_ALL = locale
  return finalEnv
export function exec(file: string, args?: Array<string> | null, options?: ExecFileOptions, isLogOutIfDebug = true): Promise<string> {
  if (log.isDebugEnabled) {
    const logFields: any = {
      args: args == null ? "" : removePassword(args.join(" ")),
        logFields.cwd = options.cwd
      if (options.env != null) {
        const diffEnv = { ...options.env }
        for (const name of Object.keys(process.env)) {
          if (process.env[name] === options.env[name]) {
            delete diffEnv[name]
        logFields.env = safeStringifyJson(diffEnv)
    log.debug(logFields, "executing")
    execFile(
        maxBuffer: 1000 * 1024 * 1024,
        env: getProcessEnv(options == null ? null : options.env),
      (error, stdout, stderr) => {
        if (error == null) {
          if (isLogOutIfDebug && log.isDebugEnabled) {
            if (stdout.length > 0) {
              logFields.stdout = stdout
            if (stderr.length > 0) {
              logFields.stderr = stderr
            log.debug(logFields, "executed")
          resolve(stdout.toString())
          let message = chalk.red(removePassword(`Exit code: ${(error as any).code}. ${error.message}`))
          if (stdout.length !== 0) {
            if (file.endsWith("wine")) {
              stdout = stdout.toString()
            message += `\n${chalk.yellow(stdout.toString())}`
          if (stderr.length !== 0) {
              stderr = stderr.toString()
            message += `\n${chalk.red(stderr.toString())}`
          // TODO: switch to ECMA Script 2026 Error class with `cause` property to return stack trace
          reject(new ExecError(file, (error as any).code, message, "", `${error.code || ExecError.code}`))
export interface ExtraSpawnOptions {
  isPipeInput?: boolean
function logSpawn(command: string, args: Array<string>, options: SpawnOptions) {
  // use general debug.enabled to log spawn, because it doesn't produce a lot of output (the only line), but important in any case
  if (!log.isDebugEnabled) {
  const argsString = removePassword(args.join(" "))
    command: command + " " + (command === "docker" ? argsString : removePassword(argsString)),
  if (options != null && options.cwd != null) {
  log.debug(logFields, "spawning")
export function doSpawn(command: string, args: Array<string>, options?: SpawnOptions, extraOptions?: ExtraSpawnOptions): ChildProcess {
    options = {}
  options.env = getProcessEnv(options.env)
  if (options.stdio == null) {
    const isDebugEnabled = debug.enabled
    // do not ignore stdout/stderr if not debug, because in this case we will read into buffer and print on error
    options.stdio = [extraOptions != null && extraOptions.isPipeInput ? "pipe" : "ignore", isDebugEnabled ? "inherit" : "pipe", isDebugEnabled ? "inherit" : "pipe"] as any
  logSpawn(command, args, options)
    return _spawn(command, args, options)
    throw new Error(`Cannot spawn ${command}: ${e.stack || e}`)
export function spawnAndWrite(command: string, args: Array<string>, data: string, options?: SpawnOptions) {
  const childProcess = doSpawn(command, args, options, { isPipeInput: true })
  const timeout = setTimeout(() => childProcess.kill(), 4 * 60 * 1000)
  return new Promise<any>((resolve, reject) => {
    handleProcess(
      "close",
      childProcess,
          clearTimeout(timeout)
          resolve(undefined)
      error => {
          reject(error)
    childProcess.stdin!.end(data)
export function spawn(command: string, args?: Array<string> | null, options?: SpawnOptions, extraOptions?: ExtraSpawnOptions): Promise<any> {
    handleProcess("close", doSpawn(command, args || [], options, extraOptions), command, resolve, reject)
function handleProcess(event: string, childProcess: ChildProcess, command: string, resolve: ((value?: any) => void) | null, reject: (reason?: any) => void) {
  childProcess.on("error", reject)
  let out = ""
  if (childProcess.stdout != null) {
    childProcess.stdout.on("data", (data: string) => {
      out += data
  let errorOut = ""
  if (childProcess.stderr != null) {
    childProcess.stderr.on("data", (data: string) => {
      errorOut += data
  childProcess.once(event, (code: number) => {
        command: path.basename(command),
        pid: childProcess.pid,
      if (out.length > 0) {
        fields.out = out
      log.debug(fields, "exited")
      if (resolve != null) {
        resolve(out)
      reject(new ExecError(command, code, out, errorOut))
function formatOut(text: string, title: string) {
  return text.length === 0 ? "" : `\n${title}:\n${text}`
export class ExecError extends Error {
  alreadyLogged = false
  static code = "ERR_ELECTRON_BUILDER_CANNOT_EXECUTE"
    readonly exitCode: number,
    out: string,
    errorOut: string,
    code = ExecError.code
    super(`${command} process failed ${code}${formatOut(String(exitCode), "Exit code")}${formatOut(out, "Output")}${formatOut(errorOut, "Error output")}`)
    ;(this as NodeJS.ErrnoException).code = code
export function use<T, R>(value: T | Nullish, task: (value: T) => R): R | null {
  return value == null ? null : task(value)
export function isEmptyOrSpaces(s: string | Nullish): s is "" | Nullish {
  return s == null || s.trim().length === 0
export function isTokenCharValid(token: string) {
  return /^[.\w/=+-]+$/.test(token)
export function addValue<K, T>(map: Map<K, Array<T>>, key: K, value: T) {
  const list = map.get(key)
    map.set(key, [value])
  } else if (!list.includes(value)) {
    list.push(value)
export function replaceDefault(inList: Array<string> | Nullish, defaultList: Array<string>): Array<string> {
  if (inList == null || (inList.length === 1 && inList[0] === "default")) {
    return defaultList
  const index = inList.indexOf("default")
    const list = inList.slice(0, index)
    list.push(...defaultList)
    if (index !== inList.length - 1) {
      list.push(...inList.slice(index + 1))
    inList = list
  return inList
export function getPlatformIconFileName(value: string | Nullish, isMac: boolean) {
  if (!value.includes(".")) {
    return `${value}.${isMac ? "icns" : "ico"}`
  return value.replace(isMac ? ".ico" : ".icns", isMac ? ".icns" : ".ico")
export function isPullRequest() {
  // TRAVIS_PULL_REQUEST is set to the pull request number if the current job is a pull request build, or false if it’s not.
  function isSet(value: string | undefined) {
    // value can be or null, or empty string
    return value && value !== "false"
    isSet(process.env.TRAVIS_PULL_REQUEST) ||
    isSet(process.env.CIRCLE_PULL_REQUEST) ||
    isSet(process.env.BITRISE_PULL_REQUEST) ||
    isSet(process.env.APPVEYOR_PULL_REQUEST_NUMBER) ||
    isSet(process.env.GITHUB_BASE_REF)
export function isEnvTrue(value: string | Nullish) {
    value = value.trim()
  return value === "true" || value === "" || value === "1"
export class InvalidConfigurationError extends Error {
  constructor(message: string, code = "ERR_ELECTRON_BUILDER_INVALID_CONFIGURATION") {
    super(message)
export async function executeAppBuilder(
  args: Array<string>,
  childProcessConsumer?: (childProcess: ChildProcess) => void,
  extraOptions: SpawnOptions = {},
  maxRetries = 0
  const command = appBuilderPath
  const env: any = {
    SZA_PATH: await getPath7za(),
    FORCE_COLOR: chalk.level === 0 ? "0" : "1",
  const cacheEnv = process.env.ELECTRON_BUILDER_CACHE
  if (cacheEnv != null && cacheEnv.length > 0) {
    env.ELECTRON_BUILDER_CACHE = path.resolve(cacheEnv)
  if (extraOptions.env != null) {
    Object.assign(env, extraOptions.env)
  function runCommand() {
      const childProcess = doSpawn(command, args, {
        stdio: ["ignore", "pipe", process.stdout],
        ...extraOptions,
      if (childProcessConsumer != null) {
        childProcessConsumer(childProcess)
      handleProcess("close", childProcess, command, resolve, error => {
        if (error instanceof ExecError && error.exitCode === 2) {
          error.alreadyLogged = true
  if (maxRetries === 0) {
    return runCommand()
    return retry(runCommand, { retries: maxRetries, interval: 1000 })
import { log } from "builder-util"
export const trimStringWithWarn = (str: string, maxLength: number, warnMessage: string): string => {
  if (str.length <= maxLength) {
  log.warn({ length: str.length, maxLength }, warnMessage)
  return str.substring(0, maxLength)
// if baseUrl path doesn't ends with /, this path will be not prepended to passed pathname for new URL(input, base)
/** @internal */
export function newBaseUrl(url: string): URL {
  const result = new URL(url)
  if (!result.pathname.endsWith("/")) {
    result.pathname += "/"
// addRandomQueryToAvoidCaching is false by default because in most cases URL already contains version number,
// so, it makes sense only for Generic Provider for channel files
export function newUrlFromBase(pathname: string, baseUrl: URL, addRandomQueryToAvoidCaching = false): URL {
  const result = new URL(pathname, baseUrl)
  // search is not propagated (search is an empty string if not specified)
  const search = baseUrl.search
  if (search != null && search.length !== 0) {
    result.search = search
  } else if (addRandomQueryToAvoidCaching) {
    result.search = `noCache=${Date.now().toString(32)}`
export function getChannelFilename(channel: string): string {
  return `${channel}.yml`
