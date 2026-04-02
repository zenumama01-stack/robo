 * Unit tests for AIAgentPermissionHelper
 * Tests permission hierarchy logic, owner checks, and default behavior.
// Create a mock AIEngineBase Instance
const mockAgents: Array<{ ID: string; OwnerUserID: string }> = [];
const mockAgentPermissions: Array<{
    AgentID: string;
    RoleID: string | null;
    CanView: boolean;
    CanRun: boolean;
    CanEdit: boolean;
    CanDelete: boolean;
    UserInfo: class {
        ID: string = '';
        UserRoles: Array<{ RoleID: string }> = [];
    MJAIAgentEntity: class {},
vi.mock('../BaseAIEngine', () => ({
    AIEngineBase: {
            get Agents() { return mockAgents; },
            get AgentPermissions() { return mockAgentPermissions; },
import { AIAgentPermissionHelper, EffectiveAgentPermissions } from '../AIAgentPermissionHelper';
function createUser(id: string, roleIds: string[] = []): { ID: string; UserRoles: Array<{ RoleID: string }> } {
        UserRoles: roleIds.map(rid => ({ RoleID: rid })),
describe('AIAgentPermissionHelper', () => {
        mockAgents.length = 0;
        mockAgentPermissions.length = 0;
    describe('GetEffectivePermissions', () => {
        it('should grant all permissions to owner', async () => {
            mockAgents.push({ ID: 'agent-1', OwnerUserID: 'user-1' });
            const user = createUser('user-1');
            const perms = await AIAgentPermissionHelper.GetEffectivePermissions('agent-1', user as never);
            expect(perms).toEqual({
                isOwner: true,
        it('should grant view and run by default when no permission records exist', async () => {
            mockAgents.push({ ID: 'agent-1', OwnerUserID: 'owner-1' });
            const user = createUser('user-2');
            expect(perms.canView).toBe(true);
            expect(perms.canRun).toBe(true);
            expect(perms.canEdit).toBe(false);
            expect(perms.canDelete).toBe(false);
            expect(perms.isOwner).toBe(false);
        it('should deny all permissions when agent not found', async () => {
            const perms = await AIAgentPermissionHelper.GetEffectivePermissions('non-existent', user as never);
            expect(perms.canView).toBe(false);
            expect(perms.canRun).toBe(false);
        it('should apply hierarchical logic: delete implies edit, run, view', async () => {
            mockAgentPermissions.push({
                UserID: 'user-2',
                RoleID: null,
                CanView: false,
                CanRun: false,
                CanEdit: false,
                CanDelete: true,
            expect(perms.canDelete).toBe(true);
            expect(perms.canEdit).toBe(true);
        it('should apply hierarchical logic: edit implies run and view', async () => {
                CanEdit: true,
                CanDelete: false,
        it('should apply hierarchical logic: run implies view', async () => {
                CanRun: true,
        it('should grant view-only permissions', async () => {
                CanView: true,
        it('should match permissions by role', async () => {
                RoleID: 'role-admin',
            const user = createUser('user-2', ['role-admin']);
        it('should combine user and role permissions with OR logic', async () => {
            // User permission: view only
            // Role permission: run only
                RoleID: 'role-runner',
            const user = createUser('user-2', ['role-runner']);
            // Combined: view (user) + run (role) + hierarchical (run->view)
        it('should not match permissions for a different agent', async () => {
                AgentID: 'agent-2',  // Different agent
            // Agent-1 has no permissions for user-2, so default behavior applies
            // With permission records existing (for agent-2), agent-1 has none matching
            // But we only filter by agentId, so agent-2 permissions aren't matched
            // Default behavior: open view/run
        it('should deny all when no matching user or role permissions exist for the agent', async () => {
                UserID: 'other-user',
            // Permissions exist for agent-1 but not for user-2, so no match found
            // No matching permissions = all false
    describe('HasPermission', () => {
        it('should return true for owner with any permission', async () => {
            expect(await AIAgentPermissionHelper.HasPermission('agent-1', user as never, 'view')).toBe(true);
            expect(await AIAgentPermissionHelper.HasPermission('agent-1', user as never, 'run')).toBe(true);
            expect(await AIAgentPermissionHelper.HasPermission('agent-1', user as never, 'edit')).toBe(true);
            expect(await AIAgentPermissionHelper.HasPermission('agent-1', user as never, 'delete')).toBe(true);
        it('should return false for delete when user only has edit permission', async () => {
            expect(await AIAgentPermissionHelper.HasPermission('agent-1', user as never, 'delete')).toBe(false);
