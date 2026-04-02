import { RoleInfo } from '@memberjunction/core';
import { AgentPermissionsService, PermissionRow } from '../services/agent-permissions.service';
/** Simple name/ID pair for the grantee search-select */
interface GranteeOption {
    selector: 'mj-agent-permissions-panel',
    templateUrl: './agent-permissions-panel.component.html',
    styleUrls: ['./agent-permissions-panel.component.css'],
        trigger('slideDown', [
                style({ opacity: 0, transform: 'translateY(-8px)' }),
                animate('200ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
                animate('150ms ease-in', style({ opacity: 0, transform: 'translateY(-8px)' }))
    providers: [AgentPermissionsService]
export class AgentPermissionsPanelComponent implements OnInit {
    /** The agent whose permissions are being managed */
    private _agent: AIAgentEntityExtended | null = null;
    set Agent(value: AIAgentEntityExtended | null) {
        const prev = this._agent;
        this._agent = value;
        if (value && value !== prev) {
    get Agent(): AIAgentEntityExtended | null {
        return this._agent;
    /** Emitted after a permission is saved or deleted */
    // --- View state ---
    public Rows: PermissionRow[] = [];
    public OwnerName: string | null = null;
    // --- Form state ---
    public IsFormOpen = false;
    public EditingRowId: string | null = null;
    public FormGrantType: 'user' | 'role' = 'user';
    public FormGranteeId: string | null = null;
    public FormCanView = false;
    public FormCanRun = false;
    public FormCanEdit = false;
    public FormCanDelete = false;
    public FormComments = '';
    public SearchQuery = '';
    public FilteredGrantees: GranteeOption[] = [];
        private permsSvc: AgentPermissionsService,
        if (this._agent) {
        if (!this._agent) return;
            this.Rows = await this.permsSvc.LoadAll(this._agent.ID);
            this.OwnerName = await this.permsSvc.ResolveOwnerName(this._agent.OwnerUserID || null);
    // List interactions
    public trackById(_index: number, row: PermissionRow): string {
        return row.ID;
    public OnAddNew(): void {
        this.IsFormOpen = true;
        this.updateFilteredGrantees();
    public OnEditRow(row: PermissionRow): void {
        this.EditingRowId = row.ID;
        this.FormGrantType = row.GrantType;
        this.FormGranteeId = row.GrantType === 'user' ? row.UserID : row.RoleID;
        this.FormCanView = row.CanView;
        this.FormCanRun = row.CanRun;
        this.FormCanEdit = row.CanEdit;
        this.FormCanDelete = row.CanDelete;
        this.FormComments = row.Comments || '';
        // Set search query to the grantee name
        this.SearchQuery = row.GrantedToName;
    public async OnDeleteRow(row: PermissionRow): Promise<void> {
        const deleted = await this.permsSvc.DeletePermission(row.Entity);
            this.Rows = await this.permsSvc.RefreshPermissions(this._agent.ID);
            this.PermissionsChanged.emit();
    // Form interactions
    public OnCancelForm(): void {
    public OnSearchChanged(): void {
    public OnPermToggle(level: 'run' | 'edit' | 'delete'): void {
        if (level === 'delete' && this.FormCanDelete) {
            this.FormCanEdit = true;
            this.FormCanRun = true;
            this.FormCanView = true;
        } else if (level === 'edit' && this.FormCanEdit) {
        } else if (level === 'run' && this.FormCanRun) {
        if (!this.FormGranteeId) return false;
        return this.FormCanView || this.FormCanRun || this.FormCanEdit || this.FormCanDelete;
    public async OnSave(): Promise<void> {
        if (!this._agent || !this.FormGranteeId || this.IsSaving) return;
            const existingEntity = this.EditingRowId
                ? this.Rows.find(r => r.ID === this.EditingRowId)?.Entity
            const saved = await this.permsSvc.SavePermission(
                this._agent,
                this.FormGrantType,
                this.FormGranteeId,
                this.FormCanView,
                this.FormCanRun,
                this.FormCanEdit,
                this.FormCanDelete,
                this.FormComments || null,
                existingEntity
    // Helpers
        this.IsFormOpen = false;
        this.EditingRowId = null;
        this.FormGrantType = 'user';
        this.FormGranteeId = null;
        this.FormCanView = false;
        this.FormCanRun = false;
        this.FormCanEdit = false;
        this.FormCanDelete = false;
        this.FormComments = '';
        this.FilteredGrantees = [];
    private updateFilteredGrantees(): void {
        const source: GranteeOption[] = this.FormGrantType === 'user'
            ? this.permsSvc.Users.map((u: MJUserEntity) => ({ ID: u.ID, Name: u.Name }))
            : this.permsSvc.Roles.map((r: RoleInfo) => ({ ID: r.ID, Name: r.Name }));
        this.FilteredGrantees = query
            ? source.filter(g => g.Name.toLowerCase().includes(query))
            : source;
        // Limit displayed results to prevent overwhelming the UI
        if (this.FilteredGrantees.length > 20) {
            this.FilteredGrantees = this.FilteredGrantees.slice(0, 20);
