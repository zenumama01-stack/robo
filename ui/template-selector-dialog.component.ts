import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MJTemplateEntity, MJTemplateCategoryEntity } from '@memberjunction/core-entities';
export interface TemplateSelectorConfig {
  /** Filter criteria for templates */
  /** Pre-selected template IDs */
  selectedTemplateIds?: string[];
  /** Show only active templates */
  showActiveOnly?: boolean;
export interface TemplateSelectorResult {
  /** Selected templates */
  selectedTemplates: MJTemplateEntity[];
 * Template selector dialog that allows users to search and select from existing templates.
 * This dialog provides a searchable interface with category filtering and template preview.
  selector: 'mj-template-selector-dialog',
  templateUrl: './template-selector-dialog.component.html',
  styleUrls: ['./template-selector-dialog.component.css'],
  imports: [CommonModule, ReactiveFormsModule]
export class TemplateSelectorDialogComponent implements OnInit, OnDestroy {
  config: TemplateSelectorConfig = { title: 'Select Template' };
  public result = new Subject<TemplateSelectorResult | null>();
  templates$ = new BehaviorSubject<MJTemplateEntity[]>([]);
  filteredTemplates$ = new BehaviorSubject<MJTemplateEntity[]>([]);
  categories$ = new BehaviorSubject<MJTemplateCategoryEntity[]>([]);
  // Search and filtering
  selectedCategory: string | null = null;
  selectedTemplates: Set<string> = new Set();
    private dialogRef: DialogRef,
    // Initialize selected templates if provided
    if (this.config.selectedTemplateIds) {
      this.selectedTemplates = new Set(this.config.selectedTemplateIds);
        this.filterTemplates(searchTerm || '');
      // Load both templates and categories in parallel
        this.loadTemplates(),
        this.loadCategories()
      console.error('Error loading template data:', error);
        'Error loading templates. Please try again.',
  private async loadTemplates() {
      if (this.config.showActiveOnly !== false) {
        filter = "IsActive = 1";
        filter += filter ? ` AND ${this.config.extraFilter}` : this.config.extraFilter;
      const result = await rv.RunView<MJTemplateEntity>({
        EntityName: 'MJ: Templates',
        const templates = result.Results || [];
        this.templates$.next(templates);
        this.filteredTemplates$.next(templates);
        throw new Error(result.ErrorMessage || 'Failed to load templates');
      console.error('Error loading templates:', error);
      this.templates$.next([]);
      this.filteredTemplates$.next([]);
  private async loadCategories() {
      const result = await rv.RunView<MJTemplateCategoryEntity>({
        EntityName: 'MJ: Template Categories',
        const categories = result.Results || [];
        this.categories$.next(categories);
        throw new Error(result.ErrorMessage || 'Failed to load categories');
      console.error('Error loading categories:', error);
      this.categories$.next([]);
  private filterTemplates(searchTerm: string) {
    const allTemplates = this.templates$.value;
    let filtered = allTemplates;
    // Apply search filter
    if (searchTerm.trim()) {
      filtered = filtered.filter(template => 
        template.Name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        template.Description?.toLowerCase().includes(searchTerm.toLowerCase())
    // Apply category filter
    if (this.selectedCategory) {
        template.CategoryID === this.selectedCategory
    this.filteredTemplates$.next(filtered);
  // === Category Management ===
  onCategoryChange(categoryId: string | null) {
    this.selectedCategory = categoryId === '' ? null : categoryId;
    this.filterTemplates(this.searchControl.value || '');
  getCategoryDisplayName(categoryId: string): string {
    return category?.Name || 'Unknown Category';
  toggleTemplateSelection(template: MJTemplateEntity) {
      if (this.selectedTemplates.has(template.ID)) {
        this.selectedTemplates.delete(template.ID);
        this.selectedTemplates.add(template.ID);
      this.selectedTemplates.clear();
  isTemplateSelected(template: MJTemplateEntity): boolean {
    return this.selectedTemplates.has(template.ID);
  getSelectedTemplateObjects(): MJTemplateEntity[] {
    return allTemplates.filter(template => this.selectedTemplates.has(template.ID));
  getTemplateStatusColor(template: MJTemplateEntity): string {
    if (!template.IsActive) return '#6c757d';
    if (template.DisabledAt && new Date(template.DisabledAt) <= new Date()) return '#dc3545';
    if (template.ActiveAt && new Date(template.ActiveAt) > new Date()) return '#ffc107';
  getTemplateStatusText(template: MJTemplateEntity): string {
    if (!template.IsActive) return 'Inactive';
    if (template.DisabledAt && new Date(template.DisabledAt) <= new Date()) return 'Disabled';
    if (template.ActiveAt && new Date(template.ActiveAt) > new Date()) return 'Scheduled';
    return 'Active';
  getTemplatePreview(template: MJTemplateEntity): string {
    if (!template.Description) return 'No description available';
    return template.Description.length > 100 
      ? template.Description.substring(0, 100) + '...' 
      : template.Description;
  formatDate(date: Date | string | null): string {
    if (!date) return '';
    return d.toLocaleDateString();
  selectTemplates() {
    const selectedTemplateObjects = this.getSelectedTemplateObjects();
    if (selectedTemplateObjects.length === 0) {
        'Please select at least one template',
    const result: TemplateSelectorResult = {
      selectedTemplates: selectedTemplateObjects
      selectedTemplates: [],
