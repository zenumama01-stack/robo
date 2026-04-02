import { configInfo, mj_core_schema, SQLOutputConfig } from "../Config/config";
 * Utility class for logging SQL to a run file that can be fresh for each run or appended to depending on the settings in the configuration
export class SQLLogging {
    private static _SQLLoggingFilePath: string = '';
    private static _OmitRecurringScriptsFromLog: boolean = false;
    public static get SQLLoggingFilePath(): string {
        return SQLLogging._SQLLoggingFilePath;
    public static get OmitRecurringScriptsFromLog(): boolean {
        return SQLLogging._OmitRecurringScriptsFromLog
    public static initSQLLogging() {
        SQLLogging._OmitRecurringScriptsFromLog = configInfo.SQLOutput.omitRecurringScriptsFromLog;
        if (!SQLLogging.SQLLoggingFilePath) {
            // not already set up, so proceed, otherwise we do nothing as we're already good to go
            const config = configInfo.SQLOutput;
            if(!config){
                logError("MetadataLoggingConfig is required to enable metadata logging");
            if (!config.enabled)
                return; // we are not doing anything here....
            if (config.folderPath) {
                const dirExists: boolean = fs.existsSync(config.folderPath);
                if (!dirExists) {
                    fs.mkdirSync(config.folderPath, {recursive: true });
                const fileName: string = config.fileName || this.createFileName();
                SQLLogging._SQLLoggingFilePath = path.join(config.folderPath,fileName);
                if (!config.appendToFile || !fs.existsSync(SQLLogging.SQLLoggingFilePath)) {
                    //create an empty file
                    fs.writeFileSync(SQLLogging.SQLLoggingFilePath, '');
                logStatus(`Metadata logging enabled. File path: ${SQLLogging.SQLLoggingFilePath}`);
                logError("folderPath is required to enable metadata logging");
     public static finishSQLLogging() {
        if (SQLLogging.SQLLoggingFilePath) {
            if (SQLLogging.getFileLength(SQLLogging.SQLLoggingFilePath) === 0) {
                // no content in the file, so delete it
                fs.unlinkSync(SQLLogging.SQLLoggingFilePath);
                logStatus(" >>> SQL logging file was empty and has been deleted");
            else if(configInfo.SQLOutput.convertCoreSchemaToFlywayMigrationFile){
                SQLLogging.convertSQLLogToFlywaySchema();
     protected static createFileName(): string {
        const year = date.getUTCFullYear();
        const month = String(date.getUTCMonth() + 1).padStart(2, '0'); // Month is 0-based
        const day = String(date.getUTCDate()).padStart(2, '0');
        const hour = String(date.getUTCHours()).padStart(2, '0');
        const minute = String(date.getUTCMinutes()).padStart(2, '0');
        const second = String(date.getUTCSeconds()).padStart(2, '0');
        const fileName = `CodeGen_Run_${year}-${month}-${day}_${hour}-${minute}-${second}.sql`;
     * Adds the provided SQL to the log file for the run
     * @param contents - the executable SQL to log
     * @param description - a description of what is being logged that will be emitted and wrapped in comments
    public static async appendToSQLLogFile(contents: string, description?: string, isRecurringScript: boolean = false): Promise<void> {
            if (isRecurringScript && SQLLogging.OmitRecurringScriptsFromLog) {
                return; // is a recurring script and the flag to omit recurring scripts is set
            if(!contents || !SQLLogging.SQLLoggingFilePath){
            if(description){
                const comment = `/* ${description} */\n`;
                contents = `${comment}${contents}`;
            contents = `${contents}\n\n`;
            fs.appendFileSync(SQLLogging.SQLLoggingFilePath, contents);
           logError("Unable to log metadata SQL text to file", ex);
    * Executes the given SQL query using the given DataSource object.
    * @param ds - The DataSource object to use to execute the query.
    public static async LogSQLAndExecute(ds: sql.ConnectionPool, query: string, description?: string, isRecurringScript: boolean = false): Promise<any> {
        SQLLogging.appendToSQLLogFile(query, description, isRecurringScript);
        const result = await ds.request().query(query);
    protected static getFileLength(filePath: string): number {
            const stats = fs.statSync(filePath);
            return stats.size;
    protected static convertSQLLogToFlywaySchema(): void {
        if(!this.SQLLoggingFilePath || !configInfo.SQLOutput.convertCoreSchemaToFlywayMigrationFile){
        let data: string = fs.readFileSync(this.SQLLoggingFilePath, 'utf-8');
        // Get schema placeholder mappings, defaulting to legacy behavior if not specified
        const schemaPlaceholders = configInfo.SQLOutput.schemaPlaceholders || [
            { schema: mj_core_schema(), placeholder: '${flyway:defaultSchema}' }
        // Apply each schema-to-placeholder mapping in order
        for (const mapping of schemaPlaceholders) {
            // Negative lookahead to avoid matching schema_CreatedAt, schema_UpdatedAt, etc.
            const regex: RegExp = new RegExp(`${this.escapeRegex(mapping.schema)}(?!(_(\\w+)At))`, 'g');
            const beforeCount = (data.match(regex) || []).length;
            data = data.replace(regex, mapping.placeholder);
            logStatus(`   >>> Replaced ${beforeCount} instances of ${mapping.schema} with ${mapping.placeholder}`);
        fs.writeFileSync(`${this.SQLLoggingFilePath}`, data);
        logStatus(`   >>> Flyway Migration File Completed`);
      * Escapes special regex characters in a string
      * @returns The escaped string safe for use in regex
