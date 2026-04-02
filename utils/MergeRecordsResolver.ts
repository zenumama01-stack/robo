import { LogError, Metadata, CompositeKey } from '@memberjunction/core';
import { Arg, Ctx, Field, InputType, Int, Mutation, ObjectType, PubSub, PubSubEngine, Query, Resolver } from 'type-graphql';
export class EntityDependencyResult {
  EntityName: string; // required
  RelatedEntityName: string; // required
  FieldName: string; // required
@Resolver(EntityDependencyResult)
export class EntityDependencyResolver {
  @Query(() => [EntityDependencyResult])
  async GetEntityDependencies(
    @Arg('entityName', () => String) entityName: string,
    @Ctx() { dataSource, userPayload, providers }: AppContext,
      return md.GetEntityDependencies(entityName);
      const ctx = z.object({ message: z.string() }).catch(null).parse(err)?.message ?? JSON.stringify(err);
      throw new Error(ctx);
export class RecordDependencyResult {
@Resolver(RecordDependencyResult)
export class RecordDependencyResolver {
  @Query(() => [RecordDependencyResult])
  async GetRecordDependencies(
    @Arg('CompositeKey', () => CompositeKeyInputType) ckInput: CompositeKeyInputType,
      const ck = new CompositeKey(ckInput.KeyValuePairs);
      const result = await md.GetRecordDependencies(entityName, ck);
      // Map PrimaryKey to CompositeKey for GraphQL response
      return result.map(dep => ({
        EntityName: dep.EntityName,
        RelatedEntityName: dep.RelatedEntityName,
        FieldName: dep.FieldName,
        CompositeKey: dep.PrimaryKey // Map PrimaryKey to CompositeKey
class FieldMapping {
class FieldMappingOutput {
  @Field(() => [CompositeKeyInputType])
  @Field(() => [FieldMapping], { nullable: true })
  FieldMap?: FieldMapping[];
export class RecordMergeRequestOutput {
  SurvivingRecordID: number;
  @Field(() => [Int])
  RecordsToMerge: number[];
  @Field(() => [FieldMappingOutput], { nullable: true })
  FieldMap?: FieldMappingOutput[];
  CompositeKey: CompositeKeyOutputType;
  RecordMergeDeletionLogID?: number;
  OverallStatus: string;
  RecordMergeLogID: number;
  @Field(() => [RecordMergeDetailResult])
  RecordStatus: RecordMergeDetailResult[];
  @Field(() => RecordMergeRequestOutput)
  Request: RecordMergeRequestOutput;
@Resolver(RecordMergeResult)
export class RecordMergeResolver extends ResolverBase {
  @Mutation(() => RecordMergeResult)
  async MergeRecords(
    @Arg('request', () => RecordMergeRequest) request: RecordMergeRequest,
    // Check API key scope authorization for entity merge operation
    await this.CheckAPIKeyScopeAuthorization('entity:merge', request.EntityName, userPayload);
      const md = GetReadWriteProvider(providers);
      const options = {};
      const result = await md.MergeRecords(request, userPayload.userRecord, options);
export default EntityDependencyResolver;
