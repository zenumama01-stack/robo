import { Metadata, BaseEntity, BaseInfo, EntityInfo, EntityFieldInfo,RunView, UserInfo, EntitySaveOptions, LogError, EntityFieldTSType, EntityPermissionType, BaseEntityResult } from "@memberjunction/core";
import { MJUserViewEntity } from "../generated/entity_subclasses";
import { ResourcePermissionEngine } from "./ResourcePermissions/ResourcePermissionEngine";
@RegisterClass(BaseEntity, 'MJ: User Views')
export class UserViewEntityExtended extends MJUserViewEntity  {
    private _ViewEntityInfo: EntityInfo = null
     * This is a read-only property that returns the filters for this view. This information
     * is persisted in a JSON format in the FilterState column of the MJUserViewEntity table. To access
     * the filters easily, use this property.
     * @type {ViewFilterInfo[]}
     * @memberof UserViewEntitySubclass
        if (this.FilterState) {
            return [new ViewFilterInfo(JSON.parse(this.FilterState))]
     * This is a read-only property that returns the columns for this view. This information
     * is persisted in a JSON format in the GridState column of the MJUserViewEntity table. To access
     * the columns easily, use this property. 
        // now, we need to do some post-processing once we've loaded the raw data so that our 
        // columns and filters are set up correctly
        if (this.GridState) {
            const gridState = JSON.parse(this.GridState)
                const columns = gridState.columnSettings.map(c => {
                        const field = this.ViewEntityInfo.Fields.find(f => f.Name.trim().toLowerCase() === c.Name.trim().toLowerCase())
                        console.log('null column setting found in view grid state for columns - ViewID: ' + this.ID)
        // if we get here, we don't have column info, return an empty array
     * The entity info for the entity that this view is based on
     * @type {EntityInfo}
    public get ViewEntityInfo(): EntityInfo {
        return this._ViewEntityInfo
    public get ViewSortInfo(): ViewSortInfo[] {
        if (this.SortState) {
            const sortTemp = JSON.parse(this.SortState)
            if (sortTemp && sortTemp.length > 0) 
                return sortTemp.map(s => new ViewSortInfo(s))
        // if we get here return a blank array
    public get OrderByClause(): string {
        if (this.ViewSortInfo && this.ViewSortInfo.length > 0) {
            return this.ViewSortInfo.map(s => {
                let dir: string;
                if (typeof s.direction === 'string')
                    dir = s.direction.trim().toLowerCase()
                else if (s.direction === 1 ) // some legacy view metadata has 1/2 for asc/desc
                    dir = 'asc'
                else if (s.direction === 2 )
                    dir = 'desc'
                    dir = '';
                const desc = dir === ViewSortDirectionInfo.Desc.trim().toLowerCase()
                return s.field + (desc ? ' DESC' : '');
            }).join(', ')
     * Get the parsed DisplayState object from the DisplayState JSON column.
     * DisplayState contains view mode preferences and mode-specific configuration (e.g., timeline settings).
     * @returns The parsed ViewDisplayState object or null if not set/invalid
    public get ParsedDisplayState(): ViewDisplayState | null {
        if (this.DisplayState) {
                return JSON.parse(this.DisplayState) as ViewDisplayState;
                console.warn('Failed to parse DisplayState JSON:', e);
     * Set the DisplayState from a ViewDisplayState object.
     * @param state The ViewDisplayState object to serialize and store
    public setDisplayState(state: ViewDisplayState): void {
        this.DisplayState = JSON.stringify(state);
     * Get the default view mode from DisplayState, or 'grid' if not set.
    public get DefaultViewMode(): ViewDisplayMode {
        return this.ParsedDisplayState?.defaultMode || 'grid';
     * Get the timeline configuration from DisplayState.
     * @returns TimelineState or null if not configured
    public get TimelineConfig(): ViewTimelineState | null {
        return this.ParsedDisplayState?.timeline || null;
     * Update timeline configuration in DisplayState.
     * Creates DisplayState if it doesn't exist.
     * @param config The timeline configuration to set
    public setTimelineConfig(config: ViewTimelineState): void {
        const current = this.ParsedDisplayState || { defaultMode: 'grid' as ViewDisplayMode };
        current.timeline = config;
        this.setDisplayState(current);
     * Check if a specific view mode is enabled.
     * By default, all modes are enabled unless explicitly disabled.
     * @param mode The view mode to check
    public isViewModeEnabled(mode: ViewDisplayMode): boolean {
        const enabledModes = this.ParsedDisplayState?.enabledModes;
        if (!enabledModes) return true; // All modes enabled by default
        return enabledModes[mode] !== false;
     * Get the best date field for timeline display based on entity metadata.
     * Priority: 1) DefaultInView=true date fields (lowest Sequence wins)
     *           2) Any date field (lowest Sequence wins)
     * @returns The field name or null if no date fields exist
    public getDefaultTimelineDateField(): string | null {
        if (!this._ViewEntityInfo) return null;
        const dateFields = this._ViewEntityInfo.Fields.filter(f =>
            f.TSType === EntityFieldTSType.Date
        if (dateFields.length === 0) return null;
        // Priority 1: DefaultInView date fields, sorted by Sequence
        const defaultInViewDateFields = dateFields
        if (defaultInViewDateFields.length > 0) {
            return defaultInViewDateFields[0].Name;
        // Priority 2: Any date field, sorted by Sequence
        const sortedDateFields = dateFields.sort((a, b) => a.Sequence - b.Sequence);
        return sortedDateFields[0].Name;
     * Get all date fields available for timeline configuration.
     * @returns Array of field info objects for date fields
    public getAvailableDateFields(): EntityFieldInfo[] {
        if (!this._ViewEntityInfo) return [];
        return this._ViewEntityInfo.Fields.filter(f =>
    override async LoadFromData(data: any): Promise<boolean> {
        // in this case we need to make sure we ge the _ViewEntityInfo property set up correctly
        if (data && data.EntityID) {
            const match = md.Entities.find(e => e.ID === data.EntityID)
                this._ViewEntityInfo = match
                throw new Error('Unable to find entity info for entity ID: ' + data.EntityID)
        return await super.LoadFromData(data)
     * This property determines if the specified user can edit the view object. All of the below assumes the user has base Create/Update permissions on the "MJ: User Views" entity. 
     * The flow of the logic is:
     *  1) The view is a new record, by definition the user can edit this because it is new
     *  2) The user is the owner of the current view - e.g. UserID in the view record matches the ID of the user provided, allowed
     *  3) The user is a sysadmin (Type === 'Owner' in the User object), allowed
     *  4) If neither of the above conditions are met, the Resource Permissions are checked to see if the user, directly or via Roles, has either Edit or Owner permissions on the current view
        if (this._cachedCanUserEdit === null) {
            this._cachedCanUserEdit = this.CalculateUserCanEdit()
        return this._cachedCanUserEdit;
    private _cachedCanUserEdit: boolean = null
     * This property determines if the specified user can view the view at all.
    public get UserCanView(): boolean {
        if (this._cachedCanUserView === null) {
            this._cachedCanUserView = this.CalculateUserCanView()
        return this._cachedCanUserView;
    private CalculateUserCanView(): boolean {
        const bOwner = this.UserID === md.CurrentUser.ID;
        if (bOwner) {
            return true
            // not the owner, let's see if the user has permissions or not
            const rt = ResourcePermissionEngine.Instance.ResourceTypes.find((rt: any) => rt.Name === 'MJ: User Views');
            if (!rt)
                throw new Error('Resource Type User Views not found');
            const permLevel = ResourcePermissionEngine.Instance.GetUserResourcePermissionLevel(rt.ID, this.ID, md.CurrentUser);
            if (permLevel) // any permission level allows view access
            else // perm level not found so return false
    private _cachedCanUserView: boolean = null
     * This property determines if the specified user can delete the view object. All of the below assumes the user has base Delete permissions on the "MJ: User Views" entity.
     *  1) The view is a new record, by definition the user can't delete this because it is new
     *  4) If neither of the above conditions are met, the Resource Permissions are checked to see if the user, directly or via Roles, has OWNER permissions on the current view
        if (this._cachedUserCanDelete === null) {
            this._cachedUserCanDelete = this.CalculateUserCanDelete()
        return this._cachedUserCanDelete;
    private _cachedUserCanDelete: boolean = null
    protected ResetCachedCanUserSettings() {
        this._cachedCanUserEdit = null;
        this._cachedUserCanDelete = null;
    private CalculateUserCanDelete(): boolean {
        if (!this.IsSaved)
            return false; // new records can't be deleted
            // EXISTING record in the database
            // check to see if the current user is the OWNER of this view via the UserID property in the record, if there's a match, the user OWNS this views
            const user: UserInfo = this.ContextCurrentUser || md.CurrentUser; // take the context current user if it is set, otherwise use the global current user
            if (this.UserID === user.ID || user.Type.trim().toLowerCase() === 'owner' ) {
                return this.CheckPermissions(EntityPermissionType.Delete, false); // exsiting records OWNED by current user, can be edited so long as we have Update permissions;
                // if the user is not an admin, and they are NOT the owner of the view, we check the permissions on the resource
                const perms = ResourcePermissionEngine.Instance.GetUserResourcePermissionLevel(this.ViewResourceTypeID, this.ID, user);
                return perms === 'Owner'; // this is the only level that can delete a view
    private CalculateUserCanEdit(): boolean {
        if (!this.IsSaved) {
            return this.CheckPermissions(EntityPermissionType.Create, false); // new records an be edited so long as we have Create permissions
            // so of course they can save it
            if (this.UserID === user.ID || user.Type.trim().toLowerCase() === 'owner') {
                return this.CheckPermissions(EntityPermissionType.Update, false); // exsiting records OWNED by current user, can be edited so long as we have Update permissions;
                return perms === 'Owner' || perms === 'Edit'; // these are the only two levels that can save a view
     * Returns the ID of the Resource Type metadata record that corresponds to the User Views entity
    public get ViewResourceTypeID(): string {
        if (!this._ViewResourceTypeID) {
            const rt = ResourcePermissionEngine.Instance.ResourceTypes;
            const rtUV = rt.find(r => r.Entity === 'MJ: User Views');
            if (!rtUV)
                throw new Error('Unable to find Resource Type for User Views entity');
            this._ViewResourceTypeID = rtUV.ID;
        return this._ViewResourceTypeID;
    private _ViewResourceTypeID: string = null
    override async Load(ID: string, EntityRelationshipsToLoad?: string[]): Promise<boolean> {
        // first load up the view info, use the superclass to do this
        const result = await super.Load(ID, EntityRelationshipsToLoad)
            // first, cache a copy of the entity info for the entity that is used in this view
            const match = md.Entities.find(e => e.ID === this.EntityID) 
                throw new Error('Unable to find entity info for entity ID: ' + this.EntityID)
        this.ResetCachedCanUserSettings();
    override async Delete(): Promise<boolean> {
        if (this.UserCanDelete) {
            // if we get here, the user can delete the view, so we delete it
            if (super.Delete()) {
            // if we get here, the user can't delete the view, so we don't delete it, add a last error and return false
            const res: BaseEntityResult = new BaseEntityResult();
            res.Success = false;
            res.Message = 'User does not have permission to delete this view';
            res.StartedAt = new Date();
            res.EndedAt = new Date();
            this.ResultHistory.push(res);
    override async Save(options?: EntitySaveOptions): Promise<boolean> {
        if (this.UserCanEdit) {
            // we want to preprocess the Save() call because we need to regenerate the WhereClause in some situations
            const id = this.ID;
            const filterStateField = this.Fields.find(c => c.Name.toLowerCase() == 'filterstate');
            const smartFilterEnabledField = this.Fields.find(c => c.Name.toLowerCase() == 'smartfilterenabled');
            const smartFilterPromptField = this.Fields.find(c => c.Name.toLowerCase() == 'smartfilterprompt');
            if (!this.ID ||
                options?.IgnoreDirtyState || 
                filterStateField?.Dirty ||
                smartFilterEnabledField?.Dirty ||
                smartFilterPromptField?.Dirty) {
                // either we're ignoring dirty state or the filter state is dirty, so we need to update the where clause
                await this.UpdateWhereClause(options?.IgnoreDirtyState);
            // now call our superclass to do the actual save()
            if (await super.Save(options)) {
            // if we get here, the user can't edit the view, so we don't save it, add a last error and return false
            res.Message = this.ID ? 'User does not have permission to edit this view' : 'User does not have permission to create a new view';
    public async SetDefaultsFromEntity(e: EntityInfo) {
        this.EntityID = e.ID;
        const newGridState = new ViewGridState();
        newGridState.columnSettings = [];
        e.Fields.filter(f => f.DefaultInView).forEach(f => {
            newGridState.columnSettings.push({
                width: f.DefaultColumnWidth,
                orderIndex: newGridState.columnSettings.length
        this.GridState = JSON.stringify(newGridState); // default columns for a view are the ones with DefaultInView turned on
    override NewRecord(): boolean {
        const result = super.NewRecord();
            if (this.ContextCurrentUser) {
                this.UserID = this.ContextCurrentUser.ID;
                if (md.CurrentUser)
                    this.UserID = md.CurrentUser.ID;   
                    throw new Error('Unable to determine current user for new view record');
            this.IsDefault = false;
            this.WhereClause = '';
            this.FilterState = JSON.stringify({"logic" : "and", "filters" : [] }); // blank default for filter
            this.GridState = JSON.stringify({}); // blank object initially
            this.CustomFilterState = false;
            this.CustomWhereClause = false;
            //this.SmartFilterEnabled = false;
     * This method is used to update the view's where clause based on the following logic.
     * 1) If the view has a regular Filter State (which is typically set by an end-user in a UI), the FilterState will be processed and a WHERE clause will be generated
     * 2) If SmartFilterEnabled === 1 and the view has a SmartFilterPrompt, the SmartFilterPrompt will be processed by AI and the SmartFilterWhereClause will be generated. SmartFilterWhereClause will only be generated whenever the SmartFilterPrompt changes.
     * 3) If CustomWhereClause === 1, this function will NOT modify the WhereClause because the sysadmin has set CustomWhereClause === 1 which means we don't want any changes to this particular view's WhereClause
     * IMPORTANT NOTE: This method does not save the record. You still call .Save() to save the record as desired. If you want to get the new WhereClause based on the FilterState but not 
     * update the FilterState column, call the GenerateWhereClause() method.
     * KEY ASSUMPTION: The server code must set a property in the MJGlobal.Properties array with a key of OPENAI_API_KEY to use the AI functionality. If this property is not set, the AI functionality will not work.
    public async UpdateWhereClause(ignoreDirtyState?: boolean) {
        if (this.CustomWhereClause && (this.CustomWhereClause === true || this.CustomWhereClause === 1))
            // if the CustomWhereClause is set to true or 1, we don't want to update the WhereClause
        // if we get here, we need to update the WhereClause, first check to see if we have a Smart Filter or not
        if (this.SmartFilterEnabled && (this.SmartFilterEnabled === true || this.SmartFilterEnabled === 1) &&
            this.SmartFilterPrompt && this.SmartFilterPrompt.length > 0) {
            if (this.SmartFilterImplemented) {
                // The following block of code is only intended to execute when we're in an execution context where the 
                // lowest level sub-class supports SmartFilter WHere Clause generation, which is determined by the class
                // implementation by overriding the SmartFilterImplemented property and returning true
                // So, if we're here we handle the Smart Filter                
                // we have a smart filter prompt (e.g. a prompt for the AI to create the where clause)
                // if the SmartFilterPrompt has changed, then we need to update the SmartFilterWhereClause using AI
                // otherwise, we don't need to do anything other than just use the SmartFilterWhereClause as it is
                if (!this.ID || ignoreDirtyState || this.Fields.find(c => c.Name.toLowerCase() == 'smartfilterprompt')?.Dirty) {
                    // the prompt has changed (or is newly populated, either way it is dirty) so use the AI to figure this out
                    const result = await this.GenerateSmartFilterWhereClause(this.SmartFilterPrompt, this.ViewEntityInfo);
                    this.SmartFilterWhereClause = result.whereClause;
                    this.SmartFilterExplanation = result.userExplanation;
                // now that we have the SmartFilterWhereClause, we need to update the WhereClause property
                this.WhereClause = this.SmartFilterWhereClause;
                // while we do have smart filter in this view and the prompt is populated and might be dirty/etc, we don't have a sub-class that supports smart filter
                // so we do NOTHING here. The idea is that this code will execute again on the SERVER side where the sub-class will be able to handle the smart filter
                // and properly generate the SmartFilterWhereClause.
            this.WhereClause = this.GenerateWhereClause(this.FilterState, this.ViewEntityInfo);
     * This is a stub method - the intent is for the server-only sub-class to override this method and implement the AI functionality to generate a where clause based on the prompt provided
     * @param prompt - string from the end user describing the filter they want to apply
     * @param entityInfo - entity info for the entity that the view is based on
    public async GenerateSmartFilterWhereClause(prompt: string, entityInfo: EntityInfo): Promise<{whereClause: string, userExplanation: string}> {
        return { whereClause: '', userExplanation: ''}; // stub function returns blank where clause. Sub-Class will do this.
     * This is a stub method that always returns false - the intent is for the server-only sub-class to override this and return true if it supprots smart filters and then the rest of the smart filter
     * infrastructure will be enabled from the UpdateWhereClause() method.
    protected get SmartFilterImplemented(): boolean {
        return false; // stub function returns false. Sub-Class will do this.
        // call the superclass first and set the value internally there
        if (FieldName.toLowerCase() == 'entityid') {
            // we're updating the entityID, need to upate the _ViewEntityInfo property so it is always in sync
            const match = md.Entities.find(e => e.ID === Value)
                throw new Error('Unable to find entity info for entity ID: ' + Value)    
     * Create a where clause for SQL from the structured filter state JSON information
     * @param FilterState A string containing a valid Filter State JSON string - this uses the format that the Kendo Filter component uses which is generic and can be used anywhere
     * with/without kendo
     * @param EntityInfo The entity info for the entity that the UserView is based on
     * @returns a string that represents a valid SQL WHERE clause
     * @example Example Filter State JSON below
        FilterState = `{
          "logic": "or",
          "filters": [{
            "field": "Name",
            "operator": "startswith",
            "value": "A"
              "field": "TotalRevenue",
              "operator": "gt",
              "value": 10000000
              "field": "NumberEmployees",
              "operator": "gte",
              "value": 25
              "field": "InformationTechnologyExpense",
              "value": 500000
              "logic": "and",
                "field": "City",
                "operator": "eq",
                "value": "Chicago"
                "field": "ActivityCount",
                "value": 5
            "field": "LatestActivityDate",
            "value": "2023-01-01T06:00:00.000Z"
    protected GenerateWhereClause(FilterState: string, entityInfo: EntityInfo): string {
        return this.processFilterGroup(JSON.parse(FilterState), entityInfo);
    private wrapQuotes(value: string, needQuotes: boolean): string {
        return needQuotes ? `'${value}'` : value;
    private convertFilterToSQL(
        field: string,
        operator: string,
        entity: EntityInfo
        let op: string = '';
        const f = entity.Fields.find((f) => f.Name.trim().toLowerCase() === field.trim().toLowerCase());
            throw new Error('Unable to find field ' + field + ' in entity ' + entity.Name);
        let newValue = value;
        if (f.TSType === EntityFieldTSType.Boolean) {
                newValue = value ? '1' : '0';
                newValue = value.trim().toLowerCase() === 'true' ? '1' : '0';
                // Handle numbers, null, undefined, etc.
        switch (operator) {
            case 'eq':
                op = '= ' + this.wrapQuotes(newValue, f.NeedsQuotes);
            case 'neq':
                op = '<> ' + this.wrapQuotes(newValue, f.NeedsQuotes);
            case 'gt':
                op = '> ' + this.wrapQuotes(newValue, f.NeedsQuotes);
            case 'gte':
                op = '>= ' + this.wrapQuotes(newValue, f.NeedsQuotes);
            case 'lt':
                op = '< ' + this.wrapQuotes(newValue, f.NeedsQuotes);
            case 'lte':
                op = '<= ' + this.wrapQuotes(newValue, f.NeedsQuotes);
            case 'startswith':
                op = `LIKE '${newValue}%'`;
            case 'endswith':
                op = `LIKE '%${newValue}'`;
            case 'contains':
                op = `LIKE '%${newValue}%'`;
            case 'doesnotcontain':
                op = `NOT LIKE '%${newValue}%'`;
            case 'isnull':
            case 'isempty':
                op = 'IS NULL';
            case 'isnotnull':
            case 'isnotempty':
                op = 'IS NOT NULL';
        return `[${field}] ${op}`;
    private processFilterGroup(filterGroup: any, entity: EntityInfo): string {
        // each filter has two properties, logic and filters
        // logic is either 'and' or 'or' and is what we use to determine the SQL logic operator
        // filters is an array of filters, each filter has a field, operator, and value,
        let bFirst: boolean = true;
        const logic: string = filterGroup.logic.toUpperCase();
        for (const filter of filterGroup.filters) {
            if (!bFirst) 
                whereClause += ` ${logic} `;
                bFirst = false;
            // if an individual filter has a "logic" property, it's a group and we need to process it with parenthesis and recurisely
            if (filter.logic && filter.logic.length > 0) {
                // this is a group, we process it with parenthesis
                whereClause += `(${this.processFilterGroup(filter, entity)})`;
                // this is an individual filter, easy to process
                whereClause += `(${this.convertFilterToSQL(
                    filter.field,
                    filter.operator,
                    filter.value,
                )})`;
        return whereClause;
export class ViewInfo {
     * Returns a list of views for the specified user. If no user is specified, the current user is used.
     * @param contextUser optional, the user to use for context when loading the view
     * @param entityId optional, the entity ID to filter the views by, if not provided, there is no filter on EntityID and all views for the user are returned
     * @returns an array of UserViewEntityBase objects
     * @memberof ViewInfo
    static async GetViewsForUser(entityId?: string, contextUser?: UserInfo): Promise<UserViewEntityExtended[]> {
            ExtraFilter: `UserID = '${contextUser ? contextUser.ID : md.CurrentUser.ID}'
                         ${entityId ? ` AND EntityID = '${entityId}'` : ''}`
        const rd = result?.Results as Array<any>;
        if (result && result.Success && rd) 
            return rd;
     * Returns a view ID for a given viewName
     * @param viewName Name of the view to lookup the ID for 
     * @returns the ID of the User View record that matches the provided name, if found
    static async GetViewID(viewName: string): Promise<string> {
        const result = await rv.RunView({EntityName: 'MJ: User Views', ExtraFilter: `Name = '${viewName}'`})
        if (result && result.Success && rd && rd.length > 0) {
            return rd[0].ID
            throw new Error('Unable to find view with name: ' + viewName)
     * Gets a User View entity from the UserViewEngine cache.
     * @param viewId record ID for the view to load
     * @param contextUser optional, the user context for loading views
     * @returns UserViewEntityBase (or a subclass of it)
     * @throws Error if the view is not found in the engine cache
    static async GetViewEntity(viewId: string, contextUser?: UserInfo): Promise<UserViewEntityExtended> {
        const { UserViewEngine } = await import('../engines/UserViewEngine');
        // Ensure the engine is configured before use
        await UserViewEngine.Instance.Config(false, contextUser);
        const view = UserViewEngine.Instance.GetViewById(viewId);
        if (view) {
        throw new Error('Unable to find view with ID: ' + viewId);
     * Gets a User View entity from the UserViewEngine cache by name.
     * @param viewName name for the view to load
    static async GetViewEntityByName(viewName: string, contextUser?: UserInfo): Promise<UserViewEntityExtended> {
        const view = UserViewEngine.Instance.GetViewByName(viewName);
        throw new Error('Unable to find view with name: ' + viewName);
 * Column pinning position for AG Grid
export type ViewColumnPinned = 'left' | 'right' | null;
 * Column information for a saved view, including AG Grid-specific properties
    /** Entity field ID */
    /** Display name for column header (from entity metadata) */
    /** User-defined display name override for column header */
    userDisplayName?: string = null
    /** Whether column is hidden */
    /** Column order index */
    // AG Grid-specific properties
    /** Column pinning position ('left', 'right', or null for not pinned) */
    pinned?: ViewColumnPinned = null
    /** Flex grow factor (for auto-sizing columns) */
    flex?: number = null
    minWidth?: number = null
    maxWidth?: number = null
    format?: ColumnFormat = null
    /** Reference to the entity field metadata (not persisted) */
export const ViewSortDirectionInfo = {
    Asc: 'Asc',
    Desc: 'Desc',
export type ViewSortDirectionInfo = typeof ViewSortDirectionInfo[keyof typeof ViewSortDirectionInfo];
export class ViewSortInfo extends BaseInfo {
    direction: ViewSortDirectionInfo = null
        if (initData && initData.dir && typeof initData.dir == 'string') {
            this.direction = initData.dir.trim().toLowerCase() == 'asc' ? ViewSortDirectionInfo.Asc : ViewSortDirectionInfo.Desc
 * Sort setting for a single column
export interface ViewGridSortSetting {
    dir: 'asc' | 'desc';
 * Column setting as persisted in GridState JSON
 * This is the serializable form of ViewColumnInfo (without EntityField reference)
export interface ViewGridColumnSetting {
    hidden?: boolean;
    orderIndex?: number;
    pinned?: ViewColumnPinned;
    /** Flex grow factor */
 * Text styling options for column headers and cells
export interface ColumnTextStyle {
    /** Bold text */
    /** Italic text */
    /** Underlined text */
    /** Text color (CSS color value) */
    /** Background color (CSS color value) */
    backgroundColor?: string;
 * Conditional formatting rule for dynamic cell styling
export interface ColumnConditionalRule {
    /** Condition type */
    condition: 'equals' | 'notEquals' | 'greaterThan' | 'lessThan' | 'greaterThanOrEqual' | 'lessThanOrEqual' | 'between' | 'contains' | 'startsWith' | 'endsWith' | 'isEmpty' | 'isNotEmpty';
    /** Value to compare against */
    value?: string | number | boolean;
    /** Second value for 'between' condition */
    value2?: number;
    /** Style to apply when condition is met */
    style: ColumnTextStyle;
 * Column formatting configuration
 * Supports value formatting, alignment, header/cell styling, and conditional formatting
export interface ColumnFormat {
     * Format type - determines which formatter to use
     * 'auto' uses smart defaults based on field metadata
    type?: 'auto' | 'number' | 'currency' | 'percent' | 'date' | 'datetime' | 'boolean' | 'text';
    /** Decimal places for number/currency/percent types */
    decimals?: number;
    /** Currency code (ISO 4217) for currency type, e.g., 'USD', 'EUR' */
    currencyCode?: string;
    /** Show thousands separator for number types */
    thousandsSeparator?: boolean;
     * Date format preset or custom pattern
     * Presets: 'short', 'medium', 'long'
     * Custom: Any valid date format string
    dateFormat?: 'short' | 'medium' | 'long' | string;
    /** Label to display for true values (boolean type) */
    trueLabel?: string;
    /** Label to display for false values (boolean type) */
    falseLabel?: string;
    /** How to display boolean values */
    booleanDisplay?: 'text' | 'checkbox' | 'icon';
    /** Header styling (bold, italic, color, etc.) */
    headerStyle?: ColumnTextStyle;
    /** Cell styling (applies to all cells in the column) */
    cellStyle?: ColumnTextStyle;
    /** Conditional formatting rules (applied in order, first match wins) */
    conditionalRules?: ColumnConditionalRule[];
 * Grid state persisted in UserView.GridState column
 * Contains column configuration, sort settings, and optional filter state
    /** Sort settings - array of field/direction pairs */
    sortSettings?: ViewGridSortSetting[];
    /** Column settings - visibility, width, order, pinning, etc. */
    columnSettings?: ViewGridColumnSetting[];
    /** Filter state (Kendo-compatible format) */
    filter?: ViewFilterInfo;
    /** Aggregate calculations and display configuration */
    aggregates?: ViewGridAggregatesConfig;
 * View display modes supported by the entity viewer
export type ViewDisplayMode = 'grid' | 'cards' | 'timeline';
export type ViewTimelineSegmentGrouping = 'none' | 'day' | 'week' | 'month' | 'quarter' | 'year';
export interface ViewTimelineState {
    segmentGrouping?: ViewTimelineSegmentGrouping;
export interface ViewCardState {
    /** Custom card size */
 * Grid-specific display configuration
export interface ViewGridDisplayState {
    defaultMode: ViewDisplayMode;
    timeline?: ViewTimelineState;
    cards?: ViewCardState;
    grid?: ViewGridDisplayState;
 * Display location for aggregate values
export type AggregateDisplayType = 'column' | 'card';
 * Value formatting options for aggregates
export interface AggregateValueFormat {
    /** Number of decimal places */
    /** Currency code (ISO 4217) for currency format, e.g., 'USD', 'EUR' */
    /** Show thousands separator */
    /** Prefix to add before value (e.g., '$') */
    /** Suffix to add after value (e.g., '%') */
    /** Date format for date aggregates */
 * Conditional styling for aggregate values
export interface AggregateConditionalStyle {
    /** Condition operator */
    operator: 'eq' | 'neq' | 'gt' | 'gte' | 'lt' | 'lte' | 'between';
    value: number | string;
    /** Second value for 'between' operator */
    value2?: number | string;
    /** Style class to apply: 'success' (green), 'warning' (yellow), 'danger' (red), 'info' (blue), 'muted' (gray) */
    style: 'success' | 'warning' | 'danger' | 'info' | 'muted';
 * Configuration for a single aggregate expression.
 * Each aggregate can optionally have a `smartPrompt` for AI-generated expressions.
export interface ViewGridAggregate {
     * Unique ID for this aggregate (for UI reference).
     * Auto-generated if not provided.
     * SQL expression to calculate.
     * If smartPrompt is set and expression is empty, the expression will be
     * auto-generated from the natural language prompt.
     * Natural language prompt for AI-generated expression.
     * When set, the server will use AI to generate/update the expression
     * based on this description.
     *   - "Total revenue"
     *   - "Average order size excluding cancelled orders"
     *   - "Count of unique customers"
     * The generated expression is stored in `expression` for caching.
     * Re-generation only happens when smartPrompt changes.
    smartPrompt?: string;
     * Display type:
     * - 'column': Show in pinned row under a specific column
     * - 'card': Show in summary card panel
    displayType: AggregateDisplayType;
     * For 'column' displayType: which column to display under.
     * Should match a field name in the grid.
    column?: string;
    /** Optional description (shown in tooltip) */
    /** Value formatting */
    format?: AggregateValueFormat;
    /** Icon for card display (Font Awesome class) */
     * Conditional styling rules (applied in order, first match wins).
     * Used for visual indicators like red/yellow/green based on value.
    conditionalStyles?: AggregateConditionalStyle[];
     * Whether this aggregate is enabled (visible).
     * Allows users to toggle without deleting configuration.
     * Sort order for display (lower = earlier).
     * Cards are sorted by this, columns use column order.
    order?: number;
 * Display settings for the aggregate panel/row
export interface ViewGridAggregateDisplay {
    /** Whether to show column-bound aggregates in pinned row */
    showColumnAggregates?: boolean;
    /** Where to show column aggregates: pinned top or bottom row */
    columnPosition?: 'top' | 'bottom';
    /** Whether to show card aggregates in a panel */
    showCardAggregates?: boolean;
    /** Where to show the card panel */
    cardPosition?: 'right' | 'bottom';
    /** Card panel width in pixels (for 'right' position) */
    cardPanelWidth?: number;
    /** Card layout style */
    cardLayout?: 'horizontal' | 'vertical' | 'grid';
    /** Number of columns for 'grid' layout */
    cardGridColumns?: number;
    /** Card panel title (optional) */
    cardPanelTitle?: string;
    /** Whether card panel is collapsible */
    cardPanelCollapsible?: boolean;
    /** Whether card panel starts collapsed */
    cardPanelStartCollapsed?: boolean;
 * Complete aggregate configuration for a view's grid state
export interface ViewGridAggregatesConfig {
    /** Display settings for aggregate panel/row */
    display?: ViewGridAggregateDisplay;
     * Aggregate expressions and their display configuration.
     * When smartPrompt is set:
     *   - If expression is empty, server generates it from the prompt
     *   - If expression exists, it's cached; regenerate only when smartPrompt changes
    expressions?: ViewGridAggregate[];
 * Default display settings for aggregates
export const DEFAULT_AGGREGATE_DISPLAY: Required<ViewGridAggregateDisplay> = {
    showColumnAggregates: true,
    columnPosition: 'bottom',
    showCardAggregates: true,
    cardPosition: 'right',
    cardPanelWidth: 280,
    cardLayout: 'vertical',
    cardGridColumns: 2,
    cardPanelTitle: 'Summary',
    cardPanelCollapsible: true,
    cardPanelStartCollapsed: false
