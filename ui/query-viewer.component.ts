import { RunQuery, RunQueryParams, Metadata, QueryInfo } from '@memberjunction/core';
import { QueryDataGridComponent } from '../query-data-grid/query-data-grid.component';
    QueryParameterValues,
    getQueryGridStateKey,
    getQueryParamsKey
} from '../query-data-grid/models/query-grid-types';
 * A composite component that provides a complete query viewing experience.
 * - Automatic parameter form display when query has parameters
 * - Grid state persistence to User Settings
 * - Parameter persistence to User Settings
 * - Auto-run capability when all required params have saved values
 * <mj-query-viewer
 *   [QueryId]="selectedQueryId"
 *   [AutoRun]="true"
 *   (EntityLinkClick)="openRecord($event)">
 * </mj-query-viewer>
    selector: 'mj-query-viewer',
    templateUrl: './query-viewer.component.html',
    styleUrls: ['./query-viewer.component.css'],
export class QueryViewerComponent implements OnInit, OnDestroy {
    private _queryId: string | null = null;
     * The ID of the query to display
    set QueryId(value: string | null) {
        const previous = this._queryId;
        this._queryId = value;
            this.onQueryIdChanged();
    get QueryId(): string | null {
        return this._queryId;
     * Whether to auto-run the query when all required params have saved values
    @Input() AutoRun: boolean = true;
    @Input() SelectionMode: QueryGridSelectionMode = 'single';
     * Visual configuration for the grid
     * Whether to persist grid state
     * Whether to persist parameter values
    @Input() PersistParameters: boolean = true;
     * Fired when an entity link is clicked in the grid
     * Fired when query execution starts
    @Output() QueryStart = new EventEmitter<void>();
     * Fired when query execution completes
    @Output() QueryComplete = new EventEmitter<RunQueryResult>();
     * Fired when query execution fails
    @Output() QueryError = new EventEmitter<Error>();
     * Fired when user wants to open the full query record
    @Output() OpenQueryRecord = new EventEmitter<{ queryId: string; queryName: string }>();
    @ViewChild(QueryDataGridComponent) DataGrid!: QueryDataGridComponent;
    public QueryInfo: QueryInfo | null = null;
    public QueryData: Record<string, unknown>[] = [];
    public IsLoading: boolean = false;
    public ShowParamsPanel: boolean = false;
    public ShowInfoPanel: boolean = false;
    public HasRun: boolean = false;
    public LastError: string | null = null;
    public ExecutionTimeMs: number | null = null;
    public SavedGridState: QueryGridState | null = null;
    public SavedParams: QueryParameterValues = {};
    private userInfoEngine = UserInfoEngine.Instance;
    // Query Loading
    private async onQueryIdChanged(): Promise<void> {
        this.QueryInfo = null;
        this.QueryData = [];
        this.HasRun = false;
        this.LastError = null;
        this.ExecutionTimeMs = null;
        this.SavedGridState = null;
        this.SavedParams = {};
        this.ShowParamsPanel = false;
        if (!this._queryId) {
        // Load query info from metadata
        this.QueryInfo = this.metadata.Queries.find(q => q.ID === this._queryId) || null;
            this.LastError = `Query with ID ${this._queryId} not found`;
        // Load saved state from User Settings
        // Determine if we should show params or auto-run
        const hasParams = this.QueryInfo.Parameters && this.QueryInfo.Parameters.length > 0;
        const hasRequiredParams = hasParams && this.QueryInfo.Parameters.some(p => p.IsRequired);
        if (hasParams) {
            // Check if all required params have saved values
            const canAutoRun = this.AutoRun && this.canAutoRunWithSavedParams();
            if (canAutoRun) {
                // Auto-run with saved parameters
                await this.RunQuery(this.SavedParams);
                // Show parameter panel
                this.ShowParamsPanel = true;
            // No parameters - auto-run immediately
            await this.RunQuery({});
    private canAutoRunWithSavedParams(): boolean {
        if (!this.QueryInfo) return false;
        const requiredParams = this.QueryInfo.Parameters?.filter(p => p.IsRequired) || [];
        for (const param of requiredParams) {
            const savedValue = this.SavedParams[param.Name];
            if (savedValue === null || savedValue === undefined || savedValue === '') {
            if (param.Type === 'array' && Array.isArray(savedValue) && savedValue.length === 0) {
        if (!this._queryId) return;
            // Load grid state
            if (this.PersistState) {
                const gridStateKey = getQueryGridStateKey(this._queryId);
                const gridStateSetting = await this.getUserSetting(gridStateKey);
                if (gridStateSetting) {
                    this.SavedGridState = JSON.parse(gridStateSetting) as QueryGridState;
            // Load saved parameters
            if (this.PersistParameters) {
                const paramsKey = getQueryParamsKey(this._queryId);
                const paramsSetting = await this.getUserSetting(paramsKey);
                if (paramsSetting) {
                    this.SavedParams = JSON.parse(paramsSetting) as QueryParameterValues;
            console.warn('Error loading saved query state:', error);
    private async saveGridState(state: QueryGridState): Promise<void> {
        if (!this._queryId || !this.PersistState) return;
            const key = getQueryGridStateKey(this._queryId);
            await this.setUserSetting(key, JSON.stringify(state));
            console.warn('Error saving grid state:', error);
    private async saveParameters(params: QueryParameterValues): Promise<void> {
        if (!this._queryId || !this.PersistParameters) return;
            const key = getQueryParamsKey(this._queryId);
            await this.setUserSetting(key, JSON.stringify(params));
            console.warn('Error saving parameters:', error);
    private async getUserSetting(key: string): Promise<string | undefined> {
        return this.userInfoEngine.GetSetting(key);
    private async setUserSetting(key: string, value: string): Promise<void> {
        await this.userInfoEngine.SetSetting(key, value);
    // Query Execution
    public async RunQuery(params: QueryParameterValues): Promise<void> {
        if (!this.QueryInfo || !this._queryId) return;
        this.QueryStart.emit();
        // Save parameters for next time
        this.SavedParams = params;
        await this.saveParameters(params);
            const runQuery = new RunQuery();
            const runParams: RunQueryParams = {
                QueryID: this._queryId,
                Parameters: params as Record<string, unknown>
            const result = await runQuery.RunQuery(runParams);
            this.ExecutionTimeMs = Math.round(performance.now() - startTime);
                this.QueryData = result.Results || [];
                this.HasRun = true;
                this.QueryComplete.emit(result);
                this.LastError = result.ErrorMessage || 'Query execution failed';
                this.QueryError.emit(new Error(this.LastError));
                    this.LastError,
            this.LastError = errorMessage;
            this.QueryError.emit(error instanceof Error ? error : new Error(errorMessage));
                `Error running query: ${errorMessage}`,
    public OnParametersSubmit(params: QueryParameterValues): void {
        this.RunQuery(params);
    public OnParamsPanelClose(): void {
    public OnGridStateChange(event: QueryGridStateChangedEvent): void {
        this.saveGridState(event.state);
    public OnEntityLinkClick(event: QueryEntityLinkClickEvent): void {
        this.EntityLinkClick.emit(event);
    public OnRowDoubleClick(event: QueryRowClickEvent): void {
        this.RowDoubleClick.emit(event);
    public OnSelectionChange(event: QuerySelectionChangedEvent): void {
        this.SelectionChange.emit(event);
    public OnRefreshRequest(): void {
        if (this.HasRun) {
            this.RunQuery(this.SavedParams);
        } else if (this.QueryInfo?.Parameters?.length) {
            this.RunQuery({});
    public OpenParametersPanel(): void {
    public OpenInfoPanel(): void {
        this.ShowInfoPanel = true;
    public CloseInfoPanel(): void {
        this.ShowInfoPanel = false;
    public OnOpenQueryRecord(event: { queryId: string; queryName: string }): void {
        this.OpenQueryRecord.emit(event);
        this.OnRefreshRequest();
    public get HasParameters(): boolean {
        return (this.QueryInfo?.Parameters?.length || 0) > 0;
