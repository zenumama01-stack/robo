import { ResourceData, MJCredentialEntity, MJCredentialTypeEntity } from '@memberjunction/core-entities';
import { CredentialEditPanelComponent } from '@memberjunction/ng-credentials';
type ViewMode = 'grid' | 'list';
type StatusFilter = '' | 'active' | 'inactive' | 'expired' | 'expiring';
@RegisterClass(BaseResourceComponent, 'CredentialsListResource')
    selector: 'mj-credentials-list-resource',
    templateUrl: './credentials-list-resource.component.html',
    styleUrls: ['./credentials-list-resource.component.css'],
export class CredentialsListResourceComponent extends BaseResourceComponent implements OnInit, OnDestroy {
    public credentials: MJCredentialEntity[] = [];
    public filteredCredentials: MJCredentialEntity[] = [];
    public viewMode: ViewMode = 'grid';
    public selectedTypeFilter = '';
    public selectedStatusFilter: StatusFilter = '';
    public showActiveOnly = false;
    // Selection for bulk operations
    public selectedCredentials = new Set<string>();
    private _isAllSelected = false;
    @ViewChild('editPanel') editPanel!: CredentialEditPanelComponent;
        return 'Credentials';
        return this.checkEntityPermission('MJ: Credentials', 'Create');
        return this.checkEntityPermission('MJ: Credentials', 'Update');
        return this.checkEntityPermission('MJ: Credentials', 'Delete');
            const [credResult, typeResult] = await rv.RunViews([
                this.credentials = credResult.Results as MJCredentialEntity[];
            console.error('Error loading credentials:', error);
            MJNotificationService.Instance.CreateSimpleNotification('Error loading credentials', 'error', 3000);
            // Handle navigation params from Data.Configuration (passed via NavigationService)
            this.handleNavigationConfig();
    // === Navigation Handling ===
    private handleNavigationConfig(): void {
        const config = this.Data?.Configuration;
        // Apply filters from navigation config
        if (config.typeId) {
            this.selectedTypeFilter = config.typeId as string;
        if (config.openCreatePanel) {
            // Open create panel (optionally with type/category pre-selected)
                if (config.categoryId) {
                    this.createNewCredentialWithType(config.typeId as string, config.categoryId as string);
                } else if (config.typeId) {
                    this.createNewCredentialWithType(config.typeId as string);
                    this.createNewCredential();
    public createNewCredential(): void {
        if (this.editPanel) {
            this.editPanel.open(null);
    public createNewCredentialWithType(typeId?: string, categoryId?: string): void {
            this.editPanel.open(null, typeId, categoryId);
    public editCredential(credential: MJCredentialEntity, event?: Event): void {
            this.editPanel.open(credential);
    public onCredentialSaved(credential: MJCredentialEntity): void {
        // Check if it's a new credential or update
        const existingIndex = this.credentials.findIndex(c => c.ID === credential.ID);
            this.credentials[existingIndex] = credential;
            // Add new
            this.credentials.unshift(credential);
    public onCredentialDeleted(credentialId: string): void {
        this.credentials = this.credentials.filter(c => c.ID !== credentialId);
        this.selectedCredentials.delete(credentialId);
    public async deleteCredential(credential: MJCredentialEntity, event?: Event): Promise<void> {
            MJNotificationService.Instance.CreateSimpleNotification('You do not have permission to delete credentials', 'warning', 3000);
        const confirmed = confirm(`Are you sure you want to delete "${credential.Name}"? This action cannot be undone.`);
            const success = await credential.Delete();
                MJNotificationService.Instance.CreateSimpleNotification(`Credential "${credential.Name}" deleted successfully`, 'success', 3000);
                this.credentials = this.credentials.filter(c => c.ID !== credential.ID);
                this.selectedCredentials.delete(credential.ID);
                MJNotificationService.Instance.CreateSimpleNotification('Failed to delete credential', 'error', 3000);
            console.error('Error deleting credential:', error);
            MJNotificationService.Instance.CreateSimpleNotification('Error deleting credential', 'error', 3000);
    public async toggleCredentialActive(credential: MJCredentialEntity, event?: Event): Promise<void> {
        if (!this.UserCanUpdate) {
            MJNotificationService.Instance.CreateSimpleNotification('You do not have permission to update credentials', 'warning', 3000);
            credential.IsActive = !credential.IsActive;
            const success = await credential.Save();
                const status = credential.IsActive ? 'activated' : 'deactivated';
                MJNotificationService.Instance.CreateSimpleNotification(`Credential "${credential.Name}" ${status}`, 'success', 2000);
                MJNotificationService.Instance.CreateSimpleNotification('Failed to update credential', 'error', 3000);
            console.error('Error updating credential:', error);
            MJNotificationService.Instance.CreateSimpleNotification('Error updating credential', 'error', 3000);
    public toggleSelection(credential: MJCredentialEntity, event?: Event): void {
        if (this.selectedCredentials.has(credential.ID)) {
            this.selectedCredentials.add(credential.ID);
        this.updateAllSelectedState();
        if (this._isAllSelected) {
            this.selectedCredentials.clear();
            this.filteredCredentials.forEach(c => this.selectedCredentials.add(c.ID));
        this._isAllSelected = !this._isAllSelected;
    public isAllSelected(): boolean {
        return this._isAllSelected;
    public isSelected(credential: MJCredentialEntity): boolean {
        return this.selectedCredentials.has(credential.ID);
    public clearSelection(): void {
        this._isAllSelected = false;
    private updateAllSelectedState(): void {
        this._isAllSelected = this.filteredCredentials.length > 0 &&
            this.filteredCredentials.every(c => this.selectedCredentials.has(c.ID));
    public async deleteSelected(): Promise<void> {
        if (!this.UserCanDelete || this.selectedCredentials.size === 0) return;
        const count = this.selectedCredentials.size;
        const confirmed = confirm(`Are you sure you want to delete ${count} credential(s)? This action cannot be undone.`);
        let failCount = 0;
        for (const credId of this.selectedCredentials) {
            const credential = this.credentials.find(c => c.ID === credId);
            if (credential) {
                        this.credentials = this.credentials.filter(c => c.ID !== credId);
                        failCount++;
        if (successCount > 0) {
                `${successCount} credential(s) deleted${failCount > 0 ? `, ${failCount} failed` : ''}`,
                failCount > 0 ? 'warning' : 'success',
            MJNotificationService.Instance.CreateSimpleNotification('Failed to delete credentials', 'error', 3000);
    // === Filtering ===
    public onTypeFilterChange(typeId: string): void {
        this.selectedTypeFilter = typeId;
    public onStatusFilterChange(status: StatusFilter): void {
        this.selectedStatusFilter = status;
    public onActiveFilterChange(showActive: boolean): void {
        this.showActiveOnly = showActive;
        this.selectedTypeFilter = '';
        this.selectedStatusFilter = '';
        this.showActiveOnly = false;
        return this.searchText !== '' ||
            this.selectedTypeFilter !== '' ||
            this.selectedStatusFilter !== '' ||
            this.showActiveOnly;
        let filtered = [...this.credentials];
        const thirtyDaysFromNow = new Date();
        thirtyDaysFromNow.setDate(thirtyDaysFromNow.getDate() + 30);
        // Filter by active status
        if (this.showActiveOnly) {
            filtered = filtered.filter(c => c.IsActive);
        // Filter by status
        if (this.selectedStatusFilter) {
                const statusClass = this.getStatusClass(c);
                switch (this.selectedStatusFilter) {
                    case 'active': return statusClass === 'active';
                    case 'inactive': return statusClass === 'inactive';
                    case 'expired': return statusClass === 'expired';
                    case 'expiring': return statusClass === 'expiring';
                    default: return true;
        // Filter by type
        if (this.selectedTypeFilter) {
            filtered = filtered.filter(c => c.CredentialTypeID === this.selectedTypeFilter);
        // Filter by search text
        if (this.searchText.trim()) {
            const search = this.searchText.toLowerCase().trim();
            filtered = filtered.filter(c =>
                c.Name.toLowerCase().includes(search) ||
                (c.Description && c.Description.toLowerCase().includes(search)) ||
                (c.CredentialType && c.CredentialType.toLowerCase().includes(search))
        this.filteredCredentials = filtered;
    // === View Mode ===
    public setViewMode(mode: ViewMode): void {
    public getTypeById(typeId: string): MJCredentialTypeEntity | undefined {
        return this.types.find(t => t.ID === typeId);
    public getTypesByCategory(): Map<string, MJCredentialTypeEntity[]> {
        const grouped = new Map<string, MJCredentialTypeEntity[]>();
        for (const type of this.types) {
            const category = type.Category;
            if (!grouped.has(category)) {
                grouped.set(category, []);
            grouped.get(category)!.push(type);
    public getStatusClass(credential: MJCredentialEntity): string {
            return 'inactive';
        if (credential.ExpiresAt) {
            const expiresAt = new Date(credential.ExpiresAt);
            const thirtyDays = 30 * 24 * 60 * 60 * 1000;
            if (expiresAt < now) {
            if (expiresAt.getTime() - now.getTime() < thirtyDays) {
                return 'expiring';
    public getStatusLabel(credential: MJCredentialEntity): string {
        const statusClass = this.getStatusClass(credential);
        const labels: Record<string, string> = {
            'active': 'Active',
            'inactive': 'Inactive',
            'expired': 'Expired',
            'expiring': 'Expiring Soon'
        return labels[statusClass] || 'Unknown';
    public getStatusIcon(credential: MJCredentialEntity): string {
            'active': 'fa-solid fa-check-circle',
            'inactive': 'fa-solid fa-minus-circle',
            'expired': 'fa-solid fa-times-circle',
            'expiring': 'fa-solid fa-clock'
        return icons[statusClass] || 'fa-solid fa-circle';
    public formatDateTime(date: Date | null | undefined): string {
    public getTimeAgo(date: Date | null | undefined): string {
        const then = new Date(date);
        const diffMs = now.getTime() - then.getTime();
        if (diffDays < 30) return `${diffDays}d ago`;
        return this.formatDate(date);
    public getTypeIcon(credential: MJCredentialEntity): string {
        const type = this.getTypeById(credential.CredentialTypeID);
        if (!type) return 'fa-solid fa-key';
            'AI': 'fa-solid fa-brain',
            'Communication': 'fa-solid fa-envelope',
            'Storage': 'fa-solid fa-cloud',
            'Database': 'fa-solid fa-database',
            'Authentication': 'fa-solid fa-shield-halved',
            'Integration': 'fa-solid fa-plug'
        return iconMap[type.Category] || 'fa-solid fa-key';
    // === Stats ===
    public get activeCount(): number {
        return this.credentials.filter(c => c.IsActive).length;
        return this.activeCount;
    public get expiringCount(): number {
        return this.credentials.filter(c =>
            c.ExpiresAt &&
            new Date(c.ExpiresAt) >= now &&
            new Date(c.ExpiresAt) <= thirtyDaysFromNow &&
            c.IsActive
    public getExpiringSoonCount(): number {
        return this.expiringCount;
    public get expiredCount(): number {
            c.ExpiresAt && new Date(c.ExpiresAt) < now
    public getExpiredCount(): number {
        return this.expiredCount;
    // === Status Helpers ===
    public isExpired(credential: MJCredentialEntity): boolean {
        if (!credential.ExpiresAt) return false;
        return new Date(credential.ExpiresAt) < new Date();
    public isExpiringSoon(credential: MJCredentialEntity): boolean {
        return expiresAt >= now && expiresAt <= thirtyDaysFromNow;
    // === Bulk Operations ===
    public async bulkToggleActive(active: boolean): Promise<void> {
        if (!this.UserCanUpdate || this.selectedCredentials.size === 0) return;
            if (credential && credential.IsActive !== active) {
                    credential.IsActive = active;
                        credential.IsActive = !active;
        this.clearSelection();
        const action = active ? 'activated' : 'deactivated';
                `${successCount} credential(s) ${action}${failCount > 0 ? `, ${failCount} failed` : ''}`,
        } else if (failCount > 0) {
            MJNotificationService.Instance.CreateSimpleNotification(`Failed to ${action.slice(0, -1)} credentials`, 'error', 3000);
    public async bulkDelete(): Promise<void> {
