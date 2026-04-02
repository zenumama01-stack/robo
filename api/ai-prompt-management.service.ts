import { DialogService, DialogRef } from '@progress/kendo-angular-dialog';
import { MJTemplateEntity } from '@memberjunction/core-entities';
  TemplateSelectorDialogComponent, 
  TemplateSelectorConfig, 
  TemplateSelectorResult 
} from './template-selector-dialog.component';
export class AIPromptManagementService {
   * Opens the template selector dialog for linking existing templates to AI prompts
  openTemplateSelectorDialog(config: TemplateSelectorConfig & { viewContainerRef?: ViewContainerRef }): Observable<TemplateSelectorResult | null> {
    const dialogRef: DialogRef = this.dialogService.open({
      title: config.title,
      content: TemplateSelectorDialogComponent,
      preventAction: (action) => {
        // Prevent closing on backdrop click
        return action === 'close';
    // Configure the dialog component
    const dialogComponent = dialogRef.content.instance as TemplateSelectorDialogComponent;
    dialogComponent.config = {
      multiSelect: config.multiSelect ?? false,
      selectedTemplateIds: config.selectedTemplateIds,
      showActiveOnly: config.showActiveOnly ?? true
    // Create a subject to handle the result
    const resultSubject = new Subject<TemplateSelectorResult | null>();
    // Subscribe to the dialog component's result
    dialogComponent.result.subscribe({
        resultSubject.next(result);
        resultSubject.error(error);
    // Handle dialog close
    dialogRef.result.subscribe({
        // Dialog was closed, emit null if no result was already emitted
        if (!resultSubject.closed) {
          resultSubject.next(null);
   * Opens a template creation dialog and returns the created template
  openCreateTemplateDialog(config: {
    promptName?: string;
  }): Observable<MJTemplateEntity | null> {
    // For now, we'll return a placeholder - in a full implementation,
    // this would open a template creation dialog
    const resultSubject = new Subject<MJTemplateEntity | null>();
    // Simulate async operation
