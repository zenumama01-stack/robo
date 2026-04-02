import { Metadata, CompositeKey, DatabaseProviderBase } from '@memberjunction/core';
import { Arg, Ctx, Field, InputType, ObjectType, Query, Resolver } from 'type-graphql';
import { CompositeKeyInputType, CompositeKeyOutputType } from '../generic/KeyInputOutputTypes.js';
export class EntityRecordNameInput {
  @Field(() => CompositeKeyInputType)
export class EntityRecordNameResult {
  @Field(() => CompositeKeyOutputType)
@Resolver(EntityRecordNameResult)
export class EntityRecordNameResolver {
  @Query(() => EntityRecordNameResult)
  async GetEntityRecordName(
    @Arg('EntityName', () => String) EntityName: string,
    @Arg('CompositeKey', () => CompositeKeyInputType) primaryKey: CompositeKey,
  ): Promise<EntityRecordNameResult> {
    return await this.InnerGetEntityRecordName(md, EntityName, primaryKey);
  @Query(() => [EntityRecordNameResult])
  async GetEntityRecordNames(
    @Arg('info', () => [EntityRecordNameInput]) info: EntityRecordNameInput[],
    @Ctx() {providers}: AppContext
  ): Promise<EntityRecordNameResult[]> {
    const result: EntityRecordNameResult[] = [];
    for (const i of info) {
      result.push(await this.InnerGetEntityRecordName(md, i.EntityName, i.CompositeKey));
  async InnerGetEntityRecordName(md: DatabaseProviderBase, EntityName: string, primaryKey: CompositeKeyInputType): Promise<EntityRecordNameResult> {
    const pk = new CompositeKey(primaryKey.KeyValuePairs);
    const e = md.Entities.find((e) => e.Name === EntityName);
      const recordName = await md.GetEntityRecordName(e.Name, pk);
      if (recordName) return { Success: true, Status: 'OK', CompositeKey: pk, RecordName: recordName, EntityName };
          Status: `Name for record, or record ${pk.ToString()} itself not found, could be an access issue if user doesn't have Row Level Access (RLS) if RLS is enabled for this entity`,
          CompositeKey: pk,
          EntityName,
    } else return { Success: false, Status: `Entity ${EntityName} not found`, CompositeKey: pk, EntityName };
export default EntityRecordNameResolver;
