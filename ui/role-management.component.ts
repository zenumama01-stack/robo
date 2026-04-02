import { ResourceData, MJRoleEntity } from '@memberjunction/core-entities';
import { RoleDialogData, RoleDialogResult } from './role-dialog/role-dialog.component';
interface RoleStats {
  totalRoles: number;
  systemRoles: number;
  customRoles: number;
  activeRoles: number;
  type: 'all' | 'system' | 'custom';
  selector: 'mj-role-management',
  templateUrl: './role-management.component.html',
  styleUrls: ['./role-management.component.css']
@RegisterClass(BaseDashboard, 'RoleManagement')
export class RoleManagementComponent extends BaseDashboard implements OnDestroy {
  public filteredRoles: MJRoleEntity[] = [];
  public selectedRole: MJRoleEntity | null = null;
  public showRoleDialog = false;
  public roleDialogData: RoleDialogData | null = null;
  public stats: RoleStats = {
    totalRoles: 0,
    systemRoles: 0,
    customRoles: 0,
    activeRoles: 0
    type: 'all',
  public showCreateDialog = false;
  public showEditDialog = false;
  public expandedRoleId: string | null = null;
  // Role permissions (simplified view)
  public rolePermissions: Map<string, string[]> = new Map();
    return "Role Management"
      // Load roles
      const roles = await this.loadRoles();
      console.error('Error loading role data:', error);
      this.error = 'Failed to load role data. Please try again.';
    let filtered = [...this.roles];
    if (filters.type !== 'all') {
      filtered = filtered.filter(role => {
        const isSystem = this.isSystemRole(role);
        return filters.type === 'system' ? isSystem : !isSystem;
      filtered = filtered.filter(role =>
        role.Name?.toLowerCase().includes(searchLower) ||
        role.Description?.toLowerCase().includes(searchLower)
    this.filteredRoles = filtered;
    const systemRoles = this.roles.filter(r => this.isSystemRole(r));
      totalRoles: this.roles.length,
      systemRoles: systemRoles.length,
      customRoles: this.roles.length - systemRoles.length,
      activeRoles: this.roles.length // All roles are considered active for now
  public isSystemRole(role: MJRoleEntity): boolean {
    // System roles typically have certain naming patterns or flags
    const systemRoleNames = ['Administrator', 'User', 'Guest', 'Developer'];
    return systemRoleNames.includes(role.Name || '');
  public onTypeFilterChange(type: 'all' | 'system' | 'custom'): void {
    this.updateFilter({ type });
  public toggleRoleExpansion(roleId: string): void {
    this.expandedRoleId = this.expandedRoleId === roleId ? null : roleId;
  public isRoleExpanded(roleId: string): boolean {
    return this.expandedRoleId === roleId;
  public createNewRole(): void {
    this.roleDialogData = {
    this.showRoleDialog = true;
  public editRole(role: MJRoleEntity): void {
  public confirmDeleteRole(role: MJRoleEntity): void {
    this.selectedRole = role;
  public async deleteRole(): Promise<void> {
    if (!this.selectedRole) return;
      // Load role entity to delete
      const role = await this.metadata.GetEntityObject<MJRoleEntity>('MJ: Roles');
      const loadResult = await role.Load(this.selectedRole.ID);
      if (loadResult) {
        const deleteResult = await role.Delete();
          this.selectedRole = null;
          throw new Error(role.LatestResult?.Message || 'Failed to delete role');
        throw new Error('Role not found or permission denied');
      console.error('Error deleting role:', error);
        this.error = error instanceof Error ? error.message : 'Failed to delete role';
  public getRoleIcon(role: MJRoleEntity): string {
    if (this.isSystemRole(role)) {
      return 'fa-shield-halved';
    return 'fa-user-tag';
  public getRoleTypeLabel(role: MJRoleEntity): string {
    return this.isSystemRole(role) ? 'System' : 'Custom';
  public getRoleTypeClass(role: MJRoleEntity): string {
    return this.isSystemRole(role) ? 'badge-system' : 'badge-custom';
  public onRoleDialogResult(result: RoleDialogResult): void {
    this.showRoleDialog = false;
    this.roleDialogData = null;
      // Refresh the role list to show changes
