 * Replaces the main content area when Collections tab is selected
  selector: 'mj-library-full-view',
        <div class="collections-search">
            placeholder="Search collections and artifacts..."
          <button class="btn-secondary" (click)="refresh()" title="Refresh">
        @if (!isLoading && filteredCollections.length === 0) {
            <p>{{ searchQuery ? 'No collections found' : 'No collections yet' }}</p>
        @if (!isLoading && filteredCollections.length > 0) {
            @for (collection of filteredCollections; track collection) {
                class="library-folder"
                (click)="openCollection(collection)">
                <div class="folder-icon">
                <div class="folder-info">
                  <div class="folder-name">{{ collection.Name }}</div>
                    <div class="folder-meta">
                      {{ collection.Description }}
    .collections-search {
    .collections-search i {
    .library-search-input::placeholder {
    .folder-icon {
    .folder-icon i {
    .folder-info {
    .folder-name {
    .folder-meta {
export class LibraryFullViewComponent implements OnInit {
  async loadCollections(): Promise<void> {
      const filter = `EnvironmentID='${this.environmentId}'` +
        this.applySearch();
  onSearchChange(query: string): void {
  private applySearch(): void {
      this.filteredCollections = this.collections.filter(c =>
        c.Name.toLowerCase().includes(query) ||
        (c.Description && c.Description.toLowerCase().includes(query))
  openCollection(collection: MJCollectionEntity): void {
  navigateTo(crumb: { id: string; name: string }): void {
