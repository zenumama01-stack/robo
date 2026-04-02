  SortField,
  SortDirection,
interface SortOption {
  field: SortField;
  selector: 'mj-action-toolbar',
  templateUrl: './action-toolbar.component.html',
  styleUrls: ['./action-toolbar.component.css'],
export class ActionToolbarComponent implements OnInit, OnDestroy {
  @Input() TotalCount = 0;
  @Input() FilteredCount = 0;
  @Output() NewActionClick = new EventEmitter<void>();
  @Output() RefreshClick = new EventEmitter<void>();
  public SortField: SortField = 'name';
  public SortDirection: SortDirection = 'asc';
  public Filters: ActionFilters = {
    statuses: [],
    types: [],
    approvalStatuses: [],
    hasExecutions: null
  public ShowFiltersDropdown = false;
  public ShowSortDropdown = false;
  public SortOptions: SortOption[] = [
    { field: 'name', label: 'Name', icon: 'fa-solid fa-font' },
    { field: 'updated', label: 'Last Updated', icon: 'fa-solid fa-clock' },
    { field: 'status', label: 'Status', icon: 'fa-solid fa-circle-check' },
    { field: 'type', label: 'Type', icon: 'fa-solid fa-tag' },
    { field: 'category', label: 'Category', icon: 'fa-solid fa-folder' }
  public StatusOptions = [
    { value: 'Active', label: 'Active', color: 'success' },
    { value: 'Pending', label: 'Pending', color: 'warning' },
    { value: 'Disabled', label: 'Disabled', color: 'error' }
  public TypeOptions = [
    { value: 'Generated', label: 'AI Generated', icon: 'fa-solid fa-robot' },
    { value: 'Custom', label: 'Custom', icon: 'fa-solid fa-code' }
  private searchInput$ = new Subject<string>();
    this.setupSearchDebounce();
    this.StateService.ViewMode$.pipe(takeUntil(this.destroy$)).subscribe(mode => {
    this.StateService.SortConfig$.pipe(takeUntil(this.destroy$)).subscribe(config => {
      this.SortField = config.field;
      this.SortDirection = config.direction;
    this.StateService.Filters$.pipe(takeUntil(this.destroy$)).subscribe(filters => {
      this.Filters = filters;
  private setupSearchDebounce(): void {
    this.searchInput$.pipe(
    ).subscribe(term => {
      this.StateService.setSearchTerm(term);
  public onSearchInput(term: string): void {
    this.searchInput$.next(term);
    this.StateService.setSearchTerm('');
  public setViewMode(mode: ActionViewMode): void {
    this.StateService.setViewMode(mode);
  public setSortField(field: SortField): void {
    this.StateService.setSortField(field);
    this.ShowSortDropdown = false;
    this.StateService.toggleSortDirection();
  public toggleStatus(status: string): void {
    const current = [...this.Filters.statuses];
    const index = current.indexOf(status);
      current.splice(index, 1);
      current.push(status);
    this.StateService.setStatusFilter(current);
  public toggleType(type: string): void {
    const current = [...this.Filters.types];
    const index = current.indexOf(type);
      current.push(type);
    this.StateService.setTypeFilter(current);
  public isStatusSelected(status: string): boolean {
    return this.Filters.statuses.includes(status);
  public isTypeSelected(type: string): boolean {
    return this.Filters.types.includes(type);
    this.StateService.clearFilters();
  public hasActiveFilters(): boolean {
    return this.StateService.hasActiveFilters();
  public getActiveFilterCount(): number {
    if (this.Filters.searchTerm) count++;
    count += this.Filters.statuses.length;
    count += this.Filters.types.length;
    count += this.Filters.approvalStatuses.length;
    if (this.Filters.hasExecutions != null) count++;
  public toggleFiltersDropdown(): void {
    this.ShowFiltersDropdown = !this.ShowFiltersDropdown;
    if (this.ShowFiltersDropdown) {
  public toggleSortDropdown(): void {
    this.ShowSortDropdown = !this.ShowSortDropdown;
    if (this.ShowSortDropdown) {
      this.ShowFiltersDropdown = false;
  public closeDropdowns(): void {
  public getSortLabel(): string {
    const option = this.SortOptions.find(o => o.field === this.SortField);
    return option?.label || 'Sort';
  public getSortIcon(): string {
    return this.SortDirection === 'asc' ? 'fa-solid fa-arrow-up-short-wide' : 'fa-solid fa-arrow-down-wide-short';
    this.NewActionClick.emit();
  public onRefresh(): void {
    this.RefreshClick.emit();
