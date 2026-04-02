import { ButtonModule } from '@progress/kendo-angular-buttons';
import { ArtifactPermissionService, ArtifactPermission, ArtifactPermissionSet } from '../../services/artifact-permission.service';
import { UserPickerComponent, UserSearchResult } from '../shared/user-picker.component';
interface PermissionDisplay extends ArtifactPermission {
    isEditing: boolean;
    editingPermissions: ArtifactPermissionSet;
    selector: 'mj-artifact-share-modal',
    imports: [FormsModule, WindowModule, ButtonModule, UserPickerComponent],
        @if (isOpen && artifact) {
                [title]="'Share: ' + artifact.Name"
                <div class="share-modal-content">
                    <!-- Add User Section -->
                    <div class="add-user-section">
                            Share with User
                        <mj-user-picker
                            [excludeUserIds]="getExcludedUserIds()"
                            (userSelected)="onUserSelected($event)"
                        ></mj-user-picker>
                        @if (selectedUser) {
                            <div class="permissions-form">
                                <div class="selected-user-info">
                                        <div class="user-name">{{ selectedUser.name }}</div>
                                        <div class="user-email">{{ selectedUser.email }}</div>
                                <div class="permissions-grid">
                                    <label class="permission-checkbox disabled">
                                        <input type="checkbox" [checked]="true" disabled>
                                        <span class="permission-label">
                                        <span class="permission-desc">View artifact content</span>
                                    @if (availablePermissions.includes('Share')) {
                                        <label class="permission-checkbox">
                                            <input type="checkbox" [(ngModel)]="newPermissions.canShare">
                                            <span class="permission-desc">Share with others</span>
                                    @if (availablePermissions.includes('Edit')) {
                                            <input type="checkbox" [(ngModel)]="newPermissions.canEdit">
                                            <span class="permission-desc">Edit and delete artifact</span>
                                    <button kendoButton (click)="onAddUser()" [disabled]="!selectedUser" class="btn-primary">
                                        Add User
                                    <button kendoButton (click)="onClearSelection()" class="btn-secondary">
                    <!-- Current Permissions Section -->
                    <div class="permissions-list-section">
                            Shared With ({{ permissions.length }})
                        @if (permissions.length === 0) {
                                <i class="fa-solid fa-user-slash"></i>
                                <p>Not shared with anyone yet</p>
                            <div class="permissions-list">
                                @for (permission of permissions; track permission.id) {
                                    <div class="permission-item">
                                        <div class="permission-details">
                                            <div class="permission-user">
                                                <span class="user-name">{{ permission.userName }}</span>
                                                @if (permission.sharedByUserName) {
                                                    <span class="shared-by">shared by {{ permission.sharedByUserName }}</span>
                                            @if (!permission.isEditing) {
                                                <div class="permission-badges">
                                                    <span class="permission-badge">
                                                        <i class="fa-solid fa-eye"></i> Read
                                                    @if (permission.canShare) {
                                                            <i class="fa-solid fa-share-nodes"></i> Share
                                                    @if (permission.canEdit) {
                                                            <i class="fa-solid fa-pen-to-square"></i> Edit
                                                <div class="permissions-edit-grid">
                                                    <label class="permission-checkbox-small disabled">
                                                        <span>Read</span>
                                                        <label class="permission-checkbox-small">
                                                            <input type="checkbox" [(ngModel)]="permission.editingPermissions.canShare">
                                                            <input type="checkbox" [(ngModel)]="permission.editingPermissions.canEdit">
                                                            <span>Edit</span>
                                        @if (canModifyPermissions) {
                                            <div class="permission-actions">
                                                    <button kendoButton class="btn-icon" (click)="onEditPermission(permission)" title="Edit">
                                                    <button kendoButton class="btn-icon btn-danger" (click)="onRevokePermission(permission)" title="Remove">
                                                    <button kendoButton class="btn-icon btn-success" (click)="onSavePermission(permission)" title="Save">
                                                    <button kendoButton class="btn-icon" (click)="onCancelEdit(permission)" title="Cancel">
                <div class="modal-actions">
                    <button kendoButton (click)="onCancel()">Close</button>
    styleUrls: ['./artifact-share-modal.component.css']
export class ArtifactShareModalComponent implements OnInit, OnChanges {
    @Input() artifact: MJArtifactEntity | null = null;
    @Output() saved = new EventEmitter<void>();
    @Output() cancelled = new EventEmitter<void>();
    permissions: PermissionDisplay[] = [];
    selectedUser: UserSearchResult | null = null;
    availablePermissions: string[] = [];
    canModifyPermissions: boolean = false;
    newPermissions: ArtifactPermissionSet = {
        canRead: true,
        canShare: false,
        canEdit: false
        private permissionService: ArtifactPermissionService,
        if (this.artifact) {
            await this.loadPermissions();
            await this.updateAvailablePermissions();
        // Reload permissions when modal opens or artifact changes
        const modalOpened = changes['isOpen']?.currentValue === true && changes['isOpen']?.previousValue === false;
        const artifactChanged = changes['artifact'] && !changes['artifact'].isFirstChange();
        if ((modalOpened || artifactChanged) && this.artifact) {
    private async loadPermissions(): Promise<void> {
        if (!this.artifact) return;
        const perms = await this.permissionService.loadPermissions(this.artifact.ID, this.currentUser);
        this.permissions = perms.map(p => ({
            ...p,
            isEditing: false,
            editingPermissions: {
                canRead: p.canRead,
                canShare: p.canShare,
                canEdit: p.canEdit
    private async updateAvailablePermissions(): Promise<void> {
        // Check if current user is owner
        const isOwner = await this.permissionService.isOwner(this.artifact.ID, this.currentUser.ID, this.currentUser);
        // Check if user has share permission
        const hasSharePermission = await this.permissionService.checkPermission(
            this.artifact.ID,
            this.currentUser.ID,
            'share',
            this.currentUser
        // Allow modification if user is owner OR has Share permission
        this.canModifyPermissions = isOwner || hasSharePermission;
        // Get user's current permissions
        const userPerms: ArtifactPermissionSet = {
            canShare: hasSharePermission,
            canEdit: await this.permissionService.checkPermission(
                'edit',
        this.availablePermissions = this.permissionService.getAvailablePermissions(userPerms, isOwner);
        console.log('Share modal permissions:', {
            artifactId: this.artifact?.ID,
            userId: this.artifact?.UserID,
            currentUserId: this.currentUser.ID,
            isOwner,
            availablePermissions: this.availablePermissions
    getExcludedUserIds(): string[] {
        const ids = this.permissions.map(p => p.userId);
        ids.push(this.currentUser.ID); // Can't share with yourself
        if (this.artifact?.UserID) {
            ids.push(this.artifact.UserID); // Owner already has all permissions
        return ids;
    onUserSelected(user: UserSearchResult): void {
    onClearSelection(): void {
        this.newPermissions = {
    async onAddUser(): Promise<void> {
        if (!this.selectedUser || !this.artifact) return;
            // Check if user is owner
            // Get current user's permissions
                canShare: await this.permissionService.checkPermission(
            // Validate permissions
            if (!this.permissionService.validatePermissions(this.newPermissions, userPerms, isOwner)) {
                alert('You cannot grant permissions you do not have');
            // Grant permission
            await this.permissionService.grantPermission(
                this.selectedUser.id,
                this.newPermissions,
            this.onClearSelection();
            this.saved.emit();
            console.error('Error adding user:', error);
            alert('Failed to add user. Please try again.');
    onEditPermission(permission: PermissionDisplay): void {
        permission.isEditing = true;
    onCancelEdit(permission: PermissionDisplay): void {
        permission.isEditing = false;
        permission.editingPermissions = {
            canRead: permission.canRead,
            canShare: permission.canShare,
            canEdit: permission.canEdit
    async onSavePermission(permission: PermissionDisplay): Promise<void> {
            if (!this.permissionService.validatePermissions(permission.editingPermissions, userPerms, isOwner)) {
            await this.permissionService.updatePermission(
                permission.id,
                permission.editingPermissions,
            console.error('Error updating permission:', error);
            alert('Failed to update permissions. Please try again.');
    async onRevokePermission(permission: PermissionDisplay): Promise<void> {
        if (!confirm(`Remove ${permission.userName}'s access to this artifact?`)) {
            await this.permissionService.revokePermission(permission.id, this.currentUser);
            console.error('Error revoking permission:', error);
            alert('Failed to revoke permission. Please try again.');
        this.cancelled.emit();
