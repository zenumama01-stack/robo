import { BaseEntity, RunView, UserInfo } from '@memberjunction/core';
import { RelatedEntityConfig, EntityConfig } from '../config';
 * Handles loading and processing of related entities for records
export class RelatedEntityHandler {
   * Load related entities for a record
  async loadRelatedEntities(
    parentRecord: BaseEntity,
    relationConfig: RelatedEntityConfig,
    parentEntityConfig: EntityConfig,
    existingRelatedEntities: RecordData[],
    processRecordData: (
      isNewRecord?: boolean,
      currentDepth?: number,
      ancestryPath?: Set<string>,
    ) => Promise<RecordData>,
  ): Promise<RecordData[]> {
      const parentPrimaryKey = this.getRecordPrimaryKey(parentRecord);
      if (!parentPrimaryKey) {
        this.logWarning('Unable to determine primary key for parent record', verbose);
      const relatedRecords = await this.queryRelatedEntities(
        parentPrimaryKey, 
      if (!relatedRecords) {
      const relatedEntityConfig = this.createRelatedEntityConfig(relationConfig, parentEntityConfig);
      return await this.processRelatedRecords(
        relatedRecords,
        relatedEntityConfig,
        existingRelatedEntities,
        processRecordData,
      this.logError(`Error loading related entities for ${relationConfig.entity}`, error, verbose);
   * Queries the database for related entities
  private async queryRelatedEntities(
    parentPrimaryKey: string,
  ): Promise<BaseEntity[] | null> {
    const filter = this.buildRelatedEntityFilter(parentPrimaryKey, relationConfig);
      console.log(`Loading related entities: ${relationConfig.entity} with filter: ${filter}`);
      EntityName: relationConfig.entity,
      this.logWarning(`Failed to load related entities ${relationConfig.entity}: ${result.ErrorMessage}`, verbose);
      console.log(`Found ${result.Results.length} related records for ${relationConfig.entity}`);
   * Builds the filter for querying related entities
  private buildRelatedEntityFilter(parentPrimaryKey: string, relationConfig: RelatedEntityConfig): string {
    let filter = `${relationConfig.foreignKey} = '${parentPrimaryKey}'`;
    if (relationConfig.filter) {
      filter += ` AND (${relationConfig.filter})`;
   * Creates entity config for related entity processing
  private createRelatedEntityConfig(
    parentEntityConfig: EntityConfig
  ): EntityConfig {
      entity: relationConfig.entity,
      pull: {
        excludeFields: relationConfig.excludeFields || [],
        lookupFields: relationConfig.lookupFields || {},
        externalizeFields: relationConfig.externalizeFields || [],
        relatedEntities: relationConfig.relatedEntities || {},
        ignoreVirtualFields: parentEntityConfig.pull?.ignoreVirtualFields || false,
        ignoreNullFields: parentEntityConfig.pull?.ignoreNullFields || false
   * Processes all related records (both existing and new)
  private async processRelatedRecords(
    dbRecords: BaseEntity[],
    relatedEntityConfig: EntityConfig,
    processRecordData: Function,
    const dbRecordMap = this.buildDatabaseRecordMap(dbRecords);
    const relatedRecords: RecordData[] = [];
    // Process existing related entities first (preserving order)
    await this.processExistingRelatedEntities(
      dbRecordMap,
      processedIds,
    // Process new related entities (append to end)
    await this.processNewRelatedEntities(
      dbRecords,
    return relatedRecords;
   * Builds a map of database records by primary key for efficient lookup
  private buildDatabaseRecordMap(dbRecords: BaseEntity[]): Map<string, BaseEntity> {
    const dbRecordMap = new Map<string, BaseEntity>();
    for (const relatedRecord of dbRecords) {
      const relatedPrimaryKey = this.getRecordPrimaryKey(relatedRecord);
      if (relatedPrimaryKey) {
        dbRecordMap.set(relatedPrimaryKey, relatedRecord);
    return dbRecordMap;
   * Processes existing related entities
  private async processExistingRelatedEntities(
    dbRecordMap: Map<string, BaseEntity>,
    relatedRecords: RecordData[],
    processedIds: Set<string>,
    for (const existingRelatedEntity of existingRelatedEntities) {
      const existingPrimaryKey = existingRelatedEntity.primaryKey?.ID;
      if (!existingPrimaryKey) {
        this.logWarning('Existing related entity missing primary key, skipping', verbose);
      const dbRecord = dbRecordMap.get(existingPrimaryKey);
      if (!dbRecord) {
          console.log(`Related entity ${existingPrimaryKey} no longer exists in database, removing from results`);
        continue; // Skip deleted records
      const recordData = await this.processExistingRelatedEntity(
        dbRecord,
        existingPrimaryKey,
        existingRelatedEntity,
        relatedRecords.push(recordData);
        processedIds.add(existingPrimaryKey);
   * Processes a single existing related entity
  private async processExistingRelatedEntity(
    dbRecord: BaseEntity,
    existingPrimaryKey: string,
    existingRelatedEntity: RecordData,
  ): Promise<RecordData | null> {
    const relatedRecordPrimaryKey = this.buildPrimaryKeyForRecord(
      relationConfig.entity
    const fieldOverrides = this.createFieldOverrides(dbRecord, relationConfig);
    if (!fieldOverrides) {
    return await processRecordData(
      relatedRecordPrimaryKey,
      '', // targetDir not needed for related entities
      false, // isNewRecord = false for existing records
      existingRelatedEntity, // Pass existing data for change detection
      currentDepth + 1,
      fieldOverrides // Pass the field override for @parent:ID
   * Processes new related entities
  private async processNewRelatedEntities(
      if (!relatedPrimaryKey || processedIds.has(relatedPrimaryKey)) {
        continue; // Skip already processed records
      const recordData = await this.processNewRelatedEntity(
        relatedRecord,
        relatedPrimaryKey,
        processedIds.add(relatedPrimaryKey);
   * Processes a single new related entity
  private async processNewRelatedEntity(
    relatedRecord: BaseEntity,
    relatedPrimaryKey: string,
    const fieldOverrides = this.createFieldOverrides(relatedRecord, relationConfig);
      true, // isNewRecord = true for new records
      undefined, // No existing data for new records
   * Creates field overrides for @parent:ID replacement
  private createFieldOverrides(record: BaseEntity, relationConfig: RelatedEntityConfig): Record<string, any> | null {
      const data = record.GetAll();
      if (data[relationConfig.foreignKey] !== undefined) {
        return { [relationConfig.foreignKey]: '@parent:ID' };
      if ((record as any)[relationConfig.foreignKey] !== undefined) {
   * Builds primary key for a record
  private buildPrimaryKeyForRecord(
    const relatedRecordPrimaryKey: Record<string, any> = {};
    const entityInfo = this.syncEngine.getEntityInfo(entityName);
    for (const pk of entityInfo?.PrimaryKeys || []) {
      if (pk.Name === 'ID') {
        relatedRecordPrimaryKey[pk.Name] = primaryKeyValue;
        // For compound keys, get the value from the related record
        relatedRecordPrimaryKey[pk.Name] = this.getFieldValue(record, pk.Name);
    return relatedRecordPrimaryKey;
   * Get the primary key value from a record
  private getRecordPrimaryKey(record: BaseEntity): string | null {
    if (!record) return null;
    // Try to get ID directly
    if ((record as any).ID) return (record as any).ID;
    // Try to get from GetAll() method if it's an entity object
      if (data.ID) return data.ID;
    // Try common variations
    if ((record as any).id) return (record as any).id;
    if ((record as any).Id) return (record as any).Id;
   * Get a field value from a record, handling both entity objects and plain objects
  private getFieldValue(record: BaseEntity, fieldName: string): any {
    // Try to get field directly using bracket notation with type assertion
    if ((record as any)[fieldName] !== undefined) return (record as any)[fieldName];
      if (data[fieldName] !== undefined) return data[fieldName];
   * Log warning message if verbose mode is enabled
  private logWarning(message: string, verbose?: boolean): void {
      console.warn(message);
   * Log error message if verbose mode is enabled
  private logError(message: string, error: any, verbose?: boolean): void {
      console.error(`${message}: ${error}`);
