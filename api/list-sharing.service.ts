  MJRoleEntity
import { ListShareInfo, ListPermissionLevel, ShareRecipient, ListShareResult } from '../models/list-sharing.models';
 * Known List ResourceType ID from the database
const LIST_RESOURCE_TYPE_ID = 'E64D433E-F36B-1410-8560-0041FA62858A';
 * Service for managing list sharing using the existing ResourcePermission system.
 * Lists are already registered as a ResourceType, so we use ResourcePermissions
 * to share lists with users and roles.
export class ListSharingService {
  // Cache for users and roles (for autocomplete)
  private usersCache: MJUserEntity[] | null = null;
  private rolesCache: MJRoleEntity[] | null = null;
   * Get the ResourceType ID for Lists
  getListResourceTypeId(): string {
    return LIST_RESOURCE_TYPE_ID;
   * Get all shares for a specific list
  async getListShares(listId: string): Promise<ListShareInfo[]> {
        ExtraFilter: `ResourceTypeID = '${LIST_RESOURCE_TYPE_ID}' AND ResourceRecordID = '${listId}'`,
      // Convert to ListShareInfo objects with user/role details
      const shares: ListShareInfo[] = [];
      for (const permission of result.Results) {
        const shareInfo = await this.convertToListShareInfo(permission);
        if (shareInfo) {
          shares.push(shareInfo);
   * Get permission level for a specific user on a specific list
  async getUserPermissionLevel(listId: string, userId: string): Promise<ListPermissionLevel | null> {
    // Check direct user permission
    const userResult = await rv.RunView<MJResourcePermissionEntity>({
      ExtraFilter: `ResourceTypeID = '${LIST_RESOURCE_TYPE_ID}' AND ResourceRecordID = '${listId}' AND Type = 'User' AND UserID = '${userId}' AND Status = 'Approved'`,
      return userResult.Results[0].PermissionLevel as ListPermissionLevel;
    // If no direct permission, could check role-based permissions
    // For now, return null
   * Share a list with a user
  async shareListWithUser(
    listId: string,
    permissionLevel: ListPermissionLevel,
    sharedByUserId: string
  ): Promise<ListShareResult> {
      // Check if share already exists
      const existing = await this.findExistingShare(listId, 'User', userId);
        // Update existing share
        existing.PermissionLevel = permissionLevel;
        existing.Status = 'Approved';
        const saved = await existing.Save();
          shareId: existing.ID,
          message: saved ? 'Share updated successfully' : 'Failed to update share'
      // Create new share
      const permission = await md.GetEntityObject<MJResourcePermissionEntity>('MJ: Resource Permissions');
      permission.ResourceTypeID = LIST_RESOURCE_TYPE_ID;
      permission.ResourceRecordID = listId;
      permission.Type = 'User';
      permission.PermissionLevel = permissionLevel;
      permission.Status = 'Approved';
        shareId: permission.ID,
        message: saved ? 'List shared successfully' : 'Failed to share list'
        message: `Error sharing list: ${errorMessage}`
   * Share a list with a role
  async shareListWithRole(
    roleId: string,
      const existing = await this.findExistingShare(listId, 'Role', roleId);
      permission.Type = 'Role';
      permission.RoleID = roleId;
        message: saved ? 'List shared with role successfully' : 'Failed to share list with role'
  async updateSharePermission(
    shareId: string,
    newPermissionLevel: ListPermissionLevel
      const loaded = await permission.Load(shareId);
          message: 'Share not found'
      permission.PermissionLevel = newPermissionLevel;
        message: saved ? 'Permission updated successfully' : 'Failed to update permission'
        message: `Error updating permission: ${errorMessage}`
   * Remove a share (revoke access)
  async removeShare(shareId: string): Promise<ListShareResult> {
      const deleted = await permission.Delete();
        success: deleted,
        message: deleted ? 'Share removed successfully' : 'Failed to remove share'
        message: `Error removing share: ${errorMessage}`
   * Get lists shared with the current user (that they don't own)
  async getListsSharedWithUser(userId: string): Promise<string[]> {
      ExtraFilter: `ResourceTypeID = '${LIST_RESOURCE_TYPE_ID}' AND Type = 'User' AND UserID = '${userId}' AND Status = 'Approved'`,
      Fields: ['ResourceRecordID'],
    return result.Results.map((r: { ResourceRecordID: string }) => r.ResourceRecordID);
   * Get lists that the user has shared with others
  async getListsSharedByUser(userId: string): Promise<Map<string, number>> {
    // First get all lists owned by the user
    if (!listsResult.Success || !listsResult.Results || listsResult.Results.length === 0) {
    const listIds = listsResult.Results.map((l: { ID: string }) => l.ID);
    const listIdFilter = listIds.map((id: string) => `'${id}'`).join(',');
    // Get all shares for these lists
    const sharesResult = await rv.RunView<MJResourcePermissionEntity>({
      ExtraFilter: `ResourceTypeID = '${LIST_RESOURCE_TYPE_ID}' AND ResourceRecordID IN (${listIdFilter}) AND Status = 'Approved'`,
    const shareCountMap = new Map<string, number>();
    if (sharesResult.Success && sharesResult.Results) {
      for (const share of sharesResult.Results) {
        const typedShare = share as { ResourceRecordID: string };
        const current = shareCountMap.get(typedShare.ResourceRecordID) || 0;
        shareCountMap.set(typedShare.ResourceRecordID, current + 1);
    return shareCountMap;
   * Check if user can share a list (must be owner or have Owner permission)
  async canUserShareList(listId: string, userId: string): Promise<boolean> {
    // Check if user is the owner
      ExtraFilter: `ID = '${listId}' AND UserID = '${userId}'`,
    if (listResult.Success && listResult.Results && listResult.Results.length > 0) {
    // Check if user has Owner permission
    const permissionLevel = await this.getUserPermissionLevel(listId, userId);
    return permissionLevel === 'Owner';
   * Search users for sharing autocomplete
  async searchUsers(searchTerm: string, limit: number = 10): Promise<ShareRecipient[]> {
      ExtraFilter: `(Name LIKE '%${searchTerm}%' OR Email LIKE '%${searchTerm}%') AND IsActive = 1`,
    return result.Results.map(user => ({
      type: 'User' as const
   * Search roles for sharing autocomplete
  async searchRoles(searchTerm: string, limit: number = 10): Promise<ShareRecipient[]> {
      ExtraFilter: `Name LIKE '%${searchTerm}%'`,
    return result.Results.map(role => ({
      id: role.ID,
      type: 'Role' as const
   * Get all available users (cached)
  async getAllUsers(forceRefresh: boolean = false): Promise<MJUserEntity[]> {
    if (!forceRefresh && this.usersCache) {
      this.usersCache = result.Results;
   * Get all available roles (cached)
  async getAllRoles(forceRefresh: boolean = false): Promise<MJRoleEntity[]> {
    if (!forceRefresh && this.rolesCache) {
      return this.rolesCache;
      this.rolesCache = result.Results;
   * Helper to find existing share
  private async findExistingShare(
    type: 'User' | 'Role',
    recipientId: string
  ): Promise<MJResourcePermissionEntity | null> {
    const idField = type === 'User' ? 'UserID' : 'RoleID';
      ExtraFilter: `ResourceTypeID = '${LIST_RESOURCE_TYPE_ID}' AND ResourceRecordID = '${listId}' AND Type = '${type}' AND ${idField} = '${recipientId}'`,
   * Convert MJResourcePermissionEntity to ListShareInfo
  private async convertToListShareInfo(permission: MJResourcePermissionEntity): Promise<ListShareInfo | null> {
    if (permission.Type === 'User' && permission.UserID) {
        ExtraFilter: `ID = '${permission.UserID}'`,
          listId: permission.ResourceRecordID,
          recipientId: permission.UserID,
          recipientName: user.Name,
          recipientEmail: user.Email,
          permissionLevel: permission.PermissionLevel as ListPermissionLevel,
          status: permission.Status as 'Approved' | 'Pending' | 'Rejected',
          startSharingAt: permission.StartSharingAt ? new Date(permission.StartSharingAt) : undefined,
          endSharingAt: permission.EndSharingAt ? new Date(permission.EndSharingAt) : undefined
    } else if (permission.Type === 'Role' && permission.RoleID) {
      const roleResult = await rv.RunView<MJRoleEntity>({
        ExtraFilter: `ID = '${permission.RoleID}'`,
      if (roleResult.Success && roleResult.Results && roleResult.Results.length > 0) {
        const role = roleResult.Results[0];
          type: 'Role',
          recipientId: permission.RoleID,
          recipientName: role.Name,
   * Get sharing summary for a single list
  async getListSharingSummary(listId: string): Promise<{ listId: string; totalShares: number; userShares: number; roleShares: number; isSharedWithMe: boolean; isSharedByMe: boolean }> {
      ExtraFilter: `ResourceTypeID = '${LIST_RESOURCE_TYPE_ID}' AND ResourceRecordID = '${listId}' AND Status = 'Approved'`,
      Fields: ['ID', 'Type'],
    let userShares = 0;
    let roleShares = 0;
      for (const share of result.Results) {
        const typedShare = share as { ID: string; Type: string };
        if (typedShare.Type === 'User') {
          userShares++;
        } else if (typedShare.Type === 'Role') {
          roleShares++;
      totalShares: userShares + roleShares,
      userShares,
      roleShares,
      isSharedWithMe: false, // Would need current user context to determine
      isSharedByMe: userShares + roleShares > 0
   * Clear internal caches
    this.usersCache = null;
    this.rolesCache = null;
