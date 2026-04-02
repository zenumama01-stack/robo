import { MJAIPromptCategoryEntity, MJAIPromptTypeEntity } from '@memberjunction/core-entities';
interface PromptFilter {
  categoryId: string;
  typeId: string;
interface PromptWithTemplate {
  prompt: any;
  template: any;
  templateContent: any;
  category: any;
  type: any;
  selector: 'mj-prompt-filter-panel',
  templateUrl: './prompt-filter-panel.component.html',
  styleUrls: ['./prompt-filter-panel.component.css']
export class PromptFilterPanelComponent implements OnInit {
  @Input() prompts: PromptWithTemplate[] = [];
  @Input() filteredPrompts: PromptWithTemplate[] = [];
  @Input() categories: MJAIPromptCategoryEntity[] = [];
  @Input() types: MJAIPromptTypeEntity[] = [];
  @Input() filters: PromptFilter = {
    categoryId: 'all',
    typeId: 'all',
    status: 'all'
  @Output() filtersChange = new EventEmitter<PromptFilter>();
  public categoryOptions: Array<{text: string; value: string}> = [];
  public typeOptions: Array<{text: string; value: string}> = [];
    this.buildFilterOptions();
  private buildFilterOptions(): void {
      { text: 'All Categories', value: 'all' },
      ...this.categories.map(cat => ({ text: cat.Name, value: cat.ID }))
    this.typeOptions = [
      { text: 'All Types', value: 'all' },
      ...this.types.map(type => ({ text: type.Name, value: type.ID }))
  public updateCategories(categories: MJAIPromptCategoryEntity[]): void {
    this.categories = categories;
