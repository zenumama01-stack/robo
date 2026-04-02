 * Side panel component for displaying and managing threaded message replies
 * Shows parent message at top with all replies in chronological order
  selector: 'mj-thread-panel',
  templateUrl: './thread-panel.component.html',
  styleUrls: ['./thread-panel.component.css']
export class ThreadPanelComponent implements OnInit, OnDestroy {
  @Input() parentMessageId!: string;
  @Output() replyAdded = new EventEmitter<MJConversationDetailEntity>();
  public parentMessage: MJConversationDetailEntity | null = null;
  public replies: MJConversationDetailEntity[] = [];
  public replyText: string = '';
    private cdRef: ChangeDetectorRef
    await this.loadThreadData();
   * Loads the parent message and all replies
  private async loadThreadData(): Promise<void> {
      // Load parent message from cache
      const parent = await this.dataCache.getConversationDetail(this.parentMessageId, this.currentUser);
      if (!parent) {
        this.errorMessage = 'Failed to load parent message';
      this.parentMessage = parent;
      // Load all replies
      await this.loadReplies();
      console.error('Error loading thread data:', error);
      this.errorMessage = 'Error loading thread. Please try again.';
   * Loads all replies for the parent message
  private async loadReplies(): Promise<void> {
      const result = await rv.RunView<MJConversationDetailEntity>(
          ExtraFilter: `ParentID='${this.parentMessageId}'`,
        this.replies = result.Results || [];
        console.error('Failed to load replies:', result.ErrorMessage);
        this.errorMessage = 'Failed to load replies';
      console.error('Error loading replies:', error);
      this.errorMessage = 'Error loading replies';
   * Handles sending a new reply
  async onSendReply(): Promise<void> {
    if (!this.canSendReply) return;
      const reply = await this.dataCache.createConversationDetail(this.currentUser);
      reply.ConversationID = this.conversationId;
      reply.ParentID = this.parentMessageId;
      reply.Message = this.replyText.trim();
      reply.Role = 'User';
      const saved = await reply.Save();
        // Add to local list
        this.replies = [...this.replies, reply];
        this.replyText = '';
        // Emit event to parent
        this.replyAdded.emit(reply);
        // Update parent message thread count
        if (this.parentMessage) {
          (this.parentMessage as any).ThreadCount = ((this.parentMessage as any).ThreadCount || 0) + 1; // TODO: ThreadCount field doesn't exist on MJConversationDetailEntity yet
        console.error('Failed to save reply:', reply.LatestResult?.Message);
        this.errorMessage = 'Failed to send reply. Please try again.';
      console.error('Error sending reply:', error);
      this.errorMessage = 'Error sending reply. Please try again.';
   * Handles closing the thread panel
   * Gets the display text for a message
  getMessageText(message: MJConversationDetailEntity): string {
    return message.Message || '';
   * Gets the timestamp display for a message
  getMessageTime(message: MJConversationDetailEntity): string {
    if (!message.__mj_CreatedAt) return '';
    const date = new Date(message.__mj_CreatedAt);
   * Gets the sender name for a message
  getSenderName(message: MJConversationDetailEntity): string {
    if (message.Role === 'AI') return 'AI Assistant';
    return message.User || 'User';
   * Checks if current user can send reply
  get canSendReply(): boolean {
    return !this.isSending && !this.isLoading && this.replyText.trim().length > 0;
   * Gets the reply count text
  get replyCountText(): string {
    const count = this.replies.length;
    return count === 1 ? '1 reply' : `${count} replies`;
