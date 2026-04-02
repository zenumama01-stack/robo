import { LogError, UserInfo } from "@memberjunction/core";
import { MJAIAgentEntity } from "@memberjunction/core-entities";
import { AIEngineBase } from "./BaseAIEngine";
 * Effective permissions for a user on a specific AI agent
export interface EffectiveAgentPermissions {
    canView: boolean;
    canRun: boolean;
    canEdit: boolean;
    canDelete: boolean;
    isOwner: boolean;
 * Helper class for managing AI Agent permissions.
 * Provides methods to check user permissions, get accessible agents, and uses AIEngineBase cached metadata.
export class AIAgentPermissionHelper {
     * Checks if a user has a specific permission for an agent.
     * @param agentId - The ID of the agent to check
     * @param user - The user to check permissions for
     * @param permission - The permission level to check ('view', 'run', 'edit', or 'delete')
     * @returns True if the user has the specified permission
    public static async HasPermission(
        user: UserInfo,
        permission: 'view' | 'run' | 'edit' | 'delete'
        const effective = await this.GetEffectivePermissions(agentId, user);
        switch (permission) {
            case 'view':
                return effective.canView;
            case 'run':
                return effective.canRun;
            case 'edit':
                return effective.canEdit;
                return effective.canDelete;
     * Gets the aggregate permissions for a user on a specific agent.
     * Implements hierarchical permission logic:
     * - Delete implies Edit, Run, and View
     * - Edit implies Run and View
     * - Run implies View
     * Default behavior when no permission records exist:
     * - Anyone can View and Run (open by default)
     * - Only owner can Edit and Delete
     * Checks ownership first, then combines all matching user and role permissions.
     * @param agentId - The ID of the agent
     * @returns Object containing all effective permissions and ownership status
    public static async GetEffectivePermissions(
        user: UserInfo
    ): Promise<EffectiveAgentPermissions> {
            // Ensure AIEngineBase is loaded with cached metadata
            await AIEngineBase.Instance.Config(false, user);
            // Find agent from cached metadata
            const agent = AIEngineBase.Instance.Agents.find(a => a.ID === agentId);
                throw new Error(`Agent ${agentId} not found in cached metadata`);
            // Check if user is owner - owners have all permissions
            const isOwner = agent.OwnerUserID === user.ID;
            if (isOwner) {
                    canView: true,
                    canRun: true,
                    canEdit: true,
                    canDelete: true,
                    isOwner: true
            // Get permissions from cached metadata
            const agentPermissions = AIEngineBase.Instance.AgentPermissions.filter(
                p => p.AgentID === agentId
            // DEFAULT BEHAVIOR: If no permission records exist, grant View and Run to everyone
            // This minimizes administrative overhead while protecting Edit/Delete through ownership
            if (agentPermissions.length === 0) {
                    canEdit: false,  // Only owner can edit
                    canDelete: false, // Only owner can delete
                    isOwner: false
            // Get user's role IDs
            const userRoleIds = user.UserRoles?.map(ur => ur.RoleID) || [];
            // Find all matching permissions (direct user permissions or role permissions)
            const matchingPermissions = agentPermissions.filter(p =>
                p.UserID === user.ID || (p.RoleID && userRoleIds.includes(p.RoleID))
            // Aggregate base permissions - any matching permission grants access (OR logic)
            const hasDelete = matchingPermissions.some(p => p.CanDelete === true);
            const hasEdit = matchingPermissions.some(p => p.CanEdit === true);
            const hasRun = matchingPermissions.some(p => p.CanRun === true);
            const hasView = matchingPermissions.some(p => p.CanView === true);
            // Apply hierarchical logic:
            // Delete → Edit → Run → View
            const canDelete = hasDelete;
            const canEdit = hasDelete || hasEdit;
            const canRun = hasDelete || hasEdit || hasRun;
            const canView = hasDelete || hasEdit || hasRun || hasView;
                canView,
                canRun,
                canEdit,
                canDelete,
            LogError(error, 'Error getting effective permissions for agent');
            // Fail closed - deny all permissions on error
                canView: false,
                canRun: false,
                canEdit: false,
                canDelete: false,
     * Gets all agents that a user has access to with a specific permission level.
     * Uses cached metadata from AIEngineBase instead of querying database.
     * @param permission - The minimum permission level required ('view', 'run', 'edit', or 'delete')
     * @returns Array of agents the user can access with the specified permission
    public static async GetAccessibleAgents(
    ): Promise<MJAIAgentEntity[]> {
            // Filter agents from cached metadata based on permissions
            const accessibleAgents: MJAIAgentEntity[] = [];
            for (const agent of AIEngineBase.Instance.Agents) {
                const hasAccess = await this.HasPermission(agent.ID, user, permission);
                if (hasAccess) {
                    accessibleAgents.push(agent);
            return accessibleAgents;
            LogError(error, 'Error getting accessible agents');
     * Clears the AIEngineBase cache. Useful when permissions have been modified.
     * This forces a reload of all cached metadata including permissions.
    public static ClearCache(): void {
        // Force reload by calling Config with forceRefresh=true
        AIEngineBase.Instance.Config(true).catch(error => {
            LogError(error, 'Error clearing AIEngineBase cache');
     * Refreshes the AIEngineBase cache to pick up permission changes.
     * @param user - The user context for server-side operations
    public static async RefreshCache(user: UserInfo): Promise<void> {
            await AIEngineBase.Instance.Config(true, user);
            LogError(error, 'Error refreshing AIEngineBase cache');
