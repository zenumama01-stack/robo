import { WindowService, WindowRef, WindowCloseResult } from '@progress/kendo-angular-dialog';
import { SettingsComponent } from '@memberjunction/ng-explorer-settings';
 * Service to open the Settings component in a full-screen modal dialog.
 * This keeps the workspace intact while allowing users to configure settings.
export class SettingsDialogService {
  private windowRef: WindowRef | null = null;
  constructor(private windowService: WindowService) {}
   * Opens the Settings component in a full-screen Kendo Window
   * @param containerRef ViewContainerRef to anchor the window (usually from the shell component)
  open(containerRef: ViewContainerRef): void {
    // Don't open multiple windows
    if (this.windowRef) {
    this.windowRef = this.windowService.open({
      content: SettingsComponent,
      title: 'Settings',
      width: window.innerWidth,
      height: window.innerHeight,
      minWidth: 400,
      minHeight: 300,
      resizable: false,
      draggable: false,
      cssClass: 'settings-fullscreen-window',
      appendTo: containerRef
    // Handle window close
    this.windowRef.result.subscribe((result) => {
      this.windowRef = null;
   * Closes the settings window if open
      this.windowRef.close();
   * Check if the settings window is currently open
  get isOpen(): boolean {
    return this.windowRef !== null;
