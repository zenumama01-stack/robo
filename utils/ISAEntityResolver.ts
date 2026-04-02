import { EntityInfo, IEntityDataProvider, Metadata, UserInfo } from '@memberjunction/core';
import { Arg, Ctx, Field, ObjectType, Query, Resolver } from 'type-graphql';
 * Result type for the IS-A child entity discovery query.
 * Returns the name of the child entity type that has a record matching
 * the given parent entity's primary key, or null if no child exists.
export class ISAChildEntityResult {
    ChildEntityName?: string;
 * Result type for the IS-A child entities discovery query (plural).
 * Returns all child entity type names that have records matching the given
 * parent entity's primary key. Used for overlapping subtype parents.
export class ISAChildEntitiesResult {
    ChildEntityNames?: string[];
 * Resolver for IS-A entity hierarchy discovery.
 * Provides GraphQL endpoints for client-side code to discover child entity
 * records in an IS-A hierarchy. This enables bidirectional chain construction
 * where a loaded entity discovers its more-derived child type.
@Resolver(ISAChildEntityResult)
export class ISAEntityResolver {
     * primary key value. The server executes a single UNION ALL query across
     * all child entity tables for maximum efficiency.
     * @param EntityName The parent entity name to check children for
     * @param RecordID The primary key value to search for in child tables
     * @returns The child entity name if found, or null with Success=true if no child exists
    @Query(() => ISAChildEntityResult)
    async FindISAChildEntity(
        @Arg('RecordID', () => String) RecordID: string,
    ): Promise<ISAChildEntityResult> {
            const entityInfo = md.Entities.find(e => e.Name === EntityName);
                    ErrorMessage: `Entity '${EntityName}' not found`
            if (!entityInfo.IsParentType) {
            // Cast to IEntityDataProvider to access the optional FindISAChildEntity method
            const entityProvider = provider as unknown as IEntityDataProvider;
            if (!entityProvider.FindISAChildEntity) {
                    ErrorMessage: 'Provider does not support FindISAChildEntity'
            const result = await entityProvider.FindISAChildEntity(
                entityInfo,
                RecordID,
                userPayload?.userRecord
                    ChildEntityName: result.ChildEntityName
                ErrorMessage: e instanceof Error ? e.message : String(e)
     * Discovers ALL IS-A child entities that have records with the given primary
     * key value. Used for overlapping subtype parents (AllowMultipleSubtypes = true)
     * where multiple children can coexist. The server executes a single UNION ALL
     * query across all child entity tables for maximum efficiency.
    @Query(() => ISAChildEntitiesResult)
    async FindISAChildEntities(
    ): Promise<ISAChildEntitiesResult> {
                return { Success: true, ChildEntityNames: [] };
            if (!entityProvider.FindISAChildEntities) {
                    ErrorMessage: 'Provider does not support FindISAChildEntities'
            const results = await entityProvider.FindISAChildEntities(
                ChildEntityNames: results.map(r => r.ChildEntityName)
export default ISAEntityResolver;
