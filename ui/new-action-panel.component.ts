import { trigger, transition, style, animate } from '@angular/animations';
import { Metadata, LogError, CompositeKey } from '@memberjunction/core';
type ActionType = 'Custom' | 'Generated';
  selector: 'mj-new-action-panel',
  templateUrl: './new-action-panel.component.html',
  styleUrls: ['./new-action-panel.component.css'],
  animations: [
    trigger('slideIn', [
      transition(':enter', [
        style({ transform: 'translateX(100%)', opacity: 0 }),
        animate('250ms ease-out', style({ transform: 'translateX(0)', opacity: 1 }))
      transition(':leave', [
        animate('200ms ease-in', style({ transform: 'translateX(100%)', opacity: 0 }))
export class NewActionPanelComponent implements OnInit, OnDestroy {
  @Input() PreselectedCategoryId: string | null = null;
  @Output() ActionCreated = new EventEmitter<MJActionEntity>();
  @Output() Close = new EventEmitter<void>();
  public IsOpen = false;
  public Name = '';
  public CategoryID: string | null = null;
  public Type: ActionType = 'Custom';
  // Validation
  public Errors: { [key: string]: string } = {};
  public TypeOptions: Array<{ value: ActionType; label: string; icon: string; description: string }> = [
      value: 'Custom',
      label: 'Custom Action',
      icon: 'fa-solid fa-code',
      description: 'Write your own action code with full control'
      value: 'Generated',
      label: 'AI Generated',
      icon: 'fa-solid fa-robot',
      description: 'Let AI generate the action code from a prompt'
    this.StateService.NewActionPanelOpen$.pipe(
    ).subscribe(isOpen => {
      this.IsOpen = isOpen;
      if (isOpen) {
        this.resetForm();
  private resetForm(): void {
    this.Name = '';
    this.CategoryID = this.PreselectedCategoryId || (this.StateService.SelectedCategoryId !== 'all' && this.StateService.SelectedCategoryId !== 'uncategorized'
      ? this.StateService.SelectedCategoryId
    this.Type = 'Custom';
    this.Errors = {};
  public onClose(): void {
    this.StateService.closeNewActionPanel();
    this.Close.emit();
  public onBackdropClick(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('panel-backdrop')) {
      this.onClose();
  public validate(): boolean {
    if (!this.Name || this.Name.trim().length === 0) {
      this.Errors['name'] = 'Action name is required';
    } else if (this.Name.trim().length < 3) {
      this.Errors['name'] = 'Action name must be at least 3 characters';
    } else if (this.Name.trim().length > 425) {
      this.Errors['name'] = 'Action name must be less than 425 characters';
    if (!this.CategoryID) {
      this.Errors['category'] = 'Please select a category';
    return Object.keys(this.Errors).length === 0;
  public async onSave(): Promise<void> {
    if (!this.validate()) {
      const action = await md.GetEntityObject<MJActionEntity>('MJ: Actions');
      action.Name = this.Name.trim();
      action.Description = this.Description.trim() || null;
      action.CategoryID = this.CategoryID!;
      action.Type = this.Type;
      action.Status = 'Pending'; // New actions start as pending
      const saved = await action.Save();
        this.ActionCreated.emit(action);
        // Open the full action record for editing
        this.Errors['general'] = 'Failed to save action. Please try again.';
      LogError('Failed to create action', undefined, error);
      this.Errors['general'] = 'An error occurred while creating the action.';
  public selectType(type: ActionType): void {
    this.Type = type;
  public getCategoryOptions(): Array<{ text: string; value: string }> {
    const options: Array<{ text: string; value: string }> = [];
    // Sort categories by path for easier selection
    const sortedCategories = [...this.Categories].sort((a, b) =>
      this.getCategoryPath(a).localeCompare(this.getCategoryPath(b))
    sortedCategories.forEach(category => {
      options.push({
        text: this.getCategoryPath(category),
  private getCategoryPath(category: MJActionCategoryEntity): string {
    const path: string[] = [category.Name];
    let currentParentId = category.ParentID;
      const parent = this.Categories.find(c => c.ID === currentParentId);
        path.unshift(parent.Name);
        currentParentId = parent.ParentID || null;
    return path.join(' / ');
