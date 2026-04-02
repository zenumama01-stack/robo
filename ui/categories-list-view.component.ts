import { MJActionCategoryEntity, MJActionEntity } from '@memberjunction/core-entities';
interface CategoryWithStats extends MJActionCategoryEntity {
  actionCount?: number;
  activeActionCount?: number;
  selector: 'mj-categories-list-view',
    <div class="categories-list-view" mjFillContainer>
      <!-- Header with search -->
          <h2><i class="fa-solid fa-sitemap"></i> Action Categories</h2>
          <div class="results-count">{{ filteredCategories.length }} of {{ categories.length }} categories</div>
            placeholder="Search categories..." 
      <!-- Categories Grid -->
      <div class="categories-grid">
        @if (filteredCategories.length > 0) {
          @for (category of filteredCategories; track category.ID) {
            <div class="category-card" (click)="openCategory(category)">
              <div class="category-header">
                <div class="category-icon">
                <h3 class="category-name">{{ category.Name }}</h3>
              @if (category.Description) {
                <div class="category-description">
                  {{ category.Description }}
              <div class="category-stats">
                  <span class="stat-value">{{ category.actionCount || 0 }}</span>
                  <span class="stat-label">Total Actions</span>
                  <span class="stat-value active">{{ category.activeActionCount || 0 }}</span>
                  <span class="stat-label">Active</span>
              <div class="category-footer">
                  (click)="viewActions(category, $event)">
                  <i class="fa-solid fa-cogs"></i> View Actions
            <h3>No categories found</h3>
            <p>Try adjusting your search criteria</p>
          <mj-loading [showText]="false" size="large"></mj-loading>
    .categories-list-view {
        .header-title {
            i {
          .results-count {
          kendo-textbox {
      .categories-grid {
        align-content: start;
        .category-card {
          &:hover {
            .category-icon {
          .category-description {
          .category-stats {
                &.active {
          .category-footer {
            margin-top: auto;
        padding: 4rem;
export class CategoriesListViewComponent implements OnInit, OnDestroy {
  public categories: CategoryWithStats[] = [];
  public filteredCategories: CategoryWithStats[] = [];
  private setupSearch(): void {
    this.searchTerm$.pipe(
      this.applyFilter();
      console.log('Loading categories data...');
      const [categoriesResult, actionsResult] = await rv.RunViews([
      console.log('Categories result:', categoriesResult);
      console.log('Actions result for categories:', actionsResult);
      if (!categoriesResult.Success) {
        throw new Error('Failed to load categories: ' + categoriesResult.ErrorMessage);
      console.log(`Loaded ${categories.length} categories and ${actions.length} actions`);
      // Calculate stats for each category
      this.categories = categories.map(category => {
          ...category,
          activeActionCount: categoryActions.filter(a => a.Status === 'Active').length
        } as CategoryWithStats;
      console.error('Error loading categories data:', error);
      LogError('Failed to load categories data', undefined, error);
      this.categories = [];
      this.filteredCategories = [];
  private applyFilter(): void {
    if (!searchTerm) {
      this.filteredCategories = [...this.categories];
      this.filteredCategories = this.categories.filter(category => 
        category.Name.toLowerCase().includes(searchTerm) ||
        (category.Description || '').toLowerCase().includes(searchTerm)
  public openCategory(category: MJActionCategoryEntity): void {
      entityName: 'MJ: Action Categories',
      recordId: category.ID
  public viewActions(category: MJActionCategoryEntity, event: Event): void {
    // This could navigate to the actions list with a pre-applied category filter
    // For now, just open the category
    this.openCategory(category);
