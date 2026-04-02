import { CollectionPermissionService, CollectionPermission, PermissionSet } from '../../services/collection-permission.service';
interface PermissionDisplay extends CollectionPermission {
    editingPermissions: PermissionSet;
    selector: 'mj-collection-share-modal',
        @if (isOpen && collection) {
                [title]="'Share: ' + collection.Name"
                                        <span class="permission-desc">View collection and artifacts</span>
                                            <span class="permission-desc">Add/remove artifacts</span>
                                    @if (availablePermissions.includes('Delete')) {
                                            <input type="checkbox" [(ngModel)]="newPermissions.canDelete">
                                            <span class="permission-desc">Delete collection</span>
                                                    @if (permission.canDelete) {
                                                            <input type="checkbox" [(ngModel)]="permission.editingPermissions.canDelete">
    styleUrls: ['./collection-share-modal.component.css']
export class CollectionShareModalComponent implements OnInit, OnChanges {
    @Input() collection: MJCollectionEntity | null = null;
    @Input() currentUserPermissions: CollectionPermission | null = null;
    newPermissions: PermissionSet = {
        canDelete: false
        private permissionService: CollectionPermissionService,
            this.updateAvailablePermissions();
        // Reload permissions when modal opens or collection/permissions change
        const collectionChanged = changes['collection'] && !changes['collection'].isFirstChange();
        const permissionsChanged = changes['currentUserPermissions'] && !changes['currentUserPermissions'].isFirstChange();
        if ((modalOpened || collectionChanged || permissionsChanged) && this.collection) {
        if (!this.collection) return;
        const perms = await this.permissionService.loadPermissions(this.collection.ID, this.currentUser);
                canEdit: p.canEdit,
                canDelete: p.canDelete
    private updateAvailablePermissions(): void {
        // User is owner if:
        // 1. OwnerID is null/undefined (backwards compatibility with old collections)
        // 2. OwnerID matches current user ID
        const isOwner = !this.collection?.OwnerID || this.collection.OwnerID === this.currentUser.ID;
        this.canModifyPermissions = isOwner || (this.currentUserPermissions?.canShare || false);
        const userPerms = this.currentUserPermissions || {
            collectionId: this.collection?.ID,
            ownerId: this.collection?.OwnerID,
        if (this.collection?.OwnerID) {
            ids.push(this.collection.OwnerID); // Owner already has all permissions
        if (!this.selectedUser || !this.collection) return;
            // User is owner if OwnerID is null (old collections) or matches current user
            const isOwner = !this.collection.OwnerID || this.collection.OwnerID === this.currentUser.ID;
            // Use cascade grant to apply permissions to all child collections
            await this.permissionService.grantPermissionCascade(
            canEdit: permission.canEdit,
            canDelete: permission.canDelete
            const isOwner = !this.collection?.OwnerID || this.collection?.OwnerID === this.currentUser.ID;
            // Use cascade update to apply changes to all child collections
            await this.permissionService.updatePermissionCascade(
                this.collection!.ID,
                permission.userId,
        if (!confirm(`Remove ${permission.userName}'s access to this collection and all its child collections?`)) {
            // Use cascade revoke to remove access from all child collections
            await this.permissionService.revokePermissionCascade(
