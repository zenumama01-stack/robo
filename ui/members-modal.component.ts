interface ConversationMember {
  role: 'owner' | 'member';
  selector: 'mj-members-modal',
        [title]="modalTitle"
        <div class="members-modal-content">
          <div class="add-member-section">
            <h4>Add Member</h4>
            <div class="add-member-form">
                [(value)]="newMemberEmail"
                placeholder="Enter email address"
                [style.flex]="1">
                [(ngModel)]="newMemberRole"
                [data]="roleOptions"
                [style.width.px]="120">
              <button kendoButton [primary]="true" [disabled]="isLoading" (click)="onAddMember()">
          <div class="members-section">
            <h4>Current Members ({{ members.length }})</h4>
                @if (members.length === 0) {
                    <p>No additional members yet</p>
                @for (member of members; track member) {
                    <div class="member-info">
                      <div class="member-details">
                        <div class="member-name">{{ member.userName }}</div>
                        <div class="member-email">{{ member.userEmail }}</div>
                    <div class="member-controls">
                      <span class="member-role" [class.owner]="member.role === 'owner'">
                        {{ member.role === 'owner' ? 'Owner' : 'Member' }}
                      @if (member.role !== 'owner') {
                          class="btn-remove"
                          (click)="onRemoveMember(member)"
                          title="Remove member">
                <mj-loading text="Loading members..." size="medium"></mj-loading>
    .members-modal-content {
    .add-member-section {
    .add-member-form {
      border: 1px solid #d9d9d9;
    .member-item:last-child {
    .member-info {
    .member-info i {
    .member-details {
    .member-name {
    .member-email {
    .member-controls {
    .member-role {
    .member-role.owner {
    .btn-remove {
    .btn-remove:hover {
export class MembersModalComponent {
  @Output() membersChanged = new EventEmitter<void>();
  members: ConversationMember[] = [];
  newMemberEmail = '';
  newMemberRole: 'member' | 'owner' = 'member';
  get modalTitle(): string {
    return `Manage Members: ${this.conversation?.Name || 'Conversation'}`;
  roleOptions = [
    { value: 'member', label: 'Member' },
    { value: 'owner', label: 'Owner' }
    if (this.isVisible && this.conversation) {
      this.loadMembers();
  private async loadMembers(): Promise<void> {
    if (!this.conversation) return;
      // TODO: Load from ConversationMembers entity when available
      // For now, show the owner
      this.members = [
          id: 'owner-' + this.conversation.ID,
          userId: this.conversation.UserID || '',
          userName: 'Owner',
          userEmail: this.currentUser.Email,
          role: 'owner',
          addedAt: this.conversation.__mj_CreatedAt
      console.error('Error loading members:', error);
      this.errorMessage = 'Failed to load members';
  async onAddMember(): Promise<void> {
    const email = this.newMemberEmail.trim();
    // Simple email validation
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      await this.dialogService.alert('Invalid Email', 'Please enter a valid email address');
    // Check if already a member
    if (this.members.some(m => m.userEmail === email)) {
      await this.dialogService.alert('Already a Member', 'This user is already a member');
      // TODO: Create ConversationMember entity when available
      const newMember: ConversationMember = {
        id: 'temp-' + Date.now(),
        userId: 'unknown',
        userName: email.split('@')[0],
        userEmail: email,
        role: this.newMemberRole,
        addedAt: new Date()
      this.members.push(newMember);
      this.newMemberEmail = '';
      this.newMemberRole = 'member';
      this.membersChanged.emit();
      console.error('Error adding member:', error);
      this.errorMessage = 'Failed to add member';
  async onRemoveMember(member: ConversationMember): Promise<void> {
      title: 'Remove Member',
      message: `Remove ${member.userEmail} from this conversation?`,
      // TODO: Delete ConversationMember entity when available
      this.members = this.members.filter(m => m.id !== member.id);
      console.error('Error removing member:', error);
      this.errorMessage = 'Failed to remove member';
