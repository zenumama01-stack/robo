import { MJConversationEntity, MJResourcePermissionEntity, MJUserEntity } from '@memberjunction/core-entities';
interface SharePermission {
  permissionId: string | null;
  permissionLevel: 'View' | 'Edit' | 'Owner';
  selector: 'mj-share-modal',
        [title]="'Share: ' + (conversation.Name || '')"
        (close)="onClose()">
        <div class="share-content">
            <h4>Add People</h4>
            <div class="add-user-form">
                [(value)]="newUserEmail"
              <button kendoButton [primary]="true" (click)="onAddUser()">
            <h4>People with Access</h4>
            <div class="permission-list">
                  <p>No one has been given access yet</p>
                      <div class="user-name">{{ permission.userName }}</div>
                      <div class="user-email">{{ permission.userEmail }}</div>
                  <div class="permission-controls">
                      [data]="accessLevels"
                      [value]="getAccessLevel(permission)"
                      (valueChange)="onAccessLevelChange(permission, $event)"
                      (click)="onRemoveUser(permission)"
                      title="Remove access">
          <div class="link-section">
            <h4>Share Link</h4>
            <div class="link-controls">
                [(ngModel)]="isPublicLink"
                (valueChange)="onTogglePublicLink()">
              <label>Anyone with the link can view</label>
            @if (isPublicLink) {
              <div class="link-display">
                  [value]="shareLink"
                <button kendoButton (click)="onCopyLink()">
          <button kendoButton (click)="onClose()">Close</button>
    .share-content { display: flex; flex-direction: column; gap: 24px; }
    .add-user-section h4,
    .permissions-section h4,
    .link-section h4 { margin: 0 0 12px 0; font-size: 14px; font-weight: 600; }
    .add-user-form { display: flex; gap: 8px; }
    .permission-list { border: 1px solid #D9D9D9; border-radius: 4px; max-height: 300px; overflow-y: auto; }
    .permission-item { padding: 12px; border-bottom: 1px solid #E8E8E8; display: flex; justify-content: space-between; align-items: center; }
    .permission-item:last-child { border-bottom: none; }
    .user-info { display: flex; align-items: center; gap: 12px; flex: 1; }
    .user-info i { font-size: 32px; color: #999; }
    .user-details { display: flex; flex-direction: column; }
    .user-name { font-size: 14px; font-weight: 500; }
    .user-email { font-size: 12px; color: #666; }
    .permission-controls { display: flex; align-items: center; gap: 8px; }
    .btn-remove { padding: 6px 8px; background: transparent; border: none; cursor: pointer; border-radius: 3px; color: #999; }
    .btn-remove:hover { background: #FFEBEE; color: #D32F2F; }
    .empty-state { padding: 24px; text-align: center; color: #999; }
    .link-controls { display: flex; align-items: center; gap: 12px; margin-bottom: 12px; }
    .link-controls label { font-size: 13px; }
    .link-display { display: flex; gap: 8px; }
export class ShareModalComponent implements OnInit {
  @Input() conversation!: MJConversationEntity;
  public permissions: SharePermission[] = [];
  public newUserEmail: string = '';
  public isPublicLink: boolean = false;
  public shareLink: string = '';
  public accessLevels = [
    { label: 'Can View', value: 'View' },
    { label: 'Can Edit', value: 'Edit' },
    { label: 'Owner', value: 'Owner' }
  private readonly CONVERSATIONS_RESOURCE_TYPE_ID = '81D4BC3D-9FEB-EF11-B01A-286B35C04427';
      this.loadPermissions();
      this.updateShareLink();
      const result = await rv.RunView<MJResourcePermissionEntity>({
        EntityName: 'MJ: Resource Permissions',
        ExtraFilter: `ResourceTypeID='${this.CONVERSATIONS_RESOURCE_TYPE_ID}' AND ResourceRecordID='${this.conversation.ID}' AND Status='Approved'`,
        const permissionPromises = result.Results.map(async (perm) => {
          if (perm.UserID) {
            const userRv = new RunView();
            const userResult = await userRv.RunView<MJUserEntity>({
              ExtraFilter: `ID='${perm.UserID}'`,
            if (userResult.Success && userResult.Results && userResult.Results.length > 0) {
              const user = userResult.Results[0];
                permissionId: perm.ID,
                userId: perm.UserID,
                userEmail: user.Email,
                userName: user.Name,
                permissionLevel: perm.PermissionLevel || 'View'
              } as SharePermission;
        const resolvedPermissions = await Promise.all(permissionPromises);
        this.permissions = resolvedPermissions.filter(p => p !== null) as SharePermission[];
      console.error('Failed to load permissions:', error);
  getAccessLevel(permission: SharePermission): string {
    return permission.permissionLevel;
  async onAccessLevelChange(permission: SharePermission, level: 'View' | 'Edit' | 'Owner'): Promise<void> {
    permission.permissionLevel = level;
    await this.savePermission(permission);
    const email = this.newUserEmail.trim();
    // Check if user already has access
    if (this.permissions.some(p => p.userEmail === email)) {
      await this.dialogService.alert('User Already Has Access', 'This user already has access');
      // Look up user by email
        ExtraFilter: `Email='${email}'`,
      if (!userResult.Success || !userResult.Results || userResult.Results.length === 0) {
        await this.dialogService.alert('User Not Found', 'No user found with that email address');
      const newPermission: SharePermission = {
        permissionId: null,
        userId: user.ID,
        permissionLevel: 'View'
      await this.savePermission(newPermission);
      this.permissions.push(newPermission);
      this.newUserEmail = '';
      this.toastService.success(`Access granted to ${user.Email}`);
      console.error('Failed to add user:', error);
      this.toastService.error('Failed to add user');
  async onRemoveUser(permission: SharePermission): Promise<void> {
      title: 'Remove Access',
      message: `Remove access for ${permission.userEmail}?`,
      if (permission.permissionId) {
        const permEntity = await md.GetEntityObject<MJResourcePermissionEntity>('MJ: Resource Permissions');
        await permEntity.Load(permission.permissionId);
        const deleteResult = await permEntity.Delete();
          throw new Error('Failed to delete permission');
      this.permissions = this.permissions.filter(p => p.userId !== permission.userId);
      this.toastService.success(`Access removed for ${permission.userEmail}`);
      console.error('Failed to remove user:', error);
      this.toastService.error('Failed to remove user');
  private async savePermission(permission: SharePermission): Promise<void> {
        // Update existing permission
        permEntity.PermissionLevel = permission.permissionLevel;
        // Create new permission
        permEntity.ResourceTypeID = this.CONVERSATIONS_RESOURCE_TYPE_ID;
        permEntity.ResourceRecordID = this.conversation.ID;
        permEntity.Type = 'User';
        permEntity.UserID = permission.userId;
        permEntity.Status = 'Approved';
      const saveResult = await permEntity.Save();
        throw new Error('Failed to save permission');
      // Update the permission ID if it was a new permission
      if (!permission.permissionId) {
        permission.permissionId = permEntity.ID;
      console.error('Failed to save permission:', error);
  async onTogglePublicLink(): Promise<void> {
      // Note: Public link functionality uses the conversation ID directly.
      // For enhanced security with unique tokens, password protection, and expiration,
      // future migration should add: PublicAccessToken, PublicAccessPassword, PublicAccessExpiresAt fields
      console.error('Failed to toggle public link:', error);
      await this.dialogService.alert('Error', 'Failed to update sharing settings');
  private updateShareLink(): void {
    if (this.isPublicLink && this.conversation) {
      // Generate shareable link
      const baseUrl = window.location.origin;
      this.shareLink = `${baseUrl}/chat/${this.conversation.ID}`;
      this.shareLink = '';
  async onCopyLink(): Promise<void> {
      await navigator.clipboard.writeText(this.shareLink);
      this.toastService.success('Link copied to clipboard');
      console.error('Failed to copy link:', err);
      this.toastService.error('Failed to copy link');
