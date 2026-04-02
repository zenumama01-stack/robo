import { MJUserEntity, MJRoleEntity, MJUserRoleEntity, ResourceData } from '@memberjunction/core-entities';
import { UserDialogData, UserDialogResult } from './user-dialog/user-dialog.component';
interface UserStats {
  totalUsers: number;
  activeUsers: number;
  inactiveUsers: number;
  adminUsers: number;
  selector: 'mj-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
@RegisterClass(BaseDashboard, 'UserManagement')
export class UserManagementComponent extends BaseDashboard implements OnDestroy {
  public users: MJUserEntity[] = [];
  public filteredUsers: MJUserEntity[] = [];
  public selectedUser: MJUserEntity | null = null;
  // Selection state for bulk actions
  public selectedUserIds = new Set<string>();
  public showUserDialog = false;
  public userDialogData: UserDialogData | null = null;
  // Bulk action dialog state
  public showBulkActionConfirm = false;
  public bulkActionType: 'enable' | 'disable' | 'delete' | null = null;
  public showBulkRoleAssign = false;
  public bulkRoleId: string = '';
  public stats: UserStats = {
    totalUsers: 0,
    activeUsers: 0,
    inactiveUsers: 0,
    adminUsers: 0  // This will be based on roles, not Type
    role: '',
  // Mobile expansion state
  private expandedUserIds = new Set<string>();
  // User-Role mapping
  private userRoleMap = new Map<string, string[]>(); // userId -> roleIds[]
  // Grid configuration
  public gridConfig = {
    pageSize: 20,
    sortField: 'Name',
    return "User Management"
      // Load users, roles, and user-role relationships in parallel
      const [users, roles, userRoles] = await Promise.all([
        this.loadUsers(),
        this.loadRoles(),
        this.loadUserRoles()
      this.users = users;
      // Build user-role mapping
      this.buildUserRoleMapping(userRoles);
      console.error('Error loading user data:', error);
      this.error = 'Failed to load user data. Please try again.';
  private async loadUsers(): Promise<MJUserEntity[]> {
    const result = await rv.RunView<MJUserEntity>({
  private async loadUserRoles(): Promise<MJUserRoleEntity[]> {
    const result = await rv.RunView<MJUserRoleEntity>({
      EntityName: 'MJ: User Roles',
  private buildUserRoleMapping(userRoles: MJUserRoleEntity[]): void {
    this.userRoleMap.clear();
    userRoles.forEach(userRole => {
      const userId = userRole.UserID;
      const roleId = userRole.RoleID;
      if (!this.userRoleMap.has(userId)) {
        this.userRoleMap.set(userId, []);
      this.userRoleMap.get(userId)!.push(roleId);
    let filtered = [...this.users];
    if (filters.status !== 'all') {
      filtered = filtered.filter(user => 
        filters.status === 'active' ? user.IsActive : !user.IsActive
    if (filters.role) {
      filtered = filtered.filter(user => {
        const userRoles = this.userRoleMap.get(user.ID) || [];
        return userRoles.includes(filters.role);
        user.Name?.toLowerCase().includes(searchLower) ||
        user.Email?.toLowerCase().includes(searchLower) ||
        user.FirstName?.toLowerCase().includes(searchLower) ||
        user.LastName?.toLowerCase().includes(searchLower)
    this.filteredUsers = filtered;
      totalUsers: this.users.length,
      activeUsers: this.users.filter(u => u.IsActive).length,
      inactiveUsers: this.users.filter(u => !u.IsActive).length,
      adminUsers: this.users.filter(u => u.Type === 'Owner').length  // Using Owner as admin type
  public onRoleFilterChange(role: string): void {
    this.updateFilter({ role });
  public selectUser(user: MJUserEntity): void {
    this.selectedUser = user;
    this.showEditDialog = true;
  public createNewUser(): void {
    this.userDialogData = {
      mode: 'create',
      availableRoles: this.roles
    this.showUserDialog = true;
  public editUser(user: MJUserEntity): void {
      user: user,
      mode: 'edit',
  public confirmDeleteUser(user: MJUserEntity): void {
  public async deleteUser(): Promise<void> {
    if (!this.selectedUser) return;
      // Load user entity to delete
      const user = await this.metadata.GetEntityObject<MJUserEntity>('MJ: Users');
      const loadResult = await user.Load(this.selectedUser.ID);
        const deleteResult = await user.Delete();
          this.selectedUser = null;
          throw new Error(user.LatestResult?.Message || 'Failed to delete user');
        throw new Error('User not found or permission denied');
      console.error('Error deleting user:', error);
        this.error = error instanceof Error ? error.message : 'Failed to delete user';
  public async toggleUserStatus(user: MJUserEntity): Promise<void> {
      user.IsActive = !user.IsActive;
      await user.Save();
      console.error('Error updating user status:', error);
        user.IsActive = !user.IsActive; // Revert on error
  public exportUsers(): void {
    if (this.filteredUsers.length === 0) {
      this.error = 'No users to export';
      const headers = ['Name', 'First Name', 'Last Name', 'Email', 'Type', 'Status', 'Created', 'Updated'];
      const csvRows = [headers.join(',')];
      // Add user data
      this.filteredUsers.forEach(user => {
          this.escapeCSV(user.Name || ''),
          this.escapeCSV(user.FirstName || ''),
          this.escapeCSV(user.LastName || ''),
          this.escapeCSV(user.Email || ''),
          this.escapeCSV(user.Type || ''),
          user.IsActive ? 'Active' : 'Inactive',
          user.__mj_CreatedAt ? new Date(user.__mj_CreatedAt).toLocaleDateString() : '',
          user.__mj_UpdatedAt ? new Date(user.__mj_UpdatedAt).toLocaleDateString() : ''
        csvRows.push(row.join(','));
      const csvContent = csvRows.join('\n');
      if (link.download !== undefined) {
        link.setAttribute('href', url);
        link.setAttribute('download', `users_export_${new Date().toISOString().split('T')[0]}.csv`);
        link.style.visibility = 'hidden';
      console.error('Error exporting users:', error);
      this.error = 'Failed to export users';
  private escapeCSV(value: string): string {
    if (value.includes(',') || value.includes('"') || value.includes('\n')) {
  public getStatusIcon(user: MJUserEntity): string {
    return user.IsActive ? 'fa-check-circle' : 'fa-times-circle';
  public getStatusClass(user: MJUserEntity): string {
    return user.IsActive ? 'status-active' : 'status-inactive';
  public getUserTypeIcon(user: MJUserEntity): string {
    switch (user.Type) {
      case 'Owner':
    const first = user.FirstName?.charAt(0) || '';
    const last = user.LastName?.charAt(0) || '';
    return (first + last).toUpperCase() || user.Name?.charAt(0).toUpperCase() || 'U';
  public onUserDialogResult(result: UserDialogResult): void {
    this.showUserDialog = false;
    this.userDialogData = null;
      // Refresh the user list to show changes
  // Selection methods for bulk actions
  public get isAllSelected(): boolean {
    return this.filteredUsers.length > 0 &&
           this.filteredUsers.every(user => this.selectedUserIds.has(user.ID));
  public get isIndeterminate(): boolean {
    const selectedCount = this.filteredUsers.filter(user => this.selectedUserIds.has(user.ID)).length;
    return selectedCount > 0 && selectedCount < this.filteredUsers.length;
  public get hasSelection(): boolean {
    return this.selectedUserIds.size > 0;
  public get selectedCount(): number {
    return this.selectedUserIds.size;
    return filters.status !== 'all' || filters.role !== '';
  public get activeFilterCount(): number {
    if (filters.status !== 'all') count++;
    if (filters.role !== '') count++;
      role: ''
    this.showMobileFilters = false;
    if (this.isAllSelected) {
      // Deselect all filtered users
      this.filteredUsers.forEach(user => this.selectedUserIds.delete(user.ID));
      // Select all filtered users
      this.filteredUsers.forEach(user => this.selectedUserIds.add(user.ID));
  public toggleUserSelection(userId: string, event?: Event): void {
    if (this.selectedUserIds.has(userId)) {
      this.selectedUserIds.delete(userId);
      this.selectedUserIds.add(userId);
  public isUserSelected(userId: string): boolean {
    return this.selectedUserIds.has(userId);
    this.selectedUserIds.clear();
  // Bulk action methods
  public confirmBulkAction(action: 'enable' | 'disable' | 'delete'): void {
    if (!this.hasSelection) return;
    this.bulkActionType = action;
    this.showBulkActionConfirm = true;
  public cancelBulkAction(): void {
    this.showBulkActionConfirm = false;
    this.bulkActionType = null;
  public async executeBulkAction(): Promise<void> {
    if (!this.bulkActionType || !this.hasSelection) return;
      const selectedUsers = this.users.filter(user => this.selectedUserIds.has(user.ID));
      switch (this.bulkActionType) {
          await this.bulkSetUserStatus(selectedUsers, true);
        case 'disable':
          await this.bulkSetUserStatus(selectedUsers, false);
          await this.bulkDeleteUsers(selectedUsers);
      console.error('Bulk action failed:', error);
        this.error = error instanceof Error ? error.message : 'Bulk action failed';
  private async bulkSetUserStatus(users: MJUserEntity[], isActive: boolean): Promise<void> {
      user.IsActive = isActive;
      const result = await user.Save();
        throw new Error(`Failed to update user ${user.Name}: ${user.LatestResult?.Message}`);
  private async bulkDeleteUsers(users: MJUserEntity[]): Promise<void> {
      const result = await user.Delete();
        throw new Error(`Failed to delete user ${user.Name}: ${user.LatestResult?.Message}`);
  // Bulk role assignment
  public openBulkRoleAssign(): void {
    this.bulkRoleId = '';
    this.showBulkRoleAssign = true;
  public cancelBulkRoleAssign(): void {
    this.showBulkRoleAssign = false;
  public async executeBulkRoleAssign(): Promise<void> {
    if (!this.bulkRoleId || !this.hasSelection) return;
      const selectedUserIds = Array.from(this.selectedUserIds);
      for (const userId of selectedUserIds) {
        // Check if user already has this role
        const existingRoles = this.userRoleMap.get(userId) || [];
        if (!existingRoles.includes(this.bulkRoleId)) {
          const userRole = await this.metadata.GetEntityObject<MJUserRoleEntity>('MJ: User Roles');
          userRole.NewRecord();
          userRole.UserID = userId;
          userRole.RoleID = this.bulkRoleId;
          const result = await userRole.Save();
            console.warn(`Failed to assign role to user ${userId}:`, userRole.LatestResult?.Message);
      console.error('Bulk role assignment failed:', error);
        this.error = error instanceof Error ? error.message : 'Bulk role assignment failed';
  public getBulkActionMessage(): string {
    const count = this.selectedCount;
        return `Are you sure you want to enable ${count} user${count > 1 ? 's' : ''}?`;
        return `Are you sure you want to disable ${count} user${count > 1 ? 's' : ''}?`;
        return `Are you sure you want to delete ${count} user${count > 1 ? 's' : ''}? This action cannot be undone.`;
  public getBulkActionTitle(): string {
        return 'Enable Users';
        return 'Disable Users';
        return 'Delete Users';
        return 'Confirm Action';
  public getBulkActionButtonText(): string {
        return `Enable ${count} User${count > 1 ? 's' : ''}`;
        return `Disable ${count} User${count > 1 ? 's' : ''}`;
        return `Delete ${count} User${count > 1 ? 's' : ''}`;
        return 'Confirm';
  // Expansion methods
  public toggleUserExpansion(userId: string): void {
    if (this.expandedUserIds.has(userId)) {
      this.expandedUserIds.delete(userId);
      this.expandedUserIds.add(userId);
  public isUserExpanded(userId: string): boolean {
    return this.expandedUserIds.has(userId);
  // Get roles for a specific user
  public getUserRoles(userId: string): MJRoleEntity[] {
    const roleIds = this.userRoleMap.get(userId) || [];
    return this.roles.filter(role => roleIds.includes(role.ID));
