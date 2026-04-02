import { MJArtifactPermissionEntity, MJArtifactEntity, MJCollectionArtifactEntity } from '@memberjunction/core-entities';
import { CollectionPermissionService } from './collection-permission.service';
export interface ArtifactPermission {
    canShare: boolean;
    sharedByUserId: string | null;
    sharedByUserName: string | null;
    sharedAt: Date;
export interface EffectivePermission extends ArtifactPermission {
    source: 'owner' | 'explicit' | 'collection';
    collectionName?: string; // If inherited from collection
export interface ArtifactPermissionSet {
export class ArtifactPermissionService {
        private collectionPermissionService: CollectionPermissionService
     * Load all explicit permissions for an artifact
    async loadPermissions(artifactId: string, currentUser: UserInfo): Promise<ArtifactPermission[]> {
        const result = await rv.RunView<MJArtifactPermissionEntity>({
            EntityName: 'MJ: Artifact Permissions',
            return result.Results.map(p => this.mapToPermission(p));
     * Check if user has specific permission for an artifact (HYBRID CHECK)
     * Checks in order: Owner > Explicit Permission > Collection Inheritance
    async checkPermission(
        permission: 'read' | 'edit' | 'share',
        // 1. Check ownership - owner has all permissions
        const artifact = await this.getArtifact(artifactId, currentUser);
        if (artifact && artifact.UserID === userId) {
        // 2. Check explicit artifact permission
        const explicit = await this.getExplicitPermission(artifactId, userId, currentUser);
        if (explicit) {
            return this.hasPermission(explicit, permission);
        // 3. Check collection permission inheritance
        const collections = await this.getArtifactCollections(artifactId, currentUser);
        for (const collection of collections) {
            const collectionPermission = await this.collectionPermissionService.checkPermission(
                collection.CollectionID,
                currentUser
            if (collectionPermission && this.hasCollectionPermission(collectionPermission, permission)) {
                return true; // Inherited from collection
        // 4. No access
     * Get explicit permission record for a user on an artifact
    async getExplicitPermission(
    ): Promise<ArtifactPermission | null> {
            ExtraFilter: `ArtifactID='${artifactId}' AND UserID='${userId}'`,
            return this.mapToPermission(result.Results[0]);
     * Get all effective permissions for an artifact (owner + explicit + inherited)
    async getEffectiveUsers(artifactId: string, currentUser: UserInfo): Promise<EffectivePermission[]> {
        const effectivePermissions: EffectivePermission[] = [];
        const seenUsers = new Set<string>();
        // 1. Add owner
        if (artifact && artifact.UserID) {
            effectivePermissions.push({
                id: '', // No permission record for owner
                userId: artifact.UserID,
                userName: artifact.User || 'Owner',
                canShare: true,
                sharedByUserId: null,
                sharedByUserName: null,
                sharedAt: artifact.__mj_CreatedAt,
                source: 'owner'
            seenUsers.add(artifact.UserID);
        // 2. Add explicit permissions
        const explicitPerms = await this.loadPermissions(artifactId, currentUser);
        for (const perm of explicitPerms) {
            if (!seenUsers.has(perm.userId)) {
                    ...perm,
                    source: 'explicit'
                seenUsers.add(perm.userId);
        // 3. Add collection-inherited permissions
            const collectionPerms = await this.collectionPermissionService.loadPermissions(
            for (const collPerm of collectionPerms) {
                if (!seenUsers.has(collPerm.userId)) {
                        userId: collPerm.userId,
                        userName: collPerm.userName,
                        canRead: collPerm.canRead,
                        canEdit: collPerm.canEdit,
                        canShare: collPerm.canShare,
                        sharedByUserId: collPerm.sharedByUserId,
                        sharedByUserName: collPerm.sharedByUserName,
                        sharedAt: collPerm.sharedAt,
                        source: 'collection',
                        collectionName: collection.Collection || 'Unknown Collection'
                    seenUsers.add(collPerm.userId);
        return effectivePermissions;
     * Grant explicit permission to a user
    async grantPermission(
        permissions: ArtifactPermissionSet,
        sharedByUserId: string,
    ): Promise<MJArtifactPermissionEntity> {
        const permission = await md.GetEntityObject<MJArtifactPermissionEntity>(
            'MJ: Artifact Permissions',
        permission.ArtifactID = artifactId;
        permission.UserID = userId;
        permission.CanRead = permissions.canRead;
        permission.CanEdit = permissions.canEdit;
        permission.CanShare = permissions.canShare;
        permission.SharedByUserID = sharedByUserId;
        const saved = await permission.Save();
            throw new Error(permission.LatestResult?.Message || 'Failed to grant permission');
        return permission;
     * Update existing permission
    async updatePermission(
        permissionId: string,
        await permission.Load(permissionId);
        return await permission.Save();
     * Revoke explicit permission
    async revokePermission(permissionId: string, currentUser: UserInfo): Promise<boolean> {
        return await permission.Delete();
     * Validate that requested permissions don't exceed granter's permissions
    validatePermissions(
        requested: ArtifactPermissionSet,
        granter: ArtifactPermissionSet,
        isOwner: boolean
        if (isOwner) return true; // Owner can grant anything
        // Can't grant permissions you don't have
        if (requested.canEdit && !granter.canEdit) return false;
        if (requested.canShare && !granter.canShare) return false;
     * Get available permissions for a user to grant based on their own permissions
    getAvailablePermissions(userPermissions: ArtifactPermissionSet, isOwner: boolean): string[] {
            return ['Read', 'Edit', 'Share'];
        const available = ['Read']; // Always have read
        if (userPermissions.canEdit) available.push('Edit');
        if (userPermissions.canShare) available.push('Share');
        return available;
     * Check if user is owner of artifact
    async isOwner(artifactId: string, userId: string, currentUser: UserInfo): Promise<boolean> {
        return artifact ? artifact.UserID === userId : false;
     * Get all permissions for current user on an artifact (convenience method for UI)
    async getUserPermissions(artifactId: string, currentUser: UserInfo): Promise<ArtifactPermissionSet> {
        const [canRead, canEdit, canShare] = await Promise.all([
            this.checkPermission(artifactId, currentUser.ID, 'read', currentUser),
            this.checkPermission(artifactId, currentUser.ID, 'edit', currentUser),
            this.checkPermission(artifactId, currentUser.ID, 'share', currentUser)
        return { canRead, canEdit, canShare };
     * Helper: Get artifact record
    private async getArtifact(artifactId: string, currentUser: UserInfo): Promise<MJArtifactEntity | null> {
            ExtraFilter: `ID='${artifactId}'`,
     * Helper: Get all collections containing this artifact
    private async getArtifactCollections(
    ): Promise<MJCollectionArtifactEntity[]> {
                SELECT ID FROM [__mj].[vwArtifactVersions] WHERE ArtifactID='${artifactId}'
        return result.Success && result.Results ? result.Results : [];
     * Helper: Check if permission set has specific permission
    private hasPermission(permission: ArtifactPermission, type: 'read' | 'edit' | 'share'): boolean {
                return permission.canRead;
                return permission.canEdit;
            case 'share':
                return permission.canShare;
     * Helper: Check if collection permission has specific permission
    private hasCollectionPermission(
        permission: { canRead: boolean; canEdit: boolean; canShare: boolean },
        type: 'read' | 'edit' | 'share'
    private mapToPermission(entity: MJArtifactPermissionEntity): ArtifactPermission {
            id: entity.ID,
            artifactId: entity.ArtifactID,
            userId: entity.UserID,
            userName: entity.User || '',
            canRead: entity.CanRead,
            canEdit: entity.CanEdit,
            canShare: entity.CanShare,
            sharedByUserId: entity.SharedByUserID || null,
            sharedByUserName: entity.SharedByUser || null,
            sharedAt: entity.__mj_CreatedAt
