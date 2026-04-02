  Input,
  EventEmitter,
  OnChanges,
  SimpleChanges,
  ChangeDetectionStrategy
interface BreadcrumbItem {
  selector: 'mj-action-breadcrumb',
  templateUrl: './action-breadcrumb.component.html',
  styleUrls: ['./action-breadcrumb.component.css'],
export class ActionBreadcrumbComponent implements OnChanges {
  @Input() SelectedCategoryId = 'all';
  @Input() Categories: MJActionCategoryEntity[] = [];
  @Output() CategorySelect = new EventEmitter<string>();
  public Breadcrumbs: BreadcrumbItem[] = [];
    if (changes['SelectedCategoryId'] || changes['Categories']) {
      this.buildBreadcrumbs();
  private buildBreadcrumbs(): void {
    this.Breadcrumbs = [];
    // Always start with "All Actions"
    this.Breadcrumbs.push({
      icon: 'fa-solid fa-layer-group'
    if (this.SelectedCategoryId === 'all') {
    if (this.SelectedCategoryId === 'uncategorized') {
        icon: 'fa-solid fa-inbox'
    // Build path from root to selected category
    const categoryMap = new Map<string, MJActionCategoryEntity>();
    this.Categories.forEach(c => categoryMap.set(c.ID, c));
    const path: BreadcrumbItem[] = [];
    let currentId: string | null = this.SelectedCategoryId;
      const category = categoryMap.get(currentId);
      if (category) {
        path.unshift({
          icon: 'fa-solid fa-folder'
        currentId = category.ParentID || null;
    this.Breadcrumbs.push(...path);
  public selectCategory(id: string): void {
    this.CategorySelect.emit(id);
  public isLast(index: number): boolean {
    return index === this.Breadcrumbs.length - 1;
