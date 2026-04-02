import { loadEntityConfig } from '../config';
export interface StatusOptions {
export interface StatusCallbacks {
export interface StatusResult {
  new: number;
export interface EntityStatusResult {
  directory: string;
export class StatusService {
  constructor(syncEngine: SyncEngine) {
  async checkStatus(options: StatusOptions, callbacks?: StatusCallbacks): Promise<{
    summary: StatusResult;
    details: EntityStatusResult[];
      process.cwd(),
      options.dir,
    callbacks?.onLog?.(`Found ${entityDirs.length} entity ${entityDirs.length === 1 ? 'directory' : 'directories'} to check`);
    const details: EntityStatusResult[] = [];
    let totalNew = 0;
    let totalModified = 0;
        callbacks?.onWarn?.(`Skipping ${entityDir} - no valid entity configuration`);
      callbacks?.onLog?.(`Checking ${entityConfig.entity} in ${entityDir}`);
      const result = await this.checkEntityDirectory(
      details.push({
        entityName: entityConfig.entity,
        directory: entityDir,
      totalNew += result.new;
      totalModified += result.modified;
      // Report directory summary
      if (result.new > 0 || result.modified > 0 || result.deleted > 0) {
        callbacks?.onLog?.(`  New: ${result.new}, Modified: ${result.modified}, Deleted: ${result.deleted}, Unchanged: ${result.unchanged}`);
        callbacks?.onLog?.(`  All ${result.unchanged} records are up to date`);
        new: totalNew,
        modified: totalModified,
        unchanged: totalUnchanged
      details
  private async checkEntityDirectory(
    callbacks?: StatusCallbacks
  ): Promise<StatusResult> {
    const result: StatusResult = { new: 0, modified: 0, deleted: 0, unchanged: 0 };
    // Find files matching the configured pattern
    const jsonFiles = await fastGlob(pattern, {
      ignore: ['.mj-sync.json', '.mj-folder.json', '**/*.backup'],
      dot: true  // Include dotfiles (files starting with .)
    for (const file of jsonFiles) {
        const filePath = path.join(entityDir, file);
        const recordData: RecordData = await fs.readJson(filePath);
        if (recordData.primaryKey) {
          const entity = await this.syncEngine.loadEntity(entityConfig.entity, recordData.primaryKey);
            result.deleted++;
            // Check if modified
            const currentChecksum = this.syncEngine.calculateChecksum(recordData.fields);
            if (recordData.sync?.checksum !== currentChecksum) {
              result.modified++;
              result.unchanged++;
          // New record
          result.new++;
        callbacks?.onWarn?.(`Failed to check ${file}: ${error}`);
    // Recursively process subdirectories
    const entries = await fs.readdir(entityDir, { withFileTypes: true });
        const subDir = path.join(entityDir, entry.name);
        // Load subdirectory config and merge with parent config
        let subEntityConfig = { ...entityConfig };
        const subDirConfig = await loadEntityConfig(subDir);
        if (subDirConfig) {
          // Check if this is a new entity type (has different entity name)
          if (subDirConfig.entity && subDirConfig.entity !== entityConfig.entity) {
            // This is a different entity type, skip it (will be processed separately)
          // Merge defaults: parent defaults + subdirectory overrides
          subEntityConfig = {
            ...entityConfig,
            ...subDirConfig,
              ...entityConfig.defaults,
              ...(subDirConfig.defaults || {})
        // Process subdirectory with merged config
        const subResult = await this.checkEntityDirectory(
          subDir,
          subEntityConfig,
        result.new += subResult.new;
        result.modified += subResult.modified;
        result.deleted += subResult.deleted;
        result.unchanged += subResult.unchanged;
