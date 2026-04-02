import { MJAIPromptTypeEntity, MJAIPromptCategoryEntity, MJTemplateEntity, MJTemplateContentEntity, ResourceData, UserInfoEngine } from '@memberjunction/core-entities';
interface PromptWithTemplate extends Omit<AIPromptEntityExtended, 'Template'> {
  Template: string; // From AIPromptEntityExtended (view field)
  MJTemplateEntity?: MJTemplateEntity; // Our added field for the actual template entity
  TemplateContents?: MJTemplateContentEntity[];
  CategoryName?: string;
  TypeName?: string;
 * User preferences for the Prompt Management dashboard
interface PromptManagementUserPreferences {
  viewMode: 'grid' | 'list' | 'priority-matrix';
  selectedCategory: string;
 * AI Prompts Resource - displays AI prompt management
@RegisterClass(BaseResourceComponent, 'AIPromptsResource')
  selector: 'app-prompt-management',
  templateUrl: './prompt-management.component.html',
  styleUrls: ['./prompt-management.component.css']
export class PromptManagementComponent extends BaseResourceComponent implements OnInit, OnDestroy {
  private readonly USER_SETTINGS_KEY = 'AI.Prompts.UserPreferences';
  public viewMode: 'grid' | 'list' | 'priority-matrix' = 'grid';
  public expandedPromptId: string | null = null;
  public prompts: PromptWithTemplate[] = [];
  public filteredPrompts: PromptWithTemplate[] = [];
  public categories: MJAIPromptCategoryEntity[] = [];
  public types: MJAIPromptTypeEntity[] = [];
  public selectedCategory = 'all';
  public selectedPrompt: PromptWithTemplate | null = null;
    'Loading AI prompts...',
    'Fetching templates...',
    'Organizing categories...',
    'Almost there...'
  public selectedPromptForTest: AIPromptEntityExtended | null = null;
  /** Check if user can create AI Prompts */
  /** Check if user can read AI Prompts */
  public get UserCanReadPrompts(): boolean {
    return this.checkEntityPermission('MJ: AI Prompts', 'Read');
  /** Check if user can update AI Prompts */
    return this.checkEntityPermission('MJ: AI Prompts', 'Update');
  /** Check if user can delete AI Prompts */
    return this.checkEntityPermission('MJ: AI Prompts', 'Delete');
        const prefs = JSON.parse(savedPrefs) as PromptManagementUserPreferences;
      console.warn('[PromptManagement] Failed to load user preferences:', error);
  private applyUserPreferencesFromStorage(prefs: PromptManagementUserPreferences): void {
    if (prefs.selectedCategory) {
      this.selectedCategory = prefs.selectedCategory;
  private getCurrentPreferences(): PromptManagementUserPreferences {
      selectedCategory: this.selectedCategory,
      console.warn('[PromptManagement] Failed to persist user preferences:', error);
      // Configure both engines in parallel (no-op if already loaded)
        AIEngineBase.Instance.Config(false),
        TemplateEngineBase.Instance.Config(false)
      const prompts = AIEngineBase.Instance.Prompts;
      this.categories = AIEngineBase.Instance.PromptCategories;
      this.types = AIEngineBase.Instance.PromptTypes;
      // Get cached data from TemplateEngineBase
      const templates = TemplateEngineBase.Instance.Templates as MJTemplateEntity[];
      const templateContents = TemplateEngineBase.Instance.TemplateContents;
      const templateMap = new Map(templates.map(t => [t.ID, t]));
      const templateContentMap = new Map<string, MJTemplateContentEntity[]>();
      templateContents.forEach(tc => {
        const contents = templateContentMap.get(tc.TemplateID) || [];
        contents.push(tc);
        templateContentMap.set(tc.TemplateID, contents);
      const categoryMap = new Map(this.categories.map(c => [c.ID, c.Name]));
      const typeMap = new Map(this.types.map(t => [t.ID, t.Name]));
      // Combine the data - keep the actual entity objects
      this.prompts = prompts.map(prompt => {
        const template = templateMap.get(prompt.ID);
        // Add the extra properties directly to the entity
        (prompt as any).MJTemplateEntity = template;
        (prompt as any).TemplateContents = template ? (templateContentMap.get(template.ID) || []) : [];
        (prompt as any).CategoryName = prompt.CategoryID ? categoryMap.get(prompt.CategoryID) || 'Unknown' : 'Uncategorized';
        (prompt as any).TypeName = prompt.TypeID ? typeMap.get(prompt.TypeID) || 'Unknown' : 'Untyped';
        return prompt as PromptWithTemplate;
      this.filteredPrompts = [...this.prompts];
      console.error('Error loading prompt data:', error);
      MJNotificationService.Instance.CreateSimpleNotification('Error loading prompts', 'error', 3000);
    if (state.selectedCategory) this.selectedCategory = state.selectedCategory;
  public setViewMode(mode: 'grid' | 'list' | 'priority-matrix'): void {
    this.expandedPromptId = null;
  public togglePromptExpansion(promptId: string): void {
    this.expandedPromptId = this.expandedPromptId === promptId ? null : promptId;
    this.filteredPrompts = this.prompts.filter(prompt => {
          prompt.CategoryName?.toLowerCase().includes(searchLower) ||
          prompt.TypeName?.toLowerCase().includes(searchLower);
      if (this.selectedCategory !== 'all' && prompt.CategoryID !== this.selectedCategory) {
      if (this.selectedType !== 'all' && prompt.TypeID !== this.selectedType) {
        const isActive = prompt.Status === 'Active';
    this.filteredPrompts = this.applySorting(this.filteredPrompts);
   * Sort the prompts by the specified column
  private applySorting(prompts: PromptWithTemplate[]): PromptWithTemplate[] {
    return prompts.sort((a, b) => {
        case 'Category':
          valueA = a.CategoryName;
          valueB = b.CategoryName;
        case 'Type':
          valueA = a.TypeName;
          valueB = b.TypeName;
  public onCategoryChange(categoryId: string): void {
    this.selectedCategory = categoryId;
  public openPrompt(promptId: string): void {
    const compositeKey = new CompositeKey([{ FieldName: 'ID', Value: promptId }]);
    this.navigationService.OpenEntityRecord('MJ: AI Prompts', compositeKey);
   * Show the detail panel for a prompt
  public showPromptDetails(prompt: PromptWithTemplate, event?: Event): void {
    this.selectedPrompt = prompt;
    // Delay clearing selectedPrompt for smoother animation
        this.selectedPrompt = null;
  public openPromptFromPanel(): void {
    if (this.selectedPrompt) {
      this.openPrompt(this.selectedPrompt.ID);
  public testPrompt(promptId: string, event?: Event): void {
    this.testHarnessService.openForPrompt(promptId);
    this.selectedPromptForTest = null;
  public createNewPrompt(): void {
    // Use the standard MemberJunction pattern to open a new AI Prompt form
    // Empty CompositeKey indicates a new record
    this.navigationService.OpenEntityRecord('MJ: AI Prompts', new CompositeKey([]));
  public getPromptIcon(prompt: PromptWithTemplate): string {
    if (prompt.TypeName?.toLowerCase().includes('system')) {
      return 'fa-solid fa-cogs';
    } else if (prompt.TypeName?.toLowerCase().includes('user')) {
      return 'fa-solid fa-user';
    } else if (prompt.TypeName?.toLowerCase().includes('chat')) {
    return 'fa-solid fa-comment-dots';
  public getStatusClass(status: string): string {
    return status === 'Active' ? 'active' : 'inactive';
           this.selectedCategory !== 'all' || 
           this.selectedStatus !== 'all';
  public get filteredPromptsAsEntities(): AIPromptEntityExtended[] {
    // The prompts are already AIPromptEntityExtended instances with extra properties
    return this.filteredPrompts as AIPromptEntityExtended[];
    this.selectedCategory = 'all';
    return 'Prompts';
