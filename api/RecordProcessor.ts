import { BaseEntity, RunView, UserInfo, EntityInfo } from '@memberjunction/core';
import { SyncEngine, RecordData } from '../lib/sync-engine';
import { EntityConfig } from '../config';
import { JsonWriteHelper } from './json-write-helper';
import { EntityPropertyExtractor } from './EntityPropertyExtractor';
import { FieldExternalizer } from './FieldExternalizer';
import { RelatedEntityHandler } from './RelatedEntityHandler';
import { METADATA_KEYWORDS, createKeywordReference } from '../constants/metadata-keywords';
 * Handles the core processing of individual record data into the sync format
export class RecordProcessor {
  private propertyExtractor: EntityPropertyExtractor;
  private fieldExternalizer: FieldExternalizer;
  private relatedEntityHandler: RelatedEntityHandler;
    private syncEngine: SyncEngine,
    private contextUser: UserInfo
    this.propertyExtractor = new EntityPropertyExtractor();
    this.fieldExternalizer = new FieldExternalizer();
    this.relatedEntityHandler = new RelatedEntityHandler(syncEngine, contextUser);
   * Processes a record into the standardized RecordData format
  async processRecord(
    entityConfig: EntityConfig,
    verbose?: boolean,
    isNewRecord: boolean = true,
    existingRecordData?: RecordData,
    currentDepth: number = 0,
    ancestryPath: Set<string> = new Set(),
  ): Promise<RecordData> {
    // Extract all properties from the entity
    const allProperties = this.propertyExtractor.extractAllProperties(record, fieldOverrides);
    // Process fields and related entities
    const { fields, relatedEntities } = await this.processEntityData(
      allProperties,
      primaryKey,
      entityConfig,
      existingRecordData,
      currentDepth,
      ancestryPath,
    // Calculate checksum and sync metadata
    const syncData = await this.calculateSyncMetadata(
    // Build the final record data with proper ordering
    return JsonWriteHelper.createOrderedRecordData(
      relatedEntities,
      syncData
   * Processes entity data into fields and related entities
  private async processEntityData(
    existingRecordData: RecordData | undefined,
    currentDepth: number,
    ancestryPath: Set<string>,
  ): Promise<{ fields: Record<string, any>; relatedEntities: Record<string, RecordData[]> }> {
    const fields: Record<string, any> = {};
    const relatedEntities: Record<string, RecordData[]> = {};
    // Process individual fields
    await this.processFields(
    // Process related entities if configured
    await this.processRelatedEntities(
    return { fields, relatedEntities };
   * Processes individual fields from the entity
  private async processFields(
    fields: Record<string, any>,
    const entityInfo = this.syncEngine.getEntityInfo(entityConfig.entity);
    for (const [fieldName, fieldValue] of Object.entries(allProperties)) {
      if (this.shouldSkipField(fieldName, fieldValue, primaryKey, entityConfig, entityInfo)) {
      let processedValue = await this.processFieldValue(
      fields[fieldName] = processedValue;
   * Determines if a field should be skipped during processing
  private shouldSkipField(
    entityInfo: EntityInfo | null
    // Skip primary key fields
    if (primaryKey[fieldName] !== undefined) {
    // Skip internal fields
    if (fieldName.startsWith('__mj_')) {
    // Skip excluded fields
    if (entityConfig.pull?.excludeFields?.includes(fieldName)) {
    // Skip virtual fields if configured
    if (this.shouldSkipVirtualField(fieldName, entityConfig, entityInfo)) {
    // Skip null fields if configured
    if (entityConfig.pull?.ignoreNullFields && fieldValue === null) {
   * Checks if a virtual field should be skipped
  private shouldSkipVirtualField(
    if (!entityConfig.pull?.ignoreVirtualFields || !entityInfo) {
    const fieldInfo = entityInfo.Fields.find(f => f.Name === fieldName);
    return fieldInfo?.IsVirtual === true;
   * Processes a single field value through various transformations
  private async processFieldValue(
    let processedValue = fieldValue;
    // Convert Date objects to ISO strings
    processedValue = this.serializeDateValue(processedValue);
    // Apply lookup field conversion if configured
    processedValue = await this.applyLookupFieldConversion(
      processedValue,
    // Trim string values
    processedValue = this.trimStringValue(processedValue);
    // Apply field externalization if configured
    processedValue = await this.applyFieldExternalization(
    return processedValue;
   * Serializes Date objects to ISO strings for JSON storage
  private serializeDateValue(value: any): any {
      // Check if the date is valid
      if (isNaN(value.getTime())) {
        return null; // Invalid dates become null
   * Applies lookup field conversion if configured
  private async applyLookupFieldConversion(
    const lookupConfig = entityConfig.pull?.lookupFields?.[fieldName];
    if (!lookupConfig || fieldValue == null) {
      return await this.convertGuidToLookup(String(fieldValue), lookupConfig, verbose);
        console.warn(`Failed to convert ${fieldName} to lookup: ${error}`);
      return fieldValue; // Keep original value if lookup fails
   * Trims string values to remove whitespace
  private trimStringValue(value: any): any {
    return typeof value === 'string' ? value.trim() : value;
   * Applies field externalization if configured
  private async applyFieldExternalization(
    if (!entityConfig.pull?.externalizeFields || fieldValue == null) {
    const externalizePattern = this.getExternalizationPattern(fieldName, entityConfig);
    if (!externalizePattern) {
      const existingFileReference = existingRecordData?.fields?.[fieldName];
      const recordData = this.createRecordDataForExternalization(allProperties);
      return await this.fieldExternalizer.externalizeField(
        externalizePattern,
        entityConfig.pull?.mergeStrategy || 'merge',
        console.warn(`Failed to externalize field ${fieldName}: ${error}`);
      return fieldValue; // Keep original value if externalization fails
   * Gets the externalization pattern for a field
  private getExternalizationPattern(fieldName: string, entityConfig: EntityConfig): string | null {
    const externalizeConfig = entityConfig.pull?.externalizeFields;
    if (!externalizeConfig) return null;
    if (Array.isArray(externalizeConfig)) {
      return this.getArrayExternalizationPattern(fieldName, externalizeConfig);
      return this.getObjectExternalizationPattern(fieldName, externalizeConfig);
   * Gets externalization pattern from array configuration
  private getArrayExternalizationPattern(
    externalizeConfig: any[]
    if (externalizeConfig.length > 0 && typeof externalizeConfig[0] === 'string') {
      // Simple string array format
      if ((externalizeConfig as string[]).includes(fieldName)) {
        return createKeywordReference('file', `{Name}.${fieldName.toLowerCase()}.md`);
      // Array of objects format
      const fieldConfig = (externalizeConfig as Array<{field: string; pattern: string}>)
        .find(config => config.field === fieldName);
      if (fieldConfig) {
        return fieldConfig.pattern;
   * Gets externalization pattern from object configuration
  private getObjectExternalizationPattern(
    externalizeConfig: Record<string, any>
    const fieldConfig = externalizeConfig[fieldName];
      const extension = fieldConfig.extension || '.md';
      return createKeywordReference('file', `{Name}.${fieldName.toLowerCase()}${extension}`);
   * Creates a BaseEntity-like object for externalization processing
  private createRecordDataForExternalization(allProperties: Record<string, any>): BaseEntity {
    return allProperties as any as BaseEntity;
   * Processes related entities for the record
  private async processRelatedEntities(
    relatedEntities: Record<string, RecordData[]>,
    if (!entityConfig.pull?.relatedEntities) {
    for (const [relationKey, relationConfig] of Object.entries(entityConfig.pull.relatedEntities)) {
        const existingRelated = existingRecordData?.relatedEntities?.[relationKey] || [];
        const relatedRecords = await this.relatedEntityHandler.loadRelatedEntities(
          relationConfig,
          existingRelated,
          this.processRecord.bind(this), // Pass bound method reference
        if (relatedRecords.length > 0) {
          relatedEntities[relationKey] = relatedRecords;
          console.warn(`Failed to load related entities for ${relationKey}: ${error}`);
   * Calculates sync metadata including checksum and last modified timestamp
  private async calculateSyncMetadata(
  ): Promise<{ lastModified: string; checksum: string }> {
    // Determine if we should include external file content in checksum
    const hasExternalizedFields = this.hasExternalizedFields(fields, entityConfig);
    const checksum = hasExternalizedFields
      ? await this.syncEngine.calculateChecksumWithFileContent(fields, targetDir)
      : this.syncEngine.calculateChecksum(fields);
    if (verbose && hasExternalizedFields) {
      console.log(`Calculated checksum including external file content for record`);
    // Compare with existing checksum to determine if data changed
    const existingChecksum = existingRecordData?.sync?.checksum;
    const existingTimestamp = existingRecordData?.sync?.lastModified;
    if (existingChecksum === checksum) {
      // No change detected - preserve existing sync metadata
        console.log(`No changes detected for record, preserving existing timestamp`);
        lastModified: existingTimestamp!,
        checksum: checksum
      // Change detected - update timestamp
      const newTimestamp = new Date().toISOString();
        if (existingChecksum) {
          console.log(`Changes detected for record, updating timestamp`);
          console.log(`New record, generating initial timestamp`);
        lastModified: newTimestamp,
   * Checks if the record has externalized fields
  private hasExternalizedFields(fields: Record<string, any>, entityConfig: EntityConfig): boolean {
    return !!entityConfig.pull?.externalizeFields &&
           Object.values(fields).some(value =>
             typeof value === 'string' && value.startsWith(METADATA_KEYWORDS.FILE)
   * Convert a GUID value to @lookup syntax by looking up the human-readable value
  private async convertGuidToLookup(
    guidValue: string,
    lookupConfig: { entity: string; field: string },
    if (!guidValue || typeof guidValue !== 'string') {
      return guidValue;
        EntityName: lookupConfig.entity,
        ExtraFilter: `ID = '${guidValue}'`,
        const targetRecord = result.Results[0];
        const lookupValue = targetRecord[lookupConfig.field];
        if (lookupValue != null) {
          return createKeywordReference('lookup', `${lookupConfig.entity}.${lookupConfig.field}=${lookupValue}`);
        console.warn(`Lookup failed for ${guidValue} in ${lookupConfig.entity}.${lookupConfig.field}`);
      return guidValue; // Return original GUID if lookup fails
        console.warn(`Error during lookup conversion: ${error}`);
