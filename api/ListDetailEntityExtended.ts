import { BaseEntity, BaseEntityResult, CompositeKey, EntityInfo, Metadata } from "@memberjunction/core";
import { MJListDetailEntity } from "../generated/entity_subclasses";
@RegisterClass(BaseEntity, 'MJ: List Details')
export class ListDetailEntityExtended extends MJListDetailEntity  {
    private _recordCompositeKey: CompositeKey | null = null;
    private _sourceEntityInfo: EntityInfo | null = null;
     * Sets the RecordID from a source entity record.
     * For single PK entities, stores just the raw PK value.
     * For composite PK entities, stores the concatenated key format (Field1|Value1||Field2|Value2).
     * @param entityInfo The EntityInfo for the source entity
     * @param record The source record (can be a BaseEntity or plain object with PK values)
    public SetRecordIDFromEntity(entityInfo: EntityInfo, record: BaseEntity | Record<string, unknown>): void {
        this._sourceEntityInfo = entityInfo;
            // Single PK: store just the raw value
            const value = record instanceof BaseEntity ? record.Get(pkField) : record[pkField];
            this.RecordID = String(value);
            // Composite PK: store concatenated format
            compositeKey.LoadFromEntityInfoAndRecord(entityInfo, record);
            this.RecordID = compositeKey.ToConcatenatedString();
        // Clear cached composite key since RecordID changed
        this._recordCompositeKey = null;
     * Gets a CompositeKey from the stored RecordID.
     * Lazily builds and caches the CompositeKey.
     * @param entityInfo Optional EntityInfo - required if not previously set via SetRecordIDFromEntity
     * @returns CompositeKey representing the stored RecordID
    public GetCompositeKey(entityInfo?: EntityInfo): CompositeKey {
        if (this._recordCompositeKey) {
            return this._recordCompositeKey;
        const effectiveEntityInfo = entityInfo || this._sourceEntityInfo;
        if (!effectiveEntityInfo) {
            // Try to get entity info from the List's EntityID
            const list = md.Entities.find(e => e.Name === 'MJ: Lists');
                throw new Error('Cannot determine entity info. Provide entityInfo parameter or call SetRecordIDFromEntity first.');
        this._recordCompositeKey = new CompositeKey();
        if (effectiveEntityInfo && effectiveEntityInfo.PrimaryKeys.length === 1) {
            // Single PK: RecordID is just the raw value
            const pkField = effectiveEntityInfo.PrimaryKeys[0].Name;
            this._recordCompositeKey.KeyValuePairs = [{ FieldName: pkField, Value: this.RecordID }];
            // Composite PK or unknown: try to parse as concatenated string
            this._recordCompositeKey.LoadFromConcatenatedString(this.RecordID);
            // If parsing failed (no delimiters found), treat as single value
            if (this._recordCompositeKey.KeyValuePairs.length === 0) {
                const pkField = effectiveEntityInfo?.PrimaryKeys[0]?.Name || 'ID';
     * Extracts the raw primary key value(s) from the RecordID.
     * For single PK, returns the raw value directly.
     * For composite PK, returns an object with field names and values.
     * @returns The raw PK value (string) for single PK, or Record<string, unknown> for composite
    public GetRawPrimaryKeyValue(entityInfo: EntityInfo): string | Record<string, unknown> {
            // Single PK: RecordID is the raw value
            return this.RecordID;
            // Composite PK: parse and return as object
            const compositeKey = this.GetCompositeKey(entityInfo);
            for (const kvp of compositeKey.KeyValuePairs) {
                result[kvp.FieldName] = kvp.Value;
     * Static utility to build the appropriate RecordID value from a source record.
     * Use this when you need to build a RecordID without creating a MJListDetailEntity.
     * @param record The source record
     * @returns The properly formatted RecordID string
    public static BuildRecordID(entityInfo: EntityInfo, record: BaseEntity | Record<string, unknown>): string {
            // Single PK: return just the raw value
            // Composite PK: return concatenated format
            return compositeKey.ToConcatenatedString();
     * Static utility to extract raw PK value from a RecordID string.
     * Handles both single PK (raw value) and composite PK (concatenated) formats.
     * @param recordId The RecordID value from a ListDetail record
     * @returns For single PK: the raw value. For composite PK: the full concatenated string.
    public static ExtractPrimaryKeyValue(recordId: string, entityInfo: EntityInfo): string {
        // For single PK entities, RecordID is stored as raw value, so return as-is
        // For composite PK entities, RecordID is stored as concatenated string, return as-is
        // The caller should use this value appropriately based on their context
            if(!this.ListID){
                throw new Error('ListID cannot be null');
            if(!this.RecordID){
                throw new Error('RecordID cannot be null');
            if(!this.ContextCurrentUser){
                throw new Error('ContextCurrentUser cannot be null');
            const rvResult = await rv.RunView({
                ExtraFilter: `ListID = '${this.ListID}' AND RecordID = '${this.RecordID}'`
            if(rvResult.Results.length > 0){
                throw new Error(`Record ${this.RecordID} already exists in List ${this.ListID}`);
            const saveResult = await super.Save();
            if (currentResultCount === this.ResultHistory.length) {0
                newResult.Message = e.message;
