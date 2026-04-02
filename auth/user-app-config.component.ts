import { Component, Input, Output, EventEmitter, ChangeDetectorRef, NgZone } from '@angular/core';
import { MJApplicationEntity, MJUserApplicationEntity } from '@memberjunction/core-entities';
import { ApplicationManager, BaseApplication, UserAppConfig } from '@memberjunction/ng-base-application';
  userAppId: string | null;
  isDirty: boolean;
 * Full-screen modal dialog for configuring user's application visibility and order.
  selector: 'mj-user-app-config',
  templateUrl: './user-app-config.component.html',
  styleUrls: ['./user-app-config.component.css']
export class UserAppConfigComponent {
  @Input() showDialog = false;
  @Output() showDialogChange = new EventEmitter<boolean>();
  @Output() configSaved = new EventEmitter<void>();
  allApps: AppConfigItem[] = [];
  activeApps: AppConfigItem[] = [];
  availableApps: AppConfigItem[] = [];
  errorMessage = '';
  // Panel collapse state (for mobile)
  availablePanelCollapsed = false;
  selectedPanelCollapsed = false;
  draggedItem: AppConfigItem | null = null;
  draggedIndex = -1;
  dropTargetIndex = -1;
   * Opens the dialog and loads user's app configuration
  async open(): Promise<void> {
    this.showDialogChange.emit(true);
    await this.loadConfiguration();
   * Closes the dialog without saving
    this.showDialogChange.emit(false);
  private async loadConfiguration(): Promise<void> {
      this.allApps = this.buildAppConfigItems(systemApps, userApps);
      this.refreshAppLists();
      this.errorMessage = 'Failed to load app configuration. Please try again.';
  private buildAppConfigItems(systemApps: BaseApplication[], userApps: MJUserApplicationEntity[]): AppConfigItem[] {
        userAppId: userApp?.ID || null,
        sequence: userApp?.Sequence ?? 999, // Default high sequence for unselected
        isActive: userApp?.IsActive ?? false,
        isDirty: false
   * Separates apps into active and available lists based on isActive state
  private refreshAppLists(): void {
    this.activeApps = this.allApps
      .filter(item => item.isActive)
      .sort((a, b) => a.sequence - b.sequence);
    this.availableApps = this.allApps
      .filter(item => !item.isActive)
      .sort((a, b) => a.app.Name.localeCompare(b.app.Name));
  onDragStart(event: DragEvent, item: AppConfigItem, index: number): void {
  onDragOver(event: DragEvent): void {
  onDragEnter(event: DragEvent, index: number): void {
  onDragEnd(event: DragEvent): void {
  onDrop(event: DragEvent): void {
      const [movedItem] = this.activeApps.splice(this.draggedIndex, 1);
      this.activeApps.splice(this.dropTargetIndex, 0, movedItem);
      this.activeApps.forEach((item, idx) => {
        if (item.sequence !== idx) {
          item.sequence = idx;
          item.isDirty = true;
  addApp(item: AppConfigItem): void {
    item.isActive = true;
    item.sequence = this.activeApps.length;
  removeApp(item: AppConfigItem): void {
    item.isActive = false;
    item.sequence = 999;
    this.activeApps.forEach((activeItem, index) => {
      if (activeItem.sequence !== index) {
        activeItem.sequence = index;
        activeItem.isDirty = true;
  moveUp(item: AppConfigItem): void {
    const index = this.activeApps.indexOf(item);
      const prevItem = this.activeApps[index - 1];
      const tempSeq = item.sequence;
      item.sequence = prevItem.sequence;
      prevItem.sequence = tempSeq;
      prevItem.isDirty = true;
      // Re-sort and create new array reference to trigger change detection
      this.activeApps = [...this.activeApps].sort((a, b) => a.sequence - b.sequence);
  moveDown(item: AppConfigItem): void {
    if (index < this.activeApps.length - 1) {
      const nextItem = this.activeApps[index + 1];
      item.sequence = nextItem.sequence;
      nextItem.sequence = tempSeq;
      nextItem.isDirty = true;
  hasChanges(): boolean {
    return this.allApps.some(item => item.isDirty);
    if (!this.hasChanges()) {
      for (const item of this.allApps) {
        if (!item.isDirty) continue;
        if (item.userAppId) {
          await this.updateUserApplication(md, item);
        } else if (item.isActive) {
          await this.createUserApplication(md, item);
        // If not active and no existing record, nothing to do
      this.sharedService.CreateSimpleNotification('App configuration saved successfully!', 'success', 3000);
      this.configSaved.emit();
      this.errorMessage = 'Failed to save configuration. Please try again.';
  private async updateUserApplication(md: Metadata, item: AppConfigItem): Promise<void> {
    await userApp.Load(item.userAppId!);
    userApp.Sequence = item.sequence;
    userApp.IsActive = item.isActive;
      throw new Error(`Failed to update UserApplication for ${item.app.Name}: ${userApp.LatestResult}`);
    item.isDirty = false;
    LogStatus(`Updated UserApplication for ${item.app.Name}: sequence=${item.sequence}, isActive=${item.isActive}`);
  private async createUserApplication(md: Metadata, item: AppConfigItem): Promise<void> {
    userApp.ApplicationID = item.app.ID;
      throw new Error(`Failed to create UserApplication for ${item.app.Name}: ${userApp.LatestResult}`);
    item.userAppId = userApp.ID;
    LogStatus(`Created UserApplication for ${item.app.Name}: sequence=${item.sequence}`);
  async reset(): Promise<void> {
