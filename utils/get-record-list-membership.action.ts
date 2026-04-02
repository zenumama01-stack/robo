 * Action to find which lists a specific record belongs to.
 *   ActionName: 'Get Record List Membership',
 *     { Name: 'RecordID', Value: 'contact-123' },
 *     { Name: 'UserID', Value: 'user-456' }  // Optional: filter by list owner
@RegisterClass(BaseAction, "Get Record List Membership")
export class GetRecordListMembershipAction extends BaseAction {
      const recordId = this.getStringParam(params, 'RecordID');
      const userId = this.getStringParam(params, 'UserID');
      if (!recordId) {
          Message: 'RecordID is required'
        const entityResult = await rv.RunView({
      // Get list details for this record
        ExtraFilter: `RecordID = '${recordId}'`,
          Message: `Failed to query list membership: ${detailsResult.ErrorMessage}`
      if (details.length === 0) {
        this.addOutputParam(params, 'Lists', []);
        this.addOutputParam(params, 'MembershipDetails', []);
        this.addOutputParam(params, 'Count', 0);
          Message: 'Record is not a member of any lists'
      // Get the lists
      const listIds = [...new Set(details.map(d => d.ListID))];
      const listIdFilter = listIds.map(id => `'${id}'`).join(',');
      let listFilter = `ID IN (${listIdFilter}) AND EntityID = '${resolvedEntityId}'`;
        listFilter += ` AND UserID = '${userId}'`;
      const listsResult = await rv.RunView<MJListEntity>({
        ExtraFilter: listFilter,
      if (!listsResult.Success) {
          Message: `Failed to query lists: ${listsResult.ErrorMessage}`
      const lists = listsResult.Results || [];
      // Build membership details
      interface MembershipDetail {
        listId: string;
        listName: string;
        addedAt: Date;
        sequence: number;
      const membershipDetails: MembershipDetail[] = [];
      const listMap = new Map<string, MJListEntity>();
      for (const list of lists) {
        listMap.set(list.ID, list);
      for (const detail of details) {
        const list = listMap.get(detail.ListID);
        if (list) {
          membershipDetails.push({
            listId: detail.ListID,
            listName: list.Name,
            addedAt: detail.Get('__mj_CreatedAt') as Date,
            status: detail.Status,
            sequence: detail.Sequence
      const listOutput = lists.map(l => ({
        ID: l.ID,
        Name: l.Name,
        Description: l.Description,
        UserID: l.UserID,
        CategoryID: l.CategoryID
      this.addOutputParam(params, 'Lists', listOutput);
      this.addOutputParam(params, 'MembershipDetails', membershipDetails);
      this.addOutputParam(params, 'Count', lists.length);
        Message: `Record is a member of ${lists.length} list(s)`
        Message: `Failed to get record list membership: ${errorMessage}`
