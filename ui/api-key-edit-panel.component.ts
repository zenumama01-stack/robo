import { Component, EventEmitter, HostListener, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { MJAPIKeyEntity, MJAPIScopeEntity, MJAPIKeyScopeEntity, MJAPIKeyUsageLogEntity } from '@memberjunction/core-entities';
/** Scope with selection state */
    originallySelected: boolean;
/** Usage log display item */
interface UsageLogItem {
    statusCode: number;
    responseTime: number;
    ipAddress: string;
 * Panel for viewing and editing existing API keys
    selector: 'mj-api-key-edit-panel',
    templateUrl: './api-key-edit-panel.component.html',
    styleUrls: ['./api-key-edit-panel.component.css']
export class APIKeyEditPanelComponent implements OnChanges {
    @Input() KeyId: string | null = null;
    @Output() Updated = new EventEmitter<MJAPIKeyEntity>();
    @Output() Revoked = new EventEmitter<MJAPIKeyEntity>();
    // Current key
    public APIKey: MJAPIKeyEntity | null = null;
    public IsRevoking = false;
    // Edit mode
    public IsEditing = false;
    public EditLabel = '';
    public EditExpiresAt: Date | null = null;
    public HasScopeChanges = false;
    // Usage logs
    public UsageLogs: UsageLogItem[] = [];
    public IsLoadingLogs = true;
    // Tabs
    public ActiveTab: 'details' | 'scopes' | 'usage' = 'details';
    // Revoke confirmation
    public ShowRevokeConfirm = false;
    public RevokeConfirmText = '';
    async ngOnChanges(changes: SimpleChanges): Promise<void> {
        if (changes['KeyId'] && this.KeyId) {
            await this.loadKey();
     * Handle escape key to close panel
        if (this.Visible && !this.IsSaving && !this.IsRevoking) {
     * Load the API key and related data
    private async loadKey(): Promise<void> {
        this.resetState();
            const key = await this.md.GetEntityObject<MJAPIKeyEntity>('MJ: API Keys');
            if (await key.Load(this.KeyId!)) {
                this.APIKey = key;
                this.EditLabel = key.Label;
                this.EditDescription = key.Description || '';
                this.EditExpiresAt = key.ExpiresAt;
                // Load scopes and usage in parallel
                    this.loadScopes(),
                    this.loadUsageLogs()
            console.error('Error loading API key:', error);
            this.ErrorMessage = 'Failed to load API key details';
     * Load all scopes and mark assigned ones
    private async loadScopes(): Promise<void> {
            // Get all scopes from cache, load assigned scopes from DB
            const allScopes = base.Scopes;
            const assignedScopesResult = await rv.RunView<MJAPIKeyScopeEntity>({
                ExtraFilter: `APIKeyID='${this.KeyId}'`,
            if (assignedScopesResult.Success) {
                const assignedScopeIds = new Set(
                    assignedScopesResult.Results.map(ks => ks.ScopeID)
                // Build category UI config from root scopes
                for (const scope of allScopes) {
                    const isSelected = assignedScopeIds.has(scope.ID);
                        selected: isSelected,
                        originallySelected: isSelected
                        scopes: scopes.sort((a, b) => a.scope.Name.localeCompare(b.scope.Name)),
                        expanded: scopes.some(s => s.selected),
                        allSelected: scopes.length > 0 && scopes.every(s => s.selected)
     * Load usage logs for this key
    private async loadUsageLogs(): Promise<void> {
        this.IsLoadingLogs = true;
            const result = await rv.RunView<MJAPIKeyUsageLogEntity>({
                EntityName: 'MJ: API Key Usage Logs',
                this.UsageLogs = result.Results.map(log => ({
                    timestamp: log.__mj_CreatedAt,
                    endpoint: log.Endpoint || '/unknown',
                    method: log.Method || 'GET',
                    statusCode: log.StatusCode || 200,
                    responseTime: log.ResponseTimeMs || 0,
                    ipAddress: log.IPAddress || 'Unknown'
            console.error('Error loading usage logs:', error);
            this.IsLoadingLogs = false;
     * Reset component state
    private resetState(): void {
        this.IsEditing = false;
        this.ShowRevokeConfirm = false;
        this.RevokeConfirmText = '';
        this.SuccessMessage = '';
        this.HasScopeChanges = false;
        this.ActiveTab = 'details';
     * Toggle edit mode
    public toggleEdit(): void {
        if (this.IsEditing) {
            // Cancel edit
            this.EditLabel = this.APIKey!.Label;
            this.EditDescription = this.APIKey!.Description || '';
            this.EditExpiresAt = this.APIKey!.ExpiresAt;
        this.IsEditing = !this.IsEditing;
     * Save changes to the key
        if (!this.APIKey) return;
            this.APIKey.Label = this.EditLabel.trim();
            this.APIKey.Description = this.EditDescription.trim() || null;
            this.APIKey.ExpiresAt = this.EditExpiresAt;
            const result = await this.APIKey.Save();
                this.SuccessMessage = 'API key updated successfully';
                this.Updated.emit(this.APIKey);
                this.ErrorMessage = 'Failed to save changes';
            console.error('Error saving key:', error);
     * Save scope changes
    public async saveScopeChanges(): Promise<void> {
            const allScopes = this.ScopeCategories.flatMap(cat => cat.scopes);
            // Find scopes to add and remove
            const toAdd = allScopes.filter(s => s.selected && !s.originallySelected);
            const toRemove = allScopes.filter(s => !s.selected && s.originallySelected);
            // Add new scope assignments
            for (const item of toAdd) {
                const keyScope = await this.md.GetEntityObject<MJAPIKeyScopeEntity>('MJ: API Key Scopes');
                keyScope.NewRecord();
                keyScope.APIKeyID = this.APIKey.ID;
                keyScope.ScopeID = item.scope.ID;
                await keyScope.Save();
                item.originallySelected = true;
            // Remove scope assignments
            for (const item of toRemove) {
                const result = await rv.RunView<MJAPIKeyScopeEntity>({
                    ExtraFilter: `APIKeyID='${this.APIKey.ID}' AND ScopeID='${item.scope.ID}'`,
                    await result.Results[0].Delete();
                item.originallySelected = false;
            this.SuccessMessage = 'Permissions updated successfully';
            console.error('Error saving scopes:', error);
            this.ErrorMessage = 'Failed to update permissions';
     * Check for scope changes and update category allSelected states
    public onScopeChange(): void {
        // Update allSelected state for each category
            category.allSelected = category.scopes.length > 0 && category.scopes.every(s => s.selected);
        // Track overall changes
        this.HasScopeChanges = allScopes.some(s => s.selected !== s.originallySelected);
     * Toggle all scopes in a category on/off
        // Toggle to opposite of current allSelected state
        // Apply to all scopes in category
            item.selected = newState;
        // Update change tracking
        this.onScopeChange();
     * Start revoke flow
    public startRevoke(): void {
        this.ShowRevokeConfirm = true;
     * Cancel revoke
    public cancelRevoke(): void {
     * Confirm and execute revoke using GraphQL client
    public async confirmRevoke(): Promise<void> {
        if (!this.APIKey || this.RevokeConfirmText !== 'REVOKE') return;
        this.IsRevoking = true;
            const result = await encryptionClient.RevokeAPIKey(this.APIKey.ID);
                // Update local state to reflect the change
                this.APIKey.Status = 'Revoked';
                this.SuccessMessage = 'API key has been revoked';
                this.Revoked.emit(this.APIKey);
                this.ErrorMessage = result.Error || 'Failed to revoke key';
            console.error('Error revoking key:', error);
            this.ErrorMessage = 'An error occurred while revoking';
            this.IsRevoking = false;
    public formatDate(date: Date | null): string {
     * Format relative time
    public formatRelativeTime(date: Date): string {
        const diff = now.getTime() - new Date(date).getTime();
        const minutes = Math.floor(diff / 60000);
        const hours = Math.floor(diff / 3600000);
        const days = Math.floor(diff / 86400000);
        if (minutes < 1) return 'Just now';
        if (minutes < 60) return `${minutes}m ago`;
        if (hours < 24) return `${hours}h ago`;
        if (days < 7) return `${days}d ago`;
     * Get status class for HTTP status code
    public getStatusClass(statusCode: number): string {
        if (statusCode >= 200 && statusCode < 300) return 'status-success';
        if (statusCode >= 400 && statusCode < 500) return 'status-warning';
        if (statusCode >= 500) return 'status-error';
     * Get assigned scope count
    public getAssignedScopeCount(): number {
     * Get selected scope count for a category (for template use)
    public getSelectedCount(category: ScopeCategory): number {
     * Close the panel
