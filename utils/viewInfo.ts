import { BaseInfo } from '../generic/baseInfo'
import { EntityFieldInfo, EntityInfo } from '../generic/entityInfo'
import { IMetadataProvider } from '../generic/interfaces';
import { LogError } from '../generic/logging';
export class ViewColumnInfo extends BaseInfo {
    ID: number = null
    hidden: boolean = null
    width?: number = null
    orderIndex?: number = null
    EntityField: EntityFieldInfo = null
export const ViewFilterLogicInfo = {
    And: 'And',
    Or: 'Or',
export type ViewFilterLogicInfo = typeof ViewFilterLogicInfo[keyof typeof ViewFilterLogicInfo];
export class ViewFilterInfo extends BaseInfo {
    logicOperator: ViewFilterLogicInfo = null
    field: string = null
    operator: string = null
    value: string = null
    filters: ViewFilterInfo[] = []
        if (initData && initData.logic) {
            this.logicOperator = initData.logic.trim().toLowerCase() == 'and' ? ViewFilterLogicInfo.And : ViewFilterLogicInfo.Or
        if (initData && initData.filters) {
            this.filters = initData.filters.map(f => new ViewFilterInfo(f))
export class ViewGridState {
    sortSettings?: any;
    columnSettings?: any;
    filter?: any;
 * This class represents a View in the system. A View is a saved set of filters, columns, and other settings that can be applied to a grid to show a specific set of data.
export class ViewInfo extends BaseInfo {
     * Unique identifier for the view record
     * Foreign key reference to the user who created this view
    UserID: number = null
     * Foreign key reference to the entity this view is based on
    EntityID: number = null
     * Name of the view for display and reference
     * Detailed description of what this view displays and its purpose
     * Foreign key reference to the View Categories entity for organizing views
    CategoryID: number = null
     * When true, this view is available to other users besides the creator
    IsShared: boolean = null
     * When true, this view is the default view for the entity
    IsDefault: boolean = null
     * JSON string containing the complete grid state including columns, sorting, and filters
    GridState: string = null
     * JSON string containing the filter configuration for this view
    FilterState: string = null
     * JSON string containing custom filter configuration added by the user
    CustomFilterState: string = null
     * SQL WHERE clause generated from the filter state for query execution
    WhereClause: string = null
     * Custom SQL WHERE clause that can be added to supplement the generated WHERE clause
    CustomWhereClause: string = null
     * Date and time when this view was created
     * Date and time when this view was last updated
     * Name of the user who owns this view (from related User entity)
    UserName: string = null
     * Type of user who owns this view (from related User entity)
    UserType: string = null
     * Name of the entity this view is based on (from related Entity)
     * Base view name for the entity used in queries
    private _Filter: ViewFilterInfo[] = []
    public get Filter(): ViewFilterInfo[] {
        return this._Filter
    private _Columns: ViewColumnInfo[] = []
    public get Columns(): ViewColumnInfo[] {
        return this._Columns
    public InitFromData(md: IMetadataProvider, initData: any) {
                if (initData.EntityID) {
                    const match = mdEntities.find(e => e.ID == initData.EntityID) 
                        this._EntityInfo = match 
                else if (initData._EntityInfo) 
                    this._EntityInfo = initData._EntityInfo
                // set up the filters and the columns
                if (initData.GridState) {
                    const gridState = JSON.parse(initData.GridState)
                    if (gridState && gridState.columnSettings) {
                        this._Columns = gridState.columnSettings.map(c => {
                            // find the entity field and put it in place inside the View Metadata for easy access
                            if (c) {
                                // check to make sure the current item is non-null to ensure metadata isn't messed up 
                                const field = this._EntityInfo.Fields.find(f => f.Name.trim().toLowerCase() == c.Name.trim().toLowerCase())
                                return new ViewColumnInfo({...c, EntityField: field})
                                LogError('null column setting found in view grid state for columns - ViewID: ' + initData.ID)
                if (initData.FilterState) {
                    this._Filter = [new ViewFilterInfo(JSON.parse(initData.FilterState))]
        catch(e) {
        this.InitFromData(md, initData);
