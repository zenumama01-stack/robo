import { Metadata, RoleInfo, RunView } from '@memberjunction/core';
import { MJAIAgentPermissionEntity, MJUserEntity } from '@memberjunction/core-entities';
 * Enriched permission row for display in the permissions panel.
 * Includes resolved names and computed effective permissions.
export interface PermissionRow {
    GrantedToName: string;
    GrantType: 'user' | 'role';
    EffectiveCanView: boolean;
    EffectiveCanRun: boolean;
    EffectiveCanEdit: boolean;
    EffectiveCanDelete: boolean;
    Comments: string | null;
    /** The underlying entity object for save/delete operations */
    Entity: MJAIAgentPermissionEntity;
 * Service for loading and managing AI Agent permission data.
 * Decoupled from UI components so it can be reused across
 * panel, dialog, and slideover variants.
export class AgentPermissionsService {
    private users: MJUserEntity[] = [];
    private roles: RoleInfo[] = [];
    private permissions: MJAIAgentPermissionEntity[] = [];
    public get Users(): MJUserEntity[] { return this.users; }
    public get Roles(): RoleInfo[] { return this.roles; }
    public get Permissions(): MJAIAgentPermissionEntity[] { return this.permissions; }
     * Load all data needed for the permissions panel.
     * Fetches permissions, users, and roles in parallel.
    public async LoadAll(agentId: string): Promise<PermissionRow[]> {
        const [perms] = await Promise.all([
            this.loadPermissions(agentId),
        this.permissions = perms;
        return this.buildRows();
     * Reload just the permissions for the given agent.
    public async RefreshPermissions(agentId: string): Promise<PermissionRow[]> {
        this.permissions = await this.loadPermissions(agentId);
     * Save a new or existing permission record.
    public async SavePermission(
        grantType: 'user' | 'role',
        granteeId: string,
        canView: boolean,
        canRun: boolean,
        canEdit: boolean,
        canDelete: boolean,
        comments: string | null,
        existingEntity?: MJAIAgentPermissionEntity
        const entity = existingEntity ||
            await md.GetEntityObject<MJAIAgentPermissionEntity>('MJ: AI Agent Permissions');
        entity.AgentID = agent.ID;
        entity.UserID = grantType === 'user' ? granteeId : null;
        entity.RoleID = grantType === 'role' ? granteeId : null;
        entity.CanView = canView;
        entity.CanRun = canRun;
        entity.CanEdit = canEdit;
        entity.CanDelete = canDelete;
        entity.Comments = comments;
        return entity.Save();
     * Delete a permission record.
    public async DeletePermission(entity: MJAIAgentPermissionEntity): Promise<boolean> {
        return entity.Delete();
     * Resolve the owner name for a given user ID.
    public async ResolveOwnerName(ownerUserId: string | null): Promise<string> {
        if (!ownerUserId) return 'Not Set';
        const cached = this.users.find(u => u.ID === ownerUserId);
        if (cached) return cached.Name;
        const userEntity = await md.GetEntityObject<MJUserEntity>('MJ: Users');
        const loaded = await userEntity.Load(ownerUserId);
        return loaded ? userEntity.Name : 'Unknown';
    private async loadPermissions(agentId: string): Promise<MJAIAgentPermissionEntity[]> {
        const result = await rv.RunView<MJAIAgentPermissionEntity>({
    private async loadUsers(): Promise<void> {
            this.users = result.Results;
    private async loadRoles(): Promise<void> {
        this.roles = md.Roles;
    private buildRows(): PermissionRow[] {
        return this.permissions.map(p => {
            const grantType: 'user' | 'role' = p.UserID ? 'user' : 'role';
            const grantedToName = p.UserID
                ? this.users.find(u => u.ID === p.UserID)?.Name || 'Unknown User'
                : this.roles.find(r => r.ID === p.RoleID)?.Name || 'Unknown Role';
            const effectiveCanDelete = p.CanDelete || false;
            const effectiveCanEdit = effectiveCanDelete || (p.CanEdit || false);
            const effectiveCanRun = effectiveCanEdit || (p.CanRun || false);
            const effectiveCanView = effectiveCanRun || (p.CanView || false);
                ID: p.ID,
                AgentID: p.AgentID,
                UserID: p.UserID,
                RoleID: p.RoleID,
                GrantedToName: grantedToName,
                GrantType: grantType,
                CanView: p.CanView || false,
                CanRun: p.CanRun || false,
                CanEdit: p.CanEdit || false,
                CanDelete: p.CanDelete || false,
                EffectiveCanView: effectiveCanView,
                EffectiveCanRun: effectiveCanRun,
                EffectiveCanEdit: effectiveCanEdit,
                EffectiveCanDelete: effectiveCanDelete,
                Comments: p.Comments || null,
                Entity: p
