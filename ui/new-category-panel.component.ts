  selector: 'mj-new-category-panel',
  templateUrl: './new-category-panel.component.html',
  styleUrls: ['./new-category-panel.component.css'],
export class NewCategoryPanelComponent implements OnInit, OnDestroy {
  @Input() PreselectedParentId: string | null = null;
  @Output() CategoryCreated = new EventEmitter<MJActionCategoryEntity>();
  public ParentID: string | null = null;
    this.StateService.NewCategoryPanelOpen$.pipe(
        if (this.PreselectedParentId) {
          this.ParentID = this.PreselectedParentId;
    this.StateService.closeNewCategoryPanel();
      this.Errors['name'] = 'Category name is required';
    } else if (this.Name.trim().length < 2) {
      this.Errors['name'] = 'Category name must be at least 2 characters';
    } else if (this.Name.trim().length > 200) {
      this.Errors['name'] = 'Category name must be less than 200 characters';
    // Check for duplicate names within same parent
    const existingCategory = this.Categories.find(c =>
      c.Name.toLowerCase() === this.Name.trim().toLowerCase() &&
      (c.ParentID || null) === (this.ParentID || null)
      this.Errors['name'] = 'A category with this name already exists at this level';
      const category = await md.GetEntityObject<MJActionCategoryEntity>('MJ: Action Categories');
      category.Name = this.Name.trim();
      category.Description = this.Description.trim() || null;
      category.ParentID = this.ParentID || null;
        this.CategoryCreated.emit(category);
        this.Errors['general'] = 'Failed to save category. Please try again.';
      LogError('Failed to create category', undefined, error);
      this.Errors['general'] = 'An error occurred while creating the category.';
  public getParentOptions(): Array<{ text: string; value: string | null }> {
    const options: Array<{ text: string; value: string | null }> = [
      { text: '(No Parent - Root Category)', value: null }
    // Sort categories by name for easier selection
      a.Name.localeCompare(b.Name)
