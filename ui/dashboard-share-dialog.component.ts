import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges, ChangeDetectorRef, ViewEncapsulation } from '@angular/core';
import { MJDashboardEntity, MJDashboardPermissionEntity, DashboardEngine, MJUserEntity } from '@memberjunction/core-entities';
 * Represents a user share with their permissions
export interface UserSharePermission {
    /** The permission entity (may be new or existing) */
    Permission: MJDashboardPermissionEntity;
    /** The user being shared with */
    User: MJUserEntity;
    /** Whether this is a newly added share */
    IsNew: boolean;
    /** Whether this share has been marked for removal */
    MarkedForRemoval: boolean;
 * Result emitted when the dialog is closed
export interface ShareDialogResult {
    /** The action taken: 'save' or 'cancel' */
    Action: 'save' | 'cancel';
    /** The dashboard that was shared (only on save) */
    Dashboard?: MJDashboardEntity;
 * Dialog component for sharing dashboards with other users.
 * Allows setting granular permissions (CanRead, CanEdit, CanDelete, CanShare).
    selector: 'mj-dashboard-share-dialog',
    templateUrl: './dashboard-share-dialog.component.html',
    styleUrls: ['./dashboard-share-dialog.component.css'],
    encapsulation: ViewEncapsulation.None
export class DashboardShareDialogComponent implements OnChanges {
    @Input() Dashboard: MJDashboardEntity | null = null;
    @Output() Result = new EventEmitter<ShareDialogResult>();
    /** Current shares for this dashboard */
    public UserShares: UserSharePermission[] = [];
    /** All available users for sharing (excludes owner and already shared) */
    public AvailableUsers: MJUserEntity[] = [];
    /** All users in the system */
    private allUsers: MJUserEntity[] = [];
    public IsLoading = false;
    /** Error message to display */
    public Error: string | null = null;
    /** Search filter for available users */
    public UserSearchFilter = '';
        if (changes['Visible'] && this.Visible && this.Dashboard) {
            this.resetDialog();
     * Reset dialog state
    private resetDialog(): void {
        this.Error = null;
        this.UserShares = [];
        this.AvailableUsers = [];
        this.UserSearchFilter = '';
     * Load existing shares and available users
        if (!this.Dashboard) return;
            // Load all users
            const usersResult = await rv.RunView<MJUserEntity>({
            if (usersResult.Success) {
                this.allUsers = usersResult.Results;
            // Get existing shares from DashboardEngine
            const existingShares = DashboardEngine.Instance.GetDashboardShares(this.Dashboard.ID);
            // Build user shares list
            for (const permission of existingShares) {
                const user = this.allUsers.find(u => u.ID === permission.UserID);
                    this.UserShares.push({
                        Permission: permission,
                        IsNew: false,
                        MarkedForRemoval: false
            // Update available users
            this.updateAvailableUsers();
            console.error('Error loading share data:', error);
            this.Error = 'Failed to load sharing data. Please try again.';
     * Update the list of available users (excludes owner and already shared)
    private updateAvailableUsers(): void {
        const sharedUserIds = new Set(
            this.UserShares
                .filter(s => !s.MarkedForRemoval)
                .map(s => s.User.ID)
        // Exclude dashboard owner and already shared users
        this.AvailableUsers = this.allUsers.filter(user =>
            user.ID !== this.Dashboard!.UserID && !sharedUserIds.has(user.ID)
     * Get filtered available users based on search
    public get FilteredAvailableUsers(): MJUserEntity[] {
        if (!this.UserSearchFilter.trim()) {
            return this.AvailableUsers.slice(0, 10); // Show first 10 by default
        const filter = this.UserSearchFilter.toLowerCase();
        return this.AvailableUsers
            .filter(user =>
                user.Name.toLowerCase().includes(filter) ||
                (user.Email && user.Email.toLowerCase().includes(filter))
     * Check if there are unsaved changes
    public get HasChanges(): boolean {
        return this.UserShares.some(s => s.IsNew || s.MarkedForRemoval || s.Permission.Dirty);
     * Add a user to the share list
    public async addUserShare(user: MJUserEntity): Promise<void> {
        const permission = await md.GetEntityObject<MJDashboardPermissionEntity>('MJ: Dashboard Permissions');
        permission.NewRecord();
        permission.DashboardID = this.Dashboard.ID;
        permission.UserID = user.ID;
        permission.CanRead = true;  // Default to read access
        permission.CanEdit = false;
        permission.CanDelete = false;
        permission.CanShare = false;
            IsNew: true,
     * Mark a share for removal
    public removeUserShare(share: UserSharePermission): void {
        if (share.IsNew) {
            // If it's new, just remove from the array
            this.UserShares = this.UserShares.filter(s => s !== share);
            // Otherwise mark for removal
            share.MarkedForRemoval = true;
     * Undo removal of a share
    public undoRemove(share: UserSharePermission): void {
        share.MarkedForRemoval = false;
     * Get shares that are not marked for removal
    public get ActiveShares(): UserSharePermission[] {
        return this.UserShares.filter(s => !s.MarkedForRemoval);
     * Get shares marked for removal
    public get RemovedShares(): UserSharePermission[] {
        return this.UserShares.filter(s => s.MarkedForRemoval);
     * Toggle CanEdit permission (also enables CanRead)
    public toggleCanEdit(share: UserSharePermission): void {
        share.Permission.CanEdit = !share.Permission.CanEdit;
        if (share.Permission.CanEdit) {
            share.Permission.CanRead = true;
     * Toggle CanDelete permission (also enables CanRead and CanEdit)
    public toggleCanDelete(share: UserSharePermission): void {
        share.Permission.CanDelete = !share.Permission.CanDelete;
        if (share.Permission.CanDelete) {
            share.Permission.CanEdit = true;
     * Toggle CanShare permission (also enables CanRead)
    public toggleCanShare(share: UserSharePermission): void {
        share.Permission.CanShare = !share.Permission.CanShare;
        if (share.Permission.CanShare) {
     * Save all permission changes
        if (!this.HasChanges) {
            this.onCancel();
            // Process removals first
            for (const share of this.UserShares.filter(s => s.MarkedForRemoval && !s.IsNew)) {
                const deleted = await share.Permission.Delete();
                    throw new Error(`Failed to remove share for ${share.User.Name}`);
            // Process new and modified shares
            for (const share of this.UserShares.filter(s => !s.MarkedForRemoval && (s.IsNew || s.Permission.Dirty))) {
                const saved = await share.Permission.Save();
                    throw new Error(`Failed to save share for ${share.User.Name}: ${share.Permission.LatestResult?.Message}`);
            // Refresh the engine cache
            await DashboardEngine.Instance.Config(true);
            this.Result.emit({
                Action: 'save',
                Dashboard: this.Dashboard!
            console.error('Error saving shares:', error);
            this.Error = error instanceof Error ? error.message : 'Failed to save sharing settings.';
     * Cancel and close the dialog
    public onCancel(): void {
        this.Result.emit({ Action: 'cancel' });
     * Get initials for a user (for avatar display)
    public getUserInitials(user: MJUserEntity): string {
        const name = user.Name || user.Email || '?';
        const parts = name.split(' ');
        if (parts.length >= 2) {
            return (parts[0][0] + parts[1][0]).toUpperCase();
        return name.substring(0, 2).toUpperCase();
