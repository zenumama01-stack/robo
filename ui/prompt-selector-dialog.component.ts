import { Subject, BehaviorSubject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
export interface PromptSelectorConfig {
  /** Whether to show the "Create New" option */
  /** Filter criteria for prompts */
  /** Allow multiple selection */
  /** Pre-selected prompt IDs */
  /** Already linked prompt IDs (will be grayed out and not selectable) */
export interface PromptSelectorResult {
  /** Selected prompts */
  selectedPrompts: AIPromptEntityExtended[];
  /** Whether user chose to create new */
  createNew?: boolean;
 * Unified prompt selector dialog that can be used for:
 * - Selecting context compression prompts (single select)
 * - Adding general prompts to agents (multi-select)
 * - Any other prompt selection scenario
  selector: 'mj-prompt-selector-dialog',
  templateUrl: './prompt-selector-dialog.component.html',
  styleUrls: ['./prompt-selector-dialog.component.css']
export class PromptSelectorDialogComponent implements OnInit, OnDestroy {
  // Input configuration
  config: PromptSelectorConfig = { title: 'Select Prompts' };
  public result = new Subject<PromptSelectorResult | null>();
  // Data and UI state
  prompts$ = new BehaviorSubject<AIPromptEntityExtended[]>([]);
  filteredPrompts$ = new BehaviorSubject<AIPromptEntityExtended[]>([]);
  // Search and selection
  selectedPrompts: Set<string> = new Set();
  linkedPrompts: Set<string> = new Set();
  // View mode
  viewMode: 'grid' | 'list' = 'list';
    this.setupSearch();
    this.loadPrompts();
    // Initialize selected prompts if provided
    if (this.config.selectedPromptIds) {
      this.selectedPrompts = new Set(this.config.selectedPromptIds);
    // Initialize linked prompts if provided
    if (this.config.linkedPromptIds) {
      this.linkedPrompts = new Set(this.config.linkedPromptIds);
  private setupSearch() {
    this.searchControl.valueChanges
      .subscribe(searchTerm => {
        this.filterPrompts(searchTerm || '');
  private async loadPrompts() {
      // Build filter - default to active prompts
      let filter = "Status = 'Active'";
      if (this.config.extraFilter) {
        filter += ` AND ${this.config.extraFilter}`;
        const prompts = result.Results || [];
        this.prompts$.next(prompts);
        this.filteredPrompts$.next(prompts);
        throw new Error(result.ErrorMessage || 'Failed to load prompts');
      console.error('Error loading prompts:', error);
        'Error loading prompts. Please try again.',
      this.prompts$.next([]);
      this.filteredPrompts$.next([]);
  private filterPrompts(searchTerm: string) {
    const allPrompts = this.prompts$.value;
    if (!searchTerm.trim()) {
      this.filteredPrompts$.next(allPrompts);
    const filtered = allPrompts.filter(prompt => 
      prompt.Name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      prompt.Description?.toLowerCase().includes(searchTerm.toLowerCase())
    this.filteredPrompts$.next(filtered);
  // === Selection Management ===
  togglePromptSelection(prompt: AIPromptEntityExtended) {
    // Prevent selection of already linked prompts
    if (this.isPromptLinked(prompt)) {
        `"${prompt.Name}" is already linked to this agent`,
    if (this.config.multiSelect) {
      if (this.selectedPrompts.has(prompt.ID)) {
        this.selectedPrompts.delete(prompt.ID);
        this.selectedPrompts.add(prompt.ID);
      // Single select - replace current selection
      this.selectedPrompts.clear();
  isPromptSelected(prompt: AIPromptEntityExtended): boolean {
    return this.selectedPrompts.has(prompt.ID);
  isPromptLinked(prompt: AIPromptEntityExtended): boolean {
    return this.linkedPrompts.has(prompt.ID);
  getSelectedPromptObjects(): AIPromptEntityExtended[] {
    return allPrompts.filter(prompt => this.selectedPrompts.has(prompt.ID));
  // === UI Helpers ===
    this.viewMode = this.viewMode === 'grid' ? 'list' : 'grid';
  getPromptStatusColor(prompt: AIPromptEntityExtended): string {
    switch (prompt.Status) {
  getPromptStatusText(prompt: AIPromptEntityExtended): string {
    return prompt.Status || 'Unknown';
  selectPrompts() {
    const selectedPromptObjects = this.getSelectedPromptObjects();
    if (selectedPromptObjects.length === 0) {
        'Please select at least one prompt',
    const result: PromptSelectorResult = {
      selectedPrompts: selectedPromptObjects
  createNew() {
      selectedPrompts: [],
      createNew: true
