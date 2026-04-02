 * Shared data service for conversations
 * This is a SINGLETON service that manages the conversation data (list of conversations)
 * Selection state (which conversation is active) is managed by parent components locally
 * This architecture enables multiple conversation panels to coexist (e.g., in tabs)
 * without conflicting over which conversation is "active"
export class ConversationDataService {
  // The list of conversations - shared across all components
  public conversations: MJConversationEntity[] = [];
  // Observable for conversation list changes (for components that need reactive updates)
  private _conversations$ = new BehaviorSubject<MJConversationEntity[]>([]);
  public readonly conversations$ = this._conversations$.asObservable();
  // Search query for filtering - shared across sidebars
   * Gets a conversation by ID
   * @param id The conversation ID
   * @returns The conversation entity or null if not found
  getConversationById(id: string): MJConversationEntity | null {
    return this.conversations.find(c => c.ID === id) || null;
   * Gets filtered conversations based on search query
      return this.conversations;
    return this.conversations.filter(c =>
   * Gets pinned conversations
  get pinnedConversations(): MJConversationEntity[] {
    return this.conversations.filter(c => c.IsPinned);
   * Sets the search query
   * @param query The search query string
  setSearchQuery(query: string): void {
   * Clears the search query
  clearSearchQuery(): void {
   * Adds a conversation to the list
   * @param conversation The conversation to add
  addConversation(conversation: MJConversationEntity): void {
    this.conversations = [conversation, ...this.conversations];
    this._conversations$.next(this.conversations);
   * Updates a conversation in the list by directly modifying the entity object
   * Angular change detection will pick up the changes automatically
  updateConversationInPlace(id: string, updates: Partial<MJConversationEntity>): void {
    const conversation = this.conversations.find(c => c.ID === id);
      Object.assign(conversation, updates);
      // Emit update to trigger reactive subscribers
   * Removes a conversation from the list
   * @param id The conversation ID to remove
   * @returns True if the conversation was the active one (caller may need to handle)
  removeConversation(id: string): void {
    this.conversations = this.conversations.filter(c => c.ID !== id);
   * Loads conversations from the database.
   * Skips if already loaded to prevent redundant DB calls.
   * @param environmentId The environment ID to filter by
  async loadConversations(environmentId: string, currentUser: UserInfo): Promise<void> {
    // Skip if already loaded - prevents redundant DB calls when multiple components initialize
    if (this.conversations.length > 0) {
    await this.fetchConversations(environmentId, currentUser);
   * Force refresh conversations from the database, ignoring cache.
   * Use this when user explicitly requests a refresh.
  async refreshConversations(environmentId: string, currentUser: UserInfo): Promise<void> {
   * Internal method to fetch conversations from the database.
  private async fetchConversations(environmentId: string, currentUser: UserInfo): Promise<void> {
      const filter = `EnvironmentID='${environmentId}' AND UserID='${currentUser.ID}' AND (IsArchived IS NULL OR IsArchived=0)`;
      const result = await rv.RunView<MJConversationEntity>(
          OrderBy: 'IsPinned DESC, __mj_UpdatedAt DESC',
        this.conversations = result.Results || [];
        console.error('Failed to load conversations:', result.ErrorMessage);
        this.conversations = [];
      console.error('Error loading conversations:', error);
   * Creates a new conversation
   * @param name The conversation name
   * @param environmentId The environment ID
   * @param description Optional description
   * @param projectId Optional project ID
   * @returns The created conversation entity
  async createConversation(
    environmentId: string,
    projectId?: string
  ): Promise<MJConversationEntity> {
    const conversation = await md.GetEntityObject<MJConversationEntity>('MJ: Conversations', currentUser);
    conversation.Name = name;
    conversation.EnvironmentID = environmentId;
    if (description) conversation.Description = description;
    if (projectId) conversation.ProjectID = projectId;
      this.addConversation(conversation);
      return conversation;
      throw new Error(conversation.LatestResult?.Message || 'Failed to create conversation');
   * Deletes a conversation
  async deleteConversation(id: string, currentUser: UserInfo): Promise<boolean> {
    const loaded = await conversation.Load(id);
      throw new Error('Conversation not found');
    const deleted = await conversation.Delete();
      this.removeConversation(id);
      throw new Error(conversation.LatestResult?.Message || 'Failed to delete conversation');
   * Deletes multiple conversations in a batch operation
   * @param ids - Array of conversation IDs to delete
   * @param currentUser - Current user info
   * @returns Object with successful deletions and failed deletions with error info
  async deleteMultipleConversations(
    ids: string[],
    successful: string[];
    failed: Array<{ id: string; name: string; error: string }>;
    const successful: string[] = [];
    const failed: Array<{ id: string; name: string; error: string }> = [];
    for (const id of ids) {
        const name = conversation?.Name || 'Unknown';
        const deleted = await this.deleteConversation(id, currentUser);
          successful.push(id);
          failed.push({ id, name, error: 'Delete returned false' });
        failed.push({
          name: conversation?.Name || 'Unknown',
    return { successful, failed };
   * Updates a conversation - saves to database AND updates in-place in the array
  async saveConversation(
    updates: Partial<MJConversationEntity>,
    // Apply updates
      // Update the in-memory conversation directly
      this.updateConversationInPlace(id, updates);
      throw new Error(conversation.LatestResult?.Message || 'Failed to update conversation');
   * Toggles the pinned status of a conversation
  async togglePin(id: string, currentUser: UserInfo): Promise<void> {
      await this.saveConversation(id, { IsPinned: !conversation.IsPinned }, currentUser);
   * Archives a conversation
  async archiveConversation(id: string, currentUser: UserInfo): Promise<void> {
    await this.saveConversation(id, { IsArchived: true }, currentUser);
