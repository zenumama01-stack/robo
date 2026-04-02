import { ActionGalleryComponent, ActionGalleryConfig } from './action-gallery.component';
export interface ActionGalleryDialogConfig extends ActionGalleryConfig {
  minWidth?: number;
  minHeight?: number;
  submitButtonText?: string;
  cancelButtonText?: string;
  preSelectedActions?: string[];
export class ActionGalleryDialogService {
   * Opens the Action Gallery in a dialog for single selection
   * @param config Configuration for the gallery
   * @returns Observable that emits the selected action when confirmed
  openForSingleSelection(
    config: ActionGalleryDialogConfig = {}, 
    viewContainerRef?: ViewContainerRef
  ): Observable<MJActionEntity | null> {
    const resultSubject = new Subject<MJActionEntity | null>();
    // Configure for single selection
    const galleryConfig: ActionGalleryDialogConfig = {
      selectionMode: true,
      multiSelect: false
    this.openDialog(galleryConfig, viewContainerRef, (component) => {
      this.dialogRef!.result.subscribe((result) => {
        if (result && (result as any).action === 'submit') {
          const selectedActions = component.getSelectedActions();
          resultSubject.next(selectedActions[0] || null);
   * Opens the Action Gallery in a dialog for multiple selection
   * @returns Observable that emits the selected actions when confirmed
  openForMultiSelection(
  ): Observable<MJActionEntity[]> {
    const resultSubject = new Subject<MJActionEntity[]>();
    // Configure for multi selection
      multiSelect: true
          resultSubject.next(selectedActions);
          resultSubject.next([]);
   * Opens the Action Gallery in a dialog for browsing only (no selection)
  openForBrowsing(
      selectionMode: false,
      enableQuickTest: true
      title: config.title || 'Action Gallery',
      width: config.width || 1200,
      height: config.height || 800,
      minWidth: config.minWidth || 800,
      minHeight: config.minHeight || 600,
      content: ActionGalleryComponent,
        { text: 'Close', themeColor: 'base' }
    const component = this.dialogRef.content.instance as ActionGalleryComponent;
    component.config = galleryConfig;
    this.dialogRef.result.subscribe(() => {
  private openDialog(
    config: ActionGalleryDialogConfig,
    viewContainerRef: ViewContainerRef | undefined,
    resultHandler: (component: ActionGalleryComponent) => void
      title: config.title || 'Select Actions',
        { text: config.cancelButtonText || 'Cancel' },
        { text: config.submitButtonText || 'Select', themeColor: 'primary', action: 'submit' }
    component.preSelectedActions = config.preSelectedActions || [];
    // Handle result
    resultHandler(component);
