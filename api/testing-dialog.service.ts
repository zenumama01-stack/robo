import { TestRunDialogComponent } from '../components/test-run-dialog.component';
export interface TestDialogOptions {
  testId?: string;
  suiteId?: string;
  mode?: 'test' | 'suite';
export class TestingDialogService {
  private activeDialogs: DialogRef[] = [];
  OpenTestRunDialog(options?: TestDialogOptions): DialogRef {
    // Close and destroy all existing dialogs
    this.closeAllDialogs();
      title: 'Run Test',
      content: TestRunDialogComponent,
      appendTo: options?.viewContainerRef
    // Track this dialog
    this.activeDialogs.push(dialogRef);
    // Remove from tracking and destroy when closed
    dialogRef.result.subscribe(
        this.removeDialog(dialogRef);
    const dialogInstance = dialogRef.content.instance as TestRunDialogComponent;
    if (options?.mode) {
      dialogInstance.runMode = options.mode;
    if (options?.testId) {
      dialogInstance.selectedTestId = options.testId;
      dialogInstance.runMode = 'test';
    if (options?.suiteId) {
      dialogInstance.selectedSuiteId = options.suiteId;
      dialogInstance.runMode = 'suite';
    return dialogRef;
  private closeAllDialogs(): void {
    // Close all dialogs in reverse order (newest first)
    while (this.activeDialogs.length > 0) {
      const dialog = this.activeDialogs.pop();
      if (dialog) {
          dialog.close();
          // Dialog might already be closed
  private removeDialog(dialogRef: DialogRef): void {
    const index = this.activeDialogs.indexOf(dialogRef);
      this.activeDialogs.splice(index, 1);
  OpenTestDialog(testId: string, viewContainerRef?: ViewContainerRef): DialogRef {
    return this.OpenTestRunDialog({ testId, mode: 'test', viewContainerRef });
  OpenSuiteDialog(suiteId: string, viewContainerRef?: ViewContainerRef): DialogRef {
    return this.OpenTestRunDialog({ suiteId, mode: 'suite', viewContainerRef });
