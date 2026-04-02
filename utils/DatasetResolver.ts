import { Arg, Ctx, Field, InputType, Int, ObjectType, Query, Resolver } from 'type-graphql';
import { ResolverBase } from '../generic/ResolverBase.js';
export class DatasetResultType {
  @Field(() => Date)
  Results: string;
export class DatasetItemFilterTypeGQL {
@Resolver(DatasetResultType)
export class DatasetResolverExtended extends ResolverBase {
  @Query(() => DatasetResultType)
  async GetDatasetByName(
    @Arg('DatasetName', () => String) DatasetName: string,
    @Arg('ItemFilters', () => [DatasetItemFilterTypeGQL], { nullable: 'itemsAndList' }) ItemFilters?: DatasetItemFilterTypeGQL[]
    // Check API key scope authorization for dataset read
    await this.CheckAPIKeyScopeAuthorization('dataset:read', DatasetName, userPayload);
      const md = GetReadOnlyProvider(providers, {allowFallbackToReadWrite: true});
      const result = await md.GetDatasetByName(DatasetName, ItemFilters);
          DatasetID: result.DatasetID,
          DatasetName: result.DatasetName,
          Status: result.Status,
          LatestUpdateDate: result.LatestUpdateDate,
          Results: JSON.stringify(result.Results),
        throw new Error('Error retrieving Dataset: ' + DatasetName);
      throw new Error('Error retrieving Dataset: ' + DatasetName + '\n\n' + err);
export class DatasetStatusResultType {
  EntityUpdateDates: string;
@Resolver(DatasetStatusResultType)
export class DatasetStatusResolver extends ResolverBase {
  @Query(() => DatasetStatusResultType)
  async GetDatasetStatusByName(
      const result = await md.GetDatasetStatusByName(DatasetName, ItemFilters);
          EntityUpdateDates: JSON.stringify(result.EntityUpdateDates),
        throw new Error('Error retrieving Dataset Status: ' + DatasetName);
      throw new Error('Error retrieving Dataset Status: ' + DatasetName + '\n\n' + err);
