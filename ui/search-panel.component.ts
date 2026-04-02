  ElementRef
  SearchService,
  SearchResult,
  SearchFilter,
  GroupedSearchResults,
  DateRange
} from '../../services/search.service';
 * Search panel component providing global search UI
 * Can be displayed as a modal or slide-out panel
 * Supports filtering, date ranges, and result navigation
  selector: 'mj-search-panel',
  templateUrl: './search-panel.component.html',
  styleUrls: ['./search-panel.component.css']
export class SearchPanelComponent implements OnInit, OnDestroy {
  @Output() resultSelected = new EventEmitter<SearchResult>();
  @ViewChild('searchInput') searchInput?: ElementRef<HTMLInputElement>;
  public activeFilter: SearchFilter = 'all';
  public dateRange: DateRange = { start: null, end: null };
  public isSearching: boolean = false;
  public results: GroupedSearchResults = {
    conversations: [],
    collections: [],
    tasks: [],
  public recentSearches: string[] = [];
  public selectedIndex: number = -1;
  constructor(private searchService: SearchService) {}
    this.subscribeToSearchState();
    this.loadRecentSearches();
   * Subscribe to search service state
  private subscribeToSearchState(): void {
    this.searchService.isSearching$
      .subscribe(isSearching => {
        this.isSearching = isSearching;
    this.searchService.searchResults$
      .subscribe(results => {
        this.results = results;
        this.selectedIndex = -1;
    this.searchService.searchFilter$
      .subscribe(filter => {
        this.activeFilter = filter;
    this.searchService.dateRange$
      .subscribe(range => {
        this.dateRange = range;
   * Load recent searches
  private loadRecentSearches(): void {
    this.recentSearches = this.searchService.getRecentSearches();
  public onSearchInput(): void {
      this.performSearch();
      this.searchService.clearResults();
   * Perform search
  private async performSearch(): Promise<void> {
    await this.searchService.search(
      this.searchQuery,
   * Set search filter
  public setFilter(filter: SearchFilter): void {
    this.searchService.setSearchFilter(filter);
    this.focusSearchInput();
   * Select a result
  public selectResult(result: SearchResult): void {
    this.resultSelected.emit(result);
   * Use recent search
  public useRecentSearch(query: string): void {
    this.searchQuery = query;
   * Clear recent searches
  public clearRecentSearches(): void {
    this.searchService.clearRecentSearches();
    this.recentSearches = [];
   * Close panel
   * Focus search input
  private focusSearchInput(): void {
      this.searchInput?.nativeElement.focus();
  @HostListener('keydown', ['$event'])
  public handleKeyboard(event: KeyboardEvent): void {
    if (!this.isOpen) return;
    const allResults = this.getAllResultsFlat();
        if (allResults.length > 0) {
          this.selectedIndex = Math.min(this.selectedIndex + 1, allResults.length - 1);
        if (this.selectedIndex > 0) {
          this.selectedIndex--;
        if (this.selectedIndex >= 0 && allResults[this.selectedIndex]) {
          this.selectResult(allResults[this.selectedIndex]);
   * Get all results as flat array
  private getAllResultsFlat(): SearchResult[] {
      ...this.results.conversations,
      ...this.results.messages,
      ...this.results.artifacts,
      ...this.results.collections,
      ...this.results.tasks
   * Check if result is selected
  public isResultSelected(result: SearchResult): boolean {
    const index = allResults.findIndex(r => r.id === result.id && r.type === result.type);
    return index === this.selectedIndex;
   * Get icon for result type
  public getResultIcon(type: string): string {
      case 'conversation':
        return 'fa-comments';
      case 'message':
      case 'collection':
        return 'fa-folder';
      case 'task':
        return 'fa-tasks';
   * Get filter display text
  public getFilterText(filter: SearchFilter): string {
        return 'All';
      case 'conversations':
      case 'messages':
        return 'Messages';
        return 'Artifacts';
      case 'collections':
      case 'tasks':
   * Highlight matched text in result
  public highlightMatch(text: string, query: string): string {
    if (!query) return text;
    const regex = new RegExp(`(${this.escapeRegex(query)})`, 'gi');
  private escapeRegex(str: string): string {
    return str.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
   * Handle date range change
  public onDateRangeChange(): void {
    this.searchService.setDateRange(this.dateRange);
   * Clear date range
  public clearDateRange(): void {
    this.dateRange = { start: null, end: null };
    this.searchService.setDateRange({ start: null, end: null });
   * Watch for panel open state changes
