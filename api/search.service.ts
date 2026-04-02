import { BehaviorSubject, Observable, Subject, of } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, map } from 'rxjs/operators';
  MJArtifactEntity
import { RunView, UserInfo, Metadata } from '@memberjunction/core';
 * Types of searchable content
export type SearchResultType = 'conversation' | 'message' | 'artifact' | 'collection' | 'task';
 * Filter options for search
export type SearchFilter = 'all' | 'conversations' | 'messages' | 'artifacts' | 'collections' | 'tasks';
 * Unified search result
export interface SearchResult {
  type: SearchResultType;
  preview: string;
  matchedText?: string;
  collectionName?: string;
 * Search results grouped by type
export interface GroupedSearchResults {
  conversations: SearchResult[];
  messages: SearchResult[];
  artifacts: SearchResult[];
  collections: SearchResult[];
  tasks: SearchResult[];
 * Date range filter
export interface DateRange {
  start: Date | null;
  end: Date | null;
 * Service for searching across conversations, messages, and artifacts
 * Provides debounced search with result ranking and filtering
export class SearchService {
  private _searchQuery$ = new Subject<string>();
  private _searchFilter$ = new BehaviorSubject<SearchFilter>('all');
  private _dateRange$ = new BehaviorSubject<DateRange>({ start: null, end: null });
  private _isSearching$ = new BehaviorSubject<boolean>(false);
  private _searchResults$ = new BehaviorSubject<GroupedSearchResults>({
  // Recent searches stored in memory (could be persisted to localStorage)
  private recentSearches: string[] = [];
  private readonly MAX_RECENT_SEARCHES = 10;
  public readonly searchQuery$ = this._searchQuery$.asObservable();
  public readonly searchFilter$ = this._searchFilter$.asObservable();
  public readonly dateRange$ = this._dateRange$.asObservable();
  public readonly isSearching$ = this._isSearching$.asObservable();
  public readonly searchResults$ = this._searchResults$.asObservable();
    this.initializeSearch();
   * Initialize debounced search
  private initializeSearch(): void {
    this._searchQuery$.pipe(
    ).subscribe(query => {
      if (query && query.trim().length > 0) {
        this.addToRecentSearches(query);
   * Search across all content types
  public async search(
  ): Promise<GroupedSearchResults> {
    if (!query || query.trim().length === 0) {
      const emptyResults: GroupedSearchResults = {
      this._searchResults$.next(emptyResults);
      return emptyResults;
    this._isSearching$.next(true);
    this._searchQuery$.next(query);
      const filter = this._searchFilter$.value;
      const dateRange = this._dateRange$.value;
      // Search based on active filter
      const [conversations, messages, artifacts, collections, tasks] = await Promise.all([
        filter === 'all' || filter === 'conversations'
          ? this.searchConversations(query, environmentId, currentUser, dateRange)
          : Promise.resolve([]),
        filter === 'all' || filter === 'messages'
          ? this.searchMessages(query, environmentId, currentUser, dateRange)
        filter === 'all' || filter === 'artifacts'
          ? this.searchArtifacts(query, environmentId, currentUser, dateRange)
        filter === 'all' || filter === 'collections'
          ? this.searchCollections(query, environmentId, currentUser, dateRange)
        filter === 'all' || filter === 'tasks'
          ? this.searchTasks(query, environmentId, currentUser, dateRange)
          : Promise.resolve([])
      const results: GroupedSearchResults = {
        conversations,
        artifacts,
        collections,
        tasks,
        total: conversations.length + messages.length + artifacts.length + collections.length + tasks.length
      this._searchResults$.next(results);
      this._isSearching$.next(false);
   * Search conversations by name and description
  private async searchConversations(
    dateRange: DateRange
  ): Promise<SearchResult[]> {
    let filter = `EnvironmentID='${environmentId}' AND (IsArchived IS NULL OR IsArchived=0)`;
    filter += ` AND (LOWER(Name) LIKE '%${this.escapeSQL(lowerQuery)}%' OR LOWER(Description) LIKE '%${this.escapeSQL(lowerQuery)}%')`;
    if (dateRange.start) {
      filter += ` AND __mj_CreatedAt >= '${dateRange.start.toISOString()}'`;
    if (dateRange.end) {
      filter += ` AND __mj_CreatedAt <= '${dateRange.end.toISOString()}'`;
      console.error('Failed to search conversations:', result.ErrorMessage);
    return result.Results.map(conv => this.mapConversationToSearchResult(conv, query));
   * Search message content
  private async searchMessages(
    // First get conversations in this environment
    let filter = `ConversationID IN (SELECT ID FROM vwConversations WHERE EnvironmentID='${environmentId}' AND (IsArchived IS NULL OR IsArchived=0))`;
    filter += ` AND LOWER(Message) LIKE '%${this.escapeSQL(lowerQuery)}%'`;
    filter += ` AND (HiddenToUser IS NULL OR HiddenToUser=0)`;
      console.error('Failed to search messages:', result.ErrorMessage);
    return result.Results.map(msg => this.mapMessageToSearchResult(msg, query));
   * Search artifacts by name and description
   * Includes artifacts from both conversations and collections
  private async searchArtifacts(
    // Search artifacts directly by name and description
    let filter = `EnvironmentID='${environmentId}'`;
        ExtraFilter: `${filter} AND (Visibility IS NULL OR Visibility='Always')`,
      console.error('Failed to search artifacts:', result.ErrorMessage);
    const searchResults: SearchResult[] = [];
    // For each artifact, check if it's in a collection for context
    for (const artifact of result.Results) {
      // Check for collection associations
      const collResult = await rv.RunView<MJCollectionArtifactEntity>(
            SELECT ID FROM [__mj].[vwArtifactVersions] WHERE ArtifactID='${artifact.ID}'
      if (collResult.Success && collResult.Results && collResult.Results.length > 0) {
        const collArtifact = collResult.Results[0];
        searchResults.push(
          this.mapArtifactToSearchResult(
            artifact,
            collArtifact.CollectionID,
            collArtifact.Collection
        // No collection association
        searchResults.push(this.mapArtifactToSearchResult(artifact, query));
    return searchResults;
   * Search collections by name and description
   * Includes both owned collections and shared collections user has access to
  private async searchCollections(
    const ownerFilter = `OwnerID='${currentUser.ID}'`;
      WHERE UserID='${currentUser.ID}'
    filter += ` AND (OwnerID IS NULL OR ${ownerFilter} OR ${permissionSubquery})`;
      console.error('Failed to search collections:', result.ErrorMessage);
    return result.Results.map(coll => this.mapCollectionToSearchResult(coll, query));
   * Search tasks by name, description, and notes
   * Only includes tasks in conversations user has access to
  private async searchTasks(
    // Build filter using same logic as TasksFullViewComponent
    let filter = `ParentID IS NULL AND (UserID = '${currentUser.ID}' OR ConversationDetailID IN (
      UserID ='${currentUser.ID}' OR
        SELECT ID FROM [${c.SchemaName}].[${c.BaseView}] WHERE UserID='${currentUser.ID}'
      console.error('Failed to search tasks:', result.ErrorMessage);
    return result.Results.map(task => this.mapTaskToSearchResult(task, query));
   * Map conversation entity to search result
  private mapConversationToSearchResult(conversation: MJConversationEntity, query: string): SearchResult {
    const name = conversation.Name || 'Untitled Conversation';
    const description = conversation.Description || '';
    // Calculate relevance score
    if (name.toLowerCase().includes(lowerQuery)) score += 10;
    if (description.toLowerCase().includes(lowerQuery)) score += 5;
    // Find matched text
    const matchedText = this.extractMatchContext(
      name.toLowerCase().includes(lowerQuery) ? name : description,
      query
      id: conversation.ID,
      title: name,
      preview: description || 'No description',
      matchedText,
      conversationId: conversation.ID,
      conversationName: name,
      createdAt: conversation.__mj_CreatedAt,
      relevanceScore: score
   * Map message entity to search result
  private mapMessageToSearchResult(message: MJConversationDetailEntity, query: string): SearchResult {
    const messageText = message.Message || '';
    const matchedText = this.extractMatchContext(messageText, query);
      id: message.ID,
      type: 'message',
      title: `Message in ${message.Conversation || 'Unknown Conversation'}`,
      preview: this.truncateText(messageText, 150),
      conversationId: message.ConversationID,
      conversationName: message.Conversation || undefined,
      createdAt: message.__mj_CreatedAt,
      relevanceScore: 5
   * Map artifact entity to search result
  private mapArtifactToSearchResult(
    artifact: MJArtifactEntity,
    collectionId?: string,
    collectionName?: string
  ): SearchResult {
    const name = artifact.Name || 'Untitled Artifact';
    const description = artifact.Description || '';
      id: artifact.ID,
      collectionName,
      artifactType: artifact.Type || undefined,
      createdAt: artifact.__mj_CreatedAt,
   * Map collection entity to search result
  private mapCollectionToSearchResult(collection: MJCollectionEntity, query: string): SearchResult {
    const name = collection.Name || 'Untitled Collection';
    const description = collection.Description || '';
      collectionName: name,
      createdAt: collection.__mj_CreatedAt,
   * Map task entity to search result
  private mapTaskToSearchResult(task: MJTaskEntity, query: string): SearchResult {
    const name = task.Name || 'Untitled Task';
    const description = task.Description || '';
      id: task.ID,
      createdAt: task.__mj_CreatedAt,
   * Extract context around matched text
  private extractMatchContext(text: string, query: string, contextLength: number = 100): string {
    const index = lowerText.indexOf(lowerQuery);
    if (index === -1) return this.truncateText(text, contextLength);
    const start = Math.max(0, index - contextLength / 2);
    const end = Math.min(text.length, index + query.length + contextLength / 2);
    let context = text.substring(start, end);
    if (start > 0) context = '...' + context;
    if (end < text.length) context = context + '...';
   * Truncate text to specified length
    return text.substring(0, maxLength) + '...';
   * Escape SQL special characters
  private escapeSQL(value: string): string {
    return value.replace(/'/g, "''");
  public setSearchFilter(filter: SearchFilter): void {
    this._searchFilter$.next(filter);
   * Set date range filter
  public setDateRange(range: DateRange): void {
    this._dateRange$.next(range);
    this._searchFilter$.next('all');
    this._dateRange$.next({ start: null, end: null });
   * Clear search results
  public clearResults(): void {
    this._searchResults$.next({
   * Get recent searches
  public getRecentSearches(): string[] {
    return [...this.recentSearches];
   * Add to recent searches
  private addToRecentSearches(query: string): void {
    const trimmed = query.trim();
    if (!trimmed) return;
    // Remove if already exists
    this.recentSearches = this.recentSearches.filter(q => q !== trimmed);
    this.recentSearches.unshift(trimmed);
    // Keep only MAX_RECENT_SEARCHES
    if (this.recentSearches.length > this.MAX_RECENT_SEARCHES) {
      this.recentSearches = this.recentSearches.slice(0, this.MAX_RECENT_SEARCHES);
    this.saveRecentSearches();
   * Load recent searches from localStorage
      const stored = localStorage.getItem('mj-recent-searches');
        this.recentSearches = JSON.parse(stored);
      console.error('Failed to load recent searches:', error);
   * Save recent searches to localStorage
  private saveRecentSearches(): void {
      localStorage.setItem('mj-recent-searches', JSON.stringify(this.recentSearches));
      console.error('Failed to save recent searches:', error);
