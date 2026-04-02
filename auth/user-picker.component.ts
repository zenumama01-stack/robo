export interface UserSearchResult {
    firstName: string;
    lastName: string;
    selector: 'mj-user-picker',
        <div class="user-picker">
                    class="user-search-input"
                    (keydown.enter)="onSearch()"
                    [disabled]="isSearching"
                <button class="search-button" (click)="onSearch()" [disabled]="isSearching">
            @if (showResults && searchResults.length > 0) {
                <div class="search-results-dropdown">
                    @for (user of searchResults; track user.id) {
                        <div class="user-search-item" (click)="onSelectUser(user)">
                                <div class="user-name">{{ user.name }}</div>
                                <div class="user-email">{{ user.email }}</div>
            @if (showResults && searchResults.length === 0 && searchQuery.length >= 2 && !isSearching) {
                        <span>No users found</span>
        .user-picker {
        .user-search-input {
        .user-search-input:focus {
        .user-search-input:disabled {
        .search-button {
        .search-button:hover:not(:disabled) {
            background: #5558E3;
        .search-button:disabled {
        .search-button i {
        .search-results-dropdown {
        .user-search-item {
        .user-search-item:hover {
        .no-results span {
export class UserPickerComponent implements OnInit, OnDestroy {
    @Input() excludeUserIds: string[] = [];
    @Input() placeholder: string = 'Search for a user (press Enter)...';
    @Output() userSelected = new EventEmitter<UserSearchResult>();
    searchQuery: string = '';
    searchResults: UserSearchResult[] = [];
    isSearching: boolean = false;
    showResults: boolean = false;
        // Add click outside listener to close dropdown
        document.addEventListener('click', this.handleClickOutside.bind(this));
        document.removeEventListener('click', this.handleClickOutside.bind(this));
    private handleClickOutside(event: MouseEvent): void {
        if (!target.closest('.user-picker')) {
            this.showResults = false;
    onSearch(): void {
        if (!this.searchQuery || this.searchQuery.trim().length < 2) {
        this.performSearch(this.searchQuery.trim());
    onSelectUser(user: UserSearchResult): void {
        this.userSelected.emit(user);
    private async performSearch(query: string): Promise<void> {
        if (!query || query.length < 2) {
        this.showResults = true;
            // Escape single quotes in query to prevent SQL errors
            // Build exclude filter
            let excludeFilter = '';
            if (this.excludeUserIds.length > 0) {
                const ids = this.excludeUserIds.map(id => `'${id}'`).join(',');
                excludeFilter = ` AND ID NOT IN (${ids})`;
            // Search in Name, Email, FirstName, and LastName (case-insensitive)
            const searchFilter = `(
                Name LIKE '%${escapedQuery}%' OR
                Email LIKE '%${escapedQuery}%' OR
                FirstName LIKE '%${escapedQuery}%' OR
                LastName LIKE '%${escapedQuery}%'
            )${excludeFilter}`;
                ExtraFilter: searchFilter,
            console.log('User search result:', {
                count: result.Results?.length,
                this.searchResults = result.Results.map(user => ({
                    id: user.ID,
                    firstName: user.FirstName || '',
                    lastName: user.LastName || ''
                console.warn('User search returned no results or failed:', result.ErrorMessage);
            console.error('Error searching users:', error);
