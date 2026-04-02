import { ListSharingService } from '../../services/list-sharing.service';
  ListShareDialogConfig,
  ListShareDialogResult,
  ListShareInfo,
  ListPermissionLevel,
  ShareRecipient
} from '../../models/list-sharing.models';
 * Dialog component for managing list sharing.
 * Allows users to share lists with other users or roles with different permission levels.
 * - User/role search with autocomplete
 * - Permission level selection (View, Edit, Owner)
 * - Current shares management
 * - Remove access functionality
  selector: 'mj-list-share-dialog',
  templateUrl: './list-share-dialog.component.html',
  styleUrls: ['./list-share-dialog.component.css']
export class ListShareDialogComponent implements OnInit, OnDestroy {
  @Input() config!: ListShareDialogConfig;
  @Output() complete = new EventEmitter<ListShareDialogResult>();
  searchType: 'User' | 'Role' = 'User';
  showSearchResults = false;
  currentShares: ListShareInfo[] = [];
  searchResults: ShareRecipient[] = [];
  selectedRecipient: ShareRecipient | null = null;
  selectedPermission: ListPermissionLevel = 'View';
  sharesAdded: ListShareInfo[] = [];
  sharesUpdated: ListShareInfo[] = [];
  sharesRemoved: string[] = [];
  // Permission options
  permissionOptions: { value: ListPermissionLevel; label: string; description: string }[] = [
    { value: 'View', label: 'Viewer', description: 'Can view list contents' },
    { value: 'Edit', label: 'Editor', description: 'Can add/remove items' },
    { value: 'Owner', label: 'Co-owner', description: 'Full control including sharing' }
    private sharingService: ListSharingService,
    return `Share "${this.config?.listName || 'List'}"`;
    return this.sharesAdded.length > 0 ||
           this.sharesUpdated.length > 0 ||
           this.sharesRemoved.length > 0;
    ).subscribe(async (searchText: string) => {
      if (searchText.trim().length >= 2) {
        await this.performSearch(searchText);
    await this.loadCurrentShares();
    this.searchType = 'User';
    this.selectedRecipient = null;
    this.selectedPermission = 'View';
    this.sharesAdded = [];
    this.sharesUpdated = [];
    this.sharesRemoved = [];
   * Load current shares for the list
  private async loadCurrentShares(): Promise<void> {
      this.currentShares = await this.sharingService.getListShares(this.config.listId);
      console.error('Error loading shares:', error);
      this.currentShares = [];
   * Switch search type
  setSearchType(type: 'User' | 'Role'): void {
    this.searchType = type;
    if (this.searchText.trim().length >= 2) {
      this.searchSubject.next(this.searchText);
  private async performSearch(searchText: string): Promise<void> {
      if (this.searchType === 'User') {
        this.searchResults = await this.sharingService.searchUsers(searchText);
        this.searchResults = await this.sharingService.searchRoles(searchText);
      // Filter out recipients that already have access
      const existingIds = new Set(this.currentShares.map(s => s.recipientId));
      this.searchResults = this.searchResults.filter(r => !existingIds.has(r.id));
      // Also filter out the list owner
      this.searchResults = this.searchResults.filter(r => r.id !== this.config?.currentUserId);
      this.showSearchResults = this.searchResults.length > 0;
      console.error('Error searching:', error);
   * Select a recipient from search results
  selectRecipient(recipient: ShareRecipient): void {
    this.selectedRecipient = recipient;
    this.searchText = recipient.name;
   * Add the selected recipient as a share
  async addShare(): Promise<void> {
    if (!this.selectedRecipient || !this.config) return;
      if (this.selectedRecipient.type === 'User') {
        result = await this.sharingService.shareListWithUser(
          this.config.listId,
          this.selectedRecipient.id,
          this.selectedPermission,
          this.config.currentUserId
        result = await this.sharingService.shareListWithRole(
      if (result.success && result.shareId) {
        // Create a ListShareInfo for tracking
        const newShare: ListShareInfo = {
          shareId: result.shareId,
          listId: this.config.listId,
          type: this.selectedRecipient.type,
          recipientId: this.selectedRecipient.id,
          recipientName: this.selectedRecipient.name,
          recipientEmail: this.selectedRecipient.email,
          permissionLevel: this.selectedPermission,
          status: 'Approved'
        this.currentShares.push(newShare);
        this.sharesAdded.push(newShare);
        console.error('Failed to add share:', result.message);
      console.error('Error adding share:', error);
   * Update permission level for an existing share
  async updateSharePermission(share: ListShareInfo, newLevel: ListPermissionLevel): Promise<void> {
    if (share.permissionLevel === newLevel) return;
      const result = await this.sharingService.updateSharePermission(share.shareId, newLevel);
        share.permissionLevel = newLevel;
        // Track the update if not already tracked
        const existingUpdate = this.sharesUpdated.find(s => s.shareId === share.shareId);
        if (!existingUpdate) {
          this.sharesUpdated.push({ ...share });
        console.error('Failed to update permission:', result.message);
   * Remove a share
  async removeShare(share: ListShareInfo): Promise<void> {
      const result = await this.sharingService.removeShare(share.shareId);
        // Remove from current shares
        this.currentShares = this.currentShares.filter(s => s.shareId !== share.shareId);
        // Track the removal
        this.sharesRemoved.push(share.shareId);
        // Remove from added if it was just added in this session
        this.sharesAdded = this.sharesAdded.filter(s => s.shareId !== share.shareId);
        console.error('Failed to remove share:', result.message);
      console.error('Error removing share:', error);
   * Get icon for share type
  getShareTypeIcon(type: 'User' | 'Role'): string {
    return type === 'User' ? 'fa-solid fa-user' : 'fa-solid fa-users';
   * Get permission label
  getPermissionLabel(level: ListPermissionLevel): string {
    const option = this.permissionOptions.find(o => o.value === level);
    return option?.label || level;
   * Close dialog with results
  onDone(): void {
    const result: ListShareDialogResult = {
      sharesAdded: this.sharesAdded,
      sharesUpdated: this.sharesUpdated,
      sharesRemoved: this.sharesRemoved
   * Check if current user can modify shares
  get canModifyShares(): boolean {
    return this.config?.isOwner === true;
