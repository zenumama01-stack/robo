import { Metadata, RunView, CompositeKey, EntityInfo } from "@memberjunction/core";
 * Action to retrieve records from a list with optional filtering and pagination.
 * // Get all records from a list
 *   ActionName: 'Get List Records',
 *     { Name: 'ListID', Value: 'abc-123-def' }
 * // Get filtered records with pagination
 *     { Name: 'FilterByStatus', Value: 'Active' },
 *     { Name: 'MaxRecords', Value: 100 },
 *     { Name: 'IncludeRecordDetails', Value: true }
@RegisterClass(BaseAction, "Get List Records")
export class GetListRecordsAction extends BaseAction {
      const filterByStatus = this.getStringParam(params, 'FilterByStatus');
      const maxRecords = this.getNumericParam(params, 'MaxRecords', 1000);
      const includeRecordDetails = this.getBooleanParam(params, 'IncludeRecordDetails', false);
      const orderBy = this.getStringParam(params, 'OrderBy') || 'Sequence ASC';
      // Verify list exists and get entity info
      const list = listResult.Results[0];
      // Build filter
      let filter = `ListID = '${listId}'`;
      if (filterByStatus) {
        filter += ` AND Status = '${filterByStatus}'`;
      // Get list details
      const detailsResult = await rv.RunView<MJListDetailEntity>({
        MaxRows: maxRecords,
      if (!detailsResult.Success) {
          ResultCode: 'QUERY_FAILED',
          Message: `Failed to query list records: ${detailsResult.ErrorMessage}`
      const details = detailsResult.Results || [];
      // Optionally load full record details
      interface RecordWithDetails {
        ListDetailID: string;
        RecordID: string;
        Sequence: number;
        AdditionalData: string | null;
        RecordDetails?: Record<string, unknown>;
      let records: RecordWithDetails[] = details.map(d => ({
        ListDetailID: d.ID,
        RecordID: d.RecordID,
        Sequence: d.Sequence,
        Status: d.Status,
        AdditionalData: d.AdditionalData
      if (includeRecordDetails && details.length > 0) {
        // Get the entity name from the list's entity
        const entityName = list.Entity; // This is the denormalized entity name
        if (entityName) {
          // Get the entity's primary key field(s)
          const entityInfo = md.Entities.find(e => e.Name === entityName);
          if (entityInfo && entityInfo.PrimaryKeys.length > 0) {
            // Build appropriate filter for single vs composite keys
            const extraFilter = this.buildRecordFilter(entityInfo, details);
            const recordsResult = await rv.RunView({
            if (recordsResult.Success && recordsResult.Results) {
              // Create a map for quick lookup
              // For single PK, RecordID is just the raw value
              // For composite PK, RecordID is the concatenated string
              const recordMap = new Map<string, Record<string, unknown>>();
              const isSinglePK = entityInfo.PrimaryKeys.length === 1;
              for (const rec of recordsResult.Results) {
                let keyString: string;
                if (isSinglePK) {
                  // Single PK: use raw value
                  const pkField = entityInfo.PrimaryKeys[0].Name;
                  keyString = String(rec.Get ? rec.Get(pkField) : rec[pkField]);
                  // Composite PK: use concatenated format
                  const compositeKey = new CompositeKey();
                  compositeKey.LoadFromEntityInfoAndRecord(entityInfo, rec);
                  keyString = compositeKey.ToConcatenatedString();
                recordMap.set(keyString, rec.GetAll ? rec.GetAll() : rec);
              // Attach record details
              records = records.map(r => ({
                RecordDetails: recordMap.get(r.RecordID) || undefined
      this.addOutputParam(params, 'Records', records);
      this.addOutputParam(params, 'TotalCount', details.length);
      this.addOutputParam(params, 'EntityName', list.Entity);
        Message: `Retrieved ${details.length} record(s) from list '${list.Name}'`
        Message: `Failed to get list records: ${errorMessage}`
   * Build the SQL filter to select records that match the given list details.
   * For single PK entities, uses a simple IN clause with the raw RecordID values.
   * For composite PK entities, uses an OR clause with concatenated key matching.
  private buildRecordFilter(entityInfo: EntityInfo, details: MJListDetailEntity[]): string {
    const primaryKeys = entityInfo.PrimaryKeys;
    const recordIds = details.map(d => d.RecordID);
      // For single PK entities, RecordID stores just the raw value (not concatenated format)
      const escapedValues = recordIds.map(rid => `'${rid.replace(/'/g, "''")}'`);
      if (escapedValues.length === 0) {
        return '1=0'; // No valid records
      return `${pkField} IN (${escapedValues.join(',')})`;
      // Composite key case: build concatenation expression and match against RecordID values
      // Build SQL expression that concatenates the PK fields in the same format as RecordID
      // Build IN clause with the RecordID values
      const escapedRecordIds = recordIds.map(id => `'${id.replace(/'/g, "''")}'`).join(',');
      return `(${compositeKeyExpr}) IN (${escapedRecordIds})`;
