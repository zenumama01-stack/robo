export interface CollectionPermission {
export interface PermissionSet {
export class CollectionPermissionService {
     * Load all permissions for a collection
    async loadPermissions(collectionId: string, currentUser: UserInfo): Promise<CollectionPermission[]> {
        const result = await rv.RunView<MJCollectionPermissionEntity>({
            EntityName: 'MJ: Collection Permissions',
     * Check if user has permission for a collection
    ): Promise<CollectionPermission | null> {
            ExtraFilter: `CollectionID='${collectionId}' AND UserID='${userId}'`,
     * Check permissions for multiple collections at once (efficient bulk loading)
    async checkBulkPermissions(
        collectionIds: string[],
    ): Promise<Map<string, CollectionPermission>> {
        const resultMap = new Map<string, CollectionPermission>();
        if (collectionIds.length === 0) {
            return resultMap;
        // Build filter for all collection IDs
        const collectionFilter = collectionIds.map(id => `CollectionID='${id}'`).join(' OR ');
            ExtraFilter: `(${collectionFilter}) AND UserID='${userId}'`,
            for (const entity of result.Results) {
                const permission = this.mapToPermission(entity);
                resultMap.set(entity.CollectionID, permission);
     * Grant permission to a user
        permissions: PermissionSet,
    ): Promise<MJCollectionPermissionEntity> {
        const permission = await md.GetEntityObject<MJCollectionPermissionEntity>(
            'MJ: Collection Permissions',
        permission.CollectionID = collectionId;
        permission.CanDelete = permissions.canDelete;
     * Grant permission and cascade to all child collections
    async grantPermissionCascade(
        // Grant permission on current collection
        await this.grantPermission(collectionId, userId, permissions, sharedByUserId, currentUser);
        // Grant permissions on all child collections recursively
        await this.grantChildPermissions(collectionId, userId, permissions, sharedByUserId, currentUser);
     * Recursively grant permissions on all child collections
    private async grantChildPermissions(
        parentCollectionId: string,
        const childrenResult = await rv.RunView({
            ExtraFilter: `ParentID='${parentCollectionId}'`,
                // Check if permission already exists
                const existing = await this.checkPermission(child.ID, userId, currentUser);
                    // Permission exists, update it instead
                    await this.updatePermission(existing.id, permissions, currentUser);
                    // Grant new permission
                    await this.grantPermission(child.ID, userId, permissions, sharedByUserId, currentUser);
                // Recursively grant to grandchildren
                await this.grantChildPermissions(child.ID, userId, permissions, sharedByUserId, currentUser);
     * Update permission and cascade to all child collections
    async updatePermissionCascade(
        // Update permission on current collection
        const permission = await this.checkPermission(collectionId, userId, currentUser);
            await this.updatePermission(permission.id, permissions, currentUser);
        // Get all child collections and update recursively
        await this.updateChildPermissions(collectionId, userId, permissions, currentUser);
     * Recursively update permissions on all child collections
    private async updateChildPermissions(
                // Update permission if it exists for this user on the child collection
                const childPermission = await this.checkPermission(child.ID, userId, currentUser);
                if (childPermission) {
                    await this.updatePermission(childPermission.id, permissions, currentUser);
                // Recursively update grandchildren
                await this.updateChildPermissions(child.ID, userId, permissions, currentUser);
     * Revoke permission
     * Revoke permission and cascade to all child collections
    async revokePermissionCascade(
        // Revoke permission on current collection
            await this.revokePermission(permission.id, currentUser);
        // Revoke permissions on all child collections recursively
        await this.revokeChildPermissions(collectionId, userId, currentUser);
     * Recursively revoke permissions on all child collections
    private async revokeChildPermissions(
                // Revoke permission if it exists for this user on the child collection
                    await this.revokePermission(childPermission.id, currentUser);
                // Recursively revoke from grandchildren
                await this.revokeChildPermissions(child.ID, userId, currentUser);
        requested: PermissionSet,
        granter: PermissionSet,
        if (requested.canDelete && !granter.canDelete) return false;
    getAvailablePermissions(userPermissions: PermissionSet, isOwner: boolean): string[] {
            return ['Read', 'Share', 'Edit', 'Delete'];
        if (userPermissions.canDelete) available.push('Delete');
     * Copy all permissions from parent collection to child collection
    async copyParentPermissions(
        childCollectionId: string,
        const parentPermissions = await this.loadPermissions(parentCollectionId, currentUser);
        for (const perm of parentPermissions) {
            // Check if permission already exists for this user on the child collection
            const existing = await this.checkPermission(childCollectionId, perm.userId, currentUser);
                // Permission already exists (e.g., owner permission), skip to avoid duplicate
                console.log(`Skipping duplicate permission for user ${perm.userId} on collection ${childCollectionId}`);
            await this.grantPermission(
                childCollectionId,
                perm.userId,
                    canRead: perm.canRead,
                    canShare: perm.canShare,
                    canEdit: perm.canEdit,
                    canDelete: perm.canDelete
                perm.sharedByUserId || currentUser.ID,
     * Delete all permissions for a collection
    async deleteAllPermissions(collectionId: string, currentUser: UserInfo): Promise<void> {
        const permissions = await this.loadPermissions(collectionId, currentUser);
        for (const perm of permissions) {
            await this.revokePermission(perm.id, currentUser);
     * Create owner permission record (all permissions enabled)
    async createOwnerPermission(
        ownerId: string,
            collectionId,
                canDelete: true
            ownerId, // Owner grants to themselves
    private mapToPermission(entity: MJCollectionPermissionEntity): CollectionPermission {
            collectionId: entity.CollectionID,
            canDelete: entity.CanDelete,
