import { Component, OnDestroy, ChangeDetectorRef, NgZone } from '@angular/core';
  ResourceData
import { PermissionDialogData, PermissionDialogResult } from './permission-dialog/permission-dialog.component';
interface PermissionsStats {
  restrictedEntities: number;
  totalPermissions: number;
  entitySearch: string;
  accessLevel: 'all' | 'public' | 'restricted' | 'custom';
  roleId: string | null;
interface EntityAccess {
  isPublic: boolean;
  permissions: MJEntityPermissionEntity[];
  rolePermissions: Map<string, PermissionLevel>;
interface PermissionLevel {
  canCreate: boolean;
  canRead: boolean;
  canUpdate: boolean;
  selector: 'mj-entity-permissions',
  templateUrl: './entity-permissions.component.html',
  styleUrls: ['./entity-permissions.component.css']
@RegisterClass(BaseDashboard, 'EntityPermissions')
export class EntityPermissionsComponent extends BaseDashboard implements OnDestroy {
  public entityAccess: EntityAccess[] = [];
  public filteredEntityAccess: EntityAccess[] = [];
  public roles: MJRoleEntity[] = [];
  // Permission dialog state
  public showPermissionDialog = false;
  public permissionDialogData: PermissionDialogData | null = null;
  public stats: PermissionsStats = {
    publicEntities: 0,
    restrictedEntities: 0,
    totalPermissions: 0
    entitySearch: '',
    accessLevel: 'all',
    roleId: null
  public expandedEntityId: string | null = null;
  public viewMode: 'grid' | 'list' = 'list';
  public showMobileFilters = false;
    return "Permissions"
      // Load all required data in parallel
      const [entities, permissions, roles] = await Promise.all([
        this.loadEntities(),
        this.loadEntityPermissions(),
        this.loadRoles()
      // Process the data
      this.roles = roles;
      this.processEntityAccess(entities, permissions);
      console.error('Error loading permissions data:', error);
      this.error = 'Failed to load permissions data. Please try again.';
  private async loadEntities(): Promise<MJEntityEntity[]> {
  private async loadEntityPermissions(): Promise<MJEntityPermissionEntity[]> {
    const result = await rv.RunView<MJEntityPermissionEntity>({
      OrderBy: 'EntityID, RoleID'
  private async loadRoles(): Promise<MJRoleEntity[]> {
    const result = await rv.RunView<MJRoleEntity>({
      EntityName: 'MJ: Roles',
  private processEntityAccess(entities: MJEntityEntity[], permissions: MJEntityPermissionEntity[]): void {
    // Group permissions by entity
    const permissionsByEntity = new Map<string, MJEntityPermissionEntity[]>();
    for (const permission of permissions) {
      const entityId = permission.EntityID;
      if (!permissionsByEntity.has(entityId)) {
        permissionsByEntity.set(entityId, []);
      permissionsByEntity.get(entityId)!.push(permission);
    // Create EntityAccess objects
    this.entityAccess = entities.map(entity => {
      const entityPermissions = permissionsByEntity.get(entity.ID) || [];
      const rolePermissions = new Map<string, PermissionLevel>();
      // Process permissions by role
      for (const permission of entityPermissions) {
        if (permission.RoleID) {
          rolePermissions.set(permission.RoleID, {
            canCreate: permission.CanCreate || false,
            canRead: permission.CanRead || false,
            canUpdate: permission.CanUpdate || false,
            canDelete: permission.CanDelete || false
        isPublic: entity.AllowAllRowsAPI || false,
        permissions: entityPermissions,
        rolePermissions
    let filtered = [...this.entityAccess];
    // Apply entity search
    if (filters.entitySearch) {
      const searchLower = filters.entitySearch.toLowerCase();
      filtered = filtered.filter(ea =>
        ea.entity.Name?.toLowerCase().includes(searchLower) ||
        ea.entity.Description?.toLowerCase().includes(searchLower)
    // Apply access level filter
    switch (filters.accessLevel) {
      case 'public':
        filtered = filtered.filter(ea => ea.isPublic);
      case 'restricted':
        filtered = filtered.filter(ea => !ea.isPublic && ea.permissions.length === 0);
        filtered = filtered.filter(ea => !ea.isPublic && ea.permissions.length > 0);
    // Apply role filter
    if (filters.roleId) {
        ea.rolePermissions.has(filters.roleId!)
    this.filteredEntityAccess = filtered;
    const publicEntities = this.entityAccess.filter(ea => ea.isPublic).length;
    const customPermissions = this.entityAccess.filter(ea => !ea.isPublic && ea.permissions.length > 0).length;
    const totalPermissions = this.entityAccess.reduce((sum, ea) => sum + ea.permissions.length, 0);
      totalEntities: this.entityAccess.length,
      publicEntities,
      restrictedEntities: this.entityAccess.length - publicEntities - customPermissions,
      totalPermissions
    this.updateFilter({ entitySearch: value });
  public onAccessLevelChange(level: 'all' | 'public' | 'restricted' | 'custom'): void {
    this.updateFilter({ accessLevel: level });
  public onRoleFilterChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.updateFilter({ roleId: value || null });
  public toggleEntityExpansion(entityId: string): void {
    this.expandedEntityId = this.expandedEntityId === entityId ? null : entityId;
  public isEntityExpanded(entityId: string): boolean {
    return this.expandedEntityId === entityId;
  public editEntityPermissions(entityAccess: EntityAccess): void {
    console.log('Opening permission dialog for entity:', entityAccess.entity.Name);
    console.log('Entity permissions:', entityAccess.permissions);
    console.log('Available roles:', this.roles);
    this.permissionDialogData = {
      entity: entityAccess.entity,
      roles: this.roles,
      existingPermissions: entityAccess.permissions
    this.showPermissionDialog = true;
    console.log('Dialog data set:', this.permissionDialogData);
    console.log('Dialog visible:', this.showPermissionDialog);
  public onPermissionDialogResult(result: PermissionDialogResult): void {
    this.showPermissionDialog = false;
    this.permissionDialogData = null;
      // Refresh the data after save
  public async savePermissions(): Promise<void> {
    // This method is now handled by the dialog component
    // Keeping for backwards compatibility but not used
    console.warn('savePermissions method is deprecated - use PermissionDialogComponent instead');
  public getAccessLevelClass(entityAccess: EntityAccess): string {
    if (entityAccess.isPublic) {
      return 'access-public';
    } else if (entityAccess.permissions.length === 0) {
      return 'access-restricted';
      return 'access-custom';
  public getAccessLevelLabel(entityAccess: EntityAccess): string {
      return 'Public';
      return 'Restricted';
      return 'Custom';
  public getRoleName(roleId: string): string {
    const role = this.roles.find(r => r.ID === roleId);
    return role?.Name || 'Unknown Role';
  public hasPermission(entityAccess: EntityAccess, roleId: string, permission: keyof PermissionLevel): boolean {
    const rolePermission = entityAccess.rolePermissions.get(roleId);
    return rolePermission ? rolePermission[permission] : false;
