import { CompositeKey, EntityDeleteOptions, EntityPermissionType, EntitySaveOptions, Metadata, RunView } from '@memberjunction/core';
import { MJFileCategoryEntity, MJFileEntity } from '@memberjunction/core-entities';
import { AppContext, Arg, Ctx, DeleteOptionsInput, Int, Mutation } from '@memberjunction/server';
import { MJFileCategoryResolver as FileCategoryResolverBase, MJFileCategory_ } from '../generated/generated.js';
import { GetReadWriteProvider } from '../util.js';
export class FileCategoryResolver extends FileCategoryResolverBase {
  async DeleteFileCategory(
    @Arg('options___', () => DeleteOptionsInput) options: DeleteOptionsInput,
    key.LoadFromSingleKeyValuePair('ID', ID);
    const p = GetReadWriteProvider(providers);    
    if (!(await this.BeforeDelete(p, key))) {
    const user = this.GetUserFromPayload(userPayload);
    const fileEntity = await p.GetEntityObject<MJFileEntity>('MJ: Files', user);
    const fileCategoryEntity = await p.GetEntityObject<MJFileCategoryEntity>('MJ: File Categories', user);
    fileEntity.CheckPermissions(EntityPermissionType.Update, true);
    fileCategoryEntity.CheckPermissions(EntityPermissionType.Delete, true);
    await fileCategoryEntity.Load(ID);
    const returnValue = fileCategoryEntity.GetAll();
    // Any files using the deleted category fall back to its parent
    await p.BeginTransaction();
      // SHOULD USE BaseEntity for each of these records to ensure object model
      // is used everywhere - new code below. The below is SLOWER than a single
      // Update statement, but it ensures that the object model is used everywhere
      // in case there are sub-classes and business logic/etc for the updates
      // the direct SQL would bypass that logic.
      // const sSQL = `UPDATE [${mj_core_schema}].[File]
      //                 SET [CategoryID]=${fileCategoryEntity.ParentID}
      //                 WHERE [CategoryID]=${fileCategoryEntity.ID}`;
      // await dataSource.query(sSQL);
      const filesResult = await rv.RunView(
          ExtraFilter: `CategoryID='${fileCategoryEntity.ID}'`,
      if (filesResult) {
        // iterate through each of the files in filesResult.Results
        // and update the CategoryID to fileCategoryEntity.ParentID
        for (const file of filesResult.Results) {
          await fileEntity.Load(file.ID);
          fileEntity.CategoryID = fileCategoryEntity.ParentID;
      await fileCategoryEntity.Delete(options);
      await p.CommitTransaction();
      await p.RollbackTransaction();
    await this.AfterDelete(p, key); // fire event
