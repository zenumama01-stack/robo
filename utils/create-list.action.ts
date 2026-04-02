import { MJListEntity, MJListDetailEntity, MJEntityEntity } from "@memberjunction/core-entities";
 * Action to create a new list and optionally add initial records.
 *   ActionName: 'Create List',
 *     { Name: 'Name', Value: 'My Priority Contacts' },
 *     { Name: 'Description', Value: 'High priority contacts for Q4' },
 *     { Name: 'EntityName', Value: 'Contacts' },
 *     { Name: 'CategoryID', Value: 'cat-123' },
 *     { Name: 'AddRecordIDs', Value: ['contact1', 'contact2'] }
@RegisterClass(BaseAction, "Create List")
export class CreateListAction extends BaseAction {
      const name = this.getStringParam(params, 'Name');
      const description = this.getStringParam(params, 'Description');
      const entityName = this.getStringParam(params, 'EntityName');
      const entityId = this.getStringParam(params, 'EntityID');
      const categoryId = this.getStringParam(params, 'CategoryID');
      const addRecordIds = this.getArrayParam(params, 'AddRecordIDs');
          Message: 'Name is required'
      if (!entityName && !entityId) {
          Message: 'Either EntityName or EntityID is required'
      // Resolve entity ID if name provided
      let resolvedEntityId = entityId;
      if (entityName && !entityId) {
        const entityResult = await rv.RunView<MJEntityEntity>({
          ExtraFilter: `Name = '${entityName}'`,
        if (!entityResult.Success || !entityResult.Results || entityResult.Results.length === 0) {
            Message: `Entity '${entityName}' not found`
        resolvedEntityId = entityResult.Results[0].ID;
      // Create the list
      const list = await md.GetEntityObject<MJListEntity>('MJ: Lists', params.ContextUser);
      list.Name = name;
      list.Description = description || '';
      list.EntityID = resolvedEntityId!;
      list.UserID = params.ContextUser.ID;
        list.CategoryID = categoryId;
      const saveResult = await list.Save();
          ResultCode: 'SAVE_FAILED',
          Message: 'Failed to create list'
      // Add initial records if provided
      let recordsAdded = 0;
      if (addRecordIds && addRecordIds.length > 0) {
        for (const recordId of addRecordIds) {
            listDetail.ListID = list.ID;
            listDetail.Sequence = recordsAdded;
            listDetail.Status = 'Active';
            const detailSaveResult = await listDetail.Save();
            if (detailSaveResult) {
              recordsAdded++;
      this.addOutputParam(params, 'ListID', list.ID);
      this.addOutputParam(params, 'ListName', list.Name);
      this.addOutputParam(params, 'RecordsAdded', recordsAdded);
        ListID: list.ID,
        ListName: list.Name,
        RecordsAdded: recordsAdded
        Message: `List '${name}' created successfully${recordsAdded > 0 ? ` with ${recordsAdded} initial record(s)` : ''}`
        Message: `Failed to create list: ${errorMessage}`
