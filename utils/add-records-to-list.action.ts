import { MJListEntity, MJListDetailEntity } from "@memberjunction/core-entities";
 * Action to add one or more records to a list.
 *   ActionName: 'Add Records to List',
 *     { Name: 'ListID', Value: 'abc-123-def' },
 *     { Name: 'RecordIDs', Value: ['rec1', 'rec2', 'rec3'] },
 *     { Name: 'SkipDuplicates', Value: true },
 *     { Name: 'DefaultStatus', Value: 'Active' }
@RegisterClass(BaseAction, "Add Records to List")
export class AddRecordsToListAction extends BaseAction {
      const listId = this.getStringParam(params, 'ListID');
      const recordIds = this.getArrayParam(params, 'RecordIDs');
      const skipDuplicates = this.getBooleanParam(params, 'SkipDuplicates', true);
      const defaultStatus = this.getStringParam(params, 'DefaultStatus') || 'Active';
      const additionalData = this.getStringParam(params, 'AdditionalData');
      if (!listId) {
          ResultCode: 'MISSING_PARAMETER',
          Message: 'ListID is required'
      if (!recordIds || recordIds.length === 0) {
          Message: 'RecordIDs is required and must contain at least one record ID'
      // Validate list exists
      const listResult = await rv.RunView<MJListEntity>({
        ExtraFilter: `ID = '${listId}'`,
      if (!listResult.Success || !listResult.Results || listResult.Results.length === 0) {
          ResultCode: 'LIST_NOT_FOUND',
          Message: `List with ID '${listId}' not found`
      // Get existing memberships if skipping duplicates
      let existingRecordIds = new Set<string>();
      if (skipDuplicates) {
        const recordIdFilter = recordIds.map((id: string) => `'${id}'`).join(',');
        const existingResult = await rv.RunView<MJListDetailEntity>({
          ExtraFilter: `ListID = '${listId}' AND RecordID IN (${recordIdFilter})`,
        if (existingResult.Success && existingResult.Results) {
          for (const detail of existingResult.Results) {
            existingRecordIds.add(detail.RecordID);
      // Add records to list
      let addedCount = 0;
      let skippedCount = 0;
      let failedCount = 0;
      for (const recordId of recordIds) {
        // Skip if already exists
        if (skipDuplicates && existingRecordIds.has(recordId)) {
          skippedCount++;
          const listDetail = await md.GetEntityObject<MJListDetailEntity>('MJ: List Details', params.ContextUser);
          listDetail.ListID = listId;
          listDetail.RecordID = recordId;
          listDetail.Sequence = 0;
          listDetail.Status = defaultStatus as 'Active' | 'Complete' | 'Disabled' | 'Error' | 'Other' | 'Pending' | 'Rejected';
          if (additionalData) {
            listDetail.AdditionalData = additionalData;
          const saveResult = await listDetail.Save();
            addedCount++;
            failedCount++;
            errors.push(`Failed to add record '${recordId}'`);
          errors.push(`Error adding record '${recordId}': ${errorMessage}`);
        TotalRecords: recordIds.length,
        Added: addedCount,
        Skipped: skippedCount,
        Failed: failedCount,
        Errors: errors
      this.addOutputParam(params, 'TotalRecords', result.TotalRecords);
      this.addOutputParam(params, 'Added', result.Added);
      this.addOutputParam(params, 'Skipped', result.Skipped);
      this.addOutputParam(params, 'Failed', result.Failed);
      this.addOutputParam(params, 'Errors', result.Errors);
      const success = failedCount === 0;
        Success: success,
        ResultCode: success ? 'SUCCESS' : 'PARTIAL_SUCCESS',
        Message: success
          ? `Successfully added ${addedCount} record(s) to list${skippedCount > 0 ? `, skipped ${skippedCount} duplicate(s)` : ''}`
          : `Added ${addedCount}, failed ${failedCount}. Errors: ${errors.join('; ')}`
        ResultCode: 'UNEXPECTED_ERROR',
        Message: `Failed to add records to list: ${errorMessage}`
  private getStringParam(params: RunActionParams, name: string): string | undefined {
      p.Name.toLowerCase() === name.toLowerCase() && p.Type === 'Input'
    return param?.Value != null ? String(param.Value) : undefined;
  private getArrayParam(params: RunActionParams, name: string): string[] {
    if (!param?.Value) return [];
    if (Array.isArray(param.Value)) {
      return param.Value.map(v => String(v));
        const parsed = JSON.parse(param.Value);
        return Array.isArray(parsed) ? parsed.map(v => String(v)) : [param.Value];
        return [param.Value];
    return [String(param.Value)];
