import { BaseEngine, BaseEnginePropertyConfig, BaseEntity, IMetadataProvider, UserInfo } from "@memberjunction/core";
import { MJActionExecutionLogEntity, MJActionResultCodeEntity, MJEntityActionFilterEntity, MJEntityActionInvocationEntity, MJEntityActionInvocationTypeEntity, MJEntityActionParamEntity } from "@memberjunction/core-entities";
import { ActionParam, RunActionParams } from "./ActionEngine-Base";
import { EntityActionEntityExtended } from "./EntityActionEntity-Extended";
 * Parameters type for invoking an entity action
export class EntityActionInvocationParams {
     * The entity action to be invoked
    public EntityAction: EntityActionEntityExtended;
     * The type of entity/action invocation to be performed
    public InvocationType: MJEntityActionInvocationTypeEntity;
     * The user context for the invocation.  
    public ContextUser?: UserInfo;
     * If the invocation type is single record oriented, this parameter will be needed
    public EntityObject?: BaseEntity;
     * If the invocation type is view-oriented, this parameter will be needed
    public ViewID?: string;
     * If the invocation type is list-oriented, this parameter will be needed
    public ListID?: string;
export class EntityActionResult {
     * Note that the log entry will be created 
 * The purpose of this class is to handle the invocation of actions for entities in all of the supported invocation contexts.
export class EntityActionEngineBase extends BaseEngine<EntityActionEngineBase> {
    public static get Instance(): EntityActionEngineBase {
        return super.getInstance<EntityActionEngineBase>("EntityActionEngineBase");
    // internal instance properties used for the singleton pattern
    private _EntityActions: EntityActionEntityExtended[] = [];
    private _EntityActionParams: MJEntityActionParamEntity[] = [];
    private _EntityActionInvocationTypes: MJEntityActionInvocationTypeEntity[] = [];
    private _EntityActionFilters: MJEntityActionFilterEntity[] = [];
    private _EntityActionInvocations: MJEntityActionInvocationEntity[] = [];
        const configs: Partial<BaseEnginePropertyConfig>[] = [
                EntityName: 'MJ: Entity Action Invocation Types',
                PropertyName: '_EntityActionInvocationTypes',
                EntityName: 'MJ: Entity Action Filters',
                PropertyName: '_EntityActionFilters',
                EntityName: 'MJ: Entity Action Invocations',
                PropertyName: '_EntityActionInvocations',
                EntityName: 'MJ: Entity Actions', // sub-class for this will handle dynamic loading of filters, invocations, and params when needed by callers of those read-only properties
                PropertyName: '_EntityActions',
                EntityName: 'MJ: Entity Action Params',
                PropertyName: '_EntityActionParams',
        await this.Load(configs, provider, forceRefresh, contextUser);
     * List of all the MJEntityActionInvocationTypeEntity objects that are available for use in the system. Make sure you call Config() before any other methods on this class.
    public get InvocationTypes(): MJEntityActionInvocationTypeEntity[] {
        return this._EntityActionInvocationTypes;
     * List of all the MJEntityActionFilterEntity objects that are available for use in the system. Make sure you call Config() before any other methods on this class.
    public get Filters(): MJEntityActionFilterEntity[] {
        return this._EntityActionFilters;
     * List of all the MJEntityActionInvocationEntity objects that are available for use in the system. Make sure you call Config() before any other methods on this class.
    public get Invocations(): MJEntityActionInvocationEntity[] {
        return this._EntityActionInvocations;
     * List of all the Entity Action objects that are available for use in the system. Make sure you call Config() before any other methods on this class.
    public get EntityActions(): EntityActionEntityExtended[] {
        return this._EntityActions;
     * List of all of the Entity Action Params that are available for use in the system. Make sure you call Config() before any other methods on this class.
    public get Params(): MJEntityActionParamEntity[] {   
        return this._EntityActionParams;
     * Helper method to get the EntityActionEntityExtended object for a given entity name
     * @param entityName 
     * @param status Optional, if provided will filter the results based on the status
    public GetActionsByEntityName(entityName: string, status?: 'Active' | 'Pending' | 'Disabled'): EntityActionEntityExtended[] {
        return this._EntityActions.filter(e => (!status || e.Status === status) && e.Entity.trim().toLowerCase() === entityName.trim().toLowerCase());
     * Helper method to get the EntityActionEntityExtended object for a given entity ID
    public GetActionsByEntityID(entityID: string): EntityActionEntityExtended[] {
        return this._EntityActions.filter(e => e.EntityID === entityID);
     * Helper method to get the EntityActionEntityExtended object for a given entity name and invocation type
     * @param invocationType 
    public GetActionsByEntityNameAndInvocationType(entityName: string, invocationType: string, status?: 'Active' | 'Pending' | 'Disabled'): EntityActionEntityExtended[] {
        const entityActions = this.GetActionsByEntityName(entityName, status);
        // now extract the ones that have the right invocation type
        return entityActions.filter(e => { 
            const invocations = e.Invocations.find(i => (!status || i.Status === status) && i.InvocationType.trim().toLowerCase() === invocationType.trim().toLowerCase())
            return invocations ? true : false;
